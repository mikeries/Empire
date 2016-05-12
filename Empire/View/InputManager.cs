using Empire.Model;
using Empire.Network;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.View
{
    static class InputManager
    {
        static InputManager()
        {
            NetworkInterface.PacketReceived += ProcessIncomingPacket;
        }

        internal static void Update()
        {
            ProcessLocalInput();
        }

        internal static void ProcessRemoteInput(ShipCommand commandPacket)
        {
            if (commandPacket.Owner != ConnectionManager.ConnectionID)
            { 
                OnCommandReceived(commandPacket);
            }
        }

        internal static void ProcessLocalInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();

                int commands = 0;
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    commands += (int)CommandFlags.Left;
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    commands += (int)CommandFlags.Right;
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    commands += (int)CommandFlags.Thrust;
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    commands += (int)CommandFlags.Shields;
                }
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    commands += (int)CommandFlags.Shoot;
                }

                string source = ConnectionManager.ConnectionID;
                ShipCommand commandPacket = new ShipCommand(source, commands);
                OnCommandReceived(commandPacket);

                ConnectionManager.SendShipCommand(commandPacket);

        }

        private static void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            if (e.Packet.Type == PacketType.ShipCommand)
            {
                ShipCommand command = e.Packet as ShipCommand;
                ProcessRemoteInput(command);
            }
        }

        public static event EventHandler<CommandReceivedEventArgs> CommandReceived = delegate { };
        private static void OnCommandReceived(ShipCommand command)
        {
            CommandReceived?.Invoke(null, new CommandReceivedEventArgs(command));
        }
    }
}
