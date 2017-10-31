using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dusk.Screens.Helpers
{
    public sealed class TextBoxHelper : TextBox
    {

        public static readonly DependencyProperty SuggestionsSourceProperty = DependencyProperty.RegisterAttached(
            "SuggestionsSource", typeof(IEnumerable<string>), typeof(TextBoxHelper), new UIPropertyMetadata(null, SuggestionSourceChanged));

        public static void SetSuggestionsSource(TextBox element, IEnumerable<string> value)
        {
            element.SetValue(SuggestionsSourceProperty, value);
        }

        public static IEnumerable<string> GetSuggestionsSource(TextBox element)
        {
            return (IEnumerable<string>)element.GetValue(SuggestionsSourceProperty);
        }


        private static void SuggestionSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!awooo.IsRunning) return;
            if (!(sender is TextBox tb)) return;


            tb.TextChanged -= OnTextChanged;
            tb.PreviewKeyDown -= OnPreviewKeyDown;
            if (e.NewValue == null) return;
            tb.TextChanged += OnTextChanged;
            tb.PreviewKeyDown += OnPreviewKeyDown;
        }


        static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (!(e.OriginalSource is TextBox tb)) return;

            if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
            {
                tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Properties.Settings.Default.EnableSuggestions) return;

            if ((from change in e.Changes where change.RemovedLength > 0 select change).Any() &&
                (from change in e.Changes where change.AddedLength > 0 select change).Any() == false) return;


            if (!(e.OriginalSource is TextBox tb)) return;


            var values = GetSuggestionsSource(tb);

            if (values == null || string.IsNullOrEmpty(tb.Text)) return;

            var startIndex = 0;
            var matchingString = tb.Text;

            if (string.IsNullOrEmpty(matchingString)) return;

            var textLength = matchingString.Length;

            var match =
            (
                from
                    value
                in
                (
                    from subvalue
                    in values
                    where subvalue != null && subvalue.Length >= textLength
                    select subvalue
                )
                where value.Substring(0, textLength).Equals(matchingString, StringComparison.CurrentCultureIgnoreCase)
                select value.Substring(textLength, value.Length - textLength)
            ).FirstOrDefault();


            if (string.IsNullOrEmpty(match)) return;

            var matchStart = (startIndex + matchingString.Length);
            tb.TextChanged -= OnTextChanged;
            tb.Text += match;
            tb.CaretIndex = matchStart;
            tb.SelectionStart = matchStart;
            tb.SelectionLength = (tb.Text.Length - startIndex);
            tb.TextChanged += OnTextChanged;
        }
    }
}
