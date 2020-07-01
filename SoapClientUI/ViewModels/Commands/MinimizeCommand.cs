using System.Windows;

namespace SoapClientUI.ViewModels.Commands
{
    public class MinimizeCommand : AbstractCommand
    {

        private readonly Window Window;

        public MinimizeCommand(Window Window) {
            this.Window = Window;
        }

        public override bool CanExecute(object parameter)
        {
            return Window is object;
        }

        public override void Execute(object parameter)
        {
            switch (Window.WindowState)
            {
                case WindowState.Minimized:
                    Window.WindowState = WindowState.Normal;
                    break;
                default:
                    Window.WindowState = WindowState.Minimized;
                    break;
            }
        }
    }
}
