using Coinche.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds 
        private const int SPLASH_FADE_TIME = 500;     // Miliseconds 

        /// <summary>
        /// Override startup client
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreen splash = new SplashScreen("splash.jpg");
            splash.Show(false, true);
            
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
 
            base.OnStartup(e);
            MainWindow main = new MainWindow();
 
            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                System.Threading.Thread.Sleep(remainingTimeToShowSplash);
            
            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            main.Show();
        }
    }
}
