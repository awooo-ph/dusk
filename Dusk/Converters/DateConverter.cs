using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dusk.Converters
{
    class DateConverter : ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is DateTime date)) return "N/A";
            if (date > DateTime.Now.AddYears(-147))
                return date.ToString("d");
            return "N/A";
        }
    }
}
