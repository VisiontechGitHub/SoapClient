using MaterialDesignThemes.Wpf;
using SoapClientUI.Properties;
using SoapClientUI.ViewModels.Commands;
using SoapClientUI.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoapClientUI.ViewModels
{
    public class MainWindowModel
    {

        public ICollection<MenuItem> MenuItems { get; } = new Collection<MenuItem>();
        public ICommand NavigateSettingsCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand FullscreenCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainWindowModel(Window Window, ContentControl contentControl)
        {
            NavigateSettingsCommand = new NavigationCommand<SettingsPage>(contentControl);
            CloseCommand = new CloseCommand(Window);
            FullscreenCommand = new FullscreenCommand(Window);
            MinimizeCommand = new MinimizeCommand(Window);
            RefreshCommand = new RefreshCommand();

            MenuItems.Add(new MenuItem(PackIconKind.FileReport, Resources.App_Menu_Processed, Resources.App_Menu_Processed_Description, new NavigationCommand<ProcessedPage>(contentControl)));
        }

    }
}
