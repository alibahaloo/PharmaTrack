using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace PharmaTrack.PWA.Helpers
{
    public class JwtAuthorizationHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _storage;
        public JwtAuthorizationHandler(ILocalStorageService storage) =>
          _storage = storage;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            var token = await _storage.GetItemAsync<string>("accessToken");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, ct);
        }
    }
}
