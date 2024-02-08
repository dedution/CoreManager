using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.modules.networking
{
    public class NetworkClientController
    {
        private List<NetworkClient> m_ClientList = new List<NetworkClient>();

        // Received by client from a host when client connects
        public void onClientPopulate(object _rawdata)
        {
            // Inject clients on list
        }

        // Received on client connect
        public void onClientConnected(object _rawdata)
        {
            // Register client on list
        }

        // Received on client disconnect
        public void onClientDisconnected(object _rawdata)
        {
            // Unregister client on list
        }

        public void Reset()
        {

        }
    }
}