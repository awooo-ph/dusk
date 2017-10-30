using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Dusk.Annotations;
using Dusk.Screens;

namespace Dusk
{
    internal sealed class MainViewModel : INotifyPropertyChanged
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

        private bool _IsAdding;

        public bool IsAdding
        {
            get => _IsAdding;
            set
            {
                if (value == _IsAdding) return;
                _IsAdding = value;
                OnPropertyChanged(nameof(IsAdding));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}