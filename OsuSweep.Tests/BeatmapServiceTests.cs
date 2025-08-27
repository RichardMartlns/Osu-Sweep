using OsuSweep.Services;

namespace OsuSweep.Tests
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
    }
}
