using System;
using System.Linq;
using System.Windows.Input;
using Dusk.Data;
using Dusk.Models;
using FastMember;

namespace Dusk.Screens.ViewModels
{
    class NewPersonViewModel : ViewModelBase
    {
        private static NewPersonViewModel _instance;
        private Properties.Settings Settings = Properties.Settings.Default;

        public static NewPersonViewModel Instance => _instance ?? (_instance = new NewPersonViewModel());

        private Person _Model;

        public Person Model
        {
            get => _Model ?? (_Model = new Person() { BarangayId = Properties.Settings.Default.LastBarangay });
            set
            {
                if (value == _Model) return;
                _Model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        private ICommand _resetCommand;

        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new DelegateCommand(d =>
        {
            Model = null;
        }));

        private ICommand _cancelCommand;

        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new DelegateCommand(d =>
        {
            Model = null;
            IsOpen = false;
        }));

        private ICommand _saveCommand;

        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(d =>
        {
            Properties.Settings.Default.LastBarangay = Model.BarangayId;
            Model.Save();
            CacheValues(Model);
            Model = null;
            IsOpen = false;

        }, d => Model?.CanSave() ?? false));

        private void CacheValues(Person person)
        {
            var cache = PersonCache.Instance;
            var props = typeof(Person).GetProperties();
            var model = ObjectAccessor.Create(person);
            foreach (var prop in props)
            {
                if (prop.IsDefined(typeof(IgnoreAttribute), true) || !prop.CanWrite) continue;
                if (((model[prop.Name] + "").Trim() != "") && cache[prop.Name].All(x => x.ToLower() != model[prop.Name].ToString().ToLower()))
                    cache[prop.Name].Add(model[prop.Name].ToString());
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }

        private bool _IsOpen;

        public bool IsOpen
        {
            get => _IsOpen;
            set
            {
                if (value == _IsOpen) return;
                _IsOpen = value;
                OnPropertyChanged(nameof(IsOpen));
            }
        }

        private ICommand _hideUsageCommand;

        public ICommand HideUsageCommand => _hideUsageCommand ?? (_hideUsageCommand = new DelegateCommand(d =>
        {
            Properties.Settings.Default.ShowUsageCount = false;
        }, o => Properties.Settings.Default.ShowUsageCount));

        private ICommand _showUsageCommand;

        public ICommand ShowUsageCommand => _showUsageCommand ?? (_showUsageCommand = new DelegateCommand(d =>
        {
            Properties.Settings.Default.ShowUsageCount = true;
        }, o => !Properties.Settings.Default.ShowUsageCount));


        public bool EnableSuggestion
        {
            get => Settings.EnableSuggestions;
            set
            {
                if (value == Settings.EnableSuggestions) return;
                Settings.EnableSuggestions = value;
                Settings.Save();
                OnPropertyChanged(nameof(EnableSuggestion));
            }
        }

        public bool IsUsageCountVisible
        {
            get => Settings.ShowUsageCount;
            set
            {
                if (value == Settings.ShowUsageCount) return;
                Settings.ShowUsageCount = value;
                Settings.Save();
                OnPropertyChanged(nameof(IsUsageCountVisible));
            }
        }


    }
}
