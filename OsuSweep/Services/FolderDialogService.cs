using Microsoft.Win32;
using OsuSweep.Core.Contracts.Services;

namespace OsuSweep.Services
{
    public class FolderDialogService : IFolderDialogService
    {
        public string? ShowDialog()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select the 'Osu!'songs folder"
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FolderName;
            }

            return null;
        }
    }
}
