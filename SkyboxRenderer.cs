using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace monogameMinecraft
{
    public class SkyboxRenderer
    {
        public GraphicsDevice device;
        public Effect skyboxEffect;
       
        public TextureCube skyboxTexture;
        public TextureCube skyboxTextureNight;
        public VertexBuffer skyboxVertexBuffer;
        public List<VertexPosition> skyboxVertices;
        public GamePlayer player;
        public float curDateTime = 0f;
        public GameTimeManager gametimeManager;
        public SkyboxRenderer(GraphicsDevice device, Effect skyboxEffect, TextureCube skyboxTex,GamePlayer player,Texture2D skyboxTexPX, Texture2D skyboxTexPY, Texture2D skyboxTexPZ, Texture2D skyboxTexNX, Texture2D skyboxTexNY, Texture2D skyboxTexNZ
            , Texture2D skyboxTexPXN, Texture2D skyboxTexPYN, Texture2D skyboxTexPZN, Texture2D skyboxTexNXN, Texture2D skyboxTexNYN, Texture2D skyboxTexNZN,GameTimeManager gametimeManager 
            )
        {
            this.device = device;
            this.skyboxEffect = skyboxEffect;
           
            this.skyboxTexture = skyboxTex;
            this.player = player;
            this.gametimeManager = gametimeManager;
            this.skyboxVertices = new List<VertexPosition> {
           new VertexPosition( new Vector3(  -1.0f, -1.0f, -1.0f)),
    new VertexPosition(new Vector3(  1.0f, -1.0f, -1.0f)),
   new VertexPosition(  new Vector3( 1.0f,  1.0f, -1.0f)),
   new VertexPosition(new Vector3(   1.0f,  1.0f, -1.0f)),
   new VertexPosition( new Vector3( -1.0f,  1.0f, -1.0f)),
   new VertexPosition( new Vector3( -1.0f, -1.0f, -1.0f)),

   new VertexPosition(new Vector3(-1.0f, -1.0f,  1.0f)),
   new VertexPosition(new Vector3(   1.0f, -1.0f,  1.0f)),
   new VertexPosition( new Vector3(  1.0f,  1.0f,  1.0f)),
   new VertexPosition( new Vector3(  1.0f,  1.0f,  1.0f)),
  new VertexPosition(new Vector3(   -1.0f,  1.0f,  1.0f)),
  new VertexPosition( new Vector3(  -1.0f, -1.0f,  1.0f)),

  new VertexPosition(  new Vector3( -1.0f,  1.0f,  1.0f)),
   new VertexPosition(new Vector3(  -1.0f,  1.0f, -1.0f)),
   new VertexPosition(new Vector3(  -1.0f, -1.0f, -1.0f)),
   new VertexPosition(new Vector3(  -1.0f, -1.0f, -1.0f)),
  new VertexPosition( new Vector3(  -1.0f, -1.0f,  1.0f)),
  new VertexPosition( new Vector3(  -1.0f,  1.0f,  1.0f)),

  new VertexPosition( new Vector3(   1.0f,  1.0f,  1.0f )),
  new VertexPosition( new Vector3(   1.0f,  1.0f, -1.0f)),
   new VertexPosition(new Vector3(   1.0f, -1.0f, -1.0f)),
   new VertexPosition( new Vector3(  1.0f, -1.0f, -1.0f)),
  new VertexPosition( new Vector3(   1.0f, -1.0f,  1.0f)),
  new VertexPosition( new Vector3(   1.0f,  1.0f,  1.0f)),

   new VertexPosition(new Vector3(  -1.0f, -1.0f, -1.0f)),
  new VertexPosition( new Vector3(   1.0f, -1.0f, -1.0f)),
new VertexPosition(   new Vector3(   1.0f, -1.0f,  1.0f)),
   new VertexPosition(new Vector3(   1.0f, -1.0f,  1.0f)),
  new VertexPosition(  new Vector3( -1.0f, -1.0f,  1.0f)),
  new VertexPosition(  new Vector3( -1.0f, -1.0f, -1.0f)),

  new VertexPosition( new Vector3(  -1.0f,  1.0f, -1.0f)),
   new VertexPosition(new Vector3(   1.0f,  1.0f, -1.0f)),
  new VertexPosition( new Vector3(   1.0f,  1.0f,  1.0f)),
  new VertexPosition( new Vector3(   1.0f,  1.0f,  1.0f)),
  new VertexPosition( new Vector3(  -1.0f,  1.0f,  1.0f)),
  new VertexPosition( new Vector3(  -1.0f,  1.0f, -1.0f) ),
            };
            skyboxVertexBuffer = new VertexBuffer(device, typeof(VertexPosition), 36, BufferUsage.None);
            skyboxVertexBuffer.SetData<VertexPosition>(skyboxVertices.ToArray());
            int width=skyboxTexPX.Width;
            int height=skyboxTexPX.Height;
            Color[] data=new Color[width*height];
            skyboxTexture = new TextureCube(device,128, false, SurfaceFormat.Color);
            skyboxTexPX.GetData(data);
            skyboxTexture.SetData(CubeMapFace.PositiveX, data);
            skyboxTexPY.GetData(data);
            skyboxTexture.SetData(CubeMapFace.PositiveY, data);
            skyboxTexPZ.GetData(data);
            skyboxTexture.SetData(CubeMapFace.PositiveZ, data);
            skyboxTexNX.GetData(data);
            skyboxTexture.SetData(CubeMapFace.NegativeX, data);
            skyboxTexNY.GetData(data);
            skyboxTexture.SetData(CubeMapFace.NegativeY, data);
            skyboxTexNZ.GetData(data);
            skyboxTexture.SetData(CubeMapFace.NegativeZ, data);
 
            skyboxTextureNight = new TextureCube(device, 128, false, SurfaceFormat.Color);
            skyboxTexPXN.GetData(data);
            skyboxTextureNight.SetData(CubeMapFace.PositiveX, data);
            skyboxTexPYN.GetData(data);
            skyboxTextureNight.SetData(CubeMapFace.PositiveY, data);
            skyboxTexPZN.GetData(data);
            skyboxTextureNight.SetData(CubeMapFace.PositiveZ, data);
            skyboxTexNXN.GetData(data);
            skyboxTextureNight.SetData(CubeMapFace.NegativeX, data);
            skyboxTexNYN.GetData(data);
            skyboxTextureNight.SetData(CubeMapFace.NegativeY, data);
            skyboxTexNZN.GetData(data);
            skyboxTextureNight.SetData(CubeMapFace.NegativeZ, data);

        }
        public void Draw( )
        {
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState ;
            device.DepthStencilState = DepthStencilState.None;
            skyboxEffect.Parameters["World"].SetValue(Matrix.CreateScale(50f) * Matrix.CreateTranslation(player.cam.position));
            skyboxEffect.Parameters["View"].SetValue(player.cam.viewMatrix);
            skyboxEffect.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
                 skyboxEffect.Parameters["SkyBoxTexture"].SetValue(skyboxTexture);
               skyboxEffect.Parameters["SkyBoxTextureNight"].SetValue(skyboxTextureNight);
         
          //  Debug.WriteLine(gametimeManager.dateTime);
            float time=(gametimeManager.dateTime-0.25f)%1f;
            float mixValue=0f;
            if (0f<= time&&time < 0.15f)
            {
                mixValue = 0;
            }
            else if (0.15f<=time&&time<0.35f)
            {
                mixValue = MathHelper.SmoothStep(0f, 1f, (time - 0.15f) * 5f);
            }
            else if (0.35f <= time && time < 0.65f)
            {
                mixValue = 1;
            }
            else if (0.65f <= time && time < 0.85f)
            {
                mixValue = MathHelper.SmoothStep(1f, 0f, (time - 0.65f) * 5f);
            }
            else if (0.85f <= time && time < 1f)
            {
                mixValue = 0;
            }
            skyboxEffect.Parameters["mixValue"].SetValue(mixValue);
            skyboxEffect.Parameters["CameraPosition"].SetValue(player.cam.position);
            device.SetVertexBuffer(skyboxVertexBuffer);
            foreach(var pass in skyboxEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 36);
            }
            device.DepthStencilState = DepthStencilState.Default;
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.CullCounterClockwiseFace;
            device.RasterizerState = rasterizerState1;
        }
    }
}
