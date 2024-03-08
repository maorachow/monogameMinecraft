using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogameMinecraft
{
    public interface UIElement
    {
        public static Dictionary<string, Texture2D> UITextures = new Dictionary<string, Texture2D>();
        public static Rectangle ScreenRect = new Rectangle(0, 0, 800, 480);
        public static Rectangle ScreenRectInital=new Rectangle(0,0,800,480);
        public void GetScreenSpaceRect();
        public void Draw();
        public void DrawString(string text);
        public void Update();
        public void OnResize();
        public string text {get;set;}
        public static List<UIElement> menuUIs = new List<UIElement>();
        public static List<UIElement> settingsUIs = new List<UIElement>();
        public static List<UIElement> inGameUIs = new List<UIElement>();
    }
}
