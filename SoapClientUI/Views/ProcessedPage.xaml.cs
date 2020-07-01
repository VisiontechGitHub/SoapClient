using SoapClientUI.ViewModels;
using System.Windows.Controls;

namespace SoapClientUI.Views
{
    public partial class ProcessedPage : Page
    {
        public ProcessedPage()
        {
            InitializeComponent();

            DataContext = new ProcessedPageModel();
        }

        private void ListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer ScrollViewer && ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight && DataContext is ProcessedPageModel ProcessedPageModel && !ProcessedPageModel.FetchEventsCommand.Executing)
            {
                ProcessedPageModel.FetchEventsCommand.Execute(e);
            }
        }
    }
}
