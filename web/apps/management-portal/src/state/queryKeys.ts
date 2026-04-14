export const portalKeys = {
  session: ['management-session'] as const,
  drinks: ['drinks'] as const,
  drink: (id: string) => ['drink', id] as const,
  tags: ['tags'] as const,
  ingredients: ['ingredients'] as const,
  imports: ['imports'] as const,
  importBatch: (id: string) => ['import-batch', id] as const,
  audit: ['audit-log'] as const,
};
