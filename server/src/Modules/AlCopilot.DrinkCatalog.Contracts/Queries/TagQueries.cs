using AlCopilot.DrinkCatalog.Contracts.DTOs;
using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Queries;

public sealed record GetTagsQuery : IRequest<List<TagDto>>;
