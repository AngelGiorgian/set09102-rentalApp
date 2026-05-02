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
    private async Task ApproveIncomingRentalAsync(RentalSummaryDto? rental)
    {
        await UpdateIncomingRentalStatusAsync(rental, "Approved", "approve");
    }

    [RelayCommand]
    private async Task RejectIncomingRentalAsync(RentalSummaryDto? rental)
    {
        await UpdateIncomingRentalStatusAsync(rental, "Rejected", "reject");
    }

    private async Task UpdateIncomingRentalStatusAsync(RentalSummaryDto? rental, string newStatus, string actionLabel)
    {
        if (rental is null)
        {
            return;
        }

        if (!rental.IsRequested)
        {
            SetError("Only requested rentals can be updated here.");
            return;
        }

        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
            actionLabel == "approve" ? "Approve Rental" : "Reject Rental",
            $"Are you sure you want to {actionLabel} this rental request?",
            "Yes",
            "No");

        if (!confirm)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ClearError();

            var result = await _rentalService.UpdateRentalStatusAsync(rental.Id, newStatus);

            if (!result.IsSuccess)
            {
                SetError(result.Message);
                return;
            }

            await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Success",
                result.Message,
                "OK");

            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to update rental: {ex.Message}");
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