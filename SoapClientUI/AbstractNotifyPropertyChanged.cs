using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoapClientUI
{
    public abstract class AbstractNotifyPropertyChanged : INotifyPropertyChanged
    {
        protected void SetProperty<T>(ref T backingStore, T value, Action onChanged = null, [CallerMemberName]string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                backingStore = value;
                onChanged?.Invoke();
                OnPropertyChanged(propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
