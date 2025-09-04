using System.Windows;
using System.Windows.Controls;
using SharpVectors.Converters;
using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels;

namespace OsuSweep.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var folderDialogService = new FolderDialogService(); // Implemente ou use um mock
            var beatmapService = new BeatmapService(); // Implemente ou use um mock
            var localizationService = new LocalizationService();

            DataContext = new MainViewModel(folderDialogService, beatmapService, localizationService);
        }

        private void SvgViewbox_Loaded(object sender, RoutedEventArgs e)
        {
            var svgViewbox = sender as SvgViewbox;
            if (svgViewbox == null || svgViewbox.Source == null)
            {
                System.Diagnostics.Debug.WriteLine("SvgViewbox não carregou o recurso: " + (svgViewbox?.Source?.ToString() ?? "Nulo"));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SvgViewbox carregado com sucesso: " + svgViewbox.Source);
            }
        }
    }
}