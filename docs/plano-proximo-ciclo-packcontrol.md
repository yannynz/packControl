# Plano do Proximo Ciclo - PackControl

Versao: 0.1
Data: 2026-04-04
Status: recorte pratico do que falta para sair do checkpoint de deploy tecnico e avancar para go-live vendavel

## 1. Premissas

- as estimativas abaixo assumem `1 desenvolvedor + apoio de IA`;
- o tempo esta em `dias uteis ideais`;
- quando houver dependencia externa, o tempo de codigo e separado do tempo de espera;
- a ordem abaixo segue o fechamento recomendado em `README.md` e `docs/status-go-live-packcontrol.md`;
- onde houver decisao de produto pendente, o item aparece separado da implementacao.

## 2. Leitura rapida

Resumo do estado atual:
- o sistema ja esta pronto para `deploy tecnico interno`;
- a persistencia ainda esta em snapshot unico `JSONB`;
- o login atual usa cookie, mas ainda sem `MFA`, `CSRF` e endurecimento final;
- o estimador aparece hoje como parametro/configuracao, mas o motor e o fluxo comercial ainda nao estao fechados;
- o fiscal `A1` ja existe na trilha tecnica, mas ainda sem homologacao real por emitente;
- o codigo ainda declara duas lacunas estruturais para go-live fiscal: `A3` e `DANFE` oficial.

## 3. Ordem recomendada com dificuldade e tempo

| Ordem | Item | Estado atual | Dificuldade | Tempo de implementacao | Principais dificuldades |
|---|---|---|---|---:|---|
| `1` | persistencia relacional com migrations | hoje o backend grava um unico snapshot `JSONB` em `public.app_state_snapshots` | `Muito alta` | `8 a 12d` | quebrar o `AppStateStore` por agregados, desenhar schema sem travar a operacao atual, migrar leitura/escrita gradualmente, criar migrations formais e ajustar testes |
| `2` | backup, restore e healthchecks ampliados | existe healthcheck basico de banco/storage, mas nao existe trilha real de restauracao | `Alta` | `3 a 5d` | definir estrategia de backup de banco + artefatos, provar restore fim a fim, tratar retention e acrescentar healthchecks aderentes ao novo modelo relacional |
| `3` | `MFA` TOTP e endurecimento de sessao | UI ja exibe indicador de `MFA`, mas o login atual ainda nao implementa challenge real | `Alta` | `4 a 6d` | modelar segredo, enrollment, recovery, enforce por perfil, fluxo de login em duas etapas e testes de regressao de autenticacao |
| `4` | `CSRF`, `rate limiting`, segredos fora de config plana e autorizacao fina | autenticacao por cookie existe, mas a protecao final de producao ainda nao | `Media/Alta` | `3 a 5d` | compatibilizar `CSRF` com SPA Angular, limitar login sem quebrar uso local, mover segredos para ambiente seguro e revisar permissoes por modulo |
| `5` | decisao de corte para estimador/orcamento | o produto pede isso, mas ainda e uma frente aberta e com impacto em prazo | `Media` | `0.5 a 1d` | decidir com clareza se entra no primeiro corte ou se sai conscientemente para nao contaminar escopo e cronograma |
| `6` | estimador deterministico e orcamento | hoje existem parametros exibidos em configuracoes, mas o motor e o workflow comercial ainda nao estao implementados | `Alta` | `8 a 12d` | transformar heuristica em regra deterministica, modelar margem/override sem perder auditoria, encaixar no pedido consolidado e validar coerencia com o negocio real |
| `7` | fiscal real `A1` por emitente | a trilha tecnica com `Unimake.DFe` ja monta, assina e transmite, mas falta homologacao real | `Alta` | `4 a 7d` de codigo + `3 a 10d` de dependencia externa | fechar emitente alvo, matriz fiscal, credenciamento, certificado valido, rejeicoes reais do autorizador e evidencias de homologacao |
| `8` | `DANFE` oficial | hoje a representacao do `DANFE` ainda esta em modo simplificado | `Media` | `3 a 5d` | integrar biblioteca oficial ou homologada, arquivar artefatos certos e garantir download/impressao sem quebrar o fluxo atual |
| `9` | `A3` operacional via agente local | existe modelagem e checkpoint parcial, mas o proprio codigo declara que o fluxo ainda nao foi concluido | `Muito alta` | `10 a 15d` | job remoto, heartbeat, discovery local, assinatura fora do servidor, `PIN`, variacao de driver/middleware e homologacao com hardware real |

## 4. Detalhamento item a item

### `1. Persistencia relacional com migrations`

Tempo sugerido:
- desenho do schema e estrategia de transicao: `2d`
- infraestrutura de persistencia e migrations: `2 a 3d`
- migracao dos modulos principais: `3 a 5d`
- ajuste de testes e smokes: `1 a 2d`

Dificuldades concretas:
- o projeto inteiro hoje gira em torno de `AppStateStore`, entao nao e uma troca localizada;
- nao ha `EF Core`, `DbContext` ou migrations prontas no codigo atual;
- varios servicos leem e gravam estado em memoria antes de persistir o snapshot, o que exige recorte por agregado.

### `2. Backup, restore e healthchecks ampliados`

Tempo sugerido:
- estrategia de backup de banco e storage: `1d`
- scripts/processos de restore: `1 a 2d`
- healthchecks e validacao operacional: `1 a 2d`

Dificuldades concretas:
- o restore precisa provar consistencia entre banco e artefatos de disco;
- hoje o healthcheck de banco valida apenas `select 1`, o que e insuficiente para operacao real;
- sem persistencia relacional pronta, parte desta frente fica acoplada ao item `1`.

### `3. MFA TOTP e endurecimento de sessao`

Tempo sugerido:
- modelo e segredo `TOTP`: `1 a 2d`
- fluxo backend de challenge e recovery: `1 a 2d`
- ajuste frontend de login: `1 a 2d`
- testes: `1d`

Dificuldades concretas:
- o login atual valida email/senha diretamente sobre o estado em memoria;
- o frontend nao possui hoje tela de challenge `MFA`;
- sera preciso decidir exatamente quais perfis entram em enforce no primeiro corte.

### `4. CSRF, rate limiting, segredos e autorizacao fina`

Tempo sugerido:
- `CSRF`: `1 a 2d`
- `rate limiting`: `0.5 a 1d`
- segredos fora de config plana: `1d`
- revisao de autorizacao por modulo: `1 a 2d`

Dificuldades concretas:
- `CSRF` em app com cookie precisa alinhar backend, proxy e frontend;
- `rate limiting` mal regulado pode atrapalhar ambiente de desenvolvimento e smoke;
- a autorizacao atual esta funcional, mas ainda nao esta claramente granular por modulo sensivel.

### `5. Decisao de corte para estimador/orcamento`

Tempo sugerido:
- revisao funcional e decisao: `0.5 a 1d`

Dificuldades concretas:
- este item mexe no recorte do MVP;
- se entrar, ele puxa interface, motor, regras de negocio e validacao comercial;
- se sair, precisa ficar explicitamente removido do criterio de go-live do primeiro cliente.

### `6. Estimador deterministico e orcamento`

Tempo sugerido:
- modelagem de parametros e motor: `3 a 4d`
- workflow comercial e historico: `2 a 3d`
- UI de estimativa e orcamento: `2 a 3d`
- testes e calibracao inicial: `1 a 2d`

Dificuldades concretas:
- hoje os parametros do estimador expostos em configuracoes sao essencialmente estaticos;
- falta calibracao real por etapa, gargalo e prazo;
- ha risco alto de parecer funcional na tela, mas incoerente para a operacao se nao houver regra real fechada.

### `7. Fiscal real A1 por emitente`

Tempo sugerido:
- fechar emitente, matriz e credenciamento: `0.5 a 1d` de trabalho interno + espera externa
- endurecimentos fiscais faltantes: `2 a 3d`
- rodada de homologacao e correcoes: `2 a 4d`

Dificuldades concretas:
- depende de contador, credenciamento, certificado e ambiente corretos;
- a trilha tecnica esta adiantada, mas o risco agora esta nas rejeicoes reais e no onboarding do emitente;
- parte do tempo nao e codigo: e validacao operacional e fiscal.

### `8. DANFE oficial`

Tempo sugerido:
- integracao de renderizacao: `1 a 2d`
- arquivamento, download e validacao: `1 a 2d`
- smoke com fluxo fiscal: `1d`

Dificuldades concretas:
- a representacao atual nao e a saida oficial esperada para campo;
- a escolha da biblioteca impacta licenca, empacotamento e manutencao;
- precisa encaixar no fluxo atual de arquivos fiscais sem gerar duplicidade de artefato.

### `9. A3 operacional via agente local`

Tempo sugerido:
- agente conhecido + heartbeat: `2 a 3d`
- fila/job fiscal: `2 a 3d`
- discovery local e assinatura com `PIN`: `3 a 4d`
- homologacao com hardware real: `3 a 5d`

Dificuldades concretas:
- este e o item com maior variabilidade tecnica do projeto;
- depende de driver, middleware e hardware do certificado;
- exige desenho hibrido confiavel entre ERP central e maquina local.

## 5. Caminhos praticos

### Caminho minimo para avancar mais rapido

Sequencia:
1. item `1`
2. item `2`
3. itens `3` e `4`
4. item `5`
5. item `7`
6. item `8`

Tempo estimado:
- `22 a 36d` de execucao tecnica
- mais `3 a 10d` de dependencia externa para homologacao fiscal real

Observacao:
- este caminho assume `A1` como meio principal e `A3` postergado;
- tambem assume que o estimador/orcamento pode ficar fora do primeiro corte ou entrar depois.

### Caminho completo incluindo estimador

Sequencia:
1. itens `1` e `2`
2. itens `3` e `4`
3. item `6`
4. item `7`
5. item `8`

Tempo estimado:
- `30 a 48d` de execucao tecnica
- mais `3 a 10d` de dependencia externa para homologacao fiscal real

### Caminho completo incluindo `A3`

Tempo adicional sobre os caminhos acima:
- `10 a 15d` de execucao tecnica
- mais variacao operacional de hardware e homologacao local

## 6. Proxima tarefa recomendada

Se o objetivo agora for sair do checkpoint com maior impacto e menor risco, a proxima implementacao deve ser:

1. iniciar a persistencia relacional;
2. logo em seguida fechar backup/restore;
3. na mesma janela, preparar `MFA` e hardening minimo;
4. so depois abrir a rodada fiscal real por emitente.

Leitura pratica:
- a maior dificuldade tecnica atual nao esta mais no shell web nem no fiscal canonico;
- ela esta na base estrutural de persistencia e na seguranca de producao.
