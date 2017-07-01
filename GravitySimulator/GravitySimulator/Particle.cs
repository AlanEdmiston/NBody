using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GravitySimulator
{
    class Particle
    {
        public double mass;
        public DVector3 position;
        public DVector3 velocity;
        public DVector3 accelaration;
        public DVector3 GradV = new DVector3();
        public Vector2 drawVect = new Vector2();
    }
}
