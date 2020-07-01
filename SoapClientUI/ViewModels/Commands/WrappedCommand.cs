using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SoapClientUI.ViewModels.Commands
{
    public class WrappedCommand : AbstractCommand
    {

        public readonly ICollection<Action<object>> PreCallbacks = new Collection<Action<object>>();
        public readonly ICollection<Action<object>> PostCallbacks = new Collection<Action<object>>();
        private readonly ICommand Command;

        private bool executing = false;
        public bool Executing
        {
            get { return executing; }
            set
            {
                SetProperty(ref executing, value);
            }
        }

        public WrappedCommand(ICommand Command) {
            this.Command = Command;
        }

        public override bool CanExecute(object parameter)
        {
            return Command.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            Executing = true;
            PreCallbacks.ToList().ForEach(callback => callback.DynamicInvoke(parameter));
            Task.Run(() => {
                try
                {
                    Command.Execute(parameter);
                }
                finally
                {
                    PostCallbacks.ToList().ForEach(callback => callback.DynamicInvoke(parameter));
                    Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
                    Executing = false;
                }
            });
        }
    }
}
