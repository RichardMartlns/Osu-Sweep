using OsuSweep.Core.Models;
using OsuSweep.Services;
using OsuSweep.ViewModels.Base;
using OsuSweep.ViewModels.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace OsuSweep.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        private readonly IBeatmapService _beatmapService;
        private readonly IFolderDialogService _folderDialogService;
        private string _selectedFolderPath = string.Empty;
        private string _deletionSummaryMessage = string.Empty;
        private bool _isScanning;
        private bool _isReadyForSelection;
        private string _statusMessage = "Pronto para começar!";

        // Indicates whether the mode’s beatmaps should be selected for deletion.
        private bool _deleteOsu;
        private bool _deleteTaiko;
        private bool _deleteCatch;
        private bool _deleteMania;

        public ObservableCollection<BeatmapSet> FoundBeatmaps { get; } = new ObservableCollection<BeatmapSet>();


        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (SetProperty(ref _selectedFolderPath, value))
                {
                    (ScanCommand as AsyncRelayCommand)?.OnCanExecuteChanged();
                }
            }
        }

        public bool IsReadyForSelection
        {
            get => _isReadyForSelection;
            set => SetProperty(ref _isReadyForSelection, value);
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

        public string DeletionSummaryMessage
        {
            get => _deletionSummaryMessage;
            set => SetProperty(ref _deletionSummaryMessage, value);
        }

        public bool DeleteOsu
        {
            get => _deleteOsu;
            set
            {
                if (SetProperty(ref _deleteOsu, value))
                    _ = UpdateDeletionPreviewAsync();
            }

        }

        public bool DeleteTaiko
        {
            get => _deleteTaiko;
            set
            {
                if (SetProperty(ref _deleteTaiko, value))
                    _ = UpdateDeletionPreviewAsync();
            }
        }

        public bool DeleteCatch
        {
            get => _deleteCatch;
            set
            {
                if (SetProperty(ref _deleteCatch, value))
                    _ = UpdateDeletionPreviewAsync();
            }
        }

        public bool DeleteMania
        {
            get => _deleteMania;
            set
            {
                if (SetProperty(ref _deleteMania, value))
                    _ = UpdateDeletionPreviewAsync();
            }
        }

        public ICommand ScanCommand { get; }
        public ICommand SelectFolderCommand { get; }

        public MainViewModel(IFolderDialogService folderDialogService, IBeatmapService beatmapService)
        {
            _beatmapService = beatmapService;
            _folderDialogService = folderDialogService;
            

            ScanCommand = new AsyncRelayCommand(
                () => StartScanAsync(SelectedFolderPath),
                () => !string.IsNullOrEmpty(SelectedFolderPath)
            );

            // The 'Select Folder' button can only be clicked if no analysis is in progress.
            SelectFolderCommand = new RelayCommand(
                (parameter) => SelectFolder(),

                (parameter) => !IsScanning
             );
        }

        /// <summary>
        /// Orchestrates the entire analysis process, starting with folder scanning and then fetching the metadata.
        /// </summary>
        private async Task StartScanAsync(string songsFolderPath)
        {
            if (string.IsNullOrEmpty(songsFolderPath)) return;

            IsReadyForSelection = false;
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
                    beatmap.GameModes = metadata.Difficulties.Select(d => d.Mode).Distinct().ToList();
                    beatmap.IsMetadataLoaded = true;
                    beatmap.Difficulties = metadata.Difficulties;
                }

                StatusMessage = "Busca de metadados concluída!";
                IsReadyForSelection = true;
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

        /// <summary>
        /// Triggered when a game mode selection changes.
        /// Starts the task of calculating the target list and the space to be freed.
        /// </summary>
        private async Task UpdateDeletionPreviewAsync()
        {
            Debug.WriteLine("Recalculando a pré visualização da deleção...");

            // Identify which modes the user has selected for deletion.
            var modesToDelete = new List<string>();
            if (DeleteOsu) modesToDelete.Add("osu");
            if (DeleteTaiko) modesToDelete.Add("taiko");
            if (DeleteCatch) modesToDelete.Add("catch");
            if (DeleteMania) modesToDelete.Add("mania");

            Debug.WriteLine($"Modos selecionados para deleção: {string.Join(", ", modesToDelete)}");

            if (modesToDelete.Count == 0)
            {
                DeletionSummaryMessage = string.Empty;
                return;
            }

            try
            {
                IsScanning = true;
                DeletionSummaryMessage = "Calculando....";

                var deletionTargets = new List<string>();
                var beatmapsAnalyzed = FoundBeatmaps.Where(b => b.IsMetadataLoaded && b.GameModes.Any());
                foreach (var beatmap in beatmapsAnalyzed)
                {
                    bool isFullDeletionTarget = !beatmap.GameModes.Except(modesToDelete).Any();
                    if (isFullDeletionTarget)
                    {
                        deletionTargets.Add(beatmap.FolderPath);
                    }
                    else
                    {
                        bool isPartialDeletionTarget = beatmap.GameModes.Any(mode => modesToDelete.Contains(mode));
                        if (isPartialDeletionTarget)
                        {
                            Debug.WriteLine($"[Deleção Parcial] A pasta '{beatmap.FolderPath}' contém modos a serem apagados.");
                        }
                    }
                }
                long totalSizeInBytes = await _beatmapService.CalculateTargetsSizeAsync(deletionTargets);

                DeletionSummaryMessage = $"{deletionTargets.Count} pastas a serem deletadas, liberando {FormatBytes(totalSizeInBytes)}";
            }
            catch (Exception ex)
            {
                DeletionSummaryMessage = $"Erro ao calcular: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes;
            while (dblSByte >= 1024 && i < suffixes.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:0.##} {suffixes[i]}";
        }
    }
}







