# Runbook de Implantacao Fiscal

Versao: 0.1  
Data: 2026-03-28  
Status: Runbook operacional para homologacao, virada e go-live fiscal por emitente

Documento complementar:
- `docs/checklist-implantacao-fiscal-emitente.md`
- `docs/backlog-modulo-fiscal-nfe.md`
- `docs/prd-modulo-fiscal-nfe.md`
- `docs/sds-modulo-fiscal-nfe.md`
- `docs/runbook-deploy-packcontrol.md`

## 1. Objetivo

Padronizar a implantacao fiscal do PackControl por emitente, cobrindo:
- preparacao;
- homologacao;
- virada para producao;
- smoke controlado;
- rollback;
- estabilizacao inicial.

Este runbook vale por emitente. Nao existe liberacao fiscal global para todos os `CNPJs` de uma vez.

Premissas operacionais desta rodada:
- o primeiro rollout fiscal fica restrito a emitentes de Sao Paulo/SP, Brasil;
- cada emitente entra com um unico meio principal de emissao, `A1` ou `A3`;
- o produto continua suportando ambos, mas nao precisamos abrir os dois meios ao mesmo tempo para o mesmo emitente no primeiro go-live.

## 2. Papeis

| Papel | Responsabilidade |
|---|---|
| Implantacao | conduzir checklist, janela de virada e evidencias |
| Tecnologia | configurar ambiente, segredos, storage, monitoracao e suporte tecnico |
| Contador | validar matriz fiscal e excecoes |
| Operacao/Financeiro | executar homologacoes funcionais e validar documentos |
| Aprovador final | autorizar go-live e corte do emissor paralelo |

## 3. Pre-requisitos

Antes de iniciar:
- checklist por emitente aberto em `docs/checklist-implantacao-fiscal-emitente.md`;
- baseline de deploy tecnico validada com `GET /health/ready`;
- emitente alvo definido;
- acesso aos ambientes de homologacao e producao;
- certificado principal disponivel;
- storage persistente e restore testado;
- responsaveis nomeados e com agenda alinhada para a janela.

## 4. Sequencia operacional

### Etapa 1 - Preparacao do emitente

1. Preencher a identificacao do emitente.
2. Fechar `NF-00` a `NF-05`.
3. Validar matriz fiscal com contador.
4. Registrar serie, ambiente e politica de numeracao.
5. Confirmar meio principal do emitente: `A1` ou `A3`.
6. Registrar meio contingente/opcional, se houver.

Saida obrigatoria:
- checklist `F0` fechado.

### Etapa 2 - Homologacao interna do core

1. Validar precondicoes fiscais no ERP.
2. Confirmar snapshot fiscal completo.
3. Validar classificacao de erro e timeline.
4. Confirmar reflexo em pedido e financeiro.

Saida obrigatoria:
- checklist `F1` fechado.

### Etapa 3 - Homologacao real `A1`

Aplicar quando o emitente usar `A1`.

1. Configurar certificado `A1` no vault.
2. Executar emissao real em homologacao.
3. Validar protocolo, chave, `XML` e `DANFE`.
4. Reexecutar consulta posterior do documento.

Saida obrigatoria:
- checklist `F2` fechado.

### Etapa 4 - Eventos fiscais

1. Executar cancelamento em homologacao.
2. Executar inutilizacao em homologacao.
3. Executar `CC-e` em homologacao.
4. Validar reprocesso e restore de artefatos.

Saida obrigatoria:
- checklist `F3` fechado.

### Etapa 5 - Homologacao `A3`

Aplicar quando o emitente usar `A3`.

1. Instalar/configurar agente local.
2. Validar heartbeat e diagnostico.
3. Validar discovery do certificado.
4. Executar emissao real em homologacao com `A3`.
5. Simular falha de dispositivo/driver para validar suporte.

Saida obrigatoria:
- checklist `F4` fechado.

### Etapa 6 - Preparacao de producao

1. Ativar monitoracao e alertas.
2. Revisar runbook com operacao e suporte.
3. Validar rollback.
4. Aprovar janela de go-live.

Saida obrigatoria:
- checklist `F5` pronto para a janela.

### Etapa 7 - Go-live controlado

1. Confirmar janela aberta.
2. Confirmar que o emissor paralelo continua disponivel apenas como contingencia.
3. Executar smoke de producao com documento real controlado.
4. Validar pedido, financeiro, documento e archive.
5. Registrar evidencias.
6. Encerrar dependencia do emissor paralelo para o emitente liberado.

Saida obrigatoria:
- emitente operando em producao pelo ERP.

## 5. Checklist de janela de virada

Executar no dia do go-live:

### `T-1d`

- [ ] segredos e certificados revisados.
- [ ] monitoracao e alertas validados.
- [ ] storage acessivel.
- [ ] restore testado.
- [ ] responsaveis confirmados para a janela.

### `T-2h`

- [ ] ambiente de producao responsivo.
- [ ] fila/worker fiscal saudavel.
- [ ] agente `A3` online, quando aplicavel.
- [ ] contatos de suporte disponiveis.
- [ ] emissor paralelo congelado para evitar duplicidade.

### `T0`

- [ ] primeira emissao de producao executada.
- [ ] protocolo retornado.
- [ ] `XML` e `DANFE` arquivados.
- [ ] reflexo no pedido validado.
- [ ] reflexo no financeiro validado.

### `T+2h`

- [ ] consulta posterior validada.
- [ ] sem erro repetitivo na fila.
- [ ] sem divergencia de numeracao.
- [ ] operacao ciente do novo fluxo.

### `T+1d`

- [ ] revisar alertas e logs.
- [ ] revisar fila de erro.
- [ ] revisar aceite da operacao.
- [ ] decidir encerramento formal da janela.

## 6. Evidencias minimas obrigatorias

Guardar por emitente:
- numero e chave da `NF-e` homologada com `A1`, quando aplicavel;
- numero e chave da `NF-e` homologada com `A3`, quando aplicavel;
- numero e chave da primeira `NF-e` real de producao;
- protocolo dos eventos de cancelamento, inutilizacao e `CC-e`;
- caminho de `XML` e `DANFE`;
- evidencias de restore;
- print/export de monitoracao basica;
- checklist assinado/aprovado.

## 7. Criterios de rollback

Executar rollback da virada se ocorrer qualquer um dos itens abaixo:
- emissao de producao sem retorno confiavel de protocolo;
- falha recorrente de assinatura ou autorizador sem causa controlada;
- artefato fiscal nao persistido;
- divergencia de numeracao;
- pedido ou financeiro fora de sincronismo apos a emissao;
- agente `A3` instavel quando o emitente depender dele;
- operacao sem suporte para seguir com seguranca.

## 8. Procedimento de rollback

1. Suspender novas emissoes pelo ERP para o emitente afetado.
2. Registrar incidente e horario.
3. Preservar logs, tentativas e artefatos.
4. Revalidar estado do ultimo documento.
5. Redirecionar temporariamente a operacao para o fluxo contingente aprovado.
6. Corrigir causa raiz.
7. Reexecutar smoke controlado antes de nova tentativa de virada.

Observacao:
- rollback operacional nao autoriza apagar tentativa, numero ou trilha fiscal ja gerada.

## 9. Estabilizacao inicial

Nos primeiros dias apos a virada:
- acompanhar fila fiscal e erros a cada janela combinada;
- revisar certificados e validade;
- revisar tentativas, rejeicoes e tempo medio de resolucao;
- revisar aderencia da operacao ao novo fluxo;
- abrir correcao imediata para qualquer desvio em pedido, financeiro ou archive.

## 10. Resultado esperado

Ao final deste runbook:
- o emitente esta apto a emitir de fato pelo ERP;
- existe trilha fiscal auditavel;
- existe suporte inicial estruturado;
- o emissor paralelo deixa de ser fluxo principal para o emitente liberado.
