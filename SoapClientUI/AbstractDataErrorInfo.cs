using System.ComponentModel;

namespace SoapClientUI
{
    public abstract class AbstractDataErrorInfo : AbstractNotifyPropertyChanged, IDataErrorInfo
    {

        public string Error { get; }

        public string this[string columnName] => ValidateProperty(columnName);

        public virtual string ValidateProperty(string columnName)
        {
            return string.Empty;
        }

    }
}
