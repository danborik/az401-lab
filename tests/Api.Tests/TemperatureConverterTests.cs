using Lab.Common;

namespace Api.Tests;

public class TemperatureConverterTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    public void CelsiusToFahrenheit_ConvertsKnownValues(double c, double f) =>
        Assert.Equal(f, TemperatureConverter.CelsiusToFahrenheit(c), precision: 6);

    [Theory]
    [InlineData(32, 0)]
    [InlineData(212, 100)]
    public void FahrenheitToCelsius_ConvertsKnownValues(double f, double c) =>
        Assert.Equal(c, TemperatureConverter.FahrenheitToCelsius(f), precision: 6);

    [Theory]
    [InlineData(-5, "Freezing")]
    [InlineData(5, "Cold")]
    [InlineData(15, "Mild")]
    [InlineData(25, "Warm")]
    [InlineData(35, "Hot")]
    public void Describe_ReturnsBand(double c, string expected) =>
        Assert.Equal(expected, TemperatureConverter.Describe(c));
}
