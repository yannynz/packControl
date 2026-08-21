export const userRoleLabels: Record<string, string> = {
  Administrator: 'Administrador',
  Sales: 'Comercial',
  Engineering: 'Engenharia',
  Production: 'Producao',
  Logistics: 'Logistica',
  Finance: 'Financeiro',
  Management: 'Gestao'
};

export const materialCategoryLabels: Record<string, string> = {
  estrutura: 'Estrutura',
  acabamento: 'Acabamento',
  base: 'Base',
  componente: 'Componente'
};

export const technicalTypeLabels: Record<string, string> = {
  aco: 'Aco',
  borracha: 'Borracha',
  madeira: 'Madeira',
  pertinax: 'Pertinax',
  poliester: 'Poliester',
  papel_calibrado: 'Papel calibrado'
};

export const auditEventLabels: Record<string, string> = {
  'order.created': 'Pedido criado',
  'order.seeded': 'Pedido seedado',
  'order.approved': 'Pedido aprovado',
  'order.attachment_added': 'Arquivo anexado',
  'order.operational_projection_created': 'Operacao projetada',
  'production.order_advanced': 'OP avancada',
  'logistics.dispatched': 'Despacho confirmado',
  'logistics.withdrawal': 'Retirada configurada',
  'logistics.adverse': 'Ocorrencia logistica',
  'finance.entry_settled': 'Lancamento liquidado',
  'customer.created': 'Cliente criado',
  'register.created': 'Cadastro criado',
  'register.updated': 'Cadastro atualizado'
};

export function formatMappedLabel(value: string, map: Record<string, string>): string {
  return map[value] ?? humanizeCode(value);
}

export function humanizeCode(value: string): string {
  return value
    .replace(/[_-]+/g, ' ')
    .replace(/\b\w/g, (char) => char.toUpperCase());
}
