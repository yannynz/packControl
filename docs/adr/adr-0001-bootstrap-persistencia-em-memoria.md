# ADR-0001 - Transicao da Persistencia Inicial para PostgreSQL Snapshot

Data: 2026-03-26  
Status: Atualizado

## Contexto

O bootstrap do PackControl nasceu com estado transacional apenas em memoria para tirar o produto do estado puramente documental. Essa decisao cumpriu o papel inicial, mas ficou insuficiente depois que entraram no repositorio:
- ativos tecnicos por cliente;
- regras comerciais por produto/cliente;
- parser real de `PDF` e analise real de `DXF`;
- `split/merge` auditavel de OPs;
- financeiro com boleto e `NF-e` pronta para adaptador;
- smoke com reinicio da API precisando manter login, pedidos, producao e fiscal.

Ao mesmo tempo, ainda nao existe modelagem relacional final do MVP com migrations formais por agregado.

## Decisao

A API passa a operar com persistencia configuravel:
- `InMemory` continua disponivel como fallback de desenvolvimento rapido, sem dependencia externa;
- `PostgreSQL` passa a ser a opcao de persistencia duravel do core transacional;
- a persistencia atual em `PostgreSQL` grava um snapshot `JSONB` do `AppStateStore` na tabela `public.app_state_snapshots`, indexada por `snapshot_key`;
- o contrato HTTP permanece o mesmo;
- `FileSystemStorage` continua salvando anexos, artefatos tecnicos e artefatos fiscais em `backend/storage`.

Em termos praticos:
- o modelo operacional em memoria continua sendo o `AppStateStore`;
- `IAppStatePersistence` carrega o snapshot no bootstrap e salva o snapshot a cada mutacao relevante;
- a configuracao fica em `StatePersistence:Provider`, `ConnectionString`, `Schema` e `SnapshotKey`.

## Consequencias

Positivas:
- reiniciar a API nao perde mais usuarios seedados, clientes, pedidos, producao, financeiro e fiscal quando `PostgreSQL` esta habilitado;
- o produto ganhou durabilidade sem quebrar rotas, DTOs ou a navegacao do frontend;
- a troca futura para persistencia relacional por agregado continua encapsulada na infraestrutura.

Negativas:
- a persistencia atual ainda nao e relacional; ela salva o estado como snapshot unico;
- ainda nao existem migrations formais, `healthcheck` especifico de banco nem estrategia completa de backup/restore;
- o crescimento do snapshot precisa ser monitorado conforme o dominio aumentar;
- anexos e artefatos continuam em disco local, o que exige volume persistente ou storage externo no deploy.

## Proximo passo esperado

Evoluir a persistencia atual para:
- modelo relacional por agregado em `PostgreSQL`;
- migrations formais e `healthcheck` de banco;
- politica de backup/restore;
- storage persistente para anexos, `XML`, `DANFE` e derivados tecnicos.
