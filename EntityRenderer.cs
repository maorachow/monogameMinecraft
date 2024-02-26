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
        public GraphicsDevice device;
        public Model zombieModel;
        public Model zombieModelRef;
        public GamePlayer player;
        public Texture2D zombieTex;
        public EntityRenderer(MinecraftGame game,GraphicsDevice device,GamePlayer player,Effect shader,Model model,Texture2D zombieTex,Model zombieModelRef)
        {
            this.device = device;
            this.basicShader = shader;
            this.zombieModel = model;
            this.zombieModelRef = zombieModelRef;
            this.player = player;
            this.zombieTex = zombieTex;
            
        }

        public void Draw()
        {
        //    basicShader.Parameters["View"].SetValue(player.cam.GetViewMatrix());
         //   basicShader.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
     //      
        //    basicShader.Parameters["Texture"].SetValue(zombieTex);

     /*       foreach (var bone in zombieModel.Bones) {
                Debug.WriteLine(bone.Name);        

              foreach (var mesh in bone.Meshes)
                {
                    foreach(var meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = basicShader;
                    }
                
                 //   mesh.Draw();
                }
            }*/
      //      zombieModel.Bones["head"].Transform = Matrix.CreateScale(0.5f);
       //    
         foreach(var entity in EntityBeh.worldEntities)
            {
                switch(entity.typeID)
                {
                    case 0:
                        DrawZombie(entity);
                        break;
                }
            }
         /*   foreach (ModelBone bone in zombieModel.Bones)
                {
                foreach(var mesh in bone.Meshes)
                {
                foreach (Effect effect in mesh.Effects)
                    {
                        //effect.EnableDefaultLighting();
                       
                        effect.Parameters["View"].SetValue(player.cam.GetViewMatrix());
                        effect.Parameters["World"].SetValue(world*bone.ModelTransform);
                        effect.Parameters["Projection"].SetValue(player.cam.projectionMatrix);
                }
                mesh.Draw();
                }
                  
                   
                }*/

            }
        public void DrawZombie(EntityBeh entity)
        {
           // Debug.WriteLine(zombieModelRef.Equals(zombieModel));
            Matrix world = (Matrix.CreateTranslation(entity.position));
            //              Debug.WriteLine(entity.rotationX + "  " + entity.rotationY + "  " + entity.rotationZ);
            if(entity.isEntityHurt)
            {
            foreach (ModelMesh mesh in zombieModel.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    BasicEffect basicEffect = effect as BasicEffect;
                    basicEffect.DiffuseColor = Color.Red.ToVector3();
                }
       
            }
            }
            else
            {
                foreach (ModelMesh mesh in zombieModel.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        BasicEffect basicEffect = effect as BasicEffect;
                        basicEffect.DiffuseColor = Color.White.ToVector3();
                    }

                }
            }
           
            zombieModel.Bones["head"].Transform = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(entity.rotationY), -MathHelper.ToRadians(entity.rotationX), 0) * zombieModelRef.Bones["head"].Transform;
            zombieModel.Bones["body"].Transform = Matrix.CreateFromQuaternion(entity.bodyQuat) * zombieModelRef.Bones["body"].Transform ;
            zombieModel.Bones["rightLeg"].Transform=Matrix.CreateFromYawPitchRoll(0,MathHelper.ToRadians(MathHelper.Clamp(MathF.Cos(entity.entityLifetime*6f)*entity.curSpeed*15f,-55f,55f)) ,0)*zombieModelRef.Bones["rightLeg"].Transform ;
            zombieModel.Bones["leftLeg"].Transform = Matrix.CreateFromYawPitchRoll(0, -MathHelper.ToRadians(MathHelper.Clamp(MathF.Cos(entity.entityLifetime*6f)* entity.curSpeed  * 15f,-55f,55f)), 0) * zombieModelRef.Bones["leftLeg"].Transform;
            zombieModel.Draw(world, player.cam.GetViewMatrix(), player.cam.projectionMatrix);
        }
            //      basicShader.Parameters["World"].SetValue(world * bone.Transform);
           
        }
    }
 
