using StarterApp.Models.Api;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request);
    Task<(bool IsSuccess, string Message)> UpdateRentalStatusAsync(int rentalId, string status);

    Task<List<RentalSummaryDto>> GetOutgoingRentalsAsync();
    Task<List<RentalSummaryDto>> GetIncomingRentalsAsync();
}