using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public class SSRRenderer
    {

        public GraphicsDevice graphicsDevice;
        public GamePlayer player;
        public GBufferRenderer gBufferRenderer;
        public RenderTarget2D renderTargetSSR;
        public Effect SSREffect;

        public SSRRenderer(GraphicsDevice graphicsDevice, GamePlayer player, GBufferRenderer gBufferRenderer, Effect sSREffect)
        {
            this.graphicsDevice = graphicsDevice;
            this.player = player;
            this.gBufferRenderer = gBufferRenderer;
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;
            this.renderTargetSSR = new RenderTarget2D(graphicsDevice,width,height,false,SurfaceFormat.Vector4,DepthFormat.Depth24);
            SSREffect = sSREffect;
        }
        public void Draw()
        {
            SSREffect.Parameters["ProjectionDepthTex"].SetValue(gBufferRenderer.renderTargetProjectionDepth);
            SSREffect.Parameters["NormalTex"].SetValue(gBufferRenderer.renderTargetNormalWS);
            SSREffect.Parameters["AlbedoTex"].SetValue(gBufferRenderer.renderTargetAlbedo);
            SSREffect.Parameters["matInverseView"].SetValue(Matrix.Invert(player.cam.viewMatrix));
            SSREffect.Parameters["matInverseProjection"].SetValue(Matrix.Invert(player.cam.projectionMatrix));
            SSREffect.Parameters["matView"].SetValue((player.cam.viewMatrix));
            SSREffect.Parameters["matProjection"].SetValue((player.cam.projectionMatrix));
            SSREffect.Parameters["CameraPos"].SetValue(player.cam.position);
            SSREffect.Parameters["RoughnessMap"].SetValue(gBufferRenderer.renderTargetRoughness);
            RenderQuad(renderTargetSSR, SSREffect);
        }

        public void RenderQuad(RenderTarget2D target, Effect quadEffect, bool isPureWhite = false)
        {
            graphicsDevice.SetRenderTarget(target);

            if (isPureWhite)
            {
                graphicsDevice.Clear(Color.White);
                graphicsDevice.SetRenderTarget(null);
                graphicsDevice.Clear(Color.CornflowerBlue);
                return;
            }
            graphicsDevice.Clear(new Color(0,0,0,0));
            graphicsDevice.SetVertexBuffer(gBufferRenderer.quadVertexBuffer);
            graphicsDevice.Indices = gBufferRenderer.quadIndexBuffer;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphicsDevice.RasterizerState = rasterizerState;
            foreach (var pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4);
            }
            //    graphicsDevice.Clear(Color.White);
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
