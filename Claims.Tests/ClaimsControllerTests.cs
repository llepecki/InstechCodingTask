using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests;

public class ClaimsControllerTests
{
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions();

    public ClaimsControllerTests()
    {
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task Get_Claims()
    {
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(_ =>
            {});

        var client = application.CreateClient();

        var response = await client.GetAsync("/Claims");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_Get_Delete_Claim()
    {
        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(_ => {});
        var client = application.CreateClient();

        var now = DateTime.UtcNow;

        // Create cover
        var newCover = new CoverWriteModel(
            DateOnly.FromDateTime(now),
            DateOnly.FromDateTime(now.AddDays(90)),
            CoverType.Yacht);
        
        var newCoverContent = new StringContent(JsonSerializer.Serialize(newCover, _serializerOptions), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/Covers", newCoverContent);
        var createdCover = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<CoverReadModel>(_serializerOptions);

        // Create claim
        var newClaim = new ClaimWriteModel(
            createdCover!.Id,
            now.AddDays(30),
            "TestClaim",
            ClaimType.Fire,
            999);

        var newClaimContent = new StringContent(JsonSerializer.Serialize(newClaim, _serializerOptions), Encoding.UTF8, "application/json");

        response = await client.PostAsync("/Claims", newClaimContent);
        var createdClaim = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ClaimReadModel>(_serializerOptions);

        Assert.Equal("TestClaim", createdClaim!.Name);
        Assert.Equal(ClaimType.Fire, createdClaim.Type);
        Assert.Equal(999, createdClaim.DamageCost);

        // Delete claim
        _ = await client.DeleteAsync($"/Claims/{createdClaim.Id}");
        response = await client.GetAsync($"/Claims/{createdClaim.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // TODO: Write more tests:
    // Test BadRequest when validation should fail
    // Test getting multiple claims
    // Test saving audits

    // TODO: Implement db cleanup using IAsyncLifetime
}
