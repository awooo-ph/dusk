using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dusk.Data;
using Dusk.Models;
using FastMember;
using Microsoft.Win32;

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

                Attachment.Cache.Remove(NewAttachment);
                
                if (value == null) return;
                if (NewAttachment?.PersonId != value.Id)
                {
                    NewAttachment = new Attachment()
                    {
                        PersonId = value.Id,
                    };
                    Attachment.Cache.Add(NewAttachment);
                }
                
                Attachments.Refresh();
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

        private ICommand _changePictureCommand;

        public ICommand ChangePictureCommand => _changePictureCommand ?? (_changePictureCommand = new DelegateCommand(
            d =>
            {
                var dlg = new OpenFileDialog();
                dlg.Title = "Select Picture";
                dlg.Multiselect = false;
                dlg.Filter =
                    @"All Images|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|
                                                            BMP Files|*.BMP;*.DIB;*.RLE|
                                                            JPEG Files|*.JPG;*.JPEG;*.JPE;*.JFIF|
                                                            GIF Files|*.GIF|
                                                            PNG Files|*.PNG";

                dlg.InitialDirectory = Directory.Exists(Settings.PicturePath) ? Settings.PicturePath :
                    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (!dlg.ShowDialog(Application.Current.MainWindow) ?? false) return;
                Settings.PicturePath = Path.GetDirectoryName(dlg.FileName);
                try
                {
                    Model.Picture = File.ReadAllBytes(dlg.FileName);
                }
                catch (Exception e)
                {
                    Messenger.Default.Broadcast(Messages.Error, e);
                }

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
                if(!value)
                    Model = null;
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

        private Attachment _NewAttachment;

        public Attachment NewAttachment
        {
            get => _NewAttachment;
            set
            {
                if (value == _NewAttachment) return;
                _NewAttachment = value;
                OnPropertyChanged(nameof(NewAttachment));
            }
        }

        private ListCollectionView _attachments;

        public ListCollectionView Attachments
        {
            get
            {
                if (_attachments != null)
                    return _attachments;
                _attachments = new ListCollectionView(Attachment.Cache);
                _attachments.Filter = FilterAttachment;
                
                return _attachments;
            }
        }

        private bool FilterAttachment(object o)
        {
            if (!(o is Attachment a)) return false;
            if (Model == null) return false;
            var p = Model;
            return a.PersonId == p.Id || a.Id == 0;
        }

        private ICommand _attachmentCommand;

        public ICommand AttachmentCommand => _attachmentCommand ?? (_attachmentCommand =
            new DelegateCommand<Attachment>(
                d =>
                {
                    if (d.Id == 0)
                    {
                        AddAttachment(d);
                    }
                    else
                    {

                    }

                }, d => d != null));

        private void AddAttachment(Attachment d)
        {
            if (Model==null) return;
            var p = Model;
            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    Filter =
                        @"All Images|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG|
BMP Files|*.BMP;*.DIB;*.RLE|
JPEG Files|*.JPG;*.JPEG;*.JPE;*.JFIF|
GIF Files|*.GIF|
PNG Files|*.PNG",
                    Title = "Select Picture",
                };
                if (!(dialog.ShowDialog() ?? false))
                    return;

                d.PersonId = p.Id;
                d.Data = File.ReadAllBytes(dialog.FileName);
                d.Description = Path.GetFileNameWithoutExtension(dialog.FileName);
                d.Save();
                NewAttachment = new Attachment()
                {
                    PersonId = p.Id
                };
                Attachment.Cache.Add(NewAttachment);
            }
            catch (Exception e)
            {
                //
            }
        }
    }
}
