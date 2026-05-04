/// @file RentalRepositoryTests.cs
/// @brief Unit tests for the rental repository
/// @author StarterApp Development Team
/// @date 2025

using System.Net;
using System.Text;
using StarterApp.Models.Api;
using StarterApp.Repositories;
using StarterApp.Tests.Fakes;

namespace StarterApp.Tests.Repositories;

/// @brief Test suite for the RentalRepository class
/// @details Verifies rental request submission, rental list mapping, and rental status updates
public class RentalRepositoryTests
{
    /// @brief Creates a RentalRepository instance using a fake HTTP handler and fake token provider
    /// @param handler The fake message handler used to simulate API responses
    /// @param tokenProvider The fake token provider used to control auth token behaviour
    /// @return A configured RentalRepository instance
    private static RentalRepository CreateRepository(
        FakeHttpMessageHandler handler,
        FakeTokenProvider tokenProvider)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test")
        };

        return new RentalRepository(httpClient, tokenProvider);
    }

    /// @brief Tests that RequestRentalAsync fails when no token is available
    /// @details Verifies that rental requests cannot be submitted without authentication
    [Fact]
    public async Task RequestRentalAsync_ReturnsError_WhenNoTokenIsAvailable()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler();
        var tokenProvider = new FakeTokenProvider { Token = null };
        var repository = CreateRepository(handler, tokenProvider);

        var request = new CreateRentalRequest
        {
            ItemId = 10,
            StartDate = "2026-05-10",
            EndDate = "2026-05-12"
        };

        // Act
        var result = await repository.RequestRentalAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("You are not logged in.", result.Message);
    }

    /// @brief Tests that UpdateRentalStatusAsync returns success when the API responds successfully
    /// @details Verifies that the PATCH request sends the token and succeeds on a valid response
    [Fact]
    public async Task UpdateRentalStatusAsync_ReturnsSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = request =>
            {
                Assert.Equal("PATCH", request.Method.Method);
                Assert.Equal("/rentals/25/status", request.RequestUri!.PathAndQuery);
                Assert.NotNull(request.Headers.Authorization);
                Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
                Assert.Equal("test-token", request.Headers.Authorization!.Parameter);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
                };
            }
        };

        var tokenProvider = new FakeTokenProvider { Token = "test-token" };
        var repository = CreateRepository(handler, tokenProvider);

        // Act
        var result = await repository.UpdateRentalStatusAsync(25, "Approved");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Rental approved successfully.", result.Message);
    }

    /// @brief Tests that GetIncomingRentalsAsync correctly maps nested item, borrower, and owner objects
    /// @details Verifies fallback mapping when summary properties are missing from the root rental object
    [Fact]
    public async Task GetIncomingRentalsAsync_MapsNestedObjectsCorrectly()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = request =>
            {
                Assert.Equal("/rentals/incoming", request.RequestUri!.PathAndQuery);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        [
                          {
                            "id": 101,
                            "itemId": 0,
                            "status": "Requested",
                            "startDate": "2026-05-11",
                            "endDate": "2026-05-15",
                            "requestedAt": "2026-05-08T12:00:00Z",
                            "totalPrice": 48.50,
                            "item": {
                              "id": 77,
                              "title": "Canon Camera"
                            },
                            "borrower": {
                              "firstName": "Angel",
                              "lastName": "Giorgian"
                            },
                            "owner": {
                              "firstName": "Diana",
                              "lastName": "Giorgian"
                            }
                          }
                        ]
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var tokenProvider = new FakeTokenProvider { Token = "test-token" };
        var repository = CreateRepository(handler, tokenProvider);

        // Act
        var result = await repository.GetIncomingRentalsAsync();

        // Assert
        Assert.Single(result);

        var rental = result[0];
        Assert.Equal(101, rental.Id);
        Assert.Equal(77, rental.ItemId);
        Assert.Equal("Canon Camera", rental.ItemTitle);
        Assert.Equal("Requested", rental.Status);
        Assert.Equal("Angel Giorgian", rental.BorrowerName);
        Assert.Equal("Diana Giorgian", rental.OwnerName);
        Assert.Equal(48.50m, rental.TotalPrice);
        Assert.True(rental.IsRequested);
    }

    /// @brief Tests that GetOutgoingRentalsAsync correctly reads a wrapped rentals payload
    /// @details Verifies parsing when the API returns an object containing a "rentals" array
    [Fact]
    public async Task GetOutgoingRentalsAsync_ReturnsRentals_WhenResponseContainsRentalsProperty()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = request =>
            {
                Assert.Equal("/rentals/outgoing", request.RequestUri!.PathAndQuery);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "rentals": [
                            {
                              "id": 202,
                              "itemId": 15,
                              "itemTitle": "MacBook Pro",
                              "status": "Out for Rent",
                              "startDate": "2026-05-12",
                              "endDate": "2026-05-14",
                              "requestedAt": "2026-05-09T10:30:00Z",
                              "totalPrice": 80.00,
                              "ownerName": "Owner Example",
                              "borrowerName": "Borrower Example"
                            }
                          ]
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var tokenProvider = new FakeTokenProvider { Token = "test-token" };
        var repository = CreateRepository(handler, tokenProvider);

        // Act
        var result = await repository.GetOutgoingRentalsAsync();

        // Assert
        Assert.Single(result);

        var rental = result[0];
        Assert.Equal(202, rental.Id);
        Assert.Equal(15, rental.ItemId);
        Assert.Equal("MacBook Pro", rental.ItemTitle);
        Assert.Equal("Out for Rent", rental.Status);
        Assert.Equal("Owner Example", rental.OwnerName);
        Assert.Equal("Borrower Example", rental.BorrowerName);
        Assert.True(rental.IsOutForRent);
    }

    /// @brief Tests that RequestRentalAsync returns the API error message when the request fails
    /// @details Verifies that repository error extraction works for failed rental requests
    [Fact]
    public async Task RequestRentalAsync_ReturnsApiErrorMessage_WhenApiRequestFails()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = request =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("/rentals", request.RequestUri!.PathAndQuery);

                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(
                        """
                        {
                          "error": "Validation failed",
                          "message": "End date must be after start date"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var tokenProvider = new FakeTokenProvider { Token = "test-token" };
        var repository = CreateRepository(handler, tokenProvider);

        var request = new CreateRentalRequest
        {
            ItemId = 99,
            StartDate = "2026-05-20",
            EndDate = "2026-05-19"
        };

        // Act
        var result = await repository.RequestRentalAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("End date must be after start date", result.Message);
    }
}