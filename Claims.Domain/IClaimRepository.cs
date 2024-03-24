using CSharpFunctionalExtensions;

namespace Claims.Domain;

public interface IClaimRepository
{
    public Task<IReadOnlyCollection<Claim>> GetAll(CancellationToken cancellationToken);

    public Task<Maybe<Claim>> Get(Guid id, CancellationToken cancellationToken);

    public Task<Result<Claim, IReadOnlyDictionary<string, string>>> Upsert(Claim claim);

    public Task Delete(Claim cover);
}