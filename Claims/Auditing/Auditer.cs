using System.Collections.Concurrent;

namespace Claims.Auditing;

public interface IAuditer
{
    void AuditClaim(string id, string httpRequestType);

    void AuditCover(string id, string httpRequestType);
}

public interface IAuditReader
{
    IReadOnlyCollection<ClaimAudit> ReadClaimAudits();

    IReadOnlyCollection<CoverAudit> ReadCoverAudits();
}

public class Auditer : IAuditer, IAuditReader
{
    private readonly ConcurrentQueue<ClaimAudit> _claimAudits = new();
    private readonly ConcurrentQueue<CoverAudit> _coverAudits = new();

    public void AuditClaim(string id, string httpRequestType) => _claimAudits.Enqueue(new ClaimAudit
    {
        ClaimId = id,
        Created = DateTime.UtcNow,
        HttpRequestType = httpRequestType
    });

    public void AuditCover(string id, string httpRequestType) => _coverAudits.Enqueue(new CoverAudit
    {
        CoverId = id,
        Created = DateTime.UtcNow,
        HttpRequestType = httpRequestType
    });

    public IReadOnlyCollection<ClaimAudit> ReadClaimAudits()
    {
        IEnumerable<ClaimAudit> GetClaimAuditsInner()
        {
            while (_claimAudits.TryDequeue(out var audit))
            {
                yield return audit;
            }
        }

        return GetClaimAuditsInner().ToArray();
    }

    public IReadOnlyCollection<CoverAudit> ReadCoverAudits()
    {
        IEnumerable<CoverAudit> GetCoverAuditsInner()
        {
            while (_coverAudits.TryDequeue(out var audit))
            {
                yield return audit;
            }
        }

        return GetCoverAuditsInner().ToArray();
    }
}
