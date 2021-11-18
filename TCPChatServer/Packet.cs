using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer {

    // Packet sent from server to client, in this case only a welcome message
    public enum ServerPackets {
        welcome = 1,
        spawnPlayer,
        PlayerPosition,
        PlayerRotation,
        PlayerDisconnected
    }


    // Packet sent from client to server, in this case confirming the welcome message
    public enum ClientPackets {
        welcomeReceived = 1,
        playerMovement,
        playerRotation,
        playerDisconnected
    }


    // A packet is a piece of information that gets sent over a network
    public class Packet : IDisposable {


        public byte nullByte = 00000000;

        private List<byte> buffer;
        private byte[] byteArray;
        private int readPointer;
        
        public int ReadPointer { get { return readPointer; } }

        public Packet() {

            buffer = new List<byte>();    // New and empty packet gets created
            readPointer = 0;            // Set the pointer to 0 in order to start reading at the start of the packet
        }


        // Make a Packet with an ID, such a packet can then be used in transmitting
        public Packet(int id) {

            buffer = new List<byte>();    // New and empty packet gets created
            readPointer = 0;            // Set the pointer to 0 in order to start reading at the start of the packet

            Write(id);
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

            Write(dataSet);
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

            //Funcs.PrintMessage("Inserted packet length to first position.");

            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }


        #endregion

        #region Packet Data Writing Functions


        // Add a bytearray to the datastream
        void Write(byte[] _dataSet) {
            buffer.AddRange(_dataSet);
        }


        /// <summary>
        /// Add an integer to the packet/bytestream, mainly used for adding the packet id that is used in sending
        /// </summary>
        /// <param name="_intValue"> The actual integer value that is added to the packet (4 bytes) </param>
        public void Write(int _intValue) {

            GameServerApplication.countIntSend++;

            //Funcs.PrintMessage(_intValue.ToString() + " sent as no. " + GameServerApplication.countIntSend + 
                //" from: " + (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod());

            buffer.AddRange(BitConverter.GetBytes(_intValue));
        }


        // Add a string to the packet/datastream
        public void Write(string _stringValue) {

            int a = Encoding.Unicode.GetByteCount(_stringValue);   // A string isn't always the same size, which is why the length of the string
                                                                   // has to be added to the datastream, so the other end knows how long to read
                                                                   // keep reading for just the string, an integer is always 4 bytes
            Write(a);
            
            buffer.AddRange(Encoding.Unicode.GetBytes(_stringValue)); // Add to the packet/datastream the string itself
        }

        public void Write(float _value) {

            buffer.AddRange(BitConverter.GetBytes(_value));
        }


        public void Write(Vector3 value, bool doStuff = false) {

            Write(value.X);
            Write(value.Y);
            Write(value.Z);

            if (doStuff && 1 == 0) {
                Funcs.PrintMessage(4, $"Sending packet with vector3 data: ");
                Funcs.PrintData(GetPacketBytes());

                Funcs.PrintData(BitConverter.GetBytes(value.X));
                Funcs.PrintData(BitConverter.GetBytes(value.Y));
                Funcs.PrintData(BitConverter.GetBytes(value.Z));

                Funcs.PrintMessage(2, "");
            }
        }


        public void Write(Quaternion value) {

            Write(value.X);
            Write(value.Y);
            Write(value.Z);
            Write(value.W);
        }

        #endregion

        #region Packet Data Reading Functions


        // Reads a single byte of the datastream
        public byte ReadByte(bool moveDataPointer) {

            if(buffer.Count > readPointer) {

                byte byteRead = byteArray[readPointer];
                if (moveDataPointer)
                    readPointer++;

                return byteRead;

            } else {
                Funcs.PrintMessage(0, "Value of type 'byte' could not be read!", false);
                return nullByte;
            }
        }


        // Reads a byte array with specified size in the datastream
        public byte[] ReadBytes(int byteArraySize, bool moveDataPointer) {

            if(buffer.Count > readPointer) {

                byte[] bytesRead = buffer.GetRange(readPointer, byteArraySize).ToArray();
                if (moveDataPointer)
                    readPointer += byteArraySize;

                return bytesRead;

            } else {
                Funcs.PrintMessage(0, "Value of type 'byte[]' could not be read!", false);
                return null;
            }
        }


        // Reads an int in the datastream
        public int ReadInt(bool moveDataPointer, bool showReadPointers = false) {

            if (buffer.Count > readPointer) {

                int intRead = BitConverter.ToInt32(byteArray, readPointer);

                if(showReadPointers)
                    Funcs.PrintMessage(4, "Read Pointer before reading integer is: " + readPointer);

                if (moveDataPointer)
                    readPointer += 4;   // Increase pointer by 4 because an int is 32 bits = 4 bytes

                if(showReadPointers)
                    Funcs.PrintMessage(4, "Read Pointer after reading integer is: " + readPointer);

                return intRead;

            } else {
                Funcs.PrintMessage(0, "Value of type 'int' could not be read!", false);
                return 0;
            }
        }


        // Reads a string in the datastream
        public string ReadString(bool moveDataPointer) {

            if (buffer.Count > readPointer) {

                int stringSize = ReadInt(true);

                string stringRead = Encoding.Unicode.GetString(byteArray, readPointer, stringSize);
                if (moveDataPointer)
                    readPointer += stringSize;

                return stringRead;

            } else {
                Funcs.PrintMessage(0, "Value of type 'string' could not be read!", false);
                return null;
            }
        }


        public float ReadFloat(bool _moveReadPos = true) {

            if (buffer.Count > readPointer) {

                // If there are unread bytes
                float _value = BitConverter.ToSingle(byteArray, readPointer); // Convert the bytes to a float

                if (_moveReadPos) {

                    // If _moveReadPos is true
                    readPointer += 4; // Increase readPos by 4
                }

                return _value; // Return the float

            } else {

                throw new Exception("Could not read value of type 'float'!");
            }
        }


        public Vector3 ReadVector3(bool _moveReadPos) {

            return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }


        public Quaternion ReadQuaternion(bool _moveReadPos) {

            return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        #endregion

        #endregion


        public void Dispose() {

            NullifyPacket(true);
            GC.SuppressFinalize(this);
        }
    }
}