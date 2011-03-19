using System;
using System.Collections.Generic;
using System.Text;
using VirtualBicycle.MathLib;
using Microsoft.Xna.Framework;
using System.IO;

namespace VirtualBicycle.CollisionModel
{
    /// <summary>
    /// 碰撞包围球树
    /// </summary>
    /// <remarks>
    /// 该树的任意节点最多有8个子节点
    /// 
    /// 构造节点时，计算传给的所有三角形的包围球并将尝试将它们分成8份，然后将分好的三角形传下去
    /// 该树不是对称的，不一定是满树。树的形态由几何形态决定。
    /// 最后一级节点将只有一个三角形，以及三角形的剪枝包围球
    /// </remarks>
    public class BBTreeNode : BaseTreeNode
    {
        /// <summary>
        /// 剪枝包围球
        /// </summary>
        public BoundingSphere ball;

        public BBTreeNode(BinaryReader br)
        {
            ball.Center.X = br.ReadSingle();
            ball.Center.Y = br.ReadSingle();
            ball.Center.Z = br.ReadSingle();

            ball.Radius = br.ReadSingle();

            int childrenCount = br.ReadInt32();

            if (childrenCount == 0)
            {
                faceData = new TreeFace();
                faceData.ReadData(br);
            }
            else
            {
                children = new BaseTreeNode[childrenCount];
                for (int i = 0; i < childrenCount; i++)
                {
                    children[i] = new AABBTreeNode(br);
                }
            }

        }
        public BBTreeNode(List<TreeFace> remains)
        {
            int i;
            int rcount = remains.Count;

            for (i = 0; i < rcount; i++)
                ball.Center += remains[i].centre;
            //for (i = 0; i < rcount; i++)
            //    bBall.vCentre += remains[i].vA;

            ball.Center *= (1f / (float)i);// /= (float)i;

            ball.Radius = 0;
            for (i = 0; i < rcount; i++)
            {
                float dist = MathEx.Distance(ref ball.Center, ref remains[i].a); // bBall.vCentre & remains[i].vA;
                if (dist > ball.Radius) ball.Radius = dist;

                dist = MathEx.Distance(ref ball.Center, ref remains[i].b); // bBall.vCentre & remains[i].vB;
                if (dist > ball.Radius) ball.Radius = dist;

                dist = MathEx.Distance(ref ball.Center, ref remains[i].c); // bBall.vCentre & remains[i].vC;
                if (dist > ball.Radius) ball.Radius = dist;
            }

            if (rcount > 1)
            {
                float xoy = ball.Center.Z;
                float yoz = ball.Center.X;
                float xoz = ball.Center.Y;


                List<TreeFace>[] lim = new List<TreeFace>[8] 
                    { new List<TreeFace>(), new List<TreeFace>(), 
                      new List<TreeFace>(), new List<TreeFace>(), 
                      new List<TreeFace>(), new List<TreeFace>(), 
                      new List<TreeFace>(), new List<TreeFace>() };


                for (i = 0; i < rcount; i++)
                {
                    float fxy = remains[i].centre.Z - xoy;// xoy[remains[i].mCentre];
                    float fyz = remains[i].centre.X - yoz;//yoz[remains[i].mCentre];
                    float fxz = remains[i].centre.Y - xoz;//xoz[remains[i].mCentre];

                    if (fxz > 0) // y>0
                        //1-4卦限
                        if (fyz > 0) // x>0
                            if (fxy > 0) // z>0
                                lim[0].Add(remains[i]);
                            else
                                lim[3].Add(remains[i]);
                        else
                            if (fxy > 0) // z>0
                                lim[1].Add(remains[i]);
                            else
                                lim[2].Add(remains[i]);
                    else

                        //5-8卦限
                        if (fyz > 0) // x>0
                            if (fxy > 0) // z>0
                                lim[4].Add(remains[i]);
                            else
                                lim[7].Add(remains[i]);

                        else
                            if (fxy > 0) // z>0
                                lim[5].Add(remains[i]);
                            else
                                lim[6].Add(remains[i]);
                }
                //remains.Clear();

                int childrenUBound = -1;
                bool[] usestat = new bool[8];
                for (i = 0; i < 8; i++)
                    if (lim[i].Count > 0)
                    {
                        childrenUBound++;
                        usestat[i] = true;
                    }
                    else
                        usestat[i] = false;


                children = new BaseTreeNode[childrenUBound + 1];

                int k = 0;
                for (i = 0; i < 8; i++)
                    if (usestat[i])
                    {
                        children[k] = new BBTreeNode(lim[i]);
                        k++;
                    }
            }
            else
            {
                //childrenUBound = -1;
                faceData = remains[0];
                remains.Clear();
            }
        }

        public override  void Save(BinaryWriter bw)
        {
            bw.Write(ball.Center.X);
            bw.Write(ball.Center.Y);
            bw.Write(ball.Center.Z);

            bw.Write(ball.Radius);

            if (children == null)
            {
                bw.Write(0);
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].Save(bw);
                }
            }
        }


        public override bool IsInDF(ref Vector3 p)
        {
            if (children != null)
            {
                if (MathEx.DistanceSquared(ref p, ref ball.Center) < ball.Radius * ball.Radius)//(p ^ bBall.vCentre)
                {
                    bool isin = new bool();
                    for (int i = 0; i < children.Length; i++)
                        isin |= children[i].IsInDF(ref p);
                    return isin;
                }
                return false;
            }
            else
            {
                if (MathEx.DistanceSquared(ref p, ref ball.Center) < ball.Radius * ball.Radius)
                {
                    Plane pl = new Plane(faceData.a, faceData.b, faceData.c);
                    pl.Normalize();
                    return Math.Abs(MathEx.PlaneRelative(ref pl, ref p)) <= 2;
                }
            }
            return false;
        }


        public override void IntersectDF(ref Triangle t, List<DirectDetectData> res)
        {
            if (MathEx.BoundingSphereIntersects(ref ball, ref t))// bBall.Intersect(ref t))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].IntersectDF(ref t, res);
                }
                else
                {
                    List<DirectDetectData> data;
                    Triangle face = (Triangle)faceData;
                    if (Triangle.TriangleIntersect(ref t, ref face, out data))
                    {
                        for (int j = 0; j < data.Count; j++)
                        {
                            res.Add(data[j]);
                        }
                    }
                }
        }
        public override void IntersectDF(ref LineSegment ra, List<DirectDetectData> res)
        {

            if (MathEx.BoundingSphereIntersects(ref ball, ref ra.Start, ref ra.End))// bBall.Intersect(ra.vStart, ra.vEnd))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].IntersectDF(ref ra, res);
                }
                else
                {
                    Vector3 pos;
                    Triangle face = (Triangle)faceData;
                    if (face.RayTriCollision(out pos, ref ra.Start, ref ra.End))
                        res.Add(new DirectDetectData(pos, face.vN, 0));
                }
        }
        public override void IntersectDF(ref BoundingSphere ball, List<DirectDetectData> res)
        {
            if (MathEx.BoundingSphereIntersects(ref ball, ref ball))// bBall.Intersect(ref ball.Center, ref ball.Radius))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].IntersectDF(ref ball, res);
                }
                else
                {
                    DirectDetectData data;
                    Triangle face = (Triangle)faceData;
                    if (MathEx.BoundingSphereIntersects(ref ball, ref face, out data.Position, out data.Normal, out data.Depth))//)ball.Intersect(ref face, out data.vPos, out data.vNormal, out data.dDepth))
                        res.Add(data);

                }
        }

        public override void IntersectBF(BoundingSphere ball, List<DirectDetectData> res)
        {
            Queue<BBTreeNode> q = new Queue<BBTreeNode>();
            q.Enqueue(this);
            while (q.Count > 0)
            {
                BBTreeNode node = q.Dequeue();
                if (MathEx.BoundingSphereIntersects(ref ball, ref node.ball))// ball.Intersect(ref node.bBall.vCentre, ref node.bBall.dRange))
                    if (node.children != null)
                    {
                        for (int i = 0; i < node.children.Length; i++)
                            q.Enqueue((BBTreeNode)node.children[i]);
                    }
                    else
                    {
                        DirectDetectData data;
                        Triangle face = (Triangle)node.faceData;
                        //if (ball.Intersect(ref face, out data.vPos, out data.vNormal, out data.dDepth))
                        if (MathEx.BoundingSphereIntersects(ref ball, ref face, out data.Position, out data.Normal, out data.Depth))
                            res.Add(data);
                    }
            }
            q.Clear();
        }


        public override void SearchDF(ref BoundingBox aabb, List<Triangle> res)
        {
            if (MathEx.BoundingSphereIntersects(ref ball, ref aabb))//(bBall.Intersect(ref aabb))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].SearchDF(ref aabb, res);
                }
                else
                {
                    Triangle t = (Triangle)faceData;
                    //if (aabb.Intersect(ref t)) res.Add(ref t);
                    if (MathEx.AABBIntersects(ref aabb, ref t))
                        res.Add(t);
                }
        }
        public override void SearchDF(ref BoundingSphere ball, List<Triangle> res)
        {
            if (MathEx.BoundingSphereIntersects(ref ball, ref ball))//  bBall.Intersect(ref ball.Center, ref ball.Radius))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].SearchDF(ref ball, res);
                }
                else
                {
                    Triangle t = (Triangle)faceData;
                    //if (ball.Intersect(ref t)) res.Add(t);
                    if (MathEx.BoundingSphereIntersects(ref ball, ref t))
                    {
                        res.Add(t);
                    }
                }
        }

        public override void IntersectDF(BBTreeNode cdTree, List<DirectDetectData> res)
        {
            //if ((cdTree.bBall.vCentre ^ bBall.vCentre) <= r * r)
            if (children != null)
                for (int i = 0; i < children.Length; i++)
                {
                    BBTreeNode ch = (BBTreeNode)children[i];
                    float r = (cdTree.ball.Radius + ch.ball.Radius);
                    if (MathEx.DistanceSquared(ref cdTree.ball.Center, ref ch.ball.Center) <= r * r)
                        cdTree.IntersectDF(ch, res); //相互遍历
                }
            else
            {
                // 有一个到了根节点
                // 那就就让另一个直接到根节点判断，就是
                Triangle t = (Triangle)faceData;
                cdTree.IntersectDF(ref t, res);
            }
        }
        public override void IntersectDF(AABBTreeNode cdTree, List<DirectDetectData> res)
        {
            //if (bBall.Intersect(cdTree.aAABB))
            if (children != null)
                for (int i = 0; i < children.Length; i++)
                {
                    BBTreeNode ch = (BBTreeNode)children[i];
                    //if (ch.bBall.Intersect(ref cdTree.aAABB))
                    if (MathEx.BoundingSphereIntersects(ref ch.ball, ref cdTree.aabb))
                        cdTree.IntersectDF(ch, res); //相互遍历
                }
            else
            {
                Triangle t = (Triangle)faceData;
                cdTree.IntersectDF(ref t, res);
            }
        }

        //[Obsolete()]
        public override void IntersectBF(AABBTreeNode cdTree, List<DirectDetectData> res)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        //[Obsolete()]
        public override void IntersectBF(BBTreeNode cdTree, List<DirectDetectData> res)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override Vector3 Centre
        {
            get { return ball.Center; }
        }
        public override CollisionTreeType TreeType
        {
            get { return CollisionTreeType.BoundingBall; }
        }

        public override void Update()
        {
            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                    children[i].Update();

                ball.Radius = 0;
                ball.Center = Vector3.Zero;

                for (int i = 0; i < children.Length; i++)
                {
                    ball.Center += children[i].Centre;
                }

                ball.Center /= (float)(children.Length);
                for (int i = 0; i < children.Length; i++)
                {
                    float dist = Vector3.Distance(ball.Center, children[i].Centre);
                    if (dist > ball.Radius)
                    {
                        ball.Radius = dist;
                    }
                }
            }
            else
            {
                ball.Center = faceData.centre;
                ball.Radius = 0;

                float dist = MathEx.Distance(ref ball.Center, ref faceData.a);
                if (dist > ball.Radius)
                {
                    ball.Radius = dist;
                }

                dist = MathEx.Distance(ref ball.Center, ref faceData.b);
                if (dist > ball.Radius)
                {
                    ball.Radius = dist;
                }

                dist = MathEx.Distance(ref ball.Center, ref faceData.c);
                if (dist > ball.Radius)
                {
                    ball.Radius = dist;
                }
            }
        }

        public unsafe override int CalculateSize()
        {
            int chSize = 0;
            if (children != null)
            {
                chSize += children.Length * sizeof(int);

                for (int i = 0; i < children.Length; i++)
                {
                    chSize += children[i].CalculateSize();
                }
            }
            return sizeof(int) * 2 + sizeof(BoundingSphere) + chSize;
        }
    }
}
