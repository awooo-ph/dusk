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

        //#if !DEBUG
        private static UpdateManager _updateManager = null;
        //#endif

        private MainWindow mainWindow;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            awooo.IsRunning = true;

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 });


            if (Dusk.Properties.Settings.Default.ShowSplash)
            {

                mainWindow = new MainWindow();
                MainWindow = mainWindow;
                MainViewModel.Instance.Dispatcher = mainWindow.Dispatcher;
                Task.Factory.StartNew(async () =>
                {
                    await mainWindow.Dispatcher.InvokeAsync(() => mainWindow.DataContext = MainViewModel.Instance);
                });


                var splash = new Splash();
                splash.Show();


            }
            else
            {
                mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();

                MainViewModel.Instance.Dispatcher = mainWindow.Dispatcher;
                mainWindow.DataContext = MainViewModel.Instance;

            }
            //#if !DEBUG
            Task.Factory.StartNew(CheckForUpdates);
            //#endif
        }
        //#if !DEBUG
        private static async void CheckForUpdates()
        {
            //Todo squirrel update


            //using (var mgr = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\dev\Releases"))
            //{
            //    await mgr.UpdateApp();
            //}

            _updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/awooo-ph/dusk", "Dusk", prerelease: true);

            //_updateManager = new UpdateManager(@"C:\Users\7\Source\Repos\Dusk\deploy");

            if (_updateManager.IsInstalledApp)
            {
                //_updateManager.UpdateApp()
                // var up = await _updateManager.CheckForUpdate();
                await _updateManager.UpdateApp();
            }

        }
        //#endif

        protected override void OnExit(ExitEventArgs e)
        {
            //#if !DEBUG
            _updateManager?.Dispose();
            //#endif
            Dusk.Properties.Settings.Default.Save();

            base.OnExit(e);
        }

    }
}