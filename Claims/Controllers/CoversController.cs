using System.Globalization;
using System.Net.Mime;
using Claims.Auditing;
using Claims.Services;
using Claims.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly CosmosRepository<CoverDbModel> _coverRepository;
    private readonly IComputePremium _computePremium;
    private readonly IAuditer _auditer;

    public CoversController(CosmosRepository<CoverDbModel> coverRepository, IComputePremium computePremium, IAuditer auditer)
    {
        _coverRepository = coverRepository;
        _computePremium = computePremium;
        _auditer = auditer;
    }

    [HttpGet("premium")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ObjectResult ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        var premium = _computePremium.Compute(startDate, endDate, coverType);
        return Ok(new { premium = premium.ToString(CultureInfo.InvariantCulture) });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<ClaimReadModel>))]
    public async Task<ActionResult<IEnumerable<CoverReadModel>>> GetAll(CancellationToken cancellationToken)
    {
        var claims = await _coverRepository.GetItemsAsync(cancellationToken);

        if (claims.Count == 0)
        {
            return NoContent();
        }

        return Ok(claims.Select(CoverReadModel.FromDbModel));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(CoverReadModel))]
    public async Task<ActionResult<CoverReadModel>> Get(string id, CancellationToken cancellationToken)
    {
        var claim = await _coverRepository.GetItemAsync(id, cancellationToken);
        
        if (claim == null)
        {
            return NotFound();
        }

        return Ok(CoverReadModel.FromDbModel(claim));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(CoverReadModel))]
    public async Task<ActionResult> Create(CoverWriteModel cover)
    {
        var coverToCreate = cover.ToDbModel(Guid.NewGuid(), _computePremium.Compute(cover.StartDate, cover.EndDate, cover.Type));
        _auditer.AuditCover(coverToCreate.Id, "POST");
        var createdCover = await _coverRepository.AddItemAsync(coverToCreate);
        // TODO: Implement audit rollback in case of Cosmos failure

        return Ok(CoverReadModel.FromDbModel(createdCover));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> Delete(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        await _coverRepository.DeleteItemAsync(id);
        // TODO: Implement audit rollback in case of Cosmos failure

        return NoContent();
    }
}