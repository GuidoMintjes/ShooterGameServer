using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {
    class ServerHandle {
        

        public static void ReturnedWelcomeReceived(int clientID, Packet packet) {

            int receivedClientID = packet.ReadInt(true);
            string receivedUserName = packet.ReadString(true);

            Funcs.printMessage(3, $"{GameServer.connections[clientID].tcp.socket.Client.RemoteEndPoint} connected to this server!"
                + $" (ID {clientID} with name {receivedUserName})", true);


            GameServer.connections[clientID].userName = receivedUserName;   // Saves the username for this client



            if(clientID != receivedClientID) {

                Console.WriteLine();
                Funcs.printMessage(0, $"Client {receivedUserName} with ID {clientID} has the wrong ID: {receivedClientID}!", false);
                Console.WriteLine();

                GameServer.connections[clientID].Disconnect();
            }

            GameServer.connections[clientID].SendIntoGame(receivedUserName);
        }
    }
}