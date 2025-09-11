using OsuSweep.Core.Models;
using OsuSweep.Core.Utils;
using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels.Base;
using OsuSweep.ViewModels.Commands;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;


namespace OsuSweep.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IBeatmapService _beatmapService;
        private readonly IFolderDialogService _folderDialogService;
        private readonly ILocalizationService _localizationService;

        private List<string> _deletionTargets = new();
        private string _selectedFolderPath = string.Empty;
        private string _deletionSummaryMessage = string.Empty;
        private string _statusMessage = "Pronto para começar!";
        private bool _isScanning;
        private bool _isReadyForSelection;
        private bool _deleteOsu;
        private bool _deleteTaiko;
        private bool _deleteCatch;
        private bool _deleteMania;
        private bool _isPermanentDelete;
        private LanguageModel _selectedLanguage;


        public ObservableCollection<BeatmapSet> FoundBeatmaps { get; } = new();
        public Action? RequestViewRestart { get; set; }


        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (SetProperty(ref _selectedFolderPath, value))
                    RefreshCommands();
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
            set
            {
                if (SetProperty(ref _isScanning, value))
                    RefreshCommands();
            }
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

        public bool IsPermanentDelete
        {
            get => _isPermanentDelete;
            set => SetProperty(ref _isPermanentDelete, value);
        }

        public LanguageModel SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    ChangeLanguage(value.CultureCode);
                }
            }
        }

        public ObservableCollection<LanguageModel> AvailableLanguages { get; } = new()
        {
            new LanguageModel { DisplayName = "English", CultureCode = "en-US", IconPath = "/OsuSweep;component/Resources/Images/flag_us.png" },
            new LanguageModel { DisplayName = "Português", CultureCode = "pt-BR", IconPath = "/OsuSweep;component/Resources/Images/flag_br.png" },
            new LanguageModel { DisplayName = "Español", CultureCode = "es-ES", IconPath = "/OsuSweep;component/Resources/Images/flag_es.png" }
        };

        private string ConvertModeIdToName(int modeId)
        {
            switch (modeId)
            {
                case 0: return "osu";
                case 1: return "taiko";
                case 2: return "catch";
                case 3: return "mania";

                default: 
                    return "unknown";
            }
        }

        public bool DeleteOsu { get => _deleteOsu; set { if (SetProperty(ref _deleteOsu, value)) _ = UpdateDeletionPreviewAsync(); } }
        public bool DeleteTaiko { get => _deleteTaiko; set { if (SetProperty(ref _deleteTaiko, value)) _ = UpdateDeletionPreviewAsync(); } }
        public bool DeleteCatch { get => _deleteCatch; set { if (SetProperty(ref _deleteCatch, value)) _ = UpdateDeletionPreviewAsync(); } }
        public bool DeleteMania { get => _deleteMania; set { if (SetProperty(ref _deleteMania, value)) _ = UpdateDeletionPreviewAsync(); } }

        public ICommand ScanCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand ConfirmDeletionCommand { get; }




        public MainViewModel(IFolderDialogService folderDialogService, IBeatmapService beatmapService, ILocalizationService localizationService)
        {
            _beatmapService = beatmapService;
            _folderDialogService = folderDialogService;
            _localizationService = localizationService;


            ScanCommand = new AsyncRelayCommand(
                () => StartScanAsync(SelectedFolderPath),
                () => !string.IsNullOrEmpty(SelectedFolderPath) && !IsScanning
            );

            // The 'Select Folder' button can only be clicked if no analysis is in progress.
            SelectFolderCommand = new RelayCommand(
                (parameter) => SelectFolder(),
                (parameter) => !IsScanning
            );

            ConfirmDeletionCommand = new AsyncRelayCommand(
                ExecuteConfirmDeletionAsync,
                () => _deletionTargets.Any() && !IsScanning
            );


            var currentCultureName = CultureInfo.CurrentUICulture.Name;
            _selectedLanguage = AvailableLanguages.FirstOrDefault(lang => lang.CultureCode == currentCultureName) ?? AvailableLanguages.First();
            ChangeLanguage(_selectedLanguage.CultureCode);
        }

        private void ChangeLanguage(string cultureCode)
        {
            _localizationService.SetLanguage(cultureCode);
            RequestViewRestart?.Invoke();
        }

        private void RefreshCommands()
        {
            (ScanCommand as AsyncRelayCommand)?.OnCanExecuteChanged();
            (ConfirmDeletionCommand as AsyncRelayCommand)?.OnCanExecuteChanged();
        }

        /// <summary>
        /// Orchestrates the entire analysis process, starting with folder scanning and then fetching the metadata.
        /// </summary>
        private async Task StartScanAsync(string songsFolderPath)
        {
            if (string.IsNullOrEmpty(songsFolderPath)) return;

            IsReadyForSelection = false;
            IsScanning = true;
            StatusMessage = "";
            FoundBeatmaps.Clear();
            _deletionTargets.Clear();
            DeletionSummaryMessage = string.Empty;

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

            var allBeatmaps = FoundBeatmaps.ToList();

            if (!allBeatmaps.Any())
            {
                StatusMessage = "Análise concluída! Nenhum mapa com ID online foi encontrado.";
                IsReadyForSelection = true;
                return;
            }

            var tasks = new List<Task>();

            foreach (var beatmap in allBeatmaps)
            {
                
                bool hasApiId = beatmap.BeatmapSetId.HasValue;
                if (beatmap.BeatmapSetId.HasValue)
                {
                    tasks.Add(ProcessApiBeatmapAsync(beatmap));
                }
                else
                {
                    tasks.Add(ProcessManualBeatmapAsync(beatmap));
                }
            }

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }

            StatusMessage = "Analise completa!";
            IsReadyForSelection = true;
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


            // Identify which modes the user has selected for deletion.
            var modesToDelete = new List<string>();
            if (DeleteOsu) modesToDelete.Add("osu");
            if (DeleteTaiko) modesToDelete.Add("taiko");
            if (DeleteCatch) modesToDelete.Add("catch");
            if (DeleteMania) modesToDelete.Add("mania");

            _deletionTargets.Clear();

            if (!modesToDelete.Any())
            {
                DeletionSummaryMessage = string.Empty;
                RefreshCommands();
                return;
            }

            try
            {
                IsScanning = true;
                DeletionSummaryMessage = "Calculando....";

                var targets = await Task.Run(() =>
                {
                    var list = new List<string>();
                    var beatmapsAnalyzed = FoundBeatmaps.Where(b => b.IsMetadataLoaded && b.GameModes.Any());

                    foreach (var beatmap in beatmapsAnalyzed)
                    {
                        bool isFullDeletionTarget = !beatmap.GameModes.Except(modesToDelete).Any();
                        if (isFullDeletionTarget)
                        {
                            list.Add(beatmap.FolderPath);
                        }
                        else if (beatmap.GameModes.Any(modesToDelete.Contains))
                        {
                            var filesToDelete = _beatmapService.GetFilePathsForPartialDeletion(beatmap, modesToDelete);
                            list.AddRange(filesToDelete);
                        }
                    }
                    return list;
                });

                _deletionTargets = targets;

                long totalSizeInBytes = await _beatmapService.CalculateTargetsSizeAsync(_deletionTargets);
                int folderCount = _deletionTargets.Count(Directory.Exists);
                int fileCount = _deletionTargets.Count(File.Exists);

                DeletionSummaryMessage = $"Alvos: {folderCount} pastas e {fileCount} arquivos, liberando {FormattingUtils.FormatBytes(totalSizeInBytes)}.";
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


        private async Task ExecuteConfirmDeletionAsync()
        {
            string message = IsPermanentDelete
                ? "Os arquivos serão apagados PERMANENTEMENTE. Esta ação não pode ser desfeita. \n\nDeseja continuar ?"
                : "Os arquivos selecionados serão movidos para a Lixeira. \n\nDeseja continuar ?";

            var result = MessageBox.Show(message, "Confirmação de Limpeza", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsScanning = true;
                StatusMessage = "Limpando Arquivos...";


                await _beatmapService.DeleteTargetsAsync(_deletionTargets, IsPermanentDelete);

                StatusMessage = "Limpeza concluida!";
                _deletionTargets.Clear();
                DeletionSummaryMessage = string.Empty;
                FoundBeatmaps.Clear();

                IsScanning = false;
            }
        }

        private async Task ProcessApiBeatmapAsync(BeatmapSet beatmap)
        {
            var metadata = await _beatmapService.GetBeatmapMetadataAsync(beatmap.BeatmapSetId!.Value);
            if (metadata != null)
            {
                beatmap.Title = metadata.Title;
                beatmap.Artist = metadata.Artist;
                beatmap.Difficulties = metadata.Difficulties;
                beatmap.IsMetadataLoaded = true;
            }
        }

        private async Task ProcessManualBeatmapAsync(BeatmapSet beatmap)
            {
            await Task.Run(() =>
            {
                var modeIds = _beatmapService.GetModesFromBeatmapSetFolder(beatmap.FolderPath);

                var modeNames = modeIds.Select(id => ConvertModeIdToName(id)).ToList();

                beatmap.GameModes = modeNames;
                beatmap.IsMetadataLoaded = true;
            });
        }
    }
}







