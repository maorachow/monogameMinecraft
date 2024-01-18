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
        public BasicEffect basicShader;
        public Texture2D atlas;
        //Dictionary<Vector2Int,Chunk> RenderingChunks
        public ChunkRenderer(MinecraftGame game, BasicEffect basicShader,GraphicsDevice device)
        {
            this.game = game;
            this.basicShader = basicShader;
            this.device = device;
            buffer = new DynamicVertexBuffer(this.device, typeof(VertexPositionNormalTexture),
                        (int)2e5, BufferUsage.WriteOnly);
            basicShader.TextureEnabled = true;
            basicShader.Texture=atlas;
                 basicShader.EnableDefaultLighting();
        }

        public ChunkRenderer(MinecraftGame game, GraphicsDevice device, BasicEffect basicShader)
        {
            this.game = game;
            this.device = device;
            this.basicShader = basicShader;
            buffer = new DynamicVertexBuffer(this.device, typeof(VertexPositionNormalTexture),(int)2e6, BufferUsage.WriteOnly);
               basicShader.TextureEnabled = true;
                basicShader.Texture = atlas;
                  basicShader.EnableDefaultLighting();
        }

        public void RenderAllChunks(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks,GamePlayer player)
        {
            isBusy = true;
            BoundingFrustum frustum=new BoundingFrustum(player.cam.viewMatrix*player.cam.projectionMatrix);
            basicShader.View=player.cam.GetViewMatrix();
            basicShader.Projection=player.cam.projectionMatrix;
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
                     
                        RenderSingleChunk(c, player);
                    }
                  
                    
                }
            }
            isBusy = false;
        }
        DynamicVertexBuffer buffer;
        public static bool isBusy = false;
         void RenderSingleChunk(Chunk c,GamePlayer player)
        {
           
            basicShader.World= Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y));

            //Debug.WriteLine("render");

          //  basicShader.Alpha=1.0f;
            buffer.SetData(c.verticesOpqArray);
                device.SetVertexBuffer(buffer);
                foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, c.verticesOpqArray.Length / 3);
                }
            // Debug.WriteLine(c.verticesWTArray.Length);
         //   basicShader.Alpha = 0.5f;
            if (c.verticesWTArray.Length > 0)
            {
            buffer.SetData(c.verticesWTArray);
            device.SetVertexBuffer(buffer);
            foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, c.verticesOpqArray.Length / 3);
            }
            }
        //    basicShader.Alpha = 1f;
       
            
         
          
        }
    }
}
