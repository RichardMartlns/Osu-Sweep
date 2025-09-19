using Microsoft.VisualBasic.FileIO;
using OsuSweep.Core.Contracts.Services;
using OsuSweep.Core.Models;
using OsuSweep.Core.Utils;
using System.Diagnostics;
using System.IO;

namespace OsuSweep.Services
{
    public class DeletionService : IDeletionService
    {
        private readonly IBeatmapService _beatmapService;

        public DeletionService(IBeatmapService beatmapService)
        {
            _beatmapService = beatmapService;
        }

        public async Task<DeletionPreviewResult> CalculateDeletionPreviewAsync(
            IEnumerable<BeatmapSet> allBeatmaps,
            ICollection<string> modesToDelete)
        {
            var result = new DeletionPreviewResult();

            if (!modesToDelete.Any())
            {
                return result;
            }

            var targets = await Task.Run(() =>
            {
                var list = new List<string>();
                var beatmapsAnalyzed = allBeatmaps.Where(b => b.IsMetadataLoaded && b.GameModes.Any());

                foreach (var beatmap in beatmapsAnalyzed)
                {
                    bool isFullDeletionTarget = !beatmap.GameModes.Except(modesToDelete).Any();
                    if (isFullDeletionTarget)
                    {
                        list.Add(beatmap.FolderPath);
                    }
                    else if (beatmap.GameModes.Any(modesToDelete.Contains))
                    {
                        var filesToDelete = _beatmapService.GetFilePathsForPartialDeletion(beatmap, modesToDelete);
                        list.AddRange(filesToDelete);
                    }
                }
                return list;
            });

            result.DeletionTargets = targets;
            result.TotalSizeInBytes = await _beatmapService.CalculateTargetsSizeAsync(targets);
            result.FolderCount = targets.Count(Directory.Exists);
            result.SummaryMessage = $"Targets: {result.FolderCount} folders and {result.FileCount} files, releasing {FormattingUtils.FormatBytes(result.TotalSizeInBytes)}.";

            return result;
        }

        public async Task DeleteTargetsAsync(List<string> targets, bool isPermanent)
        {
            await Task.Run(() =>
            {
                foreach (var path in targets)
                {
                    try
                    {
                        if (isPermanent)
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }
                        }
                        else
                        {
                            if (File.Exists(path))
                            {
                                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                            else if (Directory.Exists(path))
                            {
                                FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to delete '{path}': {ex.Message}");
                    }
                }
            });
        }
    }
}
