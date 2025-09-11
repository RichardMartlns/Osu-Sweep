using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using OsuSweep.Core.Models;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using SearchOption = System.IO.SearchOption;


namespace OsuSweep.Services
{
    /// <summary>
    /// Provides services related to the beatmap business logic, such as scanning folders and fetching data from APIs.
    /// </summary>
    public class BeatmapService : IBeatmapService
    {
        #region Fields & Constants
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Regex _leadingDigitsRegex = new Regex(@"^(\d+)", RegexOptions.Compiled);
        private const string ApiBaseUrl = "http://localhost:7032/api/GetBeatmapData";

        private readonly ILogger<BeatmapService>? _logger;
        #endregion

        #region Constructor 
        public BeatmapService(ILogger<BeatmapService>? logger = null)
        {
            _logger = logger;
        }
        #endregion

        #region Scan folder
        /// <summary>
        /// Asynchronously scans a 'Songs' folder to identify all beatmap subfolders.
        /// </summary>
        /// <param name="songsFolderPath">The absolute path to the folder to be scanned.</param>
        /// <returns>A list of <see cref="BeatmapSet"/> objects, each representing a found subfolder.</returns>
        public async Task<List<BeatmapSet>> ScanSongsFolderAsync(string songsFolderPath)
        {
            if (string.IsNullOrWhiteSpace(songsFolderPath))
            {
                return new List<BeatmapSet>();
            }

            return await Task.Run(() =>
            {
                var beatmapSets = new List<BeatmapSet>();

                try
                {
                    // EnumerateDirectories is streaming-friendly for large folders.
                    var directories = Directory.EnumerateDirectories(songsFolderPath);
                    beatmapSets.AddRange(from dir in directories
                                         let folderName = new DirectoryInfo(dir).Name
                                         let beatmapId = TryExtractIdFromName(folderName)
                                         select new BeatmapSet(dir, beatmapId));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed scanning songs folder '{path}'", songsFolderPath);
                    // decide: rethrow or return partial results. We return what we have.
                }

                return beatmapSets;
            });
        }
        #endregion

        #region API Metadata
        /// <summary>
        /// Fetches a beatmap's metadata from the API using its ID.
        /// </summary>
        /// <param name="beatmapId">The ID of the BeatmapSet to fetch.</param>
        /// <returns>The beatmap data from the API, or null if the fetch fails.</returns>
        public async Task<ApiBeatmapData?> GetBeatmapMetadataAsync(int beatmapId)
        {
            var requestUrl = $"{ApiBaseUrl}?id={beatmapId}";

            try
            {
                using var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiBeatmapData>(jsonResponse);
            }
            catch (HttpRequestException hre)
            {
                _logger?.LogWarning(hre, "HTTP request failed for beatmap id {id}", beatmapId);
                return null;
            }
            catch (JsonException je)
            {
                _logger?.LogWarning(je, "Failed to deserialize beatmap metadata for id {id}", beatmapId);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error fetching metadata for id {id}", beatmapId);
                return null;
            }
        }
        #endregion

        #region Helpers: id extraction
        public int? TryExtractIdFromName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return null;
            }

            var match = _leadingDigitsRegex.Match(folderName);
            if (!match.Success)
            {
                return null;
            }

            if (int.TryParse(match.Groups[1].Value, out int id))
            {
                return id;
            }

            return null;
        }
        #endregion

        #region Size calculation
        /// <summary>
        /// Calculates the combined size of a list of targets (folders or files).
        /// </summary>
        public async Task<long> CalculateTargetsSizeAsync(List<string> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return 0;
            }

            return await Task.Run(() =>
            {
                long totalSize = 0;
                foreach (var path in targets)
                {
                    try
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
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error while calculating size for path '{path}'", path);
                    }
                }
                return totalSize;
            });
        }

        private long GetDirectorySize(string folderPath)
        {
            try
            {
                return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                                .Select(p => new FileInfo(p).Length)
                                .Sum();
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger?.LogWarning(ua, "Access denied when calculating size for '{path}'", folderPath);
                return 0;
            }
        }
        #endregion

        #region Partial deletion helper
        /// <summary>
        /// Finds the exact .osu files inside a beatmap folder that should be removed for the selected modes.
        /// </summary>
        public List<string> GetFilePathsForPartialDeletion(BeatmapSet beatmap, IEnumerable<string> modesToDelete)
        {
            var filePathsToDelete = new List<string>();

            if (beatmap == null || modesToDelete == null || !Directory.Exists(beatmap.FolderPath))
            {
                return filePathsToDelete;
            }

            var difficultiesToDelete = beatmap.Difficulties?
                 .Where(d => modesToDelete.Any(mode => string.Equals(d.Mode?.Trim(), mode.Trim(), StringComparison.OrdinalIgnoreCase)))
                 .ToList() ?? new List<BeatmapDifficulty>();

            foreach (var difficulty in difficultiesToDelete)
            {
                try
                {
                    // filenames end with [Version].osu (case-insensitive)
                    string fileEnding = $"[{difficulty.Version}].osu";
                    var foundFile = Directory.EnumerateFiles(beatmap.FolderPath, "*.osu")
                                             .FirstOrDefault(f => f.EndsWith(fileEnding, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrEmpty(foundFile))
                    {
                        filePathsToDelete.Add(foundFile);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error while searching for file '{version}' in folder '{folder}'", difficulty.Version, beatmap.FolderPath);
                }
            }

            return filePathsToDelete;
        }
        #endregion

        #region Delete targets
        /// <summary>
        /// Deletes the given list of targets. If isPermanent==false, items are moved to Recycle Bin (when supported).
        /// This implementation attempts to delete each target individually and logs failures instead of aborting the whole operation.
        /// </summary>
        public async Task DeleteTargetsAsync(List<string> targets, bool isPermanent)
        {
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                foreach (var target in targets)
                {
                    try
                    {
                        if (Directory.Exists(target))
                        {
                            if (isPermanent)
                            {
                                Directory.Delete(target, true);
                            }
                            else
                            {
                                // Move directory to Recycle Bin (Microsoft.VisualBasic helper)
                                FileSystem.DeleteDirectory(target, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                        }
                        else if (File.Exists(target))
                        {
                            if (isPermanent)
                            {
                                File.Delete(target);
                            }
                            else
                            {
                                FileSystem.DeleteFile(target, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }
                        }
                        else
                        {
                            _logger?.LogInformation("Target not found while deleting: {target}", target);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to delete target '{target}' (isPermanent={isPermanent})", target, isPermanent);
                    }
                }
            });
        }
        #endregion

        #region Mode detection from .osu files
        public List<int> GetModesFromBeatmapSetFolder(string folderPath)
        {

            var foundModes = new HashSet<int>();

            if (!Directory.Exists(folderPath))
            {
                return foundModes.ToList();
            }

            try
            {
                var osuFiles = Directory.EnumerateFiles(folderPath, "*.osu");
                foreach (var osuFile in osuFiles)
                {
                    var mode = ParseOsuFileForMode(osuFile);
                    if (mode.HasValue)
                    {
                        foundModes.Add(mode.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, $"Error analyzing folder {folderPath}", folderPath);
            }

            return foundModes.ToList();
        }

        /// <summary>
        /// Reads the initial lines of an .osu file to find and extract the game mode ID.
        /// </summary>
        /// <param name="osuFilePath">The full path to the .osu file to be parsed.</param>
        /// <returns>The game mode ID (0-3) if found; otherwise, null.</returns>
        private int? ParseOsuFileForMode(string osuFilePath)
        {
            try
            {
                // read only the header lines to be faster
                var headerLines = File.ReadLines(osuFilePath).Take(50);
                foreach (var line in headerLines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("Mode:", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmedLine.Split(':', 2);
                        if (parts.Length > 1)
                        {
                            var digits = new string(parts[1].Where(char.IsDigit).ToArray());
                            if (!string.IsNullOrEmpty(digits) && int.TryParse(digits, out int modeValue))
                            {
                                return modeValue;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse mode from .osu file '{path}'", osuFilePath);
            }
            return null;
        }
        #endregion
    }
}
