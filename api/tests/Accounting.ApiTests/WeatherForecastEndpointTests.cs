using System.Net;

namespace Accounting.ApiTests;

public sealed class WeatherForecastEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public WeatherForecastEndpointTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_UnknownRoute_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
