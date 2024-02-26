using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public class EntityManager
    {
        public static Random randomGenerator= new Random();
        public static void UpdateAllEntity(float deltaTime)
        {
            for(int i=0;i<EntityBeh.worldEntities.Count;i++)
            {
                EntityBeh.worldEntities[i].OnUpdate(deltaTime);
            }
        }
        public static void TrySpawnNewZombie(MinecraftGame game)
        {
            if (randomGenerator.NextSingle() >= 0.998 && EntityBeh.worldEntities.Count < 70)
            {
                Vector2 randSpawnPos = new Vector2(game.gamePlayer.playerPos.X+(randomGenerator.NextSingle() - 0.5f) * 80f, game.gamePlayer.playerPos.Z + (randomGenerator.NextSingle() - 0.5f) * 80f);
                Vector3 spawnPos = new Vector3(randSpawnPos.X, ChunkManager.GetChunkLandingPoint(randSpawnPos.X, randSpawnPos.Y), randSpawnPos.Y);
                EntityBeh.SpawnNewEntity(spawnPos + new Vector3(0f, 1f, 0f), 0f, 0f, 0f, 0, game);

            }
        }
    }
}
