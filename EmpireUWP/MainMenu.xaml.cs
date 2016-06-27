using EmpireUWP.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        string playerID;

        public MainMenu()
        {
            this.InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            playerID = (string)e.Parameter;

            //TODO:  need to figure out best way to initialize and connect to lobby asynchronously.
            menuManager.ConnectToLobbyAsync(playerID);
            base.OnNavigatedTo(e);
        }

        private void joinGame_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void hostGame_Click(object sender, RoutedEventArgs e)
        {
            menuManager.HostGame(playerID);  
        }
    }
}
