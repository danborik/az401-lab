namespace Lab.Common;

/// <summary>Builds greetings. The "new greeting" variant is switched by a feature flag (lab 3.10).</summary>
public class GreetingService
{
    public string Greet(string? name, bool useNewGreeting = false)
    {
        var who = string.IsNullOrWhiteSpace(name) ? "world" : name.Trim();
        return useNewGreeting
            ? $"Hello v2, {who}! You are seeing the new greeting."
            : $"Hello, {who}!";
    }
}
