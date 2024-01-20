﻿using System;
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
        public static Rectangle ScreenRect;
        public void GetScreenSpaceRect();
        public void Draw();
        public void DrawString(string text);
        public void Update();
        public void OnResize();
        public string text {get;set;}
        public static List<UIElement> menuUIs = new List<UIElement>();
    }
}
