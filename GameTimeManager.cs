using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace monogameMinecraft
{
    public class GameTimeManager
    {
        public GamePlayer player;
        public float dateTime;
        public Vector3 sunDir;
        public float sunX;
        public float sunY;
        public float sunZ=0f;
        public GameTimeManager(GamePlayer player) { this.player = player; }
        public Vector3 EulerToVec3(Vector3 euler)
        {
            float yaw = euler.Y;
            float pitch = euler.X;


    //        Debug.WriteLine(yaw + " " + pitch);

            Vector3 front=new Vector3();
            front.X = MathF.Cos(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));
            front.Y = MathF.Sin(MathHelper.ToRadians(pitch));
            front.Z = MathF.Sin(MathHelper.ToRadians(yaw)) * MathF.Cos(MathHelper.ToRadians(pitch));
            front.Normalize();
            return front;
        }
        //0.25f-0.75f night
        public void Update(float deltaTime)
        {
            dateTime += deltaTime * 0.005f;
            if (dateTime >= 1f)
            {
                dateTime = 0f;
            }
            sunX = (dateTime) * 360f;
            sunY = 20f;
            sunDir=EulerToVec3(new Vector3(sunX, sunY, sunZ))*50f;
       //     Debug.WriteLine(sunDir);


        }
    }
}
