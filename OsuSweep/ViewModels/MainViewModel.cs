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
        private readonly IFolderDialogService _folderDialogService;
        private string _selectedFolderPath = string.Empty;
        private bool _isScanning;
        private string _statusMessage = "Pronto para começar!";


        public ObservableCollection<BeatmapSet> FoundBeatmaps { get; } = new ObservableCollection<BeatmapSet>();


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
        public ICommand SelectFolderCommand { get; }

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
                // The 'Select Folder' button can only be clicked if no analysis is in progress.
                (parameter) => !IsScanning
             );
        }

        /// <summary>
        /// Orchestrates the entire analysis process, starting with folder scanning and then fetching the metadata.
        /// </summary>
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

                await FetchAllBeatmapMetadataAsync();
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

        private async Task FetchAllBeatmapMetadataAsync()
        {
            // Filter the list to get only the beatmaps that have a valid ID.
            var beatmapsToFetch = FoundBeatmaps.Where(b => b.BeatmapSetId.HasValue).ToList();
            if (!beatmapsToFetch.Any()) return;

            int count = 0;
            foreach (var beatmap in beatmapsToFetch)
            {
                count++;
                StatusMessage = $"Buscando metadados... ({count}/{beatmapsToFetch.Count})";

                // Call the service to fetch the current beatmap data.
                var metadata = await _beatmapService.GetBeatmapMetadataAsync(beatmap.BeatmapSetId!.Value);

                if (metadata != null)
                {
                    beatmap.Title = metadata.Title;
                    beatmap.Artist = metadata.Artist;
                    beatmap.GameModes = metadata.GameModes;
                    beatmap.IsMetadataLoaded = true;
                }

                StatusMessage = "Busca de metadados concluída!";

            }
        }
        /// <summary>
        /// Use the dialog service to let the user select the 'Songs' folder.
        /// </summary>
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
