using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.DrinkCatalog.Features.Audit;
using AlCopilot.Shared.Data;
using AlCopilot.Shared.Errors;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class DeleteTagHandler(
    ITagRepository tagRepository,
    AuditLogWriter auditLogWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTagCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null) return false;

        if (await tagRepository.IsReferencedByDrinksAsync(request.TagId, cancellationToken))
            throw new ConflictException($"Tag '{tag.Name.Value}' is referenced by active drinks and cannot be deleted.");

        tagRepository.Remove(tag);
        auditLogWriter.Write("tag.delete", "tag", tag.Id.ToString(), $"Deleted tag '{tag.Name.Value}'.");
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
