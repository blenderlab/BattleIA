using BattleIA;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace SampleBot
{

    class Program
    {
        private static Settings settings;

        static void Main(string[] args)
        {
            BattleLogger.logger.info("Starting BattleIA");
            var currentDir = Directory.GetCurrentDirectory();
            var configFile = Path.Combine(currentDir, "settings.json");
            // création du fichier settings.json avec les valeurs par défaut
            if (!File.Exists(configFile))
            {
                settings = new Settings();
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(Program.settings, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configFile, json);
            }
            var prm = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(configFile));
            Program.settings = prm;
            settings.BotName = CheckName(settings.BotName);

            BattleLogger.logger.info($"Démarrage du bot: {settings.BotName}");

            DoWork().GetAwaiter().GetResult();
            BattleLogger.logger.info("Bye");
            BattleLogger.logger.info("Press [ENTER] to exit.");
            Console.ReadLine();
        }

        private static MyIA ia = new MyIA();
        private static Bot bot = new Bot();
        private static UInt16 turn = 0;

        private static String CheckName(String n){
            if (n.Length > 0 ){
                return n;
            } 
            List<String> names = new List<String>();
            List<String> surnames = new List<String>();
            var random = new Random();
            names.AddRange(new String[] {"Weirdy","Strange",  "Big","Fat","Small","Thin","Dangerous","Long","Tough","Enormous" });
            surnames.AddRange(new String[] {"Cat","Tiger",  "Mouse","Rat","Beaver","Goose","Puma","Rabbit","Wolf","Fox" });
            int indexn = random.Next(names.Count);
            int indexs = random.Next(surnames.Count);
            BattleLogger.logger.info($"Generating a name : {names[indexn]} {surnames[indexs]} ");

            return names[indexn]+" "+surnames[indexs];
        }

        static async Task DoWork()
        {

            // 1 - connect to server
            var serverUrl = $"ws://{settings.ServerHost}:{settings.ServerPort}/bot";
            var client = new ClientWebSocket();
            BattleLogger.logger.info($"Connecting to {serverUrl}");
            try
            {
                await client.ConnectAsync(new Uri(serverUrl), CancellationToken.None);
            }
            catch (Exception err)
            {
                BattleLogger.logger.error(err.Message);
                return;
            }

            // 2 - Hello message with our GUID

            Guid guid = Guid.NewGuid();
            var bytes = Encoding.UTF8.GetBytes(guid.ToString());
            BattleLogger.logger.info($"Sending our GUID: {guid}");
            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            // 3 - wait data from server

            bool nameIsSent = false;
            bool isDead = false;

            var buffer = new byte[1024 * 4];
            while (!isDead)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    if (result.Count > 0)
                    {
                        byte command = buffer[0];
                        switch ((Message)command)
                        {
                            case Message.m_OK: // OK, rien à faire
                                if (result.Count != (int)MessageSize.OK) { BattleLogger.logger.error($"wrong size for 'OK': {result.Count}"); break; }
                                if (!nameIsSent)
                                {
                                    nameIsSent = true;
                                    // sending our name
                                    var bName = Encoding.UTF8.GetBytes("N" + settings.BotName);
                                    BattleLogger.logger.info($"Sending our name: {settings.BotName}");
                                    await client.SendAsync(new ArraySegment<byte>(bName), WebSocketMessageType.Text, true, CancellationToken.None);
                                    break;
                                }
                                BattleLogger.logger.info("OK, waiting our turn...");
                                break;
                            case Message.m_yourTurn: // nouveau tour, attend le niveau de détection désiré
                                if (result.Count != (int)MessageSize.Turn) { BattleLogger.logger.info($"[ERROR] wrong size for 'T': {result.Count}"); DebugWriteArray(buffer, result.Count); break; }
                                turn = (UInt16)(buffer[1] + (buffer[2] << 8));
                                bot.Energy = (UInt16)(buffer[3] + (buffer[4] << 8));
                                bot.ShieldLevel = (UInt16)(buffer[5] + (buffer[6] << 8));
                                bot.CloakLevel = (UInt16)(buffer[7] + (buffer[8] << 8));
                                BattleLogger.logger.info($"Turn #{turn} - Energy: {bot.Energy}, Shield: {bot.ShieldLevel}, Cloak: {bot.CloakLevel}");
                                ia.StatusReport(turn, bot.Energy, bot.ShieldLevel, false);
                                if (bot.Energy == 0) break;
                                // must answer with D#
                                var answerD = new byte[2];
                                answerD[0] = System.Text.Encoding.ASCII.GetBytes("D")[0];
                                answerD[1] = ia.GetScanSurface();
                                BattleLogger.logger.info($"Sending Scan: {answerD[1]}");
                                await client.SendAsync(new ArraySegment<byte>(answerD), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                            case Message.m_newInfos: // nos infos ont changées
                                if (result.Count != (int)MessageSize.Change)
                                {

                                    BattleLogger.logger.info($"[ERROR] wrong size for 'C': {result.Count}");
                                    DebugWriteArray(buffer, result.Count);
                                    break;
                                }
                                bot.Energy = (UInt16)(buffer[1] + (buffer[2] << 8));
                                bot.ShieldLevel = (UInt16)(buffer[3] + (buffer[4] << 8));
                                bot.CloakLevel = (UInt16)(buffer[5] + (buffer[6] << 8));
                                BattleLogger.logger.info($"Change - Energy: {bot.Energy}, Shield: {bot.ShieldLevel}, Cloak: {bot.CloakLevel}");
                                ia.StatusReport(turn, bot.Energy, bot.ShieldLevel, false);
                                // nothing to reply
                                if (bot.Energy == 0) break;
                                break;
                            case Message.m_mapInfos: // info sur détection, attend l'action à effectuer
                                byte surface = buffer[1];
                                int all = surface * surface;
                                if (result.Count != (2 + all)) { BattleLogger.logger.info($"[ERROR] wrong size for 'I': {result.Count}"); break; } // I#+data so 2 + surface :)
                                var x = new byte[all];
                                Array.Copy(buffer, 2, x, 0, all);
                                ia.AreaInformation(surface, x);
                                // must answer with action Move / Shield / Cloak / Shoot / None
                                var answerA = ia.GetAction(); // (byte)BotAction.None; // System.Text.Encoding.ASCII.GetBytes("N")[0];
                                BattleLogger.logger.info($"Sending Action: {(BotAction)answerA[0]}");
                                await client.SendAsync(new ArraySegment<byte>(answerA), WebSocketMessageType.Text, true, CancellationToken.None);
                                break;
                            case Message.m_dead:
                                isDead = true;
                                BattleLogger.logger.info($"We are dead!");
                                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                                break;
                            case Message.m_Respawn:
                                
                                BattleLogger.logger.info($"We are dead, But We will respawn now !!!");
                                //await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                                break;
                        }
                    } // if count > 1
                    else
                    {
                        BattleLogger.logger.info("[ERROR] " + Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                } // if text
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    BattleLogger.logger.info($"End with code {result.CloseStatus}: {result.CloseStatusDescription}");
                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }
            } // while
            BattleLogger.logger.info("Just dead!");
        } // DoWork

        private static void DebugWriteArray(byte[] data, int length)
        {
            if (length == 0) return;
            Console.Write($"[{data[0]}");
            for (int i = 1; i < length; i++)
            {
                Console.Write($", {data[i]}");
            }
            Console.Write("]");
        }
    }
}
