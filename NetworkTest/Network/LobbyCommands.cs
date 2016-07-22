using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.Network
{
    enum LobbyCommands : int
    {
        EnterLobby,     
        HostGame,
        JoinGame,
        LeaveGame,
        Chat,
        LeaveLobby,
        SetupGame,
        EjectThisUser,
    }
}
