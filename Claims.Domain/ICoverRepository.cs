using CSharpFunctionalExtensions;

namespace Claims.Domain;

public interface ICoverRepository
{
    public Task<IReadOnlyCollection<Cover>> GetAll(CancellationToken cancellationToken);

    public Task<Maybe<Cover>> Get(Guid id, CancellationToken cancellationToken);

    public Task<Result<Cover, IReadOnlyDictionary<string, string>>> Upsert(Cover cover);

    public Task Delete(Cover cover);
}