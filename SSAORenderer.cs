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
        /*     public Effect gBufferEffect;
             public ChunkRenderer chunkRenderer;
             public RenderTarget2D renderTargetPositionDepth;
             public RenderTarget2D renderTargetProjectionDepth;
             public RenderTarget2D renderTargetNormal;
             public RenderTargetBinding[] binding;*/
        public GBufferRenderer gBufferRenderer;
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

            new VertexPositionTexture(new Vector3(-1.0f,  1.0f, 0.0f),new Vector2(  0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 0.0f),new Vector2(  0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f,  1.0f, 0.0f),new Vector2(1.0f, 1.0f)), 
            new VertexPositionTexture(new Vector3(1.0f, -1.0f, 0.0f),new Vector2(1.0f, 0.0f))
         
        

            


       //     new VertexPositionTexture(new Vector3(-1.0f,  1.0f,0f),new Vector2( 0.0f, 1.0f)), 
             ,
        

        //    new VertexPositionTexture(new Vector3(1.0f, -1.0f,0f),new Vector2(1.0f, 0.0f)) ,  

        
        };


        public ushort[] quadIndices =
        {
                0, 1, 2,  
                2, 3, 0
        };
        public void InitializeVertices()
        {
            quadVertices = new VertexPositionTexture[4];
            
            quadVertices[0].Position = new Vector3(-1, 1, 0);
            quadVertices[0].TextureCoordinate = new Vector2(0,0);
             
            quadVertices[1].Position = new Vector3(1, 1, 0);
            quadVertices[1].TextureCoordinate = new Vector2(1, 0);
           
            quadVertices[2].Position = new Vector3(1, -1, 0);
            quadVertices[2].TextureCoordinate = new Vector2(1, 1);
          
            quadVertices[3].Position = new Vector3(-1, -1, 0);
            quadVertices[3].TextureCoordinate = new Vector2(0, 1);
        }
        public IndexBuffer quadIndexBuffer;
        public SSAORenderer(Effect ssaoEffect, GBufferRenderer gBufferRenderer, ChunkRenderer chunkRenderer, GraphicsDevice gd, GamePlayer player, Texture2D randNormal)
        {
            this.graphicsDevice = gd;
            this.ssaoEffect = ssaoEffect;
            this.gBufferRenderer = gBufferRenderer;
        
          
            this.ssaoTarget = new RenderTarget2D(this.graphicsDevice, 800, 600, false, SurfaceFormat.Color, DepthFormat.Depth24);
      
            this.player = player;
            InitializeVertices();
            this.quadVertexBuffer=new VertexBuffer(this.graphicsDevice,typeof(VertexPositionTexture),6,BufferUsage.None);
            
            this.quadVertexBuffer.SetData(quadVertices);
            this.quadIndexBuffer=new IndexBuffer(this.graphicsDevice,IndexElementSize.SixteenBits,6,BufferUsage.None);
            quadIndexBuffer.SetData(quadIndices);
            this.randNormal = randNormal;
            ssaoKernel.Clear();
            for (int i = 0; i < 16; ++i)
            {
                Vector3 sample = new Vector3(random.NextSingle() * 2f - 1f, random.NextSingle() * 2f - 1f, random.NextSingle() * 2f - 1f);
                sample.Normalize();
                sample *= random.NextSingle();

                float scale = (float)i /64f;
                scale = MathHelper.Lerp(0.1f,1f, scale * scale);
                sample *= scale;
                ssaoKernel.Add(sample);
            }
            for (int i = 0; i < 16; i++)
            {
                Vector3 v3 = Vector3.Zero;
                double z = random.NextDouble() * 2.0 - 1.0;
                double r = Math.Sqrt(1.0 - z * z);
                double angle = random.NextDouble() * MathHelper.TwoPi;
                v3.X = (float)(r * Math.Cos(angle));
                v3.Y = (float)(r * Math.Sin(angle));
                v3.Z = (float)z;

                v3 +=new Vector3(0,0,0);
                v3 *= 0.5f;
 
                ssaoNoise.Add(new Color(v3));
            }
            ssaoNoiseTexture = new Texture2D(graphicsDevice, 4, 4,false,SurfaceFormat.Color);
            ssaoNoiseTexture.SetData<Color>(ssaoNoise.ToArray());
        }
        Random random = new Random();
        public void Draw()
        {
               
            if (GameOptions.renderSSAO == false)
            {
                RenderQuad(ssaoTarget, this.ssaoEffect,true);
                return;
            }
            /*           ssaoKernel.Clear();
                             for (int i = 0; i < 64; ++i)
                             {
                                 Vector3 sample = new Vector3(random.NextSingle() * 2f - 1f, random.NextSingle() * 2f - 1f, random.NextSingle() * 2f - 1f);
                                 sample.Normalize();
                                 sample *= random.NextSingle();

                                 float scale = (float)i / 64f;
                                 scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                                 sample *= scale;
                                 ssaoKernel.Add(sample);
                             }*/
          
          /*  graphicsDevice.SetRenderTargets(renderTargetPositionDepth,renderTargetProjectionDepth,renderTargetNormal);
           
            chunkRenderer.RenderAllChunksGBuffer(ChunkManager.chunks, player, this.gBufferEffect);
            graphicsDevice.SetRenderTargets(null);
            graphicsDevice.Clear(Color.CornflowerBlue);*/
    /*        //     ssaoEffect.Parameters["View"].SetValue(player.cam.viewMatrix);
            ssaoEffect.Parameters["ProjectionDepthTex"].SetValue(this.renderTargetProjectionDepth);
       //     ssaoEffect.Parameters["samples"].SetValue(ssaoKernel.ToArray());
            ssaoEffect.Parameters["invProjection"].SetValue(Matrix.Invert(player.cam.projectionMatrix));
       //     ssaoEffect.Parameters["projection"].SetValue(player.cam.projectionMatrix);
           // ssaoEffect.Parameters["NoiseTex"].SetValue(ssaoNoiseTexture);
            ssaoEffect.Parameters["NormalTex"].SetValue(this.renderTargetNormal);*/
                ssaoEffect.Parameters["param_depthMap"].SetValue(gBufferRenderer.renderTargetProjectionDepth);
                    ssaoEffect.Parameters["param_normalMap"].SetValue(gBufferRenderer.renderTargetNormal);
                         ssaoEffect.Parameters["param_randomMap"].SetValue(this.ssaoNoiseTexture);
               // ssaoEffect.Parameters["ViewProjection"].SetValue(player.cam.projectionMatrix*player.cam.viewMatrix);
                   ssaoEffect.Parameters["param_intensity"].SetValue(0.7f);
                ssaoEffect.Parameters["param_scale"].SetValue(1f);
                ssaoEffect.Parameters["param_sampleRadius"].SetValue(0.2f);
                ssaoEffect.Parameters["param_randomSize"].SetValue(0.001f);
            ssaoEffect.Parameters["param_screenSize"].SetValue(100f);
            ssaoEffect.Parameters["g_matInvProjection"].SetValue(Matrix.Invert(player.cam.projectionMatrix ));

            RenderQuad(ssaoTarget, this.ssaoEffect);
        }
        public void RenderQuad(RenderTarget2D target,Effect quadEffect,bool isPureWhite=false)
        {
            graphicsDevice.SetRenderTarget(target);
           
            if(isPureWhite)
            {
                graphicsDevice.Clear(Color.White);
                graphicsDevice.SetRenderTarget(null);
                graphicsDevice.Clear(Color.CornflowerBlue);
                return;
            }
          
            graphicsDevice.SetVertexBuffer(quadVertexBuffer);
            graphicsDevice.Indices= quadIndexBuffer;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphicsDevice.RasterizerState = rasterizerState;
            foreach (var pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,0, 0,4);
            }
       //    graphicsDevice.Clear(Color.White);
            graphicsDevice.SetRenderTarget(null);
            graphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
