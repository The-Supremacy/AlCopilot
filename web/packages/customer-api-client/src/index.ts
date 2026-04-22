export type CustomerSessionDto = {
  isAuthenticated: boolean;
  displayName: string | null;
  roles: string[];
  canAccessCustomerPortal: boolean;
};

export type IngredientDto = {
  id: string;
  name: string;
  notableBrands: string[];
};

export type CustomerProfileDto = {
  favoriteIngredientIds: string[];
  dislikedIngredientIds: string[];
  prohibitedIngredientIds: string[];
  ownedIngredientIds: string[];
};

export type SaveCustomerProfileInput = CustomerProfileDto;

export type RecommendationItemDto = {
  drinkId: string;
  drinkName: string;
  description: string | null;
  missingIngredientNames: string[];
  matchedSignals: string[];
  score: number;
  recipeEntries?: RecommendationRecipeEntryDto[] | null;
};

export type RecommendationRecipeEntryDto = {
  ingredientName: string;
  quantity: string;
  isOwned: boolean;
};

export type RecommendationGroupDto = {
  key: string;
  label: string;
  items: RecommendationItemDto[];
};

export type RecommendationToolInvocationDto = {
  toolName: string;
  purpose: string;
};

export type RecommendationTurnDto = {
  turnId: string;
  sequence: number;
  role: string;
  content: string;
  recommendationGroups: RecommendationGroupDto[];
  toolInvocations: RecommendationToolInvocationDto[];
  createdAtUtc: string;
};

export type RecommendationSessionDto = {
  sessionId: string;
  title: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  turns: RecommendationTurnDto[];
};

export type RecommendationSessionSummaryDto = {
  sessionId: string;
  title: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  lastAssistantMessage: string;
};

export type SubmitRecommendationRequestInput = {
  sessionId: string | null;
  message: string;
};

const baseUrl = '';

export class CustomerApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'CustomerApiError';
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
    ...init,
  });

  if (!response.ok) {
    const raw = await response.text();
    let message = raw;

    if (raw) {
      try {
        const parsed = JSON.parse(raw) as { title?: string; detail?: string };
        message = parsed.detail || parsed.title || raw;
      } catch {
        message = raw;
      }
    }

    throw new CustomerApiError(
      response.status,
      message || `Request failed with status ${response.status}`,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get('content-type') ?? '';
  if (!contentType.includes('application/json')) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export function buildCustomerLoginUrl(returnUrl = '/') {
  const search = new URLSearchParams({ returnUrl });
  return `/api/auth/customer/login?${search.toString()}`;
}

export function buildCustomerRegisterUrl(returnUrl = '/') {
  const search = new URLSearchParams({ returnUrl });
  return `/api/auth/customer/register?${search.toString()}`;
}

export function getCustomerSession() {
  return request<CustomerSessionDto>('/api/auth/customer/session');
}

export function logoutCustomer() {
  return fetch(`${baseUrl}/api/auth/customer/logout`, {
    method: 'POST',
    credentials: 'same-origin',
  }).then(async (response) => {
    if (!response.ok) {
      const raw = await response.text();
      throw new CustomerApiError(
        response.status,
        raw || `Request failed with status ${response.status}`,
      );
    }
  });
}

export function listCustomerIngredients() {
  return request<IngredientDto[]>('/api/customer/ingredients');
}

export function getCustomerProfile() {
  return request<CustomerProfileDto>('/api/customer/profile/');
}

export function saveCustomerProfile(input: SaveCustomerProfileInput) {
  return request<CustomerProfileDto>('/api/customer/profile/', {
    method: 'PUT',
    body: JSON.stringify(input),
  });
}

export function listRecommendationSessions() {
  return request<RecommendationSessionSummaryDto[]>('/api/customer/recommendations/sessions');
}

export function getRecommendationSession(sessionId: string) {
  return request<RecommendationSessionDto>(`/api/customer/recommendations/sessions/${sessionId}`);
}

export function submitRecommendationRequest(input: SubmitRecommendationRequestInput) {
  return request<RecommendationSessionDto>('/api/customer/recommendations/messages', {
    method: 'POST',
    body: JSON.stringify(input),
  });
}
