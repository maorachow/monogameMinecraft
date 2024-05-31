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
    public class ShadowRenderer
    {
        
        public MinecraftGame game;
        public GraphicsDevice device;
        public RenderTarget2D shadowMapTarget;
        public RenderTarget2D shadowMapTargetFar;
        public Effect shadowMapShader;
        public ChunkRenderer chunkRenderer;
        public EntityRenderer entityRenderer;
        public GameTimeManager gameTimeManager;
        public Model zombieModel;
        public Matrix lightView = Matrix.CreateLookAt(new Vector3(100, 100, 100), new Vector3(0, 0, 0),
                       Vector3.Up);
        public Matrix lightViewFar = Matrix.CreateLookAt(new Vector3(100, 100, 100), new Vector3(0, 0, 0),
                     Vector3.Up);
        public Matrix lightProjection = Matrix.CreateOrthographic(100, 100, 0.01f, 200f);
        public Matrix lightProjectionFar = Matrix.CreateOrthographic(400, 400, 0.1f, 250f);
        public Matrix lightSpaceMat;
        public Matrix lightSpaceMatFar;
     
        public RenderTargetBinding[] shadowMapBinding;
        public float shadowBias;
        public ShadowRenderer(MinecraftGame game, GraphicsDevice device, Effect shadowMapShader, ChunkRenderer cr, EntityRenderer er,GameTimeManager gtr) {
            this.game = game;
            this.device = device;
            this.shadowMapShader = shadowMapShader;
            this.entityRenderer = er;
            this.chunkRenderer = cr;
            shadowMapTarget = new RenderTarget2D(device, 2048, 2048, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
            shadowMapTargetFar = new RenderTarget2D(device, 2048, 2048, false, SurfaceFormat.Rgba64, DepthFormat.Depth24);
            shadowMapBinding = new RenderTargetBinding[2];
            shadowMapBinding[0] = new RenderTargetBinding(shadowMapTarget);

            shadowMapBinding[1] = new RenderTargetBinding(shadowMapTargetFar);
             this.gameTimeManager = gtr;
        }
        public void UpdateLightMatrices(GamePlayer player)
        {
            Vector3 lightDir = gameTimeManager.sunDir;
            Vector3 lightDirFar = gameTimeManager.sunDir*2f;
            lightView = Matrix.CreateLookAt(player.playerPos+ lightDir, player.playerPos, Vector3.UnitY);
            lightViewFar = Matrix.CreateLookAt(player.playerPos + lightDirFar, player.playerPos, Vector3.UnitY);
            //    lightSpaceMat = lightView  *lightProjection;

            lightSpaceMat = lightView*lightProjection;
            lightSpaceMatFar = lightViewFar * lightProjectionFar;
        
        }

        List<Vector4> GetFrustumCornersWorldSpace( Matrix proj, Matrix view)
        {
           Matrix inv = (Matrix.Multiply(Matrix.Invert(proj), Matrix.Invert(view)));

            List<Vector4> frustumCorners=new List<Vector4>();
        for ( int x = 0; x< 2; ++x)
            {
            for ( int y = 0; y< 2; ++y)
                {
                    for ( int z = 0; z< 2; ++z)
                         {
                    Vector4 pt =Vector4.Transform(new Vector4( 2.0f * x - 1.0f,2.0f * y - 1.0f,2.0f * z - 1.0f,1.0f),inv );
                    frustumCorners.Add(pt/pt.W);
                    
                            }
                }
            }
        
       //     Debug.WriteLine(frustumCorners[7]);
            return frustumCorners;
        }

       
    Matrix GetLightSpaceMatrix(  float nearPlane,   float farPlane,GamePlayer player,Vector3 lightDir)
    {
            Matrix proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f),player.cam.aspectRatio, nearPlane, farPlane); ;
       var corners = GetFrustumCornersWorldSpace(proj, player.cam.viewMatrixHorizontal);

        Vector3 center =new Vector3(0, 0, 0);
        foreach (var v in corners)
        {
            center +=new Vector3(v.X,v.Y,v.Z);
        }
        center /= corners.Count;
            //   zombieModel.Draw(Matrix.CreateTranslation(center.X,center.Y,center.Z),player.cam.viewMatrix, player.cam.projectionMatrix);
            //         Debug.WriteLine(center);
           Matrix lightView1 = Matrix.CreateLookAt(center+ lightDir, center, Vector3.UnitY);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        foreach (var v in corners)
        {
           Vector3 trf = Vector3.Transform(new Vector3(v.X,v.Y,v.Z),lightView);
            minX = MathF.Min(minX, trf.X);
            maxX = MathF.Max(maxX, trf.X);
            minY = MathF.Min(minY, trf.Y);
            maxY = MathF.Max(maxY, trf.Y);
            minZ = MathF.Min(minZ, trf.Z);
            maxZ = MathF.Max(maxZ, trf.Z);
        }

            // Tune this parameter according to the scene
            /*     float zMult = 10.0f;
               if (minZ < 0)
               {
                   minZ *= zMult;
               }
               else
                   {
                   minZ /= zMult;
                   }
                   if (maxZ < 0)
                   {
                       maxZ /= zMult;
                   }
                   else
                   {
                       maxZ *= zMult;
                   }*/
            //   Debug.WriteLine(MathF.Abs(minX - maxX));
            //  Debug.WriteLine(MathF.Abs(minY - maxY));
        
            Matrix lightProjection1 = Matrix.CreateOrthographic(MathF.Abs(minX - maxX), MathF.Abs(minY - maxY), 0.1f,250f);
            return lightView1 * lightProjection1;
        }
        public bool isRenderingFarShadow = true;
        public void  RenderShadow(GamePlayer player)
        {
            //   UpdateLightMatrices(player);
            if (gameTimeManager.sunX >= 180f)
            {
                shadowBias = 0.006f;
            }
            else
            {
                shadowBias = -0.006f;
            }
            UpdateLightMatrices(player);
            BoundingFrustum frustum = new BoundingFrustum(game.gamePlayer.cam.viewMatrix * game.gamePlayer.cam.projectionMatrix);
            if (GameOptions.renderShadow)
            {
            device.SetRenderTarget(shadowMapTarget);
            UpdateLightMatrices(player);
        //    Debug.WriteLine(lightSpaceMat.ToString());
            chunkRenderer.RenderShadow(ChunkManager.chunks, player, lightSpaceMat, shadowMapShader);
           

            foreach (var entity in EntityBeh.worldEntities)
            {
                switch (entity.typeID)
                {
                    case 0:
                        if(frustum.Intersects(entity.entityBounds))
                        {
                        entityRenderer.DrawZombieShadow(entity, lightSpaceMat, shadowMapShader);
                        }
                       
                        break;
                }
             //       entityRenderer.DrawModelShadow(entityRenderer.zombieModel, Matrix.CreateTranslation(entity.position), lightSpaceMat,shadowMapShader);
                
                    
            }
            }
            
            if (GameOptions.renderFarShadow)
            {
         device.SetRenderTarget(shadowMapTargetFar);
            UpdateLightMatrices(player);
            //    Debug.WriteLine(lightSpaceMat.ToString());
            chunkRenderer.RenderShadow(ChunkManager.chunks, player, lightSpaceMatFar, shadowMapShader);
          

            foreach (var entity in EntityBeh.worldEntities)
            {
                switch (entity.typeID)
                {
                    case 0:
                        if (frustum.Intersects(entity.entityBounds))
                        {
                            entityRenderer.DrawZombieShadow(entity, lightSpaceMatFar, shadowMapShader);
                        }

                        break;
                }
                //       entityRenderer.DrawModelShadow(entityRenderer.zombieModel, Matrix.CreateTranslation(entity.position), lightSpaceMat,shadowMapShader);


            }
            }
           
            device.SetRenderTarget(null);
            device.Clear(Color.CornflowerBlue);
        }

         

    }
}
