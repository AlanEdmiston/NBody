using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AE.AdvancedMaths;

namespace GravitySimulator
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D particleSprite;
        bool superPosition, hasRefreshed;

        Random rand = new Random();

        //TODO: thread stuff

        List<Particle> particles = new List<Particle>();
        double timeStep = new double();
        double timeMax = new double();
        double time = new double();
        double ThickSurfaceApprox = 1;  //approximkate the surface of a particle to be a thick shell of this for use with contact forces
        double GravConst = 20;
        Vector3D distance = new Vector3D();
        Vector3D deltaA = new Vector3D();
        double distanceStep = new double();
        double restitution = 0.8;
        double totalEnergy;
        Vector3D totalMomentum;

        SpriteFont font;
        Vector2 fontVector = new Vector2 { X = 600, Y = 100 };
        string energyString;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            
            double maxSize = new double();
            double maxV = new double();
            double particleRadius = 10;
            maxSize = 100;
            timeMax = 500;
            timeStep = 0.1;
            distanceStep = 0.1;
            maxV = 0;
            int particleNumber = 100;
            double particleMass = new double();
            particleMass = 1;

            Vector3D clump1Velocity = new Vector3D { x = 20, y = 0, z = 0 };
            Vector3D clump1AngularP = new Vector3D();
            Vector3D clump1Position = new Vector3D { x = 0, y = 200, z = 0 };

            Vector3D clump2Velocity = new Vector3D { x = -20, y = 5, z = 0 };
            Vector3D clump2AngularP = new Vector3D();
            Vector3D clump2Position = new Vector3D { x = 700, y = 200, z = 0 };

            ClumpCreator(300, 10, 200, clump1Position, 1, clump1Velocity, clump1AngularP);
            ClumpCreator(300, 10, 200, clump2Position, 1, clump2Velocity, clump2AngularP);

            // TODO: add different initialization set ups rubble piles, star mass/radius distributions etc. Perhaps add initialization project

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            particleSprite = this.Content.Load<Texture2D>("particle");
            font = Content.Load<SpriteFont>("SpriteFont1");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            totalEnergy = 0;
            totalMomentum = new Vector3D { x = 0, y = 0, z = 0 };

            foreach (object particle in particles)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ParticleComputer), particle);
            }

            foreach(Particle particle in particles)
            {
                particle.position += timeStep * particle.velocity - Math.Pow(timeStep, 2) * particle.GradV / 2;
            }

            totalMomentum.x = Math.Round(totalMomentum.x, 2);
            totalMomentum.y = Math.Round(totalMomentum.y, 2);
            totalMomentum.z = Math.Round(totalMomentum.z, 2);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            foreach (Particle particle in particles)
            {
                spriteBatch.Draw(particleSprite, particle.drawVect, Color.White);
            }

            spriteBatch.DrawString(font, totalMomentum.ToString(), fontVector, Color.Red);
            hasRefreshed = true;
            spriteBatch.End();

            base.Draw(gameTime);
        }

        //calculates the gravitiational potential of any given point in space
        public double PotentialField(Vector3D position)
        {
            double potential = new double();
            double dist;
            foreach (Particle particle in particles)
            {
                dist = (particle.position - position).Mod;
                if (dist > particle.radius)
                {
                    potential -= GravConst / dist;
                }
            }
            return potential;
        }

        //calculates the change in velocity of two particles due to a collision with each other
        void collision(Particle particle1, Particle particle2)
        {
            double v1, v2, v1New, v2New;

            Vector3D radial = (particle1.position - particle2.position).UnitVector;

            v1 = Vector3D.DotProduct(particle1.velocity, radial);
            v2 = Vector3D.DotProduct(particle2.velocity, radial);

            v2New = (restitution * (v1 - v2) + v1 + v2 * particle2.mass / particle1.mass) / (1 + particle2.mass / particle1.mass);
            v1New = restitution * (v2 - v1) + v2New;

            particle1.velocity += (v1New - v1) * radial;
            particle2.velocity += (v2New - v2) * radial;

            particle1.Hamiltonian = Math.Pow(particle1.velocity.Mod, 2) / 2 + PotentialField(particle1.position);
            particle2.Hamiltonian = Math.Pow(particle2.velocity.Mod, 2) / 2 + PotentialField(particle2.position);
        }
        void ClumpCreator(int particleNumber, double particleRadius, int regionSize, Vector3D position, double particleMass, Vector3D velocity, Vector3D angularMomentum)
        {
            for (int i = 0; i < particleNumber; i++)
            {
                Particle particle = new Particle();
                particle.position = new Vector3D();
                particle.velocity = new Vector3D();
                particle.mass = particleMass;
                particle.radius = particleRadius;
                do
                {
                    superPosition = false;
                    particle.position.x = rand.Next((int)position.x, (int)(regionSize + position.x));
                    particle.position.y = rand.Next((int)position.y, (int)(regionSize + position.y));
                    particle.position.z = rand.Next((int)position.z, (int)(regionSize + position.z));
                    foreach (Particle otherParticle in particles)
                    {
                        if ((otherParticle.position - particle.position).Mod < particle.radius + otherParticle.radius)
                        {
                            superPosition = true;
                            break;
                        }
                    }
                }
                while (superPosition == true);

                particle.velocity = velocity; ;
                particles.Add(particle);
            }
            foreach (Particle particle in particles)
            {
                particle.Hamiltonian = Math.Pow(particle.velocity.Mod, 2) + PotentialField(particle.position);
            }
        }

        void ParticleComputer(object oparticle)
        {
            Particle particle = (Particle)oparticle;
            particle.drawVect = new Vector2((float)particle.position.x, (float)particle.position.y);
            //Differentiate the gravitational potential field to find the force acting on the particle
            Vector3D delta = new Vector3D();
            double potential = PotentialField(particle.position);
            delta.x = distanceStep;
            delta.y = 0;
            delta.z = 0;
            //TODO: hard code conservation of energy into incrimental acceleration of particles
            particle.GradV.x = (PotentialField(particle.position + delta) - PotentialField(particle.position - delta)) / (distanceStep * 2);
            delta.x = 0;
            delta.y = distanceStep;
            particle.GradV.y = (PotentialField(particle.position + delta) - PotentialField(particle.position - delta)) / (distanceStep * 2);
            delta.y = 0;
            delta.z = distanceStep;
            particle.GradV.z = (PotentialField(particle.position + delta) - PotentialField(particle.position - delta)) / (distanceStep * 2);
            particle.accelaration = -1 * particle.GradV;
            //set change in kinetic energy to -change in potential energy to conserve hamiltonian

            //particle.position += Math.Pow(timeStep, 3) / 6 * deltaA;
            particle.velocity += (particle.accelaration - timeStep * particle.GradV) / 2;
            superPosition = false;
            foreach (Particle particle2 in particles)
            {
                if (particle != particle2 && (particle2.position - particle.position).Mod <= particle.radius + particle2.radius)
                {
                    if (Vector3D.DotProduct(particle.position - particle2.position, particle.velocity - particle2.velocity) / (particle.position - particle2.position).Mod < 0)
                    {
                        collision(particle, particle2);
                    }
                }
            }

            totalEnergy += particle.energy;
            totalMomentum += particle.mass * particle.velocity;
            //TODO: test for fast particles tunnelling through each other
            //TODO: contact forces
            //TODO: dissipative forces
            //TODO: add computationally quick code for stable multi-particle objects/ bluk behaviour approximations and annalytical solutions
        }
    }
}
