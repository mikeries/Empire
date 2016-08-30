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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LobbyService
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        LobbyService service = new LobbyService();

        public MainPage()
        {
            this.InitializeComponent();
            client.LobbyCommand += ProcessLobbyCommands;
        }

        private void ProcessLobbyCommands(object sender, LobbyCommandEventArgs e)
        {
            LobbyCommandPacket packet = e.Packet;

            switch (packet.Command)
            {
                case LobbyCommands.EjectThisUser:
                    ReportOutput("Ejecting " + packet.PlayerID + ".");
                    break;
            }
        }

        private void ReportOutput(string output)
        {
            textBox.Content = output + Environment.NewLine + textBox.Content;
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            await service.Initialize();
            ReportOutput("Server is listening.");
        }

        private async void initializeButton_Click(object sender, RoutedEventArgs e)
        {
            await client.Initialize();
            ReportOutput("Client initialized.");
        }

        private void playerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void connectJoe_Click(object sender, RoutedEventArgs e)
        {
            await client.EnterLobby("Joe");
            ReportOutput("Joe connected.");
        }

        private async void connectMike_Click(object sender, RoutedEventArgs e)
        {
            await client.EnterLobby("Mike");
            ReportOutput("Mike connected.");
        }

        private async void mikeHost_Click(object sender, RoutedEventArgs e)
        {
            await client.HostGame("Mike");
            ReportOutput("Mike hosting a game.");
        }

        private async void joeJoin_Click(object sender, RoutedEventArgs e)
        {
            await client.JoinGame("Joe", "Mike");
            ReportOutput("Joe joins Mike's game.");
        }
    }
}
