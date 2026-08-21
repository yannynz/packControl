# Revisao do Wireframe Visily

## Base visual recomendada

Usar como base visual principal o `pipeline-board` do `organizador-producao`.

O que puxar:
- cards arredondados com sombra leve;
- chips suaves para status, atributos e urgencia;
- barra superior com resumo e filtros rapidos;
- estados visuais claros para atraso e prazo curto;
- espaco em branco generoso e menos cara de tabela antiga.

O que puxar da logica operacional:
- informar o responsavel uma vez por setor e agir rapido;
- uma acao principal dominante por tela operacional;
- `Imagem` e `Materiais` como apoio, nao como foco;
- complexidade mostrada de forma compacta;
- operacao em lote na expedicao;
- fluxo de excecao para saida adversa.

O que evitar:
- mistura visual de dois produtos diferentes;
- modais longos em formato de tabela;
- muitas acoes com o mesmo peso visual;
- excesso de microcards e linhas muito densas.

## Ajustes por pagina

### Pagina 1 - Dashboard
- manter os cards de resumo, mas aproximar mais do shell do board;
- trocar a cara de dashboard generico por um painel de prioridades do dia;
- destacar `minha fila`, `gargalo atual`, `pedidos criticos` e `entregas do dia`;
- usar chips suaves para setor e prioridade;
- deixar os alertas mais visiveis e menos escondidos na lateral.

### Pagina 2 - Novo Pedido
- deixar `Novo`, `Repeticao`, `Manutencao`, `Reforma` e `Adaptacao` como cards grandes clicaveis;
- subir `pedido sem arquivo` e `pedido baseado em item antigo` para o topo;
- reduzir a quantidade de campos iniciais;
- manter resumo lateral, mas com menos informacao secundaria.

### Pagina 3 - Escopo do Pedido
- agrupar em `item principal`, `acessorios` e `servicos`;
- reforcar visualmente que o cliente pode comprar so parte do conjunto;
- trocar alguns controles pequenos por cards de item mais claros;
- adicionar destaque para `vinculado a ativo antigo`;
- deixar `duplicar item` e `adicionar componente` mais evidentes.

### Pagina 4 - Analise Tecnica
- manter a area escura do desenho, porque ela funciona bem;
- simplificar a coluna lateral em blocos: `arquivo`, `complexidade`, `materiais detectados`, `alertas`;
- reduzir a sensacao de tela tecnica demais;
- adicionar um bloco de revisoes do arquivo com historico simples.

### Pagina 5 - Estimativa Deterministica
- manter a estrutura, mas trocar a tabela dura por cards de etapa;
- mostrar `setup`, `tempo de execucao`, `fila` e `total` por etapa;
- dar mais destaque ao `gargalo previsto`;
- deixar `confianca da estimativa` e `prazo sugerido` mais visiveis;
- o grafico deve ser auxiliar, nao protagonista.

### Pagina 6 - Orcamento
- destacar mais `preco sugerido`, `margem` e `preco final`;
- deixar claro que o preco sugerido veio do sistema e o final foi ajustado pelo usuario;
- mostrar o resumo do escopo de forma mais comercial e menos tecnica;
- manter o envio ao cliente como CTA principal;
- usar um painel lateral de observacoes comerciais e condicoes.

### Pagina 7 - Pedido Consolidado
- transformar a tela em abas: `Resumo`, `Arquivos`, `Componentes`, `OPs`, `Logistica`, `Historico`;
- reduzir a mistura de tudo em uma tela so;
- dar destaque ao status geral e ao que esta travando o pedido;
- incluir historico auditavel visivel, nao escondido.

### Pagina 8 - Ordens de Producao
- usar mais linguagem de board/cards e menos linguagem de grade;
- deixar claro o relacionamento entre trabalho principal e OPs filhas;
- destacar `separar` e `juntar` como acoes centrais;
- mostrar `ultima alteracao por quem e quando`;
- usar chips para componente, setor atual, urgencia e terceirizacao.

### Pagina 9 - Producao por Setor
- esta tela deve virar mais um indice de setores do que a tela final de operacao;
- cada card de setor deve levar para uma fila operacional no estilo das paginas 15 a 17;
- mostrar poucos indicadores: fila, atraso, capacidade e eficiencia;
- evitar excesso de detalhes dentro de cada card.

### Pagina 10 - Gestao de Materiais
- manter separacao de `Materiais` fora de `Configuracoes`;
- diferenciar melhor `tipo tecnico` de `item real de estoque`;
- destacar custo, fornecedor principal e risco de falta;
- melhorar os filtros por categoria e status;
- manter tabela, mas com cards/resumos acima.

### Pagina 11 - Gestao de Estoque
- manter alertas de reposicao no topo;
- destacar reservas por pedido/OP;
- simplificar a leitura para almoxarifado;
- usar cor com moderacao para `baixo`, `critico` e `ok`;
- trazer `ultima movimentacao` e `acao de reposicao` mais para cima.

### Pagina 12 - Logistica e Expedicao
- puxar a logica de operacao em lote do sistema atual;
- criar barra de acoes com `confirmar saida`, `retirada` e `saida adversa`;
- mostrar modo de transporte com chips claros;
- deixar checklist e comprovante mais visiveis;
- melhorar a legibilidade da grade de entregas do dia.

### Pagina 13 - Financeiro
- manter a tela separada da operacao;
- simplificar um pouco o excesso de leitura analitica inicial;
- organizar em blocos claros: `receber`, `pagar`, `resultado`, `boletos`, `notas`;
- emissao de `NF-e` deve seguir a configuracao do estabelecimento, sem poluir a operacao quando estiver desligada;
- esconder qualquer leitura financeira dos perfis operacionais.

### Pagina 14 - Configuracoes
- manter foco em `usuarios`, `papeis`, `parametros do estimador`, `taxas base`, `empresas/CNPJs` e `integracoes`;
- nao deixar materiais e estoque entrarem aqui;
- usar subnavegacao por grupos para evitar tela administrativa confusa;
- deixar o modulo de acessos como o primeiro bloco.

### Pagina 15 - Fila de Montagem
- essa pagina esta no caminho certo;
- unificar o shell visual com o resto do ERP para nao parecer outro produto;
- puxar a logica da tela real de montagem: responsavel no topo e acoes rapidas por card;
- manter `Imagem` e `Materiais` como apoio;
- mostrar complexidade em estrelas ou badge compacto.

### Pagina 16 - Setor de Emborrachamento
- seguir a mesma casca visual da montagem;
- deixar `Dar baixa` como acao principal dominante;
- reduzir elementos secundarios dentro dos cards;
- reforcar prazo curto e atraso com estados visuais simples;
- manter a fila bem legivel.

### Pagina 17 - Expedicao Operacional
- unificar branding e navegacao com o resto do produto;
- manter foco em lote, checklist, entregador, veiculo, recebedor e ocorrencia;
- explicitar fluxos de coleta, entrega, retirada e terceiro;
- incluir atalho para saida adversa;
- deixar a tela preparada para tablet.

## Ajustes globais que eu faria antes de mostrar ao cliente

- unificar `CutFlow ERP` e `Producao Operacional` como um produto so;
- usar a linguagem visual do board novo em todas as telas;
- reduzir a cara de software generico SaaS nas telas de operacao;
- reforcar a diferenca entre tela gerencial e tela de execucao;
- manter termos simples, botoes grandes e caminho curto para a tarefa principal.
