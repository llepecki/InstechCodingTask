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
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

        context.ClaimAudits.AddRange(_auditReader.ReadClaimAudits());
        context.CoverAudits.AddRange(_auditReader.ReadCoverAudits());

        await context.SaveChangesAsync();
    }
}
