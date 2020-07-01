using SoapClientUI.ViewModels;
using SoapClientUI.Views;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SoapClientUI
{
    public partial class MainWindow : Window
    {

        public readonly MainWindowModel model;

        public MainWindow()
        {
            InitializeComponent();

            model = new MainWindowModel(this, Frame);
            DataContext = model;
            Frame.Content = new ProcessedPage();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OpenMenu(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggletButton)
            {
                Menu.Visibility = true.Equals(toggletButton.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
