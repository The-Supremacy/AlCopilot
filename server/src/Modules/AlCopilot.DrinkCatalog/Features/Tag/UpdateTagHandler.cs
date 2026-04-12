using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class UpdateTagHandler(
    ITagRepository tagRepository,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTagCommand, bool>
{
    public async ValueTask<bool> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null)
            return false;

        var name = TagName.Create(request.Name);

        if (await tagRepository.ExistsByNameAsync(name, request.TagId, cancellationToken))
            throw new ConflictException($"A tag with the name '{name.Value}' already exists.");

        tag.Rename(name);
        auditLogWriter.Write("tag.update", "tag", tag.Id.ToString(), $"Updated tag '{tag.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
