using System.Text.Json.Serialization;

namespace StarterApp.Models.Api;

//category api model
public sealed class CategoryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    public string DisplayName => Name; //shown in category pickker
}