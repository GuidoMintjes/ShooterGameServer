using System;
using System.IO;
using GameServer;
using System.Threading;

namespace GameServer {
    class ChatApp {

        private static bool isRunning = false;

        public static int countIntSend = 0;

        static void Main(string[] args) {

            Console.Write(@"                            Welcome to the server program!
                            
                            Press enter to use commands!
                                - 'say' to send a message to all clients


                            " + "\n");

            Console.Title = "TCP Chat Server Demo";
            int maxConnectionsStart = 10, portStart = 0;

            string INFO = File.ReadAllText(@"SERVER_INFO.txt");

            string[] INFOS = INFO.Split(';');


            try {
                maxConnectionsStart = Convert.ToInt32(INFOS[0]);
                portStart = Convert.ToInt32(INFOS[1]);
            } catch {

                Funcs.printMessage(0, "File (SERVER_INFO.txt) contents broken!", false);
                Console.ReadLine();
                Environment.Exit(0);
            }

            ThreadManager.UpdateMain();

            isRunning = true;           // Set running status to be active

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            ChatServer.StartServer(maxConnectionsStart, portStart);
        }


        // Run the logic loop
        private static void MainThread() {

            Funcs.printMessage(1, $"Main thread with loop started at {Consts.TICKMSDURATION} ms per tick!", false);
            DateTime nextCycle = DateTime.Now;

            while(isRunning) {

                while(nextCycle < DateTime.Now) {
                    
                    Logic.Update();

                    nextCycle = nextCycle.AddMilliseconds(Consts.TICKMSDURATION);

                    // Fix voor hoge CPU usage
                    if(nextCycle > DateTime.Now) {

                        Thread.Sleep(nextCycle - DateTime.Now);
                    }
                }
            }
        }
    }
}