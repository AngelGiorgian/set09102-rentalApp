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
        await UpdateIncomingRentalStatusAsync(
            rental,
            "Approved",
            "Approve Rental",
            "Are you sure you want to approve this rental request?");
    }

    [RelayCommand]
    private async Task RejectIncomingRentalAsync(RentalSummaryDto? rental)
    {
        await UpdateIncomingRentalStatusAsync(
            rental,
            "Rejected",
            "Reject Rental",
            "Are you sure you want to reject this rental request?");
    }

    [RelayCommand]
    private async Task StartIncomingRentalAsync(RentalSummaryDto? rental)
    {
        await UpdateIncomingRentalStatusAsync(
            rental,
            "Out for Rent",
            "Start Rental",
            "Mark this rental as Out for Rent?");
    }

    [RelayCommand]
    private async Task MarkIncomingReturnedAsync(RentalSummaryDto? rental)
    {
        await UpdateIncomingRentalStatusAsync(
            rental,
            "Returned",
            "Mark Returned",
            "Mark this rental as Returned?");
    }

    private async Task UpdateIncomingRentalStatusAsync(
        RentalSummaryDto? rental,
        string newStatus,
        string dialogTitle,
        string dialogMessage)
    {
        if (rental is null)
        {
            return;
        }

        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
            dialogTitle,
            dialogMessage,
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
    private async Task MarkOutgoingReturnedAsync(RentalSummaryDto? rental)
    {
        if (rental is null)
        {
            return;
        }

        var confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
            "Mark Returned",
            "Mark this rental as Returned?",
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

            var result = await _rentalService.UpdateRentalStatusAsync(rental.Id, "Returned");

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