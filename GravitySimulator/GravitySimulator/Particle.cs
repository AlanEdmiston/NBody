using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using AE.AdvancedMaths;

namespace GravitySimulator
{
    class Particle
    {
        //public bool HasCollided;
        public double mass, radius, restitustionCoeff, frictionCeoff, adhesion, stiffness, heat;
        public double Hamiltonian;
        public double energy
        {
            get
            {
                return heat + Hamiltonian * mass;
            }
        }
        public Vector3D position = new Vector3D();
        public Vector3D velocity = new Vector3D();
        public Vector3D accelaration = new Vector3D();
        public Vector3D GradV = new Vector3D();
        public Vector2 drawVect = new Vector2();
    }
}
