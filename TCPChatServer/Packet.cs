using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer {

    // Packet sent from server to client, in this case only a welcome message
    public enum ServerPackets {
        welcome = 1,
        spawnPlayer = 2,
        PlayerPosition = 3,
        PlayerRotation = 4
    }


    // Packet sent from client to server, in this case confirming the welcome message
    public enum ClientPackets {
        welcomeReceived = 1,
        playerMovement = 2
    }


    // A packet is a piece of information that gets sent over a network
    public class Packet : IDisposable {


        public byte nullByte = 00000000;

        private List<byte> buffer;
        private byte[] byteArray;
        private int readPointer;
        

        public Packet() {

            buffer = new List<byte>();    // New and empty packet gets created
            readPointer = 0;            // Set the pointer to 0 in order to start reading at the start of the packet
        }


        // Make a Packet with an ID, such a packet can then be used in transmitting
        public Packet(int id) {

            buffer = new List<byte>();    // New and empty packet gets created
            readPointer = 0;            // Set the pointer to 0 in order to start reading at the start of the packet

            PacketWrite(id);
        }


        // Used for receiving, create a packet with (received) data, which can then be read and used
        public Packet(byte[] receivedData) {

            buffer = new List<byte>();    // New and empty packet gets created
            readPointer = 0;            // Set the pointer to 0 in order to start reading at the start of the packet

            SetPacketBytes(receivedData);
        }


        #region Packet Functions

        #region Standard Functions


        // Fill packet with received data, after which it can be read
        public void SetPacketBytes(byte[] dataSet) {

            PacketWrite(dataSet);
            byteArray = buffer.ToArray();   // Set the received bytes to this byteArray, which can then be read
        }


        // Get the total length of the datastream
        public int GetPacketSize() {
            return buffer.Count;
        }


        // Get the length of the datastream that has not yet been read
        public int GetUnreadPacketSize() {
            return buffer.Count - readPointer;
        }


        // Convert the stream into readable bytes
        public byte[] GetPacketBytes() {

            byteArray = buffer.ToArray();
            return byteArray;
        }


        // Empty this instance of a packet so it can be used again
        public void NullifyPacket(bool reset) {

            if (reset) {
                buffer.Clear();     // Clear the datastream
                readPointer = 0;    // Reset the data pointer
                byteArray = null;   // Clear the readable bytearray

            } else {
                readPointer -= 4;   // Unread last read integer when not nullifying/resetting
            }
        }


        // Write the length of the packet into the packet, this is need for properly receiving it
        public void PacketWriteLength() {

            //Console.WriteLine("Inserted packet length to first position.");

            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }


        #endregion

        #region Packet Data Writing Functions


        // Add a bytearray to the datastream
        void PacketWrite(byte[] _dataSet) {
            buffer.AddRange(_dataSet);
        }


        /// <summary>
        /// Add an integer to the packet/bytestream, mainly used for adding the packet id that is used in sending
        /// </summary>
        /// <param name="_intValue"> The actual integer value that is added to the packet (4 bytes) </param>
        public void PacketWrite(int _intValue) {

            ChatApp.countIntSend++;

            //Console.WriteLine(_intValue.ToString() + " sent as no. " + ChatApp.countIntSend + 
                //" from: " + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod());

            buffer.AddRange(BitConverter.GetBytes(_intValue));
        }


        // Add a string to the packet/datastream
        public void PacketWrite(string _stringValue) {

            int a = Encoding.Unicode.GetByteCount(_stringValue);   // A string isn't always the same size, which is why the length of the string
                                                                   // has to be added to the datastream, so the other end knows how long to read
                                                                   // keep reading for just the string, an integer is always 4 bytes
            PacketWrite(a);
            
            buffer.AddRange(Encoding.Unicode.GetBytes(_stringValue)); // Add to the packet/datastream the string itself
        }

        #endregion

        #region Packet Data Reading Functions


        // Reads a single byte of the datastream
        public byte PacketReadByte(bool moveDataPointer) {

            if(buffer.Count > readPointer) {

                byte byteRead = byteArray[readPointer];
                if (moveDataPointer)
                    readPointer++;

                return byteRead;

            } else {
                Funcs.printMessage(0, "Value of type 'byte' could not be read!", false);
                return nullByte;
            }
        }


        // Reads a byte array with specified size in the datastream
        public byte[] PacketReadBytes(int byteArraySize, bool moveDataPointer) {

            if(buffer.Count > readPointer) {

                byte[] bytesRead = buffer.GetRange(readPointer, byteArraySize).ToArray();
                if (moveDataPointer)
                    readPointer += byteArraySize;

                return bytesRead;

            } else {
                Funcs.printMessage(0, "Value of type 'byte[]' could not be read!", false);
                return null;
            }
        }


        // Reads an int in the datastream
        public int PacketReadInt(bool moveDataPointer) {

            if (buffer.Count > readPointer) {

                int intRead = BitConverter.ToInt32(byteArray, readPointer);

                //Console.WriteLine(intRead.ToString() + " from: " + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod());

                if (moveDataPointer)
                    readPointer += 4;   // Increase pointer by 4 because an int is 32 bits = 4 bytes

                return intRead;

            } else {
                Funcs.printMessage(0, "Value of type 'int' could not be read!", false);
                return 0;
            }
        }


        // Reads a string in the datastream
        public string PacketReadString(bool moveDataPointer) {

            if (buffer.Count > readPointer) {

                int stringSize = PacketReadInt(true);

                string stringRead = Encoding.Unicode.GetString(byteArray, readPointer, stringSize);
                if (moveDataPointer)
                    readPointer += stringSize;

                return stringRead;

            } else {
                Funcs.printMessage(0, "Value of type 'string' could not be read!", false);
                return null;
            }
        }


        #endregion

        #endregion


        public void Dispose() {

            NullifyPacket(true);
            GC.SuppressFinalize(this);
        }
    }
}