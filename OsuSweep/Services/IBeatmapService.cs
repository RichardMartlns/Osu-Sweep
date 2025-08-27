using OsuSweep.Models;

namespace OsuSweep.Services
{
    public interface IBeatmapService
    {
        Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId);
        Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath);
        int? TryExtractIdFromName(string folderName);
    }
}