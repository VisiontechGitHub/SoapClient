using System;
using System.Windows.Controls;

namespace SoapClientUI.ViewModels.Commands
{
    public class RefreshCommand : AbstractCommand
    {
        public override bool CanExecute(object parameter)
        {
            return parameter is Frame;
        }

        public override void Execute(object parameter)
        {
            if (parameter is Frame Frame)
            {
                Frame.Content = Activator.CreateInstance(Frame.Content.GetType());
            }
        }
    }
}
