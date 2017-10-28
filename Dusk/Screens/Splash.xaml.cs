using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            Storyboard.Completed += Storyboard_Completed            ;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() => App.Current.MainWindow.Show());
            });
            
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(400));
            anim.Completed += AnimOnCompleted;
            BeginAnimation(OpacityProperty, anim);
         //   var a = new DoubleAnimation(1, TimeSpan.FromMilliseconds(400));
          //  App.Current.MainWindow.Opacity = 0;
           
         //   App.Current.MainWindow.BeginAnimation(OpacityProperty, a);
           // Close();  
        }

        private void Splash_OnWindowTransitionCompleted(object sender, RoutedEventArgs e)
        {

        }

        private void AnimOnCompleted(object sender, EventArgs eventArgs)
        {
            ((AnimationClock) sender).Completed -= AnimOnCompleted;
            Close();
        }

        private void MediaElement_OnMediaEnded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
