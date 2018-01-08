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

        private static Task _updateManager = null;

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
            _updateManager = Task.Factory.StartNew(CheckForUpdates);
        }

        private static async void CheckForUpdates()
        {
            //Todo squirrel update


            //using (var mgr = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\dev\Releases"))
            //{
            //    await mgr.UpdateApp();
            //}

            var upd = UpdateManager.GitHubUpdateManager("https://github.com/awooo-ph/dusk", "Dusk", prerelease: true);

            //_updateManager = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\deploy");

            if (upd.Result.IsInstalledApp)
            {
                //_updateManager.UpdateApp()
                // var up = await _updateManager.CheckForUpdate();
                await upd.Result.UpdateApp();
            }

        }


        protected override void OnExit(ExitEventArgs e)
        {
            Dusk.Properties.Settings.Default.Save();

            if (!_updateManager?.IsCompleted ?? true)
            {
                _updateManager?.Wait();
            }

            base.OnExit(e);
        }

    }
}