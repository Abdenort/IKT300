using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MqttCommunicationLibrary
{
    public class MqttConnection
    {
        // Fields
        private MqttClient m_Client;
        private string clientID;


        // Constructor 
        public MqttConnection(string connectionString)
        {
            m_Client = new MqttClient(connectionString);
            clientID = Guid.NewGuid().ToString();

            m_Client.MqttMsgPublishReceived += M_Client_MqttMsgPublishReceived;
        }

        // Connect 
        public void MqttConnect()
        {
            m_Client.Connect(clientID);
        }

        // Disconnect
        public void MqttDisconnect()
        {
            if (m_Client.IsConnected)
            {
                m_Client.Disconnect();
            }
        }

        public void MqttSubscribe(string topic)
        {
            if (m_Client.IsConnected)
            {
                m_Client.Subscribe(
                    new string[] { topic },
                    new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }
                    );
            }

        }

        // Publish
        public void MqttPublish(string topic, string message)
        {
            m_Client.Publish(
                topic,
                Encoding.UTF8.GetBytes(message),
                MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
                false
                );
        }

        // Event
        public event Action<string, string>? MessageReceived;

        // Event handler
        private void M_Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e) 
        {
            string message = Encoding.UTF8.GetString(e.Message);

            MessageReceived?.Invoke(e.Topic, message);
        }

    }
}
