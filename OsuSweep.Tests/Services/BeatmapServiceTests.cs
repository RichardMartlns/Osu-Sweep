using OsuSweep.Core.Models;
using OsuSweep.Services;

namespace OsuSweep.Tests.Services
{
    [TestClass]
    public class BeatmapServiceTest
    {
        [TestMethod]
        public void TryExtractIdFromName_WithStandardFolderName_ShouldReturnCorrectId()
        {
            var beatmapService = new BeatmapService();
            string folderName = "12345 Daisuke Achiwa - BASARA";
            int? expectedId = 12345;

            int? actualId = beatmapService.TryExtractIdFromName(folderName);

            Assert.AreEqual(expectedId, actualId);
        }

        [TestMethod]
        public void TryExtractIdFromName_WithNoLeadingId_ShouldReturnNull()
        {
            var beatmapService = new BeatmapService();
            string folderName = "Daisuke Achiwa - BASARA";

            int? actualId = beatmapService.TryExtractIdFromName(folderName);

            Assert.IsNull(actualId);
        }

        [TestMethod]
        public void TryExtractIdFromName_WithEmptyString_ShouldReturnNull()
        {
            var beatmapService = new BeatmapService();
            string folderName = "";

            int? actualId = beatmapService.TryExtractIdFromName(folderName);

            Assert.IsNull(actualId);
        }

        [TestMethod] // Graveyard maps
        public void TryExtractIdFromName_WithGraveyardFormatName_ShouldReturnNull()
        {
            var beatmanpService = new BeatmapService();
            string folderName = "beatmap-637490146322216146-Kid uses grandmas voice box for auto tune";

            int? actualId = beatmanpService.TryExtractIdFromName(folderName);

            Assert.IsNull(actualId);
        }

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