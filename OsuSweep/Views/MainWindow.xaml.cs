using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels;
using System.Windows;

namespace OsuSweep.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = new MainViewModel (new FolderDialogService(), new BeatmapService(), new LocalizationService());
        }
    }
}