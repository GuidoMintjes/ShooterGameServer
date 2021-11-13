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

			Console.WriteLine();
			Console.WriteLine("Sending clientID: " + toClientID);
			Console.WriteLine();

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

				Funcs.printMessage(2, $"Sending udp to client {i}", false);
				GameServer.connections[i].udp.SendData(packet);

			}
		}


		// Send a packet to all connected clients except one
		public static void UDPSendPacketToAll(int excludedClient, Packet packet) {

			packet.PacketWriteLength();
			for (int i = 1; i < GameServer.MaxConnections; i++) {

				if (i != excludedClient)
					GameServer.connections[i].udp.SendData(packet);
			}
		}

        public static void SpawnPlayer(int clientID, Player player) {

			using (Packet packet = new Packet((int) ServerPackets.spawnPlayer)) {

				packet.Write(player.id);
				packet.Write(player.userName);
				packet.Write(player.position);
				packet.Write(player.rotation);

				TCPSendPacket(clientID, packet);
            }
        }
    }
}