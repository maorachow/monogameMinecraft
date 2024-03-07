using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public class EntityManager
    {
        public static string gameWorldEntityDataPath = AppDomain.CurrentDomain.BaseDirectory;
        public static Random randomGenerator= new Random();
        public static void UpdateAllEntity(float deltaTime)
        {
            for(int i=0;i<EntityBeh.worldEntities.Count;i++)
            {
                EntityBeh.worldEntities[i].OnUpdate(deltaTime);
            }
        }
        public static void TrySpawnNewZombie(MinecraftGame game,float deltaTime)
        {
            if (randomGenerator.NextSingle() >= 1-deltaTime && EntityBeh.worldEntities.Count < 70)
            {
                Vector2 randSpawnPos = new Vector2(game.gamePlayer.playerPos.X+(randomGenerator.NextSingle() - 0.5f) * 80f, game.gamePlayer.playerPos.Z + (randomGenerator.NextSingle() - 0.5f) * 80f);
                Vector3 spawnPos = new Vector3(randSpawnPos.X, ChunkManager.GetChunkLandingPoint(randSpawnPos.X, randSpawnPos.Y), randSpawnPos.Y);
                EntityBeh.SpawnNewEntity(spawnPos + new Vector3(0f, 1f, 0f), 0f, 0f, 0f, 0, game);

            }
        }
   
        public static void ReadEntityData()
        {
            EntityBeh.worldEntities.Clear();
            //   gameWorldDataPath = WorldManager.gameWorldDataPath;

            if (!Directory.Exists(gameWorldEntityDataPath + "unityMinecraftServerData"))
            {
                Directory.CreateDirectory(gameWorldEntityDataPath + "unityMinecraftServerData");

            }
            if (!Directory.Exists(gameWorldEntityDataPath + "unityMinecraftServerData/GameData"))
            {
                Directory.CreateDirectory(gameWorldEntityDataPath + "unityMinecraftServerData/GameData");
            }

            if (!File.Exists(gameWorldEntityDataPath + "unityMinecraftServerData" + "/GameData/worldentities.json"))
            {
                FileStream fs = File.Create(gameWorldEntityDataPath + "unityMinecraftServerData" + "/GameData/worldentities.json");
                fs.Close();
            }

            byte[] worldData = File.ReadAllBytes(gameWorldEntityDataPath + "unityMinecraftServerData/GameData/worldentities.json");
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
               EntityBeh. entityDataReadFromDisk = MessagePackSerializer.Deserialize<List<EntityData>>(worldData);
            }

           
        }
        public static void SaveWorldEntityData()
        {

            FileStream fs;
            if (File.Exists(gameWorldEntityDataPath + "unityMinecraftServerData/GameData/worldentities.json"))
            {
                fs = new FileStream(gameWorldEntityDataPath + "unityMinecraftServerData/GameData/worldentities.json", FileMode.Truncate, FileAccess.Write); 
            }
            else
            {
                fs = new FileStream(gameWorldEntityDataPath + "unityMinecraftServerData/GameData/worldentities.json", FileMode.Create, FileAccess.Write);
            }
            fs.Close();

            foreach (EntityBeh e in EntityBeh.worldEntities)
            {
                e.SaveSingleEntity();
            }
            //   Debug.Log(entityDataReadFromDisk.Count);
            /*  foreach(EntityData ed in entityDataReadFromDisk){
               string tmpData=JsonSerializer.ToJsonString(ed);
               File.AppendAllText(gameWorldEntityDataPath+"unityMinecraftData/GameData/worldentities.json",tmpData+"\n");
              }*/
            byte[] tmpData = MessagePackSerializer.Serialize(EntityBeh.entityDataReadFromDisk);
            File.WriteAllBytes(gameWorldEntityDataPath + "unityMinecraftServerData/GameData/worldentities.json", tmpData);
            
        }
    }

    [MessagePackObject]
    public struct EntityData
    {
        [Key(0)]
        public int typeid;
        [Key(1)]
        public float posX;
        [Key(2)]
        public float posY;
        [Key(3)]
        public float posZ;
        [Key(4)]
        public float rotX;
        [Key(5)]
        public float rotY;
        [Key(6)]
        public float rotZ;
        [Key(7)]
        public string entityID;
        [Key(8)]
        public float entityHealth;
      

        public EntityData(int typeid, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, string entityID, float entityHealth )
        {
            this.typeid = typeid;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            this.entityID = entityID;
            this.entityHealth = entityHealth;
           
        }
    }
}
