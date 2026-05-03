using System.Globalization;
using System.Text.Json.Serialization;

namespace StarterApp.Models.Api;

public sealed class CreateRentalRequest
{
    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = string.Empty;
}

public sealed class UpdateRentalStatusRequest
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public sealed class RentalSummaryDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }

    public string ItemTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? RequestedAt { get; set; }

    public decimal? TotalPrice { get; set; }

    public string BorrowerName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;

    public string StartDateText => StartDate?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "N/A";
    public string EndDateText => EndDate?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "N/A";
    public string RequestedAtText => RequestedAt?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) ?? "N/A";
    public string DateRangeText => $"{StartDateText} - {EndDateText}";
    public string TotalPriceText => TotalPrice.HasValue ? $"£{TotalPrice.Value:0.00}" : "Price not available";
    public string StatusText => string.IsNullOrWhiteSpace(Status) ? "Unknown" : Status;

    public bool IsRequested => MatchesStatus("Requested");
    public bool IsApproved => MatchesStatus("Approved");
    public bool IsOutForRent => MatchesStatus("Out for Rent") || MatchesStatus("OutForRent");
    public bool IsReturned => MatchesStatus("Returned");

    private bool MatchesStatus(string expected)
    {
        return string.Equals(
            NormalizeStatus(Status),
            NormalizeStatus(expected),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeStatus(string value)
    {
        return value.Replace(" ", string.Empty).Trim();
    }
}