using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
namespace monogameMinecraft
{
    [MessagePackObject]
    public struct PlayerData
    {
        [Key(0)]
        public float posX;
        [Key(1)]
        public float posY;
        [Key(2)]
        public float posZ;
        [Key(3)]
        public short[] inventoryData;

        public PlayerData(float posX, float posY, float posZ, short[] inventoryData)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.inventoryData = inventoryData;
        }
    }
}
