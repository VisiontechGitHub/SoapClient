using Microsoft.Win32;
using System;

namespace SoapClientUI.ViewModels.Commands
{
    public class OpenFileCommand : AbstractCommand
    {

        private readonly Action<string> Callback;
        private readonly string Filter;

        public OpenFileCommand(Action<string> Callback, string Filter) {
            this.Callback = Callback;
            this.Filter = Filter;
        }

        public override bool CanExecute(object parameter)
        {
            return Callback is object;
        }

        public override void Execute(object parameter)
        {
            OpenFileDialog OpenFileDialog = new OpenFileDialog
            {
                Filter = Filter,
                CheckFileExists = true,
                CheckPathExists = true
            };

            bool? result = OpenFileDialog.ShowDialog();

            if (true.Equals(result))
            {
                Callback.Invoke(OpenFileDialog.FileName);
            }
        }
    }
}
