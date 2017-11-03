using System;
using System.Windows;

namespace Dusk.Converters
{
    class EqualityConverter : ConverterBase
    {

        public enum ReturnTypes
        {
            Visibility, Boolean, Long, Object
        }

        public enum Operations
        {
            Equals,
            GreaterThan,
            LessThan,
            NotEquals,
        }

        private object trueVisibility = Visibility.Visible;
        private object falseVisibility = Visibility.Collapsed;

        private ReturnTypes _returnType = ReturnTypes.Visibility;

        public ReturnTypes ReturnType
        {
            get => _returnType;
            set
            {
                _returnType = value;
                if (value == ReturnTypes.Boolean)
                {
                    trueVisibility = true;
                    falseVisibility = false;
                }
            }
        }

        public Operations Operation { get; set; } = Operations.Equals;
        public object Operand { get; set; } = 0;

        public EqualityConverter(object whenTrue, object whenFalse)
        {
            trueVisibility = whenTrue;
            falseVisibility = whenFalse;
        }

        public EqualityConverter()
        {

        }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            // if (targetType != trueVisibility.GetType()) return Binding.DoNothing;

            if (value == null)
                return falseVisibility;

            if (parameter != null)
                return value.Equals(parameter) ? trueVisibility : falseVisibility;

            if (Operation == Operations.GreaterThan)
            {
                return (double)value >= System.Convert.ToDouble(Operand) ? trueVisibility : falseVisibility;
            }
            if (Operation == Operations.LessThan)
            {
                return (double)value < System.Convert.ToDouble(Operand) ? trueVisibility : falseVisibility;
            }
            if (Operation == Operations.NotEquals)
                return value.Equals(Operand) ? falseVisibility : trueVisibility;
            return value.Equals(Operand) ? trueVisibility : falseVisibility;
        }
    }
}
