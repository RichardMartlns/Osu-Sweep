using OsuSweep.Models;
using OsuSweep.Services;
using OsuSweep.ViewModels.Base;
using OsuSweep.ViewModels.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OsuSweep.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        private readonly BeatmapService _beatmapService;
        private string _selectedFolderPath = string.Empty;
        private bool _isScanning;
        private string _statusMessage = "Pronto para começar!";
        private readonly IFolderDialogService _folderDialogService;

        public ObservableCollection<BeatmapSet> FoundBeatmaps { get; } = new ObservableCollection<BeatmapSet>();
        public ICommand SelectFolderCommand { get; }

        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set => SetProperty(ref _selectedFolderPath, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand ScanCommand { get; }

        public MainViewModel(IFolderDialogService folderDialogService)
        {
            _beatmapService = new BeatmapService();
            _folderDialogService = folderDialogService;

            ScanCommand = new RelayCommand(
                async (parameter) => await StartScanAsync(SelectedFolderPath),
                (parameter) => !IsScanning && !string.IsNullOrEmpty(SelectedFolderPath)
            );

            SelectFolderCommand = new RelayCommand(
                (parameter) => SelectFolder(),
                (parameter) => !IsScanning
             );
        }

        private async Task StartScanAsync(string songsFolderPath)
        {
            if (string.IsNullOrEmpty(songsFolderPath)) return;

            IsScanning = true;
            StatusMessage = "Analisando a pasta 'Songs'... Isso pode levar alguns minutos.";
            FoundBeatmaps.Clear();

            try
            {
                var result = await _beatmapService.ScanSongsFolderAsync(songsFolderPath);

                foreach (var beatmapSet in result)
                {
                    FoundBeatmaps.Add(beatmapSet);
                }

                StatusMessage = $"Análise concluída! {FoundBeatmaps.Count} pastas de beatmaps encontradas.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ocorreu um erro: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }
        private void SelectFolder()
        {
            var selectedPath = _folderDialogService.ShowDialog();
            if (!string.IsNullOrEmpty(selectedPath))
            {
                SelectedFolderPath = selectedPath;
            }
        }
    }
}
