using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BreakernoidsGL
{
    public enum PowerUpType
    {
        ballCatch = 0,
        multiBall,
        paddleSize
    }

    public class PowerUp : GameObject
    {
        public float speed = 200;
        public bool destroy = false;

        public PowerUp (Game myGame, PowerUpType powerUpType) :
            base (myGame)
        {
            switch (powerUpType)
            {
                case PowerUpType.ballCatch:
                    textureName = "powerup_c";
                    break;
                case PowerUpType.multiBall:
                    textureName = "powerup_b";
                    break;
                case PowerUpType.paddleSize:
                    textureName = "powerup_p";
                    break;
            }
        }

        public override void Update(float deltaTime)
        {
            position.Y += speed * deltaTime;

            if (position.Y > 768)
            {
                destroy = true;
            }

            base.Update(deltaTime);
        }
    }
}
