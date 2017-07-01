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

        int height;

        List<Particle> particles = new List<Particle>();
        double timeStep = new double();
        double timeMax = new double();
        double time = new double();
        double GravConst = 20;
        DVector3 distance = new DVector3();
        double distanceStep = new double();

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
            maxSize = 500;
            timeMax = 500;
            timeStep = 1;
            distanceStep = 1;
            maxV = 2;
            int particleNumber = new int();
            particleNumber = 2;
            double particleMass = new double();
            particleMass = 1;
            for (int i = 0; i < particleNumber; i++)
            {
                Particle particle = new Particle();
                particle.position = new DVector3();
                particle.velocity = new DVector3();
                particle.mass = particleMass;
                /*particle.position.x = (double)rand.Next(10, (int)maxSize);
                particle.position.y = (double)rand.Next(10, (int)maxSize);
                particle.position.z = (double)rand.Next(10, (int)maxSize);

                particle.velocity.x = (double)rand.Next(0, (int)maxV);
                particle.velocity.y = (double)rand.Next(0, (int)maxV);
                particle.velocity.z = (double)rand.Next(0, (int)maxV);*/

                if(i == 0)
                {
                    particle.position.x = 350;
                    particle.position.y = 350;
                }
                else
                {
                    particle.position.x = 350;
                    particle.position.y = 300;

                    particle.velocity.x = 1;
                }
                particles.Add(particle);
            }
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
            // TODO: Unload any non ContentManager content here
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
                DVector3 delta = new DVector3();
                double potential = PotentialField(particle.position);
                delta.x = distanceStep;
                delta.y = 0;
                delta.z = 0;
                //hard code conservation of energy into incrimental acceleration of particles
                particle.GradV.x = (PotentialField(VectAdd(particle.position, delta)) - potential) / distanceStep;
                delta.x = 0;
                delta.y = distanceStep;
                particle.GradV.y = PotentialField(VectAdd(particle.position, delta)) - potential / distanceStep;
                delta.y = 0;
                delta.z = distanceStep;
                particle.GradV.z = PotentialField(VectAdd(particle.position, delta)) - potential / distanceStep;
                particle.accelaration = VectMult(particle.GradV, -particle.mass * timeStep);
                particle.velocity = VectAdd(particle.velocity, particle.accelaration);
                particle.position = VectAdd(particle.position, VectMult(particle.velocity, timeStep));

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
            foreach (Particle particle in particles)
            {

            }
            spriteBatch.Begin();
            foreach (Particle particle in particles)
            {
                spriteBatch.Draw(particleSprite, particle.drawVect, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        //methods used to simplify basic vector arithmitic
        DVector3 VectAdd(DVector3 vect1, DVector3 vect2)
        {
            DVector3 outVect = new DVector3();
            outVect.x = vect1.x + vect2.x;
            outVect.y = vect1.y + vect2.y;
            outVect.z = vect1.z + vect2.z;
            return outVect;
        }

        DVector3 VectMult(DVector3 vect, double multiplier)
        {
            DVector3 outVect = new DVector3();
            outVect.x = vect.x * multiplier;
            outVect.y = vect.y * multiplier;
            outVect.z = vect.z * multiplier;
            return outVect;
        }

        //calculates the gravitiational potential of any given point in space
        double PotentialField(DVector3 position)
        {
            double potential = new double();
            double dist;
            foreach (Particle particle in particles)
            {
                dist = VectAdd(particle.position, VectMult(position, -1)).mod;
                if (dist > 2 * distanceStep)
                {
                    potential -= particle.mass * GravConst / dist;
                }
            }
            return potential;
        }
    }
}
