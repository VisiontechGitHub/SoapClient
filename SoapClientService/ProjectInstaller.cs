using SoapClientService.Logging;
using SoapClientUI;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace SoapClientService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {

        private readonly Logger Logger = new Logger(Configurations.SERVICE_NAME + " Installer");
        public ProjectInstaller()
        {
            InitializeComponent();
            AfterInstall += ProjectInstaller_AfterInstall;
        }

        private void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            ServiceController.GetServices()
                .Where(ServiceController => ServiceController.ServiceName == Configurations.SERVICE_NAME).ToList()
                .ForEach(ServiceController => ServiceController.Start());
        }

        public override void Install(IDictionary stateSaver)
        {
            try
            {
                if (!ServiceController.GetServices().Any(ServiceController => ServiceController.ServiceName == Configurations.SERVICE_NAME))
                {
                    base.Install(stateSaver);
                }
            }
            catch (System.Exception e)
            {
                Logger.LogEvent(e.Message + System.Environment.NewLine + e.StackTrace);
            }
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        public override void Uninstall(IDictionary mySavedState)
        {
            try
            {
                if (ServiceController.GetServices().Any(ServiceController => ServiceController.ServiceName == Configurations.SERVICE_NAME))
                {
                    base.Uninstall(mySavedState);
                }
            }
            catch(System.Exception e)
            {
                Logger.LogEvent(e.Message + System.Environment.NewLine + e.StackTrace);
            }
        }
    }
}
