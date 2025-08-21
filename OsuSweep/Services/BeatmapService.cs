using OsuSweep.Models;
using System.IO;

namespace OsuSweep.Services
{
    public class BeatmapService
    {

        public async Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath)
        {
            return await Task.Run(() =>
            {
                var beatmapSets = new List<BeatmapSet>();

                var directories = Directory.GetDirectories(songsFolderPath);

                foreach (var dir in directories)
                {

                    var foldername = new DirectoryInfo(dir).Name;

                    int? beatmapId = TryExtractIdFromName(foldername);

                    beatmapSets.Add(new BeatmapSet(dir, beatmapId));
                }

                return beatmapSets;
            });
        }

        private int? TryExtractIdFromName(string folderName)
        {
            string? potentialId = folderName.Split(' ').FirstOrDefault();

            if (potentialId != null)
            {
                return null;
            }

            if (int.TryParse(potentialId, out int id))
            {
                return id;
            }

            return null;
        }
    }
}
