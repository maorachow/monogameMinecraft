using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using monogameMinecraft;
using Microsoft.Xna.Framework;
using MessagePack;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace monogameMinecraft
{
    internal class ChunkManager
    {
        public static Dictionary<Vector2Int, ChunkData> chunkDataReadFromDisk = new Dictionary<Vector2Int, ChunkData>();
        public static ConcurrentDictionary<Vector2Int, Chunk> chunks = new ConcurrentDictionary<Vector2Int, Chunk>();
        public static object chunkLock=new object();
        public static Chunk GetChunk(Vector2Int pos)
        {
            if (chunks.ContainsKey(pos))
            {
            return chunks[pos];
            }
            else
            {
                return null;
            }
            
        }
        public static Vector3Int Vec3ToBlockPos(Vector3 pos)
        {
            Vector3Int intPos = new Vector3Int(FloatToInt(pos.X), FloatToInt(pos.Y), FloatToInt(pos.Z));
            return intPos;
        }
        public static int FloatToInt(float f)
        {
            if (f >= 0)
            {
                return (int)f;
            }
            else
            {
                return (int)f - 1;
            }
        }
        public static int FloorFloat(float n)
        {
            int i = (int)n;
            return n >= i ? i : i - 1;
        }

        public static int CeilFloat(float n)
        {
            int i = (int)(n + 1);
            return n >= i ? i : i - 1;
        }
        public static Vector2Int Vec3ToChunkPos(Vector3 pos)
        {
            Vector3 tmp = pos;
            tmp.X = MathF.Floor(tmp.X / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
            tmp.Z = MathF.Floor(tmp.Z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
            Vector2Int value = new Vector2Int((int)tmp.X, (int)tmp.Z);
            //  mainForm.LogOnTextbox(value.x+" "+value.y+"\n");
            return value;
        }
        public static bool isJsonReadFromDisk { get;  set; }
        public static bool isWorldDataSaved { get; private set; }
        public static string gameWorldDataPath = AppDomain.CurrentDomain.BaseDirectory;

        
        public static void SaveWorldData()
        {

            FileStream fs;
            if (File.Exists(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json"))
            {
                fs = new FileStream(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
            }
            else
            {
                fs = new FileStream(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json", FileMode.Create, FileAccess.Write);
            }
            fs.Close();
            foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
            {
                // int[] worldDataMap=ThreeDMapToWorldData(c.Value.map);
                //   int x=(int)c.Value.transform.position.x;
                //  int z=(int)c.Value.transform.position.z;
                //   WorldData wd=new WorldData();
                //   wd.map=worldDataMap;
                //   wd.posX=x;
                //   wd.posZ=z;
                //   string tmpData=JsonMapper.ToJson(wd);
                //   File.AppendAllText(Application.dataPath+"/GameData/world.json",tmpData+"\n");
                c.Value.SaveSingleChunk();
            }

            //    foreach (KeyValuePair<Vector2Int, ChunkData> wd in chunkDataReadFromDisk)
            //   {
            //      string tmpData = JsonConvert.SerializeObject(wd.Value);

            //    }
            byte[] allWorldData = MessagePackSerializer.Serialize(chunkDataReadFromDisk);
            File.WriteAllBytes(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json", allWorldData);
            isWorldDataSaved = true;
        }
        public static  bool CheckIsPosInChunk(Vector3 pos, Chunk c)
        {
            if (c == null)
            {
                return false;
            }
          
            Vector3  chunkSpacePos = pos -  new Vector3 (c.chunkPos.x, 0, c.chunkPos.y );
            if (chunkSpacePos.X >= 0 && chunkSpacePos.X < Chunk.chunkWidth && chunkSpacePos.Z >= 0 && chunkSpacePos.Z < Chunk.chunkWidth)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void UpdateWorldThread( GamePlayer player,MinecraftGame game)
        {
            BoundingFrustum frustum;
            while (true)
            {
                if (game.status == GameStatus.Quiting || game.status == GameStatus.Menu)
                {
                    return;
                }
               
                Thread.Sleep(50);
                if (player.isChunkNeededUpdate == true)
                {
                //    Debug.WriteLine("update");
               
                 frustum = new BoundingFrustum(player.cam.viewMatrix * player.cam.projectionMatrix);
                for(float x = player.playerPos.X - GameOptions.renderDistance; x < player.playerPos.X + GameOptions.renderDistance; x += Chunk.chunkWidth)
                {
                    for (float z = player.playerPos.Z - GameOptions.renderDistance; z < player.playerPos.Z + GameOptions.renderDistance; z += Chunk.chunkWidth)
                    {
                        Vector2Int chunkPos = Vec3ToChunkPos(new Vector3(x, 0, z));
                        if (GetChunk(chunkPos) == null) {
                            BoundingBox chunkBoundingBox = new BoundingBox(new Vector3(chunkPos.x, 0, chunkPos.y), new Vector3(chunkPos.x + Chunk.chunkWidth, Chunk.chunkHeight, chunkPos.y + Chunk.chunkWidth));
                            if (frustum.Intersects(chunkBoundingBox))
                            {
                                Chunk c = new Chunk(chunkPos, game.GraphicsDevice);
                            //    break;
                            }
                            else continue;
                               
                            
                        }
                        else continue;
                 
                    }
                }
                player.isChunkNeededUpdate = false;
                
                }
            }
        }
        public static void TryDeleteChunksThread( GamePlayer player,MinecraftGame game)
        {
            while (true) {

                if (game.status == GameStatus.Quiting||game.status==GameStatus.Menu)
                {
                    return;
                }
                Thread.Sleep(150);
              if (ChunkRenderer.isBusy == true)
                {
                    continue;
                }

                foreach (Chunk c in ChunkManager.chunks.Values)
                {
                   c.semaphore.Wait();
                    if ((MathF.Abs(c.chunkPos.x - player.playerPos.X )> (GameOptions.renderDistance + Chunk.chunkWidth )||MathF.Abs( c.chunkPos.y - player.playerPos.Z) > (GameOptions.renderDistance + Chunk.chunkWidth))
                        &&(c.isReadyToRender==true&&c.isTaskCompleted==true)&&c.usedByOthersCount<=0
                    /*    && (c.Value.leftChunk==null||(c.Value.leftChunk!=null&&c.Value.leftChunk.isTaskCompleted == true))
                        && (c.Value.rightChunk == null || (c.Value.rightChunk != null && c.Value.rightChunk.isTaskCompleted == true))
                        && (c.Value.frontChunk == null || (c.Value.frontChunk != null && c.Value.frontChunk.isTaskCompleted == true))
                        && (c.Value.backChunk == null || (c.Value.backChunk != null && c.Value.backChunk.isTaskCompleted == true))*/
                        )
                    {
                        // Chunk c2;
                            
                            c.isReadyToRender = false;
                            c.SaveSingleChunk();
                            c.Dispose();
                            
                            ChunkManager.chunks.TryRemove(new KeyValuePair<Vector2Int,Chunk>(c.chunkPos,c));
                      
                           
                 
                    }

                    c.semaphore.Release();

                }
               
            }
        }
        public static short GetBlock(Vector3 pos)
        {
            Vector3Int intPos = Vector3Int.FloorToIntVec3(pos);
            Chunk chunkNeededUpdate = ChunkManager.GetChunk(ChunkManager.Vec3ToChunkPos(pos));
          
            if (chunkNeededUpdate == null || chunkNeededUpdate.isMapGenCompleted == false)
            { 
                return 1;
            }
           
            Vector3Int chunkSpacePos = intPos - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
            if (chunkSpacePos.x >= 0 && chunkSpacePos.x < Chunk.chunkWidth && chunkSpacePos.y < Chunk.chunkHeight && chunkSpacePos.y >= 0 && chunkSpacePos.z >= 0 && chunkSpacePos.z < Chunk.chunkWidth)
            {
                return chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z];
            }
            else
            {
                return 0;
            }
            
            

        }
        public static short RaycastFirstBlockID(Ray ray,float distance)
        {
            for(float i = 0; i < distance; i += 0.1f)
            {
                Vector3 blockPoint=ray.origin+ ray.direction*i;
                short blockID = ChunkManager.GetBlock(blockPoint);
                if (blockID != 0)
                {
                    return blockID; 
                }
            }
            return 0;
        }
        public static Vector3 RaycastFirstPosition(Ray ray,float distance)
        {
            for (float i = 0; i < distance; i += 0.01f)
            {
                Vector3 blockPoint = ray.origin + ray.direction * i;
                short blockID = ChunkManager.GetBlock(blockPoint);
                if (blockID != 0)
                {
                    return blockPoint;
                }
            }
            return new Vector3(1024, 0, 1024);
        }
        public void BreakBlockAtPoint(Vector3 blockPoint)
        {



            SetBlockWithUpdate(blockPoint, 0);

        }

       
        public static int GetChunkLandingPoint(float x,float z) 
        {
            Vector2Int intPos = new Vector2Int((int)x, (int)z);
            Chunk locChunk=GetChunk(Vec3ToChunkPos(new Vector3(x,0,z)));
            if (locChunk == null||locChunk.isMapGenCompleted==false)
            {
              
                return 100;
            }
            Vector2Int chunkSpacePos = intPos - locChunk.chunkPos;
            chunkSpacePos.x = MathHelper.Clamp(chunkSpacePos.x, 0, Chunk.chunkWidth - 1);
            chunkSpacePos.y = MathHelper.Clamp(chunkSpacePos.y, 0, Chunk.chunkWidth - 1);
            for (int i=200;i> 0; i--)
            {
                if (locChunk.map[chunkSpacePos.x, i-1, chunkSpacePos.y] != 0)
                {
                    return i;
                }
            }
         
            return 100;

        }
        public static void SetBlockWithUpdate(Vector3 pos, short blockID)
        {

            Vector3Int intPos = new Vector3Int(ChunkManager.FloatToInt(pos.X), ChunkManager.FloatToInt(pos.Y), ChunkManager.FloatToInt(pos.Z));
            Chunk chunkNeededUpdate = ChunkManager.GetChunk(ChunkManager.Vec3ToChunkPos(pos));
            if (chunkNeededUpdate == null||chunkNeededUpdate.isReadyToRender==false)
            {
                return;
            } 
            Vector3Int chunkSpacePos = intPos - new Vector3Int(chunkNeededUpdate.chunkPos.x, 0, chunkNeededUpdate.chunkPos.y);
            if (chunkSpacePos.y < 0 || chunkSpacePos.y >= Chunk.chunkHeight)
            {
                return;
            }
                chunkNeededUpdate.map[chunkSpacePos.x, chunkSpacePos.y, chunkSpacePos.z] = blockID;
                chunkNeededUpdate.BuildChunk();
                chunkNeededUpdate.isModifiedInGame = true;
                if (chunkSpacePos.x == 0)
                {
                if (chunkNeededUpdate.leftChunk != null && chunkNeededUpdate.leftChunk.isMapGenCompleted == true)
                {
                    chunkNeededUpdate.leftChunk.BuildChunk();
                }
                }
                if (chunkSpacePos.x == Chunk.chunkWidth - 1)
                {
                if (chunkNeededUpdate.rightChunk != null && chunkNeededUpdate.rightChunk.isMapGenCompleted == true)
                    
                chunkNeededUpdate.rightChunk.BuildChunk();

                }
            if (chunkSpacePos.z == 0)
            {
            if (chunkNeededUpdate.backChunk != null && chunkNeededUpdate.backChunk.isMapGenCompleted == true)
                {
                    chunkNeededUpdate.backChunk.BuildChunk();
                }
            }
            if (chunkSpacePos.z == Chunk.chunkWidth - 1)
            {
            if (chunkNeededUpdate.frontChunk != null && chunkNeededUpdate.frontChunk.isMapGenCompleted == true)
                {
                    chunkNeededUpdate.frontChunk.BuildChunk();
                }
            }
                
              
                //   BlockModifyData b = new BlockModifyData(pos.X, pos.Y, pos.Z, blockID);
                //    Program.AppendMessage(null, new MessageProtocol(133, MessagePackSerializer.Serialize(b)));
            }
        public static void ReadJson()
        {
            chunkDataReadFromDisk.Clear();
            //   gameWorldDataPath = WorldManager.gameWorldDataPath;

            if (!Directory.Exists(gameWorldDataPath + "unityMinecraftServerData"))
            {
                Directory.CreateDirectory(gameWorldDataPath + "unityMinecraftServerData");

            }
            if (!Directory.Exists(gameWorldDataPath + "unityMinecraftServerData/GameData"))
            {
                Directory.CreateDirectory(gameWorldDataPath + "unityMinecraftServerData/GameData");
            }

            if (!File.Exists(gameWorldDataPath + "unityMinecraftServerData" + "/GameData/world.json"))
            {
                FileStream fs = File.Create(gameWorldDataPath + "unityMinecraftServerData" + "/GameData/world.json");
                fs.Close();
            }

            byte[] worldData = File.ReadAllBytes(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json");
            /*  List<ChunkData> tmpList = new List<ChunkData>();
              foreach (string s in worldData)
              {
                  ChunkData tmp = JsonConvert.DeserializeObject<ChunkData>(s);
                  tmpList.Add(tmp);
              }
              foreach (ChunkData w in tmpList)
              {
                  chunkDataReadFromDisk.Add(new Vector2Int(w.chunkPos.x, w.chunkPos.y), w);
              }*/
            if (worldData.Length > 0)
            {
                chunkDataReadFromDisk = MessagePackSerializer.Deserialize<Dictionary<Vector2Int, ChunkData>>(worldData);
            }

            isJsonReadFromDisk = true;
        }
    }
}
