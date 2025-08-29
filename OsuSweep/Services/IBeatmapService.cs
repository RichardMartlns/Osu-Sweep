using OsuSweep.Core.Models;
using System.IO;

namespace OsuSweep.Services
{
    public interface IBeatmapService
    {
        Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId);
        Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath);

        int? TryExtractIdFromName(string folderName);

        Task<long> CalculateTargetsSizeAsync(List<string> targets);
        List<string> GetFilePathsForPartialDeletion(BeatmapSet beatmap, IEnumerable<string> modesToDelete);
        
    }
}
