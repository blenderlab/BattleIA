using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;

namespace BattleIAserver
{
    class Program
    {
        static void Main(string[] args)
        {

            var currentDir = Directory.GetCurrentDirectory();
            var theFile = Path.Combine(currentDir, "settings.json");
            // création du fichier settings.json avec les valeurs par défaut
            if (!File.Exists(theFile))
            {
                MainGame.Settings = new Settings();
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(MainGame.Settings, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(theFile, json);
            }
            var prm = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(theFile));
            MainGame.Settings = prm;
            if (MainGame.Settings.MapName != ""){
                MainGame.LoadMap(MainGame.Settings.MapName);
            } else {
                MainGame.InitNewMap();
            }
        
            Server server = new Server(new IPEndPoint(IPAddress.Parse("127.0.0.1"), MainGame.Settings.ServerPort));
   /*
             * Bind required events for the server
             */

            server.OnClientConnected += (object sender, OnClientConnectedHandler e) => 
            {
                Console.WriteLine("Client with GUID: {0} Connected!", e.GetClient().GetGuid());
            };

            server.OnClientDisconnected += (object sender, OnClientDisconnectedHandler e) =>
            {
                Console.WriteLine("Client {0} Disconnected", e.GetClient().GetGuid());
            };

            server.OnMessageReceived += (object sender, OnMessageReceivedHandler e) =>
            {
                Console.WriteLine("Received Message: '{1}' from client: {0}", e.GetClient().GetGuid(), e.GetMessage());
            };

            server.OnSendMessage += (object sender, OnSendMessageHandler e) =>
            {
                Console.WriteLine("Sent message: '{0}' to client {1}", e.GetMessage(), e.GetClient().GetGuid());
            };

            // Close the application only when the close button is clicked
            ShowHelp();
            bool exit = false;
            while (!exit)
            {
                Console.Write(">");
                var key = Console.ReadKey(true);
                switch (key.KeyChar.ToString().ToLower())
                {
                    case "h":
                        ShowHelp();
                        break;
                    case "e":
                        Console.WriteLine("Exit program");
                        if (MainGame.AllBot.Count > 0)
                        {
                            Console.WriteLine("Not possible, at least 1 BOT is in arena.");
                        }
                        else
                        {
                            if (MainGame.AllViewer.Count > 0)
                            {
                                Console.WriteLine("Not possible, at least 1 VIEWER is working.");
                            }
                            else
                            {
                                exit = true;
                            }
                        }
                        break;
                    case "g":
                        Console.WriteLine("GO!");
                        MainGame.RunSimulator();
                        break;
                    case "s":
                        Console.WriteLine("Stop");
                        MainGame.StopSimulator();
                        break;
                    case "x": // debug stuff to view shield
                        foreach (OneBot x in MainGame.AllBot)
                        {
                            x.bot.ShieldLevel++;
                            if (x.bot.ShieldLevel > 10)
                                x.bot.ShieldLevel = 0;
                            MainGame.ViewerPlayerShield(x.bot.X, x.bot.Y, x.bot.ShieldLevel);
                        }
                        break;
                    case "w": // debug stuff to view cloak
                        foreach (OneBot x in MainGame.AllBot)
                        {
                            x.bot.CloakLevel++;
                            if (x.bot.CloakLevel > 10)
                                x.bot.CloakLevel = 0;
                            MainGame.ViewerPlayerCloak(x.bot.X, x.bot.Y, x.bot.CloakLevel);
                        }
                        break;
                }
            }
                        Process.GetCurrentProcess().WaitForExit();
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("h\t Display this text");
            Console.WriteLine("e\t Exit program");
            Console.WriteLine("g\t Start simulator");
            Console.WriteLine("s\t Stop simulator");
        }
    }
}