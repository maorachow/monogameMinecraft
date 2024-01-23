using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public static class UIUtility
    {
        public static void InitGameUI(MinecraftGame game)
        {

            SpriteFont sf;
            sf = game.Content.Load<SpriteFont>("defaultfont");
            Texture2D menubkgrd = game.Content.Load<Texture2D>("menubackground");
            Texture2D buttonTex = game.Content.Load<Texture2D>("buttontexture");
            UIElement.UITextures = new Dictionary<string, Texture2D> {
                    { "menubackground" ,menubkgrd},
                    { "buttontexture" ,buttonTex},
            };
            UIElement.menuUIs = new List<UIElement> {
                
                new UIImage(new Vector2(0f,0f),1f,1f,UIElement.UITextures["menubackground"],game._spriteBatch),
                new UIButton(new Vector2(0.3f, 0.5f), 0.4f, 0.2f, UIElement.UITextures["buttontexture"],new Vector2(0.4f,0.55f),sf,game._spriteBatch,game.Window, game.InitGameplay ,"Start Game")
 

            };
            UIElement.inGameUIs = new List<UIElement>
            {
                new InGameUI(sf,game.Window,game._spriteBatch, game) 
            };
            game.status = GameStatus.Menu;
        }
        
    }
}
