using OsuSweep.Core.Models;
using System.IO;

namespace OsuSweep.Services
{
    public interface IBeatmapService
    {
        Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath);
        Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId);
        int? TryExtractIdFromName(string folderName);
        Task<long> CalculateTargetsSizeAsync(List<string> targets);
        List<string> GetFilePathsForPartialDeletion(BeatmapSet beatmap, IEnumerable<string> modesToDelete);
        Task DeleteTargetsAsync(List<string> targets, bool isPermanent);

    }
}
