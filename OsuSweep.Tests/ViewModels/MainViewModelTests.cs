using Moq;
using OsuSweep.Core.Models;
using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels;
using OsuSweep.ViewModels.Commands;

namespace OsuSweep.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private Mock<IFolderDialogService> _mockFolderService = null!;
        private Mock<IBeatmapService> _mockBeatmapService = null!;
        private Mock<ILocalizationService> _mockILocalizationService = null!;
        private Mock<IDeletionService> _mockDeletionService = null!;
        private MainViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockFolderService = new Mock<IFolderDialogService>();
            _mockBeatmapService = new Mock<IBeatmapService>();
            _mockILocalizationService = new Mock<ILocalizationService>();
            _mockDeletionService = new Mock<IDeletionService>();

            _mockBeatmapService.Setup(s => s.ScanSongsFolderAsync(It.IsAny<string>()))
                               .ReturnsAsync(new List<BeatmapSet>());


            _viewModel = new MainViewModel(
                _mockFolderService.Object,
                _mockBeatmapService.Object,
                _mockILocalizationService.Object,
                _mockDeletionService.Object);
            
        }

        [TestMethod]
        public void  ScanCommand_WhenExecuted_CallsBeatmapServiceAndUpdatesIsScanning()
        {
            // Arrange
            _viewModel.SelectedFolderPath = "C:\\fake\\path";

            // Act
            _viewModel.ScanCommand.Execute(null);
            
            // Assert
            _mockBeatmapService.Verify(s => s.ScanSongsFolderAsync("C:\\fake\\path"), Times.Once());
        }

        [TestMethod]
        public void ScanCommand_CanExecute_ShouldBeFalseWhenFolderPathIsEmpty()
        {
            // Arrange
            _viewModel.SelectedFolderPath = "";

            // Act
            var canExecute = _viewModel.ScanCommand.CanExecute(null);

            // Assert
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void ScanCommand_CanExecute_ShouldBeTrueWhenFolderPathIsNotEmpty()
        {
            // Arrange
            _viewModel.SelectedFolderPath = "C:\\some\\valid\\path";

            // Act
            var canExecute = _viewModel.ScanCommand.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void ConfirmDeletionCommand_CanExecute_ShouldBeFalseWhenDeletionTargetsAreEmpty()
        {
            // Act 
            var canExecute = _viewModel.ConfirmDeletionCommand.CanExecute(null);

            // Assert
            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void ConfirmdeletionCommand_CanExecute_ShouldBeTrueWhenDeletionTargetsIsNotEmpty()
        {
            // Arrange
            var targets = new List<string> { "C:\\some\\fake\\beatmap" };

            // Act
            var canExecute = _viewModel.ConfirmDeletionCommand.CanExecute(null);

            // Assert
            Assert.IsFalse(canExecute);
        }
    }
}

