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
    public enum BlockColor
    {
        Red = 0,
        Yellow,
        Blue,
        Green,
        Purple,
        GreyHi,
        Grey
    }

    public class Block : GameObject
    {
        public Vector2 direction = new Vector2(0.707f, -0.707f);

        public BlockColor thisBlockColor;

        public Block (Game myGame, BlockColor texColor) :
            base (myGame)
        {
            thisBlockColor = texColor;
        
            switch (texColor)
            {
                case BlockColor.Red:
                    textureName = "block_red";
                    break;
                case BlockColor.Yellow:
                    textureName = "block_yellow";
                    break;
                case BlockColor.Blue:
                    textureName = "block_blue";
                    break;
                case BlockColor.Green:
                    textureName = "block_green";
                    break;
                case BlockColor.Purple:
                    textureName = "block_purple";
                    break;
                case BlockColor.GreyHi:
                    textureName = "block_grey_hi";
                    break;
                case BlockColor.Grey:
                    textureName = "block_grey";
                    break;
            }

            //textureName = "block_red";
        }

        public override void Update(float deltaTime)
        {

            base.Update(deltaTime);
        }

        public bool OnHit(Block thisBlock)
        {
            if (thisBlockColor == BlockColor.GreyHi)
            {
                thisBlockColor = BlockColor.Grey;
                textureName = "block_grey";
                thisBlock.LoadContent();
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
