﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Diagnostics.Contracts;

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
        public ShadowRenderer shadowRenderer;
        public void SetTexture(Texture2D tex)
        {
            atlas= tex;
            basicShader.Parameters["Texture"].SetValue(atlas);
        }
        public ChunkRenderer(MinecraftGame game, GraphicsDevice device,Effect basicSolidShader,ShadowRenderer shadowRenderer)
        {
            this.game = game;
            this.device = device;
            this.shadowRenderer = shadowRenderer;
            this.basicShader = basicSolidShader;
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.Default;
 
            this.shadowmapShader= shadowmapShader;
 
            shadowMapTarget = new RenderTarget2D(device, 2048, 2048, false,SurfaceFormat.Rgba64,DepthFormat.Depth24Stencil8);
        }
        public RenderTarget2D shadowMapTarget;
 
        public void RenderAllChunksOpq(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player)
        {
            basicShader.Parameters["Texture"].SetValue(atlas);
            basicShader.Parameters["View"].SetValue(player.cam.viewMatrix);
            basicShader.Parameters["Projection"].SetValue( player.cam.projectionMatrix);
            basicShader.Parameters["fogStart"].SetValue(256.0f);
            basicShader.Parameters["fogRange"].SetValue(1024.0f);
            
           // shadowmapShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
       //     RenderShadow(RenderingChunks, player,lightSpaceMat);

            isBusy = true; 
            BoundingFrustum frustum=new BoundingFrustum(player.cam.viewMatrix*player.cam.projectionMatrix);
            
            basicShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
        //    EntityRenderer.basicShader.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);
            basicShader.Parameters["ShadowMap"].SetValue(shadowRenderer.shadowMapTarget);
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }

                if(c.isReadyToRender==true && c.disposed == false)
                {
                    
                    if (frustum.Intersects(c.chunkBounds))
                    {
                     RenderSingleChunkOpq(c, player);
                      
                    }
                  
                    
                }
            }
             
            

            isBusy = false;
        }

        public void RenderAllChunksTransparent(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player)
        {
            basicShader.Parameters["Texture"].SetValue(atlas);
            basicShader.Parameters["View"].SetValue(player.cam.viewMatrix);
            basicShader.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
            basicShader.Parameters["fogStart"].SetValue(256.0f);
            basicShader.Parameters["fogRange"].SetValue(1024.0f);
        
        
            isBusy = true;
            BoundingFrustum frustum = new BoundingFrustum(player.cam.viewMatrix * player.cam.projectionMatrix);

            basicShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
      
            basicShader.Parameters["ShadowMap"].SetValue(shadowRenderer.shadowMapTarget);
 
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }
                if (c.isReadyToRender == true && c.disposed == false)
                {
                    if (frustum.Intersects(c.chunkBounds))
                    {
                        RenderSingleChunkWater(c, player);

                    }
                    if ((MathF.Abs(c.chunkPos.x - player.playerPos.X) < (256) && MathF.Abs(c.chunkPos.y - player.playerPos.Z) < (256)))
                    {


                        if (frustum.Intersects(c.chunkBounds))
                        {
                            RenderSingleChunkTransparent(c, player);

                        }


                    }
                }

            }


            isBusy = false;
        }


        public static bool isBusy = false;
       public void RenderShadow(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks,GamePlayer player,Matrix lightSpaceMat,Effect shadowmapShader)
        {

            shadowmapShader.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);
       
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }
                if((MathF.Abs(c.chunkPos.x - player.playerPos.X) < ( 128) && MathF.Abs(c.chunkPos.y - player.playerPos.Z) < (128)))
                {
                
                if (c.isReadyToRender == true && c.disposed == false)
                {
                        RenderSingleChunkShadow(c,shadowmapShader);
                }
                }
              
            }
      /*      foreach(var entity in EntityBeh.worldEntities)
            {
                EntityRenderer.DrawModelShadow(EntityRenderer.zombieModel, Matrix.CreateTranslation(entity.position),lightSpaceMat);
            }
            device.SetRenderTarget(null);
            device.Clear(Color.CornflowerBlue);
            basicShader.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);*/
        }
       
        void RenderSingleChunkShadow(Chunk c,Effect shadowmapShader)
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
        void RenderSingleChunkWater(Chunk c, GamePlayer player )
        {
            basicShader.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            if (c.verticesWTArray.Length > 0)
            {
                //buffer.SetData(c.verticesWTArray);

                //   bufferIndex.SetData(c.indicesWTArray);

                device.Indices = c.IBWT;
                device.SetVertexBuffer(c.VBWT);
                basicShader.Parameters["Alpha"].SetValue(0.7f);
                basicShader.Parameters["viewPos"].SetValue(player.playerPos);
                foreach (EffectPass pass in basicShader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesWTArray.Length / 3);
                }
            }
        }
        void RenderSingleChunkTransparent(Chunk c, GamePlayer player)
        {
         
            basicShader.Parameters["World"].SetValue(Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
         
            basicShader.Parameters["viewPos"].SetValue(player.playerPos);
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
            //    basicShader.Parameters["renderShadow"].SetValue(true);
           
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
