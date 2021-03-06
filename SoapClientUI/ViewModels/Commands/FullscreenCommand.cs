﻿using System.Windows;

namespace SoapClientUI.ViewModels.Commands
{
    public class FullscreenCommand : AbstractCommand
    {

        private readonly Window Window;

        public FullscreenCommand(Window Window) {
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
                case WindowState.Maximized:
                    Window.WindowState = WindowState.Normal;
                    break;
                default:
                    Window.WindowState = WindowState.Maximized;
                    break;
            }
        }
    }
}
