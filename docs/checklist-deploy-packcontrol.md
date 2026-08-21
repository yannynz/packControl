# Checklist de Deploy - PackControl

Versao: 0.1  
Data: 2026-03-28  
Status: Checklist tecnico minimo para subida do PackControl fora do ambiente de desenvolvimento

Documento complementar:
- `docs/runbook-deploy-packcontrol.md`
- `docs/status-go-live-packcontrol.md`

## 1. Preparacao

- [ ] `docker` e `docker compose` disponiveis no host.
- [ ] arquivo `deploy/.env` criado a partir de `deploy/.env.example`.
- [ ] porta externa definida.
- [ ] origem web definida para `Cors`.
- [ ] credenciais do `PostgreSQL` revisadas.
- [ ] volume persistente para storage previsto.

## 2. Segredos e fiscal

- [ ] segredos nao versionados no repositorio.
- [ ] `PACKCONTROL_FISCAL_ALLOW_REAL_EMISSION=false` se nao houver homologacao fiscal na rodada.
- [ ] caminho/senha do certificado preenchidos apenas se houver emitente real habilitado.
- [ ] emitente e credenciamento conferidos se o deploy incluir fiscal real.

## 3. Subida

- [ ] `docker compose --env-file deploy/.env -f deploy/docker-compose.production.yml up --build -d` executado.
- [ ] containers `postgres`, `api` e `web` em estado saudavel.
- [ ] `GET /health/live` retornando sucesso.
- [ ] `GET /health/ready` retornando sucesso.

## 4. Smoke funcional

- [ ] login funcionando.
- [ ] SPA abrindo pela porta publicada.
- [ ] `dashboard` carregando.
- [ ] clientes carregando.
- [ ] producao carregando.
- [ ] financeiro carregando.
- [ ] fiscal overview carregando.

## 5. Operacao

- [ ] logs dos containers acessiveis.
- [ ] caminho de rollback conhecido.
- [ ] volumes identificados.
- [ ] backup/restore tratados para a infraestrutura alvo.

## 6. Bloqueios de go-live vendavel

Nao chamar de producao vendavel se algum item abaixo continuar aberto:
- [ ] persistencia relacional com migrations formais.
- [ ] hardening final de seguranca.
- [ ] homologacao fiscal real do emitente.
- [ ] `DANFE` oficial.
- [ ] restore validado no ambiente alvo.
