using System;
using System.Windows.Data;
using Dusk.Models;

namespace Dusk.Converters
{
    class BarangayConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (value == null) return Binding.DoNothing;
            var brgy = (Barangay)((long)value);
            if (brgy == null) return "Not Specified";
            return $"{brgy}";
        }
    }

    class BarangayPopulationConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (value == null) return Binding.DoNothing;
            var brgy = (Barangay)(long)value;
            if (brgy == null) return "N/A";
            return brgy?.Population;
        }
    }
}
