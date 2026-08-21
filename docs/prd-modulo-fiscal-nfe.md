# PRD - Modulo Fiscal NF-e

Versao: 0.3  
Data: 2026-03-27  
Status: PRD de producao para emissao real de `NF-e` junto ao SEFAZ

Documento complementar:
- `docs/backlog-modulo-fiscal-nfe.md`

## 0.1 Estado atual x alvo deste PRD

Estado validado em `2026-03-27`:
- o ERP ja possui camada fiscal canonica com `overview`, `prepare`, `issue`, cadastro de empresa emissora e templates de operacao;
- o financeiro ja consegue emitir pela camada fiscal nova;
- a configuracao ja exibe agentes `A3`, certificado e adaptador configurado;
- a trilha `A1` ja monta `XML 55`, valida `IBGE/NCM/CFOP`, assina/transmite e consulta recibo/protocolo via `Unimake.DFe`;
- a operacao atual ainda nao e apta para producao fiscal, porque faltam certificado real do emitente, homologacao junto ao SEFAZ, eventos fiscais completos, `A3` operacional e `DANFE` oficial.

Este documento nao descreve o estado atual do codigo como suficiente. Ele descreve o alvo obrigatorio de produto para o PackControl operar faturamento real em producao.

## 0.2 Diretriz obrigatoria

Para fins deste PRD:
- nao existe go-live fiscal com `mock`, `stub` ou emissor paralelo manual;
- o modulo precisa emitir de fato junto ao SEFAZ, retornar protocolo real e arquivar artefatos oficiais;
- `A1` e `A3` sao requisitos de produto, com `A3` operado por agente local;
- no primeiro rollout, cada emitente entra com um unico meio principal de emissao, `A1` ou `A3`, embora o produto suporte ambos;
- o recorte geografico inicial do rollout fiscal fica restrito a Sao Paulo/SP, Brasil;
- `cancelamento`, `inutilizacao`, `CC-e`, consulta de protocolo, rejeicoes e reprocesso seguro fazem parte do escopo minimo de producao;
- o go-live fiscal so ocorre com credenciamento, storage persistente, observabilidade, trilha auditavel e runbook operacional fechados.

## 1. Visao do modulo

Construir um modulo fiscal de `NF-e` para o PackControl que:
- atenda emissao com certificado `A1` e `A3`;
- opere em `Homologacao` e `Producao`;
- gere, transmita, consulte e arquive `XML`, protocolo e `DANFE`;
- suporte cancelamento, inutilizacao e `CC-e`;
- seja plugavel para motores fiscais de terceiros ou para um motor proprio;
- preserve o dominio comercial/produtivo do ERP desacoplado das particularidades da SEFAZ.

O modulo nao deve ser tratado como "nota interna". Ele e um nucleo fiscal de producao, desenhado para emitir de fato junto ao SEFAZ e sustentar a operacao oficial da empresa.

## 2. Problema que o modulo resolve

Sem um modulo fiscal proprio e orquestrado pelo ERP:
- o faturamento fica quebrado entre pedido, financeiro e emissor externo;
- `A1` e `A3` viram excecoes operacionais em vez de fluxos padronizados;
- XML, protocolo, `DANFE`, eventos e rejeicoes se perdem fora da trilha do pedido;
- mudancas frequentes de `Nota Tecnica`, leiaute e reforma tributaria geram retrabalho invisivel;
- a equipe fica dependente de emissao manual, dupla digitacao e conciliacao posterior.

## 3. Contexto validado em pesquisa

Pesquisa feita em `2026-03-26`, com consulta a fontes oficiais e documentacao de bibliotecas/provedores.

Fatos confirmados:
- o Portal Nacional da `NF-e` segue publicando alteracoes frequentes de leiaute e regras; como referencia atual, a `NT 2025.001 v.1.02` foi publicada em `02/09/2025` e a `NT 2025.002 v.1.34` foi publicada em `04/12/2025`;
- o portal tambem ja publicou material da `NF-e ABI` em `24/11/2025`, indicando transicao e ampliacao de escopo regulatorio;
- a relacao de servicos web do Portal da `NF-e` continua expondo servicos como `NFeAutorizacao 4.00`, `NFeRetAutorizacao 4.00`, `RecepcaoEvento 4.00`, `NfeConsultaProtocolo 4.00`, `NfeStatusServico 4.00` e `NfeInutilizacao 4.00`;
- certificados `A1` e `A3` sao aceitos no ecossistema ICP-Brasil para fins amplos, inclusive documentos fiscais, desde que respeitada a politica da AC emitente;
- certificado `A3` continua dependente de hardware criptografico, normalmente `token` ou `cartao`, conectado a uma maquina no momento do uso.

Leitura pratica:
- o modulo precisa nascer adaptavel, porque a regra fiscal muda;
- `A3` nao pode ser tratado como mero upload de arquivo, pois exige presenca de hardware;
- a camada de dominio do ERP nao pode conhecer detalhes de `SOAP`, certificado, `schema`, rejeicao ou contingencia.

## 4. Objetivos do produto

- emitir `NF-e` a partir do pedido e/ou titulo financeiro sem redigitacao;
- suportar `A1` e `A3` desde o desenho inicial;
- reduzir falha humana no preenchimento fiscal;
- manter trilha auditavel ponta a ponta;
- transformar rejeicoes e eventos fiscais em dados operacionais do ERP;
- permitir troca de biblioteca ou provedor sem reescrever o ERP;
- suportar evolucao da reforma tributaria sem contaminar o resto do produto.

## 5. Metas de negocio

- reduzir o tempo entre aprovacao comercial e emissao fiscal;
- reduzir retrabalho de faturamento;
- reduzir dependencia de emissor paralelo;
- aumentar rastreabilidade fiscal por pedido, cliente e transportadora;
- permitir rollout progressivo por empresa/CNPJ;
- evitar lock-in irreversivel em um unico fornecedor fiscal.

## 6. Perfis de usuario

### 6.1 Financeiro/fiscal

Responsavel por preparar, emitir, cancelar e acompanhar a `NF-e`, tratando rejeicoes e eventos.

### 6.2 Comercial

Consulta situacao fiscal do pedido, mas nao altera dados tributarios sensiveis.

### 6.3 Administrador

Configura empresa emissora, certificado, serie, numeracao, templates e integracoes.

### 6.4 Suporte/implantacao

Auxilia configuracao de `A1`, `A3`, credenciamento, homologacao e diagnostico.

## 7. Principios do modulo

- fiscal desacoplado do dominio comercial;
- `snapshot` fiscal obrigatorio no momento da emissao;
- `A3` nunca expoe chave privada ao servidor central;
- toda transmissao e evento gera trilha auditavel;
- toda operacao relevante tem reprocessamento controlado;
- toda dependencia externa entra por `adapter`;
- o modulo deve funcionar por empresa/CNPJ e por estabelecimento;
- homologacao e producao devem coexistir sem gambiarras de configuracao.

## 8. Escopo do MVP fiscal para producao real

Inclui:
- cadastro de empresa emissora;
- configuracao de ambiente `Homologacao` e `Producao`;
- configuracao de certificado `A1` e `A3`;
- templates de operacao fiscal por contexto;
- congelamento do `snapshot` fiscal do pedido;
- emissao de `NF-e modelo 55`;
- assinatura digital;
- transmissao;
- consulta de recibo/protocolo;
- autorizacao de uso;
- cancelamento;
- inutilizacao;
- `CC-e`;
- geracao e arquivamento de `XML` e `DANFE`;
- trilha de eventos e tentativas;
- integracao com pedido, financeiro, logistica e auditoria;
- fila de reprocessamento e tratamento de rejeicoes;
- checklist de credenciamento por empresa/CNPJ;
- diagnostico operacional de certificado, autorizador e storage;
- runbook de emissao, reprocesso, contingencia e suporte de campo;
- gates de entrada em producao por emitente.

## 9. Fora do escopo deste modulo

Nao entram nesta primeira entrega detalhada:
- `NFS-e`;
- `MDF-e`;
- `CT-e`;
- apuracao fiscal;
- `SPED`;
- contabilizacao automatica;
- motor tributario universal para qualquer operacao do Brasil;
- substituicao completa do trabalho do contador.

## 10. Requisitos funcionais

### 10.1 Empresa emissora

O sistema deve permitir:
- cadastrar `CNPJ`, razao social, nome fantasia, IE, CNAE e regime;
- manter serie, proximo numero, ambiente e autorizador;
- parametrizar logo, observacoes de `DANFE` e contatos;
- vincular empresa emissora ao pedido.

### 10.2 Certificados

O sistema deve permitir:
- cadastrar perfil `A1` com `PFX` e senha;
- cadastrar perfil `A3` com metadados, numero de serie e vinculacao ao agente local;
- marcar certificado principal e certificado de contingencia;
- controlar validade, expiracao e alerta;
- testar assinatura e comunicacao antes de liberar emissao.

### 10.3 Templates fiscais

O sistema deve permitir:
- definir `CFOP`, natureza de operacao, finalidade, consumidor final, presenca, indicador de frete e regras de pagamento;
- manter templates por produto, cliente, UF, transportadora e empresa;
- sobrescrever template no documento, mantendo trilha.

### 10.4 Snapshot fiscal

No momento de emissao, o sistema deve congelar:
- emitente;
- destinatario;
- itens;
- `NCM`, `CFOP`, unidade, quantidade e valor;
- impostos calculados/informados;
- frete, seguro, desconto e totais;
- transportadora;
- forma de pagamento;
- endereco de entrega/retirada quando aplicavel.

### 10.5 Emissao

O modulo deve permitir:
- emitir a partir de pedido;
- emitir a partir de titulo financeiro;
- emitir manualmente com base em pedido ja consolidado;
- validar precondicoes antes de assinar;
- gerar numero/chave de acesso;
- transmitir e atualizar status;
- tratar resposta sincrona e assincrona.

### 10.6 Eventos posteriores

O modulo deve permitir:
- cancelar dentro da janela legal;
- inutilizar faixa de numeracao;
- emitir `CC-e`;
- consultar protocolo e status;
- registrar justificativa, usuario e horario do evento.

### 10.7 Rejeicoes e reprocesso

O sistema deve:
- classificar rejeicoes em `dados`, `schema`, `certificado`, `SEFAZ`, `infra`, `contingencia`;
- sugerir proximo passo;
- permitir correcao e reenvio sem perder historico;
- manter o documento original imutavel e registrar nova tentativa como evento.

### 10.8 Arquivo fiscal

O modulo deve:
- arquivar `XML` enviado;
- arquivar retorno da SEFAZ;
- arquivar `XML` autorizado;
- arquivar `DANFE`;
- manter hash, tamanho, data e origem do arquivo;
- permitir download controlado por perfil.

### 10.9 Integracao com o ERP

O modulo deve:
- refletir status fiscal no pedido;
- refletir status fiscal no financeiro;
- alimentar logistica com dados de faturamento;
- expor eventos para auditoria;
- impedir faturamento duplicado por acidente;
- permitir consulta rapida por numero, chave, pedido, cliente e protocolo.

## 11. Fluxos principais

### 11.1 Fluxo A1

1. Usuario seleciona pedido apto a faturar.
2. Sistema monta `snapshot` fiscal.
3. Sistema valida dados obrigatorios.
4. Sistema assina com certificado `A1`.
5. Sistema transmite para a SEFAZ/autorizador.
6. Sistema consulta retorno quando necessario.
7. Sistema arquiva `XML`, protocolo e `DANFE`.
8. Pedido e financeiro recebem o status fiscal.

### 11.2 Fluxo A3

1. Usuario seleciona pedido apto a faturar.
2. Sistema monta `snapshot` fiscal.
3. Sistema envia requisicao de assinatura/transmissao ao agente local `A3`.
4. Operador do ambiente local confirma `PIN` quando necessario.
5. Agente local assina/transmite usando o certificado fisico.
6. Agente devolve protocolo, `XML` autorizado e artefatos.
7. ERP arquiva e atualiza status do documento.

### 11.3 Fluxo de cancelamento

1. Usuario autorizado seleciona documento autorizado.
2. Sistema valida janela e motivo.
3. Sistema transmite evento de cancelamento.
4. Sistema atualiza status fiscal e financeiro.
5. Sistema arquiva retorno e reemissao de `DANFE` quando aplicavel.

## 12. Requisitos nao funcionais

- alta rastreabilidade;
- tolerancia a indisponibilidade temporaria da SEFAZ;
- reprocessamento idempotente;
- segregacao por perfil;
- criptografia de segredo em repouso;
- logs estruturados;
- compatibilidade com rollout por cliente/empresa;
- baixa acoplacao com bibliotecas fiscais;
- suporte a `Windows` no agente `A3`;
- documentacao operacional obrigatoria.

## 13. Requisitos especificos de `A1/A3`

### 13.1 A1

- pode ser operado de forma centralizada;
- exige armazenamento seguro do `PFX`;
- deve suportar rotacao sem indisponibilidade longa;
- deve permitir teste de validade e senha.

### 13.2 A3

- nao deve exigir que a chave privada saia do hardware;
- deve funcionar com `token`, `cartao` e cenarios equivalentes homologados pela `ICP-Brasil`;
- deve tolerar solicitacao de `PIN`;
- deve detectar ausencia de dispositivo/driver;
- deve ter modo de diagnostico assistido;
- deve operar por agente local ou bridge equivalente.

## 14. UX operacional

O modulo deve expor:
- status claro: `rascunho`, `pronta`, `assinando`, `enviada`, `autorizada`, `rejeitada`, `cancelada`, `inutilizada`;
- linha do tempo do documento;
- bloco de rejeicoes legivel;
- evidencias do certificado usado;
- download rapido de `XML` e `DANFE`;
- separacao entre erro de dado e erro de infraestrutura;
- alertas de certificado proximo do vencimento.

## 15. Dependencias externas

- credenciamento do contribuinte na SEFAZ/UF competente;
- certificado digital valido;
- parametrizacao fiscal pelo contador;
- biblioteca ou provedor de transmissao;
- storage persistente para artefatos;
- ambientes de homologacao e producao acessiveis;
- acompanhamento das `Notas Tecnicas` e `Informes Tecnicos`.

## 15.1 Gates obrigatorios de go-live

O modulo so pode entrar em producao por empresa emitente quando todos os itens abaixo estiverem fechados:
- emitente credenciado e validado no autorizador competente;
- certificado `A1` ou `A3` valido, testado e com plano de renovacao;
- meio principal do emitente definido para o go-live atual;
- serie e numeracao segregadas por ambiente;
- matriz fiscal inicial entregue e validada pelo contador;
- emitente dentro do recorte geografico inicial aprovado para rollout;
- emissao homologada, cancelamento, inutilizacao e `CC-e` testados no ambiente de homologacao;
- storage persistente de `XML` e `DANFE` validado com restore;
- monitoracao e alertas ativos;
- runbook de suporte e rollback operacional aprovados.

## 16. Decisao de producao e estimativa

Decisao aprovada para o produto:
- `Fiscal Core` proprio no PackControl;
- integracao real com SEFAZ por `adapter` homologavel;
- implementacao inicial sobre biblioteca `.NET` madura, com `Unimake.DFe` como baseline tecnica principal;
- `A3` obrigatoriamente operado por agente local;
- provedor externo aceito apenas como acelerador tatico se nao contaminar dominio, storage, numeracao e trilha fiscal do ERP.

Justificativa:
- o produto precisa dominar fluxo, artefato, numeracao, reprocesso e auditoria;
- `A3` inviabiliza SaaS puro como arquitetura unica;
- depender de emissor externo como caminho principal enfraquece pedido, financeiro e operacao de suporte.

### 16.1 Cenario A - Provedor SaaS `A1` apenas

Viabilidade:
- alta para emissao rapida;
- baixa para o requisito de `A3`.

Tempo estimado:
- `2 a 4 semanas` para primeira integracao segura.

Custo/observacao:
- nao e o caminho aprovado para o go-live do PackControl;
- serve apenas como referencia de mercado para comparacao.

### 16.2 Cenario B - Provedor/stack hibrida com bridge `A3`

Viabilidade:
- alta para ir a producao mais rapido sem reescrever toda a mensageria;
- media para manter independencia de fornecedor.

Tempo estimado:
- `4 a 8 semanas` para emissao, cancelamento, arquivo fiscal e fluxo `A3` assistido.

Custo/observacao:
- pode ser usado como fallback de aceleracao, mas nao como arquitetura alvo do produto;
- toda adocao precisa preservar dominio, storage e trilha fiscal dentro do ERP.

### 16.3 Cenario C - Motor proprio com biblioteca fiscal

Viabilidade:
- alta tecnicamente;
- alta em controle;
- media/alta em custo de manutencao.

Tempo estimado:
- `10 a 16 semanas` para MVP robusto com `A1`, `A3` via agente local, cancelamento, inutilizacao, `CC-e`, arquivo fiscal, monitoramento e homologacao inicial;
- `12 a 20 semanas` se incluir cobertura forte de rejeicoes, contingencia, reprocesso e rollout multiempresa mais seguro.

Custo/observacao:
- este e o caminho-alvo aprovado para a arquitetura de producao;
- custo interno de manutencao sobe e permanece continuo por conta de `NT`, `RTC`, `schema`, SEFAZ e particularidades operacionais.

## 17. Bibliotecas e opcoes avaliadas

### 17.1 `Unimake.DFe`

Pontos fortes:
- `MIT`;
- pacote `NuGet` atualizado em `2026-01-29` na versao `20260129.1047.45`;
- documentacao e exemplos para multiplas linguagens;
- material publico indicando suporte a `A3`;
- pacote separado para `DANFE`.

Pontos de atencao:
- ainda exige nosso desenho de dominio, storage, trilha, retries e UX;
- A3 continua dependendo de agente/maquina com hardware quando nao for `A1`.

### 17.2 Ecossistema `Zeus`

Pontos fortes:
- comunidade ampla no ecossistema `.NET`;
- releases publicas recentes em `2026-03-03`;
- pacote `DANFE HTML` recente e docs historicas para `A3` via store do Windows.

Pontos de atencao:
- requer diligencia de manutencao/licenciamento e continuidade do fork/ecossistema escolhido;
- assim como a `Unimake`, nao resolve sozinho `A3` remoto, storage, observabilidade e fluxo de negocio.

### 17.3 Provedores SaaS

Pontos fortes:
- menor esforco com `XML`, `schema`, webservice, contingencia e atualizacao regulatoria;
- integracao mais rapida;
- boa opcao para primeira ida a mercado.

Pontos de atencao:
- `A3` em nuvem pura nem sempre e suportado;
- lock-in comercial;
- custo recorrente por volume;
- menor controle fino de fluxo e observabilidade interna.

## 18. Recomendacao para o PackControl

Recomendacao fechada para producao:
- construir e operar o `Fiscal Core` dentro do PackControl;
- executar emissao real por `adapter` homologavel ligado ao SEFAZ;
- usar `Unimake.DFe` como baseline tecnica principal da primeira integracao real;
- manter `Zeus` apenas como opcao secundaria de contingencia tecnica;
- tratar `A3` como fluxo nativo do produto por agente local, nao como excecao manual;
- nao liberar go-live com dependencia de emissor paralelo fora do ERP.

## 19. Criterios de aceite do modulo

Para considerar o modulo pronto para producao:
- emitir `NF-e` real em homologacao e em producao com o autorizador da empresa;
- emitir com `A1` e com `A3` via agente local, conforme o certificado do emitente;
- consultar retorno, recibo e protocolo reais;
- cancelar, inutilizar faixa e emitir `CC-e` em integracao real;
- arquivar `XML` enviado, retorno, `XML` autorizado e `DANFE` em storage persistente;
- refletir status fiscal no pedido e no financeiro sem conciliacao manual paralela;
- gerar trilha auditavel completa por documento, evento e tentativa;
- suportar reprocesso seguro de erro transitorio sem duplicar documento;
- possuir checklist de credenciamento, monitoracao, alertas e runbook de operacao;
- executar smoke de producao por emitente antes do go-live definitivo.

## 20. Riscos principais

- mudanca frequente de leiaute e `Nota Tecnica`;
- dependencia de configuracao correta do contador;
- indisponibilidade/intermitencia de SEFAZ;
- complexidade de driver e middleware do `A3`;
- ambiente do cliente sem permissao/estabilidade para agente local;
- lock-in de provedor se o contrato nao for bem desenhado.

## 21. Referencias externas consultadas em 2026-03-26

- Portal da NF-e: https://www.nfe.fazenda.gov.br/portal/principal.aspx
- Relacao de servicos web da NF-e: https://www.nfe.fazenda.gov.br/portal/webServices.aspx?tipoConteudo=OUC/YVNWZfo=
- Lista de Notas Tecnicas do Portal NF-e: https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=04BIflQt1aY=
- MOC NF-e ABI - historico do portal: https://hom.nfe.fazenda.gov.br/PORTAL/listaHistorico.aspx?tipoConteudo=Ef+Y1blZDbU=
- Gov.br - certificado digital `A1/A3`: https://www.gov.br/pt-br/servicos/obter-certificacao-digital
- ITI - FAQ certificacao digital: https://www.gov.br/iti/pt-br/acesso-a-informacao/perguntas-frequentes/certificacao-digital
- Unimake.DFe no GitHub: https://github.com/Unimake/DFe
- Unimake.DFe no NuGet: https://www.nuget.org/packages/Unimake.DFe/
- Unimake - videos/documentacao sobre `A3`: https://wiki.unimake.com.br/index.php/Manuais:Unimake.DFe/VideosCsharp
- Zeus DFe.NET no GitHub: https://github.com/ZeusAutomacao/DFe.NET
- Zeus DFe.NET releases: https://github.com/ZeusAutomacao/DFe.NET/releases
- Zeus - orientacao publica de uso com `A3`: https://github.com/ZeusAutomacao/DFe.NET/issues/114
- Focus NFe - planos e limites de certificado: https://2025.focusnfe.com.br/precos/
- PlugNotas NFe: https://plugnotas.com.br/nfe/
- TecnoSpeed - informacoes gerais sobre `A1/A3`: https://atendimento.tecnospeed.com.br/hc/pt-br/articles/4932353103127-Certificado-Digital-Informa%C3%A7%C3%B5es-Gerais
- TecnoSpeed - modulo local `A3` para Manager SaaS: https://atendimento.tecnospeed.com.br/hc/pt-br/articles/360011945834-Instalando-o-m%C3%B3dulo-de-certificados-A3-para-uso-no-Manager-SaaS
- TecnoSpeed - eventos RTC para NF-e: https://atendimento.tecnospeed.com.br/hc/pt-br/articles/36284032224663-Eventos-da-Reforma-Tribut%C3%A1ria-do-Consumo-para-a-NF-e
