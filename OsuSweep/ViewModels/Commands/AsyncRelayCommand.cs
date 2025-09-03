using System.Windows.Input;

namespace OsuSweep.ViewModels.Commands
{
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            // The command can only be executed if it is not already running AND
            // if the custom condition (if any) is true.
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        // Notifies the UI to disable or re-enable the button.
        public async void Execute(object? parameter)
        {
            _isExecuting = true;
            OnCanExecuteChanged();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                OnCanExecuteChanged();
            }
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
