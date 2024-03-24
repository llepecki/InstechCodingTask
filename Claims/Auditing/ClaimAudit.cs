namespace Claims.Auditing;

public record ClaimAudit
{
    public int Id { get; init; } = 0;
    public Guid ClaimId { get; init; }
    public DateTime Created { get; init; }
    public string? HttpRequestType { get; init; }
}