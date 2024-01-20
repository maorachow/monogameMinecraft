using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace monogameMinecraft
{
   public class ChunkRenderer
    {

        public MinecraftGame game;
        public GraphicsDevice device;
      //  public AlphaTestEffect basicNSShader;
        public Effect basicShader;
        public Texture2D atlas;
        //Dictionary<Vector2Int,Chunk> RenderingChunks

        public void SetTexture(Texture2D tex)
        {
            atlas= tex;
            basicShader.Parameters["Texture"].SetValue(atlas);
        }
        public ChunkRenderer(MinecraftGame game, GraphicsDevice device, AlphaTestEffect basicNSShader,Effect basicSolidShader)
        {
            this.game = game;
            this.device = device;
            this.basicShader = basicSolidShader;
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.Default;
            //   this.basicNSShader=basicNSShader;
            buffer = new DynamicVertexBuffer(this.device, typeof(VertexPositionNormalTexture),(int)2e4, BufferUsage.WriteOnly);
            //  basicSolidShader.TextureEnabled = true;
            bufferIndex=new DynamicIndexBuffer(this.device,IndexElementSize.SixteenBits,(int)2e5,BufferUsage.WriteOnly);
            basicNSShader.Texture = atlas;
            //   basicShader.Texture = atlas;
         //   basicSolidShader.EnableDefaultLighting();
        }
        //  List<VertexPositionNormalTexture> allVertices;
        public void RenderAllChunks(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player)
        {
            //  allVertices= new List<VertexPositionNormalTexture>();
            basicShader.Parameters["View"].SetValue(player.cam.GetViewMatrix());
            basicShader.Parameters["Projection"].SetValue( player.cam.projectionMatrix);
        //    basicNSShader.View = player.cam.GetViewMatrix();
        //    basicNSShader.Projection = player.cam.projectionMatrix;
            isBusy = true;
            BoundingFrustum frustum=new BoundingFrustum(player.cam.viewMatrix*player.cam.projectionMatrix);
           
            foreach(var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }

                if(c.isReadyToRender==true)
                {
                    
                    if (frustum.Intersects(c.chunkBounds))
                    {
                     RenderSingleChunkOpq(c, player);
                      
                    }
                  
                    
                }
            }
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }

                if (c.isReadyToRender == true)
                {

                    if (frustum.Intersects(c.chunkBounds))
                    {
                        RenderSingleChunkTransparent(c, player);

                    }


                }
            }


            isBusy = false;
        }
         DynamicVertexBuffer buffer;
        DynamicIndexBuffer bufferIndex;
        public static bool isBusy = false;
        void RenderSingleChunkTransparent(Chunk c, GamePlayer player)
        {
            basicShader.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            if (c.verticesWTArray.Length > 0)
            {
                buffer.SetData(c.verticesWTArray);
                device.SetVertexBuffer(buffer);
                bufferIndex.SetData(c.indicesWTArray);
                device.Indices = bufferIndex;
                basicShader.Parameters["Alpha"].SetValue(0.7f);
                foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesWTArray.Length / 3);
                }
            }
        }
         void RenderSingleChunkOpq(Chunk c,GamePlayer player)
        {

            basicShader.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y))) ;
        //    basicNSShader.World= Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y));
            //Debug.WriteLine("render");

         

                basicShader.Parameters["Alpha"].SetValue(1.0f);
                buffer.SetData(c.verticesOpqArray);
                device.SetVertexBuffer(buffer);
              
                bufferIndex.SetData(c.indicesOpqArray);
                device.Indices = bufferIndex;
                foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,0, c.indicesOpqArray.Length / 3);
               }
       //     device.DepthStencilState  =DepthStencilState.None;
              
         //   device.DepthStencilState = DepthStencilState.Default;
            //   device.DepthStencilState = DepthStencilState.None;

            //    device.DepthStencilState = DepthStencilState.Default;
            //  basicShader.Parameters["Alpha"].SetValue(1.0f);
            //  basicShader.Alpha = 1f;




        }
    }
}
