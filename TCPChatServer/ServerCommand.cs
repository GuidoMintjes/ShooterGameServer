using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameServer {
    public static class ServerCommand {

        public static bool reading = false;

        public static void CommandLoop() {

            string commandRaw;

            while (true) {
                if (Console.KeyAvailable) {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key) {
                        case ConsoleKey.Enter:
                            Funcs.PrintMessage(2, "");
                            Funcs.PrintMessage(3, "Input command:", false);
                            commandRaw = Console.ReadLine();
                            Funcs.PrintMessage(2, "");

                            SendCommand(commandRaw);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        private static void SendCommand(string commandRaw) {

            string command = "", argument = "";

            if (!String.IsNullOrEmpty(commandRaw)) {

                string[] commandsRaw = commandRaw.Split(" ", 2);

                try {
                    command = commandsRaw[0];
                    argument = commandsRaw[1];
                } catch {
                    Funcs.PrintMessage(1, "Command not formatted right!", false);
                }

                switch (command) {
                    
                    default:

                        Funcs.PrintMessage(1, "Command not formatted right!", false);
                        CommandLoop();
                        break;
                }

                reading = false;

                CommandLoop();
            }
        }
    }
}