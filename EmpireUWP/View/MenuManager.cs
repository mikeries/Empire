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
                return lobby.hostedGames;
            }
            private set { }
        }
        public static List<string> availablePlayers { get
            {
                return lobby.availablePlayers;
            } private set { }
        }

        public static List<string> gameMembers  { get
            {
                return lobby.GameMembers(PlayerID);
            }
            private set { }
        }

        public static string PlayerID { get; private set; }

        private static MenuManager _currentPage;
        public static MenuManager CurrentPage { get { return _currentPage; } set { _currentPage = value; } }

        private static LobbyService lobby;

        public MenuManager()
        {
            if (lobby == null) {
                lobby = new LobbyService(this);
            }
        }

        public static async Task ConnectToLobby(string playerID)
        {
            PlayerID = playerID;
            await lobby.Initialize();
            await lobby.ConnectToLobbyAsync(PlayerID);
        }

        public static Task HostGame()
        {
            return lobby.HostGame(PlayerID);
        }

        public static Task JoinGame(string hostID)
        {
            return lobby.JoinGame(PlayerID, hostID);
        }

        public static Task LeaveGame()
        {
            return lobby.LeaveGame(PlayerID);
        }

        public static async Task EnterLobby()
        {
            await lobby.EnterLobby(PlayerID);
        }

        public static Task LeaveLobby()
        {
            return lobby.LeaveLobby(PlayerID);
        }

        public static Task InitializeGame()
        {
            return lobby.InitializeGame(PlayerID);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (CoreWindow.GetForCurrentThread().Dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal static async void PlayerListChanged()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { CurrentPage.OnPropertyChanged("availablePlayers"); });
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { CurrentPage.OnPropertyChanged("hostedGames"); });
        }
    }
}
