using System.Collections.Concurrent;

namespace Claims.Auditing;

public enum RequestType
{
    Post,
    Delete
}

public enum RequestStage
{
    Started,
    Suceeded,
    Failed
}

public interface IAuditer
{
    void AuditClaim(Guid id, RequestType requestType, RequestStage requestStage);

    void AuditCover(Guid id, RequestType requestType, RequestStage requestStage);
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

    public void AuditClaim(Guid id, RequestType requestType, RequestStage requestStage) => _claimAudits.Enqueue(new ClaimAudit
    {
        ClaimId = id,
        Created = DateTime.UtcNow,
        HttpRequestType = ToHttpRequestType(requestType, requestStage)
    });

    public void AuditCover(Guid id, RequestType requestType, RequestStage requestStage) => _coverAudits.Enqueue(new CoverAudit
    {
        CoverId = id,
        Created = DateTime.UtcNow,
        HttpRequestType = ToHttpRequestType(requestType, requestStage)
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

    private static string ToHttpRequestType(RequestType requestType, RequestStage requestStage) =>
        $"{requestType.ToString("g").ToUpper()}_{requestStage.ToString("g").ToUpper()}";
}
