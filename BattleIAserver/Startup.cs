using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BattleIAserver
{

    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();

            app.UseWebSockets();
       

            // ICI on fonctionne en THREAD !
            app.Use(async (context, next) =>
            {
                Console.WriteLine("New WebSocket connection");
                // ouverture d'une websocket, un nouveau bot se connecte
                if (context.Request.Path == "/bot")
                {
                    Console.WriteLine("WebSocket /bot");
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        //Console.WriteLine("AcceptWebSocketAsync");
                        // on l'ajoute à notre simulation !
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        //await Echo(context, webSocket);
                        Console.WriteLine("A new BOT in arena!");
                        // Démarrage d'un nouveau bot. Si on revient c'est qu'il est mort !
                        await MainGame.AddBot(webSocket);
                        Console.WriteLine($"#BOTS: {MainGame.AllBot.Count}");
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("WebSocket ERROR : Not a WebSocket establishment request.");
                    }
                }
                if (context.Request.Path == "/display")
                {
                    Console.WriteLine("[SOCKET] WebSocket /display");
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        // on l'ajoute à notre simulation !
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        Console.WriteLine("[DISPLAY] New DISPLAY!");
                        await MainGame.AddViewer(webSocket);
                        Console.WriteLine($"[DISPLAY] number= {MainGame.AllViewer.Count}");
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("WebSocket ERROR : Not a WebSocket establishment request.");
                    }
                }
                if (context.Request.Path == "/cockpit")
                {
                    Console.WriteLine("WebSocket /cockpit");
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        // on l'ajoute à notre simulation !
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        Console.WriteLine("New COCKPIT!");
                        await MainGame.AddCockpit(webSocket);
                        Console.WriteLine($"#COCKPIT: {MainGame.AllCockpit.Count}");
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("WebSocket ERROR : Not a WebSocket establishment request.");
                    }
                }

                if (context.Request.Path == "/startsim")
                {
                    Console.WriteLine("WebSocket /startsim");
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        MainGame.RunSimulator();
                        Console.WriteLine($"[SIM]: Start simulation");
                        context.Response.StatusCode = 200;

                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("WebSocket ERROR : Not a WebSocket establishment request.");
                    }
                }

                if (context.Request.Path == "/stopsim")
                {
                    Console.WriteLine("WebSocket /stopsim");
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        MainGame.StopSimulator();
                        Console.WriteLine($"[SIM]: Stop simulation");
                        context.Response.StatusCode = 200;
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("WebSocket ERROR : Not a WebSocket establishment request.");
                    }
                }
                if (context.Request.Path == "/statsim")
                {
                    Console.WriteLine("WebSocket /statsim");
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        MainGame.StopSimulator();
                        Console.WriteLine($"[SIM]: Ask server status ");
                        context.Response.StatusCode = 200;
                        var buffer = new byte[4];
                        buffer[0] = MainGame.isRunning();
                        buffer[1] = (byte)MainGame.AllBot.Count;
                        buffer[2] = (byte)MainGame.TheMap.Length;
                        buffer[3] = (byte)MainGame.TheMap.Length;
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
                        await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);                         
                        Console.WriteLine($"[SIM]: Status sent ! ");

                    }
                    
                    else
                    {
                        context.Response.StatusCode = 400;
                        Console.WriteLine("WebSocket ERROR : Not a WebSocket establishment request.");
                    }
                }

                  
            }); 

        }
    }
}
