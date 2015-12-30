using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace アルティメットバトルオージー
{
    class ParticleEngine
    {
        struct BloodColumn
        {
            public float TargetHeight;
            public float Height;
            public float Speed;

            public void Update(float dampening, float tension)
            {
                float x = TargetHeight - Height;
                Speed += tension * x - Speed * dampening;
                Height += Speed / 100;
            }
        }

        public float targetHeight = 0f;

        public PrimitiveBatch pb;
        BloodColumn[] columns = new BloodColumn[400];
        static Random rand = new Random();

        public float Tension = 2f;
        public float Dampening = 1f;
        public float Spread = 0.25f;

        RenderTarget2D metaballTarget, particlesTarget;
        SpriteBatch spriteBatch;
        AlphaTestEffect alphaTest;
        Texture2D particleTexture;

        private float Scale { get { return 2560 / (columns.Length - 1f); } }

        List<Particle> particles = new List<Particle>();

        class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Orientation;

            public Particle(Vector2 position, Vector2 velocity, float orientation)
            {
                Position = position;
                Velocity = velocity;
                Orientation = orientation;
            }
        }

        public ParticleEngine(GraphicsDevice device, Texture2D particleTexture, float h)
        {


            targetHeight = h;
            pb = new PrimitiveBatch(device);
            this.particleTexture = particleTexture;
            spriteBatch = new SpriteBatch(device);
            metaballTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height);
            particlesTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height);
            alphaTest = new AlphaTestEffect(device);
            alphaTest.ReferenceAlpha = 175;

            var view = device.Viewport;

            alphaTest.Projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) *
                Matrix.CreateOrthographicOffCenter(0, view.Width, view.Height, 0, 0, 1);

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new BloodColumn()
                {
                    Height = targetHeight,
                    TargetHeight = targetHeight,
                    Speed = 0
                };
            }


        }

        // Returns the height of the water at a given x coordinate.
        public float GetHeight(float x)
        {
            if (x < 0 || x > 2560)
                return targetHeight;

            return columns[(int)(x / Scale)].Height;
        }

        void UpdateParticle(Particle particle)
        {
            const float Gravity = 3f;
            particle.Velocity.Y += Gravity;
            particle.Position += particle.Velocity;
            particle.Orientation = GetAngle(particle.Velocity);
        }

        public void Splash(float xPosition, float speed)
        {
            int index = (int)MathHelper.Clamp(xPosition / Scale, 0, columns.Length - 1);
            
            for (int i = Math.Max(0, index - 0); i < Math.Min(columns.Length - 1, index + 1); i++)
                columns[index].Speed = speed;

            CreateSplashParticles(xPosition, speed);
        }

        private void CreateSplashParticles(float xPosition, float speed)
        {
            float y = GetHeight(xPosition);

            if (speed > 120)
            {
                for (int i = 0; i < 1; i++)
                {
                    Vector2 pos = new Vector2(xPosition, y) + GetRandomVector2(20);
                    Vector2 vel = FromPolar(MathHelper.ToRadians(GetRandomFloat(-100, -5)), GetRandomFloat(0, 0.5f * (float)Math.Sqrt(speed)));

                    //vel.Normalize();
                    //Vector2 newVel = vel  * 2;

                    CreateParticle(pos, vel);
                }
            }
        }

        private void CreateParticle(Vector2 pos, Vector2 velocity)
        {
            particles.Add(new Particle(pos, velocity, 0));
        }

        private Vector2 FromPolar(float angle, float magnitude)
        {
            return magnitude * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private float GetRandomFloat(float min, float max)
        {
            return (float)rand.NextDouble() * (max - min) + min;
        }

        private Vector2 GetRandomVector2(float maxLength)
        {
            return FromPolar(GetRandomFloat(-MathHelper.Pi, MathHelper.Pi), GetRandomFloat(0, maxLength));
        }

        private float GetAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        public void Update()
        {
            for (int i = 0; i < columns.Length; i++)
                columns[i].Update(Dampening, Tension);

            float[] lDeltas = new float[columns.Length];
            float[] rDeltas = new float[columns.Length];

            // do some passes where columns pull on their neighbours
            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (i > 0)
                    {
                        lDeltas[i] = Spread * (columns[i].Height - columns[i - 1].Height);
                        columns[i - 1].Speed += lDeltas[i];
                    }
                    if (i < columns.Length - 1)
                    {
                        rDeltas[i] = Spread * (columns[i].Height - columns[i + 1].Height);
                        columns[i + 1].Speed += rDeltas[i];
                    }
                }

                for (int i = 0; i < columns.Length; i++)
                {
                    if (i > 0)
                        columns[i - 1].Height += lDeltas[i];
                    if (i < columns.Length - 1)
                        columns[i + 1].Height += rDeltas[i];
                }
            }

            foreach (var particle in particles)
                UpdateParticle(particle);

            particles = particles.Where(x => x.Position.X >= 0 && x.Position.X <= 2560 && x.Position.Y - 5 <= GetHeight(x.Position.X)).ToList();
        }

        public void DrawToRenderTargets(SpriteBatch sb)
        {
            GraphicsDevice device = spriteBatch.GraphicsDevice;
            device.SetRenderTarget(metaballTarget);
            device.Clear(Color.Transparent);

            // draw particles to the metaball render target
            spriteBatch.Begin(0, BlendState.Additive);
            foreach (var particle in particles)
            {
                Vector2 origin = new Vector2(particleTexture.Width, particleTexture.Height) / 2f;
                sb.Draw(particleTexture, particle.Position, null, Color.White, particle.Orientation, origin, 2f, 0, 0);
                //spriteBatch.Draw(particleTexture, particle.Position, null, Color.White, particle.Orientation, origin, 2f, 0, 0);
            }
            spriteBatch.End();

            // draw a gradient above the water so the metaballs will fuse with the water's surface.
            pb.Begin(PrimitiveType.TriangleList);

            const float thickness = 5;
            float scale = Scale;
            for (int i = 1; i < columns.Length; i++)
            {
                Vector2 p1 = new Vector2((i - 1) * scale, columns[i - 1].Height);
                Vector2 p2 = new Vector2(i * scale, columns[i].Height);
                Vector2 p3 = new Vector2(p1.X, p1.Y - thickness);
                Vector2 p4 = new Vector2(p2.X, p2.Y - thickness);

                pb.AddVertex(p2, Color.Red);
                pb.AddVertex(p1, Color.Red);
                pb.AddVertex(p3, Color.Transparent);

                pb.AddVertex(p3, Color.Transparent);
                pb.AddVertex(p4, Color.Transparent);
                pb.AddVertex(p2, Color.Red);
            }

            pb.End();

            // save the results in another render target (in particlesTarget)
            device.SetRenderTarget(particlesTarget);
            device.Clear(Color.Transparent);
            spriteBatch.Begin(0, null, null, null, null, alphaTest);
            spriteBatch.Draw(metaballTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            // switch back to drawing to the backbuffer.
            device.SetRenderTarget(null);
        }

        public void Draw()
        {
            Color waveColor = Color.DarkRed;

            // draw the particles 3 times to create a bevelling effect
            spriteBatch.Begin();
            spriteBatch.Draw(particlesTarget, -Vector2.One, new Color(0.8f, 0.8f, 1f));
            spriteBatch.Draw(particlesTarget, Vector2.One, new Color(0f, 0f, 0.2f));
            spriteBatch.Draw(particlesTarget, Vector2.Zero, waveColor);
            spriteBatch.End();

            // draw the waves
            pb.Begin(PrimitiveType.TriangleList);
            Color poolColor = Color.DarkRed;
            //waveColor *= 0.8f;

            float bottom = targetHeight;

            float scale = Scale;
            for (int i = 1; i < columns.Length; i++)
            {
                Vector2 p1 = new Vector2((i - 1) * scale, columns[i - 1].Height);
                Vector2 p2 = new Vector2(i * scale, columns[i].Height);
                Vector2 p3 = new Vector2(p2.X, bottom);
                Vector2 p4 = new Vector2(p1.X, bottom);

                pb.AddVertex(p1, waveColor);
                pb.AddVertex(p2, waveColor);
                pb.AddVertex(p3, poolColor);

                pb.AddVertex(p1, waveColor);
                pb.AddVertex(p3, poolColor);
                pb.AddVertex(p4, poolColor);
            }

            pb.End();

        }

        #region

        private Random random;
        public Vector2 EmitterLocation { get; set; }
        public List<MetaParticle> metaParticles;
        private List<Texture2D> textures;
        public int numberOfParticles { get; set; }

        public ParticleEngine(List<Texture2D> textures)
        {
            numberOfParticles = 0;
            EmitterLocation = Vector2.Zero;
            this.textures = textures;
            this.metaParticles = new List<MetaParticle>();
            random = new Random();
        }

        public void UpdateMeta()
        {

            for (int particle = 0; particle < metaParticles.Count; particle++)
            {
                metaParticles[particle].Update();
                if (metaParticles[particle].lifeTime <= 0)
                {
                    metaParticles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void GenerateNewParticle(Vector2 location, int numParticles)
        {
            this.EmitterLocation = location;
            numberOfParticles = numParticles;

            for (int i = 0; i < numberOfParticles; i++)
            {
                Texture2D texture = textures[random.Next(textures.Count)];
                Vector2 position = EmitterLocation;
                Vector2 velocity = new Vector2(0f, 9.8f);
                float angle = (float)(3 * Math.PI / 2);
                float angularVelocity = 0f;
                Color color = new Color(
                            (float)random.NextDouble(),
                            (float)random.NextDouble(),
                            (float)random.NextDouble());
                float size = (float)random.NextDouble();
                int lifeTime = 100;

                metaParticles.Add(new MetaParticle(texture, position, velocity, angle, angularVelocity, color, size, lifeTime));
            }
        }

        public void DrawSingle(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < metaParticles.Count; index++)
            {
                metaParticles[index].Draw(spriteBatch);
            }
        }
        #endregion
    }
}
