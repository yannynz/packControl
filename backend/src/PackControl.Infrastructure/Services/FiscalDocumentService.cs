using System.Text;
using System.Text.Json;
using PackControl.Application.Abstractions;
using PackControl.Application.Fiscal;
using PackControl.Domain.Audit;
using PackControl.Domain.Orders;
using PackControl.Infrastructure.Persistence;

namespace PackControl.Infrastructure.Services;

public sealed class FiscalDocumentService(
    AppStateStore stateStore,
    IClock clock,
    ICurrentUserAccessor currentUserAccessor,
    IAppStatePersistence statePersistence,
    IFileStorage fileStorage,
    IFiscalNfeEngine fiscalNfeEngine) : IFiscalDocumentService
{
    public async Task<FiscalOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        lock (stateStore.SyncRoot)
        {
            return MapOverview(stateStore);
        }
    }

    public async Task<FiscalEngineDiagnosticDto> GetEngineDiagnosticAsync(Guid? companyProfileId, CancellationToken cancellationToken)
    {
        FiscalCompanyProfileState company;
        FiscalCompanyReadiness readiness;

        lock (stateStore.SyncRoot)
        {
            company = ResolveCompanyLocked(companyProfileId);
            readiness = EvaluateCompanyReadinessLocked(company);
        }

        var engineStatus = await fiscalNfeEngine.CheckStatusAsync(
            new FiscalNfeStatusRequest(
                company.AdapterName,
                company.Environment,
                company.StateCode,
                string.Equals(company.PrincipalEmissionMode, "A1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(company.PrincipalEmissionMode, "A3", StringComparison.OrdinalIgnoreCase)),
            cancellationToken);

        var blockingIssues = new List<string>();
        blockingIssues.AddRange(readiness.BlockingIssues);

        if (!engineStatus.SupportsRealEmission)
        {
            blockingIssues.Add(
                Normalize(company.AdapterName) == "mock-plugavel"
                    ? "Emitente ainda esta apontando para o adapter mock-plugavel."
                    : "Adapter real configurado, mas a emissao real continua bloqueada na configuracao tecnica.");
        }

        var adapterName = Normalize(company.AdapterName);
        if (adapterName == "unimake.dfe")
        {
            blockingIssues.AddRange(GetRealEmissionStructuralGaps());
        }

        if (adapterName != "mock-plugavel" && !engineStatus.IsReachable)
        {
            blockingIssues.Add("Nao foi possivel consultar o autorizador/SEFAZ pelo adapter configurado.");
        }
        else if (adapterName != "mock-plugavel" && !engineStatus.IsServiceOperational)
        {
            blockingIssues.Add("O autorizador respondeu, mas o servico NF-e nao esta operante.");
        }

        var canIssueRealNfe = blockingIssues.Count == 0 &&
            readiness.CanIssueInCurrentEnvironment &&
            engineStatus.IsReachable &&
            engineStatus.IsServiceOperational &&
            engineStatus.SupportsRealEmission;

        return new FiscalEngineDiagnosticDto(
            company.Id,
            company.AdapterName,
            engineStatus.ProviderName,
            company.Environment,
            company.StateCode,
            engineStatus.IsReachable,
            engineStatus.IsServiceOperational,
            engineStatus.SupportsRealEmission,
            canIssueRealNfe,
            engineStatus.StatusCode,
            engineStatus.Status,
            engineStatus.Message,
            engineStatus.ApplicationVersion,
            blockingIssues
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            engineStatus.RawResponse,
            clock.UtcNow);
    }

    public async Task<FiscalDocumentDto> PrepareAsync(PrepareFiscalDocumentCommand command, CancellationToken cancellationToken)
    {
        FiscalDocumentDto document;
        lock (stateStore.SyncRoot)
        {
            document = MapDocument(PrepareDocumentLocked(command, clock.UtcNow));
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return document;
    }

    public async Task<FiscalDocumentDto> IssueAsync(IssueFiscalDocumentCommand command, CancellationToken cancellationToken)
    {
        var documentId = command.FiscalDocumentId;
        if (documentId is null)
        {
            var prepared = await PrepareAsync(
                new PrepareFiscalDocumentCommand(
                    command.FinanceEntryId,
                    command.OrderId,
                    command.Series,
                    command.NatureOfOperation,
                    command.Cfop,
                    command.Notes),
                cancellationToken);
            documentId = prepared.Id;
        }

        var now = clock.UtcNow;
        FiscalNfeEmissionRequest fiscalRequest;
        string companyTradeName;
        string? recipientDocument;
        string adapterName;

        lock (stateStore.SyncRoot)
        {
            var state = stateStore.FiscalDocuments.SingleOrDefault(x => x.Id == documentId.Value)
                ?? throw new InvalidOperationException("Documento fiscal nao encontrado para emissao.");

            var company = stateStore.FiscalCompanies.SingleOrDefault(x => x.Id == state.CompanyProfileId)
                ?? throw new InvalidOperationException("Empresa emissora do documento fiscal nao encontrada.");
            EnsureCompanyEmissionAllowed(EvaluateCompanyReadinessLocked(company), company);

            if (string.IsNullOrWhiteSpace(state.Number))
            {
                var nfeNumber = company.LastNfeNumber + 1;
                company.LastNfeNumber = nfeNumber;
                state.Number = nfeNumber.ToString("000000000");
            }

            state.Series = Normalize(command.Series) ?? state.Series;
            state.NatureOfOperation = Normalize(command.NatureOfOperation) ?? state.NatureOfOperation;
            state.Cfop = Normalize(command.Cfop) ?? state.Cfop;
            state.Notes = Normalize(command.Notes) ?? state.Notes;
            state.EmitterSnapshot.FiscalSeries = state.Series;
            state.EmitterSnapshot.Environment = state.Environment;
            foreach (var item in state.Items)
            {
                item.Cfop = state.Cfop;
            }
            state.Amount = state.Totals.InvoiceAmount;
            state.Status = "ReadyToTransmit";
            state.UpdatedAtUtc = now;

            fiscalRequest = BuildFiscalRequest(state, company);

            companyTradeName = state.EmitterSnapshot.TradeName;
            recipientDocument = state.RecipientSnapshot.DocumentNumber;
            adapterName = company.AdapterName;

            stateStore.FiscalEvents.Add(new FiscalEventState
            {
                Id = Guid.NewGuid(),
                FiscalDocumentId = state.Id,
                EventType = "issue_requested",
                Description = $"Documento fiscal {state.Series}/{state.Number} enviado para emissao.",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    state.NatureOfOperation,
                    state.Cfop,
                    state.Amount
                }),
                ActorUserId = currentUserAccessor.UserId,
                ActorName = currentUserAccessor.DisplayName,
                OccurredAtUtc = now
            });
        }

        try
        {
            var emissionResult = await fiscalNfeEngine.IssueAsync(fiscalRequest, cancellationToken);

            var xmlArchive = await SaveTextFileAsync(
                $"nfe-{fiscalRequest.Emitter.FiscalSeries}-{emissionResult.AccessKey}.xml",
                "application/xml",
                emissionResult.XmlContent,
                cancellationToken);

            var danfeArchive = await SaveTextFileAsync(
                $"danfe-{fiscalRequest.Emitter.FiscalSeries}-{emissionResult.AccessKey}.html",
                "text/html",
                emissionResult.DanfeHtmlContent,
                cancellationToken);

            FiscalDocumentDto document;
            lock (stateStore.SyncRoot)
            {
                var state = stateStore.FiscalDocuments.Single(x => x.Id == documentId.Value);
                state.AccessKey = emissionResult.AccessKey;
                state.Protocol = emissionResult.Protocol;
                state.Status = "Authorized";
                state.LastError = null;
                state.AttemptsCount += 1;
                state.XmlArchivePath = xmlArchive.StoragePath;
                state.DanfeArchivePath = danfeArchive.StoragePath;
                state.IssuedAtUtc = now;
                state.UpdatedAtUtc = now;

                stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    AttemptNumber = state.AttemptsCount,
                    Operation = "issue",
                    AdapterName = emissionResult.EngineName,
                    Status = "Succeeded",
                    ResponseCode = emissionResult.Protocol,
                    ResponseSummary = emissionResult.Status,
                    AttemptedAtUtc = now
                });

                stateStore.FiscalArtifacts.Add(new FiscalArtifactState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    Kind = "xml",
                    FileName = xmlArchive.StoredFileName,
                    StoragePath = xmlArchive.StoragePath,
                    ContentType = xmlArchive.ContentType,
                    SizeBytes = xmlArchive.SizeBytes,
                    Sha256 = xmlArchive.Sha256,
                    CreatedAtUtc = now
                });

                stateStore.FiscalArtifacts.Add(new FiscalArtifactState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    Kind = "danfe",
                    FileName = danfeArchive.StoredFileName,
                    StoragePath = danfeArchive.StoragePath,
                    ContentType = danfeArchive.ContentType,
                    SizeBytes = danfeArchive.SizeBytes,
                    Sha256 = danfeArchive.Sha256,
                    CreatedAtUtc = now
                });

                stateStore.FiscalEvents.Add(new FiscalEventState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    EventType = "authorized",
                    Description = $"NF-e {state.Number} autorizada pelo adaptador {emissionResult.EngineName}.",
                    PayloadJson = JsonSerializer.Serialize(new
                    {
                        state.AccessKey,
                        state.Protocol,
                        state.Environment,
                        state.Amount
                    }),
                    ActorUserId = currentUserAccessor.UserId,
                    ActorName = currentUserAccessor.DisplayName,
                    OccurredAtUtc = now
                });

                UpsertLegacyInvoiceLocked(state, emissionResult.EngineName);
                MarkFinanceEntryAsInvoicedLocked(state.FinanceEntryId);

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    state.OrderId is null ? nameof(FiscalDocumentState) : nameof(Order),
                    state.OrderId ?? state.Id,
                    "fiscal.document_issued",
                    $"NF-e {state.Number} emitida pelo adaptador {emissionResult.EngineName}.",
                    JsonSerializer.Serialize(new
                    {
                        state.Series,
                        state.Amount,
                        state.AccessKey,
                        state.Protocol,
                        companyTradeName,
                        recipientDocument
                    }),
                    now));

                document = MapDocument(state);
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            return document;
        }
        catch (Exception ex)
        {
            lock (stateStore.SyncRoot)
            {
                var state = stateStore.FiscalDocuments.Single(x => x.Id == documentId.Value);
                state.Status = "Error";
                state.LastError = ex.Message;
                state.AttemptsCount += 1;
                state.UpdatedAtUtc = now;

                stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    AttemptNumber = state.AttemptsCount,
                    Operation = "issue",
                    AdapterName = adapterName,
                    Status = "Failed",
                    ResponseCode = "ERROR",
                    ResponseSummary = ex.Message,
                    AttemptedAtUtc = now
                });

                stateStore.FiscalEvents.Add(new FiscalEventState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    EventType = "rejected",
                    Description = $"Falha ao emitir documento fiscal {state.Series}/{state.Number}.",
                    PayloadJson = JsonSerializer.Serialize(new { error = ex.Message }),
                    ActorUserId = currentUserAccessor.UserId,
                    ActorName = currentUserAccessor.DisplayName,
                    OccurredAtUtc = now
                });

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalDocumentState),
                    state.Id,
                    "fiscal.document_issue_failed",
                    $"Falha ao emitir NF-e {state.Number}.",
                    JsonSerializer.Serialize(new { ex.Message, state.Series, state.Number }),
                    now));
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            throw;
        }
    }

    public async Task<FiscalDocumentDto> CancelAsync(CancelFiscalDocumentCommand command, CancellationToken cancellationToken)
    {
        var justification = NormalizeEventReason(command.Reason, "cancelamento");
        var now = clock.UtcNow;
        FiscalNfeCancellationRequest request;
        string adapterName;

        lock (stateStore.SyncRoot)
        {
            var state = stateStore.FiscalDocuments.SingleOrDefault(x => x.Id == command.FiscalDocumentId)
                ?? throw new InvalidOperationException("Documento fiscal nao encontrado para cancelamento.");

            if (string.Equals(state.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Documento fiscal ja foi cancelado.");
            }

            if (string.IsNullOrWhiteSpace(state.AccessKey) || string.IsNullOrWhiteSpace(state.Protocol))
            {
                throw new InvalidOperationException("Documento fiscal ainda nao possui chave/protocolo para cancelamento.");
            }

            var company = stateStore.FiscalCompanies.SingleOrDefault(x => x.Id == state.CompanyProfileId)
                ?? throw new InvalidOperationException("Empresa emissora do documento fiscal nao encontrada.");

            request = new FiscalNfeCancellationRequest(
                state.Id,
                state.AccessKey,
                state.Protocol,
                BuildEmitterProfile(company, state.Series, ParseFiscalNumber(state.Number)),
                justification);

            adapterName = company.AdapterName;

            stateStore.FiscalEvents.Add(new FiscalEventState
            {
                Id = Guid.NewGuid(),
                FiscalDocumentId = state.Id,
                EventType = "cancel_requested",
                Description = $"Cancelamento solicitado para a NF-e {state.Series}/{state.Number}.",
                PayloadJson = JsonSerializer.Serialize(new { reason = justification }),
                ActorUserId = currentUserAccessor.UserId,
                ActorName = currentUserAccessor.DisplayName,
                OccurredAtUtc = now
            });
        }

        try
        {
            var result = await fiscalNfeEngine.CancelAsync(request, cancellationToken);

            var xmlArchive = await SaveTextFileAsync(
                $"nfe-cancelamento-{request.Emitter.FiscalSeries}-{request.AccessKey}.xml",
                "application/xml",
                result.XmlContent,
                cancellationToken);

            var previewArchive = await SaveOptionalTextFileAsync(
                $"cancelamento-{request.Emitter.FiscalSeries}-{request.AccessKey}.html",
                "text/html",
                result.DisplayHtmlContent,
                cancellationToken);

            FiscalDocumentDto document;
            lock (stateStore.SyncRoot)
            {
                var state = stateStore.FiscalDocuments.Single(x => x.Id == command.FiscalDocumentId);
                state.Status = "Cancelled";
                state.LastError = null;
                state.UpdatedAtUtc = now;

                stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    AttemptNumber = NextAttemptNumberLocked(state.Id, "cancel"),
                    Operation = "cancel",
                    AdapterName = result.EngineName,
                    Status = "Succeeded",
                    ResponseCode = result.Protocol,
                    ResponseSummary = result.Status,
                    AttemptedAtUtc = now
                });

                stateStore.FiscalArtifacts.Add(new FiscalArtifactState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    Kind = "cancellation-xml",
                    FileName = xmlArchive.StoredFileName,
                    StoragePath = xmlArchive.StoragePath,
                    ContentType = xmlArchive.ContentType,
                    SizeBytes = xmlArchive.SizeBytes,
                    Sha256 = xmlArchive.Sha256,
                    CreatedAtUtc = now
                });

                if (previewArchive is not null)
                {
                    stateStore.FiscalArtifacts.Add(new FiscalArtifactState
                    {
                        Id = Guid.NewGuid(),
                        FiscalDocumentId = state.Id,
                        Kind = "cancellation-preview",
                        FileName = previewArchive.StoredFileName,
                        StoragePath = previewArchive.StoragePath,
                        ContentType = previewArchive.ContentType,
                        SizeBytes = previewArchive.SizeBytes,
                        Sha256 = previewArchive.Sha256,
                        CreatedAtUtc = now
                    });
                }

                stateStore.FiscalEvents.Add(new FiscalEventState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    EventType = "cancelled",
                    Description = $"NF-e {state.Number} cancelada pelo adaptador {result.EngineName}.",
                    PayloadJson = JsonSerializer.Serialize(new { request.AccessKey, result.Protocol, reason = justification }),
                    ActorUserId = currentUserAccessor.UserId,
                    ActorName = currentUserAccessor.DisplayName,
                    OccurredAtUtc = now
                });

                UpdateLegacyInvoiceStatusLocked(state.Id, "Cancelada");

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalDocumentState),
                    state.Id,
                    "fiscal.document_cancelled",
                    $"NF-e {state.Number} cancelada.",
                    JsonSerializer.Serialize(new { state.Series, state.Number, reason = justification, result.Protocol }),
                    now));

                document = MapDocument(state);
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            return document;
        }
        catch (Exception ex)
        {
            lock (stateStore.SyncRoot)
            {
                var state = stateStore.FiscalDocuments.Single(x => x.Id == command.FiscalDocumentId);
                state.LastError = ex.Message;
                state.UpdatedAtUtc = now;

                stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    AttemptNumber = NextAttemptNumberLocked(state.Id, "cancel"),
                    Operation = "cancel",
                    AdapterName = adapterName,
                    Status = "Failed",
                    ResponseCode = "ERROR",
                    ResponseSummary = ex.Message,
                    AttemptedAtUtc = now
                });

                stateStore.FiscalEvents.Add(new FiscalEventState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    EventType = "cancel_rejected",
                    Description = $"Falha ao cancelar a NF-e {state.Series}/{state.Number}.",
                    PayloadJson = JsonSerializer.Serialize(new { error = ex.Message, reason = justification }),
                    ActorUserId = currentUserAccessor.UserId,
                    ActorName = currentUserAccessor.DisplayName,
                    OccurredAtUtc = now
                });

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalDocumentState),
                    state.Id,
                    "fiscal.document_cancel_failed",
                    $"Falha ao cancelar NF-e {state.Number}.",
                    JsonSerializer.Serialize(new { ex.Message, state.Series, state.Number }),
                    now));
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            throw;
        }
    }

    public async Task<FiscalDocumentDto> ApplyCorrectionLetterAsync(
        ApplyFiscalCorrectionLetterCommand command,
        CancellationToken cancellationToken)
    {
        var correctionText = NormalizeCorrectionText(command.CorrectionText);
        var now = clock.UtcNow;
        FiscalNfeCorrectionLetterRequest request;
        string adapterName;

        lock (stateStore.SyncRoot)
        {
            var state = stateStore.FiscalDocuments.SingleOrDefault(x => x.Id == command.FiscalDocumentId)
                ?? throw new InvalidOperationException("Documento fiscal nao encontrado para CC-e.");

            if (string.Equals(state.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Documento fiscal cancelado nao aceita carta de correcao.");
            }

            if (string.IsNullOrWhiteSpace(state.AccessKey) || string.IsNullOrWhiteSpace(state.Protocol))
            {
                throw new InvalidOperationException("Documento fiscal ainda nao possui chave/protocolo para CC-e.");
            }

            var company = stateStore.FiscalCompanies.SingleOrDefault(x => x.Id == state.CompanyProfileId)
                ?? throw new InvalidOperationException("Empresa emissora do documento fiscal nao encontrada.");

            request = new FiscalNfeCorrectionLetterRequest(
                state.Id,
                state.AccessKey,
                state.Protocol,
                BuildEmitterProfile(company, state.Series, ParseFiscalNumber(state.Number)),
                BuildRecipientProfile(state.RecipientSnapshot),
                NextCorrectionLetterSequenceLocked(state.Id),
                correctionText);

            adapterName = company.AdapterName;

            stateStore.FiscalEvents.Add(new FiscalEventState
            {
                Id = Guid.NewGuid(),
                FiscalDocumentId = state.Id,
                EventType = "correction_letter_requested",
                Description = $"CC-e solicitada para a NF-e {state.Series}/{state.Number}.",
                PayloadJson = JsonSerializer.Serialize(new { correctionText }),
                ActorUserId = currentUserAccessor.UserId,
                ActorName = currentUserAccessor.DisplayName,
                OccurredAtUtc = now
            });
        }

        try
        {
            var result = await fiscalNfeEngine.CorrectAsync(request, cancellationToken);

            var xmlArchive = await SaveTextFileAsync(
                $"nfe-cce-{request.Emitter.FiscalSeries}-{request.AccessKey}.xml",
                "application/xml",
                result.XmlContent,
                cancellationToken);

            var previewArchive = await SaveOptionalTextFileAsync(
                $"cce-{request.Emitter.FiscalSeries}-{request.AccessKey}.html",
                "text/html",
                result.DisplayHtmlContent,
                cancellationToken);

            FiscalDocumentDto document;
            lock (stateStore.SyncRoot)
            {
                var state = stateStore.FiscalDocuments.Single(x => x.Id == command.FiscalDocumentId);
                state.LastError = null;
                state.UpdatedAtUtc = now;

                stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    AttemptNumber = NextAttemptNumberLocked(state.Id, "correction_letter"),
                    Operation = "correction_letter",
                    AdapterName = result.EngineName,
                    Status = "Succeeded",
                    ResponseCode = result.Protocol,
                    ResponseSummary = result.Status,
                    AttemptedAtUtc = now
                });

                stateStore.FiscalArtifacts.Add(new FiscalArtifactState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    Kind = "correction-letter-xml",
                    FileName = xmlArchive.StoredFileName,
                    StoragePath = xmlArchive.StoragePath,
                    ContentType = xmlArchive.ContentType,
                    SizeBytes = xmlArchive.SizeBytes,
                    Sha256 = xmlArchive.Sha256,
                    CreatedAtUtc = now
                });

                if (previewArchive is not null)
                {
                    stateStore.FiscalArtifacts.Add(new FiscalArtifactState
                    {
                        Id = Guid.NewGuid(),
                        FiscalDocumentId = state.Id,
                        Kind = "correction-letter-preview",
                        FileName = previewArchive.StoredFileName,
                        StoragePath = previewArchive.StoragePath,
                        ContentType = previewArchive.ContentType,
                        SizeBytes = previewArchive.SizeBytes,
                        Sha256 = previewArchive.Sha256,
                        CreatedAtUtc = now
                    });
                }

                stateStore.FiscalEvents.Add(new FiscalEventState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    EventType = "correction_letter_registered",
                    Description = $"CC-e registrada para a NF-e {state.Number} pelo adaptador {result.EngineName}.",
                    PayloadJson = JsonSerializer.Serialize(new { result.Protocol, correctionText }),
                    ActorUserId = currentUserAccessor.UserId,
                    ActorName = currentUserAccessor.DisplayName,
                    OccurredAtUtc = now
                });

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalDocumentState),
                    state.Id,
                    "fiscal.document_correction_letter_registered",
                    $"CC-e registrada para a NF-e {state.Number}.",
                    JsonSerializer.Serialize(new { state.Series, state.Number, result.Protocol }),
                    now));

                document = MapDocument(state);
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            return document;
        }
        catch (Exception ex)
        {
            lock (stateStore.SyncRoot)
            {
                var state = stateStore.FiscalDocuments.Single(x => x.Id == command.FiscalDocumentId);
                state.LastError = ex.Message;
                state.UpdatedAtUtc = now;

                stateStore.FiscalTransmissionAttempts.Add(new FiscalTransmissionAttemptState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    AttemptNumber = NextAttemptNumberLocked(state.Id, "correction_letter"),
                    Operation = "correction_letter",
                    AdapterName = adapterName,
                    Status = "Failed",
                    ResponseCode = "ERROR",
                    ResponseSummary = ex.Message,
                    AttemptedAtUtc = now
                });

                stateStore.FiscalEvents.Add(new FiscalEventState
                {
                    Id = Guid.NewGuid(),
                    FiscalDocumentId = state.Id,
                    EventType = "correction_letter_rejected",
                    Description = $"Falha ao registrar CC-e para a NF-e {state.Series}/{state.Number}.",
                    PayloadJson = JsonSerializer.Serialize(new { error = ex.Message, correctionText }),
                    ActorUserId = currentUserAccessor.UserId,
                    ActorName = currentUserAccessor.DisplayName,
                    OccurredAtUtc = now
                });

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalDocumentState),
                    state.Id,
                    "fiscal.document_correction_letter_failed",
                    $"Falha ao registrar CC-e da NF-e {state.Number}.",
                    JsonSerializer.Serialize(new { ex.Message, state.Series, state.Number }),
                    now));
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            throw;
        }
    }

    public async Task<FiscalOverviewDto> InutilizeNumberRangeAsync(
        InutilizeFiscalNumberRangeCommand command,
        CancellationToken cancellationToken)
    {
        var series = EnsureSeries(command.Series);
        var justification = NormalizeEventReason(command.Reason, "inutilizacao");
        if (command.StartNumber <= 0 || command.EndNumber <= 0)
        {
            throw new InvalidOperationException("Faixa de inutilizacao deve usar numeros positivos.");
        }

        if (command.EndNumber < command.StartNumber)
        {
            throw new InvalidOperationException("Numero final da inutilizacao deve ser maior ou igual ao inicial.");
        }

        var now = clock.UtcNow;
        FiscalCompanyProfileState company;
        FiscalNfeInutilizationRequest request;

        lock (stateStore.SyncRoot)
        {
            company = stateStore.FiscalCompanies.SingleOrDefault(x => x.Id == command.CompanyProfileId)
                ?? throw new InvalidOperationException("Empresa fiscal nao encontrada para inutilizacao.");
            EnsureCompanyEmissionAllowed(EvaluateCompanyReadinessLocked(company), company);

            if (stateStore.FiscalNumberingEvents.Any(x =>
                    x.CompanyProfileId == company.Id &&
                    string.Equals(x.Environment, company.Environment, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Series, series, StringComparison.OrdinalIgnoreCase) &&
                    RangesOverlap(x.StartNumber, x.EndNumber, command.StartNumber, command.EndNumber)))
            {
                throw new InvalidOperationException("Ja existe inutilizacao registrada para parte desta faixa.");
            }

            request = new FiscalNfeInutilizationRequest(
                company.Id,
                BuildEmitterProfile(company, series, command.StartNumber),
                series,
                command.StartNumber,
                command.EndNumber,
                justification);
        }

        try
        {
            var result = await fiscalNfeEngine.InutilizeAsync(request, cancellationToken);

            var xmlArchive = await SaveTextFileAsync(
                $"nfe-inutilizacao-{request.Series}-{request.StartNumber:D9}-{request.EndNumber:D9}.xml",
                "application/xml",
                result.XmlContent,
                cancellationToken);

            var previewArchive = await SaveOptionalTextFileAsync(
                $"inutilizacao-{request.Series}-{request.StartNumber:D9}-{request.EndNumber:D9}.html",
                "text/html",
                result.DisplayHtmlContent,
                cancellationToken);

            FiscalOverviewDto overview;
            lock (stateStore.SyncRoot)
            {
                stateStore.FiscalNumberingEvents.Add(new FiscalNumberingEventState
                {
                    Id = Guid.NewGuid(),
                    CompanyProfileId = company.Id,
                    Series = request.Series,
                    StartNumber = request.StartNumber,
                    EndNumber = request.EndNumber,
                    Environment = company.Environment,
                    AdapterName = result.EngineName,
                    Protocol = result.Protocol,
                    Status = "Inutilized",
                    Reason = justification,
                    XmlArchivePath = xmlArchive.StoragePath,
                    PreviewArchivePath = previewArchive?.StoragePath,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });

                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalCompanyProfileState),
                    company.Id,
                    "fiscal.numbering_inutilized",
                    $"Faixa {request.Series}/{request.StartNumber:D9}-{request.EndNumber:D9} inutilizada.",
                    JsonSerializer.Serialize(new { request.Series, request.StartNumber, request.EndNumber, result.Protocol }),
                    now));

                overview = MapOverview(stateStore);
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            return overview;
        }
        catch (Exception ex)
        {
            lock (stateStore.SyncRoot)
            {
                stateStore.AuditLogs.Add(AuditLog.Create(
                    currentUserAccessor.UserId,
                    currentUserAccessor.DisplayName,
                    nameof(FiscalCompanyProfileState),
                    command.CompanyProfileId,
                    "fiscal.numbering_inutilization_failed",
                    $"Falha ao inutilizar a faixa {series}/{command.StartNumber:D9}-{command.EndNumber:D9}.",
                    JsonSerializer.Serialize(new { ex.Message, series, command.StartNumber, command.EndNumber }),
                    now));
            }

            await statePersistence.SaveAsync(stateStore, cancellationToken);
            throw;
        }
    }

    public async Task<FiscalOverviewDto> UpdateCompanyProfileAsync(
        Guid companyProfileId,
        UpdateFiscalCompanyProfileCommand command,
        CancellationToken cancellationToken)
    {
        FiscalOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var company = stateStore.FiscalCompanies.SingleOrDefault(x => x.Id == companyProfileId)
                ?? throw new InvalidOperationException("Empresa fiscal nao encontrada.");

            company.TradeName = command.TradeName.Trim();
            company.DocumentNumber = command.DocumentNumber.Trim();
            company.StateRegistration = command.StateRegistration.Trim();
            company.TaxRegime = command.TaxRegime.Trim();
            company.PostalCode = command.PostalCode.Trim();
            company.Street = command.Street.Trim();
            company.StreetNumber = command.StreetNumber.Trim();
            company.District = command.District.Trim();
            company.City = command.City.Trim();
            company.StateCode = command.StateCode.Trim().ToUpperInvariant();
            company.CityIbgeCode = command.CityIbgeCode.Trim();
            company.Country = command.Country.Trim();
            company.Complement = Normalize(command.Complement);
            company.FiscalSeries = command.FiscalSeries.Trim();
            company.NfeEnabled = command.NfeEnabled;
            company.Environment = command.Environment.Trim();
            company.AdapterName = command.AdapterName.Trim();
            company.CertificateType = command.CertificateType.Trim();
            company.CertificateMedia = command.CertificateMedia.Trim();
            company.PrincipalEmissionMode = command.PrincipalEmissionMode.Trim().ToUpperInvariant();
            company.ContingencyEmissionMode = Normalize(command.ContingencyEmissionMode)?.ToUpperInvariant();
            company.CertificateLabel = Normalize(command.CertificateLabel);
            company.CertificateSerialNumber = Normalize(command.CertificateSerialNumber);
            company.AccountantValidated = command.AccountantValidated;
            company.HomologationCredentialsValidated = command.HomologationCredentialsValidated;
            company.HomologationApproved = command.HomologationApproved;
            company.ProductionCredentialsValidated = command.ProductionCredentialsValidated;
            company.ProductionApproved = command.ProductionApproved;
            company.OnboardingNotes = Normalize(command.OnboardingNotes);

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(FiscalCompanyProfileState),
                company.Id,
                "fiscal.company_updated",
                $"Perfil fiscal {company.TradeName} atualizado.",
                JsonSerializer.Serialize(new
                {
                    company.Environment,
                    company.AdapterName,
                    company.CertificateType,
                    company.CertificateMedia
                }),
                clock.UtcNow));

            overview = MapOverview(stateStore);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    public async Task<FiscalOverviewDto> UpsertOperationTemplateAsync(
        Guid? templateId,
        UpsertFiscalOperationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        FiscalOverviewDto overview;
        lock (stateStore.SyncRoot)
        {
            var target = templateId is null
                ? null
                : stateStore.FiscalOperationTemplates.SingleOrDefault(x => x.Id == templateId.Value);

            if (templateId is not null && target is null)
            {
                throw new InvalidOperationException("Template fiscal nao encontrado.");
            }

            if (target is null)
            {
                target = new FiscalOperationTemplateState
                {
                    Id = Guid.NewGuid()
                };
                stateStore.FiscalOperationTemplates.Add(target);
            }

            target.CompanyProfileId = command.CompanyProfileId;
            target.Name = command.Name.Trim();
            target.NatureOfOperation = command.NatureOfOperation.Trim();
            target.Cfop = command.Cfop.Trim();
            target.Finality = command.Finality.Trim();
            target.Active = command.Active;
            target.Notes = Normalize(command.Notes);
            target.UpdatedAtUtc = clock.UtcNow;

            stateStore.AuditLogs.Add(AuditLog.Create(
                currentUserAccessor.UserId,
                currentUserAccessor.DisplayName,
                nameof(FiscalOperationTemplateState),
                target.Id,
                templateId is null ? "fiscal.template_created" : "fiscal.template_updated",
                templateId is null
                    ? $"Template fiscal {target.Name} criado."
                    : $"Template fiscal {target.Name} atualizado.",
                JsonSerializer.Serialize(new
                {
                    target.CompanyProfileId,
                    target.Cfop,
                    target.Finality,
                    target.Active
                }),
                clock.UtcNow));

            overview = MapOverview(stateStore);
        }

        await statePersistence.SaveAsync(stateStore, cancellationToken);
        return overview;
    }

    private FiscalDocumentState PrepareDocumentLocked(PrepareFiscalDocumentCommand command, DateTime utcNow)
    {
        var financeEntry = command.FinanceEntryId is null
            ? null
            : stateStore.FinanceEntries.SingleOrDefault(x => x.Id == command.FinanceEntryId.Value);

        var resolvedOrderId = command.OrderId ?? financeEntry?.OrderId;
        var order = resolvedOrderId is null
            ? null
            : stateStore.Orders.SingleOrDefault(x => x.Id == resolvedOrderId.Value);

        if (command.FinanceEntryId is not null && financeEntry is null)
        {
            throw new InvalidOperationException("Lancamento financeiro nao encontrado para preparacao fiscal.");
        }

        if (command.OrderId is not null && order is null)
        {
            throw new InvalidOperationException("Pedido nao encontrado para preparacao fiscal.");
        }

        var company = stateStore.FiscalCompanies.FirstOrDefault(x => x.NfeEnabled)
            ?? throw new InvalidOperationException("Nenhum perfil fiscal habilitado para NF-e.");
        EnsureCompanyEmissionAllowed(EvaluateCompanyReadinessLocked(company), company);

        var customer = order is null
            ? null
            : stateStore.Customers.SingleOrDefault(x => x.Id == order.CustomerId);
        var shipment = order is null
            ? null
            : stateStore.Shipments
                .Where(x => x.OrderId == order.Id)
                .OrderByDescending(x => x.ScheduledAtUtc)
                .FirstOrDefault();

        var operationTemplate = stateStore.FiscalOperationTemplates
            .Where(x => x.Active && (x.CompanyProfileId is null || x.CompanyProfileId == company.Id))
            .OrderByDescending(x => x.CompanyProfileId == company.Id)
            .ThenBy(x => x.Name)
            .FirstOrDefault();

        var series = Normalize(command.Series) ?? company.FiscalSeries;
        var natureOfOperation = Normalize(command.NatureOfOperation) ?? operationTemplate?.NatureOfOperation ?? "Venda de produto";
        var cfop = Normalize(command.Cfop) ?? operationTemplate?.Cfop ?? "5101";
        var recipientName = financeEntry?.Counterparty ?? customer?.Name ?? "Cliente avulso";
        var recipientDocument = customer?.DocumentNumber;
        var orderNumber = financeEntry?.OrderNumber ?? order?.Number;
        var issueMode = string.Equals(company.PrincipalEmissionMode, "A3", StringComparison.OrdinalIgnoreCase)
            ? "Hibrido A1/A3"
            : "A1 centralizado";
        var emitterSnapshot = BuildEmitterSnapshot(company, series);
        var recipientSnapshot = BuildRecipientSnapshot(customer, recipientName, recipientDocument);
        var items = BuildFiscalItemsLocked(order, financeEntry, cfop);
        var totals = BuildTotalsSnapshot(items, financeEntry?.Amount);
        var payment = BuildPaymentSnapshot(financeEntry, totals.InvoiceAmount, utcNow);
        var transport = BuildTransportSnapshot(shipment, customer);
        var amount = totals.InvoiceAmount;

        var existingDraft = stateStore.FiscalDocuments.FirstOrDefault(x =>
            x.FinanceEntryId == financeEntry?.Id &&
            x.OrderId == order?.Id &&
            x.Status is "Draft" or "ReadyToTransmit");

        if (existingDraft is not null)
        {
            existingDraft.Series = series;
            existingDraft.NatureOfOperation = natureOfOperation;
            existingDraft.Cfop = cfop;
            existingDraft.RecipientName = recipientName;
            existingDraft.RecipientDocument = recipientDocument;
            existingDraft.Amount = amount;
            existingDraft.EmitterSnapshot = emitterSnapshot;
            existingDraft.RecipientSnapshot = recipientSnapshot;
            existingDraft.Items = items;
            existingDraft.Totals = totals;
            existingDraft.Payment = payment;
            existingDraft.Transport = transport;
            existingDraft.IssueMode = issueMode;
            existingDraft.CertificateType = company.CertificateType;
            existingDraft.CertificateMedia = company.CertificateMedia;
            existingDraft.AdapterName = company.AdapterName;
            existingDraft.Notes = Normalize(command.Notes);
            existingDraft.UpdatedAtUtc = utcNow;

            stateStore.FiscalEvents.Add(new FiscalEventState
            {
                Id = Guid.NewGuid(),
                FiscalDocumentId = existingDraft.Id,
                EventType = "prepared",
                Description = $"Documento fiscal {existingDraft.Series}/{existingDraft.Number} atualizado em preparacao.",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    existingDraft.NatureOfOperation,
                    existingDraft.Cfop,
                    existingDraft.Amount
                }),
                ActorUserId = currentUserAccessor.UserId,
                ActorName = currentUserAccessor.DisplayName,
                OccurredAtUtc = utcNow
            });

            return existingDraft;
        }

        var document = new FiscalDocumentState
        {
            Id = Guid.NewGuid(),
            CompanyProfileId = company.Id,
            FinanceEntryId = financeEntry?.Id,
            OrderId = order?.Id,
            OrderNumber = orderNumber,
            Number = string.Empty,
            Series = series,
            Environment = company.Environment,
            AccessKey = string.Empty,
            Protocol = null,
            AdapterName = company.AdapterName,
            IssueMode = issueMode,
            CertificateType = company.CertificateType,
            CertificateMedia = company.CertificateMedia,
            NatureOfOperation = natureOfOperation,
            Cfop = cfop,
            RecipientName = recipientName,
            RecipientDocument = recipientDocument,
            Amount = amount,
            EmitterSnapshot = emitterSnapshot,
            RecipientSnapshot = recipientSnapshot,
            Items = items,
            Totals = totals,
            Payment = payment,
            Transport = transport,
            Status = "Draft",
            LastError = null,
            AttemptsCount = 0,
            XmlArchivePath = null,
            DanfeArchivePath = null,
            CreatedAtUtc = utcNow,
            IssuedAtUtc = null,
            UpdatedAtUtc = utcNow,
            Notes = Normalize(command.Notes)
        };

        stateStore.FiscalDocuments.Add(document);
        stateStore.FiscalEvents.Add(new FiscalEventState
        {
            Id = Guid.NewGuid(),
            FiscalDocumentId = document.Id,
            EventType = "prepared",
            Description = "Documento fiscal preparado e aguardando emissao.",
            PayloadJson = JsonSerializer.Serialize(new
            {
                document.Series,
                document.NatureOfOperation,
                document.Cfop,
                document.Amount
            }),
            ActorUserId = currentUserAccessor.UserId,
            ActorName = currentUserAccessor.DisplayName,
            OccurredAtUtc = utcNow
        });

        stateStore.AuditLogs.Add(AuditLog.Create(
            currentUserAccessor.UserId,
            currentUserAccessor.DisplayName,
            nameof(FiscalDocumentState),
            document.Id,
            "fiscal.document_prepared",
            "Documento fiscal preparado para emissao.",
            JsonSerializer.Serialize(new
            {
                document.OrderNumber,
                document.NatureOfOperation,
                document.Cfop,
                document.Amount
            }),
            utcNow));

        return document;
    }

    private void UpsertLegacyInvoiceLocked(FiscalDocumentState state, string engineName)
    {
        var existing = stateStore.FiscalInvoices.SingleOrDefault(x => x.Id == state.Id);
        if (existing is null)
        {
            existing = new FiscalInvoiceState
            {
                Id = state.Id
            };
            stateStore.FiscalInvoices.Add(existing);
        }

        existing.FinanceEntryId = state.FinanceEntryId;
        existing.OrderId = state.OrderId;
        existing.OrderNumber = state.OrderNumber;
        existing.Number = state.Number;
        existing.Series = state.Series;
        existing.Environment = state.Environment;
        existing.AccessKey = state.AccessKey;
        existing.Protocol = state.Protocol ?? string.Empty;
        existing.EngineName = engineName;
        existing.CertificateType = state.CertificateType;
        existing.CertificateMedia = state.CertificateMedia;
        existing.NatureOfOperation = state.NatureOfOperation;
        existing.Cfop = state.Cfop;
        existing.XmlArchivePath = state.XmlArchivePath;
        existing.DanfeArchivePath = state.DanfeArchivePath;
        existing.CustomerName = state.RecipientName;
        existing.Status = "Emitida para adaptador fiscal";
        existing.Amount = state.Amount;
        existing.IssuedAtUtc = state.IssuedAtUtc ?? clock.UtcNow;
        existing.Notes = state.Notes;
    }

    private void MarkFinanceEntryAsInvoicedLocked(Guid? financeEntryId)
    {
        if (financeEntryId is null)
        {
            return;
        }

        var financeEntry = stateStore.FinanceEntries.SingleOrDefault(x => x.Id == financeEntryId.Value);
        if (financeEntry is not null && financeEntry.Status != "Liquidado")
        {
            financeEntry.Status = "Faturado";
        }
    }

    private FiscalOverviewDto MapOverview(AppStateStore stateStore) => new(
        stateStore.FiscalCompanies
            .OrderBy(x => x.TradeName)
            .Select(x =>
            {
                var readiness = EvaluateCompanyReadinessLocked(x);
                return new FiscalCompanyProfileItemDto(
                    x.Id,
                    x.TradeName,
                    x.DocumentNumber,
                    x.StateRegistration,
                    x.TaxRegime,
                    x.PostalCode,
                    x.Street,
                    x.StreetNumber,
                    x.District,
                    x.City,
                    x.StateCode,
                    x.CityIbgeCode,
                    x.Country,
                    x.Complement,
                    x.FiscalSeries,
                    x.NfeEnabled,
                    x.Environment,
                    x.AdapterName,
                    x.CertificateType,
                    x.CertificateMedia,
                    x.PrincipalEmissionMode,
                    x.ContingencyEmissionMode,
                    x.CertificateLabel,
                    x.CertificateSerialNumber,
                    x.AccountantValidated,
                    x.HomologationCredentialsValidated,
                    x.HomologationApproved,
                    x.ProductionCredentialsValidated,
                    x.ProductionApproved,
                    readiness.Status,
                    readiness.CanStartHomologation,
                    readiness.CanIssueInCurrentEnvironment,
                    readiness.CanGoLive,
                    readiness.BlockingIssues,
                    readiness.PendingActions,
                    x.OnboardingNotes);
            })
            .ToList(),
        stateStore.FiscalOperationTemplates
            .OrderBy(x => x.Name)
            .Select(x => new FiscalOperationTemplateDto(
                x.Id,
                x.CompanyProfileId,
                x.Name,
                x.NatureOfOperation,
                x.Cfop,
                x.Finality,
                x.Active,
                x.Notes,
                x.UpdatedAtUtc))
            .ToList(),
        stateStore.FiscalAgents
            .OrderBy(x => x.Name)
            .Select(x => new FiscalAgentRegistrationDto(
                x.Id,
                x.Name,
                x.Hostname,
                x.CertificateMedia,
                x.Online,
                x.LastSeenAtUtc,
                x.Status,
                x.Notes))
            .ToList(),
        stateStore.FiscalDocuments
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapDocument)
            .ToList(),
        stateStore.FiscalNumberingEvents
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapNumberingEventDto)
            .ToList());

    private FiscalDocumentDto MapDocument(FiscalDocumentState state) => new(
        state.Id,
        state.CompanyProfileId,
        state.FinanceEntryId,
        state.OrderId,
        state.OrderNumber,
        state.Number,
        state.Series,
        state.Environment,
        state.AccessKey,
        state.Protocol,
        state.AdapterName,
        state.IssueMode,
        state.CertificateType,
        state.CertificateMedia,
        state.NatureOfOperation,
        state.Cfop,
        state.RecipientName,
        state.RecipientDocument,
        state.Amount,
        state.Status,
        state.LastError,
        state.AttemptsCount,
        state.XmlArchivePath,
        state.DanfeArchivePath,
        state.CreatedAtUtc,
        state.IssuedAtUtc,
        state.UpdatedAtUtc,
        MapEmitterDto(state.EmitterSnapshot),
        MapRecipientDto(state.RecipientSnapshot),
        state.Items.Select(MapItemDto).ToList(),
        MapTotalsDto(state.Totals),
        MapPaymentDto(state.Payment),
        MapTransportDto(state.Transport),
        stateStore.FiscalEvents
            .Where(x => x.FiscalDocumentId == state.Id)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Select(MapEventDto)
            .ToList(),
        stateStore.FiscalArtifacts
            .Where(x => x.FiscalDocumentId == state.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(MapArtifactDto)
            .ToList(),
        state.Notes);

    private static FiscalDocumentEventDto MapEventDto(FiscalEventState state) => new(
        state.Id,
        state.EventType,
        state.Description,
        state.PayloadJson,
        state.ActorUserId,
        state.ActorName,
        state.OccurredAtUtc);

    private static FiscalDocumentArtifactDto MapArtifactDto(FiscalArtifactState state) => new(
        state.Id,
        state.Kind,
        state.FileName,
        state.StoragePath,
        state.ContentType,
        state.SizeBytes,
        state.Sha256,
        state.CreatedAtUtc);

    private static FiscalNumberingEventDto MapNumberingEventDto(FiscalNumberingEventState state) => new(
        state.Id,
        state.CompanyProfileId,
        state.Series,
        state.StartNumber,
        state.EndNumber,
        state.Environment,
        state.AdapterName,
        state.Protocol,
        state.Status,
        state.Reason,
        state.XmlArchivePath,
        state.PreviewArchivePath,
        state.CreatedAtUtc,
        state.UpdatedAtUtc);

    private static FiscalDocumentEmitterDto MapEmitterDto(FiscalEmitterSnapshotState snapshot) => new(
        snapshot.CompanyId,
        snapshot.TradeName,
        snapshot.DocumentNumber,
        snapshot.StateRegistration,
        snapshot.TaxRegime,
        snapshot.FiscalSeries,
        snapshot.Environment,
        MapAddressDto(snapshot.Address));

    private static FiscalDocumentRecipientDto MapRecipientDto(FiscalRecipientSnapshotState snapshot) => new(
        snapshot.CustomerId,
        snapshot.Name,
        snapshot.DocumentNumber,
        snapshot.StateRegistration,
        snapshot.TaxpayerIndicator,
        snapshot.Email,
        snapshot.Phone,
        MapAddressDto(snapshot.Address));

    private static FiscalDocumentItemDto MapItemDto(FiscalDocumentItemState item) => new(
        item.LineNumber,
        item.ProductTemplateId,
        item.Description,
        item.CommercialUnit,
        item.Quantity,
        item.TaxQuantity,
        item.UnitPrice,
        item.GrossAmount,
        item.DiscountAmount,
        item.TotalAmount,
        item.BillingMethod,
        item.Cfop,
        item.Ncm,
        item.OriginCode,
        item.IcmsSituationCode,
        item.IpiSituationCode,
        item.PisSituationCode,
        item.CofinsSituationCode,
        item.IcmsRate,
        item.IcmsBaseAmount,
        item.IcmsAmount,
        item.IpiRate,
        item.IpiAmount,
        item.PisRate,
        item.PisAmount,
        item.CofinsRate,
        item.CofinsAmount,
        item.Notes);

    private static FiscalDocumentTotalsDto MapTotalsDto(FiscalDocumentTotalsState totals) => new(
        totals.ProductsAmount,
        totals.DiscountAmount,
        totals.FreightAmount,
        totals.InsuranceAmount,
        totals.OtherAmount,
        totals.IcmsBaseAmount,
        totals.IcmsAmount,
        totals.IpiAmount,
        totals.PisAmount,
        totals.CofinsAmount,
        totals.InvoiceAmount);

    private static FiscalDocumentPaymentDto MapPaymentDto(FiscalDocumentPaymentState payment) => new(
        payment.PaymentMethod,
        payment.BillingType,
        payment.EntrySource,
        payment.BillingAmount,
        payment.DueAtUtc,
        payment.BoletoNumber,
        payment.BoletoLine);

    private static FiscalDocumentTransportDto MapTransportDto(FiscalDocumentTransportState transport) => new(
        transport.ShipmentId,
        transport.CarrierId,
        transport.CarrierName,
        transport.Mode,
        transport.FreightMode,
        transport.RecipientName,
        transport.DriverName,
        transport.VehiclePlate,
        transport.ScheduledAtUtc);

    private static FiscalAddressDto MapAddressDto(FiscalAddressSnapshotState address) => new(
        address.PostalCode,
        address.Street,
        address.StreetNumber,
        address.District,
        address.City,
        address.StateCode,
        address.CityIbgeCode,
        address.Country,
        address.Complement,
        address.ReferencePoint);

    private FiscalEmitterProfile BuildEmitterProfile(FiscalCompanyProfileState company, string series, int nfeNumber)
        => new(
            company.Id,
            company.TradeName,
            company.DocumentNumber,
            company.StateRegistration,
            company.TaxRegime,
            series,
            company.Environment,
            company.AdapterName,
            new FiscalCertificateProfile(
                company.CertificateType,
                company.CertificateMedia,
                company.CertificateLabel,
                company.CertificateSerialNumber),
            new FiscalPartyAddress(
                company.PostalCode,
                company.Street,
                company.StreetNumber,
                company.District,
                company.City,
                company.StateCode,
                company.CityIbgeCode,
                company.Country,
                company.Complement,
                null),
            nfeNumber);

    private static FiscalRecipientProfile BuildRecipientProfile(FiscalRecipientSnapshotState snapshot)
        => new(
            snapshot.CustomerId,
            snapshot.Name,
            snapshot.DocumentNumber,
            snapshot.StateRegistration,
            snapshot.TaxpayerIndicator,
            snapshot.Email,
            snapshot.Phone,
            MapAddressContract(snapshot.Address));

    private FiscalNfeEmissionRequest BuildFiscalRequest(FiscalDocumentState state, FiscalCompanyProfileState company)
        => new(
            state.Id,
            new FiscalEmitterProfile(
                state.EmitterSnapshot.CompanyId,
                state.EmitterSnapshot.TradeName,
                state.EmitterSnapshot.DocumentNumber,
                state.EmitterSnapshot.StateRegistration,
                state.EmitterSnapshot.TaxRegime,
                state.Series,
                state.Environment,
                company.AdapterName,
                new FiscalCertificateProfile(
                    company.CertificateType,
                    company.CertificateMedia,
                    company.CertificateLabel,
                    company.CertificateSerialNumber),
                MapAddressContract(state.EmitterSnapshot.Address),
                int.Parse(state.Number)),
            new FiscalRecipientProfile(
                state.RecipientSnapshot.CustomerId,
                state.RecipientSnapshot.Name,
                state.RecipientSnapshot.DocumentNumber,
                state.RecipientSnapshot.StateRegistration,
                state.RecipientSnapshot.TaxpayerIndicator,
                state.RecipientSnapshot.Email,
                state.RecipientSnapshot.Phone,
                MapAddressContract(state.RecipientSnapshot.Address)),
            state.NatureOfOperation,
            state.Cfop,
            state.Items.Select(MapItemContract).ToList(),
            new FiscalNfeTotals(
                state.Totals.ProductsAmount,
                state.Totals.DiscountAmount,
                state.Totals.FreightAmount,
                state.Totals.InsuranceAmount,
                state.Totals.OtherAmount,
                state.Totals.IcmsBaseAmount,
                state.Totals.IcmsAmount,
                state.Totals.IpiAmount,
                state.Totals.PisAmount,
                state.Totals.CofinsAmount,
                state.Totals.InvoiceAmount),
            new FiscalNfePayment(
                state.Payment.PaymentMethod,
                state.Payment.BillingType,
                state.Payment.EntrySource,
                state.Payment.BillingAmount,
                state.Payment.DueAtUtc,
                state.Payment.BoletoNumber,
                state.Payment.BoletoLine),
            new FiscalNfeTransport(
                state.Transport.ShipmentId,
                state.Transport.CarrierId,
                state.Transport.CarrierName,
                state.Transport.Mode,
                state.Transport.FreightMode,
                state.Transport.RecipientName,
                state.Transport.DriverName,
                state.Transport.VehiclePlate,
                state.Transport.ScheduledAtUtc),
            state.Notes);

    private static FiscalPartyAddress MapAddressContract(FiscalAddressSnapshotState address) => new(
        address.PostalCode,
        address.Street,
        address.StreetNumber,
        address.District,
        address.City,
        address.StateCode,
        address.CityIbgeCode,
        address.Country,
        address.Complement,
        address.ReferencePoint);

    private static FiscalNfeItem MapItemContract(FiscalDocumentItemState item) => new(
        item.LineNumber,
        item.ProductTemplateId,
        item.Description,
        item.CommercialUnit,
        item.Quantity,
        item.TaxQuantity,
        item.UnitPrice,
        item.GrossAmount,
        item.DiscountAmount,
        item.TotalAmount,
        item.BillingMethod,
        item.Cfop,
        item.Ncm,
        item.OriginCode,
        item.IcmsSituationCode,
        item.IpiSituationCode,
        item.PisSituationCode,
        item.CofinsSituationCode,
        item.IcmsRate,
        item.IcmsBaseAmount,
        item.IcmsAmount,
        item.IpiRate,
        item.IpiAmount,
        item.PisRate,
        item.PisAmount,
        item.CofinsRate,
        item.CofinsAmount,
        item.Notes);

    private List<FiscalDocumentItemState> BuildFiscalItemsLocked(
        Order? order,
        FinanceEntryState? entry,
        string cfop)
    {
        if (order is not null && order.ScopeItems.Count > 0)
        {
            return order.ScopeItems
                .Select((scope, index) =>
                {
                    var product = scope.ProductTemplateId is null
                        ? null
                        : stateStore.ProductTemplates.SingleOrDefault(x => x.Id == scope.ProductTemplateId.Value);

                    var quantity = decimal.Round(scope.Quantity, 4);
                    var unitPrice = RoundCurrency(scope.UnitPrice ?? product?.DefaultUnitPrice ?? 0m);
                    var grossAmount = RoundCurrency(quantity * unitPrice);
                    var icmsRate = product?.FiscalIcmsRate ?? 0m;
                    var ipiRate = product?.FiscalIpiRate ?? 0m;
                    var pisRate = product?.FiscalPisRate ?? 0m;
                    var cofinsRate = product?.FiscalCofinsRate ?? 0m;

                    return new FiscalDocumentItemState
                    {
                        LineNumber = index + 1,
                        ProductTemplateId = scope.ProductTemplateId,
                        Description = scope.ProductName ?? product?.Name ?? scope.Title,
                        CommercialUnit = Normalize(product?.FiscalCommercialUnit) ?? "UN",
                        Quantity = quantity,
                        TaxQuantity = quantity,
                        UnitPrice = unitPrice,
                        GrossAmount = grossAmount,
                        DiscountAmount = 0m,
                        TotalAmount = grossAmount,
                        BillingMethod = scope.BillingMethod,
                        Cfop = cfop,
                        Ncm = Normalize(product?.FiscalNcm) ?? "8208.90.00",
                        OriginCode = Normalize(product?.FiscalOriginCode) ?? "0",
                        IcmsSituationCode = Normalize(product?.FiscalIcmsSituationCode) ?? "00",
                        IpiSituationCode = Normalize(product?.FiscalIpiSituationCode) ?? "99",
                        PisSituationCode = Normalize(product?.FiscalPisSituationCode) ?? "49",
                        CofinsSituationCode = Normalize(product?.FiscalCofinsSituationCode) ?? "49",
                        IcmsRate = icmsRate,
                        IcmsBaseAmount = grossAmount,
                        IcmsAmount = CalculateTaxAmount(grossAmount, icmsRate),
                        IpiRate = ipiRate,
                        IpiAmount = CalculateTaxAmount(grossAmount, ipiRate),
                        PisRate = pisRate,
                        PisAmount = CalculateTaxAmount(grossAmount, pisRate),
                        CofinsRate = cofinsRate,
                        CofinsAmount = CalculateTaxAmount(grossAmount, cofinsRate),
                        Notes = scope.Notes
                    };
                })
                .ToList();
        }

        return
        [
            new FiscalDocumentItemState
            {
                LineNumber = 1,
                ProductTemplateId = null,
                Description = entry?.Description ?? "Lancamento avulso",
                CommercialUnit = "UN",
                Quantity = 1m,
                TaxQuantity = 1m,
                UnitPrice = RoundCurrency(entry?.Amount ?? 0m),
                GrossAmount = RoundCurrency(entry?.Amount ?? 0m),
                DiscountAmount = 0m,
                TotalAmount = RoundCurrency(entry?.Amount ?? 0m),
                BillingMethod = entry?.PaymentMethod,
                Cfop = cfop,
                Ncm = "8208.90.00",
                OriginCode = "0",
                IcmsSituationCode = "90",
                IpiSituationCode = "99",
                PisSituationCode = "49",
                CofinsSituationCode = "49",
                IcmsRate = 0m,
                IcmsBaseAmount = RoundCurrency(entry?.Amount ?? 0m),
                IcmsAmount = 0m,
                IpiRate = 0m,
                IpiAmount = 0m,
                PisRate = 0m,
                PisAmount = 0m,
                CofinsRate = 0m,
                CofinsAmount = 0m,
                Notes = entry?.Notes
            }
        ];
    }

    private static FiscalDocumentTotalsState BuildTotalsSnapshot(
        IReadOnlyList<FiscalDocumentItemState> items,
        decimal? targetAmount)
    {
        var productsAmount = RoundCurrency(items.Sum(x => x.GrossAmount));
        var itemDiscountAmount = RoundCurrency(items.Sum(x => x.DiscountAmount));
        var baseInvoiceAmount = RoundCurrency(productsAmount - itemDiscountAmount);
        var desiredInvoiceAmount = RoundCurrency(targetAmount ?? baseInvoiceAmount);

        decimal extraDiscountAmount = 0m;
        decimal otherAmount = 0m;

        if (desiredInvoiceAmount < baseInvoiceAmount)
        {
            extraDiscountAmount = RoundCurrency(baseInvoiceAmount - desiredInvoiceAmount);
        }
        else if (desiredInvoiceAmount > baseInvoiceAmount)
        {
            otherAmount = RoundCurrency(desiredInvoiceAmount - baseInvoiceAmount);
        }

        var discountAmount = RoundCurrency(itemDiscountAmount + extraDiscountAmount);

        return new FiscalDocumentTotalsState
        {
            ProductsAmount = productsAmount,
            DiscountAmount = discountAmount,
            FreightAmount = 0m,
            InsuranceAmount = 0m,
            OtherAmount = otherAmount,
            IcmsBaseAmount = RoundCurrency(items.Sum(x => x.IcmsBaseAmount)),
            IcmsAmount = RoundCurrency(items.Sum(x => x.IcmsAmount)),
            IpiAmount = RoundCurrency(items.Sum(x => x.IpiAmount)),
            PisAmount = RoundCurrency(items.Sum(x => x.PisAmount)),
            CofinsAmount = RoundCurrency(items.Sum(x => x.CofinsAmount)),
            InvoiceAmount = RoundCurrency(productsAmount - discountAmount + otherAmount)
        };
    }

    private FiscalDocumentPaymentState BuildPaymentSnapshot(
        FinanceEntryState? financeEntry,
        decimal invoiceAmount,
        DateTime referenceUtcNow)
        => new()
        {
            PaymentMethod = financeEntry?.PaymentMethod ?? "Sem definicao",
            BillingType = financeEntry is null
                ? "A vista"
                : financeEntry.DueAtUtc.Date > referenceUtcNow.Date
                    ? "A prazo"
                    : "A vista",
            EntrySource = financeEntry?.EntrySource,
            BillingAmount = invoiceAmount,
            DueAtUtc = financeEntry?.DueAtUtc,
            BoletoNumber = financeEntry?.BoletoNumber,
            BoletoLine = financeEntry?.BoletoLine
        };

    private static FiscalDocumentTransportState BuildTransportSnapshot(ShipmentState? shipment, Domain.Customers.Customer? customer)
    {
        var mode = shipment?.Mode ?? customer?.DefaultDeliveryMode ?? "Sem frete";

        return new FiscalDocumentTransportState
        {
            ShipmentId = shipment?.Id,
            CarrierId = shipment?.CarrierId ?? customer?.DefaultCarrierId,
            CarrierName = shipment?.CarrierName ?? customer?.DefaultCarrierName,
            Mode = mode,
            FreightMode = ResolveFreightMode(mode),
            RecipientName = shipment?.Recipient ?? customer?.Name,
            DriverName = shipment?.DriverName,
            VehiclePlate = shipment?.VehiclePlate,
            ScheduledAtUtc = shipment?.ScheduledAtUtc
        };
    }

    private static FiscalEmitterSnapshotState BuildEmitterSnapshot(FiscalCompanyProfileState company, string series)
        => new()
        {
            CompanyId = company.Id,
            TradeName = company.TradeName,
            DocumentNumber = company.DocumentNumber,
            StateRegistration = company.StateRegistration,
            TaxRegime = company.TaxRegime,
            FiscalSeries = series,
            Environment = company.Environment,
            Address = new FiscalAddressSnapshotState
            {
                PostalCode = company.PostalCode,
                Street = company.Street,
                StreetNumber = company.StreetNumber,
                District = company.District,
                City = company.City,
                StateCode = company.StateCode,
                CityIbgeCode = company.CityIbgeCode,
                Country = company.Country,
                Complement = company.Complement,
                ReferencePoint = null
            }
        };

    private static FiscalRecipientSnapshotState BuildRecipientSnapshot(
        Domain.Customers.Customer? customer,
        string recipientName,
        string? recipientDocument)
        => new()
        {
            CustomerId = customer?.Id,
            Name = recipientName,
            DocumentNumber = recipientDocument,
            StateRegistration = customer?.StateRegistration,
            TaxpayerIndicator = customer?.TaxpayerIndicator ?? "NaoContribuinte",
            Email = customer?.Email,
            Phone = customer?.Phone,
            Address = new FiscalAddressSnapshotState
            {
                PostalCode = customer?.PostalCode ?? string.Empty,
                Street = customer?.Street ?? string.Empty,
                StreetNumber = customer?.StreetNumber ?? string.Empty,
                District = customer?.District ?? string.Empty,
                City = customer?.City ?? string.Empty,
                StateCode = customer?.State ?? string.Empty,
                CityIbgeCode = customer?.CityIbgeCode,
                Country = "Brasil",
                Complement = customer?.Complement,
                ReferencePoint = customer?.ReferencePoint
            }
        };

    private static string ResolveFreightMode(string? mode)
    {
        var normalized = Normalize(mode)?.ToLowerInvariant();
        if (normalized is null)
        {
            return "Sem frete";
        }

        if (normalized.Contains("retirada", StringComparison.Ordinal))
        {
            return "Sem frete";
        }

        if (normalized.Contains("terceir", StringComparison.Ordinal) || normalized.Contains("coleta", StringComparison.Ordinal))
        {
            return "Terceiros";
        }

        if (normalized.Contains("entrega", StringComparison.Ordinal))
        {
            return "Emitente";
        }

        return "Sem frete";
    }

    private static decimal CalculateTaxAmount(decimal baseAmount, decimal rate)
        => RoundCurrency(baseAmount * Math.Max(0, rate) / 100m);

    private static decimal RoundCurrency(decimal value)
        => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private FiscalCompanyProfileState ResolveCompanyLocked(Guid? companyProfileId)
    {
        if (companyProfileId is not null)
        {
            return stateStore.FiscalCompanies.SingleOrDefault(x => x.Id == companyProfileId.Value)
                ?? throw new InvalidOperationException("Empresa fiscal nao encontrada.");
        }

        return stateStore.FiscalCompanies.OrderBy(x => x.TradeName).FirstOrDefault()
            ?? throw new InvalidOperationException("Nenhuma empresa fiscal cadastrada.");
    }

    private FiscalCompanyReadiness EvaluateCompanyReadinessLocked(FiscalCompanyProfileState company)
    {
        var blockingIssues = new List<string>();
        var pendingActions = new List<string>();

        if (Normalize(company.TradeName) is null)
        {
            blockingIssues.Add("Razao social do emitente nao preenchida.");
        }

        if (!IsValidCnpj(company.DocumentNumber))
        {
            blockingIssues.Add("CNPJ do emitente invalido.");
        }

        if (Normalize(company.StateRegistration) is null)
        {
            blockingIssues.Add("Inscricao estadual nao preenchida.");
        }

        if (Normalize(company.TaxRegime) is null)
        {
            blockingIssues.Add("Regime tributario nao preenchido.");
        }

        if (Normalize(company.PostalCode) is null ||
            Normalize(company.Street) is null ||
            Normalize(company.StreetNumber) is null ||
            Normalize(company.District) is null ||
            Normalize(company.City) is null ||
            Normalize(company.StateCode) is null ||
            Normalize(company.CityIbgeCode) is null ||
            Normalize(company.Country) is null)
        {
            blockingIssues.Add("Endereco fiscal do emitente esta incompleto.");
        }

        if (Normalize(company.CityIbgeCode) is not null && !IsValidMunicipalityCode(company.CityIbgeCode))
        {
            blockingIssues.Add("Codigo IBGE do municipio do emitente deve ter 7 digitos.");
        }

        if (!string.Equals(company.City, "Sao Paulo", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(company.StateCode, "SP", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(company.Country, "Brasil", StringComparison.OrdinalIgnoreCase))
        {
            blockingIssues.Add("Primeiro rollout fiscal esta restrito a Sao Paulo/SP, Brasil.");
        }

        var principalEmissionMode = Normalize(company.PrincipalEmissionMode)?.ToUpperInvariant();
        if (principalEmissionMode is not ("A1" or "A3"))
        {
            blockingIssues.Add("Meio principal do emitente deve ser A1 ou A3.");
        }

        var adapterName = Normalize(company.AdapterName);
        if (adapterName is null)
        {
            blockingIssues.Add("Adapter fiscal nao configurado.");
        }
        else if (adapterName is not ("mock-plugavel" or "unimake.dfe"))
        {
            blockingIssues.Add("Adapter fiscal configurado nao e suportado pelo PackControl.");
        }

        if (principalEmissionMode == "A1" && Normalize(company.CertificateLabel) is null)
        {
            blockingIssues.Add("Certificado A1 sem identificacao cadastrada.");
        }

        if (principalEmissionMode == "A3")
        {
            if (Normalize(company.CertificateSerialNumber) is null)
            {
                blockingIssues.Add("Certificado A3 sem serial cadastrado.");
            }

            if (!stateStore.FiscalAgents.Any(x => x.Online))
            {
                blockingIssues.Add("Nenhum agente A3 online para este emitente.");
            }
        }

        if (!company.NfeEnabled)
        {
            pendingActions.Add("NF-e ainda nao foi habilitada para o emitente.");
        }

        if (!company.AccountantValidated)
        {
            pendingActions.Add("Matriz fiscal aguardando validacao do contador.");
        }

        if (!company.HomologationCredentialsValidated)
        {
            pendingActions.Add("Credenciais e certificado ainda nao foram validados em homologacao.");
        }

        if (!company.HomologationApproved)
        {
            pendingActions.Add("Emitente ainda nao homologado no fluxo fiscal.");
        }

        if (!company.ProductionCredentialsValidated)
        {
            pendingActions.Add("Credenciais de producao ainda nao foram validadas.");
        }

        if (!company.ProductionApproved)
        {
            pendingActions.Add("Emitente ainda nao foi liberado para producao.");
        }

        var coreReady = blockingIssues.Count == 0;
        var canStartHomologation = coreReady &&
            company.NfeEnabled &&
            company.AccountantValidated &&
            company.HomologationCredentialsValidated;
        var canGoLive = canStartHomologation &&
            company.HomologationApproved &&
            company.ProductionCredentialsValidated &&
            company.ProductionApproved;
        var canIssueInCurrentEnvironment = string.Equals(company.Environment, "Producao", StringComparison.OrdinalIgnoreCase)
            ? canGoLive
            : canStartHomologation;

        var status = !coreReady
            ? "Rascunho"
            : !company.NfeEnabled || !company.AccountantValidated || !company.HomologationCredentialsValidated
                ? "Aguardando validacao"
                : !company.HomologationApproved
                    ? "Pronto para homologacao"
                    : !company.ProductionCredentialsValidated
                        ? "Homologado"
                        : !company.ProductionApproved
                            ? "Pronto para producao"
                            : "Liberado";

        return new FiscalCompanyReadiness(
            status,
            canStartHomologation,
            canIssueInCurrentEnvironment,
            canGoLive,
            blockingIssues,
            pendingActions);
    }

    private static void EnsureCompanyEmissionAllowed(FiscalCompanyReadiness readiness, FiscalCompanyProfileState company)
    {
        if (readiness.CanIssueInCurrentEnvironment)
        {
            return;
        }

        var reasons = readiness.BlockingIssues.Count > 0
            ? readiness.BlockingIssues
            : readiness.PendingActions;

        throw new InvalidOperationException(
            $"Emitente fiscal {company.TradeName} ainda nao esta pronto para emissao em {company.Environment}: {string.Join(" | ", reasons)}");
    }

    private static bool IsValidCnpj(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 14;
    }

    private static bool IsValidMunicipalityCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 7;
    }

    private static IReadOnlyList<string> GetRealEmissionStructuralGaps() =>
    [
        "Fluxo A3 com agente local e assinatura fora do servidor ainda nao foi concluido.",
        "Representacao DANFE do adapter real ainda esta em modo simplificado."
    ];

    private async Task<StoredFileDescriptor> SaveTextFileAsync(
        string fileName,
        string contentType,
        string content,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return await fileStorage.SaveAsync(stream, fileName, contentType, cancellationToken);
    }

    private async Task<StoredFileDescriptor?> SaveOptionalTextFileAsync(
        string fileName,
        string contentType,
        string? content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return await SaveTextFileAsync(fileName, contentType, content, cancellationToken);
    }

    private int NextAttemptNumberLocked(Guid fiscalDocumentId, string operation)
        => stateStore.FiscalTransmissionAttempts.Count(x =>
            x.FiscalDocumentId == fiscalDocumentId &&
            string.Equals(x.Operation, operation, StringComparison.OrdinalIgnoreCase)) + 1;

    private int NextCorrectionLetterSequenceLocked(Guid fiscalDocumentId)
        => stateStore.FiscalTransmissionAttempts.Count(x =>
            x.FiscalDocumentId == fiscalDocumentId &&
            string.Equals(x.Operation, "correction_letter", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Status, "Succeeded", StringComparison.OrdinalIgnoreCase)) + 1;

    private void UpdateLegacyInvoiceStatusLocked(Guid fiscalDocumentId, string status)
    {
        var legacyInvoice = stateStore.FiscalInvoices.SingleOrDefault(x => x.Id == fiscalDocumentId);
        if (legacyInvoice is not null)
        {
            legacyInvoice.Status = status;
        }
    }

    private static int ParseFiscalNumber(string? value)
    {
        if (!int.TryParse(value, out var parsed) || parsed <= 0)
        {
            throw new InvalidOperationException("Numero fiscal do documento nao e valido para a operacao solicitada.");
        }

        return parsed;
    }

    private static string NormalizeEventReason(string? value, string operationName)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            throw new InvalidOperationException($"Justificativa de {operationName} e obrigatoria.");
        }

        if (normalized.Length < 15)
        {
            throw new InvalidOperationException($"Justificativa de {operationName} deve ter pelo menos 15 caracteres.");
        }

        if (normalized.Length > 255)
        {
            throw new InvalidOperationException($"Justificativa de {operationName} deve ter no maximo 255 caracteres.");
        }

        return normalized;
    }

    private static string NormalizeCorrectionText(string? value)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            throw new InvalidOperationException("Texto da CC-e e obrigatorio.");
        }

        if (normalized.Length < 15)
        {
            throw new InvalidOperationException("Texto da CC-e deve ter pelo menos 15 caracteres.");
        }

        if (normalized.Length > 1000)
        {
            throw new InvalidOperationException("Texto da CC-e deve ter no maximo 1000 caracteres.");
        }

        return normalized;
    }

    private static string EnsureSeries(string? value)
        => Normalize(value) ?? throw new InvalidOperationException("Serie fiscal e obrigatoria para inutilizacao.");

    private static bool RangesOverlap(int startA, int endA, int startB, int endB)
        => startA <= endB && startB <= endA;

    private sealed record FiscalCompanyReadiness(
        string Status,
        bool CanStartHomologation,
        bool CanIssueInCurrentEnvironment,
        bool CanGoLive,
        IReadOnlyList<string> BlockingIssues,
        IReadOnlyList<string> PendingActions);
}
