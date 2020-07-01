using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace SoapClientUI.ViewModels.Commands
{
    public class UninstallServiceCommand : AbstractCommand
    {

        private readonly string Path;
        private readonly string ServiceName;
        public UninstallServiceCommand(string Path, string ServiceName) {
            this.Path = Path;
            this.ServiceName = ServiceName;
        }

        public override bool CanExecute(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Path) && File.Exists(Path) && ServiceController.GetServices().ToList().Any(ServiceController => ServiceName.Equals(ServiceController.ServiceName));
        }

        public override void Execute(object parameter)
        {
            using (
                AssemblyInstaller installer = new AssemblyInstaller
                {
                    Path = Path,
                    UseNewContext = true
                }
            )
            {
                installer.Uninstall(null);
            }
        }
    }
}
