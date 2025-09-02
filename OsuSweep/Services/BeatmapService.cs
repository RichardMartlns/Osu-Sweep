using OsuSweep.Core.Models;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;




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

                    var folderName = new DirectoryInfo(dir).Name;

                    int? beatmapId = TryExtractIdFromName(folderName);

                    beatmapSets.Add(new BeatmapSet(dir, beatmapId));
                }

                return beatmapSets;
            });
        }

        /// <summary>
        /// Scans the specified "Songs" folder asynchronously, identifies beatmap folders,
        /// and attempts to extract the ID from each folder name.
        /// </summary>
        /// <param name="songsFolderPath">The absolute path to the osu! "Songs" folder.</param>
        /// <returns>A Task that represents the asynchronous operation, containing a list of found BeatmapSet objects.</returns>
        /// <remarks>
        /// This is a potentially long-running I/O operation. It is executed on a background thread using Task.Run to prevent freezing the user interface.
        /// </remarks>
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
                Console.WriteLine($"HTTP request error: {e.Message}");
                return null;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error deserializing JSON: {e.Message}");
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
                    if (Directory.Exists(path))
                    {
                        totalSize += GetDirectorySize(path);
                    }
                    else if (File.Exists(path))
                    {
                        totalSize += new FileInfo(path).Length;
                    }
                    
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
                return directory.GetFiles("*", System.IO.SearchOption.AllDirectories).Sum(f => f.Length);
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Identifies the specific .osu file paths within a single beatmap folder that correspond
        /// to a list of game modes selected for partial deletion.
        /// </summary>
        /// <param name="beatmap">The BeatmapSet object to be analyzed. This object must have its 'Difficulties' property populated with data from the API.</param>
        /// <param name="modesToDelete">An IEnumerable<string> containing the game modes the user wants to delete (e.g., ["taiko", "mania"]).</param>
        /// <returns>A List<string> of absolute file paths for the .osu files that should be deleted. Returns an empty list if no matches are found.</returns>
        /// <remarks>
        /// This method works by cross-referencing the detailed 'Difficulties' list (from the API)
        /// with the actual .osu files on disk, matching them by the '[Version]' name in the filename.
        /// The file search is case-insensitive.
        /// </remarks>
        public List<string> GetFilePathsForPartialDeletion(BeatmapSet beatmap, IEnumerable<string> modesToDelete)
        {
            var filePathsToDelete = new List<string>();

            var difficultiesToDelete = beatmap.Difficulties
                 .Where(d =>
                 {
                     bool isMatch = modesToDelete.Any(modesToCompare =>
                     string.Equals(d.Mode.Trim(), modesToCompare.Trim(), StringComparison.OrdinalIgnoreCase)
                     );

                     Debug.WriteLine($"Comparison: the map’s mode '{d.Mode}'. Is this mode contained in the deletion list? → Result {isMatch}");

                     return isMatch;
                     
                 });
               

            foreach (var difficulty in difficultiesToDelete)
            {
                try
                {
                    string fileEnding = $"[{difficulty.Version}].osu";

                    string? foundFile = Directory.EnumerateFiles(beatmap.FolderPath, "*.osu")
                                                 .FirstOrDefault(f => f.EndsWith(fileEnding, StringComparison.OrdinalIgnoreCase));

                    if (foundFile != null)
                    {
                        filePathsToDelete.Add(foundFile);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while searching for file '{difficulty.Version}' in folder '{beatmap.FolderPath}': {ex.Message}");
                }
            }
            return filePathsToDelete;
        }

        public async Task deleteTargetAsync(List<string> targets, bool isPermanent)
        {
            await Task.Run(() =>
            {
                foreach (var path in targets)
                {
                    try
                    {
                        if (isPermanent)
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }
                        }
                        else
                        {
                            if (File.Exists(path))
                            {
                                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                            else if (Directory.Exists(path))
                            {
                                FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Falha ao deletar '{path}': {ex.Message}");
                    }
                }
            });
        }

        public Task DeleteTargetsAsync(List<string> targets, bool isPermanent)
        {
            throw new NotImplementedException();
        }
    }
}
