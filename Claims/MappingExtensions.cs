using Claims.Controllers;
using Claims.Domain;
using Claims.Storage;

namespace Claims;

public static class MappingExtensions
{
    public static CoverDbModel ToDbModel(this Cover cover) =>
        new CoverDbModel
        {
            Id = cover.Id.ToString(),
            StartDate = cover.StartDate,
            EndDate = cover.EndDate,
            Type = cover.Type,
            Premium = cover.Premium
        };

    public static ClaimDbModel ToDbModel(this Claim claim) =>
        new ClaimDbModel
        {
            Id = claim.Id.ToString(),
            CoverId = claim.Cover.Id.ToString(),
            Created = claim.Created,
            Name = claim.Name,
            DamageCost = claim.DamageCost
        };

    public static CoverReadModel ToReadModel(this Cover domainModel) =>
        new CoverReadModel(
            domainModel.Id,
            domainModel.StartDate,
            domainModel.EndDate,
            domainModel.Type,
            domainModel.Premium);

    public static ClaimReadModel ToReadModel(this Claim domainModel) =>
        new ClaimReadModel(
            domainModel.Id,
            domainModel.Cover.Id,
            domainModel.Created,
            domainModel.Name,
            domainModel.Type,
            domainModel.DamageCost);
}