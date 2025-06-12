using System.Text.Json.Serialization;

public class Todo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}