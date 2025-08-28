using OsuSweep.Models;
using System.IO;

namespace OsuSweep.Services
{
    public interface IBeatmapService
    {
        Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId);
        Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath);
        Task<long> CalculateTargetsSizeAsync(List<string> targets);
        int? TryExtractIdFromName(string folderName);

        
    }
}
