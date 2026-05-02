using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarterApp.Models.Api;
using StarterApp.Services;

namespace StarterApp.ViewModels;

public partial class RentalsViewModel : BaseViewModel
{
    private readonly IRentalService _rentalService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string outgoingSummary = "Loading...";

    [ObservableProperty]
    private string incomingSummary = "Loading...";

    public ObservableCollection<RentalSummaryDto> OutgoingRentals { get; } = new();
    public ObservableCollection<RentalSummaryDto> IncomingRentals { get; } = new();

    public RentalsViewModel(IRentalService rentalService, INavigationService navigationService)
    {
        _rentalService = rentalService;
        _navigationService = navigationService;
        Title = "My Rentals";

        _ = RefreshDataAsync();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var outgoing = await _rentalService.GetOutgoingRentalsAsync();
            var incoming = await _rentalService.GetIncomingRentalsAsync();

            OutgoingRentals.Clear();
            IncomingRentals.Clear();

            foreach (var rental in outgoing)
            {
                OutgoingRentals.Add(rental);
            }

            foreach (var rental in incoming)
            {
                IncomingRentals.Add(rental);
            }

            OutgoingSummary = $"{OutgoingRentals.Count} outgoing rental(s)";
            IncomingSummary = $"{IncomingRentals.Count} incoming rental(s)";
        }
        catch (Exception ex)
        {
            SetError($"Failed to load rentals: {ex.Message}");
            OutgoingSummary = "Could not load outgoing rentals";
            IncomingSummary = "Could not load incoming rentals";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.NavigateBackAsync();
    }
}