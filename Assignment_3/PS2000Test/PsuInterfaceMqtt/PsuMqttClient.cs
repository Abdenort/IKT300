using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace PsuMqttController
{
    internal class PsuMqttClient
    {
        // Fields.
        private MqttClient m_Client;
        private string clientID;
        private string m_PSUID = "";

        // Constructor.
        public PsuMqttClient(string connectionString, string psuId)
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
            
            Console.WriteLine("Connected to MQTT broker.");
        }

        // Event handler
        private void M_Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string receivedMsg = Encoding.UTF8.GetString(e.Message);
            Console.WriteLine(receivedMsg);
        }

        // Subscribe
        public void MqttSubscribe()
        {
            if (m_Client.IsConnected && !string.IsNullOrWhiteSpace(m_PSUID))
            { 
                string topic = string.Format("/PSU/PSU2000/{0}/#", m_PSUID);
                m_Client.Subscribe(new string[] { topic }, new byte[] { 2 });
            }
            
        }

        // Publish message
        public void MqttPublish(string topic, string message)
        {
            string fullTopic = string.Format("/PSU/PSU2000/{0}/{1}", m_PSUID, topic);
            m_Client.Publish(fullTopic, Encoding.UTF8.GetBytes(message),
                MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        // Disconnect
        public void MqttDisconnect()
        {
            if(m_Client.IsConnected) 
                {
                    m_Client.Disconnect();
                }
        }
    }
}