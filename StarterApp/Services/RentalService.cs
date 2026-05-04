using StarterApp.Models.Api;
using StarterApp.Repositories;

namespace StarterApp.Services;

//handles rental actions
public class RentalService : IRentalService
{
    private readonly IRentalRepository _rentalRepository;

    public RentalService(IRentalRepository rentalRepository)
    {
        _rentalRepository = rentalRepository;
    }

    //requests rental
    public Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request)
    {
        return _rentalRepository.RequestRentalAsync(request);
    }

    //updates rental status
    public Task<(bool IsSuccess, string Message)> UpdateRentalStatusAsync(int rentalId, string status)
    {
        return _rentalRepository.UpdateRentalStatusAsync(rentalId, status);
    }

    //gets outgoing rentals
    public Task<List<RentalSummaryDto>> GetOutgoingRentalsAsync()
    {
        return _rentalRepository.GetOutgoingRentalsAsync();
    }

    //gets incoming rentals
    public Task<List<RentalSummaryDto>> GetIncomingRentalsAsync()
    {
        return _rentalRepository.GetIncomingRentalsAsync();
    }
}