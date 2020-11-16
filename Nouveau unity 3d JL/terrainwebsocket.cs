/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using BattleIA;


public class terrainwebsocket : MonoBehaviour
{
    Uri u = new Uri("ws://127.0.0.1:4226/display"); //attention il faut modif le 2626
    ClientWebSocket cws = null;
    //ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
    
    void Start()
    {
        Connect();
    }

    void Update()
    {
        
    }
    async void Connect()
    {
        cws = new ClientWebSocket();
        try
        {
            await cws.ConnectAsync(u, CancellationToken.None);
            if (cws.State == WebSocketState.Open) Debug.Log("connected");
        }
        catch (Exception e) { Debug.Log("woe " + e.Message); }
    }
}*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class terrainwebsocket : MonoBehaviour
{
    WebSocket websocket;
    // ici 2 veriables 
    public static int height;
    public static int width;
    public static bool isConnect;
    public static bool receivedmapinfo;
    public static int bx1, by1, bx2, by2, cx1,cy1,ex1,ey1;
// Start is called before the first frame update
async void Start()
    {
        isConnect = false;
        receivedmapinfo = false;
        websocket = new WebSocket("ws://127.0.0.1:4626/display");//url

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            isConnect = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            isConnect = false;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            isConnect = false;
        };

        websocket.OnMessage += (bytes) =>
        {

            //Debug.Log("OnMessage!");
            //Debug.Log(bytes);

            
            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            //Debug.Log("OnMessage! " + message);
            if (bytes[0]== System.Text.Encoding.ASCII.GetBytes("M")[0])
            {
              //  Debug.Log("OnMessage! " + message);
                receivedmapinfo = true;
                height = bytes[3];
                width = bytes[1];
                Debug.Log(height);
                Debug.Log(width);
            }
           if(bytes[0]== System.Text.Encoding.ASCII.GetBytes("E")[0])
           {
                //Debug.Log("OnMessage! " + message);
                //Debug.Log("energie");
                ex1 = bytes[1];
                ey1 = bytes[2];
            }
           if(bytes[0] == System.Text.Encoding.ASCII.GetBytes("P")[0])
            {

                bx1 = bytes[1];
                by1 = bytes[2];
                bx2 = bytes[3];
                by2 = bytes[4];
                
            }
            if (bytes[0] == System.Text.Encoding.ASCII.GetBytes("C")[0])
            {
                cx1 = bytes[1];
                cy1 = bytes[2];

                 
            }
        };



        // waiting for messages
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending bytes
            await websocket.Send(new byte[] { 10, 20, 30 });

            // Sending plain text
            await websocket.SendText("plain text message");
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

}

