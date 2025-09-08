using OsuSweep.Services;
using OsuSweep.Services.Localization;
using OsuSweep.ViewModels;
using System.Windows;

namespace OsuSweep.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContextChanged += MainWindow_DataContextChanged;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainViewModel viewModel)
            {
                viewModel.RequestViewRestart = () =>
                {
                    var newWindow = new MainWindow
                    {
                        DataContext = this.DataContext
                    };

                    newWindow.Show();
                    this.Close();
                };
            }
        }
    }
}