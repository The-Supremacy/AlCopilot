using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class CreateTagHandler(
    ITagRepository tagRepository,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTagCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var name = TagName.Create(request.Name);

        if (await tagRepository.ExistsByNameAsync(name, cancellationToken: cancellationToken))
            throw new ConflictException($"A tag with the name '{name.Value}' already exists.");

        var tag = Tag.Create(name);
        tagRepository.Add(tag);
        auditLogWriter.Write("tag.create", "tag", tag.Id.ToString(), $"Created tag '{tag.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }
}
