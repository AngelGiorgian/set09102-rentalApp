using StarterApp.Models.Api;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request);

    Task<List<RentalSummaryDto>> GetOutgoingRentalsAsync();
    Task<List<RentalSummaryDto>> GetIncomingRentalsAsync();
}