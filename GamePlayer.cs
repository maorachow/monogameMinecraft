using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace monogameMinecraft
{
   public  class GamePlayer
    {
       public Camera cam;
        BoundingBox playerBounds;
        public Vector3 playerPos;
        public float moveVelocity=25f;
        public Vector3 GetBoundingBoxCenter(BoundingBox box)
        {
            return (box.Max + box.Min) / 2f;
        }
      public GamePlayer(Vector3 min, Vector3 max,Game game)
        {
            playerBounds = new BoundingBox(min, max);
            playerPos=GetBoundingBoxCenter(playerBounds);
            cam = new Camera(playerPos, new Vector3(0.0f, 0f, 1.0f), new Vector3(1.0f, 0f, 0.0f), Vector3.UnitY,game);
        }
        public void Move(Vector3 moveVec)
        {
            playerBounds.Max += moveVec;
            playerBounds.Min += moveVec;
            playerPos=GetBoundingBoxCenter(playerBounds);
            cam.position=playerPos; 
        }
       public void ProcessPlayerInputs(Vector3 dir,float deltaTime)
        {
            Vector3 playerMoveVec = new Vector3();
            Vector3 finalMoveVec = deltaTime * moveVelocity * new Vector3(dir.X,dir.Y,dir.Z);
            Debug.WriteLine(finalMoveVec);
            Move(new Vector3(0f, finalMoveVec.Y, 0f));
            if (finalMoveVec.X != 0.0f)
                Move(new Vector3(((cam.horizontalRight * finalMoveVec.X).X),0f, (cam.horizontalRight * finalMoveVec.X).Z));


            if (finalMoveVec.Z != 0.0f)
                Move(new Vector3((cam.horizontalFront * finalMoveVec.Z).X,0f, (cam.horizontalFront * finalMoveVec.Z).Z));
        }
       
    }
   public class Camera
    {
       

        public Camera(Vector3 position, Vector3 front, Vector3 right, Vector3 up,Game game)
        {
            this.position = position;
            this.front = front;
            this.right = right;
            //   this.worldUp = up;
            projectionMatrix= Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), game.GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 1000f);
        }

        public Vector3 position;
        public Vector3 front;
        public Vector3 right;
        public Vector3 up;
        public Vector3 horizontalFront;
        public Vector3 horizontalRight;
        public static Vector3 worldUp=new Vector3(0f,1f,0f);
        public Matrix viewMatrix;
        //public Matrix viewMatrix;
        public Matrix projectionMatrix;
        public float Yaw;
        public float Pitch;
        public float MovementSpeed;
        public float MouseSensitivity =0.5f;
        public void ProcessMouseMovement(float xoffset, float yoffset, bool constrainPitch = true)
        {
            xoffset *= MouseSensitivity;
            yoffset *= MouseSensitivity;

            Yaw += xoffset;
            Pitch += yoffset;

            // make sure that when pitch is out of bounds, screen doesn't get flipped
            if (constrainPitch)
            {
                
                if (Pitch > 89.0f)
                    Pitch = 89.0f;
                if (Pitch < -89.0f)
                    Pitch = -89.0f;
            }

            // update Front, Right and Up Vectors using the updated Euler angles
            updateCameraVectors();
        }
        public void updateCameraVectors()
        {
            // calculate the new Front vector
            Vector3 tmpfront;
            tmpfront.X = MathF.Cos(MathHelper.ToRadians(Yaw)) * MathF.Cos(MathHelper.ToRadians(Pitch));
            tmpfront.Y = MathF.Sin(MathHelper.ToRadians(Pitch));
            tmpfront.Z = MathF.Sin(MathHelper.ToRadians(Yaw)) * MathF.Cos(MathHelper.ToRadians(Pitch));
            horizontalFront=new Vector3(tmpfront.X,0,tmpfront.Z);
            horizontalFront.Normalize();
            tmpfront.Normalize();
            front = tmpfront;
            // also re-calculate the Right and Up vector
            Vector3 tmpright =(Vector3.Cross(front, worldUp)); 
            tmpright.Normalize();
            // normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            Vector3 tmpup = (Vector3.Cross(right, front));
            horizontalRight=Vector3.Cross(horizontalFront, worldUp);
            tmpup.Normalize();
            right= tmpright;
            up= tmpup;
            
        }
        public Matrix GetViewMatrix()
        {
            viewMatrix = Matrix.CreateLookAt(position, position + front, up);
            return viewMatrix;
        }
    }
}
