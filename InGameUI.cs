using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Linq.Expressions;

namespace monogameMinecraft
{
    public class InGameUI : UIElement
    {
        public string playerCrosshair = "+";
        public SpriteFont font;
        public SpriteBatch spriteBatch;
        public  MinecraftGame game;
        public GameWindow window;
        public Texture2D hotbarTex;
        public Texture2D selectedHotbarTex;
        public Vector2[] hotbarItemNodes;
        public float hotbarItemWidth;
        //   public static List<UIElement> UIElements = new List<UIElement>();
        string UIElement.text { get  ; set  ; }
        public InGameUI(SpriteFont sf,GameWindow gw,SpriteBatch sb,   MinecraftGame game,Texture2D hotbarTex,Texture2D selectedHotbar) { 
            this.font= sf;
            this.window = gw;
            this.spriteBatch = sb;
            this.game = game;
            this.hotbarTex = hotbarTex;
            this.selectedHotbarTex = selectedHotbar;
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
        void DrawCrosshair()
        {
            Vector2 textSize = font.MeasureString(playerCrosshair) / 2f;
            float textSizeScaling = ((float)UIElement.ScreenRect.Height / (float)UIElement.ScreenRectInital.Height) * 2f;
            textSize *= textSizeScaling;
 
            spriteBatch.DrawString(font, playerCrosshair, new Vector2(UIElement.ScreenRect.Width/2 - textSize.X, UIElement.ScreenRect.Height / 2 - textSize.Y), Color.White, 0f, new Vector2(0f, 0f), textSizeScaling, SpriteEffects.None, 1);
        }
        public void DrawString(string text)
        {
            // this. Draw();
            DrawCrosshair();
            DrawPlayerPos();
            DrawHotbar();
        }
      
        public void DrawHotbar()
        {
            float hotbaraspectratio= (float)hotbarTex.Width/(float)hotbarTex.Height;
           
            float textureSizeScaling = ((float)UIElement.ScreenRect.Height / (float)UIElement.ScreenRectInital.Height)*2f;
            int hotbarWidth =(int)( hotbarTex.Width * textureSizeScaling);
            hotbarItemWidth=hotbarWidth/9f;
            int hotbarHeight = (int)(hotbarTex.Height * textureSizeScaling);
            Rectangle hotbarRect = new Rectangle(UIElement.ScreenRect.Width / 2 - hotbarWidth / 2, UIElement.ScreenRect.Height - hotbarHeight, hotbarWidth, hotbarHeight);
            spriteBatch.Draw(hotbarTex, hotbarRect, Color.White );
            for(int i = 0; i < game.gamePlayer.inventoryData.Length; i++)
            {
                DrawBlockSpriteAtPoint(game.gamePlayer.inventoryData[i], new Vector2(UIElement.ScreenRect.Width / 2 - hotbarWidth / 2 + i * hotbarItemWidth, UIElement.ScreenRect.Height - hotbarHeight));
            }
            DrawSelectedHotbar(hotbarRect);
        }
        void DrawBlockSpriteAtPoint(short blockID,Vector2 position)
        {
            if (blockID == 0) { return; }
            spriteBatch.Draw(UIElement.UITextures["blocktexture"+blockID], new Rectangle((int)position.X, (int)position.Y, (int)hotbarItemWidth, (int)hotbarItemWidth),Color.White);
        }
        void DrawSelectedHotbar(Rectangle hotbarRect)
        {
            float textureSizeScaling = ((float)UIElement.ScreenRect.Height / (float)UIElement.ScreenRectInital.Height) * 2f;
            int selectedHotbarWidth = (int)(selectedHotbarTex.Width * textureSizeScaling);
            int selectedHotbarHeight = (int)(selectedHotbarTex.Height * textureSizeScaling);
            Rectangle selectedHotbarRect = new Rectangle((int)hotbarRect.X-(int)(3* textureSizeScaling)+game.gamePlayer.currentSelectedHotbar*(int) hotbarItemWidth , (int)hotbarRect.Y - (int)(3 * textureSizeScaling), (int)hotbarItemWidth + (int)(6 * textureSizeScaling), (int)hotbarItemWidth + (int)(6 * textureSizeScaling));
            spriteBatch.Draw(selectedHotbarTex, selectedHotbarRect, Color.White);
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
