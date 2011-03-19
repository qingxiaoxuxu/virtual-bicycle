using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using VirtualBicycle.MathLib;

namespace VirtualBicycle.CollisionModel
{
    public abstract class BaseTreeNode :  IDisposable
    {
        /// <summary>
        /// 获得子节点数组
        /// </summary>
        public BaseTreeNode[] children;
        /// <summary>
        /// 获得表面数据
        /// </summary>
        public TreeFace faceData;

        public abstract Vector3 Centre { get; }
        public abstract CollisionTreeType TreeType { get; }

        public abstract bool IsInDF(ref Vector3 p);

        public abstract void IntersectDF(ref Triangle t, List<DirectDetectData> res);
        public abstract void IntersectDF(ref LineSegment ra, List<DirectDetectData> res);
        public abstract void IntersectDF(ref BoundingSphere ball, List<DirectDetectData> res);
        public abstract void IntersectBF(BoundingSphere ball, List<DirectDetectData> res);

        //public abstract void Intersect(Quad plane, List<DirectDetectData> res);
        //public abstract void Intersect(AABB aabb, List<DirectDetectData> res);

        public abstract void IntersectDF(AABBTreeNode cdTree, List<DirectDetectData> res);
        public abstract void IntersectDF(BBTreeNode cdTree, List<DirectDetectData> res);

        public abstract void IntersectBF(AABBTreeNode cdTree, List<DirectDetectData> res);
        public abstract void IntersectBF(BBTreeNode cdTree, List<DirectDetectData> res);


        public abstract void SearchDF(ref BoundingSphere ball, List<Triangle> res);
        public abstract void SearchDF(ref BoundingBox aabb, List<Triangle> res);


        public abstract void Update();

        public abstract int CalculateSize();

        public abstract void Save(BinaryWriter bw);

        public void Dispose()
        {
            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].Dispose();
                    children[i] = null;
                }
            }
            faceData = null;
            //if (childrenUBound != -1)
            //{
            //    for (int i = 0; i <= childrenUBound; i++)
            //    {
            //        ((IDisposable)children[i]).Dispose();
            //        children[i] = null;
            //    }
            //    children = null;
            //}
            //else
            //    faceData = null;
        }
    }

    /// <summary>
    /// 三角面
    /// </summary>
    public class TreeFace
    {
        public int triIndex;
        public Vector3 a, b, c;
        public Vector3 centre;
        //public TreeFace tAB, tBC, tCA;

        public TreeFace()
        {

        }

        public void ReadData(BinaryReader br)
        {
            triIndex = br.ReadInt32();

            a.X = br.ReadSingle();
            a.Y = br.ReadSingle();
            a.Z = br.ReadSingle();

            b.X = br.ReadSingle();
            b.Y = br.ReadSingle();
            b.Z = br.ReadSingle();

            c.X = br.ReadSingle();
            c.Y = br.ReadSingle();
            c.Z = br.ReadSingle();

            centre.X = br.ReadSingle();
            centre.Y = br.ReadSingle();
            centre.Z = br.ReadSingle();
        }
        public void WriteData(BinaryWriter bw)
        {
            bw.Write(triIndex);

            bw.Write(a.X);
            bw.Write(a.Y);
            bw.Write(a.Z);

            bw.Write(b.X);
            bw.Write(b.Y);
            bw.Write(b.Z);

            bw.Write(c.X);
            bw.Write(c.Y);
            bw.Write(c.Z);

            bw.Write(centre.X);
            bw.Write(centre.Y);
            bw.Write(centre.Z);

        }

        public TreeFace(int tri, Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;

            this.centre = (a + b + c) / 3;
            this.triIndex = tri;// mPhysMesh = mesh;
            //tAB = ab; tBC = bc; tCA = ca;
        }

        public static explicit operator Triangle(TreeFace src)
        {
            return new Triangle(ref src.a, ref src.b, ref src.c);
        }

        public void Update(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;

            this.centre = (a + b + c) / 3;
        }
    }
}
