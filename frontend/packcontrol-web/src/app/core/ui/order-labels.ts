export const serviceTypeLabels: Record<string, string> = {
  New: 'Novo',
  Repeat: 'Repeticao',
  Maintenance: 'Manutencao',
  Rework: 'Reforma',
  Adaptation: 'Adaptacao'
};

export const urgencyLabels: Record<string, string> = {
  Normal: 'Normal',
  Urgent: 'Urgente',
  MachineStop: 'Parada de maquina'
};

export const orderStatusLabels: Record<string, string> = {
  Draft: 'Rascunho',
  AwaitingTechnicalAnalysis: 'Aguardando analise tecnica',
  AwaitingQuote: 'Aguardando orcamento',
  Approved: 'Aprovado',
  InProduction: 'Em producao'
};

export const scopeCategoryLabels: Record<string, string> = {
  produto_principal: 'Produto principal',
  componente: 'Componente',
  acessorio: 'Acessorio',
  servico: 'Servico',
  manutencao: 'Manutencao',
  adaptacao: 'Adaptacao'
};

export const technicalAnalysisStatusLabels: Record<string, string> = {
  PendingEngine: 'Aguardando motor',
  Completed: 'Concluida',
  Failed: 'Falhou'
};
