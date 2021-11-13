using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GameServer {
    public static class Funcs {

        private static string errorAlert = "[ERROR] ";
        private static string warningAlert = "[WARNING] ";
        private static string messageAlert = "[MSG] ";
        private static string serverAlert = "[SERVER] ";
        private static string chatAlert = "[CHAT] ";

        private static bool allowTypeWrite = false;

        //private static StreamWriter logOutputter;

        public static void Initialize() {

            PrintMessage(2, "Starting output log service...");

            // Initialize the ServerLog text file
            //logOutputter = new StreamWriter("ServerLog.txt");
            //logOutputter.WriteLine("BLAH BLAH TEST");
            //logOutputter.Close();

            File.AppendAllText(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "ServerLog.txt",
                                    "" + Environment.NewLine);
        }

        public static void PrintMessage(int alertLevel, string message, bool typeWrite = false) {

            if (ServerCommand.reading) {

                Thread T1 = new Thread(() => printer(alertLevel, message, typeWrite));

                T1.Start();

            } else {

                printer(alertLevel, message, typeWrite);
            }
        }


        private static void printer(int alertLevel, string message, bool typeWrite) {

            if (string.IsNullOrEmpty(message)) {

                Console.WriteLine();
                OutputLog("");

            } else {

                if (allowTypeWrite) {
                    switch (alertLevel) {
                        case 0:

                            string msgErr = errorAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            if (typeWrite)
                                slowType(msgErr, 3);
                            else
                                Console.WriteLine(msgErr);

                            OutputLog(msgErr);

                            break;

                        case 1:
                            string msgWarn = warningAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            if (typeWrite)
                                slowType(msgWarn, 3);
                            else
                                Console.WriteLine(msgWarn);

                            OutputLog(msgWarn);

                            break;

                        case 2:
                            string msgMsg = messageAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            if (typeWrite)
                                slowType(msgMsg, 3);
                            else
                                Console.WriteLine(msgMsg);

                            OutputLog(msgMsg);

                            break;

                        case 3:
                            string msgServer = serverAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            if (typeWrite)
                                slowType(msgServer, 3);
                            else
                                Console.WriteLine(msgServer);

                            OutputLog(msgServer);

                            break;

                        case 4:
                            string msgChat = chatAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            if (typeWrite)
                                slowType(msgChat, 3);
                            else
                                Console.WriteLine(msgChat);

                            OutputLog(msgChat);

                            break;

                        default:
                            break;
                    }
                } else {

                    switch (alertLevel) {
                        case 0:

                            string msgErr = errorAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            Console.WriteLine(msgErr);

                            OutputLog(msgErr);

                            break;

                        case 1:
                            string msgWarn = warningAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            Console.WriteLine(msgWarn);

                            OutputLog(msgWarn);

                            break;

                        case 2:
                            string msgMsg = messageAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            Console.WriteLine(msgMsg);

                            OutputLog(msgMsg);

                            break;

                        case 3:
                            string msgServer = serverAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            Console.WriteLine(msgServer);

                            OutputLog(msgServer);

                            break;

                        case 4:
                            string msgChat = chatAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                            Console.WriteLine(msgChat);

                            OutputLog(msgChat);

                            break;

                        default:
                            break;
                    }
                }
            }
        }



        public static void slowType(string message, int delay) {
            foreach (char character in message) {
                Console.Write(character);
                System.Threading.Thread.Sleep(delay);
            }
            Console.Write("\n");
        }


        private static void OutputLog(string message) {

            File.AppendAllText(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "ServerLog.txt", 
                                    message + Environment.NewLine);

            /*
            logOutputter = new StreamWriter("ServerLog.txt");
            logOutputter.WriteLine(message);
            logOutputter.NewLine += "\n";
            logOutputter.Close();
            */
        }


        public static void PrintData(byte[] data, bool sending = false) {

            string debugger = "";

            if(sending)
                debugger = "Sending packet: ";
            else
                debugger = "Receiving packet: ";

            foreach (byte byt in data) {

                debugger += byt.ToString() + " ";
            }

            PrintMessage(2, debugger, false);
        }
    }
}