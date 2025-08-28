using OsuSweep.Models;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;



namespace OsuSweep.Services
{
    /// <summary>
    /// Provides services related to the beatmap business logic, such as scanning folders and fetching data from APIs.
    /// </summary>
    public class BeatmapService : IBeatmapService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private const string ApiBaseUrl = "http://localhost:7032/api/GetBeatmapData";

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

        /// <summary>
        /// Fetch the metadata of a single beatmap set from our backend API.
        /// </summary>
        /// <param name="beatmapId">The ID of the beatmap set to be queried.</param>
        /// <returns>The beatmap data from the API, or null if the request fails.</returns>
        public async Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId)
        {
            var requestUrl = $"{ApiBaseUrl}?id={beatmapId}";

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // We use System.Text.Json to convert the API response (JSON) into a C# object.
                return JsonSerializer.Deserialize<ApiBeatmapData>(jsonResponse);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro na requisição HTTP: {e.Message}");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Erro na desserialização do JSON {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Attempts to extract a numeric ID from the beginning of a string (folder name) using Regex.
        /// </summary>
        /// <param name="folderName">The name of the folder to parse.</param>
        /// <returns>The extracted ID as an integer, or null if no leading digits are found.</returns>
        /// <remarks>
        /// This method uses the regular expression pattern "^(\d+)" to find one or more digits
        /// at the absolute start of the string. It is robust against different separators
        /// (or no separator at all) following the ID.
        /// </remarks>
        public int? TryExtractIdFromName(string folderName)
        {
            var match = Regex.Match(folderName, @"^(\d+)");

            if (match.Success)
            {
                string potentialId = match.Groups[1].Value;

                if (int.TryParse(potentialId, out int id))
                {
                    return id;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculates the total disk size in bytes for a given list of target paths.
        /// </summary>
        /// <param name="targets">A list of strings, where each string is an absolute path to a folder to be included in the size calculation.</param>
        /// <returns>A Task that represents the asynchronous operation, containing the total size in bytes (long) of all targets combined.</returns>
        /// <remarks>
        /// This is a potentially long-running I/O operation. It is executed on a background thread using Task.Run to prevent freezing the user interface.
        /// </remarks>
        public async Task<long> CalculateTargetsSizeAsync(List<string> targets)
        {
            return await Task.Run(() =>
            {
                long totalSize = 0;
                foreach (var path in targets)
                {
                    totalSize += GetDirectorySize(path);
                }
                return totalSize;
            });
        }

        /// <summary>
        /// Recursively calculates the total size of all files within a single directory.
        /// </summary>
        /// <param name="folderPath">The absolute path of the directory to measure.</param>
        /// <returns>The total size in bytes (long). Returns 0 if the directory is not found or an error occurs.</returns>
        /// <remarks>
        /// This method uses SearchOption.AllDirectories to include files in all subfolders.
        /// It's wrapped in a try-catch to safely handle potential exceptions like DirectoryNotFoundException.
        /// </remarks>
        private long GetDirectorySize(string folderPath)
        {
            try
            {
                var directory = new DirectoryInfo(folderPath);
                return directory.GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }
        }
    }
}
