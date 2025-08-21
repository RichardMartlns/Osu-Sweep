
namespace OsuSweep.Models
{

    public class BeatmapSet
    {
        public string FolderPath { get; set; }

        public int? BeatmapSetId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public List<string> GameModes { get; set; } = new List<string>();

        public bool IsMetadataLoaded { get; set; } = false;

        public BeatmapSet(string folderPath, int? beatmapSetId)
        {
            FolderPath = folderPath;
            BeatmapSetId = beatmapSetId;
        }
    }
}

