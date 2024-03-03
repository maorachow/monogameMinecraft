using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace monogameMinecraft
{
    public class EntityBeh
    {
        public static List<EntityData> entityDataReadFromDisk=new List<EntityData> ();
        public static List<EntityBeh> worldEntities = new List<EntityBeh>();
        public Vector3 position;
        public float rotationX;
        public float rotationY;
        public float bodyRotationY;
        public Quaternion bodyQuat;
        public float rotationZ;
        public int typeID;
        public string entityID;
        public BoundingBox entityBounds;
        public List< BoundingBox> blocksAround;
        public static float gravity = -9.8f;
        public Vector3 entityVec;
        public Vector3 entitySize;
        public bool isGround = false;
        public float entityHealth;
        public bool isEntityHurt;
        public float entityHurtCD;
        public Vector3 entityMotionVec;
        public MinecraftGame game;
        public Vector3 targetPos;
        public Vector3Int lastIntPos;
        public bool isNeededUpdateBlock;
        public float entityGravity;
        public float entityLifetime;
        public float curSpeed;
        public EntityBeh(Vector3 position, float rotationX, float rotationY, float rotationZ, int typeID, string entityID, float entityHealth, bool isEntityHurt,MinecraftGame game)
        {
            this.position = position;
            this.rotationX = rotationX;
            this.rotationY = rotationY;
            this.rotationZ = rotationZ;
            this.typeID = typeID;
            this.entityID = entityID;
            this.entityHealth = entityHealth;
            this.isEntityHurt = isEntityHurt;
            this.game = game;
        }
        public static void InitEntityList()
        {
            worldEntities = new List<EntityBeh>();
        }
        public void SaveSingleEntity()
        {
      
            EntityData tmpData = new EntityData(this.typeID,position.X,position.Y+entitySize.Y/2f,position.Z,this.rotationX, this.rotationY, this.rotationZ, this.entityID,this.entityHealth);
 
            foreach (EntityData ed in entityDataReadFromDisk)
            {
                if (ed.entityID == this.entityID)
                {
                     
                    entityDataReadFromDisk.Remove(ed);
                    break;
                }
            }
 
            entityDataReadFromDisk.Add(tmpData);
        }
        public static void SpawnEntityFromData(MinecraftGame game)
        {
            foreach(var etd in entityDataReadFromDisk)
            {
                
                EntityBeh tmp = new EntityBeh(new Vector3(etd.posX,etd.posY,etd.posZ), etd.rotX, etd.rotY, etd.rotZ, etd.typeid,etd.entityID, etd.entityHealth, false, game);
                tmp.entitySize = new Vector3(0.6f, 1.8f, 0.6f);
                tmp.InitBounds();
                worldEntities.Add(tmp);
            }
        }
        public static void SpawnNewEntity(Vector3 position, float rotationX, float rotationY, float rotationZ, int typeID,MinecraftGame game)
        {
            EntityBeh tmp = new EntityBeh(position, rotationX, rotationY, rotationZ, typeID, System.Guid.NewGuid().ToString("N"), 20f, false,game);
            tmp.entitySize = new Vector3(0.6f, 1.8f, 0.6f);
            tmp.InitBounds();
            worldEntities.Add(tmp);
        }
      
        public void InitBounds()
        {
            entityBounds = new BoundingBox(position - entitySize / 2f, position + entitySize / 2f);
        }
        public bool CheckIsGround()
        {
            Vector3 pos = new Vector3((entityBounds.Min.X + entityBounds.Max.X) / 2f, entityBounds.Min.Y - 0.1f, (entityBounds.Min.Z + entityBounds.Max.Z) / 2f);

            int blockID = ChunkManager.GetBlock(pos);

            if (blockID > 0 && blockID < 100)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        Vector3 lastPos;
        public Chunk curChunk;
        public bool lastChunkIsReadyToRender;
        public float Vec3Magnitude(Vector3 pos)
        {
            return (float)Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y + pos.Z * pos.Z);
        }
        public void OnUpdate(float deltaTime)
        {

           
            switch (typeID)
            {
            case 0:

                entityLifetime += deltaTime;
                    targetPos = game.gamePlayer.playerPos;
                entityMotionVec = Vector3.Lerp(entityMotionVec, Vector3.Zero, 3f * deltaTime);

             curSpeed =  MathHelper.Lerp(curSpeed,(new Vector2(position.X,position.Z) - new Vector2(lastPos.X, lastPos.Z)).Length()/deltaTime,5f*deltaTime);
            //        Debug.WriteLine(curSpeed);
            lastPos = position;
                    Vector3Int intPos = Vector3Int.FloorToIntVec3(position);
                    
                    curChunk = ChunkManager.GetChunk(ChunkManager.Vec3ToChunkPos(position));
                    
                    
                    if(curChunk != null)
                    {
                        if ( lastChunkIsReadyToRender!=curChunk.isReadyToRender&&(lastChunkIsReadyToRender==false&&curChunk.isReadyToRender==true))
                    {
                      //  Debug.WriteLine("update");
                        isNeededUpdateBlock = true;
                  //      GetBlocksAround(entityBounds);
                    }
                    }
                   
                    if(curChunk!=null)
                    {
                    lastChunkIsReadyToRender = curChunk.isReadyToRender;
                    }
                    




                    if (lastIntPos!=intPos)
                    {
                        isNeededUpdateBlock = true;
                    }
                    lastIntPos = intPos;
                    if(isNeededUpdateBlock)
                    {
                        GetBlocksAround(entityBounds);
                        isNeededUpdateBlock= false;
                    }
                    GetEntitiesAround();


            if (Vector3.Distance(position, targetPos) > 1f)
            {
                Vector3 movePos = new Vector3(targetPos.X - position.X, 0, targetPos.Z - position.Z);
                        if(movePos.X== 0 && movePos.Y == 0 && movePos.Z == 0)
                        {
                            movePos = new Vector3(0.001f, 0.001f, 0.001f);
                        }
                     Vector3 lookPos = new Vector3(targetPos.X - position.X, targetPos.Y - position.Y - 1f, targetPos.Z - position.Z);
                    Vector3 movePosN = Vector3.Normalize(movePos) * 5f*deltaTime;
                    entityVec = movePosN;
          //              Debug.WriteLine(movePos);
                    Vector3 entityRot = LookRotation(lookPos);
                    rotationX = entityRot.X; rotationY = entityRot.Y; rotationZ = entityRot.Z;
                        Quaternion headQuat=Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(rotationY),0,0);
                        bodyQuat = Quaternion.Lerp(bodyQuat, headQuat,10f * deltaTime) ;

                       


            }

 
         
           
            //  Debug.WriteLine(curSpeed);

            //     }

            //   EntityMove(entityVec.X, entityVec.Y, entityVec.Z);

            //     Debug.WriteLine(position.X + " " + position.Y + " " + position.Z);
            if (entityHealth <= 0f)
            {
                worldEntities.Remove(this);

            }
                    if (entityHurtCD >= 0f)
                    {
                        entityHurtCD -= (1f *deltaTime);
                        isEntityHurt = true;
                    }
                    else
                    {
                        isEntityHurt = false;
                    }
                    

                    Vector3 movePos1 = new Vector3(targetPos.X - position.X, 0, targetPos.Z - position.Z);

                    if (Vector3.Distance(position,targetPos)>2f)
                    {
                      
                        if (isGround && curSpeed <= 0.1f && Vec3Magnitude(movePos1) > 2f)
                        {  

                            entityGravity = 5f;
                        }


                        if (entityMotionVec.Length() < 2f)
                        {
                            EntityMove(entityVec.X, 0, entityVec.Z);
                        }
                   




                    }
                    entityVec.Y  = entityGravity * deltaTime;




                    Vector3 entityMotionVecN = entityMotionVec * deltaTime;
                 
                    
                    EntityMove(entityMotionVecN.X, entityMotionVecN.Y, entityMotionVecN.Z);
                    EntityMove(0, entityVec.Y, 0);
                    if (isGround)
                    {
                   
                        entityGravity = 0f;
                    }
                    else
                    {
                       
                        entityGravity += -9.8f * deltaTime;
                    }

                    break;
            }

        }
        public static void HurtEntity(string entityID, float hurtValue, Vector3 sourcePos)
        {
            EntityBeh entityBeh;
            int index = worldEntities.FindIndex((EntityBeh e) => { return entityID == e.entityID; });
            if (index != -1)
            {
                entityBeh = worldEntities[index];
            }
            else
            {
                return;
            }
            if (entityBeh.isEntityHurt == true)
            {
                return;
            }
            entityBeh.entityHealth -= hurtValue;
            entityBeh.entityHurtCD = 0.2f;
            entityBeh.entityMotionVec = Vector3.Normalize(entityBeh.position - sourcePos)*15f;
        }
        Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles;

            // roll (x-axis rotation)
            float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.Z = MathF.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            float sinp = 2 * (q.W * q.Y - q.Z * q.X);
           
                angles.X = MathF.Asin(sinp);

            // yaw (z-axis rotation)
            float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Y = MathF.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
 
        public Vector3 LookRotation(Vector3 fromDir)
        {
            Vector3 eulerAngles = new Vector3();

            //AngleX = arc cos(sqrt((x^2 + z^2)/(x^2+y^2+z^2)))
            eulerAngles.X = (float)Math.Acos(Math.Sqrt((fromDir.X * fromDir.X + fromDir.Z * fromDir.Z) / (fromDir.X * fromDir.X + fromDir.Y * fromDir.Y + fromDir.Z * fromDir.Z))) * 360f / (MathF.PI * 2f);
            if (fromDir.Y > 0) eulerAngles.X = 360f - eulerAngles.X;

            //AngleY = arc tan(x/z)
            eulerAngles.Y = (float)Math.Atan2((float)fromDir.X, (float)fromDir.Z) * 360f / (MathF.PI * 2f);
            if (eulerAngles.Y < 0) eulerAngles.Y += 180f;
            if (fromDir.X < 0) eulerAngles.Y += 180f;
            //AngleZ = 0
            eulerAngles.Z = 0f;
            return eulerAngles;
        }


        void EntityMove(float dx, float dy, float dz)
        {


            float movX = dx;
            float movY = dy;
            float movZ = dz;


            List<BoundingBox> allBounds = new List<BoundingBox>();
            allBounds.AddRange(entitiyBoundsAround);
            allBounds.AddRange(blocksAround);


            foreach (var bb in allBounds)
            {
                dy = BlockCollidingBoundingBoxHelper.calculateYOffset(bb,entityBounds, dy);
            }

            entityBounds = BlockCollidingBoundingBoxHelper.offset(entityBounds,0, dy, 0);

            if (movY != dy && movY < 0)
            {
                isGround = true;
                //     curGravity = 0f;
            }
            else
            {
                isGround = false;
            }

          
            foreach (var bb in allBounds)
            {
                dx =  BlockCollidingBoundingBoxHelper.calculateXOffset(bb,entityBounds, dx);
            }

            entityBounds = BlockCollidingBoundingBoxHelper.offset(entityBounds,dx, 0, 0);

            foreach (var bb in allBounds)
            {
                dz =  BlockCollidingBoundingBoxHelper.calculateZOffset(bb,entityBounds, dz);
            }

            entityBounds = BlockCollidingBoundingBoxHelper.offset(entityBounds,0, 0, dz);
            position = new Vector3((entityBounds.Min.X + entityBounds.Max.X) / 2f, entityBounds.Min.Y, (entityBounds.Min.Z + entityBounds.Max.Z) / 2f);
        }
        public List<BoundingBox> entitiyBoundsAround;
        public void GetEntitiesAround()
        {
            entitiyBoundsAround=new List<BoundingBox>();
            foreach (var entity in EntityBeh.worldEntities)
            {
                if (entity != this)
                {
                    if (MathF.Abs(entity.position.X - position.X) < 10f && MathF.Abs(entity.position.Y - position.Y) < 10f && MathF.Abs(entity.position.Z - position.Z) < 10f)
                    {
                        this.entitiyBoundsAround.Add(new BoundingBox(entity.entityBounds.Min, entity.entityBounds.Max));
                    }
                }
            }
        }
        public List< BoundingBox> GetBlocksAround(BoundingBox aabb)
        {
         
            int minX =ChunkManager.FloorFloat(aabb.Min.X- 0.1f);
            int minY =ChunkManager.FloorFloat(aabb.Min.Y - 0.1f);
            int minZ =ChunkManager.FloorFloat(aabb.Min.Z - 0.1f);
            int maxX = ChunkManager.CeilFloat(aabb.Max.X + 0.1f);
            int maxY = ChunkManager.CeilFloat(aabb.Max.Y + 0.1f);
            int maxZ =ChunkManager.FloorFloat(aabb.Max.Z + 0.1f);

            this.blocksAround = new List< BoundingBox>();

            for (int z = minZ - 1; z <= maxZ + 1; z++)
            {
                for (int x = minX - 1; x <= maxX + 1; x++)
                {
                    for (int y = minY - 1; y <= maxY + 1; y++)
                    {
                        int blockID = ChunkManager.GetBlock(new Vector3(x, y, z));
                        if (blockID > 0 && blockID < 100)
                        {
                            this.blocksAround.Add( new BoundingBox(new Vector3(x, y, z),new Vector3( x + 1, y + 1, z + 1)));
                        }
                    }
                }
            }
          

            return this.blocksAround;


        }
    }

}
