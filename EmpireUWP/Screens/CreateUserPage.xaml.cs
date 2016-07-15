using EmpireUWP.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class CreateUserPage : Page
    {

        public CreateUserPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MenuManager.CurrentPage = menuManager;
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            loginFailText.Visibility = Visibility.Collapsed;
            try
            {
                string user = LoginHelper.Capitalize(Username.Text);
                LoginHelper.CreateUser(user, passwordBox.Password, passwordBox2.Password);
                Frame.Navigate(typeof(MainMenu));
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                switch (ex.HResult)
                {
                    case 1062:
                        loginFailText.Text = "That username already exists.  Please choose another.";
                        break;
                    case 1042:
                        loginFailText.Text = "Cannot connect to login server.";
                        break;
                    default:
                        throw ex;
                }
                loginFailText.Visibility = Visibility.Visible;
            }
            catch (LoginException ex)
            {
                if (ex.HResult == (int)LoginException.LoginExceptions.PasswordsDoNotMatch)
                {
                    loginFailText.Text = "Passwords to not match.  Try again.";
                } else
                {
                    loginFailText.Text = ex.Message;
                }
                loginFailText.Visibility = Visibility.Visible;
            }

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
