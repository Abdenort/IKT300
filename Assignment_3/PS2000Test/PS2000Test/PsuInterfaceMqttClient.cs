using System;
using System.Collections.Generic;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace PsuControlApp
{
    internal class PsuInterfaceMqttClient
    {
        // Fields
        private MqttClient m_Client;
        private string clientID;
        private string m_PSUID;

        // Construtor
        public PsuInterfaceMqttClient(string connectionString, string psuId)
        {
            m_Client = new MqttClient(connectionString);
            clientID = Guid.NewGuid().ToString();
            m_PSUID = psuId;
        }

        // Connect 
        public void MqttConnect()
        {
            m_Client.MqttMsgPublishReceived += M_Client_MqttMsgPublishReceived;
            m_Client.Connect(clientID);
        }

        // Event
        public event Action<string, string>? MessageReceived;

        private void M_Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string statusPrefix = $"/PSU/PSU2000/{m_PSUID}/status/";

            if (!e.Topic.StartsWith(statusPrefix))
                return;

            string status = e.Topic.Substring(statusPrefix.Length);
            string message = Encoding.UTF8.GetString(e.Message);

            MessageReceived?.Invoke(status, message);
        }

        // Subscribe
        public void MqttSubscribe()
        {
            if (m_Client.IsConnected)
            {
                string topic = $"/PSU/PSU2000/{m_PSUID}/status/#";

                m_Client.Subscribe(
                    new string[] { topic },
                    new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }
                    );
            }

        }

        public void MqttPublish(string command, string message)
        {
            string topic = $"/PSU/PSU2000/{m_PSUID}/command/{command}";

            m_Client.Publish(
                topic,
                Encoding.UTF8.GetBytes(message),
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                false
                );
        }

        public void MqttDisconnect()
        {
            if (m_Client.IsConnected)
            {
                m_Client.Disconnect();
            }
        }
    }
}
