using PsuDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PsuDeviceLibrary
{
    public static class PsuControllerFactory
    {
        public static PsuInterfaceController Create(int psuType)
        {
            PsuInterfaceController psu = null;

            switch (psuType)
            {
                case 0:
                    psu = new DummyPsu();
                    break;
                case 1:
                    psu = new PhysicalPsuController();
                    break;
                default:
                    // code block
                    break;
            }
            return psu;
        }

    }
}
