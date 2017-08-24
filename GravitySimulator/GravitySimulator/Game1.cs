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

        int height;

        List<Particle> particles = new List<Particle>();
        double timeStep = new double();
        double timeMax = new double();
        double time = new double();
        double GravConst = 20;
        Vector3D distance = new Vector3D();
        double distanceStep = new double();
        double restitution = 1;

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
            particleNumber = 200;
            double particleMass = new double();
            particleMass = 1;
            for (int i = 0; i < particleNumber; i++)
            {
                Particle particle = new Particle();
                particle.position = new Vector3D();
                particle.velocity = new Vector3D();
                particle.mass = particleMass;
                particle.position.x = rand.Next(10, (int)maxSize);
                particle.position.y = rand.Next(10, (int)maxSize);
                particle.position.z = rand.Next(10, (int)maxSize);

                particle.velocity.x = rand.Next(-(int)maxV, (int)maxV);
                particle.velocity.y = rand.Next(-(int)maxV, (int)maxV);
                particle.velocity.z = rand.Next(-(int)maxV, (int)maxV);

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
                Vector3D delta = new Vector3D();
                double potential = PotentialField(particle.position);
                delta.x = distanceStep;
                delta.y = 0;
                delta.z = 0;
                //hard code conservation of energy into incrimental acceleration of particles
                particle.GradV.x = (PotentialField(particle.position + delta) - potential) / distanceStep;
                delta.x = 0;
                delta.y = distanceStep;
                particle.GradV.y = PotentialField(particle.position + delta) - potential / distanceStep;
                delta.y = 0;
                delta.z = distanceStep;
                particle.GradV.z = PotentialField(particle.position + delta) - potential / distanceStep;
                particle.accelaration = particle.GradV * -particle.mass * timeStep;
                particle.velocity = particle.velocity + particle.accelaration;
                particle.position = particle.position + particle.velocity * timeStep;

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

        //calculates the gravitiational potential of any given point in space
        double PotentialField(Vector3D position)
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
            Vector3D relativeVelocity = particle1.velocity - particle2.velocity;
            Vector3D positionUnitVector = particle1.position - particle2.position;
            positionUnitVector = positionUnitVector / Vector3D.Mod(positionUnitVector);
            //seperate velocity into radial and tangential components
            double radial, radial1, radial2;
            Vector3D tangential;
            radial = DotProduct(relativeVelocity, positionUnitVector);
            tangential = CrossProduct(relativeVelocity, positionUnitVector);
            if (radial > 0)//check for colliding multiple times
            {
                //use conservation of momentum and coefficient of restitution to determine velocity changes
                //m1v1+m2v2=m1v1'+m2v2'
                //e(v1-v2)=v2'-v1'
                radial1 = DotProduct(particle1.velocity, positionUnitVector);   //v1
                radial2 = -DotProduct(particle2.velocity, positionUnitVector);  //v2

                radial1 += (particle2.mass / particle1.mass) * radial2;
                radial2 = radial1 * (restitution + 1) - radial2 * (particle2.mass / particle1.mass + restitution);  //v2'
                radial1 -= (particle2.mass / particle1.mass) * radial2;  //v1'
            }
        }
    }
}
