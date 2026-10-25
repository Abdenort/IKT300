using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PS2000Library
{
    public static class PS2000Factory
    {
        public static IPS2000 Create(int psuType)
        {
            IPS2000 psu = null;

            switch (psuType)
            {
                case 0:
                    psu = new DummyPsu();
                    break;
                case 1:
                    psu = new PS2000();
                    break;
                default:
                    // code block
                    break;
            }
            return psu;
        }

    }
}
