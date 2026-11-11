using Lab.Common;

namespace Api.Tests;

public class WeatherForecasterTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);

    [Fact]
    public void Forecast_ReturnsRequestedNumberOfDays() =>
        Assert.Equal(7, WeatherForecaster.Forecast("Prague", 7, Start).Count);

    [Fact]
    public void Forecast_IsRepeatableForSameCity() =>
        Assert.Equal(
            WeatherForecaster.Forecast("Prague", 5, Start),
            WeatherForecaster.Forecast("Prague", 5, Start));

    [Fact]
    public void Forecast_TemperaturesStayInRange() =>
        Assert.All(WeatherForecaster.Forecast("Vienna", 14, Start), f =>
            Assert.InRange(f.TemperatureC, WeatherForecaster.MinTemperatureC, WeatherForecaster.MaxTemperatureC));

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Forecast_RejectsInvalidDays(int days) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => WeatherForecaster.Forecast("Prague", days, Start));

    [Fact]
    public void Forecast_RejectsEmptyCity() =>
        Assert.Throws<ArgumentException>(() => WeatherForecaster.Forecast(" ", 3, Start));
}
