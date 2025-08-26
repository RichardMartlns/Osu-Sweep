using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;


// This is the model our WPF app expects to receive.
namespace OsuSweep.Models
{
    public class ApiBeatmapData
    {
        [JsonPropertyName("beatmapSetId")]
        public int BeatmapSetId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("gameModes")]
        public List<string> GameModes { get; set; } = new List<string>();
    }
}

// This is the new model that matches the official osu! API response.
namespace OsuSweep.Backend.Models
{
    public class OsuApiBeatmap
    {
        [JsonPropertyName("beatmapset_id")]
        public string BeatmapSetId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;
    }
}



namespace OsuSweep.Backend
{
    public class GetBeatmapData
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public GetBeatmapData(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetBeatmapData>();
        }

        [Function("GetBeatmapData")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // 1. READ THE API KEY AND THE BEATMAP ID
            // We parse the query string manually for robustness.
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            string? beatmapIdStr = query["id"];

            // Read the API key from local settings (this file is not sent to Git).
            string? apiKey = Environment.GetEnvironmentVariable("OsuApiKey");
           

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(beatmapIdStr))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Por favor, forneça a chave da API e o ID do beatmap.");
                return badResponse;
            }

            // 2. MAKE THE CALL TO THE OFFICIAL OSU! API
            var osuApiUrl = $"https://osu.ppy.sh/api/get_beatmaps?k={apiKey}&s={beatmapIdStr}";

            try
            {
                var osuApiResponse = await _httpClient.GetStringAsync(osuApiUrl);
                _logger.LogInformation($"Resposta recebida da API do osu!: {osuApiResponse}");
                var osuBeatmaps = JsonSerializer.Deserialize<List<OsuSweep.Backend.Models.OsuApiBeatmap>>(osuApiResponse);

                if (osuBeatmaps == null || osuBeatmaps.Count == 0)
                {
                    // If the API did not return any map, we return 'Not Found'.
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                // 3.PROCESS AND MAP THE RESPONSE
                var firstMap = osuBeatmaps[0];
                var responseData = new OsuSweep.Models.ApiBeatmapData
                {
                    BeatmapSetId = int.Parse(firstMap.BeatmapSetId),
                    Title = firstMap.Title,
                    Artist = firstMap.Artist,
                    // We extract all unique game modes from the response.
                    GameModes = osuBeatmaps.Select(b => b.Mode).Distinct().Select(mode => mode switch
                    {
                        "0" => "osu",
                        "1" => "taiko",
                        "2" => "catch",
                        "3" => "mania",
                        _ => "unknown"
                    }).ToList()
                };

                //  // 4. RETURN OUR SIMPLIFIED JSON TO THE WPF APP
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                var jsonResponse = JsonSerializer.Serialize(responseData);
                await response.WriteStringAsync(jsonResponse);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro ao processar o pedido para a API do osu!");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}