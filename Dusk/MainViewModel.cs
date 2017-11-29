using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dusk.Annotations;
using Dusk.Models;
using Dusk.Properties;
using Dusk.Screens;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
            Messenger.Default.AddListener<Person>(Messages.PersonSelectionChanged,
                person =>
                {
                    OnPropertyChanged(nameof(HasSelected));
                    OnPropertyChanged(nameof(SelectionCount));
                    OnPropertyChanged(nameof(AllSelected));
                });
            Messenger.Default.AddListener<Person>(Messages.PersonSaved,
                person => MessageQueue.Enqueue("Changes saved.", "UNDO", person.Restore));
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

        private ListCollectionView _searchResult;

        public ListCollectionView SearchResult
        {
            get
            {
                if (_searchResult != null) return _searchResult;
                _searchResult = new ListCollectionView(Person.Cache);
                return _searchResult;
            }
        }

        public bool HasSelected => Person.Cache.Any(x => x.IsSelected);

        public long SelectionCount => Person.Cache.Count(x => x.IsSelected);

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

        private ICommand _undoChangesCommand;

        public ICommand UndoChangesCommand => _undoChangesCommand ?? (_undoChangesCommand = new DelegateCommand(d =>
        {
            foreach (var person in Person.Cache.Where(x => x.IsSelected).ToList())
            {
                person.Restore();
            }
        }));


        private bool _allSelected;

        public bool AllSelected
        {
            get { return Person.Cache.All(x => x.IsSelected); }
            set
            {
                //if (value)
                //{
                foreach (var person in Person.Cache)
                {
                    person.IsSelected = value;
                }
                //}
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

        private ICommand _deleteSelectedCommand;
        private List<Person> _deletedPersons;

        public ICommand DeleteSelectedCommand =>
            _deleteSelectedCommand ?? (_deleteSelectedCommand = new DelegateCommand(
                async d =>
                {
                    var ma = (MetroWindow)Application.Current.MainWindow;
                    var msg = await ma.ShowMessageAsync("Confirm Delete",
                        "Are you sure you want to delete the selected items?",
                        MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings()
                        {
                            AffirmativeButtonText = "YES",
                            NegativeButtonText = "NO",
                            ColorScheme = MetroDialogColorScheme.Theme,
                            DefaultButtonFocus = MessageDialogResult.Affirmative,
                        });
                    if (msg == MessageDialogResult.Negative) return;
                    //_deletedPersons?.Clear();
                    var items = Person.Cache.Where(x => x.IsSelected).ToList();
                    foreach (var person in items)
                    {
                        person.Delete();
                        person.IsSelected = false;
                    }
                    MessageQueue.Enqueue($"{items.Count} items deleted.", "UNDO", () =>
                     {
                         foreach (var person in items)
                         {
                             person.Update(nameof(Person.IsDeleted), false);
                             Person.Cache.Add(person);
                         }
                     });
                }));

        //private ICommand _undoDeleteCommand;

        //public ICommand UndoDeleteCommand => _undoDeleteCommand ?? (_undoDeleteCommand = new DelegateCommand(d =>
        //{
        //    if (_deletedPersons == null) return;
        //    foreach (var person in _deletedPersons)
        //    {
        //        person.IsDeleted = false;
        //        Person.Cache.Add(person);
        //    }
        //}));

        private SnackbarMessageQueue _messageQueue;
        public SnackbarMessageQueue MessageQueue => _messageQueue ?? (_messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(7.0)));

        private ICommand _showFullNameCommand;

        public ICommand ToggleFullNameCommand => _showFullNameCommand ?? (_showFullNameCommand = new DelegateCommand(d =>
        {
            Settings.Default.ShowFullName = !Settings.Default.ShowFullName;
            // OnPropertyChanged(nameof(ShowFullName));
        }));

        // public bool ShowFullName => Settings.Default.ShowFullName;

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