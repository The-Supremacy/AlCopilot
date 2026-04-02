namespace AlCopilot.Shared.Models;

public sealed record PagedRequest(int Page = 1, int PageSize = 20);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
