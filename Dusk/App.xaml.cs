using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Dusk.Screens;
using Squirrel;

namespace Dusk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static Task<UpdateManager> _updateManager = null;

        private MainWindow mainWindow;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            awooo.IsRunning = true;

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
            Task.Factory.StartNew(CheckForUpdates);
        }

        private static async void CheckForUpdates()
        {
            //Todo squirrel update


            //using (var mgr = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\dev\Releases"))
            //{
            //    await mgr.UpdateApp();
            //}

            _updateManager = UpdateManager.GitHubUpdateManager("https://github.com/awooo-ph/dusk", "Dusk", prerelease: true);

            //_updateManager = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\deploy");

            if (_updateManager.Result.IsInstalledApp)
            {
                //_updateManager.UpdateApp()
                // var up = await _updateManager.CheckForUpdate();
                await _updateManager.Result.UpdateApp();
            }

        }


        protected override void OnExit(ExitEventArgs e)
        {

            _updateManager?.Dispose();
            Dusk.Properties.Settings.Default.Save();

            base.OnExit(e);
        }

    }
}