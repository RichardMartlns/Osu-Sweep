using OsuSweep.Core.Models;
using OsuSweep.Services;

namespace OsuSweep.Tests.Services
{
    [TestClass]
    public class BeatmapServiceIntegrationTests
    {
        private BeatmapService _beatmapService = null!;
        private string _testDirectory = null!;

        [TestInitialize]
        public void Setup()
        {
            _beatmapService = new BeatmapService();

            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [TestMethod]
        public void GetFilePathsForPartialDeletion_WithMixedModes_ShouldReturnOnlyTargetFiles()
        {
            File.Create(Path.Combine(_testDirectory, "Artist - Title (Creator) [Hard].osu")).Close();
            File.Create(Path.Combine(_testDirectory, "Artist - Title (Creator) [Oni].osu")).Close();
            File.Create(Path.Combine(_testDirectory, "Artist - Title (Creator) [Insane].osu")).Close();

            var beatmap = new BeatmapSet(_testDirectory, 12345)
            {
                Difficulties = new List<BeatmapDifficulty>
                {
                    new BeatmapDifficulty { Version = "Hard", Mode = "osu" },
                    new BeatmapDifficulty { Version = "Oni", Mode = "taiko" },
                    new BeatmapDifficulty { Version = "Insane", Mode = "osu" }
                }
            };

            var modesToDelete = new List<string> { "taiko " };

            var expectedPaths = new List<string>
            {
                Path.Combine(_testDirectory, "Artist - Title (Creator) [Oni].osu")
            };

            var actualPaths = _beatmapService.GetFilePathsForPartialDeletion(beatmap, modesToDelete);


            CollectionAssert.AreEquivalent(expectedPaths, actualPaths);
        }
    }
}
