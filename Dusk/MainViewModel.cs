using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Dusk.Annotations;
using Dusk.Screens;

namespace Dusk
{
    sealed class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private ObservableCollection<UserControl> _screens = new ObservableCollection<UserControl>();
        
        private MainViewModel()
        {
            
        }
        
        private UserControl _CurrentScreen;

        public UserControl CurrentScreen
        {
            get
            {
                if (_CurrentScreen == null)
                {
                    _CurrentScreen = new SearchResult();
                    _screens.Add(_CurrentScreen);
                }
                return _CurrentScreen;
            }
            set
            {
                if (value == _CurrentScreen) return;
                _CurrentScreen = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
