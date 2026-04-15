export const portalKeys = {
  session: ['customer-session'] as const,
  ingredients: ['customer-ingredients'] as const,
  profile: ['customer-profile'] as const,
  recommendationSessions: ['recommendation-sessions'] as const,
  recommendationSession: (id: string) => ['recommendation-session', id] as const,
};
