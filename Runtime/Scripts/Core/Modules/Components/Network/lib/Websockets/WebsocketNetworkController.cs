using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace core.modules.networking
{
    /// <summary>
    /// Network Controller for the websockets lib
    /// </summary>

    public class websocket_sv : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            // Client connected to host!
            // Add a timer of 6 seconds to remove unidentified client
            // GameManager.Instance.NetworkManager.NotReadyClients.Add(ID, 6f);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            // Send message received to other clients
            /* if (e.IsBinary)
                Send(e.RawData); */
        }

        public void SendMessage(byte[] _rawData)
        {
            Send(_rawData);
        }
    }

    public class WebsocketNetworkController : NetworkController
    {
        private WebSocketServer m_socketServer;
        private WebSocket m_socketClient;
        private Action<byte[]> m_serviceSend;

        public override void InitAsClient(string ipAddr, int port)
        {
            m_socketClient = new WebSocket ("ws://" + ipAddr + ":" + port + "/websocket_sv");

            m_socketClient.OnOpen += (sender, e) => onOpen();
            m_socketClient.OnMessage += (sender, e) => onMessage(e.RawData);
            m_socketClient.OnError  += (sender, e) => onError(e.Message);
            m_socketClient.OnClose += (sender, e) => onClose(e.Reason);

            m_socketClient.ConnectAsync();
            
            // Register self as an isolated client 
            UpdateClientSelf();
        }

        public override void InitAsHost(string ipAddr, int port)
        {
            Debug.Log("# Starting server on " + ipAddr);

            // Reset Network Client Controller
            ResetClientController();

            // Create the server
            m_socketServer = new WebSocketServer("ws://" + ipAddr + ":" + port);

            // Disable auto disconnection system - kept disabled for now but can introduce problems in the future
            //m_socketServer.KeepClean = false;
            
            //Prepare services and boot up
            m_socketServer.AddWebSocketService<websocket_sv>("/websocket_sv", sv => m_serviceSend = sv.SendMessage);
            m_socketServer.Start();

            Debug.Log("# SERVER STARTED!");

            // Register self as an isolated client 
            UpdateClientSelf();
        }

        public override void SendMessage(NetworkMessage message)
        {
            // Cast network message into binary data
            if(m_serviceSend != null)
                m_serviceSend.Invoke(ParseDataIntoBinary(message));
        }

        ~WebsocketNetworkController()
        {
            //Worst case scenario, garbage collector will kill the discarted host.
            if (m_socketServer != null && m_socketServer.IsListening)
                m_socketServer.Stop();

            if(m_socketClient != null && m_socketClient.IsConnected)
                m_socketClient.CloseAsync(CloseStatusCode.Abnormal, "Network controller instance destroyed!");
        }
    }
}