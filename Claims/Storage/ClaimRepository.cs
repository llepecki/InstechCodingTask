using Claims.Domain;
using CSharpFunctionalExtensions;

namespace Claims.Storage;

public class ClaimRepository : IClaimRepository
{
    private readonly CosmosRepository<ClaimDbModel> _claimRepository;
    private readonly ICoverRepository _coverRepository;

    public ClaimRepository(
        CosmosRepository<ClaimDbModel> claimRepository,
        ICoverRepository coverRepository)
    {
        _claimRepository = claimRepository;
        _coverRepository = coverRepository;
    }

    public async Task<IReadOnlyCollection<Claim>> GetAll(CancellationToken cancellationToken)
    {
        var claims = await _claimRepository.GetItemsAsync(cancellationToken);

        Dictionary<string, Maybe<Cover>> covers = new Dictionary<string, Maybe<Cover>>(claims.Count);
        
        foreach (var claim in claims)
        {
            covers.Add(claim.CoverId, await _coverRepository.Get(Guid.Parse(claim.CoverId), cancellationToken));
        }
        
        return claims.Select(dbModel => new Claim
        {
            Id = Guid.Parse(dbModel.Id),
            Cover = covers[dbModel.CoverId].GetValueOrThrow("Cover not found"),
            Created = dbModel.Created,
            Name = dbModel.Name,
            Type = dbModel.Type,
            DamageCost = dbModel.DamageCost
        }).ToArray();
    }

    public async Task<Maybe<Claim>> Get(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetItemAsync(id.ToString(), cancellationToken);

        var cover = claim == null ?
            Maybe<Cover>.None :
            await _coverRepository.Get(Guid.Parse(claim.CoverId), cancellationToken);

        return claim == null ?
            Maybe<Claim>.None :
            Maybe<Claim>.From(new Claim
            {
                Id = Guid.Parse(claim.Id),
                Cover = cover.GetValueOrThrow("Cover not found"),
                Created = claim.Created,
                Name = claim.Name,
                Type = claim.Type,
                DamageCost = claim.DamageCost
            });
    }

    public async Task<Result<Claim, IReadOnlyDictionary<string, string>>> Upsert(Claim claim)
    {
        var addedClaim = await _claimRepository.UpsertItemAsync(claim.ToDbModel());
        var cover = await _coverRepository.Get(Guid.Parse(addedClaim.CoverId), CancellationToken.None);

        if (cover.HasNoValue)
        {
            return Result.Failure<Claim, IReadOnlyDictionary<string, string>>(new Dictionary<string, string> { { "cover", "Cover not found" } });
        }
        
        return Result.Success<Claim, IReadOnlyDictionary<string, string>>(new Claim
        {
            Id = Guid.Parse(addedClaim.Id),
            Cover = cover.Value,
            Created = addedClaim.Created,
            Name = addedClaim.Name,
            Type = addedClaim.Type,
            DamageCost = addedClaim.DamageCost
        });
    }

    public Task Delete(Claim cover)
    {
        return _claimRepository.DeleteItemAsync(cover.Id.ToString());
    }
}