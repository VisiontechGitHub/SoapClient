using SoapClientUI.ViewModels;
using System.Windows.Controls;

namespace SoapClientUI.Views
{
    public partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            InitializeComponent();

            DataContext = new SettingsPageModel(PasswordBox);
        }
    }
}
