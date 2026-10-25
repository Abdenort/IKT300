using System;
using System.Collections.Generic;
using System.Text;

namespace PS2000Library
{
    public interface IPS2000
    {
        double GetVoltage();

        string GetSerialNumber();

        string GetDeviceType();

        string GetArticleNumber();

        void RemoteControlOn();

        void RemoteControlOff();

        void PowerOutputOn();

        void PowerOutputOff();

        void SetVoltage(float setVolt);

        public int GetMaxVoltage();


    }
}
