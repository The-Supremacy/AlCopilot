using Mediator;

namespace AlCopilot.DrinkCatalog.Contracts.Commands;

public sealed record CreateTagCommand(string Name) : IRequest<Guid>;

public sealed record UpdateTagCommand(Guid TagId, string Name) : IRequest<bool>;

public sealed record DeleteTagCommand(Guid TagId) : IRequest<bool>;
