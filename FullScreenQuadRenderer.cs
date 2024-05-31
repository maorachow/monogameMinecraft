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
    public class FullScreenQuadRenderer
    {
        
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

        public IndexBuffer quadIndexBuffer;

        public VertexBuffer quadVertexBuffer;
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
        public void InitializeQuadBuffers(GraphicsDevice device)
        {
            this.quadVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), 6, BufferUsage.None);

            this.quadVertexBuffer.SetData(quadVertices);
            this.quadIndexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            quadIndexBuffer.SetData(quadIndices);
        }


        public void SetCameraFrustum(Camera camera, Effect effect)
        {



            Matrix view = camera.viewMatrixOrigin;
            Matrix proj = camera.projectionMatrix;
            Matrix vp = view * proj;

            // 将camera view space 的平移置为0，用来计算world space下相对于相机的vector  
            Matrix cview = view;
         //   cview.Translation = new Vector3(0.0f, 0.0f, 0.0f);
            Matrix cviewProj = cview * proj;

            // 计算viewProj逆矩阵，即从裁剪空间变换到世界空间  
            Matrix cviewProjInv = Matrix.Invert(cviewProj);
            var near = 0.1f;
            BoundingFrustum frustum = new BoundingFrustum(camera.viewMatrix*camera.projectionMatrix);
            Vector3[] corners = frustum.GetCorners();
            Vector3 topLeftCorner = corners[0] - camera.position;
            Vector3 topRightCorner = corners[1] - camera.position;
            Vector3 bottomLeftCorner = corners[3] - camera.position;
            Vector3 cameraXExtent = topRightCorner - topLeftCorner;
            Vector3 cameraYExtent = bottomLeftCorner - topLeftCorner;

            Vector4 topLeftCorner1 = Vector4.Transform(new Vector4(-1.0f, 1.0f, -1,1f),cviewProjInv)/10f;
            Vector4 topRightCorner1 = Vector4.Transform(new Vector4(1.0f, 1.0f,-1, 1f),cviewProjInv) / 10f;
            Vector4 bottomLeftCorner1 = Vector4.Transform(new Vector4(-1.0f, -1.0f,-1, 1f), cviewProjInv) / 10f;

            // 计算相机近平面上方向向量
            Vector4 cameraXExtent1 = topRightCorner1 - topLeftCorner1;
            Vector4 cameraYExtent1 = bottomLeftCorner1 - topLeftCorner1;
        //      Debug.WriteLine("corners:"+(corners[0] - camera.position)+" "+ (corners[1] - camera.position) + " " + (corners[2] - camera.position) + " " + (corners[3] - camera.position));
      //      Debug.WriteLine("corners1:" + (topLeftCorner1) + " " + (topRightCorner1) + " " + (corners[2] - camera.position) + " " + (bottomLeftCorner1));
            if (effect.Parameters["ProjectionParams2"] != null) effect.Parameters["ProjectionParams2"].SetValue(new Vector4(1.0f / near, camera.position.X, camera.position.Y, camera.position.Z));
            if (effect.Parameters["CameraViewTopLeftCorner"] != null) effect.Parameters["CameraViewTopLeftCorner"].SetValue(topLeftCorner1);
            if (effect.Parameters["CameraViewXExtent"] != null) effect.Parameters["CameraViewXExtent"].SetValue(cameraXExtent1);
            if (effect.Parameters["CameraViewYExtent"] != null) effect.Parameters["CameraViewYExtent"].SetValue(cameraYExtent1);
            //    effect.Parameters["CameraPos"].SetValue(cam.position);
        }
        public void RenderQuad(GraphicsDevice device,RenderTarget2D target, Effect quadEffect, bool isPureWhite = false,bool isRenderingOnDcreen=false)
        {
            if (isRenderingOnDcreen == false)
            {
                device.SetRenderTarget(target);
            }
            if (isPureWhite)
            {
                device.Clear(Color.White);
                device.SetRenderTarget(null);
                device.Clear(Color.CornflowerBlue);
                return;
            }

            device.SetVertexBuffer(quadVertexBuffer);
            device.Indices = quadIndexBuffer;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState;
            foreach (var pass in quadEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4);
            }
            //    graphicsDevice.Clear(Color.White);
            if (isRenderingOnDcreen == false)
            {
            device.SetRenderTarget(null);
            device.Clear(Color.CornflowerBlue);
            }
           
        }
    }
}
