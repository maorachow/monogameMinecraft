using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
 

namespace monogameMinecraft
{
    public class ShadowRenderer
    {
        public MinecraftGame game;
        public GraphicsDevice device;
        public RenderTarget2D shadowMapTarget;
        public Effect shadowMapShader;
        public ChunkRenderer chunkRenderer;
        public EntityRenderer entityRenderer;
        
        public Matrix lightView = Matrix.CreateLookAt(new Vector3(100, 100, 100), new Vector3(0, 0, 0),
                       Vector3.Up);

        public Matrix lightProjection = Matrix.CreateOrthographic(200, 200, 0.01f, 100f);
        public Matrix lightSpaceMat;
        public ShadowRenderer(MinecraftGame game,GraphicsDevice device,Effect shadowMapShader,ChunkRenderer cr,EntityRenderer er) {
            this.game = game;
            this.device = device;
            this.shadowMapShader = shadowMapShader;
            this.entityRenderer = er;
            this.chunkRenderer = cr;
            shadowMapTarget = new RenderTarget2D(device, 2048, 2048, false, SurfaceFormat.Rgba64, DepthFormat.Depth24Stencil8);
        }
        public void  RenderShadow()
        {
            lightView = Matrix.CreateLookAt(game.gamePlayer.cam.position + new Vector3(10, 50, 30), game.gamePlayer.cam.position, Vector3.UnitY);
            lightSpaceMat = lightView * lightProjection;
            device.SetRenderTarget(shadowMapTarget);

            chunkRenderer.RenderShadow(ChunkManager.chunks, game.gamePlayer, lightSpaceMat, shadowMapShader);
         //   BoundingFrustum frustum = new BoundingFrustum(game.gamePlayer.cam.viewMatrix * game.gamePlayer.cam.projectionMatrix);
            foreach (var entity in EntityBeh.worldEntities)
            {
                switch (entity.typeID)
                {
                    case 0:
                        entityRenderer.DrawZombieShadow(entity, lightSpaceMat, shadowMapShader);
                        break;
                }
             //       entityRenderer.DrawModelShadow(entityRenderer.zombieModel, Matrix.CreateTranslation(entity.position), lightSpaceMat,shadowMapShader);
                
                    
            }

            device.SetRenderTarget(null);
            device.Clear(Color.CornflowerBlue);
        }

         

    }
}
