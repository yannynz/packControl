# Backlog Tecnico - Modulo Fiscal NF-e

Versao: 0.4  
Data: 2026-03-28  
Status: Backlog de producao fechado por fases para emissao real junto ao SEFAZ, com eventos fiscais canonicos executados no ERP

## 1. Objetivo

Transformar o modulo fiscal `NF-e` do PackControl em uma trilha executavel de producao, cobrindo:
- emissao real em `Homologacao` e `Producao`;
- `A1` centralizado e `A3` via agente local;
- `cancelamento`, `inutilizacao`, `CC-e`, consulta e reprocesso;
- arquivo fiscal persistente;
- monitoracao, suporte e go-live controlado por emitente.

Este backlog deriva de:
- `docs/prd-modulo-fiscal-nfe.md`
- `docs/sds-modulo-fiscal-nfe.md`
- `docs/checklist-implantacao-fiscal-emitente.md`
- `docs/runbook-implantacao-fiscal.md`

## 2. Premissas fechadas

- o alvo e emissao real junto ao SEFAZ, nao validacao com `mock`;
- a arquitetura aprovada e `Fiscal Core` proprio + `adapter` real + agente local para `A3`;
- a baseline tecnica inicial da integracao real sera `Unimake.DFe`;
- `A3` faz parte do produto e nao sera tratado como excecao manual;
- `MDF-e` nao bloqueia o primeiro go-live de `NF-e`;
- o go-live sera feito por emitente, com liberacao controlada.
- o primeiro rollout fica restrito a emitentes operando em Sao Paulo/SP, Brasil;
- cada emitente entra com um unico meio principal de emissao, `A1` ou `A3`.

## 3. Checkpoint atual do repositorio

Estado validado em `2026-03-27`:
- camada fiscal canonica ja existe com `overview`, `prepare`, `issue`, empresa emissora e templates;
- financeiro e configuracoes ja usam a camada fiscal nova;
- cliente e produto ja alimentam o fiscal com `IBGE` do municipio, IE/indicador do destinatario e defaults tributarios por item;
- o documento preparado ja congela emitente, destinatario, itens, totais, pagamento e transporte em snapshot fiscal proprio;
- `XML` e `DANFE` ja sao arquivados no fluxo atual;
- o backend ja roteia adapters por emitente, com `mock-plugavel` para smoke e `unimake.dfe` com builder `XML 55`, assinatura/transmissao/recibo/protocolo na trilha real `A1`;
- ja existe endpoint de diagnostico do engine (`/api/fiscal/engine-diagnostic`) para testar o servico `NF-e` sem sair da plataforma;
- a emissao real agora barra explicitamente falta de certificado/configuracao antes de tentar seguir no adapter;
- a camada canonica agora tambem cobre `cancelamento`, `CC-e` e inutilizacao de faixa, com timeline e artefatos no ERP;
- o `mock-plugavel` e o `unimake.dfe` agora compartilham a trilha tecnica desses eventos, usando `RecepcaoEvento` e `Inutilizacao` no adapter real;
- ainda faltam homologacao real por emitente, `A3` operacional, `DANFE` oficial, classificacao mais forte de rejeicoes e gates de producao.

Leitura pratica:
- o backlog nao comeca do zero;
- a execucao deve partir do checkpoint atual e endurecer o que ja existe.

## 4. Fases de entrega

| Fase | Nome | Foco | Saida obrigatoria | Gate |
|---|---|---|---|---|
| `F0` | Preparacao fiscal real | emitente, matriz, credenciamento, ambientes e segredos | operacao apta a homologar | liberacao para construir integracao real |
| `F1` | Hardening do core fiscal | precondicoes, snapshots, persistencia e UX fiscal | core apto a sustentar emissao oficial | liberacao para `A1` real |
| `F2` | Emissao real `A1` | XML, assinatura, transmissao, consulta e protocolo | `NF-e` autorizada em homologacao com `A1` | liberacao para eventos fiscais |
| `F3` | Eventos e arquivo fiscal | cancelamento, inutilizacao, `CC-e`, consulta, reprocesso e archive | ciclo fiscal minimo completo | liberacao para trilha `A3` |
| `F4` | Agente `A3` | job fiscal, heartbeat, assinatura local, diagnostico e retorno | `NF-e` homologada com `A3` | liberacao para go-live multimeio |
| `F5` | Operacao e go-live | observabilidade, runbook, smoke real e rollout controlado | primeiro emitente operando em producao | go-live fechado |

## 5. Backlog detalhado por fase

### `F0` - Preparacao fiscal real

Objetivo:
- fechar os insumos sem os quais a integracao real com SEFAZ vira tentativa cega.

| ID | Item | Detalhamento | Dependencias | Saida |
|---|---|---|---|---|
| `NF-00` | definir emitente inicial | fechar primeiro `CNPJ`, cidade/`UF`, regime, autorizador e meio principal do rollout | - | alvo de go-live |
| `NF-01` | fechar matriz fiscal inicial | validar `CFOP`, natureza, finalidade, regras de frete e excecoes com contador | `NF-00` | matriz fiscal aprovada |
| `NF-02` | fechar credenciamento e ambientes | listar o que e exigido para homologacao/producao do emitente alvo | `NF-00` | checklist de credenciamento |
| `NF-03` | inventariar certificados | mapear `A1`, `A3`, validade, serial, midia e plano de renovacao | `NF-00` | inventario de certificados |
| `NF-04` | fechar storage e segredos | definir `PostgreSQL`, storage persistente, vault e estrategia de backup/restore | `NF-00` | infraestrutura fiscal minima |
| `NF-05` | fechar politica de ambientes | segregar `Homologacao` e `Producao`, serie, numeracao e segredos | `NF-02`,`NF-04` | baseline de ambientes |

Criterio de aceite:
- existe emitente alvo definido;
- o emitente alvo esta dentro do recorte Sao Paulo/SP, Brasil;
- existe checklist de credenciamento por emitente;
- o meio principal do emitente esta fechado como `A1` ou `A3`;
- existe matriz fiscal aprovada pelo contador;
- storage, banco, segredos e segregacao de ambientes estao decididos.

### `F1` - Hardening do core fiscal

Objetivo:
- transformar a espinha dorsal atual em um core fiscal de producao.

| ID | Item | Detalhamento | Dependencias | Saida |
|---|---|---|---|---|
| `NF-10` | endurecer precondicoes fiscais | validar emitente, destinatario, itens, `NCM`, `CFOP`, totais, endereco e forma de pagamento antes de assinar | `NF-01`,`NF-05` | validador fiscal forte |
| `NF-11` | fechar snapshot fiscal | persistir congelamento completo do documento fiscal, inclusive itens e totais | `NF-10` | base tributaria imutavel |
| `NF-12` | classificar falhas e rejeicoes | separar erro de dados, schema, certificado, autorizador, infra e contingencia | `NF-10` | taxonomia de erro fiscal |
| `NF-13` | persistencia relacional fiscal | sair do snapshot generico para tabelas fiscais relacionais com migrations formais | `NF-04`,`NF-11` | storage transacional fiscal |
| `NF-14` | sequence guard e idempotencia | proteger numeracao por emitente/serie/ambiente e evitar emissao duplicada | `NF-13` | numeracao segura |
| `NF-15` | secret vault fiscal | criptografar `PFX`, senha e segredos de integracao fora de config plana | `NF-04`,`NF-13` | segredos prontos para producao |
| `NF-16` | timeline e UX operacional | completar timeline, erros classificados, downloads e evidencias fiscais na UI | `NF-12`,`NF-13` | operacao fiscal legivel |
| `NF-17` | conciliacao com pedido/financeiro | garantir reflexo idempotente de status fiscal sem retrabalho manual | `NF-11`,`NF-14` | visao integrada segura |

Criterio de aceite:
- documento entra em `Draft`, passa por validacao e so segue quando apto;
- numeracao esta protegida por emitente/serie/ambiente;
- falhas e rejeicoes aparecem classificadas;
- pedido e financeiro refletem o status fiscal sem duplicidade;
- o core nao depende mais de `mock` para parecer funcional.

### `F2` - Emissao real `A1`

Objetivo:
- colocar a primeira trilha oficial de emissao real de pe com `A1`.

| ID | Item | Detalhamento | Dependencias | Saida |
|---|---|---|---|---|
| `NF-20` | implementar adapter real | criar adaptador SEFAZ real sobre `Unimake.DFe`, isolado por porta de infraestrutura. Ja executado para roteamento por emitente, consulta real de status e emissao `A1` com builder/transmissao/recibo/protocolo. | `NF-13`,`NF-15` | adapter de producao |
| `NF-21` | builder XML oficial | gerar `XML` `NF-e` modelo `55` conforme leiaute vigente e matriz fiscal aprovada. Ja executado na trilha `A1`, com validacao de `IBGE`, `NCM`, `CFOP`, documento e totais. | `NF-11`,`NF-20` | XML valido |
| `NF-22` | assinatura digital `A1` | carregar `PFX`, assinar, validar cadeia e tratar erros de certificado. Ja executado no adapter; falta homologar com certificado real do emitente. | `NF-15`,`NF-21` | assinatura `A1` funcional |
| `NF-23` | transmissao e consulta | enviar lote/documento, consultar recibo, protocolo e status do autorizador. Ja executado no adapter; falta fechar homologacao real do emitente. | `NF-20`,`NF-22` | autorizacao real |
| `NF-24` | arquivamento oficial | persistir `XML` enviado, retorno, `XML` autorizado, `DANFE` e hashes. Parcialmente executado com `XML` distribuido/artefatos; falta `DANFE` oficial. | `NF-23` | archive fiscal real |
| `NF-25` | homologacao `A1` | executar cenarios reais de homologacao do emitente alvo com checklist e evidencias | `NF-23`,`NF-24` | `A1` homologado |

Criterio de aceite:
- uma `NF-e` real e autorizada em homologacao com `A1`;
- protocolo, chave e artefatos oficiais ficam arquivados;
- consulta posterior do documento funciona;
- falha de transmissao nao gera duplicidade.

### `F3` - Eventos e arquivo fiscal

Objetivo:
- completar o ciclo minimo obrigatorio de producao alem da emissao inicial.

| ID | Item | Detalhamento | Dependencias | Saida |
|---|---|---|---|---|
| `NF-30` | cancelar documento | implementar evento de cancelamento com justificativa, protocolo e reflexo interno. Camada canonica, `mock-plugavel` e adapter real `unimake.dfe` ja executados; falta homologar por emitente. | `NF-25` | cancelamento real |
| `NF-31` | inutilizar faixa | implementar inutilizacao com faixa, motivo, consulta e trilha. Camada canonica, `mock-plugavel` e adapter real `unimake.dfe` ja executados; falta homologar por emitente. | `NF-25` | inutilizacao real |
| `NF-32` | emitir `CC-e` | implementar correcao controlada com historico e artefato proprio. Camada canonica, `mock-plugavel` e adapter real `unimake.dfe` ja executados; falta homologar por emitente. | `NF-25` | `CC-e` real |
| `NF-33` | reconciliacao e consulta | rotinas de consulta posterior, polling, retry e sincronizacao de status | `NF-24`,`NF-30`,`NF-31`,`NF-32` | reconciliacao segura |
| `NF-34` | fila de reprocesso | reprocessar erro transitorio sem perder historico e sem duplicar documento | `NF-12`,`NF-33` | reprocesso controlado |
| `NF-35` | operacao de archive | downloads, retencao, restore e trilha por documento/evento | `NF-24`,`NF-33` | arquivo fiscal operacional |

Criterio de aceite:
- cancelamento, inutilizacao e `CC-e` funcionam em integracao real;
- consulta e reconciliacao corrigem divergencia sem editar documento original;
- restore de artefatos e download operam com controle de acesso.

### `F4` - Agente `A3`

Objetivo:
- sustentar assinatura e transmissao `A3` fora do servidor central.

| ID | Item | Detalhamento | Dependencias | Saida |
|---|---|---|---|---|
| `NF-40` | registrar agente `A3` | cadastro, autenticacao, heartbeat, status e vinculo com certificado | `NF-05`,`NF-13` | agente conhecido pelo ERP |
| `NF-41` | fila de jobs fiscais | despacho por `pull`, controle de concorrencia e correlacao de job/documento | `NF-17`,`NF-40` | execucao remota controlada |
| `NF-42` | diagnostico local | discovery de certificado, store, driver, middleware, serial e disponibilidade | `NF-40` | diagnostico `A3` operacional |
| `NF-43` | fluxo de `PIN` e assinatura | coletar `PIN` localmente, assinar/transmitir e devolver artefatos ao ERP | `NF-41`,`NF-42`,`NF-20` | assinatura `A3` funcional |
| `NF-44` | homologacao `A3` | executar homologacao real com `A3`, incluindo falha de driver/dispositivo | `NF-43` | `A3` homologado |
| `NF-45` | UX e suporte `A3` | exibir fila, diagnostico, ultimo sinal, erro legivel e orientacao operacional | `NF-42`,`NF-44` | suporte `A3` viavel |

Criterio de aceite:
- um documento entra em fila `A3`, e o agente conclui assinatura/transmissao;
- ausencia de dispositivo, erro de `PIN` e falha de driver ficam auditaveis e legiveis;
- `A3` homologado pelo menos para o emitente alvo que dependa dele.

### `F5` - Operacao, homologacao final e go-live

Objetivo:
- fechar o modulo como operacao suportavel, monitoravel e apta a faturamento oficial.

| ID | Item | Detalhamento | Dependencias | Saida |
|---|---|---|---|---|
| `NF-50` | monitoracao e alertas | metricas, logs, dashboards, alertas de certificado, storage, autorizador e fila | `NF-25`,`NF-35`,`NF-44` | observabilidade fiscal |
| `NF-51` | runbooks operacionais | emissao, rejeicao, reprocesso, cancelamento, inutilizacao, `CC-e`, restore e suporte `A3` | `NF-35`,`NF-45`,`NF-50` | operacao documentada |
| `NF-52` | suite de homologacao final | smoke guiado por emitente com cenarios `A1`, `A3`, eventos e restore | `NF-30`,`NF-31`,`NF-32`,`NF-44`,`NF-50` | homologacao consolidada |
| `NF-53` | smoke de producao controlado | emitir documento real de producao em janela controlada com observacao operacional | `NF-52` | prova real de producao |
| `NF-54` | rollout por emitente | liberar uso por empresa/CNPJ com checklist, aceite e janela de suporte assistido | `NF-53` | go-live controlado |
| `NF-55` | corte do emissor paralelo | remover dependencia operacional de emissao externa/manual para o emitente liberado | `NF-54` | operacao oficial no ERP |

Criterio de aceite:
- existe monitoracao minima de producao;
- existe runbook fechado e validado;
- existe pelo menos um emitente com `NF-e` real emitida em producao pelo ERP;
- a operacao desse emitente nao depende mais de emissor paralelo.

## 6. Ordem de execucao recomendada

Sequencia obrigatoria:
1. `F0`
2. `F1`
3. `F2`
4. `F3`
5. `F4`
6. `F5`

Paralelismo aceitavel:
- `NF-03` e `NF-04` podem correr em paralelo dentro de `F0`;
- `NF-16` e `NF-17` podem correr em paralelo ao endurecimento do core;
- `NF-35` pode ser desenvolvido em paralelo aos eventos de `F3`;
- `NF-45` pode correr em paralelo a `NF-44`.

## 7. Dependencias externas

- contador com matriz fiscal inicial e revisao das operacoes reais;
- credenciamento e habilitacao do emitente nos ambientes corretos;
- certificados validos e acessiveis;
- infraestrutura de `PostgreSQL`, storage persistente e vault;
- ambiente local viavel para o agente `A3`, quando aplicavel.

## 8. Gates de go-live

Nenhum emitente entra em producao sem:
- homologacao `A1` ou `A3` conforme o meio real do emitente;
- cancelamento, inutilizacao e `CC-e` validados;
- storage e restore testados;
- monitoracao e alertas ligados;
- runbook operacional aprovado;
- smoke real de producao executado com sucesso.

## 9. Riscos dominantes

- variacao de middleware e driver no `A3`;
- mudancas frequentes de `Nota Tecnica` e schema;
- matriz fiscal incompleta ou tardia;
- rejeicoes de autorizador nao cobertas na primeira rodada;
- dependencia de storage/segredos mal configurados.

## 10. Proxima tarefa recomendada

Pelo estado atual do codigo, a primeira fase acionavel e:
- iniciar `F0` com `NF-00` a `NF-05`, fechando emitente alvo, matriz fiscal, credenciamento, inventario de certificado e infraestrutura fiscal de producao.

Artefatos de apoio para execucao:
- `docs/checklist-implantacao-fiscal-emitente.md`
- `docs/runbook-implantacao-fiscal.md`
