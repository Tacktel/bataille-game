using Coinche.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace Server
{
    /// <summary>
    /// Logique d'interaction pour Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        MainWindow mainWindow = new MainWindow();
        public Splash()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Allow drag/move window
        /// </summary>
        private void move(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        /// <summary>
        /// click handler to close window
        /// </summary>
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// click handler to minimize window
        /// </summary>
        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void usernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        /// <summary>
        /// Handle sysadmin login
        /// </summary>
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] hashBytes;
            MD5 md5 = MD5.Create();

            md5.Initialize();
            md5.ComputeHash(Encoding.UTF8.GetBytes(PW.Password));
            hashBytes = md5.Hash;
            string hash = Convert.ToBase64String(hashBytes);
            // admin1234 => 16 bytes md5 => base64
            if (UN.Text == "admin" && hash == "yTzNeLIHZSg0YhazsvcB5g==")
            {
                this.Close();
                mainWindow.Show();
            }
            else
            {
                MessageBox.Show("Invalid creditentials.");
            }
        }
    }
}
