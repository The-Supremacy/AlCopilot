using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class DeleteTagHandler(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTagCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByIdAsync(request.TagId, cancellationToken);
        if (tag is null) return false;

        if (await tagRepository.IsReferencedByDrinksAsync(request.TagId, cancellationToken))
            throw new InvalidOperationException($"Tag '{tag.Name.Value}' is referenced by active drinks and cannot be deleted.");

        tagRepository.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
