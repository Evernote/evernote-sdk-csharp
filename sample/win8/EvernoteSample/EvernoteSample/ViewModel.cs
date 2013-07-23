using Evernote.EDAM.Type;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EvernoteSample
{
    public sealed class ViewModel : INotifyPropertyChanged
    {
        private bool _isVersionOk = false;
        private string _authToken;
        private string _username;

        public ViewModel()
        {
            Notes = new ObservableCollection<Note>();
            Notebooks = new ObservableCollection<Notebook>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Note> Notes
        {
            get;
            private set;
        }

        public ObservableCollection<Notebook> Notebooks
        {
            get;
            private set;
        }

        public bool IsVersionOk
        {
            get { return _isVersionOk; }
            set { SetProperty(ref _isVersionOk, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (!object.Equals(storage, value))
            {
                storage = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}
