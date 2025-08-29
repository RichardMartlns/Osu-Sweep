using Moq;
using OsuSweep.Core.Models;
using OsuSweep.Services;
using OsuSweep.ViewModels;

namespace OsuSweep.Tests
{
    [TestClass]
    public class MainViewModelTests
    {
        [TestMethod]
        public async Task StartScanAsync_WhenCalled_UpdatesStatusMessage()
        {
            var mockFolderService = new Mock<IFolderDialogService>();
            var mockBeatmapService = new Mock<IBeatmapService>();

            mockBeatmapService.Setup(s => s.ScanSongsFolderAsync(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(10);
                    return new List<BeatmapSet>();
                });

            var viewModel = new MainViewModel(mockFolderService.Object, mockBeatmapService.Object);
            viewModel.SelectedFolderPath = "C:\\fake\\path";

            viewModel.ScanCommand.Execute(null);

            Assert.IsTrue(viewModel.IsScanning);

            await Task.Delay(50);
            Assert.IsFalse(viewModel.IsScanning);
        }
    }
}
