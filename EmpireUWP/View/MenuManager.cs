using EmpireUWP.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace EmpireUWP.View
{
    public class MenuManager : INotifyPropertyChanged
    {

        public static List<string> hostedGames  {  get
            {
                return _lobby.hostedGames;
            }
            private set { }
        }
        public static List<string> availablePlayers { get
            {
                return _lobby.availablePlayers;
            } private set { }
        }

        public static List<string> gameMembers  { get
            {
                return _lobby.GameMembers(PlayerID);
            }
            private set { }
        }

        public static string PlayerID { get; private set; }

        private static Page _currentPage;
        public static Page CurrentPage { get { return _currentPage; } set { _currentPage = value; } }
        public static MenuManager Manager { get; set; }

        private static Lobby _lobby;

        public MenuManager()
        {
            if (_lobby == null) {
                _lobby = new Lobby();
                _lobby.LobbyCommand += ProcessLobbyCommand;
                _lobby.LobbyUpdated += LobbyUpdate;
            }
        }

        public static Task InitializeLobby(string playerID)
        {
            PlayerID = playerID;

            return _lobby.Initialize();
        }

        public static Task HostGame()
        {
            return _lobby.HostGame(PlayerID);
        }

        public static Task JoinGame(string hostID)
        {
            return _lobby.JoinGame(PlayerID, hostID);
        }

        public static Task LeaveGame()
        {
            return _lobby.LeaveGame(PlayerID);
        }

        public static Task EnterLobby()
        {
            return _lobby.EnterLobby(PlayerID);
        }

        public static Task LeaveLobby()
        {
            return _lobby.LeaveLobby(PlayerID);
        }

        public static Task InitializeGame()
        {
            return _lobby.InitializeGame(PlayerID);
        }

        public static Task StartGame()
        {
            return _lobby.StartGame(PlayerID);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (CoreWindow.GetForCurrentThread().Dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private void ProcessLobbyCommand(object sender, LobbyCommandEventArgs e)
        {
            LobbyCommandPacket packet = e.Packet;
            if (packet.Command == LobbyCommands.EnterGame)
            {
                CurrentPage.Frame.Navigate(typeof(MainViewScreen));
            }
            else if (packet.Command == LobbyCommands.EjectThisUser)
            {
                Windows.UI.Xaml.Application.Current.Exit();
            }
        }

        private void LobbyUpdate(object sender, PropertyChangedEventArgs args)
        {
            PlayerListChanged();
        }

        internal static async void PlayerListChanged()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { Manager.OnPropertyChanged("availablePlayers"); });
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { Manager.OnPropertyChanged("hostedGames"); });
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { Manager.OnPropertyChanged("gameMembers"); });
        }
    }
}
