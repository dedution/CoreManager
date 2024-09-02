using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using core.modules.networking;

namespace core.modules
{
    public enum NetworkTypes
    {
        Client,
        Server
    }

    public enum NetworkStatus
    {
        Idle,
        Connected,
        Connecting,
        Failed,
        Disconnected
    }

    public class NetworkManager : BaseModule
    {
        private NetworkClient m_selfClient;
        private NetworkTypes m_currentNetworkMode = NetworkTypes.Client;
        private NetworkClientController m_networkClientController = new NetworkClientController();
        public NetworkStatus ConnectionStatus
        {
            get; set;
        }
        public NetworkTypes NetworkMode
        {
            get
            {
                return m_currentNetworkMode;
            }

            private set { }
        }
        private delegate void listenerDelegate(object _rawdata);
        private listenerDelegate m_networkListener;
        private Queue<UnityAction> m_networkQueue = new Queue<UnityAction>();
        private NetworkController m_networkController;
        
        public override void onInitialize()
        {
            m_selfClient = new NetworkClient();

            // Instantiate the network controller that communicates with the desired protocol, in this case, websockets
            // m_networkController = new WebsocketNetworkController();
        }

        public void Connect(string ipaddr)
        {
            Debug.Log("Connecting to host at: " + ipaddr);

            m_currentNetworkMode = NetworkTypes.Client;
            ConnectionStatus = NetworkStatus.Connected;
            
            // Get a comms id from the network controller
            m_selfClient.Update(0);
        }

        public void NewServer()
        {
            Debug.Log("Initialized as host!");

            m_currentNetworkMode = NetworkTypes.Server;
            ConnectionStatus = NetworkStatus.Connected;

            // Get a comms id from the network controller
            m_selfClient.Update(0);
        }

        public void Disconnect()
        {
            Debug.Log("System disconnected.");
            Reset();
            ConnectionStatus = NetworkStatus.Disconnected;
        }

        private void ProcessNetworkData(object rawdata)
        {
            m_networkQueue.Enqueue(() => m_networkListener(rawdata));
        }

        public override void UpdateModule(float deltaTime, float unscaledDeltaTime)
        {
            //Execute Actions from the Network in the Main Thread
            if (m_networkQueue.Count > 0)
                m_networkQueue.Dequeue().Invoke();
        }

        public void Reset()
        {
            m_selfClient.Update(-1);
            m_networkClientController.Reset();
        }

        // TODO
        // Handling of UDP and websockets communication
        // Messaging system implementation
        // Replace rawdata type from object to a seperate class and make it serializable for json conversions if necessary 
    }
}