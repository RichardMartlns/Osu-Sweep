using Microsoft.Extensions.Logging;
using OsuSweep.Backend.Models;
using OsuSweep.Core.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OsuSweep.Backend.Services
{
    /// <summary>
    /// Implementation of the service for interacting with the osu! API, including OAuth 2.0 token management.
    /// </summary>
    public class OsuApiService : IOsuApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OsuApiService> _logger;


        private static OsuToken? _cachedToken;

        public OsuApiService(HttpClient httpClient, ILogger<OsuApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiBeatmapData?> GetBeatmapSetAsync(int beatmapSetId)
        {
            try
            {
                var token = await GetValidAccessTokenAsync();
                if (token == null)
                {
                    _logger.LogError("Failed to obtain osu! API access token.");
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/beatmapsets/{beatmapSetId}");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);


                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Osu Api returned {Status} for beatmap set {Id}", response.StatusCode, beatmapSetId);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var osuApiBeatmapSet = JsonSerializer.Deserialize<OsuApiV2BeatmapSet>(jsonResponse, options);

                return MapToApiBeatmapData(osuApiBeatmapSet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching beatmap set {id} from osu! API v2.", beatmapSetId);
                return null;
            }
        }

        /// <summary>
        /// Gets a valid access token, reusing one from the cache if possible or requesting a new one.
        /// </summary>
        /// <returns>A valid OsuToken, or null if authentication with the credentials fails.</returns>
        private async Task<OsuToken?> GetValidAccessTokenAsync()
        {

            if (_cachedToken != null && _cachedToken.ExpiresAtUtc > DateTime.UtcNow.AddMinutes(1))
            {
                return _cachedToken;
            }

            _logger.LogInformation("Acccess token is invalid or expired. Requesting a new one.");

            var clientId = Environment.GetEnvironmentVariable("OSU_API_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("OSU_API_CLIENT_SECRET");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                _logger.LogError("OSU API client credentials not configured");
                return null;
            }

            var requestBody = new Dictionary<string, string>
            {
                { "client_id", clientId},
                { "client_secret", clientSecret },
                { "grant_type", "client_credentials" },
                { "scope", "public" }
            };

            using var tokenResponse = await _httpClient.PostAsync("https://osu.ppy.sh/oauth/token", new FormUrlEncodedContent(requestBody));
            tokenResponse.EnsureSuccessStatusCode();
            var jsonContent = await tokenResponse.Content.ReadAsStringAsync();


            var tokenData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
            var newToken = new OsuToken
            {
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? "",
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(tokenData.GetProperty("expires_in").GetInt32())
            };

            _cachedToken = newToken;
            return newToken;
        }

        /// <summary>
        /// Maps (converts) the data object received from the v2 API to our application's data model.
        /// </summary>
        /// <param name="apiData">The deserialized object from the osu! API response.</param>
        /// <returns>An ApiBeatmapData object formatted for the frontend.</returns>
        private ApiBeatmapData? MapToApiBeatmapData(OsuApiV2BeatmapSet? apiData)
        {

            if (apiData == null)
            {
                return null;
            }

            return new ApiBeatmapData
            {
                BeatmapSetId = apiData.Id,
                Title = apiData.Title ?? "",
                Artist = apiData.Artist ?? "",
                Difficulties = apiData.Beatmaps?.Select(b => new BeatmapDifficulty
                {
                    Version = b.Version ?? "",
                    Mode = b.Mode ?? ""
                }).ToList() ?? new List<BeatmapDifficulty>()
            };
        }
    }
}
