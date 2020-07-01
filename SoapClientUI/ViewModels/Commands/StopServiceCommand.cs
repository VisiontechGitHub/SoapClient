using System.Linq;
using System.ServiceProcess;

namespace SoapClientUI.ViewModels.Commands
{
    public class StopServiceCommand : AbstractCommand
    {
        private readonly string ServiceName;

        public StopServiceCommand(string ServiceName)
        {
            this.ServiceName = ServiceName;
        }

        private bool Filter(ServiceController ServiceController)
        {
            return ServiceName.Equals(ServiceController.ServiceName) && ServiceControllerStatus.Running.Equals(ServiceController.Status);
        }

        public override bool CanExecute(object parameter)
        {
            return ServiceController.GetServices().Any(Filter);
        }

        public override void Execute(object parameter)
        {
            if (ServiceController.GetServices().Where(Filter).First() is ServiceController serviceController)
            {
                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            }
        }
    }
}
