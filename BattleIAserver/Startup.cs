﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net.WebSockets;

namespace BattleIAserver
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) { }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

/*
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
                  
                    }); 
*/
        }
    }
}
