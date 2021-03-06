﻿using EmpireUWP.Model;
using EmpireUWP.Network;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP.View
{
    public class InputManager
    {
        private GameClient _connectionManager;

        public InputManager(GameClient connection)
        {
            _connectionManager = connection;
        }

        public void Update()
        {
            ProcessLocalInput();
        }

        public void ProcessRemoteInput(ShipCommand commandPacket)
        {
            if (commandPacket.Owner != _connectionManager.LocalPlayerID)
            { 
                OnCommandReceived(commandPacket);
            }
        }

        public void ProcessLocalInput()
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

            string source = _connectionManager.LocalPlayerID;
            if (source != null)
            {
                ShipCommand commandPacket = new ShipCommand(source, commands);
                OnCommandReceived(commandPacket);

                _connectionManager.SendShipCommandToHost(commandPacket);
            }

        }

        private void ProcessIncomingPacket(object sender, PacketReceivedEventArgs e)
        {
            if (e.Packet.Type == PacketType.ShipCommand)
            {
                ShipCommand command = e.Packet as ShipCommand;
                ProcessRemoteInput(command);
            }
        }

        public event EventHandler<CommandReceivedEventArgs> CommandReceived = delegate { };
        private void OnCommandReceived(ShipCommand command)
        {
            CommandReceived?.Invoke(null, new CommandReceivedEventArgs(command));
        }
    }
}
