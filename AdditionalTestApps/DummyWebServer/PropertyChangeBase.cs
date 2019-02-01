using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DummyWebServer
{
    public class PropertyChangeBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(
            ref T storage, 
            T value,
            [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.FirePropertyChanged(propertyName);
            return true;
        }
        protected void FirePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
