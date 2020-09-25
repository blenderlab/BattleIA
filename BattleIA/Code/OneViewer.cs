using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BattleIA
{
    public class OneViewer
    {
        private WebSocket webSocket = null;
        public Guid ClientGuid { get; }
        public bool MustRemove = false;

        public OneViewer(WebSocket webSocket)
        {
            this.webSocket = webSocket;
            ClientGuid = Guid.NewGuid();
        }

        public async Task WaitReceive()
        {
            // 1 - on attend la première data du client
            // qui doit etre son GUID

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;
            try
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (Exception err)
            {
                MustRemove = true;
                System.Diagnostics.Debug.WriteLine($"[VIEWER ERROR] {err.Message}");
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "[VIEWER] Error waiting data", CancellationToken.None);
                }
                catch (Exception) { }
                return;
            }
            while (!result.CloseStatus.HasValue)
            {
                if (result.Count < 1)
                {
                    MustRemove = true;
                    await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "[VIEWER] Missing data in answer", CancellationToken.None);
                    return;
                }

                string command = System.Text.Encoding.UTF8.GetString(buffer, 0, 1);
                System.Diagnostics.Debug.WriteLine($"[VIEWER] Received command '{command}'");
                if (command != "M")
                {
                    MustRemove = true;
                    await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, $"[VIEWER ERROR] Not the right answer, waiting M#, receive {command}", CancellationToken.None);
                    return;
                }
                /*if (result.Count < 1)
                {
                    MustRemove = true;
                    await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Missing data in answer 'D'", CancellationToken.None);
                    return;
                }*/
                await SendMapInfo();

                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch (Exception err)
                {
                    MustRemove = true;
                    System.Diagnostics.Debug.WriteLine($"[VIEWER ERROR] {err.Message}");
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "[VIEWER] Error waiting data", CancellationToken.None);
                    }
                    catch (Exception) { }
                    return;
                }
            }
            MustRemove = true;
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public async Task SendMapInfo()
        {
            var buffer = new byte[5 + MainGame.MapWidth * MainGame.MapHeight];
            buffer[0] = System.Text.Encoding.ASCII.GetBytes("M")[0];
            buffer[1] = (byte)MainGame.MapWidth;
            buffer[2] = (byte)(MainGame.MapWidth >> 8);
            buffer[3] = (byte)MainGame.MapHeight;
            buffer[4] = (byte)(MainGame.MapHeight >> 8);
            int index = 5;
            for (int j = 0; j < MainGame.MapHeight; j++)
                for (int i = 0; i < MainGame.MapWidth; i++)
                    buffer[index++] = (byte)MainGame.TheMap[i, j];
            /*foreach(OneClient oc in MainGame.AllBot)
            {
                buffer[5 + oc.bot.X +( oc.bot.Y * MainGame.MapWidth)] = (byte)CaseState.Ennemy;
            }*/
            try
            {
                System.Diagnostics.Debug.WriteLine("[VIEWER] Sending MAPINFO");
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine($"[VIEWER ERROR] {err.Message}");
                MustRemove = true;
            }
        }


        public async Task SendMovePlayer(byte x1, byte y1, byte x2, byte y2)
        {
            var buffer = new byte[5];
            buffer[0] = System.Text.Encoding.ASCII.GetBytes("P")[0];
            buffer[1] = x1;
            buffer[2] = y1;
            buffer[3] = x2;
            buffer[4] = y2;
            try
            {
                System.Diagnostics.Debug.WriteLine("[VIEWER] Sending move player");
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine($"[VIEWER ERROR] {err.Message}");
                MustRemove = true;
            }
        }

    }
}
