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

namespace EmpireUWP.View
{
    public class MenuManager : INotifyPropertyChanged
    {

        public static List<string> hostedGames
        {
            get
            {
                return lobby.hostedGames;
            }
            private set { }
        }
        public static List<string> availablePlayers { get
            {
                return lobby.availablePlayers;
            } private set { } }

        private static LobbyService lobby;

        public MenuManager()
        {
            lobby = new LobbyService(this);
            lobby.InitializeAsync();
        }

        public void ConnectToLobbyAsync(string PlayerID)
        {
            lobby.ConnectToLobbyAsync(PlayerID);
        }

        public void HostGame(string playerID)
        {
            lobby.HostGame(playerID);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (CoreWindow.GetForCurrentThread().Dispatcher.HasThreadAccess) { }
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        internal async void PlayerListChanged()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { OnPropertyChanged("availablePlayers"); });
        }
    }
}
