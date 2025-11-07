namespace MinimalApiApp.Models;

public class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public int? ExpirationMinutes { get; set; }
}
