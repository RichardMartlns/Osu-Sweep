using Moq;
using OsuSweep.Core.Models;
using OsuSweep.Services;

namespace OsuSweep.Tests.Unit.Services
{
    [TestClass]
    public class DeletionServiceTest
    {
        private Mock<IBeatmapService> _mockBeatmapService = null!;
        private IDeletionService _deletionService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockBeatmapService = new Mock<IBeatmapService>();

            _deletionService = new DeletionService(_mockBeatmapService.Object);
        }

        [TestMethod]
        public async Task CalculateDeletionPreviewAsync_BeatmapWithOnlyTargetModes_ShouldTargetFullFolder()
        {
            // Arrange
            var modesToDelete = new List<string> { "taiko" };
            var taikoMapPath = "C:\\Songs\\123_Taiko_Map";
            var osuMapPath = "C:\\Songs\\456_Osu_Map";

            var allBeatmaps = new List<BeatmapSet>
            {
                new BeatmapSet(taikoMapPath, 123)
                {
                    GameModes = new List<string> {"taiko"},
                    IsMetadataLoaded = true
                },
                new BeatmapSet(osuMapPath, 456)
                {
                    GameModes = new List<string> { "osu", "mania" },
                    IsMetadataLoaded = true
                }
            };

            // Act 
            var result = await _deletionService.CalculateDeletionPreviewAsync(allBeatmaps, modesToDelete);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.DeletionTargets.Count);
            Assert.AreEqual(taikoMapPath, result.DeletionTargets[0]);
        }

        [TestMethod]
        public async Task CalculateDeletionPreviewAsync_BeatmapWithMixedModes_ShouldTargetSpecificFiles()
        {
            // Arrange 
            var modesToDelete = new List<string> { "taiko" };
            var mixedMapPath = "C:\\Songs\\789_Mixed_Map";
            var specificTaikoFilePath = Path.Combine(mixedMapPath, "Artist - Title (Creator) [Oni].osu");

            var allBeatmaps = new List<BeatmapSet>
            {
                new BeatmapSet(mixedMapPath, 789)
                {
                    GameModes = new List<string> { "osu", "taiko", "mania" },
                    IsMetadataLoaded = true
                }
            };

            _mockBeatmapService
                .Setup(s => s.GetFilePathsForPartialDeletion(It.IsAny<BeatmapSet>(), modesToDelete))
                .Returns(new List<string> {  specificTaikoFilePath });

            // Act 
            var result = await _deletionService.CalculateDeletionPreviewAsync(allBeatmaps, modesToDelete);

            // Assert 
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.DeletionTargets.Count);
            Assert.AreEqual(specificTaikoFilePath, result.DeletionTargets[0]);

        }
    }
}
