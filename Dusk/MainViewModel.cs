using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Dusk.Annotations;
using Dusk.Screens;
using MaterialDesignThemes.Wpf;

namespace Dusk
{
    internal sealed class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private ObservableCollection<UserControl> _screens = new ObservableCollection<UserControl>();

        public Dispatcher Dispatcher { get; set; }

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
            Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        private bool _allSelected;

        public bool AllSelected
        {
            get { return _allSelected; }
            set
            {
                _allSelected = value; 
                OnPropertyChanged();
            }
        }

        private bool _IsUserMenuOpen;

        public bool IsUserMenuOpen
        {
            get => _IsUserMenuOpen;
            set
            {
                if (value == _IsUserMenuOpen) return;
                _IsUserMenuOpen = value;
                OnPropertyChanged(nameof(IsUserMenuOpen));
            }
        }

        private ICommand _accountSettingsCommand;

        public ICommand AccountSettingsCommand =>
            _accountSettingsCommand ?? (_accountSettingsCommand = new DelegateCommand(
                d =>
                {
                    IsUserMenuOpen = false;
                    SetStatus("Not implemented yet!", PackIconKind.Alert, 4000);
                }));

        private ICommand _logoutCommand;

        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(d =>
        {
            IsUserMenuOpen = false;
            SetStatus("Not implemented yet!", PackIconKind.Alert, 4000);
        }));

        private Timer _statusTimer = null;
        public void SetStatus(string message, PackIconKind icon, long timeout = 47000)
        {
            StatusIcon = icon;
            StatusMessage = message;
            _statusTimer?.Dispose();
            _statusTimer = new Timer(state =>
            {
                StatusIcon = PackIconKind.Xaml;
                StatusMessage = "";
            }, null, timeout, int.MaxValue);
        }

        private PackIconKind _StatusIcon = PackIconKind.Xaml;

        public PackIconKind StatusIcon
        {
            get => _StatusIcon;
            set
            {
                if (value == _StatusIcon) return;
                _StatusIcon = value;
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(StatusIconVisibility));
            }
        }


        private string _StatusMessage;

        public string StatusMessage
        {
            get => _StatusMessage;
            set
            {
                if (value == _StatusMessage) return;
                _StatusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public Visibility StatusIconVisibility
        {
            get => StatusIcon != PackIconKind.Xaml ? Visibility.Visible : Visibility.Collapsed;
            set => OnPropertyChanged();
        }

        public bool EnableUserAccounts
        {
            get => Properties.Settings.Default.EnableUserAccounts;
            set
            {
                if (value == Properties.Settings.Default.EnableUserAccounts) return;
                Properties.Settings.Default.EnableUserAccounts = value;
                OnPropertyChanged(nameof(EnableUserAccounts));
            }
        }


        private ICommand _showSettingsCommand;

        public ICommand ShowSettingsCommand => _showSettingsCommand ?? (_showSettingsCommand = new DelegateCommand(d =>
        {
            IsSettingsOpen = true;
        }));

        private bool _IsSettingsOpen;

        public bool IsSettingsOpen
        {
            get => _IsSettingsOpen;
            set
            {
                if (value == _IsSettingsOpen) return;
                _IsSettingsOpen = value;
                OnPropertyChanged(nameof(IsSettingsOpen));
            }
        }


    }


}