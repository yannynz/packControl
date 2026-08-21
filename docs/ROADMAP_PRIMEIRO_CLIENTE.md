# Roadmap para o Primeiro Cliente (Go-Live Vendável)

Este documento detalha os requisitos críticos que devem ser implementados no **packControl** antes de realizar a primeira implantação em ambiente real de produção (venda/operação).

## 1. Bloco Fiscal (Prioridade Máxima)
O sistema possui a lógica de emissão, mas carece de prontidão operacional para o mundo real.
- [ ] **Homologação com Emitente Real:** Realizar testes de emissão de NF-e (A1) em ambiente de homologação da SEFAZ utilizando um CNPJ e certificado digital reais.
- [ ] **DANFE Oficial:** Substituir a representação simplificada atual por um gerador de DANFE oficial (ex: integração com UniDANFE ou biblioteca de renderização térmica/A4 compatível).
- [ ] **Eventos Fiscais:** Validar e testar exaustivamente o fluxo de Cancelamento e Carta de Correção Eletrônica (CC-e) com retorno oficial da SEFAZ.

## 2. Persistência e Dados
Sair do modelo de snapshot único para garantir integridade em escala industrial.
- [ ] **Migração para Modelo Relacional:** Implementar Entity Framework Core (ou Dapper) com Migrations formais no PostgreSQL. Abandonar o armazenamento via `app_state_snapshots` para as entidades principais (Pedidos, OPs, Financeiro).
- [ ] **Plano de Backup/Restore:** Criar e testar scripts automatizados de dump do banco de dados e backup do volume de anexos (storage local). O "Dia 1" do cliente não pode ocorrer sem a garantia de recuperação em caso de falha de hardware.

## 3. Segurança de Produção
- [ ] **MFA (Multi-Factor Authentication):** Implementar segundo fator de autenticação para os perfis `Administrador` e `Financeiro`.
- [ ] **Hardening de API:** 
    - [ ] Rate Limiting (limite de requisições por IP).
    - [ ] Proteção CSRF (Cross-Site Request Forgery).
    - [ ] Gestão de segredos via variáveis de ambiente ou Key Vault (remover segredos do `appsettings.json`).

## 4. Maturidade Operacional
- [ ] **Relatórios Financeiros:** Exportação de fluxos de caixa em PDF/Excel para conferência do contador.
- [ ] **Interface de Suporte:** Log detalhado de auditoria de alterações (quem mudou o status da OP e quando).

---
**Status Atual:** O projeto está pronto para *Deploy Técnico* (subir containers, testar navegação). **NÃO** está pronto para *Go-Live Vendável*.
