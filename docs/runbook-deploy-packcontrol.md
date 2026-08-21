# Runbook de Deploy - PackControl

Versao: 0.1  
Data: 2026-03-28  
Status: Runbook tecnico de referencia para subir o PackControl fora do ambiente de desenvolvimento

Documento complementar:
- `docs/checklist-deploy-packcontrol.md`
- `docs/status-go-live-packcontrol.md`
- `README.md`

## 1. Objetivo

Padronizar o deploy tecnico minimo do PackControl usando os artefatos versionados no repositorio:
- `deploy/backend/Dockerfile`
- `deploy/frontend/Dockerfile`
- `deploy/nginx/packcontrol.conf`
- `deploy/docker-compose.production.yml`
- `deploy/.env.example`

Este runbook fecha a etapa de deploy tecnico. Ele nao substitui a homologacao fiscal por emitente.

## 2. Topologia de referencia

O baseline atual sobe:
- `web`: `nginx` servindo a SPA Angular e fazendo proxy para a `API`;
- `api`: `ASP.NET Core` com `ASPNETCORE_ENVIRONMENT=Production`;
- `postgres`: persistencia por snapshot em `JSONB`.

Persistencia de arquivos:
- a `API` grava anexos, `XML` e artefatos em volume persistente montado em `/var/lib/packcontrol/storage`.

## 3. Pre-requisitos

Antes de subir:
- `docker` + `docker compose` disponiveis;
- porta HTTP de exposicao definida;
- credenciais de `PostgreSQL` definidas;
- dominio/TLS definidos, se o deploy for externo;
- origem web final definida para `Cors__AllowedOrigins__0`;
- certificado fiscal apenas se a homologacao real fizer parte da rodada.

## 4. Configuracao

1. Copiar o arquivo de exemplo:

```bash
cp deploy/.env.example deploy/.env
```

2. Ajustar pelo menos:
- `PACKCONTROL_WEB_PORT`
- `PACKCONTROL_WEB_ORIGIN`
- `PACKCONTROL_DB_NAME`
- `PACKCONTROL_DB_USER`
- `PACKCONTROL_DB_PASSWORD`

3. Se a rodada incluir homologacao fiscal `A1`, ajustar tambem:
- `PACKCONTROL_FISCAL_ALLOW_REAL_EMISSION`
- `PACKCONTROL_FISCAL_CERTIFICATE_PATH`
- `PACKCONTROL_FISCAL_CERTIFICATE_PASSWORD`

Observacao:
- certificado fiscal real continua opcional para o deploy tecnico base;
- o go-live fiscal depende de credenciamento e certificado do emitente.

## 5. Subida do ambiente

```bash
docker compose --env-file deploy/.env -f deploy/docker-compose.production.yml up --build -d
```

Comando util para logs:

```bash
docker compose --env-file deploy/.env -f deploy/docker-compose.production.yml logs -f
```

## 6. Validacao minima

Executar apos a subida:

1. Verificar containers:

```bash
docker compose --env-file deploy/.env -f deploy/docker-compose.production.yml ps
```

2. Validar liveness:

```bash
curl http://localhost:${PACKCONTROL_WEB_PORT}/health/live
```

3. Validar readiness:

```bash
curl http://localhost:${PACKCONTROL_WEB_PORT}/health/ready
```

4. Abrir a SPA no browser e validar:
- login;
- `dashboard`;
- listagem de clientes;
- listagem de producao;
- tela financeira;
- `GET /api/fiscal/overview`.

## 7. Volumes e dados

Volumes usados pelo compose:
- `packcontrol-postgres`
- `packcontrol-storage`

Conteudo esperado:
- `PostgreSQL` no volume do banco;
- anexos, `XML`, `DANFE` e derivados no volume da `API`.

## 8. Rollback tecnico

Se a rodada falhar:

1. Congelar o acesso dos usuarios.
2. Coletar logs dos containers.
3. Validar o estado do `PostgreSQL`.
4. Derrubar o stack:

```bash
docker compose --env-file deploy/.env -f deploy/docker-compose.production.yml down
```

5. Restaurar banco e storage somente se houver procedimento validado para a infraestrutura alvo.

Observacao:
- este runbook nao autoriza rollback fiscal de documento ja transmitido.

## 9. Pendencias apos este runbook

Mesmo com o deploy tecnico validado, continuam fora do escopo deste documento:
- migrations relacionais formais;
- `MFA`, `CSRF` e `rate limiting`;
- homologacao fiscal real por emitente;
- `DANFE` oficial;
- restore periodico automatizado e observabilidade completa.
