export type TagDto = {
  id: string;
  name: string;
  drinkCount: number;
};

export type IngredientDto = {
  id: string;
  name: string;
  notableBrands: string[];
};

export type RecipeEntryDto = {
  ingredient: IngredientDto;
  quantity: string;
  recommendedBrand: string | null;
};

export type DrinkDto = {
  id: string;
  name: string;
  category: string | null;
  description: string | null;
  method: string | null;
  garnish: string | null;
  imageUrl: string | null;
  tags: TagDto[];
};

export type DrinkDetailDto = {
  id: string;
  name: string;
  category: string | null;
  description: string | null;
  method: string | null;
  garnish: string | null;
  imageUrl: string | null;
  tags: TagDto[];
  recipeEntries: RecipeEntryDto[];
};

export type ImportSourceInput = {
  sourceReference: string | null;
  displayName: string | null;
  contentType: string | null;
  metadata: Record<string, string | null>;
};

export type ImportDiagnosticDto = {
  rowNumber: number | null;
  code: string;
  message: string;
  severity: string;
};

export type ImportReviewConflictDto = {
  targetType: string;
  targetKey: string;
  action: string;
  summary: string;
};

export type ImportReviewRowDto = {
  targetType: string;
  targetKey: string;
  action: string;
  changeSummary: string;
  hasConflict: boolean;
  hasError: boolean;
};

export type ImportReviewSummaryDto = {
  createCount: number;
  updateCount: number;
  skipCount: number;
};

export type ImportApplySummaryDto = {
  createdCount: number;
  updatedCount: number;
  skippedCount: number;
  rejectedCount: number;
};

export type ImportDecisionInput = {
  targetType: string;
  targetKey: string;
  decision: string;
  reason: string | null;
};

export type ImportBatchDto = {
  id: string;
  strategyKey: string;
  status: string;
  sourceFingerprint: string | null;
  source: ImportSourceInput;
  diagnostics: ImportDiagnosticDto[];
  reviewConflicts: ImportReviewConflictDto[];
  reviewRows: ImportReviewRowDto[];
  reviewSummary: ImportReviewSummaryDto | null;
  applySummary: ImportApplySummaryDto | null;
  createdAtUtc: string;
  validatedAtUtc: string | null;
  reviewedAtUtc: string | null;
  appliedAtUtc: string | null;
  lastUpdatedAtUtc: string;
};

export type AuditLogEntryDto = {
  id: number;
  action: string;
  subjectType: string;
  subjectKey: string | null;
  actorUserId: string | null;
  actor: string;
  summary: string;
  occurredAtUtc: string;
};

export type ManagementSessionDto = {
  isAuthenticated: boolean;
  displayName: string | null;
  roles: string[];
  isAdmin: boolean;
  canAccessManagementPortal: boolean;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type RecipeEntryInput = {
  ingredientId: string;
  quantity: string;
  recommendedBrand: string | null;
};

export type CreateDrinkInput = {
  name: string;
  category: string | null;
  description: string | null;
  method: string | null;
  garnish: string | null;
  imageUrl: string | null;
  tagIds: string[];
  recipeEntries: RecipeEntryInput[];
};

export type UpdateDrinkInput = CreateDrinkInput;

export type CreateTagInput = {
  name: string;
};

export type CreateIngredientInput = {
  name: string;
  notableBrands: string[];
};

export type UpdateIngredientInput = {
  name: string;
  notableBrands: string[];
};

export type StartImportInput = {
  strategyKey: string;
  payload: string;
  source: ImportSourceInput;
};

export type ApplyImportBatchInput = {
  overrideDuplicateFingerprint: boolean;
  decisions: ImportDecisionInput[];
};

const baseUrl = '';

export class ManagementApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ManagementApiError';
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

    throw new ManagementApiError(
      response.status,
      message || `Request failed with status ${response.status}`,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export function buildManagementLoginUrl(returnUrl = '/') {
  const search = new URLSearchParams({ returnUrl });
  return `/api/auth/management/login?${search.toString()}`;
}

export function getManagementSession() {
  return request<ManagementSessionDto>(`/api/auth/management/session`);
}

export function logoutManagement() {
  return request<void>(`/api/auth/management/logout`, {
    method: 'POST',
  });
}

export function listDrinks(params?: { q?: string; tagIds?: string[] }) {
  const search = new URLSearchParams();
  search.set('page', '1');
  search.set('pageSize', '50');

  if (params?.q) {
    search.set('q', params.q);
  }

  params?.tagIds?.forEach((tagId) => search.append('tagIds', tagId));

  return request<PagedResult<DrinkDto>>(`/api/drink-catalog/drinks/?${search.toString()}`);
}

export function getDrink(id: string) {
  return request<DrinkDetailDto>(`/api/drink-catalog/drinks/${id}`);
}

export function createDrink(input: CreateDrinkInput) {
  return request<{ id: string }>(`/api/drink-catalog/drinks/`, {
    method: 'POST',
    body: JSON.stringify(input),
  });
}

export function updateDrink(id: string, input: UpdateDrinkInput) {
  return request<void>(`/api/drink-catalog/drinks/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ ...input, drinkId: id }),
  });
}

export function deleteDrink(id: string) {
  return request<void>(`/api/drink-catalog/drinks/${id}`, {
    method: 'DELETE',
  });
}

export function listTags() {
  return request<TagDto[]>(`/api/drink-catalog/tags/`);
}

export function createTag(input: CreateTagInput) {
  return request<{ id: string }>(`/api/drink-catalog/tags/`, {
    method: 'POST',
    body: JSON.stringify(input),
  });
}

export function updateTag(id: string, input: CreateTagInput) {
  return request<void>(`/api/drink-catalog/tags/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ ...input, tagId: id }),
  });
}

export function deleteTag(id: string) {
  return request<void>(`/api/drink-catalog/tags/${id}`, {
    method: 'DELETE',
  });
}

export function listIngredients() {
  return request<IngredientDto[]>(`/api/drink-catalog/ingredients/`);
}

export function createIngredient(input: CreateIngredientInput) {
  return request<{ id: string }>(`/api/drink-catalog/ingredients/`, {
    method: 'POST',
    body: JSON.stringify(input),
  });
}

export function updateIngredient(id: string, input: UpdateIngredientInput) {
  return request<void>(`/api/drink-catalog/ingredients/${id}/brands`, {
    method: 'PUT',
    body: JSON.stringify({ ...input, ingredientId: id }),
  });
}

export function deleteIngredient(id: string) {
  return request<void>(`/api/drink-catalog/ingredients/${id}`, {
    method: 'DELETE',
  });
}

export function startImport(input: StartImportInput) {
  return request<ImportBatchDto>(`/api/drink-catalog/imports/`, {
    method: 'POST',
    body: JSON.stringify(input),
  });
}

export function reviewImportBatch(id: string) {
  return request<ImportBatchDto>(`/api/drink-catalog/imports/${id}/review`, {
    method: 'POST',
  });
}

export function cancelImportBatch(id: string) {
  return request<ImportBatchDto>(`/api/drink-catalog/imports/${id}/cancel`, {
    method: 'POST',
  });
}

export function applyImportBatch(id: string, input: ApplyImportBatchInput) {
  return request<ImportBatchDto>(`/api/drink-catalog/imports/${id}/apply`, {
    method: 'POST',
    body: JSON.stringify(input),
  });
}

export function getImportBatch(id: string) {
  return request<ImportBatchDto>(`/api/drink-catalog/imports/${id}`);
}

export function listImportHistory() {
  return request<ImportBatchDto[]>(`/api/drink-catalog/imports/history`);
}

export function listAuditLogEntries() {
  return request<AuditLogEntryDto[]>(`/api/drink-catalog/audit-log`);
}
