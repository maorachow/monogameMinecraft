﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace monogameMinecraft
{
   public  class GamePlayer
    {
        public Camera cam;
        BoundingBox playerBounds;
        public Vector3 playerPos;
        public float moveVelocity=5f;
        float fastPlayerSpeed = 10f;
        float slowPlayerSpeed = 5f;
        public bool isLanded=false;
        public Vector3 GetBoundingBoxCenter(BoundingBox box)
        {
            return (box.Max + box.Min) / 2f;
        }
      public GamePlayer(Vector3 min, Vector3 max,Game game)
        {
            playerBounds = new BoundingBox(min, max);
            playerPos=GetBoundingBoxCenter(playerBounds);
            cam = new Camera(playerPos, new Vector3(0.0f, 0f, 1.0f), new Vector3(1.0f, 0f, 0.0f), Vector3.UnitY,game);
            GetBlocksAround(playerBounds);
        }
        public Dictionary<Vector3Int, BoundingBox> blocksAround=new Dictionary<Vector3Int, BoundingBox>();
        public void GetBlocksAround(BoundingBox aabb)
        {

            int minX = ChunkManager.FloorFloat(aabb.Min.X - 0.1f);
            int minY = ChunkManager.FloorFloat(aabb.Min.Y  - 0.1f);
            int minZ = ChunkManager.FloorFloat(aabb.Min.Z  - 0.1f);
            int maxX = ChunkManager.CeilFloat(aabb.Max.X   + 0.1f);
            int maxY = ChunkManager.CeilFloat(aabb.Max.Y  + 0.1f);
            int maxZ = ChunkManager.CeilFloat(aabb.Max.Z  + 0.1f);

            this.blocksAround = new Dictionary<Vector3Int, BoundingBox>();

            for (int z = minZ - 1; z <= maxZ + 1; z++)
            {
                for (int x = minX - 1; x <= maxX + 1; x++)
                {
                    for (int y = minY - 1; y <= maxY + 1; y++)
                    {
                        int blockID = ChunkManager.GetBlock(new Vector3(x, y, z));
                        if (blockID > 0 && blockID < 100)
                        {
                            this.blocksAround.Add(new Vector3Int(x, y, z), new BoundingBox(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1)));
                        }
                    }
                }
            }


          //  return this.blocksAround;


        }
        public void BreakBlock()
        {
            Ray ray = new Ray(cam.position, cam.front);
            Vector3 blockPoint = ChunkManager.RaycastFirstPosition(ray, 5f);
            ChunkManager.SetBlockWithUpdate(blockPoint,0);
            GetBlocksAround(playerBounds);
        }
        public void Move(Vector3 moveVec)
          {
            //  this.ySize *= 0.4;
            float dx = moveVec.X;
            float dy = moveVec.Y;
            float dz = moveVec.Z;   
                float movX = dx;
                float movY = dy;
                float movZ = dz;
                if (blocksAround.Count == 0)
                {
               //     playerBounds = BlockCollidingBoundingBoxHelper.offset(playerBounds,0, dy, 0);
            //        playerBounds = BlockCollidingBoundingBoxHelper.offset(playerBounds,dx, 0, 0);
             //       playerBounds = BlockCollidingBoundingBoxHelper.offset(playerBounds,0, 0, dz);
                }





                foreach (var bb in blocksAround)
                {
                    dy = BlockCollidingBoundingBoxHelper.calculateYOffset(bb.Value,playerBounds, dy);
                }

                playerBounds = BlockCollidingBoundingBoxHelper.offset(playerBounds, 0, dy, 0);
           
                if (movY != dy)
                {
                isLanded = true;
                //     curGravity = 0f;
                }
                else
                {
                isLanded = false;   
                }
           
                //      bool fallingFlag = (this.onGround || (dy != movY && movY < 0));

                foreach (var bb in blocksAround)
                {
                    dx = BlockCollidingBoundingBoxHelper.calculateXOffset(bb.Value,playerBounds, dx);
                }

                playerBounds = BlockCollidingBoundingBoxHelper.offset(playerBounds, dx, 0, 0);

                foreach (var bb in blocksAround)
                {
                    dz = BlockCollidingBoundingBoxHelper.calculateZOffset(bb.Value, playerBounds, dz);
                }
        //    Debug.WriteLine(dx + " " + movX + " " + dy + " " + movY + " " + dz + " " + movZ) ;
                playerBounds = BlockCollidingBoundingBoxHelper.offset(playerBounds, 0, 0, dz);
            playerPos = GetBoundingBoxCenter(playerBounds);
            cam.position = playerPos+new Vector3(0f,0.6f,0f);
        }
        public Vector3Int playerCurIntPos;
        public Vector3Int playerLastIntPos;
        public float curGravity;
        public void ApplyGravity(float deltaTime)
        {

            if (isPlayerFlying)
            {
                return;
            }
            if (isLanded == true)
            {
                curGravity = 0f;
            }
            else
            {
                curGravity += deltaTime*(-9.8f);
                curGravity = MathHelper.Clamp(curGravity, -25f, 25f);
            }
        }
        public void Jump()
        {
          
        
            if (isLanded == true)
            {
               
               
                
                curGravity = 5f;
            }
            
          
        }
        bool isPlayerFlying = false;

        public void UpdatePlayer(float deltaTime)
        {
           
            ApplyGravity(deltaTime);

        }
        public float jumpCD = 0f;
        public void ProcessPlayerInputs(Vector3 dir, float deltaTime, KeyboardState kState,MouseState mState)
        {
            if (breakBlockCD > 0f)
            {
                breakBlockCD -= deltaTime;
            }
            if (jumpCD >= 0f)
            {
                jumpCD -= deltaTime;
            }
            playerCurIntPos = new Vector3Int((int)playerPos.X, (int)playerPos.Y, (int)playerPos.Z);
            if (playerCurIntPos != playerLastIntPos)
            {
                GetBlocksAround(playerBounds);
            }
            playerLastIntPos=playerCurIntPos;
            Vector3 playerMoveVec = new Vector3();
            Vector3 finalMoveVec = deltaTime * moveVelocity * new Vector3(dir.X,dir.Y,dir.Z);
            //    Debug.WriteLine(finalMoveVec);
                if(kState.IsKeyDown(Keys.F)&&jumpCD<=0f)
                    {
                        isPlayerFlying =! isPlayerFlying;
                    jumpCD = 1f;
                    }
            if (kState.IsKeyDown(Keys.LeftControl)){
                moveVelocity = fastPlayerSpeed;
            }
            else
            {
                moveVelocity = slowPlayerSpeed;
            }
            if (dir.Y > 0f)
            {
                if (jumpCD <=0f)
                {
                   
                    Jump();
                    jumpCD = 0.01f;
                }

            }
           
            
            if (finalMoveVec.X != 0.0f)
                Move(new Vector3(((cam.horizontalRight * finalMoveVec.X).X),0f, (cam.horizontalRight * finalMoveVec.X).Z));


            if (finalMoveVec.Z != 0.0f)
                Move(new Vector3((cam.horizontalFront * finalMoveVec.Z).X,0f, (cam.horizontalFront * finalMoveVec.Z).Z));
            if (isPlayerFlying == true)
            {
            Move(new Vector3(0f, finalMoveVec.Y, 0f));
            }
            else
            {   
                Move(new Vector3(0f, curGravity*deltaTime, 0f));
            }
            if (breakBlockCD <= 0f && mState.LeftButton == ButtonState.Pressed)
            {
                BreakBlock();
                breakBlockCD = 0.3f;
            }
        }
       public float breakBlockCD=0f;
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
            updateCameraVectors();
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
            viewMatrix = Matrix.CreateLookAt(position, position + front, up);
        }
        public Matrix GetViewMatrix()
        {
           // viewMatrix = Matrix.CreateLookAt(position, position + front, up);
            return viewMatrix;
        }
    }
}
