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
        public static bool isJsonReadFromDisk { get; private set; }
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
        public static void UpdateWorldThread(int renderDistance, GamePlayer player)
        {
            while (true)
            {
             
                Thread.Sleep(50);
                BoundingFrustum frustum = new BoundingFrustum(player.cam.projectionMatrix * player.cam.viewMatrix);
                for(float x = player.playerPos.X - renderDistance; x < player.playerPos.X + renderDistance; x += Chunk.chunkWidth)
                {
                    for (float z = player.playerPos.Z - renderDistance; z < player.playerPos.Z + renderDistance; z += Chunk.chunkWidth)
                    {
                        Vector2Int chunkPos = Vec3ToChunkPos(new Vector3(x, 0, z));
                        if (GetChunk(chunkPos) == null) {
                         //   BoundingBox chunkBoundingBox = new BoundingBox(new Vector3(chunkPos.x, 0, chunkPos.y), new Vector3(chunkPos.x + Chunk.chunkWidth, Chunk.chunkHeight, chunkPos.y + Chunk.chunkWidth));
                         
                                Chunk c = new Chunk(chunkPos);
                            
                        }
                        else continue;
                 
                    }
                }
            }
        }
        public static void TryDeleteChunksThread(int renderDistance, GamePlayer player)
        {
            while (true) { 
                Thread.Sleep(50);
                if (ChunkRenderer.isBusy == true)
                {
                    continue;
                }
                foreach (var c in ChunkManager.chunks)
                {
                    if (MathF.Abs(c.Value.chunkPos.x - player.playerPos.X )> (renderDistance + Chunk.chunkWidth )||MathF.Abs( c.Value.chunkPos.y - player.playerPos.Z) > (renderDistance + Chunk.chunkWidth))
                    {
                            // Chunk c2;
                            c.Value.isReadyToRender = false;
                       c.Value.SaveSingleChunk();
                         c.Value.Dispose();
                        ChunkManager.chunks.TryRemove(c);
                   //     break;
                      //  c2
                    }
                
                }
               
            }
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
