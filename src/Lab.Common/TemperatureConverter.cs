namespace Lab.Common;

public static class TemperatureConverter
{
    public static double CelsiusToFahrenheit(double celsius) => celsius * 9 / 5 + 32;

    public static double FahrenheitToCelsius(double fahrenheit) => (fahrenheit - 32) * 5 / 9;

    public static string Describe(double celsius) => celsius switch
    {
        < 0 => "Freezing",
        < 10 => "Cold",
        < 20 => "Mild",
        < 30 => "Warm",
        _ => "Hot"
    };
}
