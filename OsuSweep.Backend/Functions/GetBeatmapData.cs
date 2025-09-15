using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OsuSweep.Backend.Services;
using System.Net;


namespace OsuSweep.Backend
{
    /// <summary>
    /// The API entrypoint that handles HTTP requests for fetching beatmap data.
    /// </summary>
    public class GetBeatmapData
    {
        private readonly ILogger<GetBeatmapData> _logger;
        private readonly IOsuApiService _osuApiService;

        public GetBeatmapData(ILogger<GetBeatmapData> logger, IOsuApiService osuApiService)
        {
            _logger = logger;
            _osuApiService = osuApiService;
        }

        /// <summary>
        /// Executes the function, validating the request and delegating data fetching to the IOsuApiService.
        /// </summary>
        /// <param name="req">The incoming HTTP request, which contains the beatmap set 'id' in the query string.</param>
        /// <returns>An HTTP response containing the beatmap data (200 OK), or an error code (400, 404).</returns>
        [Function("GetBeatmapData")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string? beatmapIdStr = req.Query["id"];

            if (!int.TryParse(beatmapIdStr, out int beatmapId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var beatmapData = await _osuApiService.GetBeatmapSetAsync(beatmapId);
            if (beatmapData == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(beatmapData);
            return response;
        }
    }
}
