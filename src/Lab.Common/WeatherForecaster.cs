namespace Lab.Common;

public record WeatherForecast(DateOnly Date, string City, int TemperatureC, int TemperatureF, string Summary);

/// <summary>Produces a fake but repeatable forecast, so tests can assert on it.</summary>
public static class WeatherForecaster
{
    public const int MinTemperatureC = -10;
    public const int MaxTemperatureC = 35;

    public static IReadOnlyList<WeatherForecast> Forecast(string city, int days, DateOnly startDate)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (days is < 1 or > 14)
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 14.");

        var random = new Random(StableSeed(city));
        var result = new List<WeatherForecast>(days);
        for (var i = 0; i < days; i++)
        {
            var c = random.Next(MinTemperatureC, MaxTemperatureC + 1);
            var f = (int)Math.Round(TemperatureConverter.CelsiusToFahrenheit(c));
            result.Add(new WeatherForecast(startDate.AddDays(i), city, c, f, TemperatureConverter.Describe(c)));
        }
        return result;
    }

    // string.GetHashCode() differs per process, so use a simple stable hash instead.
    private static int StableSeed(string text)
    {
        unchecked
        {
            var hash = 17;
            foreach (var ch in text.ToLowerInvariant())
                hash = hash * 31 + ch;
            return hash;
        }
    }
}
