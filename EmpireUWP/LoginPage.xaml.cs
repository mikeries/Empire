﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace EmpireUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            loginFailText.Visibility = Visibility.Collapsed;
            try
            {
                if (LoginHelper.TryLogin(Username.Text, passwordBox.Password))
                {
                    Frame.Navigate(typeof(MainMenu), Username.Text);
                }
                else
                {
                    loginFailText.Visibility = Visibility.Visible;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1042:
                        loginFailText.Text = "Cannot connect to login server.";
                        break;
                    case 0:
                        loginFailText.Text = "Invalid SQL username/password.  Please contact the game administrator.";
                        break;
                    default:
                        throw ex;
                }
                loginFailText.Visibility = Visibility.Visible;
            }
        }

    }
}
