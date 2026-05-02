using StarterApp.Models.Api;

namespace StarterApp.Services;

public interface IRentalService
{
    Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request);
}