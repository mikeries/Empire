using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyService
{
    public enum LobbyCommands : int
    {
        EnterLobby,     
        HostGame,
        JoinGame,
        LeaveGame,
        Chat,
        LeaveLobby,
        SetupGame,
        EnterGame,
        EjectThisUser,
    }
}
