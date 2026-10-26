using PsuMqttController;

PsuMqttClient m_Client = new PsuMqttClient("127.0.0.1", "test");

m_Client.MqttConnect();
m_Client.MqttSubscribe();

Console.ReadLine();

m_Client.MqttDisconnect();
