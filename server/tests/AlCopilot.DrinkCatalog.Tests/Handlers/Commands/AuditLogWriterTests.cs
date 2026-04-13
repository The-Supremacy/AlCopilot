using AlCopilot.DrinkCatalog.Features.Audit;
using NSubstitute;

namespace AlCopilot.DrinkCatalog.Tests.Handlers.Commands;

public sealed class AuditLogWriterTests
{
    private readonly IAuditLogEntryRepository _repository = Substitute.For<IAuditLogEntryRepository>();

    [Fact]
    public void Write_WithoutAuthenticatedActor_UsesAnonymousDisplayNameAndNoUserId()
    {
        var writer = new AuditLogWriter(_repository);

        writer.Write("tag.create", "tag", "classic", "Created tag.");

        _repository.Received(1).Add(Arg.Is<AuditLogEntry>(entry =>
            entry.Actor == "anonymous" &&
            entry.ActorUserId == null));
    }
}
