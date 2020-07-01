using MaterialDesignThemes.Wpf;
using System.Windows.Input;

namespace SoapClientUI.ViewModels
{
    public class MenuItem
    {
        public PackIconKind PackIconKind { get; }
        public string Title { get; }
        public string Description { get; }

        public ICommand Command { get; }

        public MenuItem(PackIconKind PackIconKind, string Title, string Description, ICommand Command)
        {
            this.PackIconKind = PackIconKind;
            this.Title = Title;
            this.Description = Description;
            this.Command = Command;
        }
    }
}
