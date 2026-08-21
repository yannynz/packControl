# Plano de Execucao Completo - PackControl

Versao: 0.5  
Data: 2026-03-27  
Status: Plano ativo com checkpoint atualizado para deploy tecnico, readiness operacional e trilha fiscal real `A1`

## 1. Contexto e objetivo

Transformar o `SDS` e o backlog do PackControl em um plano de execucao completo, orientado a entrega real, com:
- ordem de ataque;
- fases e marcos;
- tarefas detalhadas;
- dependencias;
- entregaveis;
- criterios de aceite;
- trilhas de documentacao e testes;
- pontos de controle de risco;
- sugestao de uso do trio `voce + Codex + GeminiCLI`.

Este documento e um plano de execucao do `MVP vendavel`, nao do produto final completo.

Documentos base:
- `docs/sds-packcontrol.md`
- `docs/backlog-packcontrol.md`
- `docs/prd-erp-facaria.md`
- `docs/wireframe-review-visily.md`

### 1.1 Estado atual do repositorio

Inventario observado no workspace analisado:
- existe SPA Angular com login, shell em PT-BR e navegacao para `Painel`, `Pedidos`, `Producao`, `Logistica`, `Transportadoras`, `Clientes`, `Produtos`, `Cadastros`, `Materiais`, `Estoque`, `Financeiro` e `Configuracoes`;
- existe API ASP.NET Core com autenticacao por cookie, seed local, auditoria minima, anexos em disco e persistencia configuravel em `InMemory` ou `PostgreSQL`;
- existe baseline de deploy tecnico em `deploy/`, com `Dockerfile` de `API`/frontend, `docker-compose.production.yml` e proxy `nginx`;
- existem endpoints de readiness operacional (`/health/live` e `/health/ready`) para validar persistencia e storage;
- o fluxo comercial basico esta executado com pedido, escopo flexivel, contexto inicial opcional, referencia antiga opcional, produtos comerciais, tabela comercial por cliente, ativos tecnicos, anexos e pedido consolidado;
- existem modulos iniciais de producao, logistica, transportadoras, materiais, estoque, financeiro, fiscal, clientes, ativos, produtos e cadastros mestres;
- `PDF` e `DXF` ja passam por analise tecnica real no upload, com retorno de motor e confianca;
- producao ja suporta `split/merge` auditavel de OPs;
- o `edge` continua separado como skeleton local;
- existem testes minimos de dominio, API e frontend, com build e suite local validados.

Leitura pratica:
- o projeto saiu da fase puramente documental e ja possui baseline funcional navegavel;
- a execucao de software ja cobre persistencia duravel opcional em `PostgreSQL`, parser tecnico real e fiscal canonico, mas ainda sem integracao real homologada com SEFAZ, modelo relacional final e deploy de producao;
- este plano agora serve como trilha de fechamento do MVP, e nao mais como ponto zero de arranque.

### 1.2 Sintese dos artefatos analisados

- `docs/prd-erp-facaria.md`: define o problema de negocio, o escopo do dominio, os fluxos principais, as fases do produto e os criterios de sucesso.
- `docs/sds-packcontrol.md`: traduz o PRD para arquitetura alvo, modulos, stack, limites de contexto, requisitos nao funcionais e estrategia tecnica de entrega.
- `docs/backlog-packcontrol.md`: quebra o MVP em marcos executaveis com pontos, dependencias e ordem de ataque.
- `docs/wireframe-review-visily.md`: consolida a leitura visual do wireframe e direciona a identidade operacional do produto.
- `wireframePackControl.pdf`: materializa `17` telas cobrindo dashboard, novo pedido, analise tecnica, OPs, materiais, estoque, logistica, financeiro, configuracoes e filas operacionais.

### 1.3 Coerencia atual entre os documentos

Pontos que estao coerentes:
- os quatro artefatos convergem na ordem de ataque `arquitetura -> shell frontend -> backend/seguranca -> modulos verticais`;
- a stack alvo esta consistente entre `SDS`, backlog e plano atual: `Angular`, `Bootstrap`, `ASP.NET Core`, `PostgreSQL`, `SignalR`, `RabbitMQ` restrito a borda e storage `S3-compatible`;
- o fluxo central do MVP esta bem definido: pedido -> analise tecnica -> estimativa/orcamento -> producao -> expedicao -> financeiro/fiscal;
- o wireframe e a revisao textual reforcam a mesma direcao de UX: linguagem operacional, cards, boards, CTA dominante e baixa densidade para operacao.

Divergencias e leituras que este plano resolve:
- o `PRD` e o `SDS` agora convergem para `NF-e` obrigatoria no MVP, com arquitetura plugavel e homologacao como trilha de fechamento;
- o backlog macro estima `134d`, enquanto o plano detalhado sobe para `159d`; a diferenca e aceita porque este arquivo considera estabilizacao, documentacao por fase, homologacao e validacao de campo;
- o desenho funcional esta mais maduro do que o desenho de implantacao real; por isso, a maior lacuna hoje nao e de escopo, e sim de inicializacao tecnica e validacao com artefatos reais.

### 1.4 Lacunas criticas apos o bootstrap

Mesmo com a baseline executada, ainda faltam insumos operacionais e tecnicos que influenciam prazo e arquitetura:
- acesso ao `FileWatcherApp` ou watcher atual, para medir reaproveitamento adicional de parser `PDF`, render `DXF` e eventos de borda;
- amostra real de arquivos `PDF` e `DXF` representativos da operacao, para validar a estrategia de analise imediata;
- matriz fiscal inicial validada com apoio contabil, inclusive confirmacao objetiva sobre necessidade de `MDF-e`;
- decisao de infraestrutura inicial para banco, fila, storage e backup;
- validacao do alvo de deploy de producao escolhido, incluindo restore e rollback operacional;
- fechamento da modelagem definitiva de ativos do cliente, estimador/orcamento e ciclo fiscal homologado.

### 1.5 Decisao de arranque deste plano

Para fins de execucao, o PackControl nao deve mais ser tratado como projeto em estado `pre-P0`.

Isso significa:
- `P0` e `P1` ja produziram a espinha dorsal real do repositorio;
- `P2`, `P3`, `P4`, `P6`, `P7`, `P8` e `P9` ja possuem fatias funcionais iniciais executadas;
- o foco imediato deixa de ser "comecar o codigo" e passa a ser fechar persistencia relacional, estimador/orcamento, homologacao fiscal, hardening final e operacao assistida do deploy;
- toda estimativa posterior continua dependente da validacao do reaproveitamento do watcher atual e do recebimento de arquivos reais da operacao.

## 2. Regras de execucao

Regras obrigatorias:
- primeiro fecha arquitetura;
- imediatamente depois sobe o `frontend shell`;
- backend, seguranca e persistencia entram na sequencia;
- modulos de negocio entram como fatias verticais;
- toda entrega gera codigo, teste e documentacao;
- nenhum modulo entra em producao sem trilha de auditoria quando for sensivel;
- nenhum modulo fiscal entra sem homologacao especifica;
- nada do `Edge` pode carregar regra de negocio central do ERP.

Regras de escopo:
- `MVP` significa vendavel e operavel, nao "simples";
- `NF-e` e obrigatoria no MVP;
- `MDF-e` fica em espera ate confirmacao operacional;
- `PDF` deve gerar analise imediata no upload;
- `DXF` deve gerar analise geometrica, score e render;
- operador nao pode ver custo, margem ou preco.

## 3. Linha mestra de entrega

Ordem principal:
1. diagnostico final, arquitetura e contratos
2. bootstrap do repositorio e convencoes base
3. frontend shell e navegacao
4. backend foundation, seguranca e persistencia
5. clientes, ativos, pedidos e anexos
6. analise tecnica `PDF/DXF`
7. estimador e orcamento
8. producao e filas operacionais
9. materiais, estoque e expedicao
10. financeiro funcional
11. fiscal NF-e
12. edge de fabrica
13. hardening, homologacao e go-live

## 4. Cronograma macro

| Fase | Nome | Duracao base | Dependencia principal | Saida de fase |
|---|---|---:|---|---|
| `P0` | Arquitetura e governanca | 5d | - | arquitetura fechada |
| `P1` | Frontend shell | 5d | `P0` | shell Angular validado |
| `P2` | Backend foundation e seguranca | 10d | `P0`,`P1` | base backend segura |
| `P3` | Clientes, ativos, pedidos e anexos | 18d | `P2` | fluxo comercial base |
| `P4` | Analise tecnica `PDF/DXF` | 18d | `P3` | painel tecnico funcional |
| `P5` | Estimador e orcamento | 10d | `P4` | proposta comercial funcional |
| `P6` | Producao e realtime | 20d | `P5` | OPs e filas operacionais |
| `P7` | Materiais, estoque e expedicao | 15d | `P6` | operacao ate a saida |
| `P8` | Financeiro funcional | 13d | `P5`,`P7` | contas a receber/pagar |
| `P9` | Fiscal NF-e | 23d | `P3`,`P8` | emissao fiscal funcional |
| `P10` | PackControl Edge | 12d | `P6`,`P4` | integracao fabril minima |
| `P11` | Hardening, homologacao e go-live | 10d | `P7`,`P8`,`P9`,`P10` | producao pronta |

Subtotal base: `159d`  
Faixa com buffer: `199d a 215d`

Observacao:
- o numero aqui e mais alto que o backlog macro porque este plano considera trabalho detalhado, validacao de campo, documentacao por fase e estabilizacao;
- na pratica, parte de `P8`, `P9` e `P10` pode ser parcialmente paralelizada;
- com ganho real de paralelismo e reaproveitamento forte do watcher atual, esse horizonte pode voltar para a faixa de `28 a 32 semanas`.

## 5. Uso sugerido do time

### 5.1 Voce

Responsabilidades principais:
- decidir escopo e tradeoffs;
- validar fluxo real da facaria;
- aprovar nomenclaturas, telas e comportamento operacional;
- falar com contador quando a pauta for fiscal;
- fornecer regras reais de negocio e edge cases;
- aprovar deploy e segredos.

### 5.2 Codex

Responsabilidades principais:
- implementar codigo;
- integrar modulos;
- escrever e manter documentacao tecnica;
- desenhar contratos e estruturas;
- executar refactors necessarios;
- produzir testes e checklist de entrega.

### 5.3 GeminiCLI

Uso recomendado:
- review independente de codigo e de arquitetura;
- segunda opiniao para fiscal e edge;
- geracao de cenarios de teste;
- revisao de clareza da documentacao;
- busca de alternativas tecnicas quando um modulo travar.

### 5.4 Regra de colaboracao

- toda decisao estrutural entra em `docs`;
- toda feature relevante precisa de criterio de aceite;
- sempre que uma implementacao fechar, o GeminiCLI revisa ou gera casos de teste;
- o usuario fecha a validacao funcional;
- o Codex faz a integracao final.

## 6. Trilhas transversais

Essas trilhas nao sao fases separadas. Elas acompanham o projeto inteiro.

### 6.1 Documentacao

Obrigatorio em cada fase:
- atualizar `SDS` quando arquitetura mudar;
- atualizar backlog quando escopo mudar;
- registrar contratos e eventos;
- manter checklist de testes;
- registrar runbooks e operacao quando o modulo for sensivel.

### 6.2 Testes

Obrigatorio em cada fase:
- testes unitarios para regra pura;
- testes de integracao para persistencia e contratos;
- testes `E2E` para fluxos-chave de UI;
- regressao dos fluxos que forem impactados;
- quando o modulo for fiscal ou edge, testes de contrato sao obrigatorios.

### 6.3 Seguranca

Obrigatorio em cada fase:
- revisar autorizacao;
- revisar auditoria;
- revisar validacao de entrada;
- revisar upload e storage quando houver arquivo;
- revisar segregacao de perfis.

### 6.4 UX operacional

Obrigatorio em cada fase:
- manter consistencia visual;
- manter a acao principal clara;
- reduzir densidade excessiva;
- priorizar leitura rapida;
- sempre revisar tablet nas telas operacionais.

## 7. Criticos de qualidade

Nenhuma fase pode ser fechada sem:
- codigo buildando;
- migrations ou scripts coerentes;
- contratos documentados;
- logs adequados;
- tratamento de erro visivel e util;
- autorizacao aplicada;
- teste minimo no nivel correto;
- validacao com base no fluxo do negocio.

## 8. Plano detalhado por fase

### `P0` - Arquitetura e governanca

Objetivo:
- fechar as decisoes que condicionam o resto do projeto.

Entregaveis de fase:
- arquitetura final do MVP;
- padroes de codigo;
- convencoes de modulo;
- convenção de eventos;
- convenção de documentacao;
- estrategia de ambientes.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P0-01` | congelar escopo do MVP | revisar o que entra e o que fica fora do MVP | - | lista oficial de escopo | voce + Codex |
| `P0-02` | fechar stack final | confirmar Angular, ASP.NET Core, PostgreSQL, RabbitMQ, storage e SignalR | `P0-01` | stack oficial | voce + Codex |
| `P0-03` | fechar fronteiras de modulo | transformar dominios do SDS em modulos reais de codigo | `P0-02` | mapa de modulos | Codex |
| `P0-04` | definir contratos de evento | padronizar eventos internos, edge e analise tecnica | `P0-03` | documento de contratos | Codex |
| `P0-05` | definir convencoes de API | naming, erros, paginacao, filtros, auth e versao | `P0-02` | padrao de API | Codex |
| `P0-06` | definir convencoes de banco | schema naming, `UUID`, auditoria, indices e soft delete | `P0-02` | padrao de dados | Codex |
| `P0-07` | definir politica de documentacao | estrutura de `docs`, ADRs, checklists e runbooks | `P0-01` | padrao documental | Codex |
| `P0-08` | definir politica de branching e releases | fluxo de branch, tags, deploy e rollback | `P0-02` | politica de entrega | voce + Codex |
| `P0-09` | definir estrategia de ambientes | `dev`, `homolog`, `prod`, segredos e dumps | `P0-02` | mapa de ambientes | Codex |
| `P0-10` | registrar ADRs iniciais | arquitetura, auth, realtime, fiscal e edge | `P0-02` | pacote inicial de ADRs | Codex |

Criterios de aceite da fase:
- nao ha duvida estrutural grande sobre stack;
- modulos e contratos estao nomeados;
- estrategia de ambiente esta fechada;
- backlog e SDS estao coerentes.

### `P1` - Frontend shell

Objetivo:
- validar cedo a casca do produto, navegacao e linguagem visual.

Entregaveis de fase:
- projeto Angular inicial;
- shell visual;
- navegacao principal;
- layout base desktop/tablet;
- base de componentes operacionais e administrativos.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P1-01` | criar workspace Angular | configurar projeto, lint, build e estrutura de features | `P0-02` | workspace Angular | Codex |
| `P1-02` | integrar Bootstrap e base visual | definir grid, spacing, forms, tabelas e utilitarios | `P1-01` | base de estilo | Codex |
| `P1-03` | construir shell de app | header, sidebar, breadcrumb, area de conteudo e footer tecnico | `P1-02` | shell navegavel | Codex |
| `P1-04` | definir informacao de navegacao | menu principal, grupos por modulo e rotas base | `P1-03` | mapa de navegacao | voce + Codex |
| `P1-05` | criar layouts base | layout `dashboard`, `form`, `board`, `admin`, `detail-tabs` | `P1-03` | layouts reutilizaveis | Codex |
| `P1-06` | criar componentes base | cards, chips, filtros, empty states, toasts, loaders e modais curtos | `P1-05` | biblioteca interna inicial | Codex |
| `P1-07` | preparar responsividade operacional | validar tablet nas telas que serao fila/expedicao | `P1-05` | baseline responsiva | Codex |
| `P1-08` | criar rotas placeholder das paginas do MVP | telas vazias navegaveis por fluxo | `P1-04`,`P1-05` | roteiro visual do produto | Codex |
| `P1-09` | validar identidade visual com wireframe | alinhar shell ao board operacional recomendado | `P1-08` | shell aprovado | voce + Codex |

Criterios de aceite da fase:
- existe shell real navegavel;
- estrutura de rotas comporta os modulos;
- linguagem visual operacional esta definida;
- frontend pode ser conectado ao backend sem retrabalho estrutural.

### `P2` - Backend foundation e seguranca

Objetivo:
- montar a fundacao segura do backend e da persistencia.

Entregaveis de fase:
- solucao backend modular;
- autenticacao funcional;
- autorizacao base;
- auditoria base;
- logs e healthchecks;
- base de storage e fila interna.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P2-01` | criar solucao ASP.NET Core modular | separar `Presentation`, `Application`, `Domain`, `Infrastructure` | `P0-03` | solution base | Codex |
| `P2-02` | evoluir persistencia `PostgreSQL` para modelo relacional | migrations, healthcheck de banco e substituicao gradual do snapshot unico | `P0-06`,`P2-01` | persistencia base madura | Codex |
| `P2-03` | criar modelo de identidade | usuarios, papeis, permissoes e claims | `P0-03`,`P2-01` | modelo de auth | Codex |
| `P2-04` | implementar autenticacao por sessao | login, logout, cookie seguro e sessao | `P2-03`,`P1-03` | auth funcional | Codex |
| `P2-05` | implementar autorizacao e guards | `RBAC`, policies, decorators e integração frontend | `P2-03`,`P2-04`,`P1-08` | autorizacao base | Codex |
| `P2-06` | implementar MFA TOTP | setup, challenge, recovery e enforce por perfil | `P2-04` | MFA funcional | Codex |
| `P2-07` | implementar auditoria base | acesso, administracao e acoes sensiveis | `P2-03`,`P2-04` | audit log inicial | Codex |
| `P2-08` | configurar logs e observabilidade basica | correlacao, erro global, healthchecks e request id | `P2-01` | operacao base | Codex |
| `P2-09` | criar abstrações de storage | object storage, naming de arquivos, hash e metadados | `P0-02`,`P2-01` | storage adapter | Codex |
| `P2-10` | criar base de outbox e jobs internos | tabela de outbox, worker e idempotencia base | `P0-04`,`P2-02` | assincronia interna | Codex |
| `P2-11` | criar hub base SignalR | autenticacao, grupos e contrato de notificacao base | `P2-04`,`P1-01` | realtime foundation | Codex |

Criterios de aceite da fase:
- login e `RBAC` funcionando;
- auditoria minima gravando eventos;
- banco, storage e outbox prontos para uso;
- frontend ja autenticado contra backend real.

### `P3` - Clientes, ativos, pedidos e anexos

Objetivo:
- entregar o fluxo comercial base ate o pedido consolidado.

Entregaveis de fase:
- cadastros de clientes e ativos;
- abertura de pedido;
- escopo flexivel;
- upload de anexos;
- pedido consolidado com historico.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P3-01` | modelar cliente | entidade, contatos, enderecos, regras comerciais e score | `P2-02` | dominio de cliente | Codex |
| `P3-02` | construir telas de cliente | listagem, detalhe, cadastro e edicao | `P3-01`,`P1-05` | modulo de clientes | Codex |
| `P3-03` | modelar ativo tecnico | entidade de ativo, historico e ligacao com cliente | `P3-01` | dominio de ativos | Codex |
| `P3-04` | construir telas de ativos | listagem, detalhe e associacao a pedido | `P3-03`,`P1-05` | modulo de ativos | Codex |
| `P3-05` | modelar pedido e status | entidade de pedido, tipos, transicoes e historico | `P2-02` | dominio de pedido | Codex |
| `P3-06` | modelar itens de escopo | produto principal, componente, acessorio, servico, manutencao e adaptacao | `P3-05`,`P3-03` | dominio de escopo | Codex |
| `P3-07` | construir fluxo de novo pedido | pagina 2 e 3 do wireframe com foco em rapidez | `P3-05`,`P3-06`,`P1-05` | fluxo de abertura | Codex |
| `P3-08` | implementar upload de arquivos | hash, versionamento, metadados, seguranca e storage | `P2-09`,`P3-05` | pipeline de anexos | Codex |
| `P3-09` | implementar historico de anexos | vinculo com pedido, ativo e revisao | `P3-08`,`P1-05` | historico de arquivos | Codex |
| `P3-10` | construir pedido consolidado | pagina 7 em abas com resumo, arquivos, componentes, OPs, logistica e historico | `P3-05`,`P3-06`,`P3-09`,`P1-05` | pedido consolidado | Codex |
| `P3-11` | aplicar autorizacao e auditoria no fluxo comercial | validar papeis e historico de mudanca | `P2-05`,`P2-07`,`P3-10` | modulo seguro | Codex |

Criterios de aceite da fase:
- pedido pode ser aberto sem arquivo;
- pedido pode usar ativo antigo;
- anexos ficam versionados e rastreaveis;
- pedido consolidado esta operacional.

### `P4` - Analise tecnica `PDF/DXF`

Objetivo:
- fazer o sistema reagir tecnicamente ao upload de arquivos.

Entregaveis de fase:
- parser de `PDF` adaptado;
- analisador de `DXF` integrado;
- retorno de confianca;
- historico tecnico;
- painel tecnico unico.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P4-01` | definir contrato de analise tecnica | request, result, erro, confianca, reprocessamento e vinculos | `P0-04`,`P2-10` | contrato tecnico | Codex |
| `P4-02` | refinar parser real de `PDF` | ampliar extracao de OP, cliente, materiais, entrega, usuario e sinais tecnicos | `P4-01` | parser documental | Codex |
| `P4-03` | normalizar retorno do `PDF` | mapear campos extraidos, confianca e flags de baixa certeza | `P4-02` | modelo documental | Codex |
| `P4-04` | evoluir analisador real de `DXF` | score, metricas, render e explicacoes | `P4-01` | motor geometrico | Codex |
| `P4-05` | integrar upload com analise imediata | disparar pipeline no upload e armazenar resultado | `P3-08`,`P4-03`,`P4-04` | pipeline automatico | Codex |
| `P4-06` | persistir revisoes tecnicas | versionar analise, origem, data e operador responsavel | `P4-05` | historico tecnico | Codex |
| `P4-07` | implementar sugestao de preenchimento | usar `PDF` para sugerir campos do pedido sem gravar cegamente | `P4-03`,`P3-07` | preenchimento assistido | Codex |
| `P4-08` | construir painel de analise tecnica | conciliar `PDF` e `DXF` em uma mesma tela | `P4-06`,`P1-05` | pagina 4 funcional | Codex |
| `P4-09` | implementar reprocessamento | permitir reanalisar arquivo e manter trilha | `P4-06` | operacao de reprocesso | Codex |
| `P4-10` | criar testes de analise | cenarios de `PDF`, `DXF`, erro, parcial e duplicidade | `P4-02`,`P4-04`,`P4-05` | suite tecnica inicial | Codex + GeminiCLI |

Criterios de aceite da fase:
- `PDF` retorna analise no momento do upload;
- `DXF` retorna score, metrica e render;
- baixas confiancas sao exibidas claramente;
- painel tecnico mostra o que veio de `PDF` e o que veio de `DXF`.

### `P5` - Estimador e orcamento

Objetivo:
- transformar analise tecnica em previsao e proposta comercial.

Entregaveis de fase:
- estimador deterministico por etapa;
- prazo sugerido;
- gargalo previsto;
- custo base e preco sugerido;
- aprovacao comercial.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P5-01` | modelar parametros do estimador | tempos base, pesos, filas e regras por etapa | `P4-06` | base do estimador | Codex |
| `P5-02` | implementar calculo de estimativa | setup, execucao, fila, total, gargalo e confianca | `P5-01` | motor de estimativa | Codex |
| `P5-03` | modelar preco sugerido e margem | custo base, margem, preco final e overrides | `P5-02` | modelo comercial | Codex |
| `P5-04` | implementar aprovacao/reprovacao | workflow, justificativa e historico | `P3-10`,`P5-03` | status comercial | Codex |
| `P5-05` | construir tela de estimativa | pagina 5 com cards de etapa, gargalo e prazo | `P5-02`,`P1-05` | estimativa visual | Codex |
| `P5-06` | construir tela de orcamento | pagina 6 com preco sugerido, margem e observacoes | `P5-03`,`P5-04`,`P1-05` | orcamento funcional | Codex |
| `P5-07` | integrar pedido consolidado com quote | exibir status, valor, historico e travas | `P3-10`,`P5-04` | consolidado ampliado | Codex |
| `P5-08` | testar cenarios comerciais | com arquivo, sem arquivo, repeticao e ajustes manuais | `P5-06` | suite comercial | Codex + GeminiCLI |

Criterios de aceite da fase:
- pedido analisado gera estimativa coerente;
- usuario consegue ajustar preco sem perder trilha;
- pedido aprovado ja fica apto para virar OP.

### `P6` - Producao e realtime

Objetivo:
- transformar pedido aprovado em operacao viva no chao de fabrica.

Entregaveis de fase:
- OPs;
- split/merge;
- setores;
- filas operacionais;
- apontamentos;
- atualizacao em tempo real.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P6-01` | modelar OPs | entidade, etapas, status e relacao com pedido | `P5-04` | dominio de producao | Codex |
| `P6-02` | gerar OPs a partir do pedido aprovado | mapping de itens, quantidades e setores iniciais | `P6-01` | geracao de OP | Codex |
| `P6-03` | endurecer split e merge | cobrir apontamentos, replanejamento e trilha completa | `P6-01` | flexibilidade de OP | Codex |
| `P6-04` | modelar filas por setor | prioridade, responsavel, SLA e atraso | `P6-01` | dominio de fila | Codex |
| `P6-05` | integrar SignalR em OPs e filas | push seletivo por setor, pedido e perfil | `P2-11`,`P6-04` | realtime operacional | Codex |
| `P6-06` | construir tela indice de setores | pagina 9 com poucos indicadores operacionais | `P6-04`,`P1-05` | indice de producao | Codex |
| `P6-07` | construir tela de fila de montagem | pagina 15 com responsavel e acoes rapidas | `P6-04`,`P6-05`,`P1-05` | fila de montagem | Codex |
| `P6-08` | construir tela de emborrachamento | pagina 16 com `dar baixa` dominante | `P6-04`,`P6-05`,`P1-05` | fila de emborrachamento | Codex |
| `P6-09` | implementar apontamentos operacionais | inicio, pausa, conclusao, retrabalho e observacao | `P6-04`,`P6-05` | feedback operacional | Codex |
| `P6-10` | integrar pedido consolidado com producao | exibir OPs, travas e historico auditavel | `P6-02`,`P3-10` | ponte pedido -> fabrica | Codex |
| `P6-11` | executar teste de operacao real | validar fluxo com exemplos concretos da facaria | `P6-07`,`P6-08`,`P6-09` | validacao de campo | voce + Codex |

Criterios de aceite da fase:
- pedido aprovado vira OP;
- OP aparece em fila correta;
- split/merge nao quebra historico;
- filas atualizam em tempo real;
- operador consegue agir com poucos cliques.

### `P7` - Materiais, estoque e expedicao

Objetivo:
- fechar o ciclo operacional ate a saida do pedido.

Entregaveis de fase:
- materiais tecnicos;
- estoque real;
- reservas e baixas;
- lote de expedicao;
- tela operacional de expedicao.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P7-01` | modelar materiais tecnicos e itens reais | separar tipo tecnico de item de estoque | `P2-02` | dominio de materiais | Codex |
| `P7-02` | implementar cadastro de materiais e fornecedores | custo, categoria, status e risco | `P7-01`,`P1-05` | cadastro funcional | Codex |
| `P7-03` | modelar saldo e movimentacao | entrada, saida, ajuste, reserva e motivo | `P7-01` | dominio de estoque | Codex |
| `P7-04` | reservar material por pedido/OP | ligacao com producao e visao de falta | `P7-03`,`P6-02` | reserva funcional | Codex |
| `P7-05` | construir tela de materiais | pagina 10 com filtros e resumos | `P7-02`,`P1-05` | tela de materiais | Codex |
| `P7-06` | construir tela de estoque | pagina 11 com alerta de reposicao e ultima movimentacao | `P7-03`,`P1-05` | tela de estoque | Codex |
| `P7-07` | modelar lote de expedicao | saida, retirada, entrega, terceiro e ocorrencia | `P6-02` | dominio logistico | Codex |
| `P7-08` | implementar checklist e comprovantes | comprovante, recebedor, transportador e evidencias | `P7-07` | expediente logistico | Codex |
| `P7-09` | construir tela gerencial de logistica | pagina 12 com lotes, acoes e entregas do dia | `P7-07`,`P1-05` | logistica gerencial | Codex |
| `P7-10` | construir tela operacional de expedicao | pagina 17 com foco em lote e tablet | `P7-07`,`P7-08`,`P6-05`,`P1-05` | expedicao operacional | Codex |
| `P7-11` | implementar saida adversa | fluxo, justificativa e historico | `P7-08`,`P7-10` | excecao logistica | Codex |

Criterios de aceite da fase:
- materiais e estoque estao separados corretamente;
- OP pode reservar material;
- expedicao funciona em lote;
- saida adversa fica registrada e rastreavel.

### `P8` - Financeiro funcional

Objetivo:
- cobrir o financeiro necessario para operar o negocio.

Entregaveis de fase:
- contas a receber;
- contas a pagar;
- baixa manual;
- status financeiro por pedido;
- visao financeira basica.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P8-01` | modelar contas a receber | titulo, vencimento, valor, status e vinculo | `P2-02` | dominio de recebiveis | Codex |
| `P8-02` | modelar contas a pagar | fornecedor, categoria, vencimento e status | `P2-02` | dominio de pagaveis | Codex |
| `P8-03` | gerar previsao financeira do pedido | gerar titulos baseados na aprovacao comercial | `P5-04`,`P8-01` | previsao automatica | Codex |
| `P8-04` | implementar baixas manuais | baixa, estorno controlado, observacao e trilha | `P8-01`,`P8-02` | operacional financeiro | Codex |
| `P8-05` | construir tela financeira | pagina 13 com receber, pagar, resultado e notas | `P8-01`,`P8-02`,`P1-05` | financeiro funcional | Codex |
| `P8-06` | integrar pedido com financeiro | exibir vinculo e pendencias por pedido | `P3-10`,`P8-03` | visao integrada | Codex |
| `P8-07` | aplicar segregacao forte de acesso | restringir visualizacao para perfis corretos | `P2-05`,`P8-05` | seguranca financeira | Codex |
| `P8-08` | testar cenarios financeiros | recebimento parcial, baixa, estorno controlado e erro | `P8-04`,`P8-05` | suite financeira | Codex + GeminiCLI |

Criterios de aceite da fase:
- existe financeiro por pedido;
- usuario autorizado consegue baixar e rastrear;
- operadores nao veem financeiro;
- tela financeira nao mistura leitura operacional.

### `P9` - Fiscal NF-e

Objetivo:
- entregar o nucleo fiscal de producao para `NF-e`, com emissao real junto ao SEFAZ.

Detalhamento dedicado:
- ver `docs/backlog-modulo-fiscal-nfe.md` para a quebra por fases `F0` a `F5`, gates de go-live e backlog tecnico do modulo.
- usar `docs/checklist-implantacao-fiscal-emitente.md` e `docs/runbook-implantacao-fiscal.md` na implantacao por emitente.

Entregaveis de fase:
- emissor fiscal;
- templates de operacao;
- snapshot fiscal;
- XML assinado;
- envio/consulta;
- cancelamento;
- inutilizacao;
- CC-e;
- DANFE;
- arquivo fiscal.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P9-01` | modelar emissor fiscal | estabelecimento, certificado, serie e numeracao | `P2-02`,`P2-05` | emissor fiscal | Codex |
| `P9-02` | modelar templates de operacao | CFOP, natureza, finalidade, regime e defaults | `P9-01` | operacoes fiscais | Codex |
| `P9-03` | modelar `FiscalDocument` | agregado, estados, itens, eventos e snapshots | `P9-02` | core fiscal | Codex |
| `P9-04` | implementar snapshot fiscal do pedido | congelar dados fiscais dos itens no momento de emissao | `P3-06`,`P9-03` | base tributaria | Codex |
| `P9-05` | construir `XML Builder` | gerar XML NF-e conforme leiaute vigente | `P9-03`,`P9-04` | emissao XML | Codex |
| `P9-06` | implementar assinatura digital | certificado, cadeia e erros de assinatura | `P9-05` | assinatura fiscal | Codex |
| `P9-07` | implementar adapter de transmissao | homologacao, consulta, retorno e protocolo | `P9-06` | integracao SEFAZ | Codex |
| `P9-08` | persistir XML, protocolo e DANFE | archive fiscal imutavel e recuperavel | `P9-07`,`P2-09` | arquivo fiscal | Codex |
| `P9-09` | implementar cancelamento | evento de cancelamento e trilha | `P9-08` | cancelamento fiscal | Codex |
| `P9-10` | implementar inutilizacao | faixa, justificativa e consulta | `P9-08` | inutilizacao fiscal | Codex |
| `P9-11` | implementar CC-e | correcao controlada e historico | `P9-08` | correcao fiscal | Codex |
| `P9-12` | integrar fiscal com pedido e financeiro | status, numero da nota e vinculos | `P8-06`,`P9-08` | visao integrada | Codex |
| `P9-13` | construir telas fiscais | configuracao do emissor, historico e eventos | `P9-12`,`P1-05` | UX fiscal | Codex |
| `P9-14` | montar suite de homologacao fiscal | emissao, cancelamento, inutilizacao e CC-e | `P9-09`,`P9-10`,`P9-11` | homolog fiscal | Codex + GeminiCLI |
| `P9-15` | validar matriz fiscal real com contador | revisar operacoes, naturezas e excecoes reais | `P9-02`,`P9-14` | fiscal validada | voce |

Criterios de aceite da fase:
- emissao em homologacao funciona ponta a ponta;
- XML, protocolo e DANFE ficam arquivados;
- eventos fiscais posteriores funcionam;
- pedido e financeiro enxergam o estado fiscal.

### `P10` - PackControl Edge

Objetivo:
- trazer sinais da fabrica para o ERP de forma robusta.

Entregaveis de fase:
- agente local;
- spool local;
- eventos publicados;
- consumo no backend;
- projeção em filas operacionais.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P10-01` | fechar eventos canonicos do edge | tipos, payloads, ids e versionamento | `P0-04`,`P6-04` | contrato edge | Codex |
| `P10-02` | criar skeleton do agente | config, lifecycle, logging e healthcheck | `P10-01` | base do Edge | Codex |
| `P10-03` | implementar watchers locais | diretorios, debounce e filtros | `P10-02` | coleta local | Codex |
| `P10-04` | implementar hash e deduplicacao | evitar repeticao e garantir idempotencia | `P10-03` | confiabilidade de evento | Codex |
| `P10-05` | implementar spool local | fila local persistente para falha de internet | `P10-02` | resiliencia local | Codex |
| `P10-06` | implementar publicacao RabbitMQ | conexao, retry e confirmacao | `P10-05` | integracao de borda | Codex |
| `P10-07` | criar consumidores backend | receber evento, validar, deduplicar e projetar | `P10-06`,`P2-10` | integracao edge -> ERP | Codex |
| `P10-08` | integrar eventos com producao | atualizar fila, OP ou sinal tecnico conforme contrato | `P10-07`,`P6-04` | efeito operacional | Codex |
| `P10-09` | integrar pipeline tecnica herdada | reaproveitar `DXF`/`PDF` quando fizer sentido | `P4-02`,`P4-04`,`P10-02` | ponte watcher -> edge | Codex |
| `P10-10` | criar instalacao e runbook do Edge | configuracao local, troubleshooting e logs | `P10-06` | kit operacional edge | Codex |
| `P10-11` | validar em ambiente semelhante ao real | simular queda de internet, duplicidade e reorder | `P10-05`,`P10-08` | validacao robusta | voce + Codex |

Criterios de aceite da fase:
- edge funciona sem inbound;
- queda de internet nao perde evento;
- backend nao duplica projeção;
- fila operacional reage ao sinal vindo da fabrica.

### `P11` - Hardening, homologacao e go-live

Objetivo:
- transformar o sistema em algo operavel em producao.

Entregaveis de fase:
- backup;
- restore testado;
- monitoramento;
- checklist de release;
- regressao do MVP;
- homolog fiscal concluida;
- plano de go-live.

| ID | Tarefa | Detalhamento | Dependencias | Entregavel | Dono sugerido |
|---|---|---|---|---|---|
| `P11-01` | configurar backup e restore | banco, storage, retention e teste de restauracao | `P2-09`,`P2-02` | estrategia de recuperacao | Codex |
| `P11-02` | endurecer seguranca final | rate limiting, headers, segredos, sessions e CSRF | `P2-06`,`P8-07`,`P9-13` | baseline segura | Codex |
| `P11-03` | consolidar observabilidade | dashboards, healthchecks e alertas minimos | `P2-08`,`P10-07` | monitoramento operacional | Codex |
| `P11-04` | executar regressao funcional do MVP | fluxos de pedido, analise, OP, estoque, financeiro, fiscal e edge | `P7`,`P8`,`P9`,`P10` | suite de regressao executada | Codex + GeminiCLI |
| `P11-05` | executar homologacao fiscal final | emissao, cancelamento, inutilizacao e CC-e em ambiente correto | `P9-14` | fiscal pronta | voce + Codex |
| `P11-06` | montar checklist de deploy | segredos, migrations, storage, filas e rollback | `P11-01`,`P11-02`,`P11-03` | runbook de deploy | Codex |
| `P11-07` | preparar dados de piloto | usuarios, cadastros, templates e emissor | `P3`,`P8`,`P9` | ambiente pronto para piloto | voce + Codex |
| `P11-08` | executar go-live controlado | liberar para uso monitorado e acompanhar eventos | `P11-06`,`P11-07` | entrada em producao | voce + Codex |
| `P11-09` | operar janela de estabilizacao | tratar bug, ajuste fino e gaps documentais | `P11-08` | estabilizacao inicial | voce + Codex |

Criterios de aceite da fase:
- backup e restore foram testados;
- regressao minima do MVP passou;
- homolog fiscal esta validada;
- existe plano claro de rollback e suporte inicial.

## 9. Decomposicao adicional dos modulos mais criticos

### 9.1 Analise tecnica `PDF`

Subtarefas internas obrigatorias:
- mapear todos os campos extraiveis do parser atual;
- separar campo `extraido`, `normalizado` e `confirmado`;
- modelar confianca por campo;
- distinguir `falha total`, `parcial`, `ambiguo` e `ok`;
- exibir sugestao de preenchimento sem sobrescrever dado confirmado pelo usuario;
- guardar o texto bruto ou evidencias suficientes para troubleshooting;
- permitir reprocessar depois de evoluir o parser.

### 9.2 Analise tecnica `DXF`

Subtarefas internas obrigatorias:
- tratar upload, hash e deduplicacao;
- versionar analise e imagem;
- manter explicacoes do score;
- guardar versao da engine;
- permitir reprocessar por mudanca de regra;
- separar sinal tecnico bruto de interpretacao de negocio.

### 9.3 Fiscal `NF-e`

Subtarefas internas obrigatorias:
- tratar numeracao sem corrida indevida;
- separar `FiscalDocument` de `Order`;
- congelar snapshot tributario;
- isolar adaptador SEFAZ;
- registrar eventos fiscais como historico imutavel;
- arquivar XML, protocolo e DANFE com retention forte;
- validar erros de transporte, assinatura e schema separadamente;
- criar matriz de testes para cenarios reais da facaria.

### 9.4 `PackControl Edge`

Subtarefas internas obrigatorias:
- `store and forward`;
- retry exponencial;
- fila local transacional;
- chaves de idempotencia;
- configuracao remota simples;
- logs legiveis para suporte remoto;
- possibilidade de rodar em ambiente Windows da fabrica;
- suporte a reconexao sem efeito duplicado no ERP.

## 10. Paralelismo recomendado

Janela 1:
- `P1` frontend shell
- `P2-01` a `P2-03` backend base

Janela 2:
- `P3-01` a `P3-06` dominio comercial
- `P3-08` pipeline de anexos

Janela 3:
- `P4-02` parser de `PDF`
- `P4-04` motor de `DXF`

Janela 4:
- `P7` materiais/logistica
- `P8` financeiro base

Janela 5:
- `P9` fiscal
- `P10-01` a `P10-03` preparacao do edge

Sugestao de uso do GeminiCLI nessas janelas:
- revisar PRs ou diffs grandes;
- gerar casos de teste;
- fazer sanity check de fiscal e edge;
- revisar nomes de contrato e mensagens de erro.

## 11. Checklist de documentacao por fase

Para cada fase, atualizar:
- descricao do modulo;
- entidades novas;
- eventos novos;
- endpoints novos;
- tela nova e fluxo correspondente;
- testes obrigatorios;
- riscos conhecidos;
- runbook, se o modulo for operacionalmente sensivel.

## 12. Checklist de testes por fase

Minimo por fase:
- unitario de regra principal;
- integracao da persistencia ou contrato;
- smoke UI da tela principal;
- validacao de permissao;
- caso de erro controlado;
- caso de auditoria quando a acao for sensivel.

Adicionais obrigatorios:
- fiscal: homologacao e contrato;
- edge: resiliencia, duplicidade e reorder;
- analise tecnica: parcial, erro e reprocessamento.

## 13. Definition of Ready

Uma tarefa so entra em execucao quando:
- objetivo esta claro;
- dependencia principal esta pronta;
- criterio de aceite existe;
- dono da decisao funcional esta identificado;
- impacto de seguranca foi pensado;
- impacto de documentacao esta conhecido.

## 14. Definition of Done

Uma tarefa so fecha quando:
- comportamento principal funciona;
- casos de erro nao quebram o fluxo;
- autorizacao esta aplicada;
- logs estao uteis;
- teste minimo existe;
- documentacao correspondente foi atualizada.

## 15. Riscos que exigem gatilho de revisao de plano

Replanejar imediatamente se:
- fiscal exigir `MDF-e` no dia 1;
- parser de `PDF` falhar em volume grande de documentos reais;
- reaproveitamento do `FileWatcherApp` for menor que 40% do previsto;
- as telas operacionais exigirem reescrita de shell ou fluxo;
- a infraestrutura barata nao sustentar fila, banco e storage;
- o escopo de financeiro crescer para conciliacao bancaria ou contabilidade.

## 16. Marcos de validacao com o negocio

Validacoes obrigatorias:
- fim de `P1`: shell e navegacao aprovados;
- fim de `P3`: fluxo comercial base aprovado;
- fim de `P4`: analise de `PDF` e `DXF` validada em arquivos reais;
- fim de `P6`: operacao de producao validada;
- fim de `P7`: expedicao validada;
- fim de `P8`: financeiro minimo validado;
- fim de `P9`: fiscal homologada;
- fim de `P11`: piloto aprovado para go-live.

## 17. Primeira fila de execucao recomendada

Ordem imediata das proximas tarefas:
1. `P0-01`
2. `P0-02`
3. `P0-03`
4. `P0-04`
5. `P1-01`
6. `P1-02`
7. `P1-03`
8. `P1-04`
9. `P2-01`
10. `P2-02`
11. `P2-03`
12. `P2-04`

Se a execucao comecar agora, o primeiro objetivo concreto nao deve ser "abrir feature". Deve ser:
- shell Angular navegavel;
- backend autenticando;
- pedido vazio abrindo e salvando.

## 18. Resultado esperado do MVP

Ao final do plano, o PackControl deve ser capaz de:
- receber um pedido sem arquivo ou com arquivo;
- analisar `PDF` e `DXF` rapidamente;
- gerar estimativa e orcamento;
- transformar pedido em OP;
- acompanhar producao em filas operacionais;
- reservar e movimentar materiais;
- expedir em lote;
- registrar financeiro basico;
- emitir `NF-e`;
- receber sinais do chao de fabrica pelo `Edge`;
- operar com rastreabilidade, auditoria e seguranca adequadas.
