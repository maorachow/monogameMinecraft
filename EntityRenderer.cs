using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Timers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
 
using System.Text;
using System.Threading.Tasks;

namespace monogameMinecraft
{
    public class EntityRenderer
    {
        public Effect basicShader;
        public Effect shadowMapShader;
        public GraphicsDevice device;
        public Model zombieModel;
        public Model zombieModelRef;
        public GamePlayer player;
        public Texture2D zombieTex;
        public static MinecraftGame game;
        public ShadowRenderer shadowRenderer;
        public EntityRenderer(MinecraftGame game, GraphicsDevice device, GamePlayer player, Effect shader, Model model, Texture2D zombieTex, Model zombieModelRef, Effect shadowmapShader, ShadowRenderer sr)
        {
            this.device = device;
            basicShader = shader;
            zombieModel = model;
            this.zombieModelRef = zombieModelRef;
            this.player = player;
            this.shadowMapShader = shadowmapShader;
            this.zombieTex = zombieTex;
            this.shadowRenderer = sr;
            EntityRenderer.game = game;


        }

        public void Draw()
        {
            //    basicShader.Parameters["View"].SetValue(player.cam.GetViewMatrix());
            //   basicShader.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
            //      
            basicShader.Parameters["TextureE"].SetValue(zombieTex);
            basicShader.Parameters["ShadowMapC"].SetValue(shadowRenderer.shadowMapTarget);
            basicShader.Parameters["LightSpaceMat"].SetValue(shadowRenderer.lightSpaceMat);
  //          basicShader.Parameters["LightSpaceMatFar"].SetValue(shadowRenderer.lightSpaceMatFar);
    //        basicShader.Parameters["ShadowMapCFar"].SetValue(shadowRenderer.shadowMapTargetFar);
            //      zombieModel.Bones["head"].Transform = Matrix.CreateScale(0.5f);

            BoundingFrustum frustum = new BoundingFrustum(game.gamePlayer.cam.viewMatrix * game.gamePlayer.cam.projectionMatrix);  
            foreach (var entity in EntityBeh.worldEntities)
            {
                if(frustum.Intersects(entity.entityBounds))
                {
                switch (entity.typeID)
                {
                    case 0:
                        DrawZombie(entity);
                        break;
                }
                }
                
            }


        }
        public static Matrix[] sharedDrawBoneMatrices;
        public void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (var bone in model.Bones)
            {


                foreach (var mesh in bone.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = basicShader;
                    }


                }
            }
            int count = model.Bones.Count;
            if (sharedDrawBoneMatrices == null || sharedDrawBoneMatrices.Length < count)
            {
                sharedDrawBoneMatrices = new Matrix[count];
            }

            model.CopyAbsoluteBoneTransformsTo(sharedDrawBoneMatrices);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {

                    effect.Parameters["World"].SetValue(sharedDrawBoneMatrices[mesh.ParentBone.Index] * world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                }

                mesh.Draw();
            }
        }

        public void DrawModelShadow(Model model, Matrix world, Matrix lightSpaceMat, Effect shadowMapShader)
        {

            foreach (var bone in model.Bones)
            {


                foreach (var mesh in bone.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = shadowMapShader;
                    }


                }
            }
            int count = model.Bones.Count;
            if (sharedDrawBoneMatrices == null || sharedDrawBoneMatrices.Length < count)
            {
                sharedDrawBoneMatrices = new Matrix[count];
            }

            model.CopyAbsoluteBoneTransformsTo(sharedDrawBoneMatrices);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {

                    effect.Parameters["World"].SetValue(sharedDrawBoneMatrices[mesh.ParentBone.Index] * world);

                    effect.Parameters["LightSpaceMat"].SetValue(lightSpaceMat);
                  
                }

                mesh.Draw();
            }
        }
        public void DrawZombieShadow(EntityBeh entity,Matrix lightSpaceMat,Effect shadowMapShader)
        {

            Matrix world = (Matrix.CreateTranslation(entity.position));
            zombieModel.Bones["head"].Transform = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(entity.rotationY), -MathHelper.ToRadians(entity.rotationX), 0) * zombieModelRef.Bones["head"].Transform;
            zombieModel.Bones["body"].Transform = Matrix.CreateFromQuaternion(entity.bodyQuat) * zombieModelRef.Bones["body"].Transform;
            zombieModel.Bones["rightLeg"].Transform = Matrix.CreateFromYawPitchRoll(0, MathHelper.ToRadians(MathHelper.Clamp(MathF.Cos(entity.entityLifetime * 6f) * entity.curSpeed * 15f, -55f, 55f)), 0) * zombieModelRef.Bones["rightLeg"].Transform;
            zombieModel.Bones["leftLeg"].Transform = Matrix.CreateFromYawPitchRoll(0, -MathHelper.ToRadians(MathHelper.Clamp(MathF.Cos(entity.entityLifetime * 6f) * entity.curSpeed * 15f, -55f, 55f)), 0) * zombieModelRef.Bones["leftLeg"].Transform;
            DrawModelShadow(zombieModel, world, lightSpaceMat,shadowMapShader);
        }
        public void DrawZombie(EntityBeh entity)
        {
            // Debug.WriteLine(zombieModelRef.Equals(zombieModel));

            //              Debug.WriteLine(entity.rotationX + "  " + entity.rotationY + "  " + entity.rotationZ);
            foreach (var bone in zombieModel.Bones)
            {


                foreach (var mesh in bone.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = basicShader;
                    }


                }
            }

                if (entity.isEntityHurt)
                {
                    foreach (ModelMesh mesh in zombieModel.Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {

                            effect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
                        }

                    }
                }
                else
                {
                    foreach (ModelMesh mesh in zombieModel.Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());
                        }

                    }
                }

                Matrix world = (Matrix.CreateTranslation(entity.position));
                zombieModel.Bones["head"].Transform = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(entity.rotationY), -MathHelper.ToRadians(entity.rotationX), 0) * zombieModelRef.Bones["head"].Transform;
                zombieModel.Bones["body"].Transform = Matrix.CreateFromQuaternion(entity.bodyQuat) * zombieModelRef.Bones["body"].Transform;
                zombieModel.Bones["rightLeg"].Transform = Matrix.CreateFromYawPitchRoll(0, MathHelper.ToRadians(MathHelper.Clamp(MathF.Cos(entity.entityLifetime * 6f) * entity.curSpeed * 15f, -55f, 55f)), 0) * zombieModelRef.Bones["rightLeg"].Transform;
                zombieModel.Bones["leftLeg"].Transform = Matrix.CreateFromYawPitchRoll(0, -MathHelper.ToRadians(MathHelper.Clamp(MathF.Cos(entity.entityLifetime * 6f) * entity.curSpeed * 15f, -55f, 55f)), 0) * zombieModelRef.Bones["leftLeg"].Transform;
                DrawModel(zombieModel, world, player.cam.viewMatrix, player.cam.projectionMatrix);

            }
            //      basicShader.Parameters["World"].SetValue(world * bone.Transform);

    }
}



