using Lab.Common;

namespace Api.Tests;

public class GreetingServiceTests
{
    private readonly GreetingService _sut = new();

    [Fact]
    public void Greet_WithoutName_GreetsWorld() =>
        Assert.Equal("Hello, world!", _sut.Greet(null));

    [Fact]
    public void Greet_TrimsName() =>
        Assert.Equal("Hello, Dan!", _sut.Greet("  Dan  "));

    [Fact]
    public void Greet_NewGreeting_UsesV2Text() =>
        Assert.StartsWith("Hello v2, Dan!", _sut.Greet("Dan", useNewGreeting: true));
}
