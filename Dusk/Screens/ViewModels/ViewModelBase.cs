using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using Dusk.Annotations;
using Dusk.Models;

namespace Dusk.Screens.ViewModels
{
    abstract class ViewModelBase : MarkupExtension, INotifyPropertyChanged
    {
        protected ViewModelBase()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
