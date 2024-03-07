using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;

namespace monogameMinecraft
{
    public class SSAORenderer

    {
        public Effect ssaoEffect;
        public Effect gBufferEffect;
        public ChunkRenderer chunkRenderer;
        public RenderTarget2D renderTargetPositionDepth;
        public RenderTarget2D renderTargetProjectionDepth;
        public RenderTarget2D renderTargetNormal;
        public RenderTargetBinding[] binding;
        public GraphicsDevice graphicsDevice;
        public GamePlayer player;
        public List<Vector3> ssaoKernel=new List<Vector3>();
        public List<Color> ssaoNoise =new List<Color>();
        public Texture2D ssaoNoiseTexture;
        public VertexBuffer quadVertexBuffer;
        public RenderTarget2D ssaoTarget;

        public Texture2D randNormal;
        public VertexPositionTexture[] quadVertices =
        {
            new VertexPositionTexture(new Vector3(-1.0f,  1.0f,0f),new Vector2( 0.0f, 1.0f)) , 
            new VertexPositionTexture(new Vector3( 1.0f, -1.0f,0f),new Vector2(1.0f, 0.0f)), 
        

            new VertexPositionTexture(new Vector3(-1.0f, -1.0f,0f),new Vector2(  0.0f, 0.0f)),


            new VertexPositionTexture(new Vector3(-1.0f,  1.0f,0f),new Vector2( 0.0f, 1.0f)), 
             new VertexPositionTexture(new Vector3(1.0f,  1.0f,0f),new Vector2(  1.0f, 1.0f)) ,
        

            new VertexPositionTexture(new Vector3(1.0f, -1.0f,0f),new Vector2(1.0f, 0.0f)) ,  

        
        };
        public ushort[] quadIndices =
        {
                0, 1, 3,  
                1, 2, 3
        };
        public IndexBuffer quadIndexBuffer;
        public SSAORenderer(Effect ssaoEffect, Effect gBufferEffect, ChunkRenderer chunkRenderer, GraphicsDevice gd, GamePlayer player, Texture2D randNormal)
        {
            this.graphicsDevice = gd;
            this.ssaoEffect = ssaoEffect;
            this.gBufferEffect = gBufferEffect;
            this.chunkRenderer = chunkRenderer;
            this.renderTargetPositionDepth = new RenderTarget2D(this.graphicsDevice, this.graphicsDevice.Adapter.CurrentDisplayMode.Width, this.graphicsDevice.Adapter.CurrentDisplayMode.Height,false,SurfaceFormat.Rgba64, DepthFormat.Depth24);
            this.renderTargetProjectionDepth = new RenderTarget2D(this.graphicsDevice, this.graphicsDevice.Adapter.CurrentDisplayMode.Width, this.graphicsDevice.Adapter.CurrentDisplayMode.Height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
            this.renderTargetNormal = new RenderTarget2D(this.graphicsDevice, this.graphicsDevice.Adapter.CurrentDisplayMode.Width, this.graphicsDevice.Adapter.CurrentDisplayMode.Height, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
            this.ssaoTarget = new RenderTarget2D(this.graphicsDevice,800,600, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
            this.binding = new RenderTargetBinding[3];
            this.binding[0] = new RenderTargetBinding(this.renderTargetPositionDepth);
            this.binding[1]= new RenderTargetBinding(this.renderTargetProjectionDepth);
            this.binding[2] = new RenderTargetBinding(this.renderTargetNormal);
            this.player = player;
            this.quadVertexBuffer=new VertexBuffer(this.graphicsDevice,typeof(VertexPositionTexture),6,BufferUsage.None);
            this.quadVertexBuffer.SetData(quadVertices);
            this.quadIndexBuffer=new IndexBuffer(this.graphicsDevice,IndexElementSize.SixteenBits,6,BufferUsage.None);
            this.randNormal = randNormal;
            Random random=new Random();
            for (int i = 0; i < 16; ++i)
            {
                Vector3 sample = new Vector3(random.NextSingle()*2f-1f, random.NextSingle() * 2f - 1f, random.NextSingle() * 2f - 1f);
                sample.Normalize();
                sample*=random.NextSingle();

                float scale = (float)i /16f;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                sample *= scale;
                ssaoKernel.Add(sample);
            }
            for(int i = 0; i < 16; i++)
            {
                Color noise = new Color(random.NextSingle()   , random.NextSingle(), 0f,1f);
                ssaoNoise.Add(noise);
            }
            ssaoNoiseTexture = new Texture2D(graphicsDevice, 4, 4,false,SurfaceFormat.Color);
            ssaoNoiseTexture.SetData<Color>(ssaoNoise.ToArray());
        }
        public void Draw()
        {
            this.binding[0] = new RenderTargetBinding(this.renderTargetPositionDepth);
            this.binding[1] = new RenderTargetBinding(this.renderTargetProjectionDepth);
            this.binding[2] = new RenderTargetBinding(this.renderTargetNormal);
            graphicsDevice.SetRenderTargets(binding);
            chunkRenderer.RenderAllChunksGBuffer(ChunkManager.chunks, player, this.gBufferEffect);
            graphicsDevice.SetRenderTargets(null);
            graphicsDevice.Clear(Color.CornflowerBlue);
           // ssaoEffect.Parameters["view"].SetValue(player.cam.viewMatrix);
        // ssaoEffect.Parameters["PositionDepthTex"].SetValue(this.renderTargetPositionDepth);
               ssaoEffect.Parameters["ProjectionDepthTex"].SetValue(this.renderTargetProjectionDepth);
            ssaoEffect.Parameters["NormalTex"].SetValue(this.renderTargetNormal);
           ssaoEffect.Parameters["NoiseTex"].SetValue(this.ssaoNoiseTexture);
           ssaoEffect.Parameters["projection"].SetValue(player.cam.projectionMatrix);
            ssaoEffect.Parameters["g_matInvProjection"].SetValue(Matrix.Invert(player.cam.projectionMatrix));
         
            ssaoEffect.Parameters["samples"].SetValue(ssaoKernel.ToArray());
            RenderQuad(ssaoTarget, this.ssaoEffect);
        }
        public void RenderQuad(RenderTarget2D target,Effect quadEffect)
        {
            graphicsDevice.SetRenderTarget(target);
           
            

            graphicsDevice.SetVertexBuffer(quadVertexBuffer);
            graphicsDevice.Indices= quadIndexBuffer;
            
            foreach (var pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0,6);
            }
            
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
