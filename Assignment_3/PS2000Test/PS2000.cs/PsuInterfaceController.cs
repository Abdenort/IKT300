using System;
using System.Collections.Generic;
using System.Text;
using PsuDeviceLibrary;

namespace PsuDeviceLibrary
{
    public interface PsuInterfaceController
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
