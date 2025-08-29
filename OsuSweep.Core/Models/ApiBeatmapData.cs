using System.Text.Json.Serialization;

namespace OsuSweep.Core.Models
{
    public class ApiBeatmapData
    {
        [JsonPropertyName("beatmapSetId")]
        public int BeatmapId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("difficulties")]
        public List<BeatmapDifficulty> Difficulties { get; set; } = new List<BeatmapDifficulty>();
        
        public int BeatmapSetId { get; set; }
    }
}
