using PsuMqttController;
using PsuDeviceLibrary;


PsuInterfaceController psu = PsuControllerFactory.Create(0);
PsuMqttClient m_Client = new PsuMqttClient("127.0.0.1", "test");

m_Client.MqttConnect();
m_Client.MqttSubscribe();
m_Client.MqttPublish("voltage", "20");

Console.WriteLine(psu.GetArticleNumber());
Console.WriteLine(psu.GetVoltage());
Console.ReadLine();

m_Client.MqttDisconnect();
