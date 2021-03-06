using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {
	class ServerSend {


		// Send a nice welcoming message to a client that just connected
		public static void WelcomeClient(int toClientID, string welcomeMessage) {

			Packet packet = new Packet((int) ServerPackets.welcome);     // Create a welcoming packet with the welcome enum

			packet.Write(welcomeMessage);                         // Add the welcome message to the packet
			packet.Write(toClientID);                               // Add the client ID to the packet

			Funcs.PrintMessage(2, "");
			Funcs.PrintMessage(2, "Sending clientID: " + toClientID);
			Funcs.PrintMessage(2, "");

			TCPSendPacket(toClientID, packet);
		}


		// Send an actual instance of a packet through TCP to a client with specified ID
		private static void TCPSendPacket(int clientID, Packet packet) {

			packet.PacketWriteLength();

			GameServer.connections[clientID].tcp.SendData(packet);
		}


		private static void UDPSendPacket(int clientID, Packet packet) {

			packet.PacketWriteLength();

			GameServer.connections[clientID].udp.SendData(packet);
		}


		// Send a packet to all connected clients
		public static void TCPSendPacketToAll(Packet packet) {

			packet.PacketWriteLength();
			for (int i = 1; i < GameServer.MaxConnections; i++) {

				GameServer.connections[i].tcp.SendData(packet);

			}
		}


		// Send a packet to all connected clients except one
		public static void TCPSendPacketToAll(int excludedClient, Packet packet) {

			packet.PacketWriteLength();
			for (int i = 1; i < GameServer.MaxConnections; i++) {

				if(i != excludedClient)
					GameServer.connections[i].tcp.SendData(packet);
			}
		}


		// Send a packet to all connected clients
		public static void UDPSendPacketToAll(Packet packet) {

			packet.PacketWriteLength();
			for (int i = 1; i < GameServer.MaxConnections; i++) {

				//Funcs.PrintMessage(2, $"Sending udp to client {i}", false);
				GameServer.connections[i].udp.SendData(packet);
			}
		}


		// Send a packet to all connected clients except one
		public static void UDPSendPacketToAll(int excludedClient, Packet packet) {

			packet.PacketWriteLength();

			//Funcs.PrintMessage(4, "I am trying to send the vector3 position: ");
			//Funcs.PrintData(packet.GetPacketBytes());

			for (int i = 1; i < GameServer.MaxConnections; i++) {

				if (i != excludedClient)
					GameServer.connections[i].udp.SendData(packet);
			}
		}

        public static void SpawnPlayer(int clientID, Player player) {

			//Funcs.PrintMessage(2, "Spawning in a player...");
			
			using (Packet packet = new Packet((int) ServerPackets.spawnPlayer)) {

				packet.Write(player.id);
				if (!string.IsNullOrEmpty(player.userName))
					packet.Write(player.userName);
				else
					packet.Write("USERNAME COULD NOT BE RECEIVED THROUGH THE NETWORK");
				packet.Write(player.position, false);
				packet.Write(player.rotation);

				//Funcs.PrintData(packet.GetPacketBytes(), true);

				TCPSendPacket(clientID, packet);
            }
        }
    }
}