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

        public Matrix lightProjection = Matrix.CreateOrthographic(200, 200, 0.01f, 150f);
        public Matrix lightSpaceMat;
        public ShadowRenderer(MinecraftGame game,GraphicsDevice device,Effect shadowMapShader,ChunkRenderer cr,EntityRenderer er) {
            this.game = game;
            this.device = device;
            this.shadowMapShader = shadowMapShader;
            this.entityRenderer = er;
            this.chunkRenderer = cr;
            shadowMapTarget = new RenderTarget2D(device, 2048, 2048, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
        }
        public void UpdateLightMatrices(GamePlayer player)
        {
            lightView = Matrix.CreateLookAt(player.playerPos+ new Vector3(10, 50, 30), player.playerPos, Vector3.UnitY);
            lightSpaceMat = lightView * lightProjection;
        }
        public void  RenderShadow(GamePlayer player)
        {
         //   UpdateLightMatrices(player);
            device.SetRenderTarget(shadowMapTarget);

            chunkRenderer.RenderShadow(ChunkManager.chunks, player, lightSpaceMat, shadowMapShader);
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
