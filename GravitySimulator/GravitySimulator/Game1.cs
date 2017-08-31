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
        //TODO: thread stuff
        int height;

        List<Particle> particles = new List<Particle>();
        double timeStep = new double();
        double timeMax = new double();
        double time = new double();
        double ThickSurfaceApprox = 1;  //approximkate the surface of a particle to be a thick shell of this for use with contact forces
        double GravConst = 20;
        Vector3D distance = new Vector3D();
        double distanceStep = new double();
        double restitution = 1;

        bool InstantCollisionApproximation = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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
            // TODO: Add your initialization logic here

            Random rand = new Random();
            double maxSize = new double();
            double maxV = new double();
            double particleRadius = 10;
            maxSize = 300;
            timeMax = 500;
            timeStep = 0.1;
            distanceStep = 1;
            maxV = 0;
            int particleNumber = new int();
            particleNumber = 2;
            double particleMass = new double();
            particleMass = 1;
            for (int i = 0; i < particleNumber; i++)
            {
                Particle particle = new Particle();
                particle.position = new Vector3D();
                particle.velocity = new Vector3D();
                particle.mass = particleMass;
                particle.radius = particleRadius;
                particle.position.x = rand.Next(100, (int)maxSize + 100);
                particle.position.y = rand.Next(100, (int)maxSize + 100);
                particle.position.z = 0;// rand.Next(100, (int)maxSize + 100);

                particle.velocity.x = 0;// rand.Next(-(int)maxV, (int)maxV);
                particle.velocity.y = 0;// rand.Next(-(int)maxV, (int)maxV);
                particle.velocity.z = 0;// rand.Next(-(int)maxV, (int)maxV);

                particles.Add(particle);
            }
            foreach(Particle particle in particles)
            {
                particle.Hamiltonian = Math.Pow(particle.velocity.Mod, 2) + PotentialField(particle.position);
            }

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

            foreach (Particle particle in particles)
            {
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
                particle.accelaration = -1 * particle.GradV / particle.mass;
                particle.velocity += timeStep * particle.accelaration;
                particle.position += timeStep * particle.velocity;

                foreach (Particle particle2 in particles)
                {
                    if(particle != particle2 && (particle2.position - particle.position).Mod <= particle.radius + particle2.radius && Vector3D.DotProduct(particle.position - particle2.position, particle.velocity - particle2.velocity) / (particle.position - particle2.position).Mod < 0)
                    {
                        collision(particle, particle2);
                    }
                }
                //TODO: test for fast particles tunnelling through each other
                //TODO: contact forces
                //TODO: dissipative forces
                //TODO: add computationally quick code for stable multi-particle objects/ bluk behaviour approximations and annalytical solutions
            }

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
                if (dist > 2 * distanceStep)
                {
                    potential -= particle.mass * GravConst / dist;
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

            particle1.Hamiltonian = Math.Pow(particle1.velocity.Mod, 2) + PotentialField(particle1.position);
            particle2.Hamiltonian = Math.Pow(particle2.velocity.Mod, 2) + PotentialField(particle2.position);
        }
    }
}
