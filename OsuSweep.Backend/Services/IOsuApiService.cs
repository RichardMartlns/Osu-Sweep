using OsuSweep.Core.Models;

namespace OsuSweep.Backend.Services
{
    /// <summary>
    /// Defines a service to interact with the osu! API, abstracting the authentication and data fetching logic.
    /// </summary>
    public interface IOsuApiService
    {
        /// <summary>
        /// Retrieves beatmap set data from the osu! v2 API.
        /// </summary>
        /// <param name="beatmapSetId">The ID of the beatmap set to retrieve.</param>
        /// <returns>The beatmap data formatted for the application, or null if not found or if an error occurs.</returns>
        Task<ApiBeatmapData?> GetBeatmapSetAsync(int beatmapSetId);
    }
}
