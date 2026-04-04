using AlCopilot.DrinkCatalog.Contracts.Commands;
using AlCopilot.Shared.Data;
using Mediator;

namespace AlCopilot.DrinkCatalog.Features.Tag;

public sealed class CreateTagHandler(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTagCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var name = TagName.Create(request.Name);

        if (await tagRepository.ExistsByNameAsync(name, cancellationToken))
            throw new InvalidOperationException($"A tag with the name '{name.Value}' already exists.");

        var tag = Tag.Create(name);
        tagRepository.Add(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }
}
