using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Claims.Storage;

public interface ICosmosEntity
{
    [JsonProperty(PropertyName = "id")]
    string Id { get; init; }
}

public class CosmosRepository<T> where T : class, ICosmosEntity
{
    private readonly Container _container;

    public CosmosRepository(
        CosmosClient dbClient,
        string databaseName,
        string containerName)
    {
        if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));
        _container = dbClient.GetContainer(databaseName, containerName);
    }

    public async Task<IReadOnlyCollection<T>> GetItemsAsync(CancellationToken cancellationToken)
    {
        var query = _container.GetItemQueryIterator<T>(new QueryDefinition("SELECT * FROM c"));
        var results = new List<T>();

        while (query.HasMoreResults)
        {
            results.AddRange(await query.ReadNextAsync(cancellationToken));
        }

        return results;
    }

    public async Task<T?> GetItemAsync(string id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id), null, cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<T> UpsertItemAsync(T item)
    {
        var response = await _container.CreateItemAsync(item, new PartitionKey(item.Id));
        return response.Resource;
    }

    public Task DeleteItemAsync(string id)
    {
        return _container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }
}