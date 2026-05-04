/// @file ITokenProvider.cs
/// @brief Token provider abstraction for accessing authentication tokens
/// @author StarterApp Development Team
/// @date 2025

namespace StarterApp.Security;

/// @brief Abstraction for retrieving the current authentication token
/// @details Allows repositories to access tokens without depending directly on MAUI SecureStorage
public interface ITokenProvider
{
    /// @brief Gets the current authentication token
    /// @return The authentication token, or null if none is available
    Task<string?> GetTokenAsync();
}