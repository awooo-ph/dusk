using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Dusk.Properties;
using Dusk.Screens;

namespace Dusk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        private MainWindow mainWindow;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            while (Config.Default.Expired)
            {
                Task.Delay(777);
            }
            
            awooo.Initialize();
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 });

            mainWindow = new MainWindow();
            MainWindow = mainWindow;
            MainViewModel.Instance.Dispatcher = mainWindow.Dispatcher;

            if (Dusk.Properties.Settings.Default.ShowSplash)
            {
                var splash = new Splash();
                splash.Show();
            }
            else
            {
                mainWindow.DataContext = MainViewModel.Instance;
                mainWindow.Show();
            }

        }


        protected override void OnExit(ExitEventArgs e)
        {
            Dusk.Properties.Settings.Default.Save();
            base.OnExit(e);
        }

    }
}