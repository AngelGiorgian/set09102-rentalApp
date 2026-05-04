/// @file MauiTokenProvider.cs
/// @brief MAUI implementation of the token provider
/// @author StarterApp Development Team
/// @date 2025

using StarterApp.Security;

namespace StarterApp.Security;

/// @brief retrieves tokens from MAUI SecureStorage
/// @details authentication token to repository classes
public class MauiTokenProvider : ITokenProvider
{
    /// @brief key used for the authentication token
    private const string AuthTokenKey = "auth_token";

    /// @brief gets the current authentication token from secure storage
    /// @return stored token, or null if not found
    public Task<string?> GetTokenAsync()
    {
        return SecureStorage.Default.GetAsync(AuthTokenKey);
    }
}