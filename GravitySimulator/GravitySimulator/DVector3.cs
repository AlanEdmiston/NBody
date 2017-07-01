using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravitySimulator
{
    class DVector3
    {
        public double x;
        public double y;
        public double z;
        public double mod
        {
            get
            {
                return Math.Sqrt(x * x + y * y + z * z);
            }
        }
    }
}
