using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Evernote.EDAM.Type;

namespace EvernoteTest
{
    public class ViewModel : INotifyPropertyChanged
    {
        private static ViewModel _theViewModel;
        public static ViewModel TheViewModel
        {
            get
            {
                if(_theViewModel == null)
                {
                    _theViewModel = new ViewModel();
                }
                return _theViewModel;
            }
        }


        public ObservableCollection<Note> Notes { get; private set; }
        public ObservableCollection<Notebook> Notebooks { get; private set; }

        public ViewModel()
        {
            Notes = new ObservableCollection<Note>();
            Notebooks = new ObservableCollection<Notebook>();
        }

        private Notebook _defaultNotebook;
        public Notebook DefaultNotebook
        {
            get { return _defaultNotebook; }
            set
            {
                if (_defaultNotebook == value) return;
                _defaultNotebook = value;
                OnPropertyChanged("DefaultNotebook");
            }
        }

        private bool _versionOK;
        public bool VersionOK
        {
            get { return _versionOK; }
            set
            {
                if (_versionOK == value) return;
                _versionOK = value;
                OnPropertyChanged("VersionOK");
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username == value) return;
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        private string _authToken;
        public string AuthToken
        {
            get { return _authToken; }
            set
            {
                if (_authToken == value) return;
                _authToken = value;
                OnPropertyChanged("AuthToken");
            }
        }
        

        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
