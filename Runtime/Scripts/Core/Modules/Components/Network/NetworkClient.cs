using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace core.modules.networking
{
    public enum NetworkClientStates
    {}
    
    public class NetworkClient
    {
        private string m_networkName = "undefined";
        private int m_networkID = -1;

        public NetworkClient()
        {
            // Init
        }

        public string GetNetworkName()
        {
            return m_networkName;
        }

        public int GetNetworkID()
        {
            return m_networkID;
        }

        public NetworkClient(int networkID, string networkName)
        {
            Update(networkID, networkName);
        }

        public void Update(int networkID, string networkName = "undefined")
        {
            m_networkName = networkName;
            m_networkID = networkID;
        }
    }
}