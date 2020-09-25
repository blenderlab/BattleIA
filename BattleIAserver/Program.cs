using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace BattleIAserver
{
    class Program
    {
        static void Main(string[] args)
        {

            var currentDir = Directory.GetCurrentDirectory();
            var pathToContentRoot = Path.Combine(currentDir, "WebPages");
            Console.WriteLine($"ContentRoot: {pathToContentRoot}");

            var theFile = Path.Combine(currentDir, "settings.json");
            // création du fichier settings.json avec les valeurs par défaut
            if(!File.Exists(theFile))
            {
                MainGame.Settings = new Settings();
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(MainGame.Settings, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(theFile, json);
            }
            var prm = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(theFile));
            MainGame.Settings = prm;
            MainGame.InitNewMap();

            var host = new WebHostBuilder()
            .UseContentRoot(pathToContentRoot)
            .UseKestrel()
            .UseStartup<Startup>()
            .ConfigureKestrel((context, options) => { options.ListenAnyIP(MainGame.Settings.ServerPort); })
            .Build();                     //Modify the building per your needs

            host.Start();                     //Start server non-blocking

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
                        if (MainGame.AllBot.Count>0)
                        {
                            Console.WriteLine("Not possible, at least 1 BOT is in arena.");
                        } else
                        {
                            if(MainGame.AllViewer.Count > 0)
                            {
                                Console.WriteLine("Not possible, at least 1 VIEWER is working.");
                            } else
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
            host.StopAsync();
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