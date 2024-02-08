using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;


namespace core.modules.networking
{
    /// <summary>
    /// Serves as a base controller of the network protocol logic
    /// Ex: Websockets, UDP, TCP
    /// </summary>
    [System.Serializable]
    public class NetworkMessage
    {
        private int m_clientID = -1;        
        private Dictionary<string, object> m_networkData = new Dictionary<string, object>();

        public NetworkMessage(NetworkClient m_messageOwner) 
        {
            m_clientID = m_messageOwner.GetNetworkID();
        }


    }

    public class NetworkController
    {
        public NetworkController()
        {
            // Register listeners
        }

        protected void onOpen()
        {
            // Chew and parse binary data back into usable state
        }

        protected void onMessage(byte[] rawData)
        {
            // Cast to NetworkMessage and get information about the message
            // Chew and parse binary data back into usable state
            // Return message back
            ParseBinaryDataIntoMessage(rawData);
        }
        protected void onError(string errorMsg)
        {
            // Chew and parse binary data back into usable state
        }

        protected void onClose(string reason)
        {
            // Chew and parse binary data back into usable state
        }

        public virtual void SendMessage(NetworkMessage message)
        {
            // Cast network message into binary data
            // ParseDataIntoBinary(message);
        }

        protected byte[] ParseDataIntoBinary(NetworkMessage message)
        {
            //Cast network message into binary data
            MemoryStream memorystream = new MemoryStream(); 
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memorystream, message);
            return memorystream.ToArray();
        }

        protected NetworkMessage ParseBinaryDataIntoMessage(byte[] rawData)
        {
            //Cast binary data into message
            MemoryStream memorystreamd = new MemoryStream(rawData);
            BinaryFormatter bfd = new BinaryFormatter();
            NetworkMessage deserializedMessage = bfd.Deserialize(memorystreamd) as NetworkMessage;

            return deserializedMessage;
        }

        public virtual void InitAsClient(string ipAddr, int port)
        {}

        public virtual void InitAsHost(string ipAddr, int port)
        {}

        protected void ResetClientController()
        {

        }

        protected void UpdateClientSelf()
        {
            
        }
    }
}