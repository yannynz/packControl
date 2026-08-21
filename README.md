# PackControl


Baseline funcional do ERP PackControl, alinhada ao `docs/plano-execucao-packcontrol.md` e atualizada ate `2026-03-28`.

Documentos de status e go-live:
- `docs/status-go-live-packcontrol.md`
- `docs/plano-proximo-ciclo-packcontrol.md`
- `docs/runbook-deploy-packcontrol.md`
- `docs/checklist-deploy-packcontrol.md`
- `docs/runbook-implantacao-fiscal.md`
- `docs/checklist-implantacao-fiscal-emitente.md`

## Estrutura

- `frontend/packcontrol-web`: SPA Angular do ERP.
- `backend/src`: API, aplicacao, dominio e infraestrutura.
- `backend/tests`: testes de dominio e API.
- `edge/src/PackControl.Edge`: skeleton do agente local de fabrica.
- `shared/PackControl.Contracts`: contratos compartilhados entre ERP e edge.
- `docs`: PRD, SDS, backlog, plano de execucao e documentos dedicados do modulo fiscal.

## Estado atual

Esta baseline cobre:
- autenticacao por cookie com seed local e perfis de `Administrador`, `Comercial`, `Engenharia` e `Financeiro`;
- painel em PT-BR com navegacao para `Pedidos`, `Producao`, `Logistica`, `Transportadoras`, `Clientes`, `Ativos`, `Produtos`, `Cadastros`, `Materiais`, `Estoque`, `Financeiro` e `Configuracoes`;
- persistencia configuravel em `InMemory` ou `PostgreSQL`, com snapshot `JSONB` em `public.app_state_snapshots`;
- storage local em disco para anexos, artefatos tecnicos e artefatos fiscais;
- cadastro operacional de clientes com apelidos, endereco completo, transportadora/modal padrao e regras comerciais por produto;
- cadastro operacional de clientes com apelidos, endereco completo, codigo `IBGE` do municipio, IE, indicador fiscal do destinatario, transportadora/modal padrao e regras comerciais por produto;
- modulo de ativos tecnicos por cliente, com componentes, materiais, revisao e referencia ao ultimo pedido;
- cadastro de produtos comerciais com meios de cobranca, setor inicial, materiais consumidos por unidade, composicao base e defaults fiscais (`NCM`, `CFOP`, unidade, origem, `CST`/`CSOSN` e aliquotas);
- area de transportadoras com horarios, contatos, area atendida, coleta/entrega e padroes de trabalho;
- abertura de pedido com contexto inicial opcional, selecao de ativo antigo e escopo flexivel por produto comercial, quantidade, cobranca e valor unitario;
- aplicacao automatica de tabela comercial do cliente ao selecionar o produto no pedido;
- pedido consolidado em abas com resumo, arquivos, componentes, OPs, logistica, financeiro previsto e historico;
- upload de anexos com analise tecnica real para `PDF` e `DXF`, incluindo motor utilizado e percentual de confianca;
- aprovacao do pedido com projecao de OPs, lote logistico, lancamentos financeiros e baixa automatica de estoque baseada na composicao do produto;
- producao com visao geral, filas dedicadas de `Montagem` e `Emborrachamento`, e suporte a `split/merge` auditavel de OPs;
- financeiro com contas a receber/pagar manuais, geracao de boleto e camada fiscal canonica com `prepare/issue`, `cancelamento`, `CC-e` e inutilizacao de faixa, onboarding do emitente, administracao de empresa/template, perfis `A1/A3`, snapshot fiscal congelado de emitente/destinatario/itens/totais/pagamento/transporte, timeline de eventos/artefatos fiscais, codigo `IBGE` no endereco fiscal, builder `XML 55`, trilha real `A1` via `Unimake.DFe` com assinatura/transmissao/recibo/protocolo, arquivamento de `XML`/`DANFE`, roteamento de adapters fiscais e bloqueios operacionais por readiness fiscal;
- readiness operacional com `GET /health/live` e `GET /health/ready`, incluindo verificacao de persistencia e storage local;
- artefatos de deploy tecnico em `deploy/`, com `Dockerfile` de `API` e `frontend`, `nginx` de borda e `docker-compose.production.yml`;
- cadastro operacional separado em `Clientes`, `Ativos`, `Produtos`, `Transportadoras`, `Cadastros`, `Materiais` e `Estoque`;
- skeleton do `PackControl Edge`.

## Estado de go-live

Feito e pronto para deploy tecnico:
- shell web, API, persistencia configuravel, storage local, build/testes e compose de referencia;
- fluxo operacional base de pedido, producao, logistica, financeiro e fiscal canonico;
- endpoints de readiness para validar `API`, persistencia e storage;
- runbook e checklist de deploy tecnico.

Ainda aberto para go-live vendavel:
- persistencia relacional com migrations formais;
- estimador deterministico e orcamento;
- hardening final de seguranca com `MFA`, `CSRF`, `rate limiting` e segredos fora de config plana;
- homologacao fiscal real por emitente com certificado/credenciamento;
- `DANFE` oficial, `A3` operacional e rollout fiscal assistido;
- backup/restore real validados na infraestrutura alvo.

## Execucao local

### Backend rapido

```bash
dotnet run --project backend/src/PackControl.Api
```

Sem variaveis extras, a API sobe com persistencia em memoria e storage local.

Overrides locais opcionais podem ficar em:
- `backend/src/PackControl.Api/appsettings.Local.json`
- `backend/src/PackControl.Api/appsettings.Development.Local.json`

Esses arquivos nao devem ir para o repositorio e ja estao ignorados no `.gitignore`.

### Backend com `PostgreSQL`

```bash
docker run --name packcontrol-pg \
  -e POSTGRES_DB=packcontrol \
  -e POSTGRES_USER=packcontrol \
  -e POSTGRES_PASSWORD=packcontrol \
  -p 55432:5432 \
  -d postgres:16-alpine

StatePersistence__Provider=PostgreSQL \
StatePersistence__ConnectionString="Host=localhost;Port=55432;Database=packcontrol;Username=packcontrol;Password=packcontrol" \
dotnet run --project backend/src/PackControl.Api
```

### Backend com homologacao fiscal `A1` local

1. Copie `backend/src/PackControl.Api/appsettings.Development.Local.example.json` para `backend/src/PackControl.Api/appsettings.Development.Local.json`.
2. Preencha `CertificatePath` ou `CertificateBase64`, junto com `CertificatePassword`.
3. Mantenha `AllowRealEmission` em `false` ate confirmar certificado, emitente e ambiente.
4. Quando for rodar o smoke real em homologacao, altere `AllowRealEmission` para `true`.
5. Suba a API com `dotnet run --project backend/src/PackControl.Api`.

Observacoes:
- o repositorio nao carrega nenhum `PFX` de exemplo; o certificado precisa vir do emitente;
- a trilha atual usa `Unimake.DFe` para assinatura/transmissao com SEFAZ;
- `UniDANFE` e um produto separado para renderizacao/impressao oficial do `DANFE`.

### Deploy tecnico de referencia

```bash
cp deploy/.env.example deploy/.env
docker compose --env-file deploy/.env -f deploy/docker-compose.production.yml up --build -d
```

Artefatos de deploy:
- `deploy/docker-compose.production.yml`
- `deploy/backend/Dockerfile`
- `deploy/frontend/Dockerfile`
- `deploy/nginx/packcontrol.conf`

Smokes minimos apos subir:
- `GET /health/live`
- `GET /health/ready`
- abrir a SPA pela porta configurada em `PACKCONTROL_WEB_PORT`

Observacoes:
- o compose de referencia sobe `web`, `api` e `postgres`;
- o storage de arquivos fica em volume persistente do container da `API`;
- a trilha fiscal real continua dependendo de certificado/credenciamento do emitente;
- o passo a passo operacional esta em `docs/runbook-deploy-packcontrol.md`.

Usuarios seed:
- `admin@packcontrol.local` / `PackControl!123`
- `comercial@packcontrol.local` / `PackControl!123`
- `engenharia@packcontrol.local` / `PackControl!123`
- `financeiro@packcontrol.local` / `PackControl!123`

### Frontend

```bash
cd frontend/packcontrol-web
npm start
```

O frontend usa proxy para `/api` apontando para `http://localhost:5010`.
Entrada local: `http://localhost:4200/entrar`

### Edge

```bash
dotnet run --project edge/src/PackControl.Edge
```

## Proximos encaixes

- evoluir a persistencia `snapshot` em `PostgreSQL` para modelo relacional com migrations formais e healthchecks de banco;
- enriquecer a analise tecnica com renderizacao e score visual mais completos para `DXF`, alem de conciliacao mais rica entre `PDF` e `DXF`;
- fechar ativos tecnicos com anexos proprios, historico de revisoes e comparacao de versoes;
- concluir estimador deterministico e orcamento com margem, custo previsto e aprovacao comercial;
- homologar o motor fiscal de `NF-e` por emitente real, incluindo cancelamento, inutilizacao e `CC-e` com certificado/ambiente oficiais;
- endurecer seguranca com MFA, `CSRF`, `rate limiting`, segregacao fina por modulo e politicas de sessao;
- endurecer o deploy de producao alem do baseline atual, com `TLS`, backup/restore validados, rollback testado e operacao assistida.

## Validacao executada

Comandos validados no checkpoint atual:

```bash
dotnet build PackControl.sln -m:1 /nodeReuse:false /p:UseSharedCompilation=false
dotnet test PackControl.sln -m:1 /nodeReuse:false /p:UseSharedCompilation=false
cd frontend/packcontrol-web && npm run build
cd frontend/packcontrol-web && npm test -- --watch=false --browsers=ChromeHeadless
docker compose --env-file deploy/.env.example -f deploy/docker-compose.production.yml config
```

Smoke manual validado:
- `GET /health`
- `GET /health/live`
- `GET /health/ready`
- `POST /api/auth/login`
- `GET /api/dashboard/summary`
- `GET /api/customers`
- `GET /api/assets`
- `GET /api/carriers`
- `GET /api/products`
- `GET /api/registers/overview`
- `GET /api/production/overview`
- `GET /api/production/sectors/Montagem`
- `GET /api/production/sectors/Emborrachamento`
- `POST /api/orders`
- `POST /api/orders/{id}/approve`
- `POST /api/production/orders/{id}/split`
- `POST /api/production/orders/merge`
- `POST /api/finance/entries`
- `POST /api/finance/entries/{id}/boleto`
- `POST /api/finance/invoices/issue`
- `GET /api/fiscal/overview`
- `GET /api/fiscal/engine-diagnostic`
- `POST /api/fiscal/documents/prepare`
- `POST /api/fiscal/documents/cancel`
- `POST /api/fiscal/documents/correction-letter`
- `POST /api/fiscal/numbering/inutilize`
- `PUT /api/fiscal/company-profiles/{id}`
- `PUT /api/fiscal/operation-templates/{id}`
- `POST /api/fiscal/documents/issue`
- `GET /api/settings/overview`

Observacao:
- o backend agora roteia por adapter fiscal configurado no emitente: `mock-plugavel` para smoke local e `unimake.dfe` para diagnostico real de status do servico `NF-e`.
- o documento fiscal preparado passou a congelar emitente, destinatario, itens, totais, pagamento e transporte; a emissao nao monta mais a nota lendo pedido/cliente ao vivo.
- o endpoint `GET /api/fiscal/engine-diagnostic` consulta o autorizador real quando o emitente esta apontando para `unimake.dfe` e agora tambem acusa ausencia de material de certificado na configuracao.
- a trilha real `A1` ja monta `XML 55`, valida `IBGE/NCM/CFOP`, assina/transmite via adapter real e consulta recibo/protocolo; sem certificado configurado, a API barra a emissao com erro explicito.
- o bloco que ainda separa o sistema do go-live fiscal nao e mais `XML/A1`; agora e homologacao real por emitente, `A3` por agente local, `DANFE` oficial e validacao operacional em campo.
- a camada canonica agora fecha o ciclo operacional de eventos fiscais (`cancelamento`, `CC-e` e inutilizacao de faixa), com timeline e artefatos no ERP; `mock-plugavel` e `unimake.dfe` ja compartilham essa trilha, e o adapter real agora depende apenas de certificado/configuracao/homologacao do emitente para fechar a operacao junto ao SEFAZ.
- para homologacao local, a API agora aceita `appsettings.Local.json` e `appsettings.{Environment}.Local.json` como camada opcional de override para certificado e endpoints.
- o backend agora expõe `GET /health/live` e `GET /health/ready`; o readiness valida persistencia e acesso de escrita ao storage configurado.
- o repositorio agora inclui um baseline de deploy tecnico em `deploy/`, com compose, `Dockerfile` e proxy web para servir a SPA e encaminhar `/api` para a `API`.
- o emitente fiscal agora possui status de onboarding, checklist interno e bloqueios de emissao por readiness, incluindo o recorte inicial de rollout para Sao Paulo/SP.
- a implantacao fiscal por emitente agora esta detalhada em `docs/checklist-implantacao-fiscal-emitente.md` e `docs/runbook-implantacao-fiscal.md`.
