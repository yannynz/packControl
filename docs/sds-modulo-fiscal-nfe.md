# SDS - Modulo Fiscal NF-e

Versao: 0.3  
Data: 2026-03-28  
Status: SDS de producao para emissao real de `NF-e` junto ao SEFAZ

Documento complementar:
- `docs/backlog-modulo-fiscal-nfe.md`

## 0.1 Estado atual x alvo deste SDS

Estado validado em `2026-03-27`:
- rotas ativas: `GET /api/fiscal/overview`, `POST /api/fiscal/documents/prepare`, `POST /api/fiscal/documents/issue`, `PUT /api/fiscal/company-profiles/{id}`, `POST/PUT /api/fiscal/operation-templates`;
- a camada canonica ja persiste `FiscalDocument`, `FiscalEvent`, `FiscalTransmissionAttempt`, `FiscalArtifact`, `FiscalOperationTemplate` e `FiscalAgentRegistration`;
- o financeiro ja emite pela camada canonica e a UI administrativa ja edita empresa emissora e templates;
- o adaptador `unimake.dfe` ja cobre builder `XML 55`, validacao fiscal basica, assinatura/transmissao/recibo/protocolo na trilha `A1`;
- a camada canonica e o adapter real agora tambem cobrem `cancelamento`, inutilizacao e `CC-e`;
- este checkpoint ainda nao representa go-live fiscal, porque faltam certificado real do emitente, homologacao real por emitente, `A3` operacional e `DANFE` oficial.

Este SDS descreve a arquitetura alvo de producao, nao o nivel atual do codigo.

## 1. Objetivo

Definir a arquitetura tecnica do modulo fiscal `NF-e` do PackControl, cobrindo:
- suporte a `A1` e `A3`;
- emissao real em `Homologacao` e `Producao`;
- motor plugavel, mas com implementacao inicial fechada para o go-live;
- acoplamento com pedido, financeiro e logistica;
- armazenamento de artefatos fiscais;
- homologacao e operacao assistida;
- criterios de escolha entre biblioteca open source e provedor externo.

## 2. Decisao arquitetural principal

O PackControl deve adotar arquitetura `hibrida e plugavel`:
- o `Fiscal Core` pertence ao ERP;
- assinatura/transmissao pertencem a um `adapter`;
- `A1` pode operar de forma centralizada;
- `A3` deve operar por agente local, nunca por upload de chave privada ao servidor central.

Traduzindo:
- o ERP conhece `FiscalDocument`, `Snapshot`, `Evento`, `Artefato`, `Tentativa`, `Status`;
- o ERP nao conhece detalhes de `SOAP`, `PFX`, `token`, `PIN`, `schema`, `WS URL` ou parser de resposta;
- esses detalhes ficam em `Infrastructure` e no `PackControl Fiscal Agent`.

Decisao fechada para producao:
- o go-live fiscal do PackControl sera sobre `Fiscal Core` proprio + `adapter` real ligado ao SEFAZ;
- a implementacao inicial deve usar biblioteca `.NET` madura, com `Unimake.DFe` como baseline tecnica principal;
- `mock`, `stub` ou emissor paralelo manual nao entram na arquitetura de producao;
- `A3` entra no go-live via agente local suportado e observavel.

## 2.1 Nao negociaveis de producao

- sem credenciamento, nao ha liberacao de emitente;
- sem storage persistente e restore validado, nao ha liberacao fiscal;
- sem trilha auditavel de emissao, evento e artefato, nao ha liberacao fiscal;
- sem `cancelamento`, `inutilizacao`, `CC-e` e consulta de protocolo, nao ha liberacao fiscal;
- sem segregacao forte entre `Homologacao` e `Producao`, nao ha liberacao fiscal.

## 3. Base externa considerada

Resumo da pesquisa em `2026-03-26`:
- o Portal da `NF-e` continua com servicos `4.00` publicados e com atualizacoes recentes de `Notas Tecnicas`;
- `NT 2025.001 v.1.02` e `NT 2025.002 v.1.34` mostram que o modulo fiscal precisara de manutencao recorrente;
- o material da `NF-e ABI` ja foi publicado em `24/11/2025`, o que reforca a necessidade de baixo acoplamento;
- certificados `A1` e `A3` seguem validos no ecossistema `ICP-Brasil`, mas `A3` continua preso a hardware;
- provedores cloud puros podem restringir `A3`, enquanto stacks hibridas exigem bridge local.

## 4. Principios tecnicos

- `adapter first`;
- `snapshot first`;
- `A3 local only`;
- `retry` com idempotencia;
- trilha imutavel para documento e eventos;
- artefato fiscal persistente e versionado;
- baixa dependencia de UI;
- compatibilidade com evolucao para outros DF-e no futuro.

## 5. Componentes da solucao

### 5.1 `Fiscal Core`

Responsavel por:
- agregado `FiscalDocument`;
- estados do documento;
- regras de transicao;
- snapshot fiscal;
- regras de numeração;
- governanca de eventos;
- autorizacao e auditoria.

### 5.2 `Fiscal Application`

Responsavel por:
- casos de uso;
- validacoes de precondicao;
- roteamento entre `A1`, `A3` e provedores;
- coordenacao de retries;
- conciliacao com pedido e financeiro;
- exposicao de endpoints internos e DTOs.

### 5.3 `Fiscal Infrastructure`

Responsavel por:
- adapters de transmissao;
- builder XML;
- assinatura;
- armazenamento de `XML`, retorno, protocolo e `DANFE`;
- conectores de biblioteca/provedor;
- persistencia e filas internas.

### 5.3.1 Componentes obrigatorios de producao

- `FiscalTransmissionWorker` para envio, polling e reconciliacao;
- `FiscalOutbox` para eventos internos e jobs do agente `A3`;
- `FiscalSecretVault` para `PFX`, senha e segredos de integracao;
- `FiscalSequenceGuard` para numeracao segura por emitente/serie/ambiente;
- `FiscalHealthMonitor` para storage, certificado, autorizador e filas.

### 5.4 `PackControl Fiscal Agent`

Responsavel por:
- operar certificados `A3` no ambiente local;
- expor capacidade do certificado ao ERP;
- receber jobs fiscais;
- solicitar `PIN` quando necessario;
- assinar/transmitir via adapter compatibilizado;
- devolver status, `XML`, protocolo e eventos.

### 5.5 `Fiscal Archive`

Responsavel por:
- armazenamento imutavel de artefatos;
- hash, metadado e integridade;
- consulta rapida;
- politica de retencao e backup.

## 6. Topologias suportadas

### 6.1 Topologia `A1` centralizada

Uso:
- emitente com `PFX`;
- assinatura e transmissao no backend central.

Vantagem:
- menor complexidade operacional.

Risco:
- exige tratamento rigoroso de segredo.

### 6.2 Topologia `A3` hibrida

Uso:
- certificado fisico em `token` ou `cartao`;
- agente local no ambiente do cliente;
- ERP central coordena, mas nao toca na chave privada.

Vantagem:
- atende `A3` sem violar o modelo de seguranca do hardware.

Risco:
- depende de `driver`, middleware, USB, PIN e sistema operacional do cliente.

### 6.3 Topologia mista

Uso:
- parte dos emitentes em `A1`;
- parte em `A3`.

Essa topologia deve ser tratada como caso normal, nao excecao.

## 7. Modelo de dominio proposto

### 7.1 Entidades principais

- `FiscalCompanyProfile`
- `FiscalCertificateProfile`
- `FiscalOperationTemplate`
- `FiscalDocument`
- `FiscalDocumentItemSnapshot`
- `FiscalTransmissionAttempt`
- `FiscalEvent`
- `FiscalArtifact`
- `FiscalNumberSequence`
- `FiscalProviderBinding`
- `FiscalAgentRegistration`

### 7.2 `FiscalDocument`

Campos minimos:
- `Id`
- `CompanyProfileId`
- `OrderId`
- `FinanceEntryId`
- `Environment`
- `Series`
- `Number`
- `AccessKey`
- `Status`
- `IssueMode`
- `AdapterName`
- `CertificateProfileId`
- `Protocol`
- `AuthorizationAtUtc`
- `CancellationAtUtc`
- `CurrentAttemptId`
- `CreatedBy`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### 7.3 Estados do documento

Estados sugeridos:
- `Draft`
- `PendingValidation`
- `ReadyToTransmit`
- `AwaitingA3Agent`
- `Signing`
- `Transmitting`
- `AwaitingReceipt`
- `Authorized`
- `Rejected`
- `Cancelled`
- `NumberUnused`
- `Error`

### 7.4 Eventos fiscais

Tipos minimos:
- `prepared`
- `signed`
- `submitted`
- `receipt_received`
- `authorized`
- `rejected`
- `cancel_requested`
- `cancel_authorized`
- `cce_requested`
- `cce_authorized`
- `number_unused_requested`
- `number_unused_authorized`
- `artifact_stored`
- `retry_scheduled`

## 8. Persistencia e storage

Persistencia recomendada:
- `PostgreSQL` relacional para entidades e eventos;
- object storage ou volume persistente para `XML`, `DANFE`, logs detalhados e anexos fiscais.

Tabelas recomendadas:
- `fiscal_company_profiles`
- `fiscal_certificate_profiles`
- `fiscal_operation_templates`
- `fiscal_documents`
- `fiscal_document_item_snapshots`
- `fiscal_transmission_attempts`
- `fiscal_events`
- `fiscal_artifacts`
- `fiscal_number_sequences`
- `fiscal_agent_registrations`

Nao armazenar:
- chave privada `A3`;
- `PFX` em texto plano;
- senha de certificado sem criptografia.

Obrigacoes adicionais de producao:
- criptografar `PFX` e senha com chave gerenciada fora do banco;
- versionar schema fiscal por `NT` e por ambiente;
- reter `XML`, `DANFE`, eventos e tentativas conforme politica fiscal da operacao;
- validar restore de artefatos e snapshot fiscal antes do go-live.

## 9. Abstracoes de codigo

Interfaces recomendadas:

```csharp
public interface IFiscalDocumentService
{
    Task<FiscalDocumentDto> PrepareAsync(PrepareFiscalDocumentCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> IssueAsync(IssueFiscalDocumentCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> CancelAsync(CancelFiscalDocumentCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> IssueCorrectionLetterAsync(IssueCorrectionLetterCommand command, CancellationToken cancellationToken);
    Task<FiscalDocumentDto> InvalidateNumberAsync(InvalidateFiscalNumberCommand command, CancellationToken cancellationToken);
}

public interface IFiscalTransportAdapter
{
    string Name { get; }
    Task<FiscalTransmissionResult> IssueAsync(FiscalTransmissionEnvelope envelope, CancellationToken cancellationToken);
    Task<FiscalEventResult> CancelAsync(FiscalEventEnvelope envelope, CancellationToken cancellationToken);
    Task<FiscalEventResult> CorrectionLetterAsync(FiscalEventEnvelope envelope, CancellationToken cancellationToken);
    Task<FiscalEventResult> InvalidateAsync(FiscalEventEnvelope envelope, CancellationToken cancellationToken);
    Task<FiscalStatusResult> QueryAsync(FiscalQueryEnvelope envelope, CancellationToken cancellationToken);
}

public interface IFiscalArtifactStore
{
    Task<FiscalArtifactRef> SaveAsync(FiscalArtifactWriteRequest request, CancellationToken cancellationToken);
}

public interface IFiscalAgentGateway
{
    Task<AgentDispatchResult> DispatchAsync(FiscalAgentJob job, CancellationToken cancellationToken);
}
```

## 10. Fluxo tecnico de emissao `A1`

1. `POST /api/fiscal/documents/prepare`
2. `Application` monta `snapshot`.
3. `Domain` valida estado e numeração.
4. `Infrastructure` monta envelope.
5. `POST /api/fiscal/documents/issue` com `fiscalDocumentId` ou emissao direta com `IssueFiscalDocumentCommand`.
6. `A1 adapter` assina e transmite.
7. `Application` grava tentativa, artefatos e evento.
8. `SignalR`/notificacao interna atualiza pedido e financeiro.

## 11. Fluxo tecnico de emissao `A3`

1. ERP monta `snapshot`.
2. ERP cria `FiscalAgentJob`.
3. Job entra em fila ou tabela de despacho.
4. `Fiscal Agent` autenticado busca o job.
5. Agente resolve certificado por serial/store/driver.
6. Agente assina e transmite localmente.
7. Agente faz `callback` ou devolve polling result.
8. ERP persiste protocolo, `XML`, `DANFE` e evento.

## 12. Desenho do agente `A3`

### 12.1 Formato

Servico local `Windows` ou tray app gerenciavel por implantacao.

### 12.2 Responsabilidades

- descobrir certificados `A3`;
- validar driver e middleware;
- manter heartbeat com o ERP;
- processar fila de jobs;
- solicitar `PIN` localmente;
- registrar logs operacionais locais;
- devolver artefatos e mensagens de erro legiveis.

### 12.3 Protocolo sugerido

Para simplicidade operacional:
- `pull` do agente para o ERP;
- `HTTPS` com `mTLS` ou token rotativo;
- sem necessidade de porta de entrada aberta na rede do cliente.

### 12.4 Motivo do `pull`

- evita expor endpoint no cliente;
- funciona melhor em ambiente com `NAT` e firewall corporativo;
- combina com o desenho atual do `Edge`.

## 13. Certificados e segredo

### 13.1 `A1`

Recomendacao:
- armazenar `PFX` cifrado com chave de aplicacao + `KMS`/segredo externo;
- nunca logar senha;
- versionar perfil de certificado;
- suportar `dual-running` na troca de certificado.

### 13.2 `A3`

Recomendacao:
- armazenar apenas metadados centrais;
- usar store do Windows ou middleware do fabricante no agente;
- manter cache de sessao apenas local e temporario;
- nunca persistir `PIN` no ERP.

## 14. Escolha de biblioteca/adapter

### 14.1 `Unimake.DFe`

Leitura tecnica:
- boa candidata para baseline do adapter;
- `MIT`;
- pacote atualizado recentemente;
- suporte amplo a DF-e;
- documentacao publica sobre `A3`.

Recomendacao:
- usar em `spike` principal do adapter proprio.

### 14.2 Ecossistema `Zeus`

Leitura tecnica:
- maduro no ecossistema `.NET`;
- bom historico comunitario;
- `DANFE` separado;
- docs publicas de `A3` via store do Windows.

Recomendacao:
- usar como segunda opcao de `spike`, especialmente se houver gap de `DANFE` ou compatibilidade.

### 14.3 Provedor externo

Leitura tecnica:
- bom para reduzir esforco com `schema`, `SOAP` e mudancas regulatórias;
- ruim quando o produto precisa controlar `A3` remotamente sem bridge local.

Recomendacao:
- manter adapter pronto para provedor;
- nao deixar o dominio do ERP dependente do payload do fornecedor.

## 15. Escolha recomendada para o PackControl

Arquitetura recomendada:
- `Fiscal Core` proprio;
- `Transport Adapter` plugavel;
- `A3 Agent` proprio ou white-label operado conosco;
- `spike` tecnico com `Unimake.DFe`;
- `spike` secundario com ecossistema `Zeus` apenas para comparacao de `DANFE` e compatibilidade.

Motivo:
- atende `A1` e `A3`;
- preserva autonomia de produto;
- evita que a regra de negocio do ERP more no fornecedor;
- permite migrar para provedor externo se o cronograma apertar.

## 16. Requisitos de integracao com outros modulos

### 16.1 Orders

- bloquear emissao sem pedido apto;
- refletir numero/chave/status fiscal;
- permitir reemissao controlada.

### 16.2 Finance

- vincular titulo, parcela e baixa;
- separar `preparar`, `emitir`, `cancelar`.

### 16.3 Logistics

- disponibilizar dados de transportadora, modalidade e rastreio do documento.

### 16.4 Audit

- registrar quem fez, quando fez, o que fez e qual foi o retorno.

### 16.5 Settings

- empresas, certificados, templates, series, ambiente, adapter ativo.

## 17. API interna recomendada

Rotas implementadas no checkpoint atual:
- `GET /api/fiscal/overview`
- `POST /api/fiscal/documents/prepare`
- `POST /api/fiscal/documents/issue`
- `PUT /api/fiscal/company-profiles/{id}`
- `POST /api/fiscal/operation-templates`
- `PUT /api/fiscal/operation-templates/{id}`

Rotas futuras recomendadas:
- `GET /api/fiscal/overview`
- `GET /api/fiscal/documents/{id}`
- `POST /api/fiscal/documents/prepare`
- `POST /api/fiscal/documents/issue`
- `POST /api/fiscal/documents/{id}/cancel`
- `POST /api/fiscal/documents/{id}/cce`
- `POST /api/fiscal/number-ranges/invalidate`
- `GET /api/fiscal/documents/{id}/artifacts`
- `GET /api/fiscal/company-profiles`
- `POST /api/fiscal/company-profiles`
- `POST /api/fiscal/certificates/test`

Rotas do agente:
- `POST /internal/fiscal-agent/register`
- `POST /internal/fiscal-agent/heartbeat`
- `POST /internal/fiscal-agent/pull`
- `POST /internal/fiscal-agent/jobs/{id}/complete`
- `POST /internal/fiscal-agent/jobs/{id}/fail`

## 18. Observabilidade

Logs obrigatorios:
- `document_id`
- `order_id`
- `company_id`
- `certificate_profile_id`
- `adapter_name`
- `environment`
- `attempt_number`
- `sefaz_status_code`
- `latency_ms`
- `agent_id` quando `A3`

Metricas:
- tempo de autorizacao;
- taxa de rejeicao;
- taxa de erro por adapter;
- taxa de erro por emitente;
- fila `A3` pendente;
- certificado proximo de expirar;
- tempo medio para resolver rejeicao.

Alertas:
- certificado vencendo em `30`, `15`, `7` e `1` dias;
- `A3 agent` offline;
- pico de rejeicoes;
- indisponibilidade de autorizador;
- falha de storage.

## 19. Seguranca

- criptografia de segredo em repouso;
- `mTLS` ou token rotativo entre agente e ERP;
- RBAC forte para acoes fiscais;
- mascaracao de segredos em log;
- segregacao de `Homologacao` e `Producao`;
- trilha imutavel de cancelamento, `CC-e` e inutilizacao;
- revisao manual obrigatoria para mudanca de serie/numeracao;
- `PFX` e senha sempre fora de configuracao plana;
- operacoes de producao exigem trilha de usuario, emitente, serie e ambiente;
- certificados com alerta ativo de expiracao e bloqueio proximo ao vencimento critico.

## 20. Testes

### 20.1 Unitarios

- transicoes de estado;
- numeracao;
- validacoes de precondicao;
- classificacao de erro.

### 20.2 Integracao

- adapter `A1`;
- storage de artefatos;
- persistencia e reprocesso;
- callback do agente `A3`.

### 20.3 Contrato

- payload do adapter;
- resposta autorizada;
- resposta rejeitada;
- cancelamento;
- `CC-e`;
- inutilizacao.

### 20.4 Homologacao

- emissao `A1`;
- emissao `A3`;
- cancelamento;
- inutilizacao;
- `CC-e`;
- rejeicoes reais selecionadas;
- troca de ambiente;
- troca de certificado.

### 20.5 Gate de producao

- smoke real por emitente em ambiente de producao controlado;
- conciliacao entre pedido, financeiro e documento emitido;
- restore de `XML` e `DANFE` validado;
- fallback operacional e runbook testados;
- monitoracao e alertas disparando como esperado.

## 21. Fases de entrega e estimativa de producao

### Fase 1 - Base fiscal e `A1`

Escopo:
- entidades, `snapshot`, storage, adapter e emissao `A1`.

Estimativa:
- `3 a 5 semanas`.

### Fase 2 - Eventos e arquivo fiscal

Escopo:
- cancelamento, inutilizacao, `CC-e`, `DANFE`, consulta, timeline e auditoria.

Estimativa:
- `2 a 3 semanas`.

### Fase 3 - `A3 agent`

Escopo:
- registro do agente, discovery de certificado, fila, `PIN`, retorno e observabilidade.

Estimativa:
- `3 a 5 semanas`.

### Fase 4 - Homologacao e rollout

Escopo:
- cenarios reais, credenciamento, runbooks e suporte de campo.

Estimativa:
- `2 a 4 semanas`.

Faixa total recomendada:
- `10 a 17 semanas`, dependendo da profundidade da homologacao e da variabilidade do `A3`.

## 22. Decisao tecnica de producao

Para o PackControl:
- construir `Fiscal Core`, persistencia, timeline, arquivo fiscal, UX e agente `A3`;
- plugar a mensageria por `adapter`, mas com integracao real obrigatoria junto ao SEFAZ;
- iniciar com `Unimake.DFe` como baseline tecnica principal;
- manter fallback para provider apenas se preservar dominio, storage, numeracao e auditoria do ERP;
- tratar `A3` como fluxo nativo do produto, nao como excecao manual.

## 23. Premissas fechadas para o primeiro go-live

- o primeiro rollout fiscal sai por emitente, com liberacao controlada por empresa/CNPJ;
- o primeiro rollout fiscal fica restrito a emitentes operando em Sao Paulo/SP, Brasil;
- a matriz fiscal inicial sera entregue e validada com apoio do contador antes da producao;
- `A1` e `A3` precisam estar cobertos pela arquitetura desde a primeira versao produtiva;
- cada emitente entra em go-live com um unico meio principal ativo, `A1` ou `A3`;
- o outro meio pode existir como contingencia operacional ou preparo de futura ampliacao, mas nao bloqueia a entrada do emitente atual;
- contingencia legal pode entrar de forma incremental, mas emissao normal, cancelamento, inutilizacao e `CC-e` sao gates obrigatorios;
- `MDF-e` nao bloqueia o primeiro go-live fiscal de `NF-e`.

## 24. Referencias externas consultadas em 2026-03-26

- Portal da NF-e: https://www.nfe.fazenda.gov.br/portal/principal.aspx
- Lista de Notas Tecnicas do Portal NF-e: https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=04BIflQt1aY=
- Relacao de servicos web da NF-e: https://www.nfe.fazenda.gov.br/portal/webServices.aspx?tipoConteudo=OUC/YVNWZfo=
- Historico do MOC NF-e ABI: https://hom.nfe.fazenda.gov.br/PORTAL/listaHistorico.aspx?tipoConteudo=Ef+Y1blZDbU=
- Gov.br - certificado digital `A1/A3`: https://www.gov.br/pt-br/servicos/obter-certificacao-digital
- ITI - FAQ certificacao digital: https://www.gov.br/iti/pt-br/acesso-a-informacao/perguntas-frequentes/certificacao-digital
- Unimake.DFe no GitHub: https://github.com/Unimake/DFe
- Unimake.DFe no NuGet: https://www.nuget.org/packages/Unimake.DFe/
- Unimake - videos/documentacao `A3`: https://wiki.unimake.com.br/index.php/Manuais:Unimake.DFe/VideosCsharp
- Zeus DFe.NET no GitHub: https://github.com/ZeusAutomacao/DFe.NET
- Zeus DFe.NET releases: https://github.com/ZeusAutomacao/DFe.NET/releases
- Zeus - orientacao publica de uso com `A3`: https://github.com/ZeusAutomacao/DFe.NET/issues/114
- Focus NFe - planos e limites de certificado: https://2025.focusnfe.com.br/precos/
- PlugNotas NFe: https://plugnotas.com.br/nfe/
- TecnoSpeed - informacoes gerais `A1/A3`: https://atendimento.tecnospeed.com.br/hc/pt-br/articles/4932353103127-Certificado-Digital-Informa%C3%A7%C3%B5es-Gerais
- TecnoSpeed - `A3` no Manager SaaS: https://atendimento.tecnospeed.com.br/hc/pt-br/articles/360011945834-Instalando-o-m%C3%B3dulo-de-certificados-A3-para-uso-no-Manager-SaaS
- TecnoSpeed - eventos RTC para NF-e: https://atendimento.tecnospeed.com.br/hc/pt-br/articles/36284032224663-Eventos-da-Reforma-Tribut%C3%A1ria-do-Consumo-para-a-NF-e
