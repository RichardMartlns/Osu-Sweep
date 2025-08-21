
namespace OsuSweep.Models
{

    public class BeatmapSet
    {
        public string FolderPath { get; set; }

        public int? BeatmapSetId { get; set; }

        public BeatmapSet(string folderPath, int? beatmapSetId)
        {
            FolderPath = folderPath;
            BeatmapSetId = beatmapSetId;
        }
    }
}

