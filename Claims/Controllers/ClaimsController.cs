using System.Net.Mime;
using Claims.Auditing;
using Claims.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly CosmosRepository<ClaimDbModel> _claimRepository;
    private readonly CosmosRepository<CoverDbModel> _coverRepository;
    private readonly IAuditer _auditer;

    public ClaimsController(
        CosmosRepository<ClaimDbModel> claimRepository,
        CosmosRepository<CoverDbModel> coverRepository,
        IAuditer auditer)
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
        var claims = await _claimRepository.GetItemsAsync(cancellationToken);

        if (claims.Count == 0)
        {
            return NoContent();
        }

        return Ok(claims.Select(ClaimReadModel.FromDbModel));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(ClaimReadModel))]
    public async Task<ActionResult<ClaimReadModel>> Get(string id, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetItemAsync(id, cancellationToken);
        
        if (claim == null)
        {
            return NotFound();
        }

        return Ok(ClaimReadModel.FromDbModel(claim));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces(MediaTypeNames.Application.Json, Type = typeof(ClaimReadModel))]
    public async Task<ActionResult<ClaimReadModel>> Create(ClaimWriteModel claim)
    {
        await ValidateClaimAgainstCover(claim);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var claimToCreate = claim.ToDbModel(Guid.NewGuid());
        _auditer.AuditClaim(claimToCreate.Id, "POST");
        var createdClaim = await _claimRepository.AddItemAsync(claimToCreate);
        // TODO: Implement audit rollback in case of Cosmos failure
        
        return CreatedAtAction(nameof(Get), new { id = createdClaim.Id }, ClaimReadModel.FromDbModel(createdClaim));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<NoContentResult> Delete(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        await _claimRepository.DeleteItemAsync(id);
        // TODO: Implement audit rollback in case of Cosmos failure

        return NoContent();
    }

    private async Task ValidateClaimAgainstCover(ClaimWriteModel claim)
    {
        var cover = await _coverRepository.GetItemAsync(claim.CoverId, CancellationToken.None);

        if (cover == null)
        {
            ModelState.AddModelError(nameof(claim.CoverId), "Cover not found");
            return;
        }

        DateTime coverPeriodStart = cover.StartDate.ToDateTime(TimeOnly.MinValue);
        DateTime coverPeriodEnd = cover.EndDate.ToDateTime(TimeOnly.MaxValue);

        if (claim.Created < coverPeriodStart || claim.Created > coverPeriodEnd)
        {
            ModelState.AddModelError(nameof(claim.Created), $"Claim date is not within cover period (from {coverPeriodStart} to {coverPeriodEnd})");
        }
    }
}
