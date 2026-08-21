using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Fiscal;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fiscal")]
public sealed class FiscalController(IFiscalDocumentService fiscalDocumentService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken)
    {
        var overview = await fiscalDocumentService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpGet("engine-diagnostic")]
    public async Task<IActionResult> EngineDiagnostic([FromQuery] Guid? companyProfileId, CancellationToken cancellationToken)
    {
        var diagnostic = await fiscalDocumentService.GetEngineDiagnosticAsync(companyProfileId, cancellationToken);
        return Ok(diagnostic);
    }

    [HttpPost("documents/prepare")]
    public async Task<IActionResult> Prepare([FromBody] PrepareFiscalDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await fiscalDocumentService.PrepareAsync(command, cancellationToken);
        return Ok(document);
    }

    [HttpPost("documents/issue")]
    public async Task<IActionResult> Issue([FromBody] IssueFiscalDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await fiscalDocumentService.IssueAsync(command, cancellationToken);
        return Ok(document);
    }

    [HttpPost("documents/cancel")]
    public async Task<IActionResult> Cancel([FromBody] CancelFiscalDocumentCommand command, CancellationToken cancellationToken)
    {
        var document = await fiscalDocumentService.CancelAsync(command, cancellationToken);
        return Ok(document);
    }

    [HttpPost("documents/correction-letter")]
    public async Task<IActionResult> ApplyCorrectionLetter(
        [FromBody] ApplyFiscalCorrectionLetterCommand command,
        CancellationToken cancellationToken)
    {
        var document = await fiscalDocumentService.ApplyCorrectionLetterAsync(command, cancellationToken);
        return Ok(document);
    }

    [HttpPost("numbering/inutilize")]
    public async Task<IActionResult> InutilizeNumberRange(
        [FromBody] InutilizeFiscalNumberRangeCommand command,
        CancellationToken cancellationToken)
    {
        var overview = await fiscalDocumentService.InutilizeNumberRangeAsync(command, cancellationToken);
        return Ok(overview);
    }

    [HttpPut("company-profiles/{companyProfileId:guid}")]
    public async Task<IActionResult> UpdateCompany(
        Guid companyProfileId,
        [FromBody] UpdateFiscalCompanyProfileCommand command,
        CancellationToken cancellationToken)
    {
        var overview = await fiscalDocumentService.UpdateCompanyProfileAsync(companyProfileId, command, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("operation-templates")]
    public async Task<IActionResult> CreateOperationTemplate(
        [FromBody] UpsertFiscalOperationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var overview = await fiscalDocumentService.UpsertOperationTemplateAsync(null, command, cancellationToken);
        return Ok(overview);
    }

    [HttpPut("operation-templates/{templateId:guid}")]
    public async Task<IActionResult> UpdateOperationTemplate(
        Guid templateId,
        [FromBody] UpsertFiscalOperationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var overview = await fiscalDocumentService.UpsertOperationTemplateAsync(templateId, command, cancellationToken);
        return Ok(overview);
    }
}
