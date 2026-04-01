namespace AlCopilot.DrinkCatalog.Contracts.DTOs;

public sealed record PagedRequest(int Page = 1, int PageSize = 20);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
