using Microsoft.WindowsAPICodePack.Dialogs;
using System;

namespace SoapClientUI.ViewModels.Commands
{
    public class OpenFolderCommand : AbstractCommand
    {

        private readonly Action<string> Callback;

        public OpenFolderCommand(Action<string> Callback) {
            this.Callback = Callback;
        }

        public override bool CanExecute(object parameter)
        {
            return Callback is object;
        }

        public override void Execute(object parameter)
        {
            using (CommonOpenFileDialog CommonOpenFileDialog = new CommonOpenFileDialog())
            {
                CommonOpenFileDialog.IsFolderPicker = true;
                if (CommonFileDialogResult.Ok.Equals(CommonOpenFileDialog.ShowDialog()))
                {
                    Callback.Invoke(CommonOpenFileDialog.FileName);
                }
            }
        }
    }
}
