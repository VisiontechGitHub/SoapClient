using System;
using System.Windows.Input;

namespace SoapClientUI.ViewModels.Commands
{
    public abstract class AbstractCommand : AbstractNotifyPropertyChanged, ICommand
    {

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

    }
}
