using System.Text.Json.Serialization;

namespace OsuSweep.Models
{
    public class ApiBeatmapData
    {
        [JsonPropertyName("beatmapSetId")]
        public int BeatmapId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("gameModes")]
        public List<string> GameModes { get; set; } = new List<string>();
    }
}
