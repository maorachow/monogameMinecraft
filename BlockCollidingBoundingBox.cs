using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace monogameMinecraft
{
    public class BlockCollidingBoundingBoxHelper
    {
        public static float calculateXOffset(BoundingBox bb1, BoundingBox bb2, float x)
        {
            if (bb2.Max.Y <= bb1.Min.Y || bb2.Min.Y >= bb1.Max.Y)
            {
                return x;
            }
            if (bb2.Max.Z <= bb1.Min.Z || bb2.Min.Z >= bb1.Max.Z)
            {
                return x;
            }
            if (x > 0 && bb2.Max.X <= bb1.Min.X)
            {
                float x1 = bb1.Min.X - bb2.Max.X;
                if (x1 < x)
                {
                    x = x1;
                }
            }
            if (x < 0 && bb2.Min.X >= bb1.Max.X)
            {
                float x2 = bb1.Max.X - bb2.Min.X;
                if (x2 > x)
                {
                    x = x2;
                }
            }

            return x;
        }

        public static float calculateYOffset(BoundingBox bb1, BoundingBox bb2, float y)
        {
            if (bb2.Max.X <= bb1.Min.X || bb2.Min.X >= bb1.Max.X)
            {
                return y;
            }
            if (bb2.Max.Z <= bb1.Min.Z || bb2.Min.Z >= bb1.Max.Z)
            {
                return y;
            }
            if (y > 0 && bb2.Max.Y <= bb1.Min.Y)
            {
                float y1 = bb1.Min.Y - bb2.Max.Y;
                if (y1 < y)
                {
                    y = y1;
                }
            }
            if (y < 0 && bb2.Min.Y >= bb1.Max.Y)
            {
                float y2 = bb1.Max.Y - bb2.Min.Y;
                if (y2 > y)
                {
                    y = y2;
                }
            }

            return y;
        }

        public static float calculateZOffset(BoundingBox bb1, BoundingBox bb2, float z)
        {
            if (bb2.Max.X <= bb1.Min.X || bb2.Min.X >= bb1.Max.X)
            {
                return z;
            }
            if (bb2.Max.Y <= bb1.Min.Y || bb2.Min.Y >= bb1.Max.Y)
            {
                return z;
            }
            if (z > 0 && bb2.Max.Z <= bb1.Min.Z)
            {
                float z1 = bb1.Min.Z - bb2.Max.Z;
                if (z1 < z)
                {
                    z = z1;
                }
            }
            if (z < 0 && bb2.Min.Z >= bb1.Max.Z)
            {
                float z2 = bb1.Max.Z - bb2.Min.Z;
                if (z2 > z)
                {
                    z = z2;
                }
            }

            return z;
        }
        public static BoundingBox offset(BoundingBox bb, float x, float y, float z)
        {
            bb.Min.X=(bb.Min.X + x);
            bb.Min.Y=(bb.Min.Y + y);
            bb.Min.Z=(bb.Min.Z + z);
            bb.Max.X=(bb.Max.X + x);
            bb.Max.Y=(bb.Max.Y + y);
            bb.Max.Z = (bb.Max.Z + z);
            //  Debug.Log("move" + x + " " + y + " " + z + " ");
            return bb;
        }
    }
}
