using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace monogameMinecraft
{
    public class ContactShadowRenderer:FullScreenQuadRenderer
    {

        public GraphicsDevice device;
        public Effect contactShadowEffect;
        public GBufferRenderer gBufferRenderer;
        public GameTimeManager gameTimeManager;
        public GamePlayer player;
        public RenderTarget2D contactShadowRenderTarget;
        public ContactShadowRenderer(GraphicsDevice device, Effect contactShadowEffect, GBufferRenderer gBufferRenderer, GameTimeManager gameTimeManager, GamePlayer player)
        {
            this.device = device;
            this.contactShadowEffect = contactShadowEffect;
            this.gBufferRenderer = gBufferRenderer;
            this.gameTimeManager = gameTimeManager;
            InitializeVertices();
            InitializeQuadBuffers(device);
            this.player = player;
            this.contactShadowRenderTarget = new RenderTarget2D(device, 800, 600, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }


        public void Draw()
        {
            var cam = player.cam;

            if (contactShadowEffect.Parameters["PositionWSTex"] != null) { contactShadowEffect.Parameters["PositionWSTex"].SetValue(gBufferRenderer.renderTargetPositionWS); }
            if (contactShadowEffect.Parameters["View"] != null) { contactShadowEffect.Parameters["View"].SetValue(cam.viewMatrix); }
            if (contactShadowEffect.Parameters["CameraPos"] != null) { contactShadowEffect.Parameters["CameraPos"].SetValue(cam.position); }
            if (contactShadowEffect.Parameters["ViewProjection"] != null) { contactShadowEffect.Parameters["ViewProjection"].SetValue(cam.viewMatrix * cam.projectionMatrix); }
            if (contactShadowEffect.Parameters["LightDir"] != null) { contactShadowEffect.Parameters["LightDir"].SetValue(gameTimeManager.sunDir); }
            RenderQuad(device, contactShadowRenderTarget, contactShadowEffect);
        }
    }
}
