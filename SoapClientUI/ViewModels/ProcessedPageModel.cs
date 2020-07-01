using Newtonsoft.Json;
using SoapClientUI.ViewModels.Commands;
using SoapClientLibrary;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Controls;
using System.Windows;

namespace SoapClientUI.ViewModels
{
    public class ProcessedPageModel : AbstractViewModel
    {

        public bool ServiceRunning {
            get {
                return ServiceController.GetServices().Any(ServiceController => Configurations.SERVICE_NAME.Equals(ServiceController.ServiceName) && ServiceControllerStatus.Running.Equals(ServiceController.Status));
            }
        }
        public bool ServiceStopped
        {
            get
            {
                return !ServiceRunning;
            }
        }
        public WrappedCommand FetchEventsCommand { get; }

        public ObservableCollection<OmaEvent> OmaEvents { get; } = new ObservableCollection<OmaEvent>();

        private readonly int Pagination = 10;
        private readonly EventLogQuery EventLogQuery =
            new EventLogQuery(
                "Application",
                PathType.LogName,
                string.Join(
                    " and ",
                    string.Format("*[System/EventID={0}]", Convert.ToInt32(EventTypes.Result)),
                    string.Format("*[System/Provider/@Name='{0}']", Configurations.SERVICE_NAME)
                )
            ) { ReverseDirection = true };

        public ProcessedPageModel() {
            FetchEventsCommand = new WrappedCommand(new FetchEventsCommand(EventLogQuery, Pagination, Collection => Collection.ToList().ForEach(EventRecord => {
                try
                {
                    OmaEvents.Add(JsonConvert.DeserializeObject<OmaEvent>(EventRecord.Properties.First().Value.ToString()));
                }
                catch { }
            })));
            FetchEventsCommand.PostCallbacks.Add(
                (object e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (e is ScrollChangedEventArgs Scea && Scea.OriginalSource is ScrollViewer ScrollViewer && ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
                        {
                            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ScrollableHeight - 1);
                        }
                    });
                }
            );
        }

     }

}
