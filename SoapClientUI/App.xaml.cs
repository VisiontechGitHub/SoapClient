using System.Windows;

namespace SoapClientUI
{
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            base.OnStartup(e);
        }

    }
}
