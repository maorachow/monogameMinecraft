using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public class GBufferRenderer
    {
        
       // public RenderTarget2D renderTargetPositionDepth;
        public RenderTarget2D renderTargetProjectionDepth;
    //    public RenderTarget2D renderTargetNormal;
        public RenderTarget2D renderTargetNormalWS;
        public RenderTarget2D renderTargetAlbedo;
        public RenderTarget2D renderTargetPositionWS;
        public RenderTargetBinding[] binding;
        public GraphicsDevice graphicsDevice;
        public GamePlayer player;
        public VertexBuffer quadVertexBuffer;
        public ChunkRenderer chunkRenderer;
        public EntityRenderer entityRenderer;
        public Effect gBufferEffect;
        public Effect gBufferEntityEffect;
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
            quadVertices[0].TextureCoordinate = new Vector2(0, 0);

            quadVertices[1].Position = new Vector3(1, 1, 0);
            quadVertices[1].TextureCoordinate = new Vector2(1, 0);

            quadVertices[2].Position = new Vector3(1, -1, 0);
            quadVertices[2].TextureCoordinate = new Vector2(1, 1);

            quadVertices[3].Position = new Vector3(-1, -1, 0);
            quadVertices[3].TextureCoordinate = new Vector2(0, 1);
        }
        public IndexBuffer quadIndexBuffer;
        public GBufferRenderer(GraphicsDevice device,Effect gBufferEffect,Effect gBufferEntityEffect,GamePlayer player,ChunkRenderer cr,EntityRenderer er)
        {
            this.graphicsDevice = device;
            this.gBufferEffect= gBufferEffect;
            this.gBufferEntityEffect= gBufferEntityEffect;
            this.player = player;
            this.chunkRenderer = cr;
            this.entityRenderer = er;
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;
     //       this.renderTargetPositionDepth = new RenderTarget2D(this.graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            this.renderTargetProjectionDepth = new RenderTarget2D(this.graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            this.renderTargetPositionWS = new RenderTarget2D(this.graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            this.renderTargetAlbedo = new RenderTarget2D(this.graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            this.renderTargetNormalWS = new RenderTarget2D(this.graphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            this.binding = new RenderTargetBinding[4];
         
            this.binding[0] = new RenderTargetBinding(this.renderTargetProjectionDepth);
            this.binding[1] = new RenderTargetBinding(this.renderTargetNormalWS);
            this.binding[2] = new RenderTargetBinding(this.renderTargetAlbedo);
            this.binding[3] = new RenderTargetBinding(this.renderTargetPositionWS);
            InitializeVertices();

            quadIndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            quadIndexBuffer.SetData(this.quadIndices);
            quadVertexBuffer=new VertexBuffer(device,typeof(VertexPositionTexture),4,BufferUsage.None);
            quadVertexBuffer.SetData(this.quadVertices);
        }
        public void Resize(int width,int height,GraphicsDevice device)
        {
            this.renderTargetProjectionDepth = new RenderTarget2D(device, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            this.renderTargetPositionWS = new RenderTarget2D(device, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            this.renderTargetAlbedo = new RenderTarget2D(device, width, height, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24);
            this.renderTargetNormalWS = new RenderTarget2D(device, width, height, false, SurfaceFormat.Vector4, DepthFormat.Depth24);
            this.binding = new RenderTargetBinding[4];

            this.binding[0] = new RenderTargetBinding(this.renderTargetProjectionDepth);
            this.binding[1] = new RenderTargetBinding(this.renderTargetNormalWS);
            this.binding[2] = new RenderTargetBinding(this.renderTargetAlbedo);
            this.binding[3] = new RenderTargetBinding(this.renderTargetPositionWS);
        }
        public void Draw()
        {
            graphicsDevice.SetRenderTargets(binding);
           
            chunkRenderer.RenderAllChunksGBuffer(ChunkManager.chunks, player, this.gBufferEffect);
            entityRenderer.DrawGBuffer(gBufferEntityEffect);
            graphicsDevice.SetRenderTargets(null);
            graphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
