using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MessagePack;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Net.Sockets;
using System.Diagnostics;
using monogameMinecraft;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

[MessagePackObject]
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        [Key(0)]
        public int x;
        [Key(1)]
        public int y;
        [SerializationConstructor]
        public Vector2Int(int a, int b)
        {
            x = a;
            y = b;
        }
        [IgnoreMember]
        public float magnitude { get { return MathF.Sqrt((float)(x * x + y * y)); } }

        // Returns the squared length of this vector (RO).
        [IgnoreMember]
        public int sqrMagnitude { get { return x * x + y * y; } }
        public override bool Equals(object other)
        {
            if (!(other is Vector2Int)) return false;

            return Equals((Vector2Int)other);
        }
        public bool Equals(Vector2Int other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }


        public static Vector2Int operator -(Vector2Int v)
        {
            return new Vector2Int(-v.x, -v.y);
        }


        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }


        public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x * b.x, a.y * b.y);
        }


        public static Vector2Int operator *(int a, Vector2Int b)
        {
            return new Vector2Int(a * b.x, a * b.y);
        }


        public static Vector2Int operator *(Vector2Int a, int b)
        {
            return new Vector2Int(a.x * b, a.y * b);
        }


        public static Vector2Int operator /(Vector2Int a, int b)
        {
            return new Vector2Int(a.x / b, a.y / b);
        }


        public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
        {
            return !(lhs == rhs);
        }

    }
    [MessagePackObject]
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        [Key(0)]
        public int x;
        [Key(1)]
        public int y;
        [Key(2)]
        public int z;

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static int FloorToInt(float f) { return (int)Math.Floor(f); }
        public static Vector3Int FloorToIntVec3(Vector3 v)
        {
            return new Vector3Int(
                FloorToInt(v.X),
                FloorToInt(v.Y),
               FloorToInt(v.Z)
            );
        }
        public static Vector3Int operator +(Vector3Int b, Vector3Int c)
        {
            Vector3Int v = new Vector3Int(b.x + c.x, b.y + c.y, b.z + c.z);
            return v;
        }
        public static Vector3Int operator -(Vector3Int b, Vector3Int c)
        {
            Vector3Int v = new Vector3Int(b.x - c.x, b.y - c.y, b.z - c.z);
            return v;
        }
        public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }


        public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
        {
            return !(lhs == rhs);
        }


        public override bool Equals(object other)
        {
            if (!(other is Vector3Int)) return false;

            return Equals((Vector3Int)other);
        }


        public override int GetHashCode()
        {
            var yHash = y.GetHashCode();
            var zHash = z.GetHashCode();
            return x.GetHashCode() ^ (yHash << 4) ^ (yHash >> 28) ^ (zHash >> 4) ^ (zHash << 28);
        }
        public bool Equals(Vector3Int other)
        {
            return this == other;
        }

    }
    public struct RandomGenerator3D
    {
        //  public System.Random rand=new System.Random(0);
        public static int GenerateIntFromVec3(Vector3Int pos)
        {
            System.Random rand = new System.Random(pos.x * pos.y * pos.z * 100);
            return rand.Next(100);
        }
    }
    [MessagePackObject]
    public class Chunk:IDisposable
    {
        [IgnoreMember]
        public static FastNoise noiseGenerator = new FastNoise();
        [IgnoreMember]
        public static FastNoise biomeNoiseGenerator = new FastNoise();
        [IgnoreMember]
        public static int worldGenType = 0;//1 superflat 0 inf
        [IgnoreMember]
        public static int chunkWidth = 16;
        [IgnoreMember]
        public static int chunkHeight = 256;
        [Key(0)]
        public short[,,] map;
        [IgnoreMember]
        public short[,,] additiveMap = new short[chunkWidth, chunkHeight, chunkWidth];
        [Key(1)]
        public Vector2Int chunkPos = new Vector2Int(0, 0);
        [IgnoreMember]
        public bool isChunkDataSavedInDisk = false;
        public Chunk(Vector2Int chunkPos)
        {
            this.chunkPos = chunkPos;
            ChunkManager.chunks.TryAdd(chunkPos, this);
            isReadyToRender = false;
            BuildChunk();
        }
    public bool isMapGenCompleted=false;
    public static Dictionary<int, List<Vector2>> blockInfo = new Dictionary<int, List<Vector2>> {
        { 1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)} },
     {2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)}},
 {3, new List<Vector2> { new Vector2(0.125f, 0f), new Vector2(0.125f, 0f), new Vector2(0.125f, 0f), new Vector2(0.125f, 0f), new Vector2(0.125f, 0f), new Vector2(0.125f, 0f) }},
 {4, new List<Vector2> { new Vector2(0.1875f, 0f), new Vector2(0.1875f, 0f), new Vector2(0.125f, 0f), new Vector2(0.0625f, 0f), new Vector2(0.1875f, 0f), new Vector2(0.1875f, 0f) }},
 {100, new List<Vector2> { new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f), new Vector2(0f, 0.0625f) }},
 {101, new List<Vector2> { new Vector2(0f, 0.0625f) } },
 {5, new List<Vector2> { new Vector2(0.375f, 0f), new Vector2(0.375f, 0f), new Vector2(0.375f, 0f), new Vector2(0.375f, 0f), new Vector2(0.375f, 0f), new Vector2(0.375f, 0f) }},
 {6, new List<Vector2> { new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f) }},
 {7, new List<Vector2> { new Vector2(0.3125f, 0f), new Vector2(0.3125f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.3125f, 0f), new Vector2(0.3125f, 0f) }},
 {8, new List<Vector2> { new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f) }},
 {9, new List<Vector2> { new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f), new Vector2(0.4375f, 0f) }},
 {10, new List<Vector2> { new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f), new Vector2(0.5625f, 0f) }},
 {11, new List<Vector2> { new Vector2(0.625f, 0f), new Vector2(0.625f, 0f), new Vector2(0.625f, 0f), new Vector2(0.625f, 0f), new Vector2(0.625f, 0f), new Vector2(0.625f, 0f) }}
    
    
    };
        
        public static System.Random worldRandomGenerator = new System.Random(0);
        [IgnoreMember]
        public Chunk frontChunk;
        [IgnoreMember]
        public Chunk backChunk;
        [IgnoreMember]
        public Chunk leftChunk;
        [IgnoreMember]
        public Chunk rightChunk;
        [IgnoreMember]
        public Chunk frontLeftChunk;
        [IgnoreMember]
        public Chunk frontRightChunk;
        [IgnoreMember]
        public Chunk backLeftChunk;
        [IgnoreMember]
        public Chunk backRightChunk;
        [IgnoreMember]
        public static int chunkSeaLevel = 63;
        public ChunkData ChunkToChunkData()
        {
            return new ChunkData(this.map, this.chunkPos);
        }
        public int updateCount = 0;
        public bool BFSIsWorking = false;
        public bool[,,] mapIsSearched;

        public bool isModifiedInGame;
        /* public void BFSInit(int x, int y, int z, int ignoreSide, int GainedUpdateCount)
        {
            updateCount = GainedUpdateCount;
            mapIsSearched = new bool[chunkWidth + 2, chunkHeight + 2, chunkWidth + 2];
            BFSIsWorking = true;
            Task.Run(() => BFSMapUpdate(x, y, z, ignoreSide));
        }
       public async void BFSMapUpdate(int x, int y, int z, int ignoreSide)
        {
            //left right bottom top back front
            //left x-1 right x+1 top y+1 bottom y-1 back z-1 front z+1
            // Task.Delay(30);
            if (!BFSIsWorking)
            {
                return;
            }
            if (updateCount > 64)
            {

                //       Program.CastToAllClients(new Message("WorldData", MessagePackSerializer.Serialize(this.ChunkToChunkData())));
                BFSIsWorking = false;
                return;
            }

            mapIsSearched[x, y, z] = true;

            try
            {
                if (GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 101 && GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
                {
                    BreakBlockAtPoint(new Vector3(chunkPos.x + x, y, chunkPos.y + z));
                }
                if (GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 && GetBlock(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z)) == 0)
                {
                    SetBlockWithUpdate(new Vector3(chunkPos.x + x, y - 1, chunkPos.y + z), 100);
                }

                if (GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 && GetBlock(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z)) == 0)
                {
                    SetBlockWithUpdate(new Vector3(chunkPos.x + x - 1, y, chunkPos.y + z), 100);
                }
                if (GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 && GetBlock(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z)) == 0)
                {
                    SetBlockWithUpdate(new Vector3(chunkPos.x + x + 1, y, chunkPos.y + z), 100);
                }
                if (GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 && GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1)) == 0)
                {
                    SetBlockWithUpdate(new Vector3(chunkPos.x + x, y, chunkPos.y + z - 1), 100);
                }
                if (GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z)) == 100 && GetBlock(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1)) == 0)
                {
                    SetBlockWithUpdate(new Vector3(chunkPos.x + x, y, chunkPos.y + z + 1), 100);
                }
            }
            catch
            {
                Console.WriteLine("outbound update");
            }

            updateCount++;
            if (!(ignoreSide == 0) && x - 1 >= 0)
            {
                try
                {
                    if (!mapIsSearched[x - 1, y, z] && map[x - 1, y, z] != 0)
                        Task.Run(() => BFSMapUpdate(x - 1, y, z, ignoreSide));
                }
                catch
                {
                    Console.WriteLine("outbound update");
                }

            }
            else if (x - 1 < 0)
            {

                if (leftChunk != null)
                {
                    leftChunk.BFSInit(chunkWidth - 1, y, z, ignoreSide, updateCount);
                }
            }
            if (!(ignoreSide == 1) && x + 1 < chunkWidth)
            {
                try
                {
                    if (!mapIsSearched[x + 1, y, z] && map[x + 1, y, z] != 0)
                        Task.Run(() => BFSMapUpdate(x + 1, y, z, ignoreSide));
                }
                catch
                {
                    Console.WriteLine("outbound update");
                }
            }
            else if (x + 1 >= chunkWidth)
            {
                if (rightChunk != null)
                {
                    rightChunk.BFSInit(0, y, z, ignoreSide, updateCount);
                }
            }
            if (!(ignoreSide == 2) && y - 1 >= 0)
            {
                try
                {
                    if (!mapIsSearched[x, y - 1, z] && map[x, y - 1, z] != 0)
                        Task.Run(() => BFSMapUpdate(x, y - 1, z, ignoreSide));

                }
                catch
                {
                    Console.WriteLine("outbound update");
                }
            }
            if (!(ignoreSide == 3) && y + 1 < chunkHeight)
            {
                try
                {
                    if (!mapIsSearched[x, y + 1, z] && map[x, y + 1, z] != 0)
                        Task.Run(() => BFSMapUpdate(x, y + 1, z, ignoreSide));
                }
                catch
                {
                    Console.WriteLine("outbound update");
                }
            }
            if (!(ignoreSide == 4) && z - 1 >= 0)
            {
                try
                {
                    if (!mapIsSearched[x, y, z - 1] && map[x, y, z - 1] != 0)
                        Task.Run(() => BFSMapUpdate(x, y, z - 1, ignoreSide));
                }
                catch { Console.WriteLine("outbound update"); }
            }
            else if (z - 1 < 0)
            {
                if (backChunk != null)
                {
                    backChunk.BFSInit(x, y, chunkWidth - 1, ignoreSide, updateCount);
                }
            }
            if (!(ignoreSide == 5) && z + 1 < chunkWidth)
            {
                try
                {
                    if (!mapIsSearched[x, y, z + 1] && map[x, y, z + 1] != 0)
                        Task.Run(() => BFSMapUpdate(x, y, z + 1, ignoreSide));
                }
                catch
                {
                    Console.WriteLine("outbound update");
                }
            }
            else if (z + 1 >= chunkWidth)
            {
                if (frontChunk != null)
                {
                    frontChunk.BFSInit(x, y, 0, ignoreSide, updateCount);
                }
            }

        }*/
        
    public void SaveSingleChunk()
        {

            if (!isModifiedInGame)
            {

                return;
            }
            if (ChunkManager.chunkDataReadFromDisk.ContainsKey(chunkPos))
            {
                ChunkManager.chunkDataReadFromDisk.Remove(chunkPos);
                short[,,] worldDataMap = map;
                ChunkData wd = new ChunkData(chunkPos);
                wd.map = worldDataMap;

                ChunkManager.chunkDataReadFromDisk.Add(chunkPos, wd);
            }
            else
            {
            short[,,] worldDataMap = map;
                ChunkData wd = new ChunkData(chunkPos);
                wd.map = worldDataMap;

                ChunkManager.chunkDataReadFromDisk.Add(chunkPos, wd);
            }
        }
        
        public static int[,] GenerateChunkBiomeMap(Vector2Int pos)
        {
            //   float[,] biomeMap=new float[chunkWidth/8+2,chunkWidth/8+2];//插值算法
            //      int[,] chunkBiomeMap=GenerateChunkBiomeMap(pos);
            int[,] biomeMapInter = new int[chunkWidth / 8 + 2, chunkWidth / 8 + 2];
            for (int i = 0; i < chunkWidth / 8 + 2; i++)
            {
                for (int j = 0; j < chunkWidth / 8 + 2; j++)
                {
                    //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                    //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                    biomeMapInter[i, j] = (int)(1f + biomeNoiseGenerator.GetSimplex(pos.x + (i - 1) * 8, pos.y + (j - 1) * 8) * 3f);
                }
            }//32,32



            return biomeMapInter;
        }
    public float[,] thisHeightMap;
        public static float[,] GenerateChunkHeightmap(Vector2Int pos)
        {
            float[,] heightMap = new float[chunkWidth / 8 + 2, chunkWidth / 8 + 2];//插值算法
            int[,] chunkBiomeMap = GenerateChunkBiomeMap(pos);

            for (int i = 0; i < chunkWidth / 8 + 2; i++)
            {
                for (int j = 0; j < chunkWidth / 8 + 2; j++)
                {
                    //           Debug.DrawLine(new Vector3(pos.x+(i-1)*8,60f,pos.y+(j-1)*8),new Vector3(pos.x+(i-1)*8,150f,pos.y+(j-1)*8),Color.green,1f);
                    //    if(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int()))

                    heightMap[i, j] = chunkSeaLevel + noiseGenerator.GetSimplex(pos.x + (i - 1) * 8, pos.y + (j - 1) * 8) * 20f + chunkBiomeMap[i, j] * 25f;
                }

            }//32,32
            int interMultiplier = 8;
            float[,] heightMapInterpolated = new float[(chunkWidth / 8 + 2) * interMultiplier, (chunkWidth / 8 + 2) * interMultiplier];
            for (int i = 0; i < (chunkWidth / 8 + 2) * interMultiplier; ++i)
            {
                for (int j = 0; j < (chunkWidth / 8 + 2) * interMultiplier; ++j)
                {
                    int x = i;
                    int y = j;
                    float x1 = (i / interMultiplier) * interMultiplier;
                    float x2 = (i / interMultiplier) * interMultiplier + interMultiplier;
                    float y1 = (j / interMultiplier) * interMultiplier;
                    float y2 = (j / interMultiplier) * interMultiplier + interMultiplier;
                    int x1Ori = (i / interMultiplier);
                    // Debug.Log(x1Ori);
                    int x2Ori = (i / interMultiplier) + 1;
                    x2Ori = Math.Clamp(x2Ori, 0, (chunkWidth / 8 + 2) - 1);
                    //   Debug.Log(x2Ori);
                    int y1Ori = (j / interMultiplier);
                    //   Debug.Log(y1Ori);
                    int y2Ori = (j / interMultiplier) + 1;
                    y2Ori = Math.Clamp(y2Ori, 0, (chunkWidth / 8 + 2) - 1);
                    //     Debug.Log(y2Ori);

                    float q11 = heightMap[x1Ori, y1Ori];
                    float q12 = heightMap[x1Ori, y2Ori];
                    float q21 = heightMap[x2Ori, y1Ori];
                    float q22 = heightMap[x2Ori, y2Ori];
                    float fxy1 = (float)(x2 - x) / (x2 - x1) * q11 + (float)(x - x1) / (x2 - x1) * q21;
                    float fxy2 = (float)(x2 - x) / (x2 - x1) * q12 + (float)(x - x1) / (x2 - x1) * q22;
                    float fxy = (float)(y2 - y) / (y2 - y1) * fxy1 + (float)(y - y1) / (y2 - y1) * fxy2;
                    heightMapInterpolated[x, y] = fxy;
                    //       Debug.Log(fxy);
                    //    Debug.Log(x1);
                    //  Debug.Log(x2);

                }
            }

            return heightMapInterpolated;
        }



      
        public async Task InitMap(Vector2Int chunkPos)
        {
            this.chunkPos = chunkPos;
            thisHeightMap = GenerateChunkHeightmap(chunkPos);
        
        //    map = additiveMap;
            verticesOpq = new List<VertexPositionNormalTexture>();
            verticesNS = new List<VertexPositionNormalTexture>();
            verticesWT = new List<VertexPositionNormalTexture>();
            indicesOpq = new List<ushort>();
            indicesNS = new List<ushort>();
            indicesWT = new List<ushort>();
            frontChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + chunkWidth));
            frontLeftChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x - chunkWidth, chunkPos.y + chunkWidth));
            frontRightChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x + chunkWidth, chunkPos.y + chunkWidth));
            backLeftChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x - chunkWidth, chunkPos.y - chunkWidth));
            backRightChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x + chunkWidth, chunkPos.y - chunkWidth));
            backChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - chunkWidth));

            leftChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x - chunkWidth, chunkPos.y));
            
            rightChunk = ChunkManager.GetChunk(new Vector2Int(chunkPos.x + chunkWidth, chunkPos.y));

          
        if (isMapGenCompleted == true)
        {
            GenerateMesh(verticesOpq, verticesNS, verticesWT,indicesOpq,indicesNS,indicesWT);
            return;
        }
        if (ChunkManager.chunkDataReadFromDisk.ContainsKey(chunkPos))
            {
            isChunkDataSavedInDisk = true;
            map =   (short[,,])ChunkManager.chunkDataReadFromDisk[chunkPos].map.Clone();
            GenerateMesh(verticesOpq, verticesNS, verticesWT, indicesOpq, indicesNS, indicesWT);
            isMapGenCompleted = true;
            return;
            }

           
            if (worldGenType == 1)
            {
                for (int i = 0; i < chunkWidth; i++)
                {
                    for (int j = 0; j < chunkWidth; j++)
                    {
                        for (int k = 0; k < chunkHeight / 4; k++)
                        {
                            map[i, k, j] = 1;
                        }
                    }
                }
            isMapGenCompleted = true;
            GenerateMesh(verticesOpq, verticesNS, verticesWT, indicesOpq, indicesNS, indicesWT);
            }
            else
            {
                FreshGenMap(chunkPos);
            GenerateMesh(verticesOpq, verticesNS, verticesWT, indicesOpq, indicesNS, indicesWT);
            }
            void FreshGenMap(Vector2Int pos)
            {
            if (isChunkDataSavedInDisk == true)
            {

            }
                map = additiveMap;
                if (worldGenType == 0)
                {
                    bool isFrontLeftChunkUpdated = false;
                    bool isFrontRightChunkUpdated = false;
                    bool isBackLeftChunkUpdated = false;
                    bool isBackRightChunkUpdated = false;
                    bool isLeftChunkUpdated = false;
                    bool isRightChunkUpdated = false;
                    bool isFrontChunkUpdated = false;
                    bool isBackChunkUpdated = false;
                    //    System.Random random=new System.Random(pos.x+pos.y);
                    int treeCount = 10;

                    int[,] chunkBiomeMap = GenerateChunkBiomeMap(pos);

                    int interMultiplier = 8;
                    int[,] chunkBiomeMapInterpolated = new int[(chunkWidth / 8 + 2) * interMultiplier, (chunkWidth / 8 + 2) * interMultiplier];
                    for (int i = 0; i < (chunkWidth / 8 + 2) * interMultiplier; ++i)
                    {
                        for (int j = 0; j < (chunkWidth / 8 + 2) * interMultiplier; ++j)
                        {
                            int x = i;
                            int y = j;
                            float x1 = (i / interMultiplier) * interMultiplier;
                            float x2 = (i / interMultiplier) * interMultiplier + interMultiplier;
                            float y1 = (j / interMultiplier) * interMultiplier;
                            float y2 = (j / interMultiplier) * interMultiplier + interMultiplier;
                            int x1Ori = (i / interMultiplier);
                            // Debug.Log(x1Ori);
                            int x2Ori = (i / interMultiplier) + 1;
                            x2Ori = Math.Clamp(x2Ori, 0, (chunkWidth / 8 + 2) - 1);
                            //   Debug.Log(x2Ori);
                            int y1Ori = (j / interMultiplier);
                            //   Debug.Log(y1Ori);
                            int y2Ori = (j / interMultiplier) + 1;
                            y2Ori = Math.Clamp(y2Ori, 0, (chunkWidth / 8 + 2) - 1);
                            //     Debug.Log(y2Ori);

                            float q11 = chunkBiomeMap[x1Ori, y1Ori];
                            float q12 = chunkBiomeMap[x1Ori, y2Ori];
                            float q21 = chunkBiomeMap[x2Ori, y1Ori];
                            float q22 = chunkBiomeMap[x2Ori, y2Ori];
                            float fxy1 = (float)(x2 - x) / (x2 - x1) * q11 + (float)(x - x1) / (x2 - x1) * q21;
                            float fxy2 = (float)(x2 - x) / (x2 - x1) * q12 + (float)(x - x1) / (x2 - x1) * q22;
                            int fxy = (int)((int)(y2 - y) / (y2 - y1) * fxy1 + (int)(y - y1) / (y2 - y1) * fxy2);
                            chunkBiomeMapInterpolated[x, y] = fxy;
                            //       Debug.Log(fxy);
                            //    Debug.Log(x1);
                            //  Debug.Log(x2);

                        }
                    }
                    for (int i = 0; i < chunkWidth; i++)
                    {
                        for (int j = 0; j < chunkWidth; j++)
                        {
                            //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.y*0.01f+j*0.01f);
                            float noiseValue = thisHeightMap[i + 8, j + 8];
                            for (int k = 0; k < chunkHeight; k++)
                            {
                                if (noiseValue > k + 3)
                                {
                                    map[i, k, j] = 1;
                                }
                                else if (noiseValue > k)
                                {
                                    if (chunkBiomeMapInterpolated[i + 8, j + 8] == 3)
                                    {
                                        map[i, k, j] = 1;
                                    }
                                    else
                                    {
                                        map[i, k, j] = 3;
                                    }

                                }


                            }
                        }
                    }


                    for (int i = 0; i < chunkWidth; i++)
                    {
                        for (int j = 0; j < chunkWidth; j++)
                        {

                            for (int k = chunkHeight - 1; k >= 0; k--)
                            {

                                if (map[i, k, j] != 0 && k >= chunkSeaLevel)
                                {
                                    map[i, k, j] = 4;
                                    break;
                                }

                           //     if (k > chunkSeaLevel && map[i, k, j] == 0 && map[i, k - 1, j] != 0 && map[i, k - 1, j] != 100 && worldRandomGenerator.Next(100) > 80)
                           //     {
                            //        map[i, k, j] = 101;
                           //     }
                                if (k < chunkSeaLevel && map[i, k, j] == 0)
                                {
                                    map[i, k, j] = 100;
                                }

                            }
                        }
                    }

                    for (int i = 0; i < chunkWidth; i++)
                    {
                        for (int j = 0; j < chunkWidth; j++)
                        {

                            for (int k = chunkHeight - 1; k >= 0; k--)
                            {

                                if (k > chunkSeaLevel && map[i, k, j] == 0 && map[i, k - 1, j] == 4 && map[i, k - 1, j] != 100)
                                {
                                    if (treeCount > 0)
                                    {
                                        //         Console.WriteLine(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(i, k, j)));
                                        if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(i, k, j)) > 98)
                                        {


                                            for (int x = -2; x < 3; x++)
                                            {
                                                for (int y = 3; y < 5; y++)
                                                {
                                                    for (int z = -2; z < 3; z++)
                                                    {
                                                        if (x + i < 0 || x + i >= chunkWidth || z + j < 0 || z + j >= chunkWidth)
                                                        {



                                                            if (x + i < 0)
                                                            {
                                                                if (z + j >= 0 && z + j < chunkWidth)
                                                                {
                                                                    if (leftChunk != null)
                                                                    {

                                                                        leftChunk.additiveMap[chunkWidth + (x + i), y + k, z + j] = 9;

                                                                        isLeftChunkUpdated = true;

                                                                        //    WorldManager.chunkLoadingQueue.UpdatePriority(leftChunk,0);
                                                                        //         leftChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }
                                                                else if (z + j < 0)
                                                                {
                                                                    if (backLeftChunk != null)
                                                                    {
                                                                        backLeftChunk.additiveMap[chunkWidth + (x + i), y + k, chunkWidth - 1 + (z + j)] = 9;

                                                                        isBackLeftChunkUpdated = true;

                                                                        //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                                                        //               backLeftChunk.isChunkMapUpdated=true;
                                                                    }

                                                                }
                                                                else if (z + j >= chunkWidth)
                                                                {
                                                                    if (frontLeftChunk != null)
                                                                    {
                                                                        frontLeftChunk.additiveMap[chunkWidth + (x + i), y + k, (z + j) - chunkWidth] = 9;

                                                                        isFrontLeftChunkUpdated = true;

                                                                        //     WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                                                        //                 frontLeftChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }

                                                            }
                                                            else
                                                            if (x + i >= chunkWidth)
                                                            {
                                                                if (z + j >= 0 && z + j < chunkWidth)
                                                                {
                                                                    if (rightChunk != null)
                                                                    {

                                                                        rightChunk.additiveMap[(x + i) - chunkWidth, y + k, z + j] = 9;

                                                                        isRightChunkUpdated = true;

                                                                        //   WorldManager.chunkLoadingQueue.UpdatePriority(rightChunk,0);
                                                                        //      rightChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }
                                                                else if (z + j < 0)
                                                                {
                                                                    if (backRightChunk != null)
                                                                    {
                                                                        backRightChunk.additiveMap[(x + i) - chunkWidth, y + k, chunkWidth + (z + j)] = 9;

                                                                        isBackRightChunkUpdated = true;

                                                                        //    WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                                                        //         backRightChunk.isChunkMapUpdated=true;
                                                                    }

                                                                }
                                                                else if (z + j >= chunkWidth)
                                                                {
                                                                    if (frontRightChunk != null)
                                                                    {
                                                                        frontRightChunk.additiveMap[(x + i) - chunkWidth, y + k, (z + j) - chunkWidth] = 9;

                                                                        isFrontRightChunkUpdated = true;

                                                                        //     WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                                                        //          frontRightChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            if (z + j < 0)
                                                            {

                                                                if (x + i >= 0 && x + i < chunkWidth)
                                                                {
                                                                    if (backChunk != null)
                                                                    {

                                                                        backChunk.additiveMap[x + i, y + k, chunkWidth + (z + j)] = 9;

                                                                        isBackChunkUpdated = true;

                                                                        //    WorldManager.chunkLoadingQueue.UpdatePriority(backChunk,0);
                                                                        //         backChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }
                                                                else if (x + i < 0)
                                                                {
                                                                    if (backLeftChunk != null)
                                                                    {
                                                                        backLeftChunk.additiveMap[chunkWidth + (x + i), y + k, chunkWidth - 1 + (z + j)] = 9;

                                                                        isBackLeftChunkUpdated = true;

                                                                        //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                                                        //            backLeftChunk.isChunkMapUpdated=true;
                                                                    }

                                                                }
                                                                else if (x + i >= chunkWidth)
                                                                {
                                                                    if (backRightChunk != null)
                                                                    {
                                                                        backRightChunk.additiveMap[(x + i) - chunkWidth, y + k, chunkWidth - 1 + (z + j)] = 9;

                                                                        isBackRightChunkUpdated = true;

                                                                        //       WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                                                        //      backRightChunk.isChunkMapUpdated=true;    
                                                                    }
                                                                }

                                                            }
                                                            else
                                                            if (z + j >= chunkWidth)
                                                            {

                                                                if (x + i >= 0 && x + i < chunkWidth)
                                                                {
                                                                    if (frontChunk != null)
                                                                    {

                                                                        frontChunk.additiveMap[x + i, y + k, (z + j) - chunkWidth] = 9;

                                                                        isFrontChunkUpdated = true;

                                                                        //    WorldManager.chunkLoadingQueue.UpdatePriority(frontChunk,0);
                                                                        //   frontChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }
                                                                else if (x + i < 0)
                                                                {
                                                                    if (frontLeftChunk != null)
                                                                    {
                                                                        frontLeftChunk.additiveMap[chunkWidth + (x + i), y + k, (z + j) - chunkWidth] = 9;

                                                                        isBackLeftChunkUpdated = true;

                                                                        //        WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                                                        //    frontLeftChunk.isChunkMapUpdated=true;
                                                                    }

                                                                }
                                                                else if (x + i >= chunkWidth)
                                                                {
                                                                    if (frontRightChunk != null)
                                                                    {
                                                                        frontRightChunk.additiveMap[(x + i) - chunkWidth, y + k, (z + j) - chunkWidth] = 9;

                                                                        isFrontRightChunkUpdated = true;

                                                                        //  WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                                                        //      frontRightChunk.isChunkMapUpdated=true;
                                                                    }
                                                                }
                                                            }


                                                        }
                                                        else
                                                        {
                                                            map[x + i, y + k, z + j] = 9;
                                                        }
                                                    }
                                                }
                                            }
                                            map[i, k, j] = 7;
                                            map[i, k + 1, j] = 7;
                                            map[i, k + 2, j] = 7;
                                            map[i, k + 3, j] = 7;
                                            map[i, k + 4, j] = 7;
                                            map[i, k + 5, j] = 9;
                                            map[i, k + 6, j] = 9;

                                            if (i + 1 < chunkWidth)
                                            {
                                                map[i + 1, k + 5, j] = 9;
                                                map[i + 1, k + 6, j] = 9;

                                            }
                                            else
                                            {
                                                if (rightChunk != null)
                                                {
                                                    rightChunk.additiveMap[0, k + 5, j] = 9;
                                                    rightChunk.additiveMap[0, k + 6, j] = 9;

                                                    //      rightChunk.isChunkMapUpdated=true;
                                                }
                                            }

                                            if (i - 1 >= 0)
                                            {
                                                map[i - 1, k + 5, j] = 9;
                                                map[i - 1, k + 6, j] = 9;

                                            }
                                            else
                                            {
                                                if (leftChunk != null)
                                                {
                                                    leftChunk.additiveMap[chunkWidth - 1, k + 5, j] = 9;
                                                    leftChunk.additiveMap[chunkWidth - 1, k + 6, j] = 9;

                                                    // leftChunk.isChunkMapUpdated=true;
                                                }
                                            }
                                            if (j + 1 < chunkWidth)
                                            {
                                                map[i, k + 5, j + 1] = 9;
                                                map[i, k + 6, j + 1] = 9;

                                            }
                                            else
                                            {
                                                if (frontChunk != null)
                                                {
                                                    frontChunk.additiveMap[i, k + 5, 0] = 9;
                                                    frontChunk.additiveMap[i, k + 6, 0] = 9;

                                                    //   frontChunk.isChunkMapUpdated=true;
                                                }
                                            }

                                            if (j - 1 >= 0)
                                            {
                                                map[i, k + 5, j - 1] = 9;
                                                map[i, k + 6, j - 1] = 9;

                                            }
                                            else
                                            {
                                                if (backChunk != null)
                                                {
                                                    backChunk.additiveMap[i, k + 5, chunkWidth - 1] = 9;
                                                    backChunk.additiveMap[i, k + 6, chunkWidth - 1] = 9;

                                                    //  backChunk.isChunkMapUpdated=true;
                                                }
                                            }

                                            treeCount--;
                                        }
                                    }
                                }

                            }
                        }
                    }
                    for (int i = 0; i < chunkWidth; i++)
                    {
                        for (int j = 0; j < chunkWidth; j++)
                        {
                            for (int k = 0; k < chunkHeight / 4; k++)
                            {

                                if (0 < k && k < 12)
                                {
                                    if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x, 0, pos.y) + new Vector3Int(i, k, j)) > 96)
                                    {

                                        map[i, k, j] = 10;

                                    }

                                }

                            }

                        }
                    }
                    for (int i = 0; i < chunkWidth; i++)
                    {
                        for (int j = 0; j < chunkWidth; j++)
                        {
                            map[i, 0, j] = 5;
                        }
                    }
                if (isBackChunkUpdated)
                {
                    if (backChunk != null&&backChunk.isReadyToRender)
                    {
                        backChunk.BuildChunk();
                    } }
                if (isLeftChunkUpdated)
                {
                if (leftChunk != null && leftChunk.isReadyToRender)
                    {
                        leftChunk.BuildChunk();
                    }
                }
                if (isFrontChunkUpdated && frontChunk.isReadyToRender)
                {
                if (frontChunk != null)
                    {
                        frontChunk.BuildChunk();
                    }
                }
                if (isRightChunkUpdated && rightChunk.isReadyToRender)
                {
                if (rightChunk != null)
                    {
                        rightChunk.BuildChunk();
                    }
                }
                  
               
                }
                else if (worldGenType == 1)
                {
                    for (int i = 0; i < chunkWidth; i++)
                    {
                        for (int j = 0; j < chunkWidth; j++)
                        {
                            //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                            for (int k = 0; k < chunkHeight / 4; k++)
                            {

                                map[i, k, j] = 1;

                            }
                        }
                    }
                }


                isMapGenCompleted= true; 
            }
        }

    public bool isReadyToRender=false;
    public List<VertexPositionNormalTexture> verticesOpq;//= new List<VertexPositionNormalTexture>();
    public List<ushort> indicesOpq;
    public List<VertexPositionNormalTexture> verticesNS; //= new List<VertexPositionNormalTexture>();
    public List<ushort> indicesNS;
    public List<VertexPositionNormalTexture> verticesWT;// = new List<VertexPositionNormalTexture>();
    public List<ushort> indicesWT;
    public BoundingBox chunkBounds;
    // void BuildMesh();
    // void BuildBlocks();
    public BoundingBox CalculateBounds()
    {
        return new BoundingBox(new Vector3(chunkPos.x,0,chunkPos.y), new Vector3(chunkPos.x+chunkWidth, GetHighestPoint(), chunkPos.y+chunkWidth));
    }



    public void GenerateMesh(List<VertexPositionNormalTexture> OpqVerts, List<VertexPositionNormalTexture> NSVerts,List<VertexPositionNormalTexture> WTVerts,List<ushort> OpqIndices,List<ushort> NSIndices,List<ushort> WTIndices)
    {
        
        int buildFaces =0;
        for (int x = 0; x < chunkWidth; x++)
        {
           
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int z = 0; z < chunkWidth; z++)
                {//new int[chunkwidth,chunkheiight,chunkwidth]
                 //     BuildBlock(x, y, z, verts, uvs, tris, vertsNS, uvsNS, trisNS);


                        int typeid = this.map[x, y, z];
                    if (typeid == 0) continue;
                   
                    
                    
                             if (typeid == 0) continue;
                             if (0 < typeid && typeid < 100)
                             {
                                 if (typeid == 9)
                                 {
                                     //Left
                                     if (CheckNeedBuildFace(x - 1, y, z,false) && GetChunkBlockType(x - 1, y, z) != 9)
                                         BuildFace(typeid, new Vector3(x, y, z), new Vector3(0,1,0),new Vector3(0,0,1), false, OpqVerts,0,OpqIndices);
                                     //Right
                                     if (CheckNeedBuildFace(x + 1, y, z, false) && GetChunkBlockType(x + 1, y, z) != 9)
                                         BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0,1,0),new Vector3(0,0,1), true, OpqVerts, 1, OpqIndices);

                                     //Bottom
                                     if (CheckNeedBuildFace(x, y - 1, z, false) && GetChunkBlockType(x, y - 1, z) != 9)
                                         BuildFace(typeid, new Vector3(x, y, z),new Vector3(0,0,1), new Vector3(1,0,0), false, OpqVerts ,2, OpqIndices);
                                     //Top
                                     if (CheckNeedBuildFace(x, y + 1, z, false) && GetChunkBlockType(x, y + 1, z) != 9)
                                         BuildFace(typeid, new Vector3(x, y + 1, z),new Vector3(0,0,1), new Vector3(1,0,0), true, OpqVerts, 3, OpqIndices);

                                     //Back
                                     if (CheckNeedBuildFace(x, y, z - 1, false) && GetChunkBlockType(x, y, z - 1) != 9)
                                         BuildFace(typeid, new Vector3(x, y, z), new Vector3(0,1,0), new Vector3(1,0,0), true, OpqVerts, 4, OpqIndices);
                                     //Front
                                     if (CheckNeedBuildFace(x, y, z + 1, false) && GetChunkBlockType(x, y, z + 1) != 9)
                                         BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0,1,0), new Vector3(1,0,0), false, OpqVerts, 5, OpqIndices);

                                 }
                                 else
                                 {
                                     if (CheckNeedBuildFace(x - 1, y, z, false))
                                         BuildFace(typeid, new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), false, OpqVerts, 0, OpqIndices);
                                     //Right
                                     if (CheckNeedBuildFace(x + 1, y, z, false))
                                         BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0, 1, 0), new Vector3(0, 0, 1), true, OpqVerts, 1, OpqIndices);

                                     //Bottom
                                     if (CheckNeedBuildFace(x, y - 1, z, false))
                                         BuildFace(typeid, new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), false, OpqVerts, 2, OpqIndices);
                                     //Top
                                     if (CheckNeedBuildFace(x, y + 1, z, false))
                                         BuildFace(typeid, new Vector3(x, y + 1, z), new Vector3(0, 0, 1), new Vector3(1, 0, 0), true, OpqVerts, 3, OpqIndices);

                                     //Back
                                     if (CheckNeedBuildFace(x, y, z - 1, false))
                                         BuildFace(typeid, new Vector3(x, y, z), new Vector3(0, 1, 0), new Vector3(1, 0, 0), true, OpqVerts, 4, OpqIndices);
                                     //Front
                                     if (CheckNeedBuildFace(x, y, z + 1, false))
                                         BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0), false, OpqVerts, 5, OpqIndices);

                                 }



                             }
                             else if (100 <= typeid && typeid < 200)
                             {

                                 if (typeid == 100)
                                 {



                                     //water
                                     //left
                                     if (CheckNeedBuildFace(x - 1, y, z,true) && GetChunkBlockType(x - 1, y, z) != 100)
                                     {
                                         if (GetChunkBlockType(x, y + 1, z) != 100)
                                         {
                                             BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f),new Vector3(0,0,1), false,WTVerts, 0,WTIndices);




                                         }
                                         else
                                         {
                                             BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f, 1f, 0f),new Vector3(0,0,1), false, WTVerts, 0, WTIndices);






                                         }

                                     }

                                     //Right
                                     if (CheckNeedBuildFace(x + 1, y, z, true) && GetChunkBlockType(x + 1, y, z) != 100)
                                     {
                                         if (GetChunkBlockType(x, y + 1, z) != 100)
                                         {
                                             BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f, 0.8f, 0f),new Vector3(0,0,1), true, WTVerts, 1, WTIndices);



                                         }
                                         else
                                         {
                                             BuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f, 1f, 0f),new Vector3(0,0,1), true, WTVerts, 1, WTIndices);



                                         }

                                     }



                                     //Bottom
                                     if (CheckNeedBuildFace(x, y - 1, z, true) && GetChunkBlockType(x, y - 1, z) != 100)
                                     {
                                         BuildFace(typeid, new Vector3(x, y, z),new Vector3(0,0,1), new Vector3(1,0,0), false, WTVerts, 2, WTIndices);




                                     }

                                     //Top
                                     if (CheckNeedBuildFace(x, y + 1, z, true) && GetChunkBlockType(x, y + 1, z) != 100)
                                     {
                                         BuildFace(typeid, new Vector3(x, y + 0.8f, z),new Vector3(0,0,1), new Vector3(1,0,0), true, WTVerts, 3, WTIndices);




                                     }




                                     //Back
                                     if (CheckNeedBuildFace(x, y, z - 1, true) && GetChunkBlockType(x, y, z - 1) != 100)
                                     {
                                         if (GetChunkBlockType(x, y + 1, z) != 100)
                                         {
                                             BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f, 0.8f, 0f), new Vector3(1,0,0), true, WTVerts, 4, WTIndices);




                                         }
                                         else
                                         {
                                             BuildFace(typeid, new Vector3(x, y, z), new Vector3(0f, 1f, 0f), new Vector3(1,0,0), true, WTVerts, 4, WTIndices);







                                         }

                                     }


                                     //Front
                                     if (CheckNeedBuildFace(x, y, z + 1, true) && GetChunkBlockType(x, y, z + 1) != 100)
                                     {
                                         if (GetChunkBlockType(x, y + 1, z) != 100)
                                         {
                                             BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f, 0.8f, 0f), new Vector3(1,0,0), false, WTVerts, 5, WTIndices);


                                         }
                                         else
                                         {
                                             BuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f, 1f, 0f), new Vector3(1,0,0), false, WTVerts, 4, WTIndices);

                                         }

                                     }
                                 }

                                 if (typeid >= 101 && typeid < 150)
                                 {

                                     if (typeid == 102)
                                     {
                                         //torch



                                     }
                                     else
                                     {
                                         Vector3 randomCrossModelOffset = new Vector3(0f, 0f, 0f);
                                         BuildFace(typeid, new Vector3(x, y, z) + randomCrossModelOffset, new Vector3(0f, 1f, 0f) + randomCrossModelOffset, new Vector3(1f, 0f, 1f) + randomCrossModelOffset, false, NSVerts, 0,NSIndices);







                                         BuildFace(typeid, new Vector3(x, y, z + 1f) + randomCrossModelOffset, new Vector3(0f, 1f, 0f) + randomCrossModelOffset, new Vector3(1f, 0f, -1f) + randomCrossModelOffset, false, NSVerts, 0, NSIndices);





                                     }


                                 }


                             }
                    
                }
            }
        }
          verticesOpqArray=verticesOpq.ToArray();
          verticesNSArray=verticesNS.ToArray();
           verticesWTArray=verticesWT.ToArray();
        indicesOpqArray=indicesOpq.ToArray();
        indicesNSArray=indicesNS.ToArray();
        indicesWTArray=indicesWT.ToArray();
        this.chunkBounds = this.CalculateBounds();
        //  Debug.WriteLine(verticesOpq.Count);

    }
    static void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<VertexPositionNormalTexture> verts, int side,List<ushort> indices)
    {
        VertexPositionNormalTexture vert00=new VertexPositionNormalTexture();
        VertexPositionNormalTexture vert01 = new VertexPositionNormalTexture();
        VertexPositionNormalTexture vert11 = new VertexPositionNormalTexture();
        VertexPositionNormalTexture vert10 = new VertexPositionNormalTexture();
        short index = (short)verts.Count;
        vert00.Position = corner;
        vert01.Position = corner + up;
        vert11.Position = corner + up + right;
        vert10.Position = corner + right;
    //    verts.Add(vert0);
    //    verts.Add(vert1);
    //    verts.Add(vert2);
   //     verts.Add(vert3);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        if (blockInfo.ContainsKey(typeid))
        {
            uvCorner = blockInfo[typeid][side];
        }
        vert00.TextureCoordinate = uvCorner;
        vert01.TextureCoordinate =new Vector2(uvCorner.X, uvCorner.Y + uvWidth.Y);
        vert11.TextureCoordinate = new Vector2(uvCorner.X + uvWidth.X, uvCorner.Y + uvWidth.Y);
        vert10.TextureCoordinate = new Vector2(uvCorner.X + uvWidth.X, uvCorner.Y);
        //    uvs.Add(uvCorner);
        //    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        //    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        Vector3 normal = Vector3.Cross(up, right);
        vert00.Normal = normal;
        vert01.Normal = normal;
        vert11.Normal = normal;
        vert10.Normal = normal;
        verts.Add(vert00);
        verts.Add(vert01);
        verts.Add(vert11);
        verts.Add(vert10);
        if (!reversed)
        {
            /*  verts.Add(vert00);
               verts.Add(vert01);
               verts.Add(vert11);
               verts.Add(vert11);
               verts.Add(vert10);
               verts.Add(vert00);*/
            indices.Add( (ushort)(index +0));
            indices.Add((ushort)(index + 1));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 3));
            indices.Add((ushort)(index + 0));
            //    tris.Add(index + 2);

            //   tris.Add(index + 0);

        }
        else
        {

            /*    verts.Add(vert01);
                verts.Add(vert00);
                verts.Add(vert11);
                verts.Add(vert10);
                verts.Add(vert11);
                verts.Add(vert00);*/
            //     indices.Add()
            indices.Add((ushort)(index + 1));
            indices.Add((ushort)(index + 0));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 3));
            indices.Add((ushort)(index + 2));
            indices.Add((ushort)(index + 0));
        
        }

    }


    bool CheckNeedBuildFace(int x, int y, int z,bool isThisNS)
    {
       // return true;
        if (y < 0) return false;
        var type = GetChunkBlockType(x, y, z);
        bool isNonSolid = false;
        if (isThisNS == true)
        {
            switch (type)
            {
                case 100:
             //       Debug.WriteLine("true");
                    return false;
                case 0:
                    return true;
                default: return false;
            }
        }
        else
        {
        if (type < 200 && type >= 100)
        {
            isNonSolid = true;
        }
        switch (isNonSolid)
        {
            case true: return true;
            case false: break;
        }
        }
      
       
        switch (type)
        {


            case 0:
                return true;
            case 9:
                return true;
            default:
                return false;
        }
    }
    public static int PredictBlockType(float noiseValue, int y)
    {
        if (noiseValue > y)
        {
            return 1;
        }
        else
        {
            if (y < chunkSeaLevel && y > noiseValue)
            {
                return 100;
            }
            return 0;
        }
        // return 0;
    }

    public int GetChunkBlockType(int x, int y, int z)
    {
        if (y < 0 || y > chunkHeight - 1)
        {
            return 0;
        }

        if ((x < 0) || (z < 0) || (x >= chunkWidth) || (z >= chunkWidth))
        {
            if (x >= chunkWidth)
            {
                if (rightChunk != null && rightChunk.isReadyToRender == true)
                {
                    return rightChunk.map[0, y, z];
                }
                else return PredictBlockType(thisHeightMap[x - chunkWidth + 25, z + 8], y);

            }
            else if (z >= chunkWidth)
            {
                if (frontChunk != null && frontChunk.isReadyToRender == true)
                {
                    return frontChunk.map[x, y, 0];
                }
                else return PredictBlockType(thisHeightMap[x + 8, z - chunkWidth + 25], y);
            }
            else if (x < 0)
            {
                if (leftChunk != null && leftChunk.isReadyToRender == true)
                {
                    return leftChunk.map[chunkWidth - 1, y, z];
                }
                else return PredictBlockType(thisHeightMap[8 + x, z + 8], y);
            }
            else if (z < 0)
            {
                if (backChunk != null && backChunk.isReadyToRender == true)
                {
                    return backChunk.map[x, y, chunkWidth - 1];
                }
                else return PredictBlockType(thisHeightMap[x + 8, 8 + z], y);
            }

        }
        return map[x, y, z];
    }

    public VertexPositionNormalTexture[] verticesOpqArray;
    public VertexPositionNormalTexture[] verticesNSArray;
    public VertexPositionNormalTexture[] verticesWTArray;
    public ushort[] indicesOpqArray;
    public ushort[] indicesNSArray;
    public ushort[] indicesWTArray;
    public int GetHighestPoint()
    {
        int returnValue = 0;
        for(int x=0;x<chunkWidth;x++)
        {
            for(int z = 0; z < chunkWidth; z++)
            {
                for(int y = chunkHeight - 1; y > 0; y--)
                {
                    if (map[x, y, z] != 0)
                    {
                        if(y>returnValue) returnValue = y;
                    }
                }
            }
        }
        return returnValue;
    }
    public async void BuildChunk()
    {

       await Task.Run(()=> InitMap(chunkPos));
        //     GenerateMesh(verticesOpq, verticesNS, verticesWT);
        //  Debug.WriteLine(verticesOpqArray.Length);
        //Debug.WriteLine(verticesWTArray.Length);
        isReadyToRender = true;
    }
    public void Dispose()
    {
        this.map= null;
        this.verticesNSArray= null;
        this.additiveMap = null;
        this.verticesWTArray= null;
        this.verticesOpqArray= null;
        this.verticesOpq= null;
        this.verticesNS= null;
        this.verticesWT= null;
        this.verticesNSArray= null;
        this.indicesOpqArray= null;
        this.indicesNSArray= null;
        this.indicesWTArray= null;
        this.indicesOpq  = null;
        this.indicesNS  = null;
        this.indicesWT  = null;
    }
    }
