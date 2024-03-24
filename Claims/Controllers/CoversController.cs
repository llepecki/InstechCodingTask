using System.Globalization;
using System.Net.Mime;
using Claims.Auditing;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoverRepository _coverRepository;
    private readonly IAuditer _auditer;

    public CoversController(ICoverRepository coverRepository, IAuditer auditer)
    {
        _coverRepository = coverRepository;
        _auditer = auditer;
    }

    [HttpGet("premium")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ObjectResult ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        var premium = Domain.ComputePremium.Compute(startDate, endDate, coverType);
        return Ok(new { premium = premium.ToString(CultureInfo.InvariantCulture) });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<ClaimReadModel>))]
    public async Task<ActionResult<IEnumerable<CoverReadModel>>> GetAll(CancellationToken cancellationToken)
    {
        var covers = await _coverRepository.GetAll(cancellationToken);

        if (covers.Count == 0)
        {
            return NoContent();
        }

        return Ok(covers.Select(claim => claim.ToReadModel()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(CoverReadModel))]
    public async Task<ActionResult<CoverReadModel>> Get(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _coverRepository.Get(id, cancellationToken);
        
        if (claim.HasNoValue)
        {
            return NotFound();
        }

        return Ok(claim.Value.ToReadModel());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(CoverReadModel))]
    public async Task<ActionResult> Create(CoverWriteModel coverWriteModel)
    {
        var createCoverResult = Cover.Create(
            coverWriteModel.StartDate,
            coverWriteModel.EndDate,
            coverWriteModel.Type);

        if (createCoverResult.IsFailure)
        {
            ModelState.AddErrors(createCoverResult.Error);
            return BadRequest(ModelState);
        }

        var coverToCreate = createCoverResult.Value;

        try
        {
            _auditer.AuditCover(coverToCreate.Id, RequestType.Post, RequestStage.Started);
            var createdCoverResult = await _coverRepository.Upsert(coverToCreate);

            if (createdCoverResult.IsFailure)
            {
                ModelState.AddErrors(createdCoverResult.Error);
                _auditer.AuditCover(coverToCreate.Id, RequestType.Post, RequestStage.Failed);
                return BadRequest(ModelState);
            }
            
            _auditer.AuditCover(coverToCreate.Id, RequestType.Post, RequestStage.Suceeded);
            return CreatedAtAction(nameof(Get), new { id = createdCoverResult.Value.Id }, createdCoverResult.Value.ToReadModel());
        }
        catch
        {
            _auditer.AuditCover(coverToCreate.Id, RequestType.Post, RequestStage.Failed);
            throw;
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> Delete(Guid id)
    {
        try
        {
            _auditer.AuditCover(id, RequestType.Delete, RequestStage.Started);
            var cover = await _coverRepository.Get(id, CancellationToken.None);
            
            if (cover.HasValue)
            {
                await _coverRepository.Delete(cover.Value);
            }

            _auditer.AuditCover(id, RequestType.Delete, RequestStage.Suceeded);
            return NoContent();
        }
        catch
        {
            _auditer.AuditCover(id, RequestType.Delete, RequestStage.Failed);
            throw;
        }
    }
}