# Backlog Inicial - PackControl

Versao: 0.3  
Data: 2026-03-26  
Status: Planejamento ativo, com checkpoint atualizado para deploy tecnico, readiness operacional e fiscal plugavel

## 1. Premissas de planejamento

Premissas usadas neste backlog:
- time principal: `1 desenvolvedor + apoio de IA`;
- documentacao detalhada faz parte do trabalho;
- backlog detalhado usa `pontos relativos`, nao dias;
- prazo macro por marco usa `dias uteis ideais`;
- ha buffer obrigatorio de `25% a 35%` para integracao, homologacao fiscal, refinamento e operacao real;
- o objetivo e um `MVP vendavel`, nao o produto final completo.

Escala de pontos:
- `1`: ajuste pequeno
- `2`: tarefa simples
- `3`: tarefa media
- `5`: tarefa relevante
- `8`: tarefa grande/arriscada
- `13`: tarefa de alta incerteza

Como ler:
- some `dias` apenas na secao de marcos;
- use `pontos` para ordenar, priorizar e quebrar sprint;
- nao trate pontos como horas.

## 2. Resumo de marcos

| Marco | Objetivo | Estimativa base |
|---|---|---|
| `M0` | arquitetura, setup, seguranca base e shell visual | 10d |
| `M1` | clientes, ativos, pedidos, escopo e anexos | 18d |
| `M2` | analise tecnica `PDF/DXF`, estimador e orcamento | 18d |
| `M3` | OPs, filas operacionais e producao | 20d |
| `M4` | materiais, estoque, logistica e expedicao | 15d |
| `M5` | financeiro funcional | 13d |
| `M6` | fiscal NF-e plugavel | 23d |
| `M7` | edge, hardening, backup e go-live | 17d |

Subtotal base: `134d`  
Faixa com buffer: `168d a 181d`

Leitura pratica:
- alvo agressivo: `24 a 26 semanas`;
- alvo realista: `28 a 32 semanas`;
- se `MDF-e` entrar no MVP, adicionar `10d a 15d`;
- se o edge inicial reaproveitar mais do watcher atual, existe chance de reduzir `4d a 6d`.

Ordem obrigatoria no arranque do projeto:
- `arquitetura`
- `frontend shell e navegacao`
- `backend foundation e seguranca`
- `modulos verticais`

## 2.1 Checkpoint da baseline executada

Estado observado no repositorio em `2026-03-27`:
- `M0` parcialmente executado: estrutura de repositorio, shell Angular, backend modular, login por cookie, auditoria minima, healthcheck, suite inicial de build/teste e persistencia configuravel em `InMemory` ou `PostgreSQL` por snapshot;
- `M7` parcialmente executado: artefatos de deploy tecnico (`Dockerfile`, `compose`, `nginx`), readiness operacional e runbook/checklist de deploy agora existem no repositorio;
- `M1` parcialmente executado: clientes com apelidos/endereco/logistica padrao, ativos tecnicos iniciais, regras comerciais por produto/cliente, pedido com escopo flexivel por produto comercial, referencia opcional a ativo antigo, anexos e pedido consolidado;
- `M2` parcialmente executado: upload tecnico com parser real de `PDF`, analise real de `DXF`, motor identificado no retorno e percentual de confianca por analise;
- `M3` parcialmente executado: pedido aprovado gera OPs, existem filas operacionais iniciais, telas dedicadas para `Montagem` e `Emborrachamento`, e `split/merge` auditavel de OPs;
- `M4` parcialmente executado: materiais, estoque, logistica, transportadoras e expedicao possuem telas e endpoints iniciais, com baixa automatica de estoque na criacao de OP;
- `M5` parcialmente executado: financeiro previsto + manual com contas a receber/pagar, boleto e vinculo com pedido;
- `M6` parcialmente executado: perfil fiscal, empresa emissora e templates de operacao editaveis, emissao canonica com `prepare/issue`, snapshot fiscal congelado do documento, certificado `A1/A3` modelado, `XML`/`DANFE` arquivados, roteamento de adapters (`mock-plugavel` e `unimake.dfe`) e diagnostico real de status do servico `NF-e`;
- `Cadastros` mestres entraram como modulo separado de `Clientes`, `Produtos` e `Transportadoras`, cobrindo tipos, setores, operacoes, fornecedores, modos de entrega e unidades.

Ainda em aberto mesmo apos a baseline:
- estimador deterministico e orcamento;
- ativos tecnicos completos do cliente, com anexos proprios, revisoes e comparacao de versoes;
- pipeline visual mais rico de `DXF`, com renderizacao e score operacional;
- filas operacionais completas, apontamentos e capacidade/roteiro fino por setor;
- endurecimento de seguranca com MFA e autorizacao fina por modulo;
- persistencia relacional com migrations formais, backup/restore real e fiscal `NF-e` homologada.

## 3. Critico para o MVP

Itens obrigatorios para vender:
- `M0` a `M7` completos;
- `NF-e` homologada e operacional;
- financeiro minimamente funcional;
- fluxo pedido -> producao -> expedicao -> faturamento -> financeiro;
- login endurecido, MFA para perfis sensiveis e trilha de auditoria;
- edge confiavel para pelo menos os sinais principais de fabrica.

## 4. Dependencias macro

Sequencia principal:
- `M0` -> `M1` -> `M2` -> `M3` -> `M4` -> `M5` + `M6` -> `M7`

Dependencias fortes:
- dentro de `M0`, o `frontend` vem imediatamente depois do fechamento de arquitetura;
- seguranca base antes de financeiro/fiscal;
- pedidos e anexos antes de analise/estimativa;
- upload de `PDF` precisa devolver analise imediata antes do refinamento manual do pedido;
- orcamento aprovado antes de OP;
- producao antes de logistica operacional;
- financeiro antes da conciliacao com fiscal;
- fiscal depende de storage, auditoria, emissor e numeracao;
- edge depende de contratos estaveis de evento e projeção no backend.

## 5. Backlog por marco

### `M0` - Arquitetura, setup e seguranca base

Objetivo:
- criar a espinha dorsal do sistema e fechar as decisoes que impactam todo o resto.

Ordem interna deste marco:
- fechar arquitetura e contratos base;
- subir shell Angular e navegacao;
- subir backend modular;
- encaixar autenticacao, auditoria e operacao inicial.

Telas relacionadas:
- shell geral do produto
- configuracoes iniciais

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M0-01` | consolidar `SDS`, backlog e convencoes tecnicas | 3 | - | base documental |
| `M0-02` | criar estrutura inicial de `frontend`, `backend`, `edge` e `docs` | 3 | `M0-01` | base de repositorio |
| `M0-03` | subir shell Angular + Bootstrap + roteamento base | 3 | `M0-02` | fundacao frontend |
| `M0-04` | subir backend modular ASP.NET Core com banco e migrations | 5 | `M0-02` | fundacao backend |
| `M0-05` | implementar login por sessao/cookie, papeis base e auditoria minima | 5 | `M0-03`,`M0-04` | seguranca inicial |
| `M0-06` | preparar logs estruturados, healthchecks e tratamento global de erro | 3 | `M0-04` | operacao inicial |

### `M1` - Clientes, ativos, pedidos e anexos

Objetivo:
- habilitar o fluxo comercial base e a abertura de pedidos reais.

Telas relacionadas:
- pagina 2 `Novo Pedido`
- pagina 3 `Escopo do Pedido`
- pagina 7 `Pedido Consolidado`

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M1-01` | modelar cliente, contato, endereco e regras comerciais basicas | 5 | `M0-04` | base comercial |
| `M1-02` | implementar CRUD de cliente e contatos | 5 | `M1-01`,`M0-03` | gestao de cliente |
| `M1-03` | modelar ativo tecnico e historico vinculado ao cliente | 5 | `M1-01` | base para repeticao/reforma |
| `M1-04` | modelar pedido, status e tipos (`novo`, `repeticao`, `reforma`, etc.) | 5 | `M1-01` | agregado pedido |
| `M1-05` | modelar itens de escopo flexiveis e ligacao com ativo antigo | 8 | `M1-04`,`M1-03` | escopo real da facaria |
| `M1-06` | implementar upload de anexos com hash, versionamento e seguranca | 8 | `M0-04` | base de arquivos |
| `M1-07` | construir tela de pedido consolidado em abas com historico | 5 | `M1-04`,`M1-05`,`M1-06`,`M0-03` | visao consolidada |

### `M2` - Analise tecnica, estimador e orcamento

Objetivo:
- converter o pedido com arquivo em informacao tecnica e proposta comercial, com analise imediata tanto para `PDF` quanto para `DXF`.

Telas relacionadas:
- pagina 4 `Analise Tecnica`
- pagina 5 `Estimativa Deterministica`
- pagina 6 `Orcamento`

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M2-01` | definir contrato interno de analise tecnica para `PDF` e `DXF` | 3 | `M0-01` | base do pipeline tecnico |
| `M2-02` | extrair/reaproveitar parser de `PDF` do watcher atual e normalizar retorno | 8 | `M2-01` | engine documental |
| `M2-03` | extrair/reaproveitar analise `DXF` e render do watcher atual | 13 | `M2-01` | engine geometrica |
| `M2-04` | integrar upload tecnico com disparo de analise imediata | 5 | `M1-06`,`M2-02`,`M2-03` | analise automatica |
| `M2-05` | persistir extracao de `PDF`, score de `DXF`, imagem, confianca e historico | 8 | `M2-02`,`M2-03` | historico tecnico |
| `M2-06` | sugerir preenchimento do pedido a partir do `PDF` analisado | 5 | `M2-05`,`M1-04`,`M1-05` | ganho operacional inicial |
| `M2-07` | implementar estimador deterministico por etapa, gargalo e prazo | 13 | `M2-05` | inteligencia deterministica |
| `M2-08` | implementar preco sugerido, margem, ajuste manual e aprovacao | 8 | `M2-07`,`M1-05` | orcamento funcional |
| `M2-09` | construir telas de analise, estimativa e orcamento com conciliacao `PDF`/`DXF` | 8 | `M2-05`,`M2-06`,`M2-07`,`M2-08`,`M0-03` | fluxo tecnico/comercial |

### `M3` - OPs, filas operacionais e producao

Objetivo:
- transformar pedido aprovado em execucao rastreavel no chao de fabrica.

Telas relacionadas:
- pagina 8 `Ordens de Producao`
- pagina 9 `Producao por Setor`
- pagina 15 `Fila de Montagem`
- pagina 16 `Setor de Emborrachamento`

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M3-01` | modelar OP, etapas, setor atual e status operacionais | 8 | `M2-06` | base produtiva |
| `M3-02` | gerar OPs a partir do pedido aprovado | 5 | `M3-01` | ponte pedido -> producao |
| `M3-03` | implementar split/merge de OPs com historico auditavel | 13 | `M3-01` | flexibilidade operacional |
| `M3-04` | modelar fila por setor, atribuicao e prioridade | 8 | `M3-01` | filas operacionais |
| `M3-05` | implementar SignalR para OPs e filas | 5 | `M0-04`,`M0-03`,`M3-04` | realtime operacional |
| `M3-06` | construir telas de producao por setor e filas principais | 8 | `M3-04`,`M3-05`,`M0-03` | operacao em tela |
| `M3-07` | implementar apontamentos rapidos e historico operacional | 5 | `M3-04`,`M3-05` | feedback de chao de fabrica |

### `M4` - Materiais, estoque, logistica e expedicao

Objetivo:
- fechar o ciclo operacional ate a entrega.

Telas relacionadas:
- pagina 10 `Gestao de Materiais`
- pagina 11 `Gestao de Estoque`
- pagina 12 `Logistica e Expedicao`
- pagina 17 `Expedicao Operacional`

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M4-01` | modelar tipo tecnico x item real de estoque | 5 | `M0-04` | separacao correta de dominio |
| `M4-02` | implementar materiais, fornecedores, custo e risco de falta | 5 | `M4-01`,`M0-03` | cadastro funcional |
| `M4-03` | implementar saldo, reserva, baixa e movimentacao | 8 | `M4-01`,`M3-02` | estoque funcional |
| `M4-04` | modelar lote de expedicao, checklist, comprovantes e ocorrencias | 8 | `M3-02` | base logistica |
| `M4-05` | construir telas gerenciais e operacionais de materiais/estoque/expedicao | 8 | `M4-02`,`M4-03`,`M4-04`,`M0-03` | operacao e visibilidade |
| `M4-06` | implementar fluxo de saida, retirada e saida adversa | 5 | `M4-04`,`M3-05` | expedicao funcional |

### `M5` - Financeiro funcional

Objetivo:
- cobrir o minimo financeiro para operar e vender.

Telas relacionadas:
- pagina 13 `Financeiro`

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M5-01` | modelar contas a receber, contas a pagar e historico | 8 | `M0-04` | base financeira |
| `M5-02` | gerar previsao financeira a partir do pedido aprovado | 5 | `M2-06`,`M5-01` | previsao automatica |
| `M5-03` | implementar baixa manual, status e vinculo com pedido | 5 | `M5-01` | operacional financeiro |
| `M5-04` | montar tela financeira com blocos de receber/pagar/resultado/notas | 5 | `M5-01`,`M5-03`,`M0-03` | visao funcional |
| `M5-05` | aplicar controles de permissao financeira e auditoria forte | 3 | `M0-05`,`M5-01` | seguranca financeira |

### `M6` - Fiscal NF-e de producao

Objetivo:
- entregar um modulo fiscal operacional em producao real junto ao SEFAZ.

Detalhamento dedicado:
- ver `docs/backlog-modulo-fiscal-nfe.md` para fases, gates e pacotes tecnicos do modulo fiscal.

Telas relacionadas:
- configuracoes fiscais
- historico fiscal por pedido
- area de notas dentro do financeiro/pedido

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M6-01` | modelar emissor fiscal, serie, numeracao e templates de operacao | 8 | `M0-04`,`M0-05` | base fiscal |
| `M6-02` | modelar agregado `FiscalDocument` e eventos fiscais | 8 | `M6-01` | core fiscal |
| `M6-03` | implementar snapshot fiscal dos itens do pedido | 8 | `M1-05`,`M6-02` | congelamento tributario |
| `M6-04` | gerar XML NF-e conforme leiaute vigente | 13 | `M6-02`,`M6-03` | emissao tecnica |
| `M6-05` | implementar assinatura digital e gestao de certificado | 8 | `M6-04` | assinatura |
| `M6-06` | implementar adapter de transmissao/consulta em homologacao | 13 | `M6-05` | integracao com SEFAZ |
| `M6-07` | persistir XML, protocolo, DANFE e status do documento | 8 | `M6-06`,`M1-06` | arquivo fiscal |
| `M6-08` | implementar cancelamento, inutilizacao e CC-e | 13 | `M6-07` | ciclo fiscal |
| `M6-09` | integrar fiscal com financeiro e pedido consolidado | 5 | `M5-01`,`M6-07` | visao integrada |
| `M6-10` | montar telas fiscais e suite de homologacao | 8 | `M6-08`,`M0-03` | operacao fiscal segura |

Observacao:
- `MDF-e` nao esta incluido neste marco base; se confirmado como obrigatorio, vira sub-marco `M6B`.

### `M7` - Edge, hardening e go-live

Objetivo:
- levar o sistema a um estado operavel e suportavel em producao.

Telas relacionadas:
- fila operacional em tempo real
- painel tecnico/administrativo de integracoes

| ID | Item | Pts | Dependencias | Entrega |
|---|---|---:|---|---|
| `M7-01` | fechar contratos de evento de fabrica e naming canonico | 3 | `M0-01` | base de integracao |
| `M7-02` | criar skeleton do `PackControl Edge` com config e logging | 5 | `M7-01` | base do agente |
| `M7-03` | implementar watchers, debounce, hash e deduplicacao | 8 | `M7-02` | coleta local |
| `M7-04` | implementar spool local, retry e publicacao em RabbitMQ | 8 | `M7-02` | resiliencia do agente |
| `M7-05` | implementar consumidor backend e projeção para producao | 8 | `M7-04`,`M3-04` | integracao edge -> ERP |
| `M7-06` | integrar pipeline DXF herdada do watcher atual | 5 | `M2-02`,`M7-02` | reaproveitamento tecnico |
| `M7-07` | configurar backup, restore, healthchecks e dashboards minimos | 5 | `M0-06` | operacao real |
| `M7-08` | executar regressao critica, checklist de deploy e go-live controlado | 8 | `M4-06`,`M5-04`,`M6-10`,`M7-07` | entrada em producao |

## 6. Corte sugerido para o primeiro go-live

Go-live recomendado:
- clientes, ativos basicos, pedidos e anexos;
- analise imediata de `PDF`, analise de `DXF` e estimativa;
- orcamento e aprovacao;
- OPs e pelo menos duas filas operacionais principais;
- materiais/estoque minimo;
- expedicao;
- contas a receber e a pagar basicas;
- NF-e funcional em producao;
- edge cobrindo os sinais principais de corte e dobra.

Pode ficar para logo apos o primeiro go-live:
- dashboards gerenciais mais ricos;
- automacoes financeiras adicionais;
- cobertura expandida do edge;
- `MDF-e`, se confirmado;
- automacoes fiscais alem do nucleo de NF-e.

## 7. Definicao de pronto

Um item so pode ser considerado pronto quando:
- a regra de negocio principal esta implementada;
- autorizacao e validacao estao coerentes;
- logs e mensagens de erro sao utilizaveis;
- o comportamento esta documentado em `docs`;
- existe ao menos um teste no nivel adequado;
- o item foi exercitado dentro do fluxo real onde se encaixa.

## 8. Riscos de planejamento

| Risco | Efeito no prazo | Mitigacao |
|---|---|---|
| Escopo fiscal crescer alem de NF-e | alto | manter `MDF-e` e outros DFes como sub-marcos separados |
| Reaproveitamento do watcher atual ser menor que o esperado | medio/alto | isolar o edge e aceitar reescrita parcial do agente |
| UX operacional exigir varias iteracoes de campo | medio | validar cedo filas e expedicao em layout funcional |
| Regras comerciais/fiscais mudarem durante a obra | alto | modelar templates e snapshots desde o inicio |
| Infra barata gerar gargalos operacionais | medio | adotar topologia restauravel e monitorada desde o MVP |

## 9. Proximos refinamentos recomendados

- transformar cada marco em `issues` reais quando o repositorio principal existir;
- detalhar a matriz fiscal real da facaria antes de iniciar `M6`;
- fechar quais eventos de corte/dobra entram na versao 1 do edge;
- definir o primeiro conjunto de KPIs operacionais e financeiros;
- vincular backlog tecnico com cada pagina do wireframe revisado.
