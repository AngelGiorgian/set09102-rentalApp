using StarterApp.Models.Api;
using StarterApp.Repositories;

namespace StarterApp.Services;

public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;

    public RentalService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    public Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request)
    {
        return _rentalRepository.RequestRentalAsync(request);
    }

    public Task<(bool IsSuccess, string Message)> UpdateRentalStatusAsync(int rentalId, string status)
    {
        return _rentalRepository.UpdateRentalStatusAsync(rentalId, status);
    }

    public Task<List<RentalSummaryDto>> GetOutgoingRentalsAsync()
    {
        return _rentalRepository.GetOutgoingRentalsAsync();
    }

    public Task<List<RentalSummaryDto>> GetIncomingRentalsAsync()
    {
        return _rentalRepository.GetIncomingRentalsAsync();
    }
}