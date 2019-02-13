using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace BreakernoidsGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D bgTexture;
        Paddle paddle;
        Ball ball;
        int hack = 0;
        List<Block> blocks = new List<Block>();
        List<PowerUp> powerUps = new List<PowerUp>();



        int[,] blockLayout = new int[,]
        {
            {5,5,5,5,5,5,5,5,5,5,5,5,5,5,5},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
            {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
            {4,4,4,4,4,4,4,4,5,5,5,5,5,5,5},
        };

        SoundEffect bounceSound, hitSound, deathSound, powerSound;

        Random random = new Random();
        Double prob = 0.2;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
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

            // TODO: use this.Content to load your game content here
            bgTexture = Content.Load<Texture2D>("bg");

            paddle = new Paddle(this);
            paddle.LoadContent();
            paddle.position = new Vector2(512, 740);

            ball = new Ball(this);
            ball.LoadContent();
            ball.position = new Vector2(512, paddle.position.Y - ball.Height - paddle.Height);

            for (int i = 0; i < blockLayout.GetLength(1); i++)
            {
                for (int j = 0; j < blockLayout.GetLength(0); j++)
                {
                    Block tempBlock = new Block(this, (BlockColor)blockLayout[j,i]);
                    tempBlock.LoadContent();
                    tempBlock.position = new Vector2(64 + i * 64, 100 + j * 32);
                    blocks.Add(tempBlock);
                }
            }

            bounceSound = Content.Load<SoundEffect>("ball_bounce");
            hitSound = Content.Load<SoundEffect>("ball_hit");
            deathSound = Content.Load<SoundEffect>("death");
            powerSound = Content.Load<SoundEffect>("powerup");


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            paddle.Update(deltaTime);
            ball.Update(deltaTime);

            if (hack > 0)
            {
                hack -= 1;
            }

            foreach (PowerUp p in powerUps)
            {
                p.Update(deltaTime);
            }


            CheckCollisions();
            CheckForPowerUps();
            DestroyPowerUp();
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);


            // TODO: Add your drawing code here
            spriteBatch.Begin();

            //Draw all sprites here
            spriteBatch.Draw(bgTexture, new Vector2(0, 0), Color.White);
            paddle.Draw(spriteBatch);
            ball.Draw(spriteBatch);

            foreach (Block b in blocks)
            {
                b.Draw(spriteBatch);
            }

            foreach (PowerUp p in powerUps)
            {
                p.Draw(spriteBatch);
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }

        protected void CheckCollisions()
        {
            float radius = ball.Width / 2;

            if (hack == 0 && (ball.position.X > (paddle.position.X - radius - paddle.Width / 2)) &&
(ball.position.X < (paddle.position.X + radius + paddle.Width / 2)) &&
(ball.position.Y < paddle.position.Y) &&
(ball.position.Y > (paddle.position.Y - radius - paddle.Height / 2)))
            {
                // Reflect based on which part of the paddle is hit

                // By default, set the normal to "up"
                Vector2 normal = -1.0f * Vector2.UnitY;

                // Distance from the leftmost to rightmost part of the paddle
                float dist = paddle.Width + (radius * 2);
                // Where within this distance the ball is at
                float ballLocation = ball.position.X -
                    (paddle.position.X - radius - paddle.Width / 2);
                // Percent between leftmost and rightmost part of paddle
                float pct = ballLocation / dist;

                if (pct < 0.33f)
                {
                    normal = new Vector2(-0.196f, -0.981f);
                }
                else if (pct > 0.66f)
                {
                    normal = new Vector2(0.196f, -0.981f);
                }

                ball.direction = Vector2.Reflect(ball.direction, normal);
                hack = 20;
                bounceSound.Play();
            }

            Block collidedBlock = null;
            foreach (Block b in blocks)
            {
                if ((ball.position.X > (b.position.X - b.Width / 2 - radius)) &&
                    (ball.position.X < (b.position.X + b.Width / 2 + radius)) &&
                    (ball.position.Y > (b.position.Y - b.Height / 2 - radius)) &&
                    (ball.position.Y < (b.position.Y + b.Height / 2 + radius)))
                {
                    collidedBlock = b;
                    break;
                }
            }

            if (collidedBlock != null)
            {
                // Assume that if our Y is close to the top or bottom of the block,
                // we're colliding with the top or bottom
                if ((ball.position.Y <
                    (collidedBlock.position.Y - collidedBlock.Height / 2)) ||
                    (ball.position.Y >
                    (collidedBlock.position.Y + collidedBlock.Height / 2)))
                {
                    ball.direction.Y = -1.0f * ball.direction.Y;
                }
                else // otherwise, we have to be colliding from the sides
                {
                    ball.direction.X = -1.0f * ball.direction.X;
                }

                hitSound.Play();

                // Now remove this block from the list
                if (collidedBlock.OnHit(collidedBlock) == true)
                {
                    blocks.Remove(collidedBlock);
                    if (random.NextDouble() < prob)
                    {
                        SpawnPowerUp(collidedBlock.position);
                    }
                }
                //blocks.Remove(collidedBlock);
            }

            if (Math.Abs(ball.position.X - 32) < radius)
            {
                // Left wall collision
                ball.direction.X = -ball.direction.X;
                bounceSound.Play();
            }
            else if (Math.Abs(ball.position.X - 992) < radius)
            {
                // Right wall collision
                ball.direction.X = -ball.direction.X;
                bounceSound.Play();
            }
            else if (Math.Abs(ball.position.Y - 32) < radius)
            {
                // Top wall collision
                ball.direction.Y = -ball.direction.Y;
                bounceSound.Play();
            }

            LoseLife();
            
        }

        void LoseLife()
        {
            float radius = ball.Width / 2;
            if (Math.Abs(ball.position.Y) > 768 + radius)
            {
                ball.position = new Vector2(512, paddle.position.Y - ball.Height - paddle.Height);
                ball.direction = new Vector2(0.707f, -0.707f);
                paddle.position = new Vector2(512, 740);
                deathSound.Play();
            }
        }

        void SpawnPowerUp(Vector2 position)
        {
            PowerUpType powerChoice = (PowerUpType)random.Next(3);
            PowerUp tempPowerUp;

            tempPowerUp = new PowerUp(this, powerChoice);
            tempPowerUp.LoadContent();
            tempPowerUp.position = position;
            powerUps.Add(tempPowerUp);


        }

        void DestroyPowerUp()
        {
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                if (powerUps[i].destroy == true)
                {
                    powerUps.RemoveAt(i);
                }
            }
        }

        void CheckForPowerUps()
        {
            Rectangle padTangle = paddle.BoundingRect;
            Rectangle tempTangle;

            foreach (PowerUp p in powerUps)
            {
                tempTangle = p.BoundingRect;
                if (tempTangle.Intersects(padTangle))
                {
                    ActivatePowerUp(p);
                }
            }
        }

        void ActivatePowerUp(PowerUp whichPowerUp)
        {
            whichPowerUp.destroy = true;
            powerSound.Play();
        }

        
    }
}
