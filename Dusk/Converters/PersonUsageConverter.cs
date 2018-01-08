using System;
using Dusk.Models;

namespace Dusk.Converters
{
    class PersonUsageConverter : ConverterBase
    {
        public PersonUsageConverter(string field)
        {
            Field = field;
        }

        public string Field { get; set; }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (value == null) return null;
            var count = PersonUsage.Instance[Field, value.ToString()];
            if (count == 0) return null;
            return count;
        }
    }
}
