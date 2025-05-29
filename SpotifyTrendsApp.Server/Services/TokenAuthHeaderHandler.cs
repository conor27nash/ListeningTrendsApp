using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.OAuth;
using Polly;
using Polly.Retry;

namespace SpotifyTrendsApp.Server.Services;

public class TokenAuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;

    public TokenAuthHeaderHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
        _policy = Policy<HttpResponseMessage>
            .HandleResult(r => r.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(1, async (_, _) =>
            {
                if (_tokenService.CurrentToken != null)
                {
                    await _tokenService.RefreshAccessTokenAsync(_tokenService.CurrentToken.RefreshToken);
                }
            });
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tokenService.CurrentToken != null && _tokenService.CurrentToken.ExpiresAt <= DateTime.UtcNow)
        {
            await _tokenService.RefreshAccessTokenAsync(_tokenService.CurrentToken.RefreshToken);
        }

        return await _policy.ExecuteAsync(async () =>
        {
            if (_tokenService.CurrentToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenService.CurrentToken.AccessToken);
            }
            return await base.SendAsync(request, cancellationToken);
        });
    }
}
