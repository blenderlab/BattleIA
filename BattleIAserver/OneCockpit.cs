using BattleIA;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BattleIAserver
{
    public class OneCockpit
    {
        public BotState State { get; private set; } = BotState.Undefined;
        private WebSocket webSocket = null;
        public Guid ClientGuid;
        public bool MustRemove = false;

        public OneCockpit(WebSocket webSocket)
        {
            this.webSocket = webSocket;
            ClientGuid = Guid.NewGuid();
            State = BotState.WaitingGUID;
        }

        public async Task WaitReceive()
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;
            try
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (Exception err)
            {
                Console.WriteLine($"[COCKPIT ERROR] {err.Message}");
                State = BotState.Disconnect;
                MustRemove = true;
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Error waiting data", CancellationToken.None);
                }
                catch (Exception) { }
                return;
            }

            while (!result.CloseStatus.HasValue)
            {

                // 1 - réception du GUID

                if (State == BotState.WaitingGUID)
                {
                    if (result.Count != 38 && result.Count != 36 && result.Count != 32) // pas de GUID ?
                    {
                        MustRemove = true;
                        State = BotState.Disconnect;
                        if (result.Count > 0)
                        {
                            var temp = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                            Console.WriteLine($"[COCKPIT ERROR GUID] {temp}");
                        }
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "No GUID", CancellationToken.None);
                        return;
                    }
                    var text = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // check que l'on a reçu un GUID
                    if (Guid.TryParse(text, out ClientGuid))
                    {
                        Console.WriteLine($"[COCKPIT] {ClientGuid}");
                        State = BotState.Ready;
                        await SendMessage("OK");
                        MainGame.SendMapInfoToCockpit(ClientGuid);
                    }
                    else
                    {
                        MustRemove = true;
                        State = BotState.Disconnect;
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, $"[{text}] is not a GUID", CancellationToken.None);
                        return;
                    }
                } // réception GUID
                else
                {

                }

                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch (Exception err)
                {
                    Console.WriteLine($"[COCKPIT ERROR] {err.Message}");
                    MustRemove = true;
                    State = BotState.Disconnect;
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Error waiting data", CancellationToken.None);
                    }
                    catch (Exception) { }
                    return;
                }


            } // while

            MustRemove = true;
            State = BotState.Disconnect;
            try
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            catch (Exception) { }


        } // WaitReceive()

        public async Task SendMessage(String text)
        {
            if (MustRemove) return;
            System.Diagnostics.Debug.WriteLine($"Sending {text} to {ClientGuid}");
            var buffer = System.Text.Encoding.UTF8.GetBytes(text);
            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception err)
            {
                Console.WriteLine($"[COCKPIT ERROR] {err.Message}");
                State = BotState.Error;
            }
        }

        public async Task SendInfo(ArraySegment<byte> buffer)
        {
            await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
        }


    }
}
