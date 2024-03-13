using System.Net.Mime;
using Claims.Auditing;
using Claims.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly CosmosRepository<ClaimDbModel> _claimsRepository;
        private readonly IAuditer _auditer;

        public ClaimsController(CosmosRepository<ClaimDbModel> claimsRepository, IAuditer auditer)
        {
            _claimsRepository = claimsRepository;
            _auditer = auditer;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Produces(MediaTypeNames.Application.Json, Type = typeof(IEnumerable<ClaimReadModel>))]
        public async Task<ActionResult<IEnumerable<ClaimReadModel>>> GetAll(CancellationToken cancellationToken)
        {
            var claims = await _claimsRepository.GetItemsAsync(cancellationToken);

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
            var claim = await _claimsRepository.GetItemAsync(id, cancellationToken);
            
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
            var claimToCreate = claim.ToDbModel(Guid.NewGuid());
            var createdClaim = await _claimsRepository.AddItemAsync(claimToCreate);
            _auditer.AuditClaim(createdClaim.Id, "POST");
            return CreatedAtAction(nameof(Get), new { id = createdClaim.Id }, ClaimReadModel.FromDbModel(createdClaim));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<NoContentResult> Delete(string id)
        {
            await _claimsRepository.DeleteItemAsync(id);
            _auditer.AuditClaim(id, "DELETE");
            return NoContent();
        }
    }
}