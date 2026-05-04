/// @file FakeHttpMessageHandler.cs
/// @brief Fake HTTP message handler for repository unit tests
/// @author StarterApp Development Team
/// @date 2025

using System.Net;
using System.Net.Http;

namespace StarterApp.Tests.Fakes;

/// @brief Fake HTTP handler for simulating API responses
/// @details Allows tests to return controlled HTTP responses without calling the real API
public class FakeHttpMessageHandler : HttpMessageHandler
{
    /// @brief Delegate used to build the fake HTTP response
    public Func<HttpRequestMessage, HttpResponseMessage> Handler { get; set; }
        = _ => new HttpResponseMessage(HttpStatusCode.NotFound);

    /// @brief Sends the fake HTTP response
    /// @param request The outgoing HTTP request
    /// @param cancellationToken Cancellation token
    /// @return The fake HTTP response defined by the handler delegate
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Handler(request));
    }
}