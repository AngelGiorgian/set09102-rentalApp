using StarterApp.Models.Api;

namespace StarterApp.Repositories;

//renatl repository contract
public interface IRentalRepository : IRepository<RentalSummaryDto>
{
    Task<(bool IsSuccess, string Message)> RequestRentalAsync(CreateRentalRequest request); //request rental
    Task<(bool IsSuccess, string Message)> UpdateRentalStatusAsync(int rentalId, string status); //updates renatl status

    Task<List<RentalSummaryDto>> GetOutgoingRentalsAsync(); //gets outgoing rentals
    Task<List<RentalSummaryDto>> GetIncomingRentalsAsync(); //gets incoming rentals
}