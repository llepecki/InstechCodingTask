namespace Claims.Auditing;

public record CoverAudit
{
    public int Id { get; init; } = 0;
    public Guid CoverId { get; init; }
    public DateTime Created { get; init; }
    public string? HttpRequestType { get; init; }
}