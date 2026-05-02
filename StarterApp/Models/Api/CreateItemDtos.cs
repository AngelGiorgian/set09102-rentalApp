using System.Text.Json.Serialization;

namespace StarterApp.Models.Api;

public sealed class CreateItemRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("dailyRate")]
    public decimal DailyRate { get; set; }

    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

public sealed class CreateItemResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}