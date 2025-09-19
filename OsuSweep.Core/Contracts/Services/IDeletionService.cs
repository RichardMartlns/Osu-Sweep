using OsuSweep.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSweep.Services
{
    public class DeletionPreviewResult
    {
        public List<string> DeletionTargets { get; set; } = new();
        public long TotalSizeInBytes { get; set; } 
        public int FolderCount { get; set; }
        public int FileCount { get; set; }
        public string SummaryMessage { get; set; } = string.Empty;
    }

    public interface IDeletionService
    {
        /// <summary>
        /// Calculates the list of files and folders to be deleted based on the selected modes.
        /// </summary>
        /// <param name="allBeatmaps">The list of all found beatmaps.</param>
        /// <param name="modesToDelete">The names of the modes to be deleted (e.g., "osu", "taiko").</param>
        /// <returns>An object with the preview results.</returns>
        Task<DeletionPreviewResult> CalculateDeletionPreviewAsync(
            IEnumerable<BeatmapSet> allBeatmaps,
            ICollection<string> modesToDelete);

        Task DeleteTargetsAsync(List<string> targets, bool isPermanent);
    }
}
