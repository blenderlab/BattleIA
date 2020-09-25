using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BattleIA
{
    public static class MainGame
    {
        public static UInt16 MapWidth = 32;
        public static UInt16 MapHeight = 22;
        private static UInt16 percentWall = 3;
        private static UInt16 percentEnergy = 5;
        public static CaseState[,] TheMap = null;

        public static Random RND = new Random();

        private static Object lockList = new Object();
        public static List<OneClient> AllBot = new List<OneClient>();

        public static void InitNewMap()
        {
            TheMap = new CaseState[MapWidth, MapHeight];
            for (int i = 0; i < MapWidth; i++)
            {
                TheMap[i, 0] = CaseState.Wall;
                TheMap[i, MapHeight - 1] = CaseState.Wall;
                for (int j = 0; j < MapHeight; j++)
                {
                    TheMap[0, j] = CaseState.Wall;
                    TheMap[MapWidth - 1, j] = CaseState.Wall;
                }
            }
            int availableCases = (MapWidth - 2) * (MapHeight - 2);
            int wallToPlace = percentWall * availableCases / 100;
            for (int n = 0; n < wallToPlace; n++)
            {
                byte x, y;
                SearchEmptyCase(out x, out y);
                TheMap[x, y] = CaseState.Wall;
            }
            int energyToPlace = percentEnergy * availableCases / 100;
            for (int n = 0; n < energyToPlace; n++)
            {
                byte x, y;
                SearchEmptyCase(out x, out y);
                TheMap[x, y] = CaseState.Energy;
            }
        }

        public static void SearchEmptyCase(out byte x, out byte y)
        {
            bool ok = false;
            do
            {
                x = (byte)(RND.Next(MapWidth - 2) + 1);
                y = (byte)(RND.Next(MapHeight - 2) + 1);
                if (TheMap[x, y] == CaseState.Empty)
                {
                    ok = true;
                }
            } while (!ok);
        }

        private static bool turnRunning = false;

        public static async void DoTurns()
        {
            if (turnRunning) return;
            turnRunning = true;
            System.Diagnostics.Debug.WriteLine("Starting turns...");
            while (turnRunning)
            {
                System.Diagnostics.Debug.WriteLine("One turns...");
                OneClient[] bots = null;
                int count = 0;
                lock (lockList)
                {
                    count = AllBot.Count;
                    if (count > 0)
                    {
                        bots = new OneClient[count];
                        AllBot.CopyTo(bots);
                    }
                }
                if (count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No bot!");
                    //Thread.Sleep(500);
                    turnRunning = false;
                }
                else
                {
                    for (int i = 0; i < bots.Length; i++)
                    {
                        await bots[i].StartNewTurn();
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("Finish turns...");
        }

        private static Object lockListViewer = new Object();
        private static List<OneViewer> allViewer = new List<OneViewer>();

        public static async Task AddViewer(WebSocket webSocket)
        {
            OneViewer client = new OneViewer(webSocket);
            List<OneViewer> toRemove = new List<OneViewer>();
            lock (lockListViewer)
            {
                foreach (OneViewer o in allViewer)
                {
                    if (o.MustRemove)
                        toRemove.Add(o);
                }
                allViewer.Add(client);
            };
            foreach (OneViewer o in toRemove)
                RemoveViewer(o.ClientGuid);
            System.Diagnostics.Debug.WriteLine($"#viewer: {allViewer.Count}");
            // on se met à l'écoute des messages de ce client
            await client.WaitReceive();
            RemoveViewer(client.ClientGuid);
        }

        public static void RemoveViewer(Guid guid)
        {
            OneViewer toRemove = null;
            lock (lockListViewer)
            {
                foreach (OneViewer o in allViewer)
                {
                    if (o.ClientGuid == guid)
                    {
                        toRemove = o;
                        break;
                    }
                }
                if (toRemove != null)
                    allViewer.Remove(toRemove);
            }
            System.Diagnostics.Debug.WriteLine($"#viewer: {allViewer.Count}");
        }

        /// <summary>
        /// Ajout d'un nouveau client avec sa websocket
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static async Task AddClient(WebSocket webSocket)
        {
            OneClient client = new OneClient(webSocket);
            List<OneClient> toRemove = new List<OneClient>();
            lock (lockList)
            {
                // au cas où, on en profite pour faire le ménage
                foreach (OneClient o in AllBot)
                {
                    if (o.State == BotState.Error || o.State == BotState.Disconnect)
                        toRemove.Add(o);
                }
                AllBot.Add(client);
            };
            // fin du ménage
            foreach (OneClient o in toRemove)
                Remove(o.ClientGuid);
            System.Diagnostics.Debug.WriteLine($"#clients: {AllBot.Count}");

            Thread t = new Thread(DoTurns);
            t.Start();

            // on se met à l'écoute des messages de ce client
            await client.WaitReceive();
            // arrivé ici, c'est que le client s'est déconnecté
            // on se retire de la liste des clients websocket
            Remove(client.ClientGuid);
        }

        /// <summary>
        /// Retrait d'un client
        /// on a surement perdu sa conenction
        /// </summary>
        /// <param name="guid">l'id du client qu'il faut enlever</param>
        public static void Remove(Guid guid)
        {
            OneClient toRemove = null;
            lock (lockList)
            {
                foreach (OneClient o in AllBot)
                {
                    if (o.ClientGuid == guid)
                    {
                        toRemove = o;
                        break;
                    }
                }
                if (toRemove != null)
                    AllBot.Remove(toRemove);
            }
            if (toRemove != null)
            {
                RefreshViewer();
            }
            System.Diagnostics.Debug.WriteLine($"#clients: {AllBot.Count}");
        }

        /// <summary>
        /// Diffusion d'un message à l'ensemble des clients !
        /// Attention à ne pas boucler genre...
        /// je Broadcast un message quand j'en reçois un...
        /// Méthode "dangereuse" à peut-être supprimer
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static void Broadcast(string text)
        {
            lock (lockList)
            {
                foreach (OneClient o in AllBot)
                {
                    o.SendMessage(text);
                }
            }
        }

        public static void RefreshViewer()
        {
            lock (lockListViewer)
            {
                foreach (OneViewer o in allViewer)
                {
                    o.SendMapInfo();
                }
            }
        }

        public static void ViewerMovePlayer(byte x1, byte y1, byte x2, byte y2)
        {
            lock (lockListViewer)
            {
                foreach (OneViewer o in allViewer)
                {
                    o.SendMovePlayer(x1, y1, x2, y2);
                }
            }
        }

    }
}
