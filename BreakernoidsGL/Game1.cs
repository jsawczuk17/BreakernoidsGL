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
using System.IO;
using System.Xml.Serialization;

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
        List<Ball> balls = new List<Ball>();
        Level level;
        String nextLevel;

        /*int[,] blockLayout = new int[,]
        {
            {5,5,5,5,5,5,5,5,5,5,5,5,5,5,5},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {2,2,2,2,2,2,2,2,2,2,2,2,2,2,2},
            {3,3,3,3,3,3,3,3,3,3,3,3,3,3,3},
            {4,4,4,4,4,4,4,4,4,4,4,4,4,4,4},
        };*/

        SoundEffect bounceSound, hitSound, deathSound, powerSound;

        Random random = new Random();
        Double prob = 0.2;
        public float preX;
        public float postX;
        public float diff;
        public bool ballCaught = false;
        public bool paddleBig = false;
        float score = 0;
        float speedMult = 0;
        SpriteFont font;

        float breakTimer;
        bool inBreak = false;
        int levelNum = 1;
        int lives = 3;
        bool gameOver = false;
        float bonusLife = 20000;

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

            //Load Level
            LoadLevel("Level1.xml");

            paddle = new Paddle(this);
            paddle.LoadContent();
            paddle.position = new Vector2(512, 740);

            //SpawnBall();
            StartLevelBreak();

            //ball = new Ball(this);
            //ball.LoadContent();
            //ball.position = new Vector2(512, paddle.position.Y - ball.Height - paddle.Height);

            /*for (int i = 0; i < blockLayout.GetLength(1); i++)
            {
                for (int j = 0; j < blockLayout.GetLength(0); j++)
                {
                    Block tempBlock = new Block(this, (BlockColor)blockLayout[j,i]);
                    tempBlock.LoadContent();
                    tempBlock.position = new Vector2(64 + i * 64, 100 + j * 32);
                    blocks.Add(tempBlock);
                }
            }
            */

            bounceSound = Content.Load<SoundEffect>("ball_bounce");
            hitSound = Content.Load<SoundEffect>("ball_hit");
            deathSound = Content.Load<SoundEffect>("death");
            powerSound = Content.Load<SoundEffect>("powerup");
            font = Content.Load<SpriteFont>("main_font");


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
            // TODO: Add your update logic here
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (breakTimer > 0)
            {
                breakTimer -= deltaTime;
            }
            if (breakTimer <= 0 && inBreak == true && gameOver == false)
            {
                inBreak = false;
                SpawnBall();
            }


            if (!inBreak)
            {
                //Instant win button
                if (Keyboard.GetState().IsKeyDown(Keys.G))
                {
                    blocks.Clear();
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }

                preX = paddle.position.X;
                paddle.Update(deltaTime);
                postX = paddle.position.X;

                foreach (Ball g in balls)
                {
                    g.Update(deltaTime);

                    if (g.caught)
                    {
                        g.position.X += postX - preX;
                    }
                }
                DeleteBalls();

                foreach (PowerUp p in powerUps)
                {
                    p.Update(deltaTime);
                }

                KeyboardState keyState = Keyboard.GetState();

                foreach (Ball g in balls)
                {

                    CheckCollisions(g);
                    if (keyState.IsKeyDown(Keys.Space))
                    {
                        ballCaught = false;
                        g.caught = false;

                    }

                }

                CheckForPowerUps();
                DestroyPowerUp();

                if (balls.Count == 0 && gameOver == false)
                {
                    LoseLife();
                }

                if (blocks.Count == 0)
                {
                    NextLevel();
                }

                base.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // TODO: Add your drawing code here
            GraphicsDevice.Clear(Color.Blue);
            spriteBatch.Begin();

            //Draw all sprites here
            spriteBatch.Draw(bgTexture, new Vector2(0, 0), Color.White);
            paddle.Draw(spriteBatch);
            foreach (Ball g in balls)
            {
                g.Draw(spriteBatch);
            }
            foreach (Block b in blocks)
            {
                b.Draw(spriteBatch);
            }
            foreach (PowerUp p in powerUps)
            {
                p.Draw(spriteBatch);
            }
            spriteBatch.DrawString(font, String.Format("Score: {0:#,###0}", score), new Vector2(40, 50), Color.White);
            spriteBatch.DrawString(font, String.Format("Lives: {0}", lives), new Vector2(835, 50), Color.White);

            if (inBreak)
            {
                string levelText = String.Format("Level {0}", levelNum);
                Vector2 strSize = font.MeasureString(levelText);
                Vector2 strLoc = new Vector2(1024 / 2, 768 / 2);
                strLoc.X -= strSize.X / 2;
                strLoc.Y -= strSize.Y / 2;
                spriteBatch.DrawString(font, levelText, strLoc, Color.White);
            }
            if (lives <= 0)
            {
                string gameOverText = String.Format("Game Over");
                Vector2 strSize = font.MeasureString("Game Over");
                Vector2 strLoc = new Vector2(1024 / 2, 768 / 2);
                strLoc.X -= strSize.X / 2;
                strLoc.Y -= strSize.Y / 2;
                spriteBatch.DrawString(font, gameOverText, strLoc, Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected void CheckCollisions(Ball thisBall)
        {
            float radius = thisBall.Width / 2;

            if (hack == 0 && (thisBall.position.X > (paddle.position.X - radius - paddle.Width / 2)) &&
(thisBall.position.X < (paddle.position.X + radius + paddle.Width / 2)) &&
(thisBall.position.Y < paddle.position.Y) &&
(thisBall.position.Y > (paddle.position.Y - radius - paddle.Height / 2)))
            {
                // Reflect based on which part of the paddle is hit

                // By default, set the normal to "up"
                Vector2 normal = -1.0f * Vector2.UnitY;

                // Distance from the leftmost to rightmost part of the paddle
                float dist = paddle.Width + (radius * 2);
                // Where within this distance the ball is at
                float ballLocation = thisBall.position.X -
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
                if (ballCaught)
                {
                    thisBall.caught = true;
                }
                else
                {
                    bounceSound.Play();
                }
                thisBall.direction = Vector2.Reflect(thisBall.direction, normal);
                thisBall.hack = 20;
                //bounceSound.Play();
            }

            Block collidedBlock = null;
            foreach (Block b in blocks)
            {
                if ((thisBall.position.X > (b.position.X - b.Width / 2 - radius)) &&
                    (thisBall.position.X < (b.position.X + b.Width / 2 + radius)) &&
                    (thisBall.position.Y > (b.position.Y - b.Height / 2 - radius)) &&
                    (thisBall.position.Y < (b.position.Y + b.Height / 2 + radius)))
                {
                    collidedBlock = b;
                    break;
                }
            }

            if (collidedBlock != null)
            {
                // Assume that if our Y is close to the top or bottom of the block,
                // we're colliding with the top or bottom
                if ((thisBall.position.Y <
                    (collidedBlock.position.Y - collidedBlock.Height / 2)) ||
                    (thisBall.position.Y >
                    (collidedBlock.position.Y + collidedBlock.Height / 2)))
                {
                    thisBall.direction.Y = -1.0f * thisBall.direction.Y;
                }
                else // otherwise, we have to be colliding from the sides
                {
                    thisBall.direction.X = -1.0f * thisBall.direction.X;
                }

                hitSound.Play();

                // Now remove this block from the list
                if (collidedBlock.OnHit(collidedBlock) == true)
                {
                    blocks.Remove(collidedBlock);
                    AddScore(100 + (100 * (int)speedMult));
                    if (random.NextDouble() < prob)
                    {
                        SpawnPowerUp(collidedBlock.position);
                    }
                }
                //blocks.Remove(collidedBlock);
            }

            if (Math.Abs(thisBall.position.X - 32) < radius)
            {
                // Left wall collision
                thisBall.direction.X = -thisBall.direction.X;
                bounceSound.Play();
            }
            else if (Math.Abs(thisBall.position.X - 992) < radius)
            {
                // Right wall collision
                thisBall.direction.X = -thisBall.direction.X;
                bounceSound.Play();
            }
            else if (Math.Abs(thisBall.position.Y - 32) < radius)
            {
                // Top wall collision
                thisBall.direction.Y = -thisBall.direction.Y;
                bounceSound.Play();
            }

            if (Math.Abs(thisBall.position.Y) > 768 + radius)
            {
                thisBall.destroy = true;
            }

            /*foreach (Ball g in balls)
            {
                if (Math.Abs(g.position.Y) > 768 + radius)
                {
                    g.destroy = true;
                }
            }

            if (balls.Count == 0)
            {
                LoseLife();
            }
            */

        }

        void LoseLife()
        {
            if (lives > 0)
            {
                lives--;
                if (lives >= 1)
                {
                    SpawnBall();
                    paddle.Swap(false);
                    paddle.position = new Vector2(512, 740);
                }
            }
            else
            {
                gameOver = true;
            }
            deathSound.Play();
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
                    AddScore(500 + (500 * (int)speedMult));
                }
            }
        }

        void ActivatePowerUp(PowerUp whichPowerUp)
        {
            if (whichPowerUp.thisType == PowerUpType.ballCatch)
            {
                ballCaught = true;
            }
            else if (whichPowerUp.thisType == PowerUpType.paddleSize)
            {
                //paddleBig = true;
                //paddle.isBig = true;
                //paddle.LoadContent();
                paddle.Swap(true);
            }
            else if (whichPowerUp.thisType == PowerUpType.multiBall)
            {
                SpawnBall();
            }
            whichPowerUp.destroy = true;
            powerSound.Play();
        }

        void SpawnBall()
        {
            Ball tempBall;

            tempBall = new Ball(this);
            tempBall.LoadContent();
            tempBall.position = new Vector2(paddle.position.X, paddle.position.Y - tempBall.Height - paddle.Height);
            tempBall.speed = level.ballSpeed + (100 * speedMult);
            balls.Add(tempBall);

        }

        void DeleteBalls()
        {
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                if (balls[i].destroy == true)
                {
                    balls.RemoveAt(i);
                }
            }
        }

        protected void LoadLevel(string levelName)
        {
            using (FileStream fs = File.OpenRead("Levels/" + levelName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Level));
                level = (Level)serializer.Deserialize(fs);
            }

            //TO DO: Generate blocks based on the level.layout array
            for (int i = 0; i < level.layout.Length; i++)
            {
                for (int j = 0; j < level.layout[i].Length; j++)
                {
                    if (level.layout[i][j] != 9)
                    {
                        Block tempBlock = new Block(this, (BlockColor)level.layout[i][j]);
                        tempBlock.LoadContent();
                        tempBlock.position = new Vector2(64 + j * 64, 100 + i * 32);
                        blocks.Add(tempBlock);
                    }

                }
            }

        }

        void NextLevel()
        {
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                powerUps[i].destroy = true;   
            }
            DestroyPowerUp();

            for (int i = balls.Count - 1; i >= 0; i--)
            {
                balls[i].destroy = true;
                DeleteBalls();
            }

            if (level.nextLevel == "Level1.xml")
            {
                speedMult++;
                //levelNum = 1;
            }

            LoadLevel(level.nextLevel);
            AddScore(5000 + 5000 * (int)speedMult + 500 * (balls.Count - 1) * (int)speedMult);
            paddle.position = new Vector2(512, 740);
            //SpawnBall();
            StartLevelBreak();
            levelNum++;

            
        }

        void AddScore(int scoreToAdd)
        {
            score += scoreToAdd;
            bonusLife -= (float)scoreToAdd;
            if (bonusLife <= 0)
            {
                lives++;
                powerSound.Play();
                bonusLife = 20000;
            }
        }

        void StartLevelBreak()
        {
            inBreak = true;
            breakTimer = 2.0f;
        }

        
    }
}
