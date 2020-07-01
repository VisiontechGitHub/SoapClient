using System;
using System.Linq;
using System.ServiceProcess;

namespace SoapClientUI.ViewModels.Commands
{
    public class StartServiceCommand : AbstractCommand
    {

        private readonly string ServiceName;
        private readonly string[] Args;

        public StartServiceCommand(string ServiceName, params string[] Args) {
            this.ServiceName = ServiceName;
            this.Args = Args;
        }

        private bool Filter(ServiceController ServiceController)
        {
            return ServiceName.Equals(ServiceController.ServiceName) && ServiceControllerStatus.Stopped.Equals(ServiceController.Status);
        }

        public override bool CanExecute(object parameter)
        {
            return ServiceController.GetServices().Any(Filter);
        }

        public override void Execute(object parameter)
        {
            TimeSpan StartTimeout = new TimeSpan(0, 0, 10);
            if (ServiceController.GetServices().Where(Filter).First() is ServiceController serviceController)
            {
                serviceController.Start(Args);
                serviceController.WaitForStatus(ServiceControllerStatus.Running, StartTimeout);
            }
        }
    }
}
