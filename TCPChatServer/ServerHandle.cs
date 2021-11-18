using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer {
    class ServerHandle {
        

        public static void ReturnedWelcomeReceived(int clientID, Packet packet) {

            int receivedClientID = packet.ReadInt(true);
            string receivedUserName = packet.ReadString(true);

            //Funcs.PrintMessage(1, receivedUserName);

            Funcs.PrintMessage(3, $"{GameServer.connections[clientID].tcp.socket.Client.RemoteEndPoint} connected to this server!"
                + $" (ID {clientID} with name {receivedUserName})", true);


            GameServer.connections[clientID].userName = receivedUserName;   // Saves the username for this client



            if(clientID != receivedClientID) {

                Funcs.PrintMessage(2, "");
                Funcs.PrintMessage(0, $"Client {receivedUserName} with ID {clientID} has the wrong ID: {receivedClientID}!", false);
                Funcs.PrintMessage(2, "");

                GameServer.connections[clientID].Disconnect();
            }

            GameServer.connections[clientID].SendIntoGame(receivedUserName);
        }


        public static void HandlePlayerMoved(int clientID, Packet packet) {

            //Funcs.PrintMessage(4, "Received player position packet!");

            using(Packet _packet = new Packet((int) ServerPackets.PlayerPosition)) {

                packet.ReadInt(true);

                Vector3 sendPosition = packet.ReadVector3(true);

                /*
                Funcs.PrintMessage(4, "");
                Funcs.PrintMessage(4, sendPosition.ToString());
                Funcs.PrintMessage(4, "");
                */

                _packet.Write(clientID);
                _packet.Write(sendPosition, true);

                /*
                Funcs.PrintMessage(4, "");
                Funcs.PrintData(_packet.GetPacketBytes(), true);
                Funcs.PrintMessage(4, "");
                */

                //ServerSend.UDPSendPacketToAll(clientID, _packet);
                ServerSend.UDPSendPacketToAll(_packet);
            }
        }


        public static void HandlePlayerRotated(int clientID, Packet packet) {

            using (Packet _packet = new Packet((int) ServerPackets.PlayerRotation)) {

                packet.ReadInt(true);

                Quaternion sendPosition = packet.ReadQuaternion(true);

                _packet.Write(clientID);
                _packet.Write(sendPosition);

                //Funcs.PrintMessage(3, "");
                //Funcs.PrintData(_packet.GetPacketBytes(), true);
                //Funcs.PrintMessage(3, "");

                ServerSend.UDPSendPacketToAll(clientID, _packet);
            }
        }


        public static void HandlePlayerScaled(int clientID, Packet packet) {

            using (Packet _packet = new Packet((int) ServerPackets.PlayerScaled)) {

                int packetSize = packet.ReadInt(true);

                Vector3 playerScale = packet.ReadVector3(true);

                _packet.Write(clientID);
                _packet.Write(playerScale);

                //Funcs.PrintMessage(4, "Sending player scaled:", false);
                
                ServerSend.TCPSendPacketToAll(_packet);
            }
        }
    }
}