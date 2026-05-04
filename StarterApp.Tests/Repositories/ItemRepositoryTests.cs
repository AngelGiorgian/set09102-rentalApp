/// @file ItemRepositoryTests.cs
/// @brief Unit tests for the item repository
/// @author StarterApp Development Team
/// @date 2025

using System.Net;
using System.Text;
using StarterApp.Models.Api;
using StarterApp.Repositories;
using StarterApp.Tests.Fakes;

namespace StarterApp.Tests.Repositories;

/// @brief Test suite for the ItemRepository class
/// @details Verifies item and category retrieval as well as create-item behaviour
public class ItemRepositoryTests
{
    /// @brief Creates an ItemRepository instance using a fake HTTP handler and fake token provider
    /// @param handler The fake message handler used to simulate API responses
    /// @param tokenProvider The fake token provider used to control auth token behaviour
    /// @return A configured ItemRepository instance
    private static ItemRepository CreateRepository(
        FakeHttpMessageHandler handler,
        FakeTokenProvider tokenProvider)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test")
        };

        return new ItemRepository(httpClient, tokenProvider);
    }

    /// @brief Tests that GetCategoriesAsync returns categories from a root-level JSON array
    /// @details Verifies that the repository correctly parses a direct array response
    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories_WhenResponseIsArray()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                      { "id": 1, "name": "Electronics" },
                      { "id": 2, "name": "Music" }
                    ]
                    """,
                    Encoding.UTF8,
                    "application/json")
            }
        };

        var tokenProvider = new FakeTokenProvider();
        var repository = CreateRepository(handler, tokenProvider);

        // Act
        var result = await repository.GetCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Electronics", result[0].Name);
        Assert.Equal(2, result[1].Id);
        Assert.Equal("Music", result[1].Name);
    }

    /// @brief Tests that GetCategoriesAsync returns categories from a wrapped JSON object
    /// @details Verifies that the repository correctly parses a response containing a "categories" property
    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategories_WhenResponseContainsCategoriesProperty()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "categories": [
                        { "id": 3, "name": "Games" }
                      ]
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            }
        };

        var tokenProvider = new FakeTokenProvider();
        var repository = CreateRepository(handler, tokenProvider);

        // Act
        var result = await repository.GetCategoriesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].Id);
        Assert.Equal("Games", result[0].Name);
    }

    /// @brief Tests that GetItemsAsync returns items from a valid API payload
    /// @details Verifies parsing of item list response including summary metadata
    [Fact]
    public async Task GetItemsAsync_ReturnsItems_WhenResponseIsValid()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = request =>
            {
                Assert.Equal("/items?page=1&pageSize=20", request.RequestUri!.PathAndQuery);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "items": [
                            {
                              "id": 10,
                              "title": "Laptop",
                              "description": "Good condition",
                              "dailyRate": 15.5,
                              "categoryId": 1,
                              "category": "Electronics",
                              "ownerId": 99,
                              "ownerName": "Angel Giorgian",
                              "ownerRating": 4.8,
                              "isAvailable": true,
                              "averageRating": 4.5,
                              "imageUrl": null,
                              "createdAt": "2026-05-01T10:00:00Z"
                            }
                          ],
                          "totalItems": 1,
                          "page": 1,
                          "pageSize": 20,
                          "totalPages": 1
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var tokenProvider = new FakeTokenProvider();
        var repository = CreateRepository(handler, tokenProvider);

        // Act
        var result = await repository.GetItemsAsync(page: 1, pageSize: 20);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal("Laptop", result.Items[0].Title);
        Assert.Equal("Electronics", result.Items[0].Category);
        Assert.True(result.Items[0].IsAvailable);
    }

    /// @brief Tests that CreateItemAsync fails when no token is available
    /// @details Verifies that the repository blocks authenticated item creation without a login token
    [Fact]
    public async Task CreateItemAsync_ReturnsError_WhenNoTokenIsAvailable()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler();
        var tokenProvider = new FakeTokenProvider { Token = null };
        var repository = CreateRepository(handler, tokenProvider);

        var request = new CreateItemRequest
        {
            Title = "Test Item",
            Description = "Test Description",
            DailyRate = 12.50m,
            CategoryId = 1,
            Latitude = 55.95,
            Longitude = -3.18
        };

        // Act
        var result = await repository.CreateItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("You are not logged in.", result.Message);
    }

    /// @brief Tests that CreateItemAsync returns success when the API responds successfully
    /// @details Verifies that the auth header is sent and a successful API response is handled correctly
    [Fact]
    public async Task CreateItemAsync_ReturnsSuccess_WhenApiReturnsSuccess()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler
        {
            Handler = request =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("/items", request.RequestUri!.PathAndQuery);
                Assert.NotNull(request.Headers.Authorization);
                Assert.Equal("Bearer", request.Headers.Authorization!.Scheme);
                Assert.Equal("test-token", request.Headers.Authorization!.Parameter);

                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
                };
            }
        };

        var tokenProvider = new FakeTokenProvider { Token = "test-token" };
        var repository = CreateRepository(handler, tokenProvider);

        var request = new CreateItemRequest
        {
            Title = "Test Item",
            Description = "Brand new",
            DailyRate = 20m,
            CategoryId = 1,
            Latitude = 55.95,
            Longitude = -3.18
        };

        // Act
        var result = await repository.CreateItemAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Item created successfully.", result.Message);
    }
}