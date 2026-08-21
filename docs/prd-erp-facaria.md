# PRD - ERP para Facarias

Versao: 0.3
Data: 2026-03-26
Status: Rascunho consolidado com checkpoint de implementacao

Documento complementar do modulo fiscal:
- `docs/prd-modulo-fiscal-nfe.md`

## 1. Visao do produto

Construir um ERP SaaS especializado em facarias que atenda o ciclo completo do negocio: cadastro comercial, orcamento, engenharia, producao sob encomenda, manutencao/reforma de ferramentas existentes, logistica, financeiro, fiscal `NF-e` plugavel, patrimonio e operacao multi-CNPJ.

Este produto deve ser tratado como um sistema completamente novo. Solucoes anteriores podem servir como referencia tecnica, especialmente para leitura de DXF e medicao da complexidade tecnica de faca grafica, mas o novo sistema nao deve herdar obrigatoriamente o desenho funcional, a modelagem de dados ou as decisoes de arquitetura do legado.

O sistema deve refletir a realidade da facaria:

- o cliente pode comprar uma faca completa, apenas parte dela, apenas um acessorio, apenas um servico ou uma adaptacao;
- o pedido pode nascer sem arquivo e ser completado depois;
- um pedido pode reaproveitar um pedido/ativo antigo com pequenas alteracoes;
- a producao pode sair unificada, separada ou ser reorganizada durante a execucao;
- o sistema deve medir tempo e custo em detalhe para sugerir preco, margem e gargalos;
- o sistema deve separar complexidade tecnica do desenho, tempo operacional por item e impacto da lotacao da fabrica;
- operadores de producao nao devem ver custo nem preco.

## 2. Problema que o produto resolve

ERPs genericos nao cobrem bem a operacao de facarias porque:

- tratam o produto como item fixo, quando na pratica o cliente compra escopos variaveis;
- nao lidam bem com faca nova, reforma, troca parcial, adaptacao e manutencao no mesmo fluxo;
- nao suportam bem split e merge de OPs sem perder organizacao interna;
- nao modelam destacadores, emborrachamento, pertinax, papel calibrado e poliester como itens com tempo e custo proprios;
- nao trazem visao detalhada de tempo por etapa para localizar gargalos;
- costumam cobrar multiempresa como licencas integrais separadas, mesmo quando a operacao e compartilhada.

## 3. Objetivos do produto

- Centralizar a operacao de ponta a ponta em um unico sistema.
- Permitir orcamento rapido e ajustavel, com preco sugerido pelo sistema e margem estimada.
- Manter historico tecnico e comercial de faca, pedido, OP, revisao e ativo do cliente.
- Medir tempos e custos reais por etapa, pessoa, material, logistica e empresa.
- Transformar leitura tecnica do arquivo e apontamentos reais em estimativas deterministicas de esforco, prazo, gargalo e capacidade.
- Dar visibilidade gerencial sem expor informacao sensivel para quem esta na producao.
- Suportar grupo empresarial com mais de um CNPJ em uma operacao integrada.

## 4. Objetivos de negocio

- Reduzir tempo de orcamento.
- Reduzir erro de precificacao.
- Melhorar previsibilidade de prazo.
- Identificar gargalos reais da operacao.
- Aumentar rastreabilidade e historico reaproveitavel.
- Consolidar custo x lucro por pedido, cliente, item e empresa.
- Criar diferencial comercial de multi-CNPJ sem licenca abusiva por empresa.

## 5. Principios do produto

- Especializado no dominio de facarias.
- Sistema novo, sem compromisso de reaproveitamento funcional do legado.
- Flexivel no escopo do pedido.
- Rastreavel por design.
- Simples para operar no chao de fabrica.
- Configuravel pelos usuarios-chave sem depender de programacao.
- Deterministico por padrao: aprendizado por calibracao e historico real, nao por IA no escopo inicial.
- Separacao clara entre complexidade tecnica, tempo operacional e capacidade fabril.
- Financeiramente orientado a margem, nao apenas a faturamento.

## 6. Perfis de usuario

### 6.1 Comercial e atendimento

Responsavel por cadastrar cliente, abrir pedido, montar escopo, conversar com cliente, enviar orcamento e acompanhar aprovacao.

### 6.2 Orcamentista

Responsavel por definir e ajustar precos, validar materiais, tempo previsto, margem e regras comerciais.

### 6.3 Engenharia/desenho

Responsavel por anexar arquivo, revisar desenho, cadastrar medidas, batidas, material trabalhado e variacoes tecnicas.

### 6.4 PCP/producao

Responsavel por gerar, separar, juntar, priorizar e replanejar OPs.

### 6.5 Operadores

Responsaveis por executar etapas, apontar tempo, registrar revisao, retrabalho e conclusao. Nao devem ver preco nem custo.

### 6.6 Logistica

Responsavel por coleta, entrega, checklist e comprovacao de movimentacao.

### 6.7 Financeiro/fiscal

Responsavel por contas a pagar, contas a receber, boletos, faturamento, emissao fiscal/NF-e e conciliacoes.

### 6.8 Diretoria/gestao

Responsavel por configuracoes, cadastros mestres, margem, indicadores, patrimonio e analise consolidada multiempresa.

### 6.9 Administrador do sistema

Responsavel por criar, alterar, bloquear e remover usuarios, manter papeis e permissoes, e administrar as variaveis configuraveis do sistema, incluindo cadastros do estimador deterministico. Na implantacao inicial, essa funcao pode ser assumida pelo desenvolvedor/implantador; em operacao normal, deve ficar com administradores internos e diretores.

## 7. Escopo funcional do dominio

## 7.1 Grupo empresarial e multiempresa

O sistema deve permitir que uma conta principal represente um grupo empresarial, contendo um ou mais CNPJs operando de forma integrada.

Cada empresa/CNPJ deve ter:

- dados fiscais e cadastrais proprios;
- configuracoes financeiras proprias;
- emissao fiscal propria quando habilitada;
- resultado por empresa;
- possibilidade de compartilhamento de clientes, materiais, ativos e operacao conforme permissao.

O grupo deve ter:

- visao consolidada de custo, receita e lucro;
- operacao compartilhada;
- cadastros reaproveitados;
- relatorios consolidados e por empresa.

## 7.2 Cadastro de clientes

O cadastro de clientes deve suportar:

- razao social;
- nome fantasia;
- apelidos/nomes internos;
- CNPJ/CPF;
- IE/IM quando aplicavel;
- enderecos de cobranca, entrega e coleta;
- contatos por setor e pessoa;
- telefones, e-mails e observacoes;
- regras comerciais;
- regras de prazo;
- regras de entrega/coleta;
- transportadora padrao e modal padrao por cliente;
- condicoes de pagamento;
- score interno de relacionamento/confianca.

### 7.2.1 Score interno do cliente

O score deve ser informativo e nunca bloquear a operacao automaticamente.

O sistema deve permitir:

- nota numerica interna;
- classificacao visual de risco/confianca;
- historico da nota;
- observacoes justificando a classificacao.

O score pode considerar fatores como:

- pontualidade no pagamento;
- recorrencia;
- volume de compra;
- qualidade dos arquivos/informacoes enviados;
- mudancas de ultima hora;
- necessidade recorrente de urgencia;
- retrabalho originado pelo cliente.

## 7.3 Ativos do cliente

O sistema deve permitir cadastro e historico de ativos tecnicos do cliente, como faca, conjunto, quadro, destacador ou outro item que ja exista e possa ser refeito, ajustado, convertido ou receber troca parcial.

Cada ativo deve guardar:

- identificacao do ativo;
- cliente proprietario;
- historico de pedidos relacionados;
- componentes existentes;
- materiais ja utilizados;
- revisoes;
- fotos e arquivos;
- intervencoes anteriores;
- observacoes tecnicas.

Deve ser possivel:

- reaproveitar um pedido antigo como base para um novo;
- clonar itens antigos para novo pedido;
- alterar apenas partes do conjunto;
- manter comparacao entre versao anterior e nova.

## 7.4 Pedido/Projeto

O pedido e o contexto comercial e tecnico do trabalho.

O texto livre do atendimento deve ser opcional e servir apenas como apoio comercial e de triagem. O centro do pedido continua sendo a composicao dos itens de escopo.

Um pedido deve permitir:

- inicio sem arquivo;
- preenchimento minimo por telefone;
- inclusao posterior de arquivo e revisao;
- vinculacao a cliente e empresa/CNPJ emissor;
- vinculacao opcional a ativo preexistente do cliente;
- abertura de itens variados no mesmo atendimento;
- aprovacao comercial;
- encaminhamento para a cadeia produtiva apos aprovacao do preco final.

## 7.5 Itens de escopo

Cada pedido pode ter um ou mais itens de escopo. O item de escopo representa exatamente o que o cliente esta comprando naquele atendimento.

Classificacoes minimas de item:

- produto principal;
- componente;
- acessorio;
- servico;
- manutencao;
- adaptacao;
- logistica embutida no servico.

Exemplos de itens que o sistema deve suportar:

- faca completa;
- so o aco;
- so a madeira;
- so troca de laminas;
- so corte a laser;
- so quadro;
- so destacador;
- so pertinax;
- so poliester;
- so papel calibrado;
- conversao de faca manual para automatica;
- combinacoes de varios itens no mesmo pedido.

## 7.6 Cadastros de produtos, materiais e suprimentos

O sistema deve permitir cadastro configuravel pelo usuario para tipos de produtos, materiais, acessorios e variacoes.

Para manter a experiencia clara e intuitiva, o sistema nao deve concentrar tudo em uma tela administrativa generica.

A organizacao recomendada da informacao deve separar:

- `Configuracoes`: usuarios, papeis, parametros do estimador, regras avancadas e integracoes;
- `Clientes`: area operacional/comercial dedicada ao cadastro, consulta e reaproveitamento de clientes;
- `Produtos`: catalogo comercial do que e vendido, com regra de cobranca, setor inicial e consumo padrao por unidade;
- `Transportadoras`: parceiros logisticos, contatos, horarios, area atendida e padroes de coleta/entrega;
- `Cadastros`: tipos de faca, tipos de destacador, tipos de borracha, tipos de material, setores, operacoes, fornecedores, unidades e demais tipos mestres;
- `Materiais e estoque`: materiais reais comprados/consumidos, custos, saldos, entradas, saidas, reservas e reposicao.

Essa divisao deve ser mantida tanto no PRD quanto no desenho de navegacao do produto.

O cadastro deve cobrir:

- tipos de faca;
- tipos de destacador;
- tipos de borracha;
- tipos de aco;
- tipos de madeira;
- tipos de pertinax;
- tipos de poliester;
- tipos de papel calibrado;
- outros materiais configuraveis;
- unidades de medida;
- custo padrao;
- tempo padrao;
- regras de precificacao.

O usuario deve conseguir cadastrar e editar os tipos em telas de `Cadastros`, sem depender de desenvolvimento.

### 7.6.1 Estrutura de composicao dos itens vendidos

Cada item vendido deve poder registrar:

- produto comercial de referencia;
- regra/meio de cobranca;
- preco base;
- quais materiais serao usados;
- tipo de cada material;
- quantidade de cada material;
- unidade de medida;
- tempo previsto de producao;
- custo previsto;
- roteiro previsto;
- margem sugerida;
- preco sugerido.

Esses defaults devem ser editaveis no pedido/OP e tambem por cliente quando existir regra comercial especifica.

Isso deve valer para faca, pertinax, papel calibrado, destacadores, poliester, borracha e demais itens configurados.

### 7.6.2 Cadastro de materiais reais

O sistema deve possuir um cadastro proprio para os materiais reais usados na operacao, separado dos tipos e regras tecnicas.

Cada material deve poder registrar:

- nome do material;
- categoria;
- tipo;
- codigo interno;
- unidade de medida;
- custo atual;
- custo medio;
- fornecedor principal;
- observacoes;
- status ativo/inativo.

Categorias minimas esperadas:

- aco;
- madeira;
- borracha;
- pertinax;
- poliester;
- papel calibrado;
- outros materiais configuraveis.

Esse cadastro deve ser usado por:

- orcamento;
- composicao dos itens vendidos;
- consumo por OP;
- compras;
- controle de estoque;
- analise de custo real.

### 7.6.3 Estoque e suprimentos

Materiais com custo e consumo real nao devem ficar escondidos em configuracoes.

O sistema deve possuir um modulo proprio de `Materiais e Estoque`, com no minimo:

- saldo atual;
- estoque minimo;
- entradas;
- saidas;
- ajustes;
- reservas por pedido/OP;
- consumo historico;
- sugestao de reposicao;
- compras futuras ou pendencias de abastecimento.

Quando a OP for criada a partir de um item vendido com composicao conhecida, o sistema deve permitir baixa automatica do estoque com base na quantidade informada no pedido, mantendo trilha auditavel da movimentacao.

Cada movimentacao de estoque deve registrar:

- material;
- tipo de movimentacao;
- quantidade;
- unidade;
- custo relacionado quando aplicavel;
- pedido/OP associado quando houver;
- usuario responsavel;
- data/hora;
- observacoes.

### 7.6.4 Experiencia de uso em materiais e estoque

Como parte relevante dos usuarios possui baixo dominio tecnologico, o modulo deve usar nomenclaturas obvias e fluxo simples.

Regras de experiencia:

- usar menu com nomes literais, como `Materiais` e `Estoque`, em vez de esconder tudo em `Administracao`;
- permitir busca simples por nome, categoria, codigo e tipo;
- destacar saldo, estoque minimo e falta de material;
- separar claramente `tipo de material` de `material em estoque`;
- evitar telas cheias de campos tecnicos para quem so precisa consultar ou apontar consumo;
- exibir custo apenas para perfis autorizados.

Visao por perfil esperada:

- operador: enxerga o material necessario para a OP e, quando permitido, aponta consumo sem ver custo;
- PCP/almoxarifado: enxerga saldo, reserva, faltas, entradas e saidas;
- compras/gestao: enxerga custo, fornecedor, estoque minimo e necessidade de reposicao;
- administrador/diretor: ajusta regras, categorias, tipos e integracoes.

## 7.7 Destacadores

O sistema deve tratar destacadores como familia propria de item, com impacto tecnico, produtivo e comercial.

Tipos minimos previstos:

- de pose;
- para maquina automatica;
- completo macho e femea;
- femea;
- macho e femea;
- dinamico;
- com speedPin;
- sem speedPin.

Regra especifica:

- destacador dinamico exige operacao de tupia/rebaixo nos orificios de passagem de aparas.

## 7.8 Arquivos e revisoes

O sistema deve aceitar:

- PDF;
- arquivo tecnico nativo usado pelos desenhistas;
- anexos complementares;
- observacoes recebidas por telefone.

Cada pedido, ativo e item deve ter historico de revisoes, contendo:

- versao;
- autor da alteracao;
- data/hora;
- motivo da revisao;
- diferenca percebida;
- arquivo vinculado.

### 7.8.1 Ingestao de arquivo e servico de analise

O fluxo padrao do novo sistema deve considerar upload de arquivo pelo proprio sistema.

Requisitos:

- cada pedido pode receber upload manual de arquivo pelo usuario autorizado;
- o arquivo deve ficar vinculado a pedido, item, revisao, cliente e usuario responsavel pelo envio;
- o backend central deve armazenar o arquivo e disparar o servico de analise tecnica quando aplicavel;
- o servico de analise pode rodar fora da fabrica, em infraestrutura centralizada;
- o resultado da analise deve voltar para o sistema como dado estruturado e rastreavel;
- monitoramento de pasta local pode existir como integracao opcional, mas nao deve ser dependencia central do produto.

## 7.9 OPs, split e merge

O sistema deve suportar:

- producao unificada;
- producao separada por componente;
- reagrupamento posterior;
- redistribuicao de itens entre OPs mesmo apos apontamentos;
- preservacao integral de historico e auditoria.

### 7.9.1 Modelo operacional

O pedido deve manter unidade comercial e tecnica.

As OPs devem representar a execucao operacional.

Um mesmo pedido pode gerar:

- uma unica OP;
- varias OPs por componente;
- varias OPs por etapa, se necessario;
- reagrupamento de itens antes ou depois do inicio da execucao.

### 7.9.2 Regras de rastreabilidade de OP

Toda OP deve ter historico completo de alteracoes.

O historico deve registrar:

- quem alterou;
- quando alterou;
- o que alterou;
- valor anterior;
- valor novo;
- modulo/tela de origem;
- justificativa opcional;
- relacao com split, merge, replanejamento, terceirizacao ou revisao.

Eventos minimos auditaveis:

- criacao de OP;
- separacao de OP;
- juncao de OP;
- troca de prioridade;
- mudanca de etapa;
- mudanca de roteiro;
- mudanca de terceirizacao;
- alteracao de prazo;
- mudanca de arquivo/revisao;
- cancelamento;
- reabertura;
- apontamento manual corrigido.

## 7.10 Roteiro e etapas

O sistema deve permitir cadastro de roteiro configuravel por tipo de item e por empresa.

Etapas podem ser internas ou terceirizadas.

Exemplos de etapas:

- desenho/engenharia;
- corte a laser;
- dobradeira;
- montagem;
- emborrachamento;
- tupia;
- acabamento;
- revisao;
- embalagem;
- expedicao.

## 7.11 Apontamento de tempo, capacidade e gargalos

O sistema deve capturar tempo detalhado desde o inicio do atendimento ate a entrega/coleta.

Tempos minimos a medir:

- abertura do pedido;
- elaboracao do orcamento;
- espera de aprovacao;
- engenharia/desenho;
- fila por setor;
- execucao por etapa;
- retrabalho;
- revisao;
- tempo de coleta;
- tempo de entrega;
- tempo total de ciclo.

Objetivos dessa captura:

- identificar gargalos;
- alimentar preco sugerido;
- comparar previsto x real;
- apoiar replanejamento;
- melhorar prazo futuro.

### 7.11.1 Complexidade tecnica x complexidade operacional x complexidade fabril

O sistema deve diferenciar tres conceitos:

- `complexidade tecnica`: dificuldade inerente ao desenho/arquivo, como curvas, metragem, serrilhas, raios delicados, 3pt, intersecoes e demais sinais geometricos;
- `complexidade operacional`: esforco necessario para produzir cada item e cada etapa, considerando roteiro, materiais, setup e tempo de execucao;
- `complexidade fabril`: impacto do pedido dentro da fabrica como um todo, considerando lotacao, fila, recursos disponiveis, dependencias entre etapas e gargalos.

Esses tres sinais nao devem ser tratados como a mesma coisa.

O sistema deve conseguir mostrar, por exemplo:

- um desenho tecnicamente complexo, mas com pouco impacto na fila atual;
- um desenho tecnicamente simples, mas com alto impacto por falta de capacidade em determinado setor;
- um item de acessorio com baixa dificuldade tecnica e alto tempo operacional por depender de etapas manuais.

### 7.11.2 Medidor deterministico de complexidade e estimativa fabril

O sistema deve possuir um motor deterministico para estimar esforco e prazo.

Esse motor deve combinar:

- metricas tecnicas extraidas do arquivo quando houver;
- tipo de item;
- materiais e quantidades;
- roteiro e etapas;
- tempos-base cadastrados;
- setup por operacao;
- operadores ou recursos necessarios;
- prioridade/urgencia;
- lotacao atual da fabrica.

Saidas minimas do motor:

- score de complexidade tecnica;
- tempo estimado por etapa;
- tempo total estimado do item;
- tempo total estimado do pedido;
- setor gargalo previsto;
- capacidade impactada;
- prazo interno sugerido;
- nivel de confianca da estimativa.

### 7.11.3 Cadastros operacionais do estimador

O motor deterministico deve ser dirigido por cadastros configuraveis pelos usuarios autorizados.

Cadastros minimos:

- setores;
- operacoes;
- recursos;
- pessoas ou perfis de recurso;
- tempos de setup;
- taxa base de producao;
- unidade da taxa;
- regras de precedencia entre etapas;
- fatores de urgencia;
- fatores de complexidade;
- faixas de lotacao e capacidade.

Cada operacao deve poder ser modelada com regras como:

- minutos por metro;
- metros por minuto;
- minutos por peca;
- pecas por hora;
- minutos fixos de setup;
- tempo adicional por troca de material;
- tempo adicional por tipo de item;
- tempo adicional por complexidade tecnica.

Exemplos esperados:

- pertinax: desenhar em `metros/minuto`, cortar em `metros/minuto`, embalar em `minutos por item` ou `minutos por lote`;
- destacador: desenho, preparacao, montagem e acabamento com suas taxas proprias;
- borracha: regras por tipo de borracha e por metragem;
- faca: etapas de corte, dobra, montagem, acabamento e revisao com tempos-base independentes.

### 7.11.4 Pessoas, setores e capacidade

O sistema deve modelar a fabrica como capacidade finita.

Isso inclui:

- quais setores existem;
- quantas pessoas e recursos atuam por setor;
- quem pode executar qual operacao;
- capacidade paralela ou nao de cada etapa;
- jornadas e horarios;
- indisponibilidades;
- terceirizacao eventual.

O estimador deve considerar:

- fila atual do setor;
- quantidade de recursos disponiveis;
- tempo ja comprometido em outras OPs;
- dependencia entre etapas;
- possibilidade de executar etapas em paralelo ou nao.

### 7.11.5 Aprendizado deterministico por historico real

O sistema deve aprender por calibracao deterministica, nao por IA no escopo inicial.

Esse aprendizado deve funcionar assim:

- o sistema registra previsto x real por etapa, item, setor, operador e pedido;
- consolida historico suficiente para calcular medias, medianas, desvios e faixas confiaveis;
- sugere ajuste de tempos-base e fatores de complexidade;
- o ajuste pode ser aceito, recusado ou sobrescrito por usuario autorizado;
- toda mudanca de parametro fica auditada com autor, data/hora e motivo.

Regras importantes:

- o sistema nao deve alterar silenciosamente parametros criticos sem rastreabilidade;
- o historico real deve servir para recalibrar o modelo deterministico;
- IA ou modelos probabilisticos avancados ficam fora do escopo inicial e entram, se entrarem, apenas em fase futura.

### 7.11.6 Apontamentos necessarios para calibracao

Para o aprendizado deterministico funcionar, o sistema deve capturar:

- inicio e fim real de cada etapa;
- pausas e esperas;
- operador ou recurso responsavel;
- retrabalho;
- motivo de atraso;
- dependencia de outro setor;
- alteracao de escopo durante a execucao;
- diferenca entre previsto e executado.

### 7.11.7 Uso do medidor no fluxo operacional

O medidor deve participar de todo o fluxo:

- na entrada do pedido, para gerar previsao inicial;
- no orcamento, para sugerir preco e margem;
- no PCP, para definir fila e prazo;
- na producao, para comparar previsto x real;
- na gestao, para localizar gargalos e recalibrar operacao.

## 7.12 Logistica

A logistica deve fazer parte do servico, mas com custos segregados no financeiro.

Modos minimos:

- entregador proprio;
- coleta propria;
- retirada pelo cliente;
- Lalamove;
- Uber;
- terceiro configuravel.

Cada movimentacao deve guardar:

- tipo de movimentacao;
- transportadora quando houver;
- responsavel;
- data/hora prevista;
- data/hora real;
- origem;
- destino;
- checklist do que foi levado ou retirado;
- comprovacao;
- observacoes.

O modulo deve possuir uma area propria de `Transportadoras` para parametrizar:

- nome do parceiro;
- contatos;
- horario de funcionamento;
- area atendida;
- se faz coleta, entrega ou ambos;
- modal padrao;
- observacoes de trabalho e restricoes operacionais.

Custos logisticos segregados:

- combustivel;
- pedagio;
- manutencao do carro;
- hora extra do motorista;
- frete terceirizado;
- outras despesas.

## 7.13 Motor de precificacao

O sistema deve sugerir preco com base em regras configuradas e historico real.

Modos minimos de precificacao:

- por metro;
- por peca;
- por componente;
- por servico;
- por acessorio;
- por urgencia;
- por cliente;
- por tabela especial.

Variaveis consideradas:

- cliente;
- tipo de item;
- tipo e quantidade de material;
- score de complexidade tecnica;
- tempo previsto por etapa;
- tempo previsto;
- tempo historico;
- carga atual da fabrica;
- urgencia;
- terceirizacao prevista;
- logistica prevista;
- margem alvo.

### 7.13.1 Preco sugerido e preco final

Para cada item e pedido, o sistema deve manter:

- custo previsto;
- preco sugerido pelo sistema;
- margem prevista sobre o preco sugerido;
- preco final ajustado pelo usuario;
- margem final;
- historico de alteracoes do preco.

O preco sugerido deve ser facilmente editavel antes do envio ao cliente.

Apos aprovacao do cliente:

- o preco final aprovado passa a reger o pedido;
- o preco sugerido do sistema continua armazenado para analise posterior;
- a cadeia produtiva segue com base no pedido aprovado;
- operadores de producao nao podem visualizar preco nem custo.

## 7.14 Controle de acesso e visibilidade

O sistema deve trabalhar com permissao por perfil e por modulo.

Regra obrigatoria:

- operadores da producao nao podem ver custo, preco, margem, contas a pagar, contas a receber ou dados financeiros sensiveis.

Perfis com acesso financeiro/comercial devem poder ver:

- custo previsto;
- custo real;
- preco sugerido;
- preco final;
- margem;
- indicadores de lucratividade.

### 7.14.1 Usuarios, logins e administracao de acesso

Cada funcionario deve possuir login proprio e identificavel.

O sistema deve suportar:

- criacao de usuario por administrador;
- criacao de usuario por diretores autorizados;
- bloqueio e desativacao de usuario;
- alteracao de papel/perfil;
- redefinicao de senha;
- auditoria de criacao, alteracao e remocao logica.

Perfis minimos previstos:

- administrador do sistema;
- diretor/gestor;
- comercial/orcamentista;
- desenhista/engenharia;
- PCP;
- operador;
- logistica;
- financeiro/fiscal.

Os perfis devem controlar:

- o que cada usuario pode ver;
- o que cada usuario pode alterar;
- quem pode mexer nas variaveis do estimador deterministico;
- quem pode aprovar recalibracoes;
- quem pode administrar usuarios.

### 7.14.2 Arquitetura de informacao e navegacao

Para maximizar a adocao em um ambiente com baixo dominio tecnologico, a navegacao principal deve ser curta, obvia e orientada por trabalho real.

Estrutura recomendada de primeiro nivel:

- `Pedidos`;
- `Producao`;
- `Logistica`;
- `Clientes`;
- `Cadastros`;
- `Materiais`;
- `Estoque`;
- `Financeiro`;
- `Configuracoes`.

Regras de desenho da navegacao:

- evitar concentrar itens demais em `Administracao`;
- separar configuracao tecnica de operacao diaria;
- manter termos conhecidos pela fabrica;
- mostrar para cada perfil apenas o que ele usa;
- reduzir a quantidade de decisoes por tela;
- priorizar busca, status e proximas acoes.

Termos preferenciais:

- `Materiais` em vez de `catalogo tecnico`;
- `Estoque` em vez de `suprimentos` quando o foco for saldo e movimentacao;
- `Configuracoes` em vez de `administracao` quando o conteudo for parametros e usuarios;
- `Pedidos` e `Producao` como areas principais de operacao.

## 7.15 Financeiro

O sistema deve ter controles classicos de ERP no financeiro.

Contas a receber:

- titulos a receber;
- vencimentos;
- baixas;
- historico;
- vinculacao ao pedido;
- criacao manual de lancamentos;
- analise por cliente e empresa.

Contas a pagar:

- fornecedores;
- despesas operacionais;
- custos de pessoal;
- despesas logisticas;
- despesas financeiras;
- historico por centro de custo.

Custos por funcionario:

- cadastro de funcionario;
- custo detalhado no contas a pagar;
- centro de custo;
- horas extras quando aplicavel;
- relacao com etapa, setor e empresa.

Boletos:

- emissao;
- registro;
- baixa;
- conciliacao basica;
- taxa bancaria quando aplicavel.

O modulo financeiro tambem deve permitir:

- criacao manual de contas a pagar e a receber;
- emissao interna de boletos ligada ao titulo;
- historico de boleto por titulo;
- emissao de `NF-e` por adaptador fiscal durante homologacao e producao;
- rastreabilidade entre pedido, titulo, boleto e documento fiscal.

## 7.16 Fiscal

O modulo fiscal faz parte do MVP, mas a emissao pode ser habilitada ou nao por estabelecimento conforme a operacao de cada empresa.

Requisitos:

- o sistema deve sair pronto para plugar o motor fiscal homologado da operacao;
- o sistema deve funcionar mesmo quando a emissao estiver desligada para um estabelecimento especifico;
- quando ativado, a emissao deve respeitar o CNPJ emissor do pedido;
- deve suportar certificado `A1` e `A3`, incluindo cenarios por arquivo, pendrive ou cartao;
- deve permitir impressao de `DANFE`;
- deve guardar `XML`, protocolo e historico do documento emitido;
- deve permitir operacao sem emissao imediata de nota quando a empresa optar por isso.

## 7.17 Patrimonio

Durante desenvolvimento e homologacao interna, o produto pode operar com um adaptador fiscal de validacao, desde que fique explicito que a operacao oficial depende do motor fiscal homologado da empresa.

O sistema deve suportar cadastro de bens da empresa, dos menores aos maiores, com:

- identificacao;
- categoria;
- empresa/CNPJ;
- valor de aquisicao;
- data de aquisicao;
- vida util;
- depreciacao;
- manutencoes;
- observacoes.

## 8. Fluxos principais

## 8.1 Fluxo de novo pedido

1. Atendimento abre pedido com ou sem arquivo.
2. Cliente e empresa/CNPJ sao definidos.
3. Usuario registra contexto inicial opcional quando precisar.
4. Usuario monta itens de escopo.
5. Sistema sugere custo, preco e margem.
6. Usuario ajusta e envia ao cliente.
7. Cliente aprova.
8. Pedido segue para engenharia e producao.
9. PCP define OP unica, OPs separadas ou agrupamento misto.
10. Etapas sao executadas com apontamento e revisao.
11. Logistica coleta/entrega quando aplicavel.
12. Financeiro e fiscal seguem conforme configuracao.

## 8.2 Fluxo de repeticao com alteracoes

1. Usuario busca pedido antigo ou ativo do cliente.
2. Sistema carrega itens, materiais, revisoes e historico.
3. Usuario clona o pedido base.
4. Usuario altera somente o que mudou.
5. Sistema recalcula custo, preco sugerido e margem.
6. Pedido segue para aprovacao e nova execucao.

## 8.3 Fluxo de reforma/manutencao

1. Usuario seleciona ativo existente.
2. Define quais partes serao trocadas, refeitas, adaptadas ou revisadas.
3. Sistema cria itens de escopo especificos.
4. Logistica pode ser criada para buscar o item no cliente.
5. Producao executa apenas o necessario.
6. Sistema registra a intervencao no historico do ativo.

## 8.4 Fluxo de split/merge de OP

1. PCP visualiza componentes dentro do pedido.
2. Usuario distribui componentes em uma ou mais OPs.
3. Se houver mudanca posterior, usuario reorganiza os componentes.
4. Sistema preserva apontamentos anteriores e gera rastreabilidade completa da alteracao.

## 9. Requisitos nao funcionais

- Interface simples, elegante, moderna e intuitiva.
- Telas curtas e orientadas por tarefa.
- Busca rapida por cliente, pedido, ativo, OP, arquivo e item.
- Auditoria forte.
- Permissao por perfil.
- Estrutura multiempresa desde a base.
- Capacidade de configuracao sem suporte tecnico para tabelas de tipos e materiais.
- Navegacao baseada em termos operacionais claros para usuarios com baixo dominio tecnologico.
- Separacao explicita entre `Configuracoes`, `Cadastros`, `Materiais` e `Estoque`.
- Interfaces operacionais sem excesso de campos ou linguagem tecnica desnecessaria.

## 10. Indicadores e dashboards

Indicadores minimos:

- lead time total por pedido;
- lead time por etapa;
- tempo de orcamento;
- tempo de aprovacao;
- fila por setor;
- tempo produtivo por operador;
- retrabalho;
- atraso por etapa;
- custo real x preco final;
- margem por pedido;
- margem por cliente;
- lucro por empresa/CNPJ;
- custo logistico por atendimento;
- custo de pessoal por centro de custo.

## 11. Roadmap por fases

## 11.1 Fase 1 - Operacao principal, analise tecnica, estimativa e rastreabilidade

Objetivo da fase:

Entregar o nucleo operacional do ERP para que a facaria consiga cadastrar cliente, montar pedido flexivel, subir arquivo, obter leitura tecnica, estimar tempo e esforco de forma deterministica, gerar preco sugerido, aprovar, produzir, medir tempo, separar/juntar OPs, registrar historico e calcular margem.

Itens detalhados da fase:

### 11.1.1 Cadastros basicos e configuraveis

- cadastro completo de clientes;
- score interno informativo do cliente;
- cadastro de grupo empresarial e empresas/CNPJs;
- cadastro de tipos de faca;
- cadastro de tipos de destacador;
- cadastro de tipos de borracha;
- cadastro de tipos de aco, madeira, pertinax, poliester e papel calibrado;
- cadastro de unidades de medida;
- cadastro de roteiros e etapas;
- cadastro de modos de entrega/coleta;
- cadastro de terceiros de logistica e terceirizacao;
- cadastro de materiais reais;
- cadastro de fornecedores de materiais.

### 11.1.2 Ativos do cliente e historico tecnico

- cadastro de faca/conjunto ja existente do cliente;
- historico de revisoes;
- vinculo entre ativo e pedidos;
- reaproveitamento de pedido antigo como base;
- comparacao entre configuracao antiga e nova.

### 11.1.3 Pedido flexivel

- criacao de pedido sem arquivo;
- registro inicial por telefone;
- inclusao posterior de arquivo;
- montagem de itens de escopo variados;
- suporte a produto, componente, acessorio, servico, manutencao e adaptacao;
- suporte a itens avulsos como so aco, so madeira, so corte a laser, so troca de laminas, so quadro, so destacador e similares.

### 11.1.4 Ingestao de arquivo e leitura tecnica

- upload de arquivo pelo sistema;
- armazenamento centralizado do arquivo;
- vinculo entre arquivo, usuario, revisao e pedido;
- disparo do servico de analise tecnica;
- retorno de metricas estruturadas para o ERP;
- uso de servico centralizado fora da fabrica, quando desejado.

### 11.1.5 Estrutura tecnica dos itens

- composicao de materiais por item;
- tipo e quantidade de material por item;
- tempo previsto por item;
- roteiro previsto por item;
- custo previsto por item;
- armazenamento do material trabalhado, medidas e quantidade de batidas.

### 11.1.5.1 Materiais e estoque operacionais

- modulo de materiais separado de configuracoes;
- saldo atual por material;
- estoque minimo;
- entradas e saidas;
- reserva por pedido/OP;
- consumo por producao;
- visoes diferentes por perfil;
- custo visivel apenas para perfis autorizados.

### 11.1.6 Medidor deterministico de complexidade e tempo

- score de complexidade tecnica por arquivo;
- estimativa de tempo por etapa;
- estimativa de tempo total por item;
- estimativa de gargalo;
- consideracao de setup por operacao;
- cadastros de taxa base por setor e item;
- consideracao de lotacao/capacidade da fabrica;
- previsao inicial de prazo interno.

### 11.1.7 Orcamento e preco sugerido

- motor de precificacao configuravel;
- precificacao por metro;
- precificacao por peca;
- precificacao por componente e servico;
- adicionais por urgencia;
- sugerir custo, preco e margem;
- permitir ajuste manual simples antes do envio;
- guardar preco sugerido do sistema e preco final aprovado.

### 11.1.8 Aprovacao e transicao para a producao

- registro de status de orcamento e aprovacao;
- data/hora de envio;
- data/hora de aprovacao;
- liberacao do pedido aprovado para a cadeia produtiva;
- preservacao do preco aprovado para analise posterior.

### 11.1.9 OPs flexiveis

- criacao de OP unica;
- criacao de OPs separadas por componente;
- merge e split posterior;
- redistribuicao de componentes entre OPs;
- agrupamento interno mantendo unidade do pedido.

### 11.1.10 Apontamento e medicao de tempo

- apontamento por etapa;
- apontamento por operador;
- apontamento por setor;
- apontamento de fila;
- apontamento de retrabalho;
- medicao do ciclo completo do pedido;
- comparacao entre previsto e realizado;
- base para recalibracao deterministica.

### 11.1.11 Rastreabilidade e auditoria

- historico completo por OP;
- registro de quem alterou;
- registro de quando alterou;
- registro de valor anterior e novo;
- rastreio de alteracoes de arquivo, prazo, etapa, split/merge e replanejamento.

### 11.1.12 Gestao de usuarios e permissoes

- login individual por funcionario;
- criacao e edicao de usuarios por admin/diretor;
- perfis por funcao;
- controle de acesso por modulo;
- controle de quem pode alterar parametros do estimador.

### 11.1.12.1 Estrutura de navegacao inicial

- menu principal com `Pedidos`, `Producao`, `Logistica`, `Clientes`, `Cadastros`, `Materiais`, `Estoque`, `Financeiro` e `Configuracoes`;
- `Clientes` fica separado de `Cadastros` porque e area de uso comercial recorrente, mesmo sendo um dado mestre;
- ocultacao de modulos nao usados por determinado perfil;
- fluxo simples para usuarios de fabrica;
- termos familiares e pouca dependencia de treinamento formal.

### 11.1.13 Logistica operacional

- coleta e entrega;
- uso de motorista proprio;
- uso de Lalamove/Uber/terceiro;
- checklist de movimentacao;
- comprovacao e historico da operacao.

### 11.1.14 Permissoes

- perfil de operacao sem visao de custo ou preco;
- perfil comercial com visao de orcamento;
- perfil financeiro com visao de custos e recebimentos;
- perfil gestor com visao consolidada.

### 11.1.15 Dashboards basicos

- tempo de orcamento;
- tempo de aprovacao;
- tempo por etapa;
- gargalo por setor;
- diferenca entre tempo previsto e tempo real;
- capacidade ocupada por setor;
- custo previsto x custo real;
- margem por pedido.

Resultado esperado da fase 1:

- a empresa consegue operar o processo principal dentro do sistema sem depender de planilhas para orcamento e acompanhamento de OP.

## 11.2 Fase 2 - Financeiro e fiscal integrados

Objetivo da fase:

Adicionar o bloco financeiro classico de ERP e o modulo fiscal `NF-e` plugavel, conectando resultado operacional com recebimentos, pagamentos e documentacao fiscal.

Itens detalhados da fase:

### 11.2.1 Contas a receber

- geracao de titulos a receber;
- baixa manual e integrada;
- vinculacao com pedidos e clientes;
- visao de inadimplencia;
- historico de recebimentos por cliente e empresa.

### 11.2.2 Contas a pagar

- fornecedores;
- despesas gerais;
- despesas por centro de custo;
- custos de pessoal;
- custos de terceiros;
- historico detalhado das obrigacoes.

### 11.2.3 Custos de pessoal

- cadastro de funcionarios;
- custo mensal por funcionario;
- horas extras;
- despesas associadas;
- relacao com centros de custo e empresa/CNPJ.

### 11.2.4 Custos logisticos detalhados

- manutencao do carro;
- combustivel;
- pedagio;
- hora extra do motorista;
- frete de apps e terceiros;
- rateio ou atribuicao direta por atendimento quando aplicavel.

### 11.2.5 Boletos

- emissao de boleto;
- registro;
- baixa;
- status de cobranca;
- taxa bancaria.

### 11.2.6 Fiscal `NF-e` plugavel

- configuracao de emissor fiscal por empresa;
- emissao de `NF-e` por adaptador homologado;
- suporte a certificado `A1`;
- suporte a certificado `A3`;
- impressao de `DANFE`;
- historico de documentos emitidos.

### 11.2.7 Relatorios financeiros e gerenciais

- fluxo de caixa basico;
- resultado por pedido;
- resultado por cliente;
- resultado por empresa;
- margem apos despesas.

Resultado esperado da fase 2:

- a empresa consegue acompanhar o dinheiro entrando e saindo com rastreabilidade por pedido, cliente e empresa, e emitir `NF-e` pelo adaptador fiscal configurado quando o estabelecimento estiver habilitado.

## 11.3 Fase 3 - Gestao ampliada e calibracao deterministica avancada

Objetivo da fase:

Expandir o ERP com patrimonio, consolidacao gerencial mais profunda e calibracao deterministica avancada baseada em historico real para precificacao, prazo e capacidade.

Itens detalhados da fase:

### 11.3.1 Patrimonio

- cadastro de bens;
- categorias;
- vida util;
- depreciacao;
- manutencoes;
- relatorios por empresa.

### 11.3.2 Consolidacao multiempresa avancada

- dashboards consolidados do grupo;
- comparacao entre empresas;
- custo e lucro consolidados;
- compartilhamento controlado de cadastros e operacoes.

### 11.3.3 Calibracao deterministica de precificacao

- uso do historico real para sugerir prazo;
- uso do historico real para sugerir custo;
- comparacao entre estimado e executado;
- recalibracao de tempos-base por regra deterministica;
- apoio a revisao de tabela de preco.

### 11.3.4 Analitica operacional avancada

- gargalos recorrentes por setor;
- gargalos por tipo de item;
- gargalos por cliente;
- gargalos por empresa;
- visao de capacidade e sobrecarga.

### 11.3.5 Analise historica dos ativos do cliente

- custo acumulado por ativo;
- frequencia de manutencao;
- historico de mudancas;
- comparativo entre refazer e reformar.

Resultado esperado da fase 3:

- a empresa usa o ERP nao so para operar, mas para decidir melhor preco, prazo, investimento e alocacao de capacidade, mantendo o modelo de estimativa auditavel e deterministico.

## 12. Fora do escopo inicial

- bloqueio automatico de operacao com base no score do cliente;
- automacao de decisao comercial sem confirmacao humana;
- exposicao de custo/preco na interface de operadores;
- dependencia obrigatoria do modulo fiscal para uso do sistema;
- IA generativa, machine learning autonomo ou recalibracao opaca no nucleo do estimador;
- cobranca de licencas integrais por cada CNPJ do mesmo grupo sem compartilhar operacao.

## 13. Riscos e atencoes

- o dominio e amplo e pode virar um monolito se todas as fases forem atacadas de uma vez;
- fiscal brasileiro exige parametrizacao cuidadosa por empresa;
- split e merge de OP apos apontamentos exigem modelagem de auditoria robusta;
- precificacao sugerida so gera confianca se o apontamento real de tempo for bem alimentado;
- multiempresa precisa nascer bem modelado para nao contaminar relatorios.

## 14. Criterios de sucesso

- tempo medio de orcamento reduzido;
- maior acuracia do preco aprovado versus custo real;
- reducao de retrabalho operacional por falta de historico;
- identificacao objetiva de gargalos;
- consolidacao de margem por pedido e por cliente;
- aceitacao do sistema pelos operadores sem dificuldade de uso.
