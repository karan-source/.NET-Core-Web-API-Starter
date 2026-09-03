using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProductionApi.Api.IntegrationTests;

public sealed class ProductsEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetProducts_ReturnsSeededCatalogue()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products?pageNumber=1&pageSize=10");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.True(root.GetProperty("totalCount").GetInt32() >= 4);
        Assert.NotEmpty(root.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task GetProducts_WithSearchTerm_FiltersResults()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products?search=Standing");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("items").EnumerateArray().ToList();

        Assert.Single(items);
        Assert.Equal("Standing Desk", items[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetProducts_WithInvalidPageSize_ReturnsValidationProblem()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products?pageSize=5000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task GetProductById_WithUnknownId_ReturnsNotFound()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutToken_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name = "Unauthorised Item",
            price = 10.00m,
            stockQuantity = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithToken_PersistsAndIsRetrievable()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/products", new
        {
            name = "Ergonomic Chair",
            description = "Adjustable lumbar support.",
            price = 349.00m,
            stockQuantity = 12
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        var getResponse = await client.GetAsync($"/api/products/{id}");
        getResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        Assert.Equal("Ergonomic Chair", document.RootElement.GetProperty("name").GetString());
        Assert.Equal(349.00m, document.RootElement.GetProperty("price").GetDecimal());
    }

    [Fact]
    public async Task CreateProduct_WithInvalidPayload_ReturnsValidationProblem()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name = "",
            price = -5.00m,
            stockQuantity = -1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty("Name", out _));
        Assert.True(errors.TryGetProperty("Price", out _));
    }

    [Fact]
    public async Task DeleteProduct_WithUnknownId_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();

        var tokenResponse = await client.PostAsJsonAsync("/api/auth/dev-token", new { email = "demo@example.com" });
        tokenResponse.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        var accessToken = document.RootElement.GetProperty("accessToken").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
