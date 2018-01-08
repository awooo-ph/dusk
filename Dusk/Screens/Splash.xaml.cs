using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Dusk.Screens
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash
    {
        public Splash()
        {
            InitializeComponent();
            // Application.Current.MainWindow.IsEnabled = false;
            // ((MetroWindow)Application.Current.MainWindow).ShowOverlay();
            // Storyboard.Completed += Storyboard_Completed;
        }


        private void Storyboard_Completed(object sender, EventArgs e)
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400));
            anim.Completed += AnimOnCompleted;
            BeginAnimation(OpacityProperty, anim);
            //   var a = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
            //  App.Current.MainWindow.Opacity = 0;

            //   App.Current.MainWindow.BeginAnimation(OpacityProperty, a);
            // Close();
        }

        private void AnimOnCompleted(object sender, EventArgs eventArgs)
        {
            ((AnimationClock)sender).Completed -= AnimOnCompleted;
        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
        }

        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

        private void Splash_OnLoaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(1.0 / 30.0);
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            var top = Canvas.GetTop(Image);
            top -= 460;
            Canvas.SetTop(Image, top);


            if (top == -(460.0 * 47.0))
            {
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        Application.Current.MainWindow.DataContext = MainViewModel.Instance;
                        Application.Current.MainWindow.Show();
                    }));
                });
            }

            if (top <= -24840.0)
            {
                timer.Tick -= TimerOnTick;
                Close();
            }
        }
    }
}