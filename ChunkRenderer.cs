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
using System.Diagnostics.Contracts;

namespace monogameMinecraft
{
   public class ChunkRenderer
    {

        public MinecraftGame game;
        public GraphicsDevice device;
      //  public AlphaTestEffect basicNSShader;
        public Effect basicShader;
        public Effect deferredShader;
        public Texture2D atlas;
        public Texture2D atlasNormal;
        public Texture2D atlasDepth;
        //Dictionary<Vector2Int,Chunk> RenderingChunks
        
        public ShadowRenderer shadowRenderer;
        public SSAORenderer SSAORenderer;
        public SSRRenderer SSRRenderer;
        public GameTimeManager gameTimeManager;
        public PointLightUpdater lightUpdater;
        public void SetTexture(Texture2D tex,Texture2D texNormal,Texture2D textureDepth)
        {
            
            atlas = tex;
            
            atlasNormal = texNormal;
            this.atlasDepth= textureDepth;
            
            basicShader.Parameters["Texture"].SetValue(atlas);
            basicShader.Parameters["TextureNormal"].SetValue(atlasNormal);
            basicShader.Parameters["TextureDepth"].SetValue(atlasDepth);
       //     basicShader.Parameters["TextureAO"].SetValue(SSAORenderer.ssaoTarget);
        }
        public ChunkRenderer(MinecraftGame game, GraphicsDevice device,Effect basicSolidShader,ShadowRenderer shadowRenderer,GameTimeManager gameTimeManager)
        {
            this.game = game;
            this.device = device;
            this.shadowRenderer = shadowRenderer;
            this.basicShader = basicSolidShader;
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.Default;
 
                this.gameTimeManager = gameTimeManager;
 
        
        }
 
        public void RenderAllChunksGBuffer(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player,Effect gBufferEffect)
        {

            gBufferEffect.Parameters["blockTex"].SetValue(atlas);
            gBufferEffect.Parameters["View"].SetValue(player.cam.viewMatrix);
            gBufferEffect.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
            BoundingFrustum frustum = new BoundingFrustum(player.cam.viewMatrix * player.cam.projectionMatrix);
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
                        RenderSingleChunkGBuffer(c, player,gBufferEffect);

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

                if (c.isReadyToRender == true && c.disposed == false)
                {
                    
                        if (frustum.Intersects(c.chunkBounds))
                        {
                            RenderSingleChunkGBufferAlphaTest(c, player, gBufferEffect);

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

                if (c.isReadyToRender == true && c.disposed == false)
                {
                    
                        if (frustum.Intersects(c.chunkBounds))
                        {
                            RenderSingleChunkGBufferWater(c, player, gBufferEffect);

                        }
                    



                }
            }
        }
        public void RenderSingleChunkGBuffer(Chunk c,GamePlayer player,Effect gBufferEffect)
        {
            Matrix world = (Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            gBufferEffect.Parameters["World"].SetValue(world);
         //   gBufferEffect.Parameters["TransposeInverseView"].SetValue(Matrix.Transpose(Matrix.Invert(world*player.cam.viewMatrix)));
          //  gBufferEffect.Parameters["roughness"].SetValue(0.0f);
            device.SetVertexBuffer(c.VBOpq);

            device.Indices = c.IBOpq;

            foreach (EffectPass pass in gBufferEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesOpqArray.Length / 3);

            }
        }

        public void RenderSingleChunkGBufferWater(Chunk c, GamePlayer player, Effect gBufferEffect)
        {
            Matrix world = (Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            gBufferEffect.Parameters["World"].SetValue(world);
        //    gBufferEffect.Parameters["TransposeInverseView"].SetValue(Matrix.Transpose(Matrix.Invert(world * player.cam.viewMatrix)));
         //   gBufferEffect.Parameters["roughness"].SetValue(1f);
            device.SetVertexBuffer(c.VBWT);

            device.Indices = c.IBWT;
            if(c.indicesWTArray.Length > 0)
            {
            foreach (EffectPass pass in gBufferEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesWTArray.Length /3);

            }
            }
           
        }


        public void RenderSingleChunkGBufferAlphaTest(Chunk c, GamePlayer player, Effect gBufferEffect)
        {
            Matrix world = (Matrix.CreateTranslation(new Vector3(c.chunkPos.x, 0, c.chunkPos.y)));
            gBufferEffect.Parameters["World"].SetValue(world);
          //  gBufferEffect.Parameters["TransposeInverseView"].SetValue(Matrix.Transpose(Matrix.Invert(world * player.cam.viewMatrix)));
         //   gBufferEffect.Parameters["roughness"].SetValue(0f);
            device.SetVertexBuffer(c.VBNS);

            device.Indices = c.IBNS;
            if (c.indicesNSArray.Length > 0)
            {
                foreach (EffectPass pass in gBufferEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.indicesNSArray.Length / 3);

                }
            }

        }
        public void RenderAllChunksOpq(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player)
        {

            isBusy = true; 
            basicShader.Parameters["Texture"].SetValue(atlas);
            basicShader.Parameters["TextureNormal"].SetValue(atlasNormal);
            basicShader.Parameters["TextureDepth"].SetValue(atlasDepth);
         
            basicShader.Parameters["View"].SetValue(player.cam.viewMatrix);
            basicShader.Parameters["Projection"].SetValue( player.cam.projectionMatrix);
            basicShader.Parameters["fogStart"].SetValue(256.0f);
            basicShader.Parameters["fogRange"].SetValue(1024.0f);
            basicShader.Parameters["LightColor"].SetValue(new Vector3(1, 1, 1));
            basicShader.Parameters["LightDir"].SetValue(gameTimeManager.sunDir);
          //  basicShader.Parameters["LightPos"].SetValue(player.playerPos + new Vector3(10, 50, 30));
            basicShader.Parameters["viewPos"].SetValue(player.cam.position);
            // shadowmapShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
            //     RenderShadow(RenderingChunks, player,lightSpaceMat);
            basicShader.Parameters["TextureAO"].SetValue(SSAORenderer.ssaoTarget);
            basicShader.Parameters["receiveAO"].SetValue(true);
            
            BoundingFrustum frustum=new BoundingFrustum(player.cam.viewMatrix*player.cam.projectionMatrix);
            
            basicShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
            basicShader.Parameters["LightSpaceMatFar"].SetValue(shadowRenderer.lightSpaceMatFar);
            basicShader.Parameters["ShadowMap"].SetValue(shadowRenderer.shadowMapTarget);
            basicShader.Parameters["shadowBias"].SetValue(shadowRenderer.shadowBias);
            
            for(int i=0;i< lightUpdater.lights.Count; i++)
            {
            basicShader.Parameters["LightPosition"+(i+1).ToString()].SetValue(lightUpdater.lights[i]);
            }
                Vector3 lightPosition1= basicShader.Parameters["LightPosition1"].GetValueVector3();
                Vector3 lightPosition2 = basicShader.Parameters["LightPosition2"].GetValueVector3();
                Vector3 lightPosition3 = basicShader.Parameters["LightPosition3"].GetValueVector3();
                Vector3 lightPosition4 = basicShader.Parameters["LightPosition4"].GetValueVector3();
        //    Debug.WriteLine(lightPosition1);
            foreach(var lightD in lightUpdater.lightsDestroying)
            {
              
                if (lightD.Equals(lightPosition1))
                {
                    basicShader.Parameters["LightPosition1"].SetValue(new Vector3(0,0,0));
                    Debug.WriteLine("destroy");
                }
                if (lightD.Equals(lightPosition2))
                {
                    basicShader.Parameters["LightPosition2"].SetValue(new Vector3(0, 0, 0));
                }
                if (lightD.Equals(lightPosition3))
                {
                    basicShader.Parameters["LightPosition3"].SetValue(new Vector3(0, 0, 0));
                }
                if (lightD.Equals(lightPosition4))
                {
                    basicShader.Parameters["LightPosition4"].SetValue(new Vector3(0, 0, 0));
                }
            }
            basicShader.Parameters["receiveReflection"].SetValue(false);
            basicShader.Parameters["receiveBackLight"].SetValue(false);
            if (gameTimeManager.sunX > 160f||gameTimeManager.sunX<=20f)
            {
                basicShader.Parameters["receiveShadow"].SetValue(false);
              
            }
            else
            {
                basicShader.Parameters["receiveShadow"].SetValue(true);
           
            }
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }
                lock(c.renderLock)
                {
             if(c.isReadyToRender==true && c.disposed == false)
                {
                    
                    if (frustum.Intersects(c.chunkBounds))
                    {
                     RenderSingleChunkOpq(c, player);
                      
                    }
                  
                    
                }
                }
               
            }
             
            

            isBusy = false;
        }

        public void RenderAllChunksTransparent(ConcurrentDictionary<Vector2Int, Chunk> RenderingChunks, GamePlayer player)
        {

            isBusy = true;
            basicShader.Parameters["Texture"].SetValue(atlas);
            basicShader.Parameters["View"].SetValue(player.cam.viewMatrix);
            basicShader.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
            basicShader.Parameters["fogStart"].SetValue(256.0f);
            basicShader.Parameters["fogRange"].SetValue(1024.0f);
            basicShader.Parameters["LightColor"].SetValue(new Vector3(1,1,1));
            basicShader.Parameters["LightDir"].SetValue(new Vector3(20, 40, 30));
        //    basicShader.Parameters["LightPos"].SetValue(player.playerPos + new Vector3(10, 50, 30));
           
            BoundingFrustum frustum = new BoundingFrustum(player.cam.viewMatrix * player.cam.projectionMatrix);

            basicShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
            basicShader.Parameters["LightSpaceMatFar"].SetValue(shadowRenderer.lightSpaceMatFar);
            basicShader.Parameters["ShadowMap"].SetValue(shadowRenderer.shadowMapTarget);
            basicShader.Parameters["ShadowMapFar"].SetValue(shadowRenderer.shadowMapTargetFar);
            basicShader.Parameters["receiveAO"].SetValue(false);
            basicShader.Parameters["TextureReflection"].SetValue(SSRRenderer.renderTargetSSR);
           
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }
                lock (c.renderLock)
                {

                    
                    if (c.isReadyToRender == true && c.disposed == false)
                {
                        basicShader.Parameters["receiveReflection"].SetValue(true);
                        basicShader.Parameters["receiveBackLight"].SetValue(false);

                        if (frustum.Intersects(c.chunkBounds))
                    {
                        RenderSingleChunkWater(c, player);

                    }
                        basicShader.Parameters["receiveReflection"].SetValue(false);
                        basicShader.Parameters["receiveBackLight"].SetValue(true);
                        if ((MathF.Abs(c.chunkPos.x - player.playerPos.X) < (256) && MathF.Abs(c.chunkPos.y - player.playerPos.Z) < (256)))
                    {


                        if (frustum.Intersects(c.chunkBounds))
                        {
                            RenderSingleChunkTransparent(c, player);

                        }


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
           
            BoundingFrustum frustum = new BoundingFrustum(player.cam.viewMatrix * player.cam.projectionMatrix);
         
            foreach (var chunk in RenderingChunks)
            {
                Chunk c = chunk.Value;
                if (c == null)
                {
                    continue;
                }              
                if((MathF.Abs(c.chunkPos.x - player.playerPos.X) < ( 256) && MathF.Abs(c.chunkPos.y - player.playerPos.Z) < (256)))
                {
                    if(frustum.Intersects(c.chunkBounds))
                    {
                    if (c.isReadyToRender == true && c.disposed == false)
                {
                        RenderSingleChunkShadow(c,shadowmapShader);
                }
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
         //   basicShader.Parameters["receiveReflection"].SetValue(true);
            if (c.verticesWTArray.Length > 0)
            {
                //buffer.SetData(c.verticesWTArray);

                //   bufferIndex.SetData(c.indicesWTArray);

                device.Indices = c.IBWT;
                device.SetVertexBuffer(c.VBWT);
                basicShader.Parameters["Alpha"].SetValue(0.7f);
                basicShader.Parameters["viewPos"].SetValue(player.cam.position);
              
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
         
            basicShader.Parameters["viewPos"].SetValue(player.cam.position);
            basicShader.Parameters["Alpha"].SetValue(1.0f);
           
            if (c.verticesNSArray.Length > 0)
            {
          //      basicShader.Parameters["receiveReflection"].SetValue(false);
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
            
                basicShader.Parameters["viewPos"].SetValue(game.gamePlayer.cam.position);
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
