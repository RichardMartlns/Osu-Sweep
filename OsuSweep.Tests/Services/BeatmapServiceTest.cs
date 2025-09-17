using OsuSweep.Services;

namespace OsuSweep.Tests.Services
{
    [TestClass]
    public class BeatmapServiceTest
    {
        private IBeatmapService _beatmapService = null!;

        [TestInitialize]
        public void Setup()
        {
            _beatmapService = new BeatmapService();
        }

        [TestMethod] // Standard Maps
        public void TryExtractIdFromname_WithStandardFolderName_ShoouldReturnCorrectId()
        {
            // Arrange
            string folderName = "12345 Daisuke Achiwa = BASARA";
            int? expectedId = 12345;

            // Act
            int? actualId = _beatmapService.TryExtractIdFromName(folderName);

            // Assert
            Assert.AreEqual(expectedId, actualId);
        }

        [TestMethod] 
        public void TryExtractIdFromName_WithNoLeadingId_ShouldReturnNull()
        {
            // Arrange
            string folderName = "Daisuke Achiwa - BASARA";

            // Act
            int? actualId = _beatmapService.TryExtractIdFromName(folderName);

            // Assert
            Assert.IsNull(actualId);
        }

        [TestMethod] 
        public void TryExtractIdFromName_WithEmptyString_ShouldReturnNull()
        {
            // Arrange
            var beatmapService = new BeatmapService();
            string folderName = "";

            // Act
            int? actualId = beatmapService.TryExtractIdFromName(folderName);

            // Assert
            Assert.IsNull(actualId);
        }

        [TestMethod] // Graveyard maps
        public void TryExtractIdFromName_WithGraveyardFormatName_ShouldReturnNull()
        {
            // Arrange
            var beatmanpService = new BeatmapService();
            string folderName = "beatmap-637490146322216146-Kid uses grandmas voice box for auto tune";

            // Act
            int? actualId = beatmanpService.TryExtractIdFromName(folderName);

            // Assert
            Assert.IsNull(actualId);
        }
    }
}
