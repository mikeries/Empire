using EmpireUWP.View;
using Microsoft.Xna.Framework.Input;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MenuManager.CurrentPage = this;
            MenuManager.Manager = menuManager;
        }

        private async void submitButton_Click(object sender, RoutedEventArgs e)
        {
            loginFailText.Visibility = Visibility.Collapsed;
            try
            {
                string user = LoginHelper.Capitalize(Username.Text);
                LoginHelper.TryLogin(user, passwordBox.Password);
                loginFailText.Text = "Connecting to the game lobby";
                loginFailText.Visibility = Visibility.Visible;
                await LoadMainMenu(user);
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
            catch (LoginException ex)
            {
                switch (ex.HResult)
                {
                    case (int)LoginException.LoginExceptions.IncorrectPassword:
                        loginFailText.Text = "Incorrect password.  Please try again.";
                        passwordBox.Password = "";
                        passwordBox.Focus(FocusState.Programmatic);
                        break;
                    case (int)LoginException.LoginExceptions.UnknownUser:
                        loginFailText.Text = "Unknown user. Please check spelling or create a new account.";
                        passwordBox.Password = "";
                        Username.Focus(FocusState.Programmatic);
                        break;
                }
                loginFailText.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task LoadMainMenu(string playerID)
        {
            await MenuManager.InitializeLobby(playerID);
            await MenuManager.EnterLobby();

            Frame.Navigate(typeof(MainMenu));
        }

        private void createUserButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateUserPage));
        }

        private void passwordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((Keys)e.Key != Keys.Enter)
                return;
            e.Handled = true;
            submitButton_Click(sender, e);
        }

        private void Username_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((Keys)e.Key != Keys.Enter)
                return;
            e.Handled = true;
            passwordBox.Focus(FocusState.Programmatic);
        }
    }
}
