using PsuDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace PsuDeviceLibrary
{
    internal class DummyPsu : PsuInterfaceController
    {
        private double voltage = 2.232;

        public string GetArticleNumber()
        {
            return "12415124";
        }

        public string GetDeviceType()
        {
            return "DummyType";
        }

        public string GetSerialNumber()
        {
            return "fun";
        }

        public double GetVoltage()
        {
            return voltage;
        }

        public void PowerOutputOff()
        {
            
        }

        public void PowerOutputOn()
        {
            
        }

        public void RemoteControlOff()
        {
           
        }

        public void RemoteControlOn()
        {
            
        }

        public void SetVoltage(float setVolt)
        {
            if(setVolt < 0 || setVolt > GetMaxVoltage())
                throw new ArgumentOutOfRangeException(nameof(setVolt));

            voltage = setVolt;
        }

        public int GetMaxVoltage()
        {
            return 15;
        }
    }
}
