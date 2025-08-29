using System.Text.Json.Serialization;

namespace OsuSweep.Core.Models
{
    public class BeatmapDifficulty
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

    }
}
