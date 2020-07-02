using System.Diagnostics;
using System.Windows;

namespace SoapClientUI.ViewModels.Commands
{
    public class CloseCommand : AbstractCommand
    {

        private readonly Window Window; 

        public CloseCommand(Window Window)
        {
            this.Window = Window;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            Window.Close();
        }
    }
}
