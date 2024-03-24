using System.Net.Mime;
using Claims.Auditing;
using Claims.Domain;
using Claims.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimRepository _claimRepository;
    private readonly ICoverRepository _coverRepository;
    private readonly IAuditer _auditer;

    public ClaimsController(IClaimRepository claimRepository, ICoverRepository coverRepository, IAuditer auditer)
    {
        _claimRepository = claimRepository;
        _coverRepository = coverRepository;
        _auditer = auditer;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<ClaimReadModel>))]
    public async Task<ActionResult<IEnumerable<ClaimReadModel>>> GetAll(CancellationToken cancellationToken)
    {
        var claims = await _claimRepository.GetAll(cancellationToken);

        if (claims.Count == 0)
        {
            return NoContent();
        }

        return Ok(claims.Select(claim => claim.ToReadModel()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(ClaimReadModel))]
    public async Task<ActionResult<ClaimReadModel>> Get(Guid id, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.Get(id, cancellationToken);
        
        if (claim.HasNoValue)
        {
            return NotFound();
        }

        return Ok(claim.Value.ToReadModel());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(ClaimReadModel))]
    public async Task<ActionResult<ClaimReadModel>> Create(ClaimWriteModel claimWriteModel)
    {
        var coverResult = await _coverRepository.Get(claimWriteModel.CoverId, CancellationToken.None);

        if (coverResult.HasNoValue)
        {
            ModelState.AddModelError(nameof(claimWriteModel.CoverId), "Cover not found");
            return BadRequest(ModelState);
        }

        var createClaimResult = Claim.Create(
            coverResult.Value,
            claimWriteModel.Created,
            claimWriteModel.Name,
            claimWriteModel.Type,
            claimWriteModel.DamageCost);

        if (createClaimResult.IsFailure)
        {
            ModelState.AddErrors(createClaimResult.Error);
            return BadRequest(ModelState);
        }

        var claimToCreate = createClaimResult.Value;

        try
        {
            _auditer.AuditClaim(claimToCreate.Id, RequestType.Post, RequestStage.Started);
            var createdClaimResult = await _claimRepository.Upsert(claimToCreate);
            
            if (createdClaimResult.IsFailure)
            {
                ModelState.AddErrors(createdClaimResult.Error);
                _auditer.AuditCover(claimToCreate.Id, RequestType.Post, RequestStage.Failed);
                return BadRequest(ModelState);
            }
            
            _auditer.AuditClaim(claimToCreate.Id, RequestType.Post, RequestStage.Suceeded);
            return CreatedAtAction(nameof(Get), new { id = createdClaimResult.Value.Id }, createClaimResult.Value.ToReadModel());
        }
        catch
        {
             _auditer.AuditClaim(claimToCreate.Id, RequestType.Post, RequestStage.Failed);
            throw;
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> Delete(Guid id)
    {
        try
        {
            _auditer.AuditClaim(id, RequestType.Delete, RequestStage.Started);
            var claim = await _claimRepository.Get(id, CancellationToken.None);
            
            if (claim.HasValue)
            {
                await _claimRepository.Delete(claim.Value);
            }

            _auditer.AuditClaim(id, RequestType.Delete, RequestStage.Suceeded);
        }
        catch
        {
             _auditer.AuditClaim(id, RequestType.Delete, RequestStage.Failed);
            throw;
        }

        return NoContent();
    }
}
