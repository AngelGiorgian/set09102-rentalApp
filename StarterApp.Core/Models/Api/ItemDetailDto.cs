using System.Text.Json.Serialization;

namespace StarterApp.Models.Api;

//item detail api model
public sealed class ItemDetailDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("dailyRate")]
    public decimal DailyRate { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [JsonPropertyName("ownerId")]
    public int OwnerId { get; set; }

    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; } = string.Empty;

    [JsonPropertyName("ownerRating")]
    public double? OwnerRating { get; set; }

    [JsonPropertyName("averageRating")]
    public double? AverageRating { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    //formated daily prices, availability, ratings and owner ratings
    public string DailyRateText => $"£{DailyRate:0.00}/day";
    public string AvailabilityText => IsAvailable ? "Available" : "Unavailable";
    public string RatingText => AverageRating.HasValue ? $"★ {AverageRating.Value:0.0}" : "No ratings";
    public string OwnerRatingText => OwnerRating.HasValue ? $"★ {OwnerRating.Value:0.0}" : "No owner rating";
}