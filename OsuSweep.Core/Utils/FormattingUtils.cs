

namespace OsuSweep.Core.Utils
{
    public static class FormattingUtils
    {
        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes;
            while (dblSByte >= 1024 && i < suffixes.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:0.##} {suffixes[i]}";
        }

        /// <summary>
        /// Converts the numeric ID of an osu! game mode to its string name.
        /// </summary>
        /// <param name="modeId">The mode ID (0-3)</param>
        /// <returns>The name of the mode in lowercase ("osu", "taiko", etc...)</returns>
        public static string ConvertModeIdToName(int modeId)
        {
            return modeId switch
            {
                0 => "osu",
                1 => "taiko",
                2 => "catch",
                3 => "mania",
                _ => "unknown",
            };
        }
    }
}
