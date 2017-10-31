using System.Windows;
using System.Windows.Media.Animation;
using Dusk.Screens;

namespace Dusk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

#if !DEBUG
        private static Task<UpdateManager> _updateManager = null;
#endif

        private MainWindow mainWindow;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            awooo.IsRunning = true;

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 });

            mainWindow = new MainWindow();
            MainWindow = mainWindow;
            MainViewModel.Instance.Dispatcher = mainWindow.Dispatcher;
            mainWindow.DataContext = MainViewModel.Instance;

            if (Dusk.Properties.Settings.Default.ShowSplash)
            {
                var splash = new Splash();
                splash.Show();
            }
            else
            {
                mainWindow.Show();
            }
#if !DEBUG
            Task.Factory.StartNew(CheckForUpdates);
#endif
        }
#if !DEBUG
        private static async void CheckForUpdates()
        {

            using (var mgr = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\dev\Releases"))
            {
                await mgr.UpdateApp();
            }

            //_updateManager = UpdateManager.GitHubUpdateManager("https://github.com/MaterialDesignInXAML/F1InXAML", "F1ix");

            //_updateManager = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\deploy");

            //if (_updateManager.Result.IsInstalledApp)
            //    await _updateManager.Result.UpdateApp();

    }
#endif

        private void App_OnExit(object sender, ExitEventArgs e)
        {
#if !DEBUG
            _updateManager?.Dispose();
#endif
            Dusk.Properties.Settings.Default.Save();
        }
    }
}