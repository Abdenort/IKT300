using System.IO.Ports;


namespace PS2000Library
{

    public class PS2000
    {
        public string SerialNumber = "";
        public string DeviceType = "";
        public string ArticleNumber = "";
        public float MaxVoltage = 0;
        public double CurrentVoltage = 0;

        private const string ComPort = "COM14";


        public double GetVoltage()
        {
            double volt = 0;
            int percentVolt = 0;

            //SD = MessageType + CastType + Direction + Length
            int SDHex = (int)0x40 + (int)0x20 + 0x10 + 5;
            byte SD = Convert.ToByte(SDHex.ToString(), 10);

            //SD, DN, OBJ, DATA, CS
            byte[] byteWithOutCheckSum = { SD, (int)0x00, (int)0x47, 0x0, 0x0 };

            int sum = 0;
            int arrayLength = byteWithOutCheckSum.Length;

            for (int i = 0; i < arrayLength; i++)
                {
                    sum += byteWithOutCheckSum[i];
                }

            string hexSum = sum.ToString("X");
            string cs1 = "";
            string cs2 = "";

            if (hexSum.Length == 4)
                {
                    cs1 = hexSum.Substring(0, hexSum.Length / 2);
                    cs2 = hexSum.Substring(hexSum.Length / 2);
                }
            else if (hexSum.Length == 3)
                {
                    cs1 = hexSum.Substring(0, 1);
                    cs2 = hexSum.Substring(1);
                }
            else if ((hexSum.Length is 2) || (hexSum.Length is 1))
                {
                    cs1 = "0";
                    cs2 = hexSum;
                }

            if (cs1 != "")
                {
                    byteWithOutCheckSum[arrayLength - 2] = Convert.ToByte(cs1, 16);
                    byteWithOutCheckSum[arrayLength - 1] = Convert.ToByte(cs2, 16);
                }

            List<byte> responseTelegram;
            using (SerialPort port = 
                new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(byteWithOutCheckSum, 0, byteWithOutCheckSum.Length);
                Thread.Sleep(500);

                responseTelegram = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        //Console.WriteLine(t);
                        responseTelegram.Add(t);
                    }
                }
                port.Close();
                Thread.Sleep(500);
            }
            if (responseTelegram.Count < 7)
            {
                Console.WriteLine("No valid telegram was read");
            }
            else
            {
                string percentVoltString = responseTelegram[5].ToString("X") + responseTelegram[6].ToString("X");
                percentVolt = Convert.ToInt32(percentVoltString, 16);
            }

            float nominalVoltage = 0;

            // get nominal voltage
            List<byte> response;
            byte[] bytesToSend = { 0x74, 0x00, 0x02, 0x00, 0x76 };

            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                port.Write(bytesToSend, 0, bytesToSend.Length);
                Thread.Sleep(50);
                response = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);
                    foreach (var t in message)
                    {
                        response.Add(t);
                    }
                }

                port.Close();
                Thread.Sleep(500);
            }

            if (response.Count < 7)
            {
                    Console.WriteLine("No telegram was read");
                }
            else
                {
                    byte[] byteArray = { response[6], response[5], response[4], response[3] };
                    nominalVoltage = BitConverter.ToSingle(byteArray, 0);
                    MaxVoltage = nominalVoltage;
                    volt = (double)percentVolt * nominalVoltage / 25600;
                    CurrentVoltage = volt;
                    Console.WriteLine(string.Format("Voltage:{0}", volt));
                }
            return volt;
        }

        public string GetSerialNumber()
        {
            // reading serial number
            List<byte> Serialresponse;
            // Remember the dataframe setup, SD, DN,   OBJ, DATA checksum1, checksum2
            // OBJ = 0x01 = 1
            byte[] serialBytesToSend = { 0x7F, 0x00, 0x01, 0x00, 0x80 };
            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(serialBytesToSend, 0, serialBytesToSend.Length);
                Thread.Sleep(500);

                Serialresponse = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                            {
                                //Console.WriteLine(t);
                                Serialresponse.Add(t);
                            }
                    }

                port.Close();

                if (Serialresponse.Count < 3)
                    {
                        Console.WriteLine("No valid serial number response");
                        return "";
                    }

                Thread.Sleep(500);



                string binary = Convert.ToString(Serialresponse[0], 2);
                string payloadLengtBinaryString = binary.Substring(4);
                int payloadLength = Convert.ToInt32(payloadLengtBinaryString, 2);

                string serialNumberString = "";

                if (Serialresponse[2] == 1) // means that I got a response on obj, which refers to the object list.
                    {
                        for (var i = 0; i < payloadLength; i++)
                            {
                                serialNumberString += Convert.ToChar(Serialresponse[3 + i]);
                            }
                    }

                Console.WriteLine(string.Format("serialNumberString:{0}", serialNumberString));

                SerialNumber = serialNumberString;
            }

            return SerialNumber;
        }

        public string GetDeviceType()
                {
                    string manufacturer = "";
                    string model = "";

                    // Get model
                    byte[] modelRequest = { 0x7F, 0x00, 0x00, 0x00, 0x7F };

                    using (SerialPort port =
                           new SerialPort(ComPort, 115200, Parity.None, 8, StopBits.One))
                    {
                        port.Open();

                        port.Write(modelRequest, 0, modelRequest.Length);
                        Thread.Sleep(500);

                        byte[] response = new byte[port.BytesToRead];
                        port.Read(response, 0, response.Length);

                        if (response.Length < 3)
                            {
                                return "";
                            }

                        string binary = Convert.ToString(response[0], 2);


                        string payloadLengthBinary = binary.Substring(4);
                        int payloadLength = Convert.ToInt32(payloadLengthBinary, 2);

                        if (response[2] == 0x00)
                        {
                            for (int i = 0; i < payloadLength; i++)
                            {
                                model += Convert.ToChar(response[3 + i]);
                            }
                        }
                    }

                    // Get manufacturer
                    byte[] manufacturerRequest = { 0x7F, 0x00, 0x08, 0x00, 0x87 };

                    using (SerialPort port =
                           new SerialPort(ComPort, 115200, Parity.None, 8, StopBits.One))
                    {
                        port.Open();

                        port.Write(manufacturerRequest, 0, manufacturerRequest.Length);
                        Thread.Sleep(500);

                        byte[] response = new byte[port.BytesToRead];
                        port.Read(response, 0, response.Length);

                        string binary = Convert.ToString(response[0], 2);

                        if (response.Length < 3)
                        {
                            return "";
                        }

                        string payloadLengthBinary = binary.Substring(4);
                        int payloadLength = Convert.ToInt32(payloadLengthBinary, 2);

                        if (response[2] == 0x08)
                        {
                            for (int i = 0; i < payloadLength; i++)
                            {
                                manufacturer += Convert.ToChar(response[3 + i]);
                            }
                        }
                    }

                    DeviceType = manufacturer + " " + model;

                    return DeviceType;
                }

        public string GetArticleNumber()
        {
            List<byte> Articleresponse;
            byte[] articleBytesToSend = { 0x7F, 0x00, 0x06, 0x00, 0x85 };
            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();
                // write to the USB port
                port.Write(articleBytesToSend, 0, articleBytesToSend.Length);
                Thread.Sleep(500);

                Articleresponse = new List<byte>();
                int length = port.BytesToRead;
                if (length > 0)
                    {
                        byte[] message = new byte[length];
                        port.Read(message, 0, length);
                        foreach (var t in message)
                            {
                                //Console.WriteLine(t);
                                Articleresponse.Add(t);
                            }
                    }

                port.Close();
                Thread.Sleep(500);

                if (Articleresponse.Count < 3)
                {
                    Console.WriteLine("No valid article number response");
                    return "";
                }

                string binary = Convert.ToString(Articleresponse[0], 2);
                string payloadLengtBinaryString = binary.Substring(4);
                int payloadLength = Convert.ToInt32(payloadLengtBinaryString, 2);

                string articleNumberString = "";

                if (Articleresponse[2] == 0x06)
                {
                    for (var i = 0; i < payloadLength; i++)
                        {
                            articleNumberString += Convert.ToChar(Articleresponse[3 + i]);
                        }
                }

                Console.WriteLine(string.Format("articalNumberString:{0}", articleNumberString));

                ArticleNumber = articleNumberString;
            }

            return ArticleNumber;
        }

        public void RemoteControlOn()
        {
            byte[] bytesToSendToTurnOnRC = { 0xF1, 0x00, 0x36, 0x10, 0x10, 0x01, 0x47 };

            List<byte> RCresponse;

            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);

                port.Open();

                port.Write(
                    bytesToSendToTurnOnRC,
                    0,
                    bytesToSendToTurnOnRC.Length
                );

                Thread.Sleep(50);

                RCresponse = new List<byte>();

                int length = port.BytesToRead;

                if (length > 0)
                    {
                        byte[] message = new byte[length];

                        port.Read(message, 0, length);

                        foreach (var t in message)
                            {
                                RCresponse.Add(t);
                            }
                    }

                port.Close();

                Thread.Sleep(500);
            }

            if (RCresponse[3] == 0)
                {
                    Console.WriteLine("Remote Control is turned on");
                }
            else
                {
                    Console.WriteLine(
                        $"Remote control is not turned on due to error: {RCresponse[3]}"
                    );
                }
        }

        public void RemoteControlOff()
        {
            byte[] bytesToSendToTurnOffRC = { 0xF1, 0x00, 0x36, 0x10, 0x00, 0x01, 0x37 };

            List<byte> RCresponse;

            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);

                port.Open();

                port.Write(
                    bytesToSendToTurnOffRC,
                    0,
                    bytesToSendToTurnOffRC.Length
                );

                Thread.Sleep(50);

                RCresponse = new List<byte>();

                int length = port.BytesToRead;

                if (length > 0)
                {
                    byte[] message = new byte[length];

                    port.Read(message, 0, length);

                    foreach (var t in message)
                    {
                        RCresponse.Add(t);
                    }
                }

                port.Close();

                Thread.Sleep(500);
            }

            if (RCresponse[3] == 0)
                {
                    Console.WriteLine("Remote Control is turned off");
                }
            else
            {
                Console.WriteLine(
                    $"Remote control is not turned off due to error: {RCresponse[3]}"
                );
            }
        }

        public void PowerOutputOn()
        {
            byte[] bytesToSendToTurnOnPower = { 0xF1, 0x00, 0x36, 0x01, 0x01, 0x01, 0x29 };

            List<byte> response;

            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();

                port.Write(
                    bytesToSendToTurnOnPower,
                    0,
                    bytesToSendToTurnOnPower.Length
                );

                Thread.Sleep(50);

                response = new List<byte>();

                int length = port.BytesToRead;

                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);

                    foreach (var t in message)
                        {
                            response.Add(t);
                        }
                }

                port.Close();
                Thread.Sleep(500);
            }

            if (response[3] == 0)
            {
                Console.WriteLine("Power output is turned on");
            }
            else
            {
                Console.WriteLine(
                    $"Power output was not turned on due to error: {response[3]}"
                );
            }
        }

        public void PowerOutputOff()
        {
            byte[] bytesToSendToTurnOffPower = { 0xF1, 0x00, 0x36, 0x01, 0x00, 0x01, 0x28 };

            List<byte> response;

            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);
                port.Open();

                port.Write(
                    bytesToSendToTurnOffPower,
                    0,
                    bytesToSendToTurnOffPower.Length
                );

                Thread.Sleep(50);

                response = new List<byte>();

                int length = port.BytesToRead;

                if (length > 0)
                {
                    byte[] message = new byte[length];
                    port.Read(message, 0, length);

                    foreach (var t in message)
                        {
                            response.Add(t);
                        }
                }

                port.Close();
                Thread.Sleep(500);
            }

            if (response[3] == 0)
                {
                    Console.WriteLine("Power output is turned off");
                }
            else
                {
                    Console.WriteLine($"Power output was not turned off due to error: {response[3]}");
                }
        }

        public  void SetVoltage(float setVolt)
        {
            if (setVolt < 0 || setVolt > MaxVoltage)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(setVolt),
                    $"Voltage must be between 0 and {MaxVoltage} V"
                );
            }

            int percentSetValue = (int)Math.Round((25600 * setVolt) / MaxVoltage);

            byte highByte = (byte)((percentSetValue >> 8) & 0xFF);

            byte lowByte = (byte)(percentSetValue & 0xFF);

            byte[] bytesToSend = { 0xF2, 0x00, 0x32, highByte, lowByte, 0x00, 0x00 };

            int sum = 0;

            for (int i = 0; i < bytesToSend.Length - 2; i++)
                {
                    sum += bytesToSend[i];
                }

            bytesToSend[^2] = (byte)((sum >> 8) & 0xFF);
            bytesToSend[^1] = (byte)(sum & 0xFF);

            List<byte> response = new List<byte>();

            using (SerialPort port = new SerialPort(ComPort, 115200, 0, 8, StopBits.One))
            {
                Thread.Sleep(500);

                port.Open();

                port.Write(
                    bytesToSend,
                    0,
                    bytesToSend.Length
                );

                Thread.Sleep(500);

                int length = port.BytesToRead;

                if (length > 0)
                {
                    byte[] message = new byte[length];

                    port.Read(message, 0, length);

                    foreach (byte b in message)
                    {
                        response.Add(b);
                    }
                }
            }

            if (response.Count > 3 && response[3] == 0)
                {
                    Console.WriteLine($"Voltage set to {setVolt} V");
                }
            else
                {
                    Console.WriteLine("Voltage could not be set");
                }
        }
    }

}
