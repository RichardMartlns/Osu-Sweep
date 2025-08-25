using OsuSweep.Models;
using System.IO;
using System.Net.Http;
using System.Text.Json;



namespace OsuSweep.Services
{
    /// <summary>
    /// Provides services related to the beatmap business logic, such as scanning folders and fetching data from APIs.
    /// </summary>
    public class BeatmapService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private const string ApiBaseUrl = "";

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
        /// Attempts to extract a numeric ID from the beginning of a string (folder name).
        /// The common format is '12345 Artist - Title'.
        /// </summary>
        /// <param name="folderName">The beatmap folder name.</param>
        /// <returns>The ID as an integer, or null if not found.</returns>
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
