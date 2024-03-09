using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public class SkyboxRenderer
    {
        public GraphicsDevice device;
        public Effect skyboxEffect;
       
        public TextureCube skyboxTexture;
        
        public VertexBuffer skyboxVertexBuffer;
        public List<VertexPosition> skyboxVertices;
        public GamePlayer player;
        public SkyboxRenderer(GraphicsDevice device, Effect skyboxEffect, TextureCube skyboxTex,GamePlayer player,Texture2D skyboxTexPX, Texture2D skyboxTexPY, Texture2D skyboxTexPZ, Texture2D skyboxTexNX, Texture2D skyboxTexNY, Texture2D skyboxTexNZ)
        {
            this.device = device;
            this.skyboxEffect = skyboxEffect;
           
            this.skyboxTexture = skyboxTex;
            this.player = player;
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
