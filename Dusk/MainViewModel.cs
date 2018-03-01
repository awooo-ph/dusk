using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dusk.Annotations;
using Dusk.Models;
using Dusk.Properties;
using Dusk.Screens.ViewModels;
using FastMember;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace Dusk
{
    internal sealed class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel _instance;



        private MainViewModel()
        {
            Messenger.Default.AddListener<Person>(Messages.ModelSelected,
                person =>
                {
                    OnPropertyChanged(nameof(HasSelected));
                    OnPropertyChanged(nameof(SelectionCount));
                    OnPropertyChanged(nameof(HasSelectedAlive));
                    OnPropertyChanged(nameof(HasSelectedDeceased));
                });
            Messenger.Default.AddListener<Person>(Messages.PersonSaved,
                person => MessageQueue.Enqueue("Changes has been saved.", "UNDO", person.Restore));

            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Settings.EnableUserAccounts))
                {
                    OnPropertyChanged(nameof(IsUsersEnabled));
                }
            };

            ToolsViewModel.Instance.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(HasSelected));
                OnPropertyChanged(nameof(SelectionCount));
                OnPropertyChanged(nameof(HasSelectedAlive));
                OnPropertyChanged(nameof(HasSelectedDeceased));
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private bool? _SelectionState = false;

        public bool? SelectionState
        {
            get => _SelectionState;
            set
            {
                if (value == _SelectionState)
                    return;
                _SelectionState = value;
                OnPropertyChanged(nameof(SelectionState));

                var students = Person.Cache.ToList();
                foreach (var student in students)
                {
                    student.Select(_SelectionState ?? false);
                }
                OnPropertyChanged(nameof(HasSelected));
            }
        }

        private bool _HasSelected;

        public bool HasSelected
        {
            get
            {
                return SearchResult.Cast<Person>().ToList().Any(x=>x.IsSelected);
            }
            set
            {
                _HasSelected = value;
                OnPropertyChanged(nameof(HasSelected));
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
        
        public bool HasSelectedAlive => SearchResult.Cast<Person>().Any(x => x.IsSelected && !x.Deceased);

        public bool HasSelectedDeceased => SearchResult.Cast<Person>().Any(x => x.IsSelected && x.Deceased);

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
                },d=>CurrentUser?.CanDelete??false));

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
        },d=>CurrentUser?.CanEdit??false));

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
        }, d => CurrentUser?.CanEdit ?? false));

        private ICommand _showSettingsCommand;
        public ICommand ShowSettingsCommand => _showSettingsCommand ??
                                               (_showSettingsCommand = new DelegateCommand(d =>
                                               {
                                                   IsSettingsOpen = true;
                                               },d=>HasLoggedIn));

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
                ToolsViewModel.Instance.PropertyChanged += (sender, args) =>
                {
                    _searchResult.Filter = Filter;
                    OnPropertyChanged(nameof(HasSelected));
                    OnPropertyChanged(nameof(SelectionCount));
                    OnPropertyChanged(nameof(HasSelectedAlive));
                    OnPropertyChanged(nameof(HasSelectedDeceased));
                };
               
                return _searchResult;
            }
        }

        private List<string> _printers;
        public ListCollectionView Printers
        {
            get
            {
                if (_printers != null) return (ListCollectionView) CollectionViewSource.GetDefaultView(_printers);
                var printer = new System.Printing.LocalPrintServer();
                var printers = printer.GetPrintQueues();
                _printers = new List<string>(printers.Count());
                var defprinter = LocalPrintServer.GetDefaultPrintQueue().Name;
                foreach (var p in printers)
                {
                    _printers.Add(p.Name);                    
                }
                ((ListCollectionView) CollectionViewSource.GetDefaultView(_printers)).MoveCurrentTo(defprinter);
                return (ListCollectionView) CollectionViewSource.GetDefaultView(_printers);
            }
        }
        
        public long SelectionCount => Person.Cache.Count(x => x.IsSelected);

        private bool Filter(object o)
        {
            var person = o as Person;
            if(person == null) return false;
            if (ToolsViewModel.Instance.FilterResult)
            {
                var filter = ToolsViewModel.Instance;
                if (filter.FilterBarangay && person.BarangayId != filter.Barangay.Id) return false;
                if (filter.FilterCivilStatus && person.CivilStatus != filter.CivilStatus) return false;
                if (filter.FilterDisability && !(person.Disability?.ToLower().Contains(filter.Disability.ToLower())??false))
                    return false;
                if (filter.FilterLivelihood)
                {
                    if(!string.IsNullOrEmpty(filter.Livelihood))
                       if(!(person.Livelihood?.ToLower().Contains(filter.Livelihood.ToLower()) ?? false))
                            return false;
                }
                    
                if (filter.FilterGender && person.Sex != filter.Gender) return false;
                switch (filter.ShowPeople)
                {
                    case ShowPeople.Alive:
                        if (person.Deceased) return false;
                        break;
                    case ShowPeople.Dead:
                        if (!person.Deceased) return false;
                        break;
                }
                if (filter.FilterAge)
                {
                    if (filter.AgeFrom + filter.AgeTo <= 0)
                    {
                        if (person.Age.HasValue) return false;
                    }
                    else
                    {
                        if (person.Age == null)
                            return false;
                        if (person.Age < filter.AgeFrom)
                            return false;
                        if (filter.AgeTo > filter.AgeFrom && person.Age > filter.AgeTo)
                            return false;
                    }
                    
                }
            }
            
            
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

        private ListCollectionView _users;
        private ObservableCollection<User> _usersCache;
        public ListCollectionView Users
        {
            get
            {
                if (_users != null) return _users;
                _usersCache = new ObservableCollection<User>(User.Cache.ToList());
                _usersCache.Add(new User() { Username = "New User" });

                _users = new ListCollectionView(_usersCache);

                User.OnNewItemSaved = user => _usersCache.Add(new User() { Username = "New User" });


                return _users;
            }
        }

        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLoggedIn));
            }
        }

        private bool _HasLoggedIn;

        public bool HasLoggedIn
        {
            get => CurrentUser!=null || !IsUsersEnabled;
            set
            {
                if(value == _HasLoggedIn)
                    return;
                _HasLoggedIn = value;
                OnPropertyChanged(nameof(HasLoggedIn));
            }
        }

        private string _LoginUsername;

        public string LoginUsername
        {
            get => _LoginUsername;
            set
            {
                if(value == _LoginUsername)
                    return;
                _LoginUsername = value;
                OnPropertyChanged(nameof(LoginUsername));
            }
        }
        
        private ICommand _loginCommand;

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand<PasswordBox>(d =>
        {
            var user = User.Cache.FirstOrDefault(x => x.Username.ToLower() == LoginUsername.ToLower());
            if (user == null && User.Cache.Count == 0)
            {
                user = new User()
                {
                    Username = LoginUsername,
                };
                user.Save();
            }
            if (user == null)
            {
                MessageBox.Show("Invalid Username and Password");
                return;
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                user.Update(nameof(User.Password),d.Password);
            }
            if (user.Password != d.Password)
            {
                MessageBox.Show("Invalid Username and Password");
                return;
            }
            CurrentUser = user;
            LoginUsername = "";
            d.Password = "";
        },d=>d?.Password?.Length>0 && !string.IsNullOrEmpty(LoginUsername)));

        private ICommand _showInfoCommand;

        public ICommand ShowInfoCommand => _showInfoCommand ?? (_showInfoCommand = new DelegateCommand<Person>(p =>
        {
            NewPersonViewModel.Instance.Model = p;
            NewPersonViewModel.Instance.IsOpen = true;
        }));

        private ICommand _logoutCommand;

        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(d =>
        {
            CurrentUser = null;
        }));

        public bool IsUsersEnabled => Settings.Default.EnableUserAccounts && User.Cache.Count > 0;

        private ICommand _deleteUserCommand;

        public ICommand DeleteUserCommand => _deleteUserCommand ?? (_deleteUserCommand = new DelegateCommand<User>(
            async usr =>
            {
                var dlg = await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(
                    "Confirm Delete", $"Are you sure you want to delete {usr.Username}?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "YES",
                        NegativeButtonText = "NO",

                    });
                if (dlg == MessageDialogResult.Negative) return;
                usr.Delete();
                _usersCache.Remove(usr);
            }, usr => !usr?.IsNew ?? true));

        private ICommand _changePasswordCommand;

        public ICommand ChangePasswordCommand =>
            _changePasswordCommand ?? (_changePasswordCommand = new DelegateCommand<User>(
                async usr =>
                {
                    var password =
                        await ((MetroWindow)Application.Current.MainWindow).ShowInputAsync("New Password",
                            "Enter new password.");
                    usr.Update(nameof(User.Password), password);

                }, usr => !usr?.IsNew ?? true));

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Dispatcher?.Invoke(() => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
        }

        private ListCollectionView _cashAssistances;

        public ListCollectionView CashAssistances
        {
            get
            {
                if (_cashAssistances != null) return _cashAssistances;
                _cashAssistances = new ListCollectionView(CashAssistance.Cache);
                
                _cashAssistances.Filter = FilterCashAssistance;
                SearchResult.CurrentChanged += (sender, args) =>
                {
                    CashAssistance.SelectedPerson = _searchResult.CurrentItem as Person;
                    if (SearchResult.IsAddingNew) return;
                    _cashAssistances.Refresh();
                };
                return _cashAssistances;
            }
        }

        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand = new DelegateCommand<Person>(
        d =>
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Filter = @"All Images|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|
BMP Files|*.BMP;*.DIB;*.RLE|
JPEG Files|*.JPG;*.JPEG;*.JPE;*.JFIF|
GIF Files|*.GIF|
PNG Files|*.PNG",
                    Title = "Select Picture",
                };
                if (!(dialog.ShowDialog() ?? false))
                    return;

                d.Update(nameof(Person.Picture),
                    File.ReadAllBytes(dialog.FileName));
            }
            catch (Exception e)
            {
                //
            }

        }, d => d != null));

        private bool FilterCashAssistance(object o)
        {
            if (!(o is CashAssistance cash)) return false;
            if (!(SearchResult.CurrentItem is Person p)) return false;
            return cash.PersonId == p.Id;
        }
    }
}