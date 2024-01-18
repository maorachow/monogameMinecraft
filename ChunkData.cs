using MessagePack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace monogameMinecraft
{
    [MessagePackObject]
    public class ChunkData
    {
        [Key(0)]
        public short[,,] map;
        [Key(1)]
        public Vector2Int chunkPos = new Vector2Int(0, 0);
    
        public ChunkData(short[,,] map, Vector2Int chunkPos)
        {
            this.map = map;
            this.chunkPos = chunkPos;
        }
        public ChunkData(Vector2Int chunkPos)
        {
            //  this.map = map;
            this.chunkPos = chunkPos;
        }
    }
}
