using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace GameServer {

    class GameServer {


        public static int MaxConnections;
        public static int Port { get; private set; }




        private static TcpListener tcpListener;     // A listener is an object which 'listens' for incoming connections,
                                                    // this is needed because in the tcp protocol you establish a connection
        private static UdpClient udpListener;


        // Keep a dictionary of all connections
        public static Dictionary<int, Client> connections = new Dictionary<int, Client>();


        // Same dictionary and void as the client
        public delegate void PacketHandler(int clientID, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;


        // Initialize the server
        public static void StartServer(int maxConnections, int port) {

            Funcs.PrintMessage(2, "Starting server...", true);
            InitializeServerData(maxConnections);

            MaxConnections = maxConnections;
            Port = port;

            // These 3 lines start the tcp listener on the 'any' IPAddress with our specific port, and makes sure the
            // callback function is called when a connection is established
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);


            // Initialize the UDP part of the networking shenanigans
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);


            Funcs.PrintMessage(2, "Server initialized on port: " + Port, true);

            ServerCommand.CommandLoop();
        }


        // Handle connection once it has been established
        private static void TCPConnectCallback(IAsyncResult aResult) {

            Funcs.PrintMessage(3, "Connection incoming!");

            // Store the tcp client instance in a local variable here
            TcpClient client = tcpListener.EndAcceptTcpClient(aResult);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);   // We have to call this function
                                                                                                // again, otherwise the tcplistener would stop
                                                                                                // listening and no other connections could be made

            Funcs.PrintMessage(2, "Someone is trying to connect from: " + client.Client.RemoteEndPoint, true);

            
            for (int i = 1; i <= MaxConnections; i++) {

                // Check if socket of this id in the dictionary is null, that would mean it is vacant
                if(connections[i].tcp.socket == null) {

                    connections[i].tcp.Connect(client);
                    return;     // Return out of method otherwise the client would take up all available spots at once
                }
            }


            Funcs.PrintMessage(2, client.Client.RemoteEndPoint + " has failed to connect to server because the server is full. " +
                                " (SERVER FULL ERROR)", true);
        }


        private static void UDPReceiveCallback(IAsyncResult Aresult) {

            try {

                //Funcs.printMessage(2, "Received some data through udp!", false);

                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataReceived = udpListener.EndReceive(Aresult, ref clientEndPoint);

                /*
                Funcs.PrintMessage(4, "");
                Funcs.PrintData(dataReceived);
                Funcs.PrintMessage(4, "");
                */

                udpListener.BeginReceive(UDPReceiveCallback, null);


                if(dataReceived.Length < 4) {

                    Funcs.PrintMessage(0, "Data from client is not big enough!!");
                    return;
                }


                using (Packet packet = new Packet(dataReceived)) {

                    int clientID = packet.ReadInt(true);

                    //Funcs.PrintData(dataReceived, false);

                    // If clientID received == 0 something went wrong so return!
                    if (clientID == 0) {
                        Funcs.PrintMessage(0, "Dit mag niet gebeuren! (GameServer.cs)");
                        return;
                    }

                    // If the endpoint is messed up also return!
                    if(connections[clientID].udp.endPoint == null) {

                        connections[clientID].udp.Connect(clientEndPoint);
                        return;
                    }


                    // ID CHECK! Otherwise nasty hackers may hack your player away from yer treasure! ARGHHH
                    if(connections[clientID].udp.endPoint.ToString() == clientEndPoint.ToString()) {

                        connections[clientID].udp.HandleData(packet);
                    }
                }

            } catch (Exception ex) {

                Funcs.PrintMessage(0, $"Error receiving through UDP: {ex}");
            }
        }


        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet) {

            try {

                if(clientEndPoint != null) {

                    //Funcs.PrintData(packet.GetPacketBytes());

                    //Funcs.PrintMessage("Sending through udp...");

                    udpListener.BeginSend(packet.GetPacketBytes(), packet.GetPacketSize(), clientEndPoint, null, null);
                }
            } catch (Exception ex) {

                Funcs.PrintMessage(0, $"Error sending through UDP: {ex}");
            }
        }


        // Initialise our connections dictionary
        private static void InitializeServerData(int maxConnections) {

            for (int i = 1; i <= maxConnections; i++) {

                connections.Add(i, new Client(i));
            }


            // Initialize the dictionary of packet handlers
            packetHandlers = new Dictionary<int, PacketHandler>() {

                { (int) ClientPackets.welcomeReceived, ServerHandle.ReturnedWelcomeReceived },
                { (int) ClientPackets.playerMovement, ServerHandle.HandlePlayerMoved },
                { (int) ClientPackets.playerRotation, ServerHandle.HandlePlayerRotated },
                { (int) ClientPackets.playerScaled, ServerHandle.HandlePlayerScaled }
            };


            Funcs.PrintMessage(2, "Packet handler dictionary initiated!", true);
        }
    }
}