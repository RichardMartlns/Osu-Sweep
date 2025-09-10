using OsuSweep.Core.Models;
using System.IO;

namespace OsuSweep.Services
{
    public interface IBeatmapService
    {
        Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath);
        Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId);
        Task<long> CalculateTargetsSizeAsync(List<string> targets);
        Task DeleteTargetsAsync(List<string> targets, bool isPermanent);
        List<string> GetFilePathsForPartialDeletion(BeatmapSet beatmap, IEnumerable<string> modesToDelete);
        List<int> GetModesFromBeatmapSetFolder(string folderPath);
        int? TryExtractIdFromName(string folderName);
    }
}
