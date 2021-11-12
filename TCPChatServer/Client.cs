using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Numerics;

namespace GameServer {

    public class Client {

        public readonly static int dataBufferSize = 4096;    // Set the (default) buffer size to 4 megabytes

        public int clientID;

        public Player player;
        public TCP tcp;
        public UDP udp;


        public string userName;         // Username of the client connected to the server that uses this instance


        public Client(int _clientID) {

            clientID = _clientID;

            tcp = new TCP(clientID);
            udp = new UDP(clientID);
        }


        public class TCP {

            public TcpClient socket;    // Information stored that gets saved in server in the callback method
            private int id;

            private NetworkStream stream;
            private byte[] receiveByteArray;

            private Packet receivedPacket;



            public TCP(int _id) {

                id = _id;
            }


            public void Connect(TcpClient _socket) {

                //Console.WriteLine("Trying to connect to server...");

                socket = _socket;

                socket.ReceiveBufferSize = dataBufferSize;  // Set the send and receive buffer sizes to the declared
                socket.SendBufferSize = dataBufferSize;     // buffer sizes at the start of the client class

                stream = socket.GetStream();                    // Gets the 'stream' of info provided by the socket
                receiveByteArray = new byte[dataBufferSize];

                receivedPacket = new Packet();

                stream.BeginRead(receiveByteArray, 0, dataBufferSize, StreamReceiveCallback, null);


                ServerSend.WelcomeClient(id, $"Welcome to the server!");  // Send welcome message
            }


            // Send data to client through TCP
            public void SendData(Packet packet) {

                try {

                    if (socket != null) {

                        stream.BeginWrite(packet.GetPacketBytes(), 0, packet.GetPacketSize(), null, null);
                    }

                } catch (Exception exc) {

                    Funcs.printMessage(0, $"Unable to send data to client {id} through TCP, err msg: {exc}", false);
                }
            }


            // Method that gets called back on when client connects to server
            void StreamReceiveCallback(IAsyncResult aResult) {

                // Handle this in a try catch block to be able to handle crashes
                try {

                    int dataLength = stream.EndRead(aResult);   // Returns an integer indicating the amount of bytes read
                                                                // in the data 'stream'

                    if (dataLength <= 0) {

                        //ChatServer.connections[id].Disconnect();    // Properly disconnects from the server

                        return;             // Return out of the method when no bytes have been read ==>
                                            // (amount of bytes read = 0)
                    }


                    byte[] dataReceived = new byte[dataLength];             // Move the received data to a local variable...
                    Array.Copy(receiveByteArray, dataReceived, dataLength); // ...

                    receivedPacket.NullifyPacket(HandleData(dataReceived));

                    // Start reading data from the stream again (if this would not be done the client would stop functioning
                    // here pretty much, just like if the server would stop listening for new connections)
                    stream.BeginRead(receiveByteArray, 0, dataBufferSize, StreamReceiveCallback, null);


                } catch (Exception exc) {

                    Console.WriteLine("Disconnected due to error: " + exc.Message);
                    //ChatServer.connections[id].Disconnect();    // Properly disconnects from the server
                }
            }


            // Handles the data and returns a boolean, this is needed because we might not want to always reset the pack
            private bool HandleData(byte[] dataArray) {

                int packetLength = 0;

                receivedPacket.SetPacketBytes(dataArray); // Load the data into the Packet instance


                // Check if what still needs to be read is an integer or bigger, if so that is the first int of the packet indicating
                // the length of that packet
                if (receivedPacket.GetUnreadPacketSize() >= 4) {

                    packetLength = receivedPacket.PacketReadInt(true);

                    // Check if packet size is 0 or less, if so, return true so that the packet will be reset
                    if (packetLength <= 0) {

                        return true;
                    }
                }


                // While this is true there is still data that needs to be handled
                while (packetLength > 0 && packetLength <= receivedPacket.GetUnreadPacketSize()) {

                    byte[] packetBytes = receivedPacket.PacketReadBytes(packetLength, true);

                    ThreadManager.ExecuteOnMainThread(() => {

                        using (Packet packet = new Packet(packetBytes)) {

                            int packetID = packet.PacketReadInt(true);

                            
                            ChatServer.packetHandlers[packetID](id, packet);


                            //Funcs.printMessage(2, "Added to packet handlers", true);
                        }
                    });

                    packetLength = 0;

                    if (receivedPacket.GetUnreadPacketSize() >= 4) {

                        packetLength = receivedPacket.PacketReadInt(true);

                        // Check if packet size is 0 or less, if so, return true so that the packet will be reset
                        if (packetLength <= 0) {

                            return true;
                        }
                    }
                }


                if (packetLength <= 1) {
                    return true;
                }


                return false;       // In this case there is still a piece of data in the packet/stream which is part of some data
                                    // in some other upcoming packet, which is why it shouldn't be destroyed
            }


            public void Disconnect() {

                socket.Close();
                stream = null;
                receiveByteArray = null;
                receivedPacket = null;
                socket = null;
            }
        }


        public class UDP {

            public IPEndPoint endPoint;

            public int clientID;


            public UDP(int _clientID) {

                clientID = _clientID;
            }


            public void Connect(IPEndPoint _endPoint) {

                endPoint = _endPoint;

                Funcs.printMessage(3, "UDP client connected!", false);
                //ServerSend.UDPTest(clientID);
            }

            public void SendData(Packet packet) {

                ChatServer.SendUDPData(endPoint, packet);
            }


            public void HandleData(Packet packet) {

                int packetLength = packet.PacketReadInt(true);
                byte[] packetData = packet.GetPacketBytes();

                ThreadManager.ExecuteOnMainThread(() => {

                    using (Packet packet = new Packet(packetData)) {

                        int packetID = packet.PacketReadInt(true);
                        ChatServer.packetHandlers[packetID](clientID, packet);
                    }
                });
            }
        }


        public void SendIntoGame(string _clientName) {

            player = new Player(clientID, userName, new Vector3(0f, 0f, 0f));

            foreach (Client _client in ChatServer.connections.Values) {

                if(_client.player != null) {

                    if(_client.clientID != clientID) {

                        ServerSend.SpawnPlayer(clientID, _client.player);
                    }
                }
            }

            foreach (Client _client in ChatServer.connections.Values) {

                if(_client.player != null) {

                    ServerSend.SpawnPlayer(_client.clientID, player);
                }
            }
        }


        public void Disconnect() {

            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected from the server!");

            userName = null;
            tcp.Disconnect();
        }
    }
}