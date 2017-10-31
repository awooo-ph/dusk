using System;
using System.Windows;

namespace Dusk.Converters
{
    class EqualityConverter : ConverterBase
    {

        public enum ReturnTypes
        {
            Visibility, Boolean
        }

        public enum Operations
        {
            Equals,
            GreaterThan,
            LessThan
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
        public long Operand { get; set; } = 0;

        public EqualityConverter(Visibility whenTrue, Visibility whenFalse)
        {
            trueVisibility = whenTrue;
            falseVisibility = whenFalse;
        }

        public EqualityConverter()
        {

        }

        private object _value;
        public EqualityConverter(object binding)
        {
            var temp = (System.Windows.Data.Binding)binding;

            //var conv = new TemplateBindingExpressionConverter();
            //  temp.TemplateBindingExtension.
            _value = binding;
        }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (parameter == null && _value != null)
                return value.Equals(_value) ? trueVisibility : falseVisibility;

            if (Operation == Operations.GreaterThan)
            {
                return (dynamic)value >= Operand ? trueVisibility : falseVisibility;
            }
            if (Operation == Operations.LessThan)
            {
                return (dynamic)value < Operand ? trueVisibility : falseVisibility;
            }
            return value.Equals(parameter) ? trueVisibility : falseVisibility;
        }
    }
}
