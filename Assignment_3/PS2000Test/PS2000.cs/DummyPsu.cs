using PsuDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace PsuDeviceLibrary
{
    internal class DummyPsu : PsuInterfaceController
    {

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
            return 2.232;
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
            
        }

        public int GetMaxVoltage()
        {
            return 15;
        }
    }
}
