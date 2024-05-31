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
    public class DeferredShadingRenderer:FullScreenQuadRenderer
    {
        public MinecraftGame game;
      
        public GraphicsDevice device;
        public Effect blockDeferredEffect;
        public ShadowRenderer shadowRenderer;
        public SSAORenderer SSAORenderer;
        public SSRRenderer SSRRenderer;
        public GameTimeManager gameTimeManager;
        public PointLightUpdater lightUpdater;
        public GBufferRenderer gBufferRenderer;
        public ContactShadowRenderer contactShadowRenderer;
        public DeferredShadingRenderer(GraphicsDevice device, Effect blockDeferredEffect, ShadowRenderer shadowRenderer, SSAORenderer sSAORenderer, SSRRenderer sSRRenderer, GameTimeManager gameTimeManager, PointLightUpdater lightUpdater, GBufferRenderer gBufferRenderer,ContactShadowRenderer contactShadowRenderer)
        {
            this.device = device;
            this.blockDeferredEffect = blockDeferredEffect;
            this.shadowRenderer = shadowRenderer;
            SSAORenderer = sSAORenderer;
            SSRRenderer = sSRRenderer;
            this.gameTimeManager = gameTimeManager;
            this.lightUpdater = lightUpdater;
            this.gBufferRenderer = gBufferRenderer;
            this.contactShadowRenderer = contactShadowRenderer;
            InitializeVertices();
            InitializeQuadBuffers(device);
        }

        public void Draw(GamePlayer player)
        {
           
            SetCameraFrustum(player.cam, blockDeferredEffect);
         //   blockDeferredEffect.Parameters["View"].SetValue(player.cam.viewMatrix);
          //  blockDeferredEffect.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
            blockDeferredEffect.Parameters["fogStart"].SetValue(256.0f);
            blockDeferredEffect.Parameters["fogRange"].SetValue(1024.0f);
            blockDeferredEffect.Parameters["LightColor"].SetValue(new Vector3(1, 1, 1));
            blockDeferredEffect.Parameters["LightDir"].SetValue(gameTimeManager.sunDir);
            //  basicShader.Parameters["LightPos"].SetValue(player.playerPos + new Vector3(10, 50, 30));
            blockDeferredEffect.Parameters["viewPos"].SetValue(player.cam.position);
            if (blockDeferredEffect.Parameters["PositionWSTex"] != null) { blockDeferredEffect.Parameters["PositionWSTex"].SetValue(gBufferRenderer.renderTargetPositionWS); }
            // shadowmapShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
            //     RenderShadow(RenderingChunks, player,lightSpaceMat);
            blockDeferredEffect.Parameters["TextureAO"].SetValue(SSAORenderer.ssaoTarget);
            blockDeferredEffect.Parameters["TextureNormals"].SetValue(gBufferRenderer.renderTargetNormalWS);
            blockDeferredEffect.Parameters["TextureAlbedo"].SetValue(gBufferRenderer.renderTargetAlbedo);
            // blockDeferredEffect.Parameters["TextureDepth"].SetValue(gBufferRenderer.renderTargetProjectionDepth);
            blockDeferredEffect.Parameters["TextureContactShadow"].SetValue(contactShadowRenderer.contactShadowRenderTarget);
            blockDeferredEffect.Parameters["TextureReflection"].SetValue(SSRRenderer.renderTargetSSR);
            //    blockDeferredEffect.Parameters["receiveAO"].SetValue(true);
            blockDeferredEffect.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
            blockDeferredEffect.Parameters["LightSpaceMatFar"].SetValue(shadowRenderer.lightSpaceMatFar);
            blockDeferredEffect.Parameters["ShadowMap"].SetValue(shadowRenderer.shadowMapTarget);
            blockDeferredEffect.Parameters["ShadowMapFar"].SetValue(shadowRenderer.shadowMapTargetFar);
            blockDeferredEffect.Parameters["shadowBias"].SetValue(shadowRenderer.shadowBias);
            for (int i = 0; i < lightUpdater.lights.Count; i++)
            {
                blockDeferredEffect.Parameters["LightPosition" + (i + 1).ToString()].SetValue(lightUpdater.lights[i]);
            }
            Vector3 lightPosition1 = blockDeferredEffect.Parameters["LightPosition1"].GetValueVector3();
            Vector3 lightPosition2 = blockDeferredEffect.Parameters["LightPosition2"].GetValueVector3();
            Vector3 lightPosition3 = blockDeferredEffect.Parameters["LightPosition3"].GetValueVector3();
            Vector3 lightPosition4 = blockDeferredEffect.Parameters["LightPosition4"].GetValueVector3();
            //    Debug.WriteLine(lightPosition1);
            foreach (var lightD in lightUpdater.lightsDestroying)
            {

                if (lightD.Equals(lightPosition1))
                {
                    blockDeferredEffect.Parameters["LightPosition1"].SetValue(new Vector3(0, 0, 0));
                    Debug.WriteLine("destroy");
                }
                if (lightD.Equals(lightPosition2))
                {
                    blockDeferredEffect.Parameters["LightPosition2"].SetValue(new Vector3(0, 0, 0));
                }
                if (lightD.Equals(lightPosition3))
                {
                    blockDeferredEffect.Parameters["LightPosition3"].SetValue(new Vector3(0, 0, 0));
                }
                if (lightD.Equals(lightPosition4))
                {
                    blockDeferredEffect.Parameters["LightPosition4"].SetValue(new Vector3(0, 0, 0));
                }
            }
        //    blockDeferredEffect.Parameters["receiveReflection"].SetValue(false);
        //    blockDeferredEffect.Parameters["receiveBackLight"].SetValue(false);
            if (gameTimeManager.sunX > 160f || gameTimeManager.sunX <= 20f)
            {
                blockDeferredEffect.Parameters["receiveShadow"].SetValue(false);

            }
            else
            {
                blockDeferredEffect.Parameters["receiveShadow"].SetValue(true);

            }
            RenderQuad(device, null,blockDeferredEffect,false,true) ;
        }


        public void FinalBlend(SpriteBatch sb,VolumetricLightRenderer vlr,GraphicsDevice device)
        {


            sb.Begin(blendState:BlendState.Additive);
            sb.Draw(vlr.lightShaftTarget, new Rectangle(0, 0, device.PresentationParameters.BackBufferWidth , device.PresentationParameters.BackBufferHeight), Color.White);
            sb.End();
        }
    }
}
