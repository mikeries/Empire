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

namespace LobbyTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //LobbyService service = new LobbyService();

        public MainPage()
        {
            this.InitializeComponent();
            client.LobbyCommand += ProcessLobbyCommands;
        }

        private async void ProcessLobbyCommands(object sender, LobbyCommandEventArgs e)
        {
            LobbyCommandPacket packet = e.Packet;

            switch (packet.Command)
            {
                case LobbyCommands.EjectThisUser:
                    ReportOutput("Ejecting " + packet.PlayerID + ".");
                    Application.Current.Exit();
                    break;
                case LobbyCommands.LeaveGame:
                    ReportOutput(packet.PlayerID + " leaves his game.");
                    await client.LeaveGame(packet.PlayerID);
                    break;
                case LobbyCommands.Disconnected:
                    ReportOutput("Unable to reach Lobby service.");
                    break;

            }
        }

        private void ReportOutput(string output)
        {
            textBox.Content += Environment.NewLine + output;
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

        private async void Host_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex >= 0)
            {
                string player = client.playerList[playerList.SelectedIndex].PlayerID;
                await client.HostGame(player);
                ReportOutput(player + " is hosting a game.");
            } else
            {
                ReportOutput("Select which player should host.");
            }
        }

        private async void Join_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex >= 0 && gameList.SelectedIndex >= 0)
            {
                string player = client.playerList[playerList.SelectedIndex].PlayerID;
                string game = client.gamesList[gameList.SelectedIndex].HostID;
                await client.JoinGame(player,game);
                ReportOutput(player + " joins "+game+"'s game.");
            }
            else
            {
                ReportOutput("Select a player and a game to join.");
            }
        }

        private async void LeaveLobby_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex >= 0)
            {
                string player = client.playerList[playerList.SelectedIndex].PlayerID;
                await client.LeaveLobby(player);
                ReportOutput(player + " leaves the lobby.");
            }
            else
            {
                ReportOutput("Select a player to leave the lobby.");
            }
        }

        private async void LeaveGame_Click(object sender, RoutedEventArgs e)
        {
            if (playerList.SelectedIndex >= 0)
            {
                string player = client.playerList[playerList.SelectedIndex].PlayerID;
                await client.LeaveGame(player);
                ReportOutput(player + " leaves his game.");
            }
            else
            {
                ReportOutput("Select a player to leave his game.");
            }
        }
    }
}
