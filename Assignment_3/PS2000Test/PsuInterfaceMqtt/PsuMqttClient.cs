using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using PsuDeviceLibrary;

namespace PsuMqttController
{
    internal class PsuMqttClient
    {
        // Fields.
        private MqttClient m_Client;
        private string clientID;
        private string m_PSUID = "";

        private PsuInterfaceController? psu;

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
            string commandPrefix = $"/PSU/PSU2000/{m_PSUID}/command/";

            // Ignore messages that are not commands
            if (!e.Topic.StartsWith(commandPrefix))
                return;

            string command = e.Topic.Substring(commandPrefix.Length);
            string receivedMsg = Encoding.UTF8.GetString(e.Message);

            Console.WriteLine($"Command: {command}");
            Console.WriteLine($"Message: {receivedMsg}");

            try
            {
                switch (command)
                {
                    case "select":
                        if (int.TryParse(receivedMsg, out int psuType))
                        {
                            SelectPsu(psuType);
                            Console.WriteLine($"Selected PSU: {psu?.GetDeviceType()}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid PSU selection.");
                        }
                        break;

                    case "getVoltage":
                        if (psu == null)
                        {
                            Console.WriteLine("No PSU selected.");
                            break;
                        }

                        double voltage = psu.GetVoltage();

                        MqttPublish(
                            "status/voltage",
                            voltage.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            );

                        Console.WriteLine($"Voltage: {voltage} V");
                        break;

                    case "setVoltage":
                        if (psu == null)
                        {
                            Console.WriteLine("No PSU selected.");
                            break;
                        }

                        if(!float.TryParse(
                            receivedMsg,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out float setVoltage))
                        {
                            Console.WriteLine("Invalid voltage value.");
                            break;
                        }

                        psu.SetVoltage(setVoltage);

                        MqttPublish(
                            "status/voltage",
                            psu.GetVoltage().ToString(System.Globalization.CultureInfo.InvariantCulture)
                            );

                        Console.WriteLine($"Voltage set to: {setVoltage} V.");
                        break;

                    case "powerOn":
                        if (psu == null)
                            break;

                        psu.PowerOutputOn();
                        Console.WriteLine("Power output ON.");
                        break;

                    case "powerOff":
                        if (psu == null)
                            break;
                        
                        psu.PowerOutputOff();
                        Console.WriteLine("Power output OFF.");
                        break;

                    case "remoteOn":
                        if (psu == null)
                            break;

                        psu.RemoteControlOn();
                        Console.WriteLine("Remote control ON.");
                        break;

                    case "remoteOff":
                        if (psu == null)
                            break;

                        psu.RemoteControlOff();
                        Console.WriteLine("Remote control OFF.");
                        break;

                    case "getInfo":
                        if(psu == null)
                        {
                            Console.WriteLine("No PSU selected.");
                            break;
                        }

                        MqttPublish("status/deviceType", psu.GetDeviceType());
                        MqttPublish("status/serialNumber", psu.GetSerialNumber());
                        MqttPublish("status/articleNumber", psu.GetArticleNumber());
                        MqttPublish("status/maxVoltage", psu.GetMaxVoltage().ToString());
                        MqttPublish("status/voltage", 
                            psu.GetVoltage().ToString(System.Globalization.CultureInfo.InvariantCulture));

                        Console.WriteLine("PSU information published.");
                        break;

                    default:
                        Console.WriteLine($"Unkown command: {command}");
                        break;
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Command failed: {ex.Message}");
            }
        }

        // Subscribe
        public void MqttSubscribe()
        {
            if (m_Client.IsConnected && !string.IsNullOrWhiteSpace(m_PSUID))
            { 
                string topic = string.Format("/PSU/PSU2000/{0}/command/#", m_PSUID);
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

        // Select PSU
        public void SelectPsu(int psuType)
        {
            if (psuType != 0 && psuType != 1)
            {
                throw new ArgumentException("Invalid PSU Type.");
            }

            psu = PsuControllerFactory.Create(psuType);
        }
    }
}