using System.Net;
using System.Net.Http.Headers;

namespace PharmaTrack.PWA.Helpers
{
    public class JwtRefreshHandler : DelegatingHandler
    {
        private readonly AuthService _auth;
        private static readonly SemaphoreSlim _refreshLock = new(1, 1);

        public JwtRefreshHandler(AuthService auth) =>
            _auth = auth;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            // 1) Proactive: if the token will expire within 1 minute, refresh it
            if (await _auth.AccessTokenExpiresSoonAsync())
            {
                await _refreshLock.WaitAsync(ct);
                try
                {
                    if (await _auth.AccessTokenExpiresSoonAsync())
                        await _auth.TryRefreshTokenAsync(ct);
                }
                finally
                {
                    _refreshLock.Release();
                }
            }

            // 2) Attach the (possibly fresh) access token
            var token = await _auth.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            // 3) Send the request
            var response = await base.SendAsync(request, ct);

            // 4) Reactive: if we still get a 401, try one more refresh-and-retry
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _refreshLock.WaitAsync(ct);
                try
                {
                    if (await _auth.TryRefreshTokenAsync(ct))
                    {
                        request.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", await _auth.GetAccessTokenAsync());
                        response = await base.SendAsync(request, ct);
                    }
                }
                finally
                {
                    _refreshLock.Release();
                }
            }

            return response;
        }
    }
}
