/// @file FakeTokenProvider.cs
/// @brief Fake token provider for repository unit tests
/// @author StarterApp Development Team


using StarterApp.Security;

namespace StarterApp.Tests.Fakes;

/// @brief Fake implementation of the token provider
/// @details Allows tests to control whether a token is present or missing
public class FakeTokenProvider : ITokenProvider
{
    /// @brief The token value to return
    public string? Token { get; set; }

    /// @brief Returns the configured token value
    /// @return The configured token, or null if none is set
    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult(Token);
    }
}