using Claims.Domain;
using CSharpFunctionalExtensions;

namespace Claims.Storage;

public class CoverRepository : ICoverRepository
{
    private readonly CosmosRepository<CoverDbModel> _coverRepository;

    public CoverRepository(CosmosRepository<CoverDbModel> coverRepository)
    {
        _coverRepository = coverRepository;
    }

    public async Task<IReadOnlyCollection<Cover>> GetAll(CancellationToken cancellationToken)
    {
        var covers = await _coverRepository.GetItemsAsync(cancellationToken);

        return covers.Select(dbModel => new Cover
            {
                Id = Guid.Parse(dbModel.Id),
                StartDate = dbModel.StartDate,
                EndDate = dbModel.EndDate,
                Type = dbModel.Type,
                Premium = dbModel.Premium
            })
            .ToArray();
    }

    public async Task<Maybe<Cover>> Get(Guid id, CancellationToken cancellationToken)
    {
        var cover = await _coverRepository.GetItemAsync(id.ToString(), cancellationToken);

        return cover == null ?
            Maybe<Cover>.None :
            Maybe<Cover>.From(new Cover
            {
                Id = Guid.Parse(cover.Id),
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,
                Type = cover.Type,
                Premium = cover.Premium
            });
    }

    public async Task<Result<Cover, IReadOnlyDictionary<string, string>>> Upsert(Cover cover)
    {
        var addedCover = await _coverRepository.UpsertItemAsync(cover.ToDbModel());
        return Result.Success<Cover, IReadOnlyDictionary<string, string>>(new Cover
        {
            Id = Guid.Parse(addedCover.Id),
            StartDate = addedCover.StartDate,
            EndDate = addedCover.EndDate,
            Type = addedCover.Type,
            Premium = addedCover.Premium
        });
    }

    public Task Delete(Cover cover)
    {
        return _coverRepository.DeleteItemAsync(cover.Id.ToString());
    }
}