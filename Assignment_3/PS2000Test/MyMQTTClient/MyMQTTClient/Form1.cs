using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
namespace MyMQTTClient
{
    public partial class Form1 : Form
    {
        MqttClient m_Client;
        string clientID;
        string m_PSUID;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string connectionString = txtConnectionString.Text;
            m_Client = new MqttClient(connectionString);
            clientID = Guid.NewGuid().ToString();

            m_Client.MqttMsgPublishReceived += M_Client_MqttMsgPublishReceived;

            m_Client.Connect(clientID);
        }

        private void M_Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string receivedMsg = e.Topic.ToString();
            this.Invoke((MethodInvoker)delegate () { SetText(receivedMsg); });
        }

        private void SetText(string text)
        {
            this.txtSubscription.Text = text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_Client != null)
            {
                m_Client.Disconnect();
            }
        }

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            if (m_Client != null)
            {
                if (txtSubscibe.Text != "")
                {
                    m_PSUID = txtSubscibe.Text;
                    string topic = string.Format("/PSU/PSU2000/{0}/#", m_PSUID);
                    m_Client.Subscribe(new string[] { topic }, new byte[] { 2 });
                }
            }
        }

        private void btnPublish_Click(object sender, EventArgs e)
        {
            string topic = string.Format("/PSU/PSU2000/{0}/{1}", m_PSUID, txtPubText.Text);
            m_Client.Publish(topic, Encoding.UTF8.GetBytes(topic), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        }
    }
}