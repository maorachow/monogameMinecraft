using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogameMinecraft
{

    /*
     0,0               1,0
     
     
     
     

    0,1                1,1
     */
    public class UIButton:UIElement
    {
        public Rectangle ButtonRect;
        public Action<UIButton> ButtonAction;
        public Vector2Int textPixelPos;
        public Vector2 textPos;
        public Vector2 textWH;
        public float textHeight;
        public Vector2 element00Pos;
        public Vector2 element01Pos;
        public Vector2 element11Pos;
        public Vector2 element10Pos;
       // public string text="123";
        SpriteBatch spriteBatch;
        public Texture2D texture;
        public SpriteFont font;
        public GameWindow window;
        public string text { get;set; }
        public UIButton(Vector2 position, float width, float height, Texture2D tex, Vector2 tPos ,SpriteFont font,SpriteBatch sb,GameWindow window,Action<UIButton> action,string text) {
            element00Pos = position;
            element10Pos = new Vector2(position.X+width,position.Y);
            element11Pos = new Vector2(position.X + width, position.Y+height);
            element01Pos = new Vector2(position.X , position.Y+height);
            Debug.WriteLine(element00Pos + " " + element10Pos + " " + element11Pos + " " + element01Pos);
            this.texture=tex; 
            this.textPos = tPos;
            this.font = font;
           
            this.spriteBatch = sb;
            this.window = window;
            this.ButtonAction = action;
            this.text = text;
            OnResize();
        }
        public void  Draw()
        {
            DrawString(null);
        }

        public void DrawString(string text)
        {
            this.text = text;
            text=text==null?" " : text;
           // ButtonRect.Center;
            
            spriteBatch.Draw(texture, ButtonRect, Color.White);
            textHeight = (element01Pos - element00Pos).Y;
           
            this.textPixelPos =new Vector2Int( ButtonRect.Center.X, ButtonRect.Center.Y);
            Vector2 textSize = font.MeasureString(text) / 2f ;
           float textSizeScaling=((float)UIElement.ScreenRect.Height/ (float)UIElement.ScreenRectInital.Height)*2f;
            textSize*=textSizeScaling;
         //   Debug.WriteLine(textSize/2f);
         // textSize.Y = 0;
         // spriteBatch.DrawString(font, text, new Vector2(textPixelPos.x,textPixelPos.y), Color.White);
            spriteBatch.DrawString(font, text, new Vector2(textPixelPos.x-textSize.X , textPixelPos.y-textSize.Y ), Color.White, 0f,new Vector2(0f,0f), textSizeScaling, SpriteEffects.None, 1);
        }

        public void  OnResize()
        {
            this.GetScreenSpaceRect( );
        }
        MouseState mouseState;
        MouseState lastMouseState;
        public bool isHovered
        {
            get{
           //     Debug.WriteLine(ButtonRect.X+" "+ ButtonRect.Y + " "+ ButtonRect.Width + " "+ ButtonRect.Height);
           //     Debug.WriteLine(UIElement.ScreenRect.X + " " + UIElement.ScreenRect.Y + " " + UIElement.ScreenRect.Width + " " + UIElement.ScreenRect.Height);
                if (this.ButtonRect.Contains(new Vector2(mouseState.X,mouseState.Y) ))
            {
                return true;

            }
            else
            {
                return false; 
            }}
            
        }
        public UIButton() { }
        public void  Update()
        {
            mouseState = Mouse.GetState();
            if (isHovered && mouseState.LeftButton==ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released)
            {
                //Debug.WriteLine("pressed");
                ButtonAction(this);
            }

                lastMouseState = mouseState;
        }
        public void GetScreenSpaceRect()
        {
            Debug.WriteLine(element00Pos + " " + element01Pos + " " + element10Pos + " " + element11Pos);
            Vector2 transformedP00 = new Vector2((element00Pos.X * UIElement.ScreenRect.Width) , (element00Pos.Y * UIElement.ScreenRect.Height));
            float width=(element10Pos - element00Pos).X * UIElement.ScreenRect.Width;
            float height=(element01Pos - element00Pos).Y * UIElement.ScreenRect.Height;
            this.ButtonRect = new Rectangle((int)transformedP00.X, (int)transformedP00.Y, (int)width, (int)height);
            Debug.WriteLine(ButtonRect.X + " " + ButtonRect.Y + " " + ButtonRect.Width + " " + ButtonRect.Height);

      //      this.textPixelPos = new Vector2Int((int)(textPos.X * UIElement.ScreenRect.Width), (int)(textPos.Y * UIElement.ScreenRect.Height));
            Debug.WriteLine(this.textPixelPos.x+" "+this.textPixelPos.y);
        }
    }
}
