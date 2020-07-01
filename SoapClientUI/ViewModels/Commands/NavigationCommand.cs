using System;
using System.Windows.Controls;

namespace SoapClientUI.ViewModels.Commands
{
    public class NavigationCommand<T> : AbstractCommand where T : Page
    {
        private readonly ContentControl ContentControl;

        public NavigationCommand(ContentControl ContentControl)
        {
            this.ContentControl = ContentControl;
        }

        public override bool CanExecute(object parameter)
        {
            return ContentControl is object;
        }

        public override void Execute(object parameter)
        {
            ContentControl.Content = Activator.CreateInstance<T>();
        }
    }
}
