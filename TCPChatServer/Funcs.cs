using System;
using System.Threading;

namespace GameServer {
    public static class Funcs {

        private static string errorAlert = "[ERROR] ";
        private static string warningAlert = "[WARNING] ";
        private static string messageAlert = "[MSG] ";
        private static string serverAlert = "[SERVER] ";
        private static string chatAlert = "[CHAT] ";

        private static bool allowTypeWrite = false;

        public static void printMessage(int alertLevel, string message, bool typeWrite) {

            if (ServerCommand.reading) {

                Thread T1 = new Thread(() => printer(alertLevel, message, typeWrite));

                T1.Start();

            } else {

                printer(alertLevel, message, typeWrite);
            }
        }


        private static void printer(int alertLevel, string message, bool typeWrite) {


            if (allowTypeWrite) {
                switch (alertLevel) {
                    case 0:

                        string msgErr = errorAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        if (typeWrite)
                            slowType(msgErr, 3);
                        else
                            Console.WriteLine(msgErr);

                        break;

                    case 1:
                        string msgWarn = warningAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        if (typeWrite)
                            slowType(msgWarn, 3);
                        else
                            Console.WriteLine(msgWarn);

                        break;

                    case 2:
                        string msgMsg = messageAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        if (typeWrite)
                            slowType(msgMsg, 3);
                        else
                            Console.WriteLine(msgMsg);

                        break;

                    case 3:
                        string msgServer = serverAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        if (typeWrite)
                            slowType(msgServer, 3);
                        else
                            Console.WriteLine(msgServer);

                        break;

                    case 4:
                        string msgChat = chatAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        if (typeWrite)
                            slowType(msgChat, 3);
                        else
                            Console.WriteLine(msgChat);

                        break;

                    default:
                        break;
                }
            } else {

                switch (alertLevel) {
                    case 0:

                        string msgErr = errorAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        Console.WriteLine(msgErr);

                        break;

                    case 1:
                        string msgWarn = warningAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        Console.WriteLine(msgWarn);

                        break;

                    case 2:
                        string msgMsg = messageAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        Console.WriteLine(msgMsg);

                        break;

                    case 3:
                        string msgServer = serverAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        Console.WriteLine(msgServer);

                        break;

                    case 4:
                        string msgChat = chatAlert + "{" + DateTime.Now.ToString("HH:mm:ss") + "} " + message;

                        Console.WriteLine(msgChat);

                        break;

                    default:
                        break;
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



        public static void PrintData(byte[] data) {

            string debugger = "Received packet: ";

            foreach (byte byt in data) {

                debugger += byt.ToString() + " ";
            }

            printMessage(2, debugger, false);
        }
    }
}