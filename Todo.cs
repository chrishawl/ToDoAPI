using System.Text.Json.Serialization;

public class Todo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; }
}