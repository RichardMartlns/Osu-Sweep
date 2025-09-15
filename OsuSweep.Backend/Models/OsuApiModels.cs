namespace OsuSweep.Backend.Models
{
    /// <summary>
    /// Represents the main structure of a Beatmap Set as returned by the osu! v2 API.
    /// </summary>
    public class OsuApiV2BeatmapSet
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";

        public List<OsuApiV2Beatmap>? Beatmaps { get; set; }
    }

    /// <summary>
    /// Represents the structure of a single difficulty (beatmap) within a Beatmap Set from the v2 API.
    /// </summary>
    public class OsuApiV2Beatmap
    {
        public string Version { get; set; } = "";
        public string Mode { get; set; } = "";
    }
}
