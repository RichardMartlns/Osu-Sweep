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
        #region Private fields
        private readonly IBeatmapService _beatmapService;
        private readonly IFolderDialogService _folderDialogService;
        private readonly ILocalizationService _localizationService;
        private readonly IDeletionService _deletionService;

        // Internal state
        internal List<string> _deletionTargets = new();
        private string _selectedFolderPath = string.Empty;
        private string _deletionSummaryMessage = string.Empty;
        private string _statusMessage = "Ready to start!";
        private bool _isScanning;
        private bool _isReadyForSelection;
        private bool _deleteOsu;
        private bool _deleteTaiko;
        private bool _deleteCatch;
        private bool _deleteMania;
        private bool _isPermanentDelete;
        private LanguageModel _selectedLanguage;
        #endregion

        #region Public properties (bindable)
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

        // Bindings for checkboxes (execute a preview update when they change)
        public bool DeleteOsu { get => _deleteOsu; set { if (SetProperty(ref _deleteOsu, value)) _ = UpdateDeletionPreviewAsync(); } }
        public bool DeleteTaiko { get => _deleteTaiko; set { if (SetProperty(ref _deleteTaiko, value)) _ = UpdateDeletionPreviewAsync(); } }
        public bool DeleteCatch { get => _deleteCatch; set { if (SetProperty(ref _deleteCatch, value)) _ = UpdateDeletionPreviewAsync(); } }
        public bool DeleteMania { get => _deleteMania; set { if (SetProperty(ref _deleteMania, value)) _ = UpdateDeletionPreviewAsync(); } }
        #endregion

        #region Commands
        public ICommand ScanCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand ConfirmDeletionCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel(IFolderDialogService folderDialogService, IBeatmapService beatmapService, ILocalizationService localizationService, IDeletionService deletionService)
        {
            _beatmapService = beatmapService;
            _folderDialogService = folderDialogService;
            _localizationService = localizationService;
            _deletionService = deletionService;


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
        #endregion

        #region Language/Localization
        private void ChangeLanguage(string cultureCode)
        {
            _localizationService.SetLanguage(cultureCode);
            RequestViewRestart?.Invoke();
        }
        #endregion

        #region Command Utilities
        private void RefreshCommands()
        {
            (ScanCommand as AsyncRelayCommand)?.OnCanExecuteChanged();
            (ConfirmDeletionCommand as AsyncRelayCommand)?.OnCanExecuteChanged();
        }
        #endregion

        #region Folder scanning / metadata
        /// <summary>
        /// Starts the complete beatmap analysis pipeline from a folder path.
        /// </summary>
        /// <remarks>
        /// This is the main entry point for the scan functionality. It clears the previous state,
        /// runs the initial folder scan, and then triggers the detailed metadata fetching.
        /// </remarks>
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

                StatusMessage = $"Analysis complete! {FoundBeatmaps.Count} beatmap folders found.";
                await FetchAllBeatmapMetadataAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }

        /// <summary>
        /// Orchestrates the metadata fetching for the list of already found beatmaps.
        /// </summary>
        /// <remarks>
        /// For each beatmap, it decides whether to fetch data via API (if an ID exists)
        /// or perform a manual parsing of local files. The tasks are executed in parallel for greater efficiency.
        /// </remarks>
        private async Task FetchAllBeatmapMetadataAsync()
        {

            var allBeatmaps = FoundBeatmaps.ToList();

            if (!allBeatmaps.Any())
            {
                StatusMessage = "Analysis complete! No maps with an online ID were found.";
                IsReadyForSelection = true;
                return;
            }

            var tasks = new List<Task>();

            foreach (var beatmap in allBeatmaps)
            {
                
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

            StatusMessage = "Analysis complete!";
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
        #endregion

        #region Deletion Preview
        /// <summary>
        /// Triggered when a game mode selection changes.
        /// Starts the task of calculating the target list and the space to be freed.
        /// </summary>
        private async Task UpdateDeletionPreviewAsync()
        {
            var modesToDelete = new List<string>();
            if (DeleteOsu) modesToDelete.Add("osu");
            if (DeleteTaiko) modesToDelete.Add("taiko");
            if (DeleteCatch) modesToDelete.Add("catch");
            if (DeleteMania) modesToDelete.Add("mania");

            if (!modesToDelete.Any())
            {
                DeletionSummaryMessage = string.Empty;
                _deletionTargets.Clear();
                RefreshCommands();
                return;
            }
            
            try
            {
                IsScanning = true;
                DeletionSummaryMessage = "Calculating...";

                var result = await _deletionService.CalculateDeletionPreviewAsync(FoundBeatmaps, modesToDelete);

                _deletionTargets = result.DeletionTargets;
                DeletionSummaryMessage = result.SummaryMessage;
                RefreshCommands();
            }
            catch (Exception ex)
            {
                DeletionSummaryMessage = $"Error while calculating: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
            }
        }
        #endregion

        #region Deletion confirmation and execution
        private async Task ExecuteConfirmDeletionAsync()
        {
            string message = IsPermanentDelete
                ? "The files will be PERMANENTLY deleted. This action cannot be undone. \n\nDo you want to continue? "
                : "The selected files will be moved to the Recycle Bin. \n\nDo you want to continue ?";

            var result = MessageBox.Show(message, "Confirmação de Limpeza", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsScanning = true;
                StatusMessage = "Cleaning Files...";

                try
                {

                    await _deletionService.DeleteTargetsAsync(_deletionTargets, IsPermanentDelete);

                    StatusMessage = "Cleanup complete!";
                    _deletionTargets.Clear();
                    DeletionSummaryMessage = string.Empty;
                    FoundBeatmaps.Clear();

                    RefreshCommands();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error while deleting: {ex.Message}";
                }
                finally
                {
                    IsScanning = false;
                }
            }
        }
        #endregion

        #region Processing each beatmap (API vs Manual)
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

                var modeNames = modeIds.Select(id => FormattingUtils.ConvertModeIdToName(id)).ToList();

                beatmap.GameModes = modeNames;
                beatmap.IsMetadataLoaded = true;
            });
        }
        #endregion
    }
}







