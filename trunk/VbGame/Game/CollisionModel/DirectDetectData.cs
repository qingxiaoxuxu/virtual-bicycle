using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace VirtualBicycle.CollisionModel
{
    public enum CollisionTreeType
    {
        BoundingBall,
        AABB
    }
    public struct DirectDetectData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public float Depth;

        public DirectDetectData(Vector3 p, Vector3 n, float depth)
        {
            Position = p;
            Normal = n;
            Depth = depth;
        }
    }

}
