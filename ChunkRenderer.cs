using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection.Emit;


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
        public Effect shadowmapShader;
        public void SetTexture(Texture2D tex)
        {
            atlas= tex;
            basicShader.Parameters["Texture"].SetValue(atlas);
        }
        public ChunkRenderer(MinecraftGame game, GraphicsDevice device,Effect basicSolidShader,Effect shadowmapShader)
        {
            this.game = game;
            this.device = device;
            this.basicShader = basicSolidShader;
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.Default;
 
            this.shadowmapShader= shadowmapShader;
 
            shadowMapTarget = new RenderTarget2D(device, 2048, 2048, false,SurfaceFormat.Single,DepthFormat.Depth24);
        }
        public RenderTarget2D shadowMapTarget;
        Matrix lightView = Matrix.CreateLookAt(new Vector3(100,100,100), new Vector3(0,0,0) ,
                         Vector3.Up);

        Matrix lightProjection = Matrix.CreateOrthographic(200, 200, 0.1f,150f);
        public void RenderAllChunks(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player)
        {
            
            basicShader.Parameters["View"].SetValue(player.cam.GetViewMatrix());
            basicShader.Parameters["Projection"].SetValue( player.cam.projectionMatrix);
            basicShader.Parameters["fogStart"].SetValue(256.0f);
            basicShader.Parameters["fogRange"].SetValue(1024.0f);
            lightView = Matrix.CreateLookAt(new Vector3(20, 50, 10) + player.cam.position, player.cam.position, Vector3.UnitY);
            Matrix lightSpaceMat = lightView * lightProjection;
            shadowmapShader.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);
            RenderShadow(RenderingChunks, player,lightSpaceMat);

            isBusy = true; 
            BoundingFrustum frustum=new BoundingFrustum(player.cam.viewMatrix*player.cam.projectionMatrix);
            
            basicShader.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);
            basicShader.Parameters["ShadowMap"].SetValue((Texture2D)shadowMapTarget);
            foreach (var chunk in RenderingChunks)
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
       
        public static bool isBusy = false;
        void RenderShadow(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks,GamePlayer player,Matrix lightSpaceMat)
        {
             
           
            device.SetRenderTarget(shadowMapTarget);
           // device.Clear(Color.Red);
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }
                if((MathF.Abs(c.chunkPos.x - player.playerPos.X) < ( 128) && MathF.Abs(c.chunkPos.y - player.playerPos.Z) < (128)))
                {
                
                if (c.isReadyToRender == true)
                {
                        RenderSingleChunkShadow(c, player);
                }
                }
              
            }
            device.SetRenderTarget(null);
            device.Clear(Color.CornflowerBlue);
            basicShader.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);
        }
       
        void RenderSingleChunkShadow(Chunk c, GamePlayer player )
        {
             Matrix world=(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            shadowmapShader.Parameters["World"].SetValue(world); 
            device.SetVertexBuffer(c.VBOpq);
 
               device.Indices = c.IBOpq;
 
            foreach (EffectPass pass in shadowmapShader.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesOpqArray.Length / 3);
           
            }
        }
        void RenderSingleChunkTransparent(Chunk c, GamePlayer player)
        {
            basicShader.Parameters["renderShadow"].SetValue(false);
            basicShader.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            if (c.verticesWTArray.Length > 0)
            {
                //buffer.SetData(c.verticesWTArray);
                
             //   bufferIndex.SetData(c.indicesWTArray);

                device.Indices = c.IBWT;
                device.SetVertexBuffer(c.VBWT);
                basicShader.Parameters["Alpha"].SetValue(0.7f);
                foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesWTArray.Length / 3);
                }
            }
            basicShader.Parameters["Alpha"].SetValue(1.0f);
            if (c.verticesNSArray.Length > 0)
            {
               
                device.SetVertexBuffer(c.VBNS);
              
                device.Indices = c.IBNS;
              
                foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesNSArray.Length / 3);
                }
            } 
               
        }
         void RenderSingleChunkOpq(Chunk c,GamePlayer player)
        {
            basicShader.Parameters["renderShadow"].SetValue(true);
            basicShader.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y))) ;
        //    basicNSShader.World= Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y));
            //Debug.WriteLine("render");

         

                basicShader.Parameters["Alpha"].SetValue(1.0f);
            
            basicShader.Parameters["viewPos"].SetValue(game.gamePlayer.playerPos);
            //    basicShader.Parameters["fogDensity"].SetValue(0.01f);
            // buffer.SetData(c.verticesOpqArray);
            device.SetVertexBuffer(c.VBOpq);
              
               // bufferIndex.SetData(c.indicesOpqArray);
                device.Indices = c.IBOpq;
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
