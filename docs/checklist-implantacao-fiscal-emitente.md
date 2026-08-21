# Checklist de Implantacao Fiscal por Emitente

Versao: 0.1  
Data: 2026-03-27  
Status: Template operacional para liberar um emitente em producao fiscal

Documento complementar:
- `docs/runbook-implantacao-fiscal.md`
- `docs/backlog-modulo-fiscal-nfe.md`
- `docs/runbook-deploy-packcontrol.md`

## 1. Identificacao do emitente

Preencher antes de qualquer acao tecnica:

| Campo | Valor |
|---|---|
| Razao social | |
| Nome fantasia | |
| `CNPJ` | |
| Cidade | Sao Paulo |
| `UF` | |
| Pais | Brasil |
| Regime tributario | |
| Inscricao estadual | |
| CNAE principal | |
| Serie de homologacao | |
| Serie de producao | |
| Responsavel interno | |
| Contador responsavel | |
| Meio principal | `A1` / `A3` |
| Meio contingente/opcional | |
| Autorizador/SEFAZ | |
| Data alvo do go-live | |

## 2. Gate `F0` - Preparacao fiscal real

### `NF-00` Definicao do emitente

- [ ] Emitente alvo definido para o rollout.
- [ ] Emitente dentro do recorte Sao Paulo/SP, Brasil.
- [ ] cidade, `UF`, regime e autorizador confirmados.
- [ ] meio principal fechado: `A1` ou `A3`.
- [ ] meio contingente/opcional registrado, se existir.

Evidencia:
- [ ] registro do emitente e aprovacao interna.

### `NF-01` Matriz fiscal inicial

- [ ] `CFOP` por operacao principal validado.
- [ ] natureza de operacao validada.
- [ ] finalidade da `NF-e` validada.
- [ ] regras de frete validadas.
- [ ] excecoes conhecidas registradas.
- [ ] `NCM` e unidades principais revisados.

Evidencia:
- [ ] matriz assinada/aprovada pelo contador.

### `NF-02` Credenciamento e ambientes

- [ ] emitente credenciado para homologacao.
- [ ] emitente credenciado para producao.
- [ ] URLs/autorizadores confirmados.
- [ ] separacao entre homologacao e producao confirmada.
- [ ] recorte Sao Paulo/SP permanece aderente ao emitente escolhido.

Evidencia:
- [ ] comprovacao de credenciamento ou validacao operacional.

### `NF-03` Inventario de certificados

- [ ] certificado principal inventariado.
- [ ] certificado contingente inventariado, se houver.
- [ ] validade registrada.
- [ ] serial/label registrado.
- [ ] plano de renovacao definido.

Se `A1`:
- [ ] `PFX` localizado e testado.
- [ ] senha validada.

Se `A3`:
- [ ] token/cartao identificado.
- [ ] driver e middleware instalados.
- [ ] porta/maquina do agente definida.

Evidencia:
- [ ] inventario de certificado anexado.

### `NF-04` Storage e segredos

- [ ] `PostgreSQL` de producao definido.
- [ ] storage persistente definido.
- [ ] vault/gestao de segredo definido.
- [ ] estrategia de backup definida.
- [ ] estrategia de restore definida.

Evidencia:
- [ ] destino de `XML` e `DANFE` validado.
- [ ] restore de teste executado.

### `NF-05` Politica de ambientes

- [ ] serie de homologacao separada da serie de producao.
- [ ] numeracao segregada por ambiente.
- [ ] segredo segregado por ambiente.
- [ ] emitente configurado nos dois ambientes.

Evidencia:
- [ ] configuracao revisada por segundo operador.

## 3. Gate `F1` - Core fiscal pronto

- [ ] precondicoes fiscais ativas antes da emissao.
- [ ] snapshot fiscal completo persistido.
- [ ] erros classificados em dados, schema, certificado, autorizador, infra e contingencia.
- [ ] numeracao protegida por emitente/serie/ambiente.
- [ ] pedido e financeiro refletindo o status fiscal.
- [ ] tela fiscal exibindo timeline, erros e artefatos.

Evidencia:
- [ ] smoke interno do core executado.

## 4. Gate `F2` - Homologacao `A1`

Preencher apenas se o emitente usar `A1`.

- [ ] `XML` oficial gerado no leiaute vigente.
- [ ] assinatura `A1` valida.
- [ ] transmissao real executada em homologacao.
- [ ] consulta de recibo/protocolo validada.
- [ ] `XML` autorizado e `DANFE` arquivados.

Evidencia:
- [ ] numero da nota homologada:
- [ ] chave de acesso:
- [ ] protocolo:
- [ ] caminho do `XML`:
- [ ] caminho do `DANFE`:

## 5. Gate `F3` - Eventos fiscais e archive

- [ ] cancelamento homologado.
- [ ] inutilizacao homologada.
- [ ] `CC-e` homologada.
- [ ] consulta posterior validada.
- [ ] reprocesso de erro transitorio validado.
- [ ] download e restore de artefatos validados.

Evidencia:
- [ ] numeros/chaves dos eventos executados:

## 6. Gate `F4` - Homologacao `A3`

Preencher apenas se o emitente usar `A3`.

- [ ] agente `A3` registrado.
- [ ] heartbeat e status online visiveis no ERP.
- [ ] discovery de certificado executado.
- [ ] teste de `PIN` executado.
- [ ] emissao real em homologacao concluida.
- [ ] erro de dispositivo/driver testado e legivel.

Evidencia:
- [ ] host do agente:
- [ ] serial do certificado:
- [ ] numero da nota homologada:
- [ ] chave de acesso:

## 7. Gate `F5` - Producao e go-live

- [ ] monitoracao ativa para storage, certificado, autorizador e fila.
- [ ] alertas ativos.
- [ ] runbook revisado com a operacao.
- [ ] rollback definido.
- [ ] janela de go-live aprovada.
- [ ] smoke de producao controlado executado.
- [ ] dependencia de emissor paralelo encerrada para este emitente.

Evidencia:
- [ ] numero da primeira nota real:
- [ ] chave de acesso:
- [ ] protocolo:
- [ ] data/hora do go-live:

## 8. Criterios de bloqueio

Nao liberar o emitente se qualquer item abaixo estiver aberto:
- [ ] credenciamento incompleto.
- [ ] certificado vencido ou proximo do vencimento sem plano.
- [ ] storage sem restore validado.
- [ ] cancelamento/inutilizacao/`CC-e` nao testados.
- [ ] runbook sem aprovacao.
- [ ] smoke de producao nao executado.

## 9. Aprovacoes

| Papel | Nome | Data | Assinatura/aceite |
|---|---|---|---|
| Responsavel do cliente | | | |
| Contador | | | |
| Implantacao | | | |
| Tecnologia | | | |
| Direcao/aprovador final | | | |
