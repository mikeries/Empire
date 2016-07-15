using EmpireUWP.Network;
using EmpireUWP.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
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
    public sealed partial class MainMenu : Page
    {

        public MainMenu()
        {
            this.InitializeComponent();         
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MenuManager.CurrentPage = menuManager;
            await MenuManager.EnterLobby();
        }

        private async void joinGame_Click(object sender, RoutedEventArgs e)
        {
            if (hostedGamesList.SelectedItems.Count == 1)
            {
                await MenuManager.JoinGame(hostedGamesList.SelectedValue.ToString());
            }
         }

        private async void hostGame_Click(object sender, RoutedEventArgs e)
        {
            await MenuManager.HostGame();
            Frame.Navigate(typeof(WaitingRoom));
        }

        private void hostedGamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hostedGamesList.SelectedItems.Count > 0)
            {
                joinGame.IsEnabled = true;
            }
        }

        private async void logoutButton_Click(object sender, RoutedEventArgs e)
        {
            await MenuManager.LeaveLobby();
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
