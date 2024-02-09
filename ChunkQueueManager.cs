using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace monogameMinecraft
{
    public class ChunkQueueManager
    {
      //  public static Queue<Vector2Int> chunkLoadingQueue = new Queue<Vector2Int>();
        public static Queue<Chunk> chunkUnloadingQueue = new Queue<Chunk>();
        public static void UnloadChunksInQueue()
        {
            while (true)
            {
               Thread.Sleep(50);
            while(chunkUnloadingQueue.Count > 0)
            {
                Chunk c=chunkUnloadingQueue.First();
                if(c != null)
                {
                   
                    c.isReadyToRender = false;
                    c.SaveSingleChunk();
                    c.Dispose();
                    ChunkManager.chunks.TryRemove(new KeyValuePair<Vector2Int,Chunk>(c.chunkPos,c));
                }
            }
            }
            
        }

    }
}
