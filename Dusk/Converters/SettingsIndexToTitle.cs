using System;

namespace Dusk.Converters
{
    class SettingsIndexToTitle : ConverterBase
    {
        private string[] _titles = new[]
        {
            "Settings",
            "User Accounts"
        };
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return _titles[(long)value];
        }
    }
}
