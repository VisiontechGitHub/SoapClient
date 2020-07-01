namespace SoapClientUI.ViewModels
{
    public abstract class AbstractViewModel : AbstractDataErrorInfo
    {

        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                SetProperty(ref isBusy, value);
            }
        }

        private static bool isLogged = false;
        public bool IsLogged
        {
            get { return isLogged; }
            set
            {
                SetProperty(ref isLogged, value);
            }
        }

        private static bool isConnected = false;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                SetProperty(ref isConnected, value);
            }
        }

        public AbstractViewModel()
        {
            IsConnected = true;
        }

    }
}
