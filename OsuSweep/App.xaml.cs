using OsuSweep.Core.Contracts.Services;
using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels;
using OsuSweep.Views;
using System.Windows;

namespace OsuSweep
{

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var folderDialogService = new FolderDialogService();
            var beatmapService = new BeatmapService();
            var localizationService = new LocalizationService();
            var deletionService = new DeletionService(beatmapService);
           

            var mainViewModel = new MainViewModel(folderDialogService, beatmapService, localizationService, deletionService);

            var mainWindow = new MainWindow();

            mainWindow.DataContext = mainViewModel;

            mainWindow.Show();
        }
    }

}
