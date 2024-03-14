using System.Text.Json.Serialization;
using Claims.Auditing;
using Claims.Services;
using Claims.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

string account = builder.Configuration.GetSection("CosmosDB").GetValue<string>("Account") ?? throw new ArgumentNullException("CosmosDB Account is not configured");
string key = builder.Configuration.GetSection("CosmosDB").GetValue<string>("Key") ?? throw new ArgumentNullException("CosmosDB Key is not configured");
string databaseName = builder.Configuration.GetSection("CosmosDB").GetValue<string>("DatabaseName") ?? throw new ArgumentNullException("CosmosDB DatabaseName is not configured");

builder.Services
    .AddSingleton(new CosmosClient(account, key))
    .AddSingleton(p => new CosmosRepository<ClaimDbModel>(p.GetRequiredService<CosmosClient>(), databaseName, "Claim"))
    .AddSingleton(p => new CosmosRepository<CoverDbModel>(p.GetRequiredService<CosmosClient>(), databaseName, "Cover"));

builder.Services.AddDbContext<AuditContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IComputePremium, ComputePremium>();

builder.Services
    .AddHostedService<AuditWorker>()
    .AddSingleton<Auditer>()
    .AddSingleton<IAuditer>(p => p.GetRequiredService<Auditer>())
    .AddSingleton<IAuditReader>(p => p.GetRequiredService<Auditer>());

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    await context.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var cosmosClient = scope.ServiceProvider.GetRequiredService<CosmosClient>();
    var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync("Claim", "/id");
    await database.Database.CreateContainerIfNotExistsAsync("Cover", "/id");
}

await app.RunAsync();

public partial class Program { }