using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dusk.Annotations;
using Dusk.Models;
using Dusk.Properties;
using FastMember;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;

namespace Dusk
{
    internal sealed class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel _instance;



        private MainViewModel()
        {
            Messenger.Default.AddListener<Person>(Messages.PersonSelectionChanged,
                person =>
                {
                    OnPropertyChanged(nameof(HasSelected));
                    OnPropertyChanged(nameof(SelectionCount));
                    OnPropertyChanged(nameof(AllSelected));
                    OnPropertyChanged(nameof(HasSelectedAlive));
                    OnPropertyChanged(nameof(HasSelectedDeceased));
                });
            Messenger.Default.AddListener<Person>(Messages.PersonSaved,
                person => MessageQueue.Enqueue("Changes has been saved.", "UNDO", person.Restore));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        public bool AllSelected
        {
            get { return Person.Cache.All(x => x.IsSelected); }
            set
            {
                foreach (var person in Person.Cache)
                {
                    person.IsSelected = value;
                }
                OnPropertyChanged();
            }
        }

        private bool _isAdding;
        public bool IsAdding
        {
            get => _isAdding;
            set
            {
                if (value == _isAdding) return;
                _isAdding = value;
                OnPropertyChanged(nameof(IsAdding));
            }
        }

        private bool _isUsersOpen;

        public bool IsUsersOpen
        {
            get => _isUsersOpen;
            set
            {
                _isUsersOpen = value;
                OnPropertyChanged();
            }
        }

        private long _settingsIndex = 0;

        public long SettingsIndex
        {
            get => _settingsIndex;
            set
            {
                _settingsIndex = value;
                OnPropertyChanged();
            }
        }

        private bool _isSettingsOpen;
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set
            {
                if (value == _isSettingsOpen) return;
                _isSettingsOpen = value;
                OnPropertyChanged(nameof(IsSettingsOpen));
            }
        }

        public bool HasSelected => Person.Cache.Any(x => x.IsSelected);

        public bool HasSelectedAlive => Person.Cache.Any(x => x.IsSelected && !x.Deceased);

        public bool HasSelectedDeceased => Person.Cache.Any(x => x.IsSelected && x.Deceased);

        private ICommand _deleteSelectedCommand;
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

        private ICommand _resurrectCommand;
        public ICommand ResurrectCommand => _resurrectCommand ?? (_resurrectCommand = new DelegateCommand(d =>
        {
            var items = Person.Cache.Where(x => x.IsSelected && x.Deceased).ToList();
            foreach (var person in items)
            {
                person.Update(nameof(Person.Deceased), false);
            }
            MessageQueue.Enqueue("Resurrection successful.", "UNDO", () =>
            {
                foreach (var person in items)
                {
                    person.Update(nameof(Person.Deceased), true);
                }
            });
        }));

        private ICommand _setDeceasedCommand;
        public ICommand SetDeceasedCommand => _setDeceasedCommand ?? (_setDeceasedCommand = new DelegateCommand(d =>
        {
            var items = Person.Cache.Where(x => x.IsSelected && !x.Deceased).ToList();
            foreach (var person in items)
            {
                person.Update(nameof(Person.Deceased), true);
            }
            MessageQueue.Enqueue("Selected items are dead.", "REVIVE", () =>
            {
                foreach (var person in items)
                {
                    person.Update(nameof(Person.Deceased), false);
                }
            });
        }));

        private ICommand _showSettingsCommand;
        public ICommand ShowSettingsCommand => _showSettingsCommand ??
                                               (_showSettingsCommand = new DelegateCommand(d =>
                                               {
                                                   IsSettingsOpen = true;
                                               }));

        private ICommand _gotoSettingsCommand;

        public ICommand GotoSettingsCommand => _gotoSettingsCommand ?? (_gotoSettingsCommand = new DelegateCommand<string>(d =>
        {
            SettingsIndex = long.Parse(d);
        }));

        private ICommand _showUsersCommand;
        public ICommand ShowUsersCommand => _showUsersCommand ?? (_showUsersCommand = new DelegateCommand(d =>
        {
            SettingsIndex = 1;
        }));

        private ICommand _showFullNameCommand;

        public ICommand ToggleFullNameCommand => _showFullNameCommand ?? (_showFullNameCommand = new DelegateCommand(
                                                     d =>
                                                     {
                                                         Settings.Default.ShowFullName = !Settings.Default.ShowFullName;
                                                         // OnPropertyChanged(nameof(ShowFullName));
                                                     }));

        private ICommand _undoChangesCommand;
        public ICommand UndoChangesCommand => _undoChangesCommand ?? (_undoChangesCommand = new DelegateCommand(d =>
        {
            foreach (var person in Person.Cache.Where(x => x.IsSelected).ToList())
            {
                person.Restore();
            }
        }));

        public Dispatcher Dispatcher { get; set; }

        private SnackbarMessageQueue _messageQueue;
        public SnackbarMessageQueue MessageQueue =>
            _messageQueue ?? (_messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(7.0)));

        private string _searchKeyword;
        private System.Timers.Timer _searchTimer;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();

                _searchTimer?.Stop();
                _searchTimer?.Dispose();
                _searchTimer = new System.Timers.Timer(471);
                _searchTimer.AutoReset = false;
                _searchTimer.Elapsed += async (sender, args) =>
                {
                    await Dispatcher.InvokeAsync(() => SearchResult.Filter = Filter, DispatcherPriority.Background);
                    _searchTimer.Dispose();
                };
                _searchTimer.Start();
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

        public long SelectionCount => Person.Cache.Count(x => x.IsSelected);

        private bool Filter(object o)
        {
            if (string.IsNullOrWhiteSpace(SearchKeyword)) return true;

            var props = new List<string>()
            {
                nameof(Person.Firstname),
                nameof(Person.Lastname),
                nameof(Person.Middlename),
                nameof(Person.Disability),
                nameof(Person.Livelihood)
            };

            var obj = ObjectAccessor.Create(o);

            foreach (var prop in props)
            {
                if (obj[prop]?.ToString().ToLower().Contains(SearchKeyword.ToLower()) ?? false)
                    return true;
            }

            return false;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Dispatcher.Invoke(() => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
        }

        public enum SettingsTitles
        {
            Settings,
            [Description("User Accounts")]
            UserAccounts,
        }
    }
}