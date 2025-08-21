using Microsoft.Win32;

namespace OsuSweep.Services
{
    public class FolderDialogService : IFolderDialogService
    {
        public string? ShowDialog()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Selecione a pasta 'Songs'do osu!"
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FolderName;
            }

            return null;
        }
    }
}
