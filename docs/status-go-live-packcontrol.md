# Status de Go-Live - PackControl

Versao: 0.1  
Data: 2026-03-28  
Status: Checkpoint consolidado do projeto com separacao explicita entre pronto para deploy tecnico e pronto para go-live vendavel

Documentos complementares:
- `README.md`
- `docs/plano-execucao-packcontrol.md`
- `docs/backlog-packcontrol.md`
- `docs/runbook-deploy-packcontrol.md`
- `docs/runbook-implantacao-fiscal.md`

## 1. Leitura executiva

O PackControl ja saiu da fase de bootstrap e hoje possui:
- `frontend` Angular navegavel e conectado a `API` real;
- `backend` ASP.NET Core funcional com autenticacao por cookie;
- persistencia configuravel em `InMemory` ou `PostgreSQL` por snapshot;
- fluxo operacional base de pedido ate financeiro/fiscal;
- trilha fiscal canonica com adapter real `A1` via `Unimake.DFe`;
- artefatos de deploy tecnico versionados no repositorio.

Traduzindo:
- o projeto esta apto para `deploy tecnico interno`;
- o projeto ainda nao esta apto para `go-live vendavel` sem fechar os blocos listados neste documento.

## 2. O que ja foi feito

### 2.1 Fundacao e plataforma

- shell Angular em PT-BR com login e modulos principais;
- API modular com `Domain`, `Application`, `Infrastructure` e controllers;
- autenticacao por cookie com seeds locais;
- tratamento global de erro e suite local de build/teste;
- persistencia configuravel em `InMemory` ou `PostgreSQL`;
- storage em disco local para anexos e artefatos;
- endpoints `GET /health/live` e `GET /health/ready`.

### 2.2 Operacao principal

- clientes, ativos, produtos, transportadoras, cadastros, materiais e estoque;
- abertura de pedido com contexto inicial, escopo flexivel e anexos;
- analise tecnica real de `PDF` e `DXF`;
- producao com filas iniciais e `split/merge` auditavel;
- logistica e expedicao em baseline funcional;
- financeiro manual com contas a receber/pagar e boleto.

### 2.3 Fiscal

- camada fiscal canonica com `prepare` e `issue`;
- snapshot fiscal congelado por documento;
- onboarding de emitente, empresa emissora e templates;
- perfil `A1/A3` modelado;
- adapter `mock-plugavel` para smoke local;
- adapter real `unimake.dfe` para emissao `A1`;
- `cancelamento`, inutilizacao e `CC-e` na camada canonica e no adapter real;
- timeline e artefatos fiscais no ERP;
- diagnostico do engine e bloqueio explicito quando falta certificado/configuracao.

### 2.4 Deploy tecnico

- `deploy/backend/Dockerfile`;
- `deploy/frontend/Dockerfile`;
- `deploy/nginx/packcontrol.conf`;
- `deploy/docker-compose.production.yml`;
- `deploy/.env.example`;
- runbook e checklist de deploy tecnico.

## 3. O que ainda falta

### 3.1 Falta fechavel por codigo

- persistencia relacional com migrations formais, saindo do snapshot unico;
- estimador deterministico e orcamento comercial;
- hardening final de seguranca com `MFA`, `CSRF`, `rate limiting`, politicas de sessao e segredo fora de config plana;
- `DANFE` oficial em vez de representacao simplificada;
- operacao `A3` via agente local alem do skeleton atual;
- backup/restore automatizados e healthchecks mais amplos de producao.

### 3.2 Falta dependente de contexto externo

- emitente real escolhido para o rollout;
- matriz fiscal validada pelo contador;
- credenciamento do emitente em homologacao e producao;
- certificado `A1` ou infraestrutura `A3` do emitente;
- segredos reais e infraestrutura alvo de producao;
- homologacao fiscal real junto ao SEFAZ;
- smoke controlado de producao com documento real.

## 4. O que bloqueia somente go-live vendavel

Itens que nao impedem subir o sistema internamente, mas impedem vender/operar em campo:
- `NF-e` homologada com emitente real;
- eventos fiscais homologados no emitente;
- `DANFE` oficial;
- `MFA` para perfis sensiveis;
- backup/restore validados no ambiente alvo;
- runbook de suporte da operacao real fechado com o cliente.

## 5. Definicao pratica de pronto

### 5.1 Pronto para deploy tecnico

Considerar pronto quando:
- `docker compose` sobe `web`, `api` e `postgres`;
- `GET /health/live` e `GET /health/ready` respondem com sucesso;
- login funciona;
- SPA abre e consome `API`;
- storage persiste anexos/artefatos no volume configurado.

### 5.2 Pronto para go-live vendavel

Considerar pronto somente quando:
- estimador e orcamento estiverem fechados ou forem conscientemente removidos do primeiro corte;
- seguranca minima de producao estiver ativa;
- fiscal estiver homologada por emitente;
- restore de banco e artefatos estiver validado;
- a operacao aceitar o fluxo sem emissor paralelo como caminho principal.

## 6. Ordem recomendada de fechamento

1. Fechar persistencia relacional e restore.
2. Fechar hardening minimo de seguranca.
3. Decidir se estimador/orcamento entram no primeiro corte.
4. Homologar fiscal real `A1` com um emitente.
5. Tratar `DANFE` oficial.
6. Postergar `A3/Edge` se o primeiro rollout puder entrar so com `A1`.
