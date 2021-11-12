using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {
    class ServerHandle {
        

        public static void ReturnedWelcomeReceived(int clientID, Packet packet) {

            int receivedClientID = packet.PacketReadInt(true);
            string receivedUserName = packet.PacketReadString(true);

            Funcs.printMessage(3, $"{ChatServer.connections[clientID].tcp.socket.Client.RemoteEndPoint} connected to this server!"
                + $" (ID {clientID} with name {receivedUserName})", true);


            ChatServer.connections[clientID].userName = receivedUserName;   // Saves the username for this client



            if(clientID != receivedClientID) {

                Console.WriteLine();
                Funcs.printMessage(0, $"Client {receivedUserName} with ID {clientID} has the wrong ID: {receivedClientID}!", false);
                Console.WriteLine();

                //ChatServer.connections[clientID].Disconnect();
            }
        }


        public static void UDPTestConfirmed(int clientID, Packet packet) {

            Funcs.printMessage(2, $"Received udp confirmation: {packet.PacketReadString(true)}", false);
        }
    }
}