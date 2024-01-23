using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace monogameMinecraft
{
    public class InGameUI : UIElement
    {
        public string playerCrosshair = "+";
        public SpriteFont font;
        public SpriteBatch spriteBatch;
        public  MinecraftGame game;
        public GameWindow window;
        //   public static List<UIElement> UIElements = new List<UIElement>();
        string UIElement.text { get  ; set  ; }
        public InGameUI(SpriteFont sf,GameWindow gw,SpriteBatch sb,   MinecraftGame game) { 
            this.font= sf;
            this.window = gw;
            this.spriteBatch = sb;
            this.game = game;
        //    this.player = player;
        }
        public void DrawPlayerPos()
        {
            if(game.gamePlayer != null)
            {
            spriteBatch.DrawString(font, new StringBuilder("Position:"+ (int)game.gamePlayer.playerPos.X+" "+ (int)game.gamePlayer.playerPos.Y +" "+ (int)game.gamePlayer.playerPos.Z ), new Vector2(0,0), Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 1);
            }
           
        }
        public void Draw()
        {
            //   DrawPlayerPos();
            DrawString(null);
        }

        public void DrawString(string text)
        {
            // this. Draw();
            Vector2 textSize = font.MeasureString(playerCrosshair) / 2f;
            float textSizeScaling = ((float)UIElement.ScreenRect.Height / (float)UIElement.ScreenRectInital.Height) * 2f;
            textSize *= textSizeScaling;
            //   Debug.WriteLine(textSize/2f);
            // textSize.Y = 0;
            // spriteBatch.DrawString(font, text, new Vector2(textPixelPos.x,textPixelPos.y), Color.White);
            spriteBatch.DrawString(font, playerCrosshair, new Vector2(UIElement.ScreenRect.Width/2 - textSize.X, UIElement.ScreenRect.Height / 2 - textSize.Y), Color.White, 0f, new Vector2(0f, 0f), textSizeScaling, SpriteEffects.None, 1);
            DrawPlayerPos();
        }

        public void  GetScreenSpaceRect()
        {
             
        }

        public void  OnResize()
        {
            
        }

        public void  Update()
        {
            
        }
    }
}
