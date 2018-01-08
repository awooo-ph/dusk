using System.Windows.Controls;

namespace Dusk.Screens
{
    /// <summary>
    /// Interaction logic for SearchResult.xaml
    /// </summary>
    public partial class SearchResult : UserControl
    {
        public SearchResult()
        {
            InitializeComponent();
        }

        private void DataGrid_OnRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //if (e.EditAction == DataGridEditAction.Commit)
            //{
            //    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
            //        new Action(() =>
            //        {
            //            var person = (Person)e.Row.Item;
            //            if (!person.CanSave())
            //                person.Reset();
            //            else
            //                person.Save();
            //        }));


            //}
        }

    }
}