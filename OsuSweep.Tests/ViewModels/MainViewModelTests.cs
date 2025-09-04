using Moq;
using OsuSweep.Core.Models;
using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels;

namespace OsuSweep.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestMethod]
        public async Task StartScanAsync_WhenCalled_UpdatesStatusMessage()
        {
            var mockFolderService = new Mock<IFolderDialogService>();
            var mockBeatmapService = new Mock<IBeatmapService>();
            var mockLocalizationService = new Mock<ILocalizationService>();

            mockBeatmapService.Setup(s => s.ScanSongsFolderAsync(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(10);
                    return new List<BeatmapSet>();
                });

            var viewModel = new MainViewModel(mockFolderService.Object, mockBeatmapService.Object, mockLocalizationService.Object);
            viewModel.SelectedFolderPath = "C:\\fake\\path";

            viewModel.ScanCommand.Execute(null);

            Assert.IsTrue(viewModel.IsScanning);

            await Task.Delay(100);
            Assert.IsFalse(viewModel.IsScanning);
        }
    }
}
