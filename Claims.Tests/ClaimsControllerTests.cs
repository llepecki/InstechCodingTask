﻿using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests
    {
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
            
            var newCoverContent = new StringContent(JsonConvert.SerializeObject(newCover));

            var response = await client.PostAsync("/Covers", newCoverContent);
            var createdCover = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<CoverReadModel>();

            // Create claim
            var newClaim = new ClaimWriteModel(
                createdCover.Id,
                now.AddDays(30),
                "TestClaim",
                ClaimType.Fire,
                999);

            var newClaimContent = new StringContent(JsonConvert.SerializeObject(newClaim));

            response = await client.PostAsync("/Claims", newClaimContent);
            var createdClaim = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ClaimReadModel>();

            Assert.Equal("TestClaim", createdClaim.Name);
            Assert.Equal(ClaimType.Fire, createdClaim.Type);
            Assert.Equal(999, createdClaim.DamageCost);

            // Delete claim
            response = await client.DeleteAsync($"/Claims/{createdClaim.Id}");
            response = await client.GetAsync($"/Claims/{createdClaim.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
