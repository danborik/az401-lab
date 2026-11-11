using System.Net;
using System.Net.Http.Json;
using Lab.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

// Integration tests: start the real API in memory and call it over HTTP.
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Greet_ReturnsGreetingWithName()
    {
        var body = await _client.GetStringAsync("/greet?name=Dan");
        Assert.Contains("Hello, Dan!", body);
    }

    [Fact]
    public async Task Version_ReturnsOk()
    {
        var response = await _client.GetAsync("/version");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Weather_ReturnsFiveDaysByDefault()
    {
        var forecast = await _client.GetFromJsonAsync<List<WeatherForecast>>("/weather?city=Prague");
        Assert.NotNull(forecast);
        Assert.Equal(5, forecast!.Count);
    }

    [Fact]
    public async Task Weather_InvalidDays_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/weather?days=99");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Error_Returns500()
    {
        var response = await _client.GetAsync("/error");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
    
    [Fact]
    public async Task Time_ReturnsOk()
    {
        var response = await _client.GetAsync("/time");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("utc", await response.Content.ReadAsStringAsync());
    }    
}
