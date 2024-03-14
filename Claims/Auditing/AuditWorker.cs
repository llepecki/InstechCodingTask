namespace Claims.Auditing;

public class AuditWorker : BackgroundService
{
    private static readonly TimeSpan NapTime = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IAuditReader _auditReader;

    public AuditWorker(IServiceScopeFactory serviceScopeFactory, IAuditReader auditReader)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _auditReader = auditReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FlushAudits();
                await Task.Delay(NapTime, stoppingToken);
            }
        }
        finally
        {
            await FlushAudits();
        }
    }

    private async Task FlushAudits()
    {
        var claimAudits = _auditReader.ReadClaimAudits();
        var coverAudits = _auditReader.ReadCoverAudits();

        if (claimAudits.Count > 0 && coverAudits.Count > 0)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

            context.ClaimAudits.AddRange(claimAudits);
            context.CoverAudits.AddRange(coverAudits);

            await context.SaveChangesAsync();
        }
    }
}
