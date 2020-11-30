using BattleIA;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace BattleIAserver
{
    public static class MainGame
    {
        public static Settings Settings;

        /// <summary>
        /// Contient le terrain de simulation
        /// </summary>
        public static CaseState[,] TheMap = null;

        public static Random RND = new Random();

        /// <summary>
        /// Objet pour verrou lors de l'utilisation de la LIST car nous sommes en thread !
        /// </summary>
        private static Object lockListBot = new Object();

        /// <summary>
        /// L'ensemble des BOTs client connectés
        /// </summary>
        public static List<OneBot> AllBot = new List<OneBot>();


        /// <summary>
        /// Liste des viewers, et lock pour cette liste.
        /// TODO: Améliorer le système de lock avec un Mutex.
        /// </summary>
        private static Object lockListViewer = new Object();
        public static List<OneDisplay> AllViewer = new List<OneDisplay>();


        /// <summary>
        /// Liste des cockpits, et lock pour cette liste.
        /// TODO: Améliorer le système de lock avec un Mutex.
        /// </summary>
        private static Object lockListCockpit = new Object();
        public static List<OneCockpit> AllCockpit = new List<OneCockpit>();
        public static List<int> respawnList_X = new List<int>(); // création de la liste
        public static List<int> respawnList_Y = new List<int>();
        /// <summary>
        /// Sommes-nous dans un tour (?)
        /// </summary>
        private static bool turnRunning = false;


        public static byte isRunning(){
            if (turnRunning) {
                return (1);
            } else {
                return (0);
            }
           
        }
        public static void LoadMap(String mapname)
        {
            // Read the file as one string.
            mapname="Maps/"+mapname;
            string[] lines = System.IO.File.ReadAllLines(@mapname);
        
            int mWidth = lines[0].Length; 
            int mHeight = 0;
            foreach (string line in lines ){
                mHeight++;
            }
                        // Now we know height & Width :
            TheMap = new CaseState[mWidth, mHeight];
            Settings.MapWidth= (UInt16)mWidth;
            Settings.MapHeight= (UInt16)mHeight;

            //pour chaque ligne du fichier : 
            int nline = 0;
        
            foreach (string line in lines) {
                //Console.WriteLine(line);
                int nchar = 0;
                // pour chaque char de la ligne 
                byte[] b=Encoding.UTF8.GetBytes(line);
                // For each character of the line :
                foreach (char c in line)  {
                    // force each character to be read as an integer 
                    // The cast it into a CaseState 
                    TheMap[nchar,nline]= (CaseState)int.Parse(c.ToString());
                    if (TheMap[nchar, nline] == CaseState.Respawn)
                    {
                        respawnList_X.Add(nchar);
                        respawnList_Y.Add(nline);
                    }
                    nchar++;
                }
                nline++;
            }
            Console.WriteLine($"[MAP] Name : {mapname}");
            Console.WriteLine($"[MAP] Size : {mWidth}x{mHeight}");
            Console.WriteLine($"[MAP] Respawn : {respawnList_X.Count}");
            

           // respawnList_X.ForEach((int x) => { BattleLogger.logger.info($"x: {x}"); });
           // respawnList_Y.ForEach((int y) => { BattleLogger.logger.info($"y: {y}"); });
        }
      


        /// <summary>
        /// Création d'un nouveau terrai de simulation, complet
        /// </summary>
        public static void InitNewMap()
        {
            Console.WriteLine($"Building a new arena {Settings.MapWidth}x{Settings.MapHeight}");
            TheMap = new CaseState[Settings.MapWidth, Settings.MapHeight];
            // les murs extérieurs
            for (int i = 0; i < Settings.MapWidth; i++)
            {
                TheMap[i, 0] = CaseState.Wall;
                TheMap[i, Settings.MapHeight - 1] = CaseState.Wall;
                for (int j = 0; j < Settings.MapHeight; j++)
                {
                    TheMap[0, j] = CaseState.Wall;
                    TheMap[Settings.MapWidth - 1, j] = CaseState.Wall;
                }
            }
            int availableCases = (Settings.MapWidth - 2) * (Settings.MapHeight - 2);
            int wallToPlace = Settings.MapPercentWall * availableCases / 100;
            MapXY xy = new MapXY();
            // on ajoute quelques blocs à l'intérieur
            for (int n = 0; n < wallToPlace; n++)
            {
                xy = SearchEmptyCase();
                TheMap[xy.X, xy.Y] = CaseState.Wall;
            }
            // et on y place des cellules d'énergie
            RefuelMap();
        }

        /// <summary>
        /// On place des celulles d'énergie
        /// </summary>
        public static void RefuelMap()
        {
            int availableCases = (Settings.MapWidth - 2) * (Settings.MapHeight - 2);
            int energyToPlace = Settings.MapPercentEnergy * availableCases / 100;
            int count = 0;
            for (int i = 0; i < Settings.MapWidth; i++)
            {
                for (int j = 0; j < Settings.MapHeight; j++)
                {
                    if (TheMap[i, j] == CaseState.Energy)
                        count++;
                }
            }
            energyToPlace -= count;

            MapXY xy = new MapXY();
            // et on y place des cellules d'énergie
            for (int n = 0; n < energyToPlace; n++)
            {
                xy = SearchEmptyCase();
                TheMap[xy.X, xy.Y] = CaseState.Energy;
                if (SimulatorThread.IsAlive)
                {
                    ViewerAddEnergy(xy.X, xy.Y);
                }
            }
        }

        /// <summary>
        /// Recherche une case vide dans le terrain de simulation
        /// </summary>
        /// <param name="x">Retourne le X de la case trouvée</param>
        /// <param name="y">Retourne le Y de la case trouvée</param>
        public static MapXY SearchEmptyCase()
        {
            bool ok = false;
            MapXY xy = new MapXY();
            do
            {
                xy.X = (byte)(RND.Next(Settings.MapWidth - 2) + 1);
                xy.Y = (byte)(RND.Next(Settings.MapHeight - 2) + 1);
                if (TheMap[xy.X, xy.Y] == CaseState.Empty)
                {
                    ok = true;
                }
            } while (!ok);
            return xy;
        }


        /// <summary>
        /// Recherche une case vide de respawn dans le terrain de simulation
        /// </summary>
        /// <param name="x">Retourne le X de la case trouvée</param>
        /// <param name="y">Retourne le Y de la case trouvée</param>
        public static MapXY SearchRespawnCase()
        {
            MapXY xy = new MapXY();
            // If no respawn, try to find an empty case :
            if (respawnList_X.Count==0){
                xy = SearchEmptyCase();
                return xy;
            }

            var rnd = new Random();
            bool ok = false;
            do
            {   
                // index au hasard parmis les cases de resapwn :
                var rand_number = rnd.Next(respawnList_X.Count-1);
                if (TheMap[respawnList_X[rand_number], respawnList_X[rand_number]] == CaseState.Respawn)
                {
                    ok = true;
                    xy.X=(byte)respawnList_X[rand_number];
                    xy.Y=(byte)respawnList_Y[rand_number];
                }
            } while (!ok);
            return xy;
        }


        public static void SendMapInfoToCockpit(Guid guid)
        {
            var buffer = new byte[5 + Settings.MapWidth * MainGame.Settings.MapHeight];
            buffer[0] = (byte)Message.m_Map;
            buffer[1] = (byte)Settings.MapWidth;
            buffer[2] = (byte)(Settings.MapWidth >> 8);
            buffer[3] = (byte)Settings.MapHeight;
            buffer[4] = (byte)(Settings.MapHeight >> 8);
            int index = 5;
            for (int j = 0; j < MainGame.Settings.MapHeight; j++)
                for (int i = 0; i < MainGame.Settings.MapWidth; i++)
                {
                    switch (MainGame.TheMap[i, j])
                    {
                        case CaseState.Wall:
                        case CaseState.Empty:
                            buffer[index++] = (byte)MainGame.TheMap[i, j];
                            break;
                        default:
                            buffer[index++] = (byte)CaseState.Empty; ;
                            break;
                    }
                }
            SendCockpitInfo(guid, new ArraySegment<byte>(buffer, 0, buffer.Length));
        }




        /// <summary>
        /// Exécute la simulation dans son ensemble !
        /// </summary>
        public static async void DoTurns()
        {
            if (turnRunning) return;
            turnRunning = true;
            Console.WriteLine("Running simulator...");
            int turnCount = 0;
            while (turnRunning)
            {
                //System.Diagnostics.Debug.WriteLine("One turns...");
                OneBot[] bots = null;
                int count = 0;
                lock (lockListBot)
                {
                    count = AllBot.Count;
                    if (count > 0)
                    {
                        bots = new OneBot[count];
                        AllBot.CopyTo(bots);
                    }
                }
                if (count == 0)
                {
                    if (Settings.EndlessMode)
         
                    {
                        // Disabled: Will spam the console until a bot joins.
                        // Console.WriteLine("Last bot left. Endless mode is active, continuing");
                    }
                    else
                    {
                        Console.WriteLine("No more BOT, ending simulator.");
                        turnRunning = false;
                    }
                }
                else
                {
                    for (int i = 0; i < bots.Length; i++)
                    {
                        Console.WriteLine($"Turn #{turnCount} Bot {bots[i].bot.Name}");
                        await bots[i].StartNewTurn();
                        DateTime start = DateTime.UtcNow;
                        while ((bots[i].State != BotState.Ready) && (DateTime.UtcNow - start).TotalSeconds < Settings.MaxDelaySecondByTurn)
                        {
                            Thread.Sleep(2);
                        }
                        if (bots[i].State != BotState.Ready)
                        {
                            // trop long, ajout pénalité !
                            // TODO: rien pour le moment
                        }
                        Thread.Sleep(Settings.DelayBetweenEachBotTurn);
                    }
                    // on génère de l'énergie si nécessaire
                    MainGame.RefuelMap();
                    turnCount++;
                    if (turnCount % MainGame.Settings.EnergyPodLessEvery == 0)
                    {
                        if (Settings.EnergyPodMax > Settings.EnergyPodMin)
                            Settings.EnergyPodMax--;
                    }
                }
            }
            Console.WriteLine("End of running.");
        }



        public static async Task AddCockpit(WebSocket webSocket)
        {
            OneCockpit client = new OneCockpit(webSocket);
            List<OneCockpit> toRemove = new List<OneCockpit>();
            lock (lockListCockpit)
            {
                foreach (OneCockpit o in AllCockpit)
                {
                    if (o.MustRemove)
                        toRemove.Add(o);
                }
                AllCockpit.Add(client);
            };
            foreach (OneCockpit o in toRemove)
                RemoveCockpit(o.ClientGuid);
            Console.WriteLine($"#cockpit: {AllCockpit.Count}");
            await client.WaitReceive();
            RemoveCockpit(client.ClientGuid);
        }

        public static void RemoveCockpit(Guid guid)
        {
            OneCockpit toRemove = null;
            lock (lockListViewer)
            {
                foreach (OneCockpit o in AllCockpit)
                {
                    if (o.ClientGuid == guid)
                    {
                        toRemove = o;
                        break;
                    }
                }
                if (toRemove != null)
                    AllCockpit.Remove(toRemove);
            }
            Console.WriteLine($"#cockpit: {AllCockpit.Count}");
        }

        public static void SendCockpitInfo(Guid guid, ArraySegment<byte> buffer)
        {
            lock (lockListViewer)
            {
                foreach (OneCockpit o in AllCockpit)
                {
                    if (o.ClientGuid == guid)
                    {
                        o.SendInfo(buffer);
                    }
                }
            }
        }

        public static void SendCockpitInfo(Guid guid, string info)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(info);
            lock (lockListViewer)
            {
                foreach (OneCockpit o in AllCockpit)
                {
                    if (o.ClientGuid == guid)
                    {
                        o.SendInfo(buffer);
                    }
                }
            }
        }


        /// <summary>
        /// Un nouveau VIEWER de la simulation
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static async Task AddViewer(WebSocket webSocket)
        {
            // on en fait un vrai client
            OneDisplay client = new OneDisplay(webSocket);
            // on profite de faire le ménage au cas où
            List<OneDisplay> toRemove = new List<OneDisplay>();
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    if (o.MustRemove)
                        toRemove.Add(o);
                }
                AllViewer.Add(client);
            };
            foreach (OneDisplay o in toRemove)
                RemoveViewer(o.ClientGuid);
            Console.WriteLine($"#display: {AllViewer.Count}");
            // on se met à l'écoute des messages de ce client
            RefreshViewer();
            await client.WaitReceive();

            RemoveViewer(client.ClientGuid);
        }

        public static void RemoveViewer(Guid guid)
        {
            OneDisplay toRemove = null;
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    if (o.ClientGuid == guid)
                    {
                        toRemove = o;
                        break;
                    }
                }
                if (toRemove != null)
                    AllViewer.Remove(toRemove);
                else
                    Console.WriteLine($"[DISPLAY ERROR] could not found {guid}");
            }
            Console.WriteLine($"#display: {AllViewer.Count}");
        }

        /// <summary>
        /// Ajout d'un nouveau client avec sa websocket
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public static async Task AddBot(WebSocket webSocket)
        {
            OneBot client = new OneBot(webSocket);
            List<OneBot> toRemove = new List<OneBot>();
            //Console.WriteLine("un peu de ménage");
            lock (lockListBot)
            {
                // au cas où, on en profite pour faire le ménage
                foreach (OneBot o in AllBot)
                {
                    if (o.State == BotState.Error || o.State == BotState.Disconnect)
                        toRemove.Add(o);
                }
                AllBot.Add(client);
            };
            // fin du ménage
            RefreshViewer();
            //Console.WriteLine("Do it!");
            foreach (OneBot o in toRemove)
                RemoveBot(o.ClientGuid);
            Console.WriteLine($"#bots: {AllBot.Count}");
          
            // on se met à l'écoute des messages de ce client
            await client.WaitReceive();
            // arrivé ici, c'est que le client s'est déconnecté
            // on se retire de la liste des clients websocket
            RemoveBot(client.ClientGuid);
        }

        public static CaseState IsEnnemyVisible(byte px, byte py, int ex, int ey)
        {
            lock (lockListBot)
            {
                foreach (OneBot o in AllBot)
                {
                    if (o.bot.X == ex && o.bot.Y == ey)
                    {
                        if (o.bot.CloakLevel == 0)
                            return CaseState.Ennemy;
                        if ((Math.Abs(ex - px) <= o.bot.CloakLevel) && (Math.Abs(ey - py) <= o.bot.CloakLevel))
                            return CaseState.Empty;
                        return CaseState.Ennemy;
                    }
                }
            };
            return CaseState.Empty;
        }

        public static Thread SimulatorThread = new Thread(DoTurns);

        public static void RunSimulator()
        {
            //Thread t = new Thread(DoTurns);
            if (SimulatorThread.IsAlive)
            {
                Console.WriteLine("Simulator is already running.");
                return;
            }
            Settings.EnergyPodMax = Settings.EnergyPodTo;
            SimulatorThread = new Thread(DoTurns);
            SimulatorThread.Start();
        }

        public static void StopSimulator()
        {
            if (SimulatorThread.IsAlive)
            {
                turnRunning = false;
                //SimulatorThread.Abort();
            }
        }

        /// <summary>
        /// Retrait d'un client
        /// on a surement perdu sa conenction
        /// </summary>
        /// <param name="guid">l'id du client qu'il faut enlever</param>
        public static void RemoveBot(Guid guid)
        {
            OneBot toRemove = null;
            lock (lockListBot)
            {
                foreach (OneBot o in AllBot)
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
                ViewerRemovePlayer(toRemove.bot.X, toRemove.bot.Y);
                RefreshViewer();
            }
            Console.WriteLine($"#bots: {AllBot.Count}");
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
            lock (lockListBot)
            {
                foreach (OneBot o in AllBot)
                {
                    o.SendMessage(text);
                }
            }
        }

        public static void RefreshViewer()
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendMapInfo();

                    o.SendBotInfo();
                }
            }
        }

        public static async Task ViewerMovePlayer(byte x1, byte y1, byte x2, byte y2)
        {
            List<OneDisplay> temp = new List<OneDisplay>();
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    temp.Add(o);
                }
            }
            foreach (OneDisplay o in temp)
            {
                await o.SendMovePlayer(x1, y1, x2, y2);
            }
        }

        public static void ViewerRemovePlayer(byte x1, byte y1)
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendRemovePlayer(x1, y1);
                }
            }
        }

        public static void ViewerClearCase(byte x1, byte y1)
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendClearCase(x1, y1);
                }
            }
        }

        public static void ViewerAddEnergy(byte x1, byte y1)
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendAddEnergy(x1, y1);
                }
            }
        }

        public static void ViewerAddBullet(byte x1, byte y1, byte direction, UInt16 duration)
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendAddBullet(x1, y1, direction, duration);
                }
            }
        }

        public static void ViewerPlayerShield(byte x1, byte y1, UInt16 s)
        {
            ViewerPlayerShield(x1, y1, (byte)(s & 0xFF), (byte)(s >> 8));
        }

        public static void ViewerPlayerShield(byte x1, byte y1, byte s1, byte s2)
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendPlayerShield(x1, y1, s1, s2);
                }
            }
        }

        public static void ViewerPlayerCloak(byte x1, byte y1, UInt16 s)
        {
            ViewerPlayerCloak(x1, y1, (byte)(s & 0xFF), (byte)(s >> 8));
        }

        public static void ViewerPlayerCloak(byte x1, byte y1, byte s1, byte s2)
        {
            lock (lockListViewer)
            {
                foreach (OneDisplay o in AllViewer)
                {
                    o.SendPlayerCloak(x1, y1, s1, s2);
                }
            }
        }

    }
}
