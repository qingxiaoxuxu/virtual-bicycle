using System;
using System.Collections.Generic;
using System.Text;
using VirtualBicycle.MathLib;
using Microsoft.Xna.Framework;
using System.IO;


namespace VirtualBicycle.CollisionModel
{
    public class AABBTreeNode : BaseTreeNode
    {
        public BoundingBox aabb;

        public AABBTreeNode(BinaryReader br)
        {
            aabb.Min.X = br.ReadSingle();
            aabb.Min.Y = br.ReadSingle();
            aabb.Min.Z = br.ReadSingle();

            aabb.Max.X = br.ReadSingle();
            aabb.Max.Y = br.ReadSingle();
            aabb.Max.Z = br.ReadSingle();

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
        public AABBTreeNode(List<TreeFace> remains)
        {
            aabb.Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            aabb.Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            int rcount = remains.Count;
            for (int i = 0; i < rcount; i++)
            {
                Vector3 va = remains[i].a;
                Vector3 vb = remains[i].b;
                Vector3 vc = remains[i].c;

                if (va.X < aabb.Min.X) aabb.Min.X = va.X;
                if (va.X > aabb.Max.X) aabb.Max.X = va.X;
                if (va.Y < aabb.Min.Y) aabb.Min.Y = va.Y;
                if (va.Y > aabb.Max.Y) aabb.Max.Y = va.Y;
                if (va.Z < aabb.Min.Z) aabb.Min.Z = va.Z;
                if (va.Z > aabb.Max.Z) aabb.Max.Z = va.Z;

                if (vb.X < aabb.Min.X) aabb.Min.X = vb.X;
                if (vb.X > aabb.Max.X) aabb.Max.X = vb.X;
                if (vb.Y < aabb.Min.Y) aabb.Min.Y = vb.Y;
                if (vb.Y > aabb.Max.Y) aabb.Max.Y = vb.Y;
                if (vb.Z < aabb.Min.Z) aabb.Min.Z = vb.Z;
                if (vb.Z > aabb.Max.Z) aabb.Max.Z = vb.Z;

                if (vc.X < aabb.Min.X) aabb.Min.X = vc.X;
                if (vc.X > aabb.Max.X) aabb.Max.X = vc.X;
                if (vc.Y < aabb.Min.Y) aabb.Min.Y = vc.Y;
                if (vc.Y > aabb.Max.Y) aabb.Max.Y = vc.Y;
                if (vc.Z < aabb.Min.Z) aabb.Min.Z = vc.Z;
                if (vc.Z > aabb.Max.Z) aabb.Max.Z = vc.Z;
            }

            if (rcount > 1)
            {
                //centre可能不在AABB内
                Vector3 centre = new Vector3();
                for (int i = 0; i < rcount; i++)
                {
                    Vector3.Add(ref centre, ref remains[i].centre, out centre);
                }
                Vector3.Multiply(ref centre, 1f / (float)rcount, out centre);

                float xoy = centre.Z;
                float yoz = centre.X;
                float xoz = centre.Y;

                List<TreeFace>[] lim = new List<TreeFace>[8] 
                    { new List<TreeFace>(), new List<TreeFace>(), 
                      new List<TreeFace>(), new List<TreeFace>(), 
                      new List<TreeFace>(), new List<TreeFace>(), 
                      new List<TreeFace>(), new List<TreeFace>() };

                for (int i = 0; i < rcount; i++)
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
                remains.Clear();

                int childrenUBound = -1;
                bool[] usestat = new bool[8];
                for (int i = 0; i < 8; i++)
                    if (lim[i].Count > 0)
                    {
                        childrenUBound++;
                        usestat[i] = true;
                    }
                    else
                        usestat[i] = false;

                children = new BaseTreeNode[childrenUBound + 1];

                int k = 0;
                for (int i = 0; i < 8; i++)
                    if (usestat[i])
                    {
                        children[k] = new AABBTreeNode(lim[i]);
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


        public override void Save(BinaryWriter bw)
        {
            bw.Write(aabb.Min.X);
            bw.Write(aabb.Min.Y);
            bw.Write(aabb.Min.Z);

            bw.Write(aabb.Max.X);
            bw.Write(aabb.Max.Y);
            bw.Write(aabb.Max.Z);

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

        public override CollisionTreeType TreeType
        {
            get { return CollisionTreeType.AABB; }
        }
        public override Vector3 Centre
        {
            get
            {
                return new Vector3((aabb.Min.X + aabb.Max.X) * 0.5f,
                                   (aabb.Min.Y + aabb.Max.Y) * 0.5f,
                                   (aabb.Min.Z + aabb.Max.Z) * 0.5f);
            }
        }

        public override bool IsInDF(ref Vector3 p)
        {
            if (MathEx.AABBIsIn(ref aabb, ref p))
                if (children != null)
                {
                    bool isin = new bool();
                    for (int i = 0; i < children.Length; i++)
                        isin |= children[i].IsInDF(ref p);
                    return isin;
                }
                else
                {
                    Plane pl = new Plane(faceData.a, faceData.b, faceData.c);
                    pl.Normalize();
                    return Math.Abs(MathEx.PlaneRelative(ref pl, ref p)) <= 2;
                }
            return false;
        }

        public override void IntersectDF(ref Triangle t, List<DirectDetectData> res)
        {
            if (MathEx.AABBIntersects(ref aabb, ref t))//aAABB.Intersect(ref t))
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
            if (children != null)
            {
                if (MathEx.AABBIntersects(ref aabb, ref ra.Start, ref ra.End))//aAABB.Intersect(ra.vStart, ra.vEnd))
                    for (int i = 0; i < children.Length; i++)
                        children[i].IntersectDF(ref ra, res);
            }
            else
            {
                //if (aAABB.Intersect(ra.vStart, ra.vEnd))
                //{
                Vector3 pos;
                Triangle face = (Triangle)faceData;
                if (face.RayTriCollision(out pos, ref ra.Start, ref ra.End))
                    res.Add(new DirectDetectData(pos, face.vN, 0));
                //}

            }
        }
        public override void IntersectDF(ref BoundingSphere ball, List<DirectDetectData> res)
        {
            if (MathEx.AABBIntersects(ref aabb, ref ball))// aAABB.Intersect(ref ball.Center, ref ball.Radius))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].IntersectDF(ref ball, res);
                }
                else
                {
                    DirectDetectData data;
                    Triangle face = (Triangle)faceData;
                    if (MathEx.BoundingSphereIntersects(ref ball, ref face, out data.Position, out data.Normal, out data.Depth))//(ball.Intersect(ref face, out data.vPos, out data.vNormal, out data.dDepth))
                        res.Add(data);

                    //Simulation.PhysMT.testtest2++;
                }
        }

        public override void IntersectBF(BoundingSphere ball, List<DirectDetectData> res)
        {
            Queue<AABBTreeNode> q = new Queue<AABBTreeNode>();
            q.Enqueue(this);
            while (q.Count > 0)
            {
                AABBTreeNode node = q.Dequeue();
                if (MathEx.AABBIntersects(ref node.aabb, ref ball))//(node.aAABB.Intersect(ref ball.Center, ref ball.Radius))
                    if (node.children != null)
                    {
                        for (int i = 0; i < node.children.Length; i++)
                            q.Enqueue((AABBTreeNode)node.children[i]);
                    }
                    else
                    {
                        DirectDetectData data;
                        Triangle face = (Triangle)node.faceData;
                        if (MathEx.BoundingSphereIntersects(ref ball, ref face, out data.Position, out data.Normal, out data.Depth))//(ball.Intersect(ref face, out data.vPos, out data.vNormal, out data.dDepth))
                            res.Add(data);
                    }
            }
            q.Clear();
        }

        public override void SearchDF(ref BoundingBox aabb, List<Triangle> res)
        {
            if (MathEx.AABBIntersects(ref aabb, ref aabb))// aAABB.Intersect(ref aabb))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].SearchDF(ref aabb, res);
                }
                else
                {
                    Triangle t = (Triangle)faceData;
                    //if (aabb.Intersect(ref t)) res.Add(t);
                    if (MathEx.AABBIntersects(ref aabb, ref t))
                        res.Add(t);
                }
        }
        public override void SearchDF(ref BoundingSphere ball, List<Triangle> res)
        {
            if (MathEx.BoundingSphereIntersects(ref ball, ref aabb))// ball.Intersect(ref aAABB))
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                        children[i].SearchDF(ref ball, res);
                }
                else
                {
                    Triangle t = (Triangle)faceData;
                    if (MathEx.BoundingSphereIntersects(ref ball, ref t))
                    {
                        res.Add(t);
                    }
                    //if (ball.Intersect(ref t)) res.Add(t);
                }
        }


        public override void IntersectDF(AABBTreeNode cdTree, List<DirectDetectData> res)
        {
            //if (aAABB.Intersect(cdTree.aAABB))
            if (children != null)
                for (int i = 0; i < children.Length; i++)
                {
                    AABBTreeNode ch = (AABBTreeNode)children[i];
                    if (MathEx.AABBIntersects(ref cdTree.aabb, ref ch.aabb))//(cdTree.aAABB.Intersect(ref ch.aAABB))
                        cdTree.IntersectDF(ch, res); //相互遍历
                }
            else
            {
                Triangle t = (Triangle)faceData;
                cdTree.IntersectDF(ref t, res);
            }
        }
        public override void IntersectDF(BBTreeNode cdTree, List<DirectDetectData> res)
        {
            //if (aAABB.Intersect(cdTree.bBall.vCentre, cdTree.bBall.dRange))
            if (children != null)
                for (int i = 0; i < children.Length; i++)
                {
                    AABBTreeNode ch = (AABBTreeNode)children[i];                    
                    if (MathEx.BoundingSphereIntersects(ref cdTree.ball, ref ch.aabb))//(cdTree.bBall.Intersect(ref ch.aAABB))
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
            Queue<AABBTreeNode> a = new Queue<AABBTreeNode>();
            Queue<AABBTreeNode> b = new Queue<AABBTreeNode>();
            a.Enqueue(this);
            b.Enqueue(cdTree);

            throw new Exception("The method or operation is not implemented.");
        }
        //[Obsolete()]
        public override void IntersectBF(BBTreeNode cdTree, List<DirectDetectData> res)
        {
            Queue<AABBTreeNode> a = new Queue<AABBTreeNode>();
            Queue<BBTreeNode> b = new Queue<BBTreeNode>();

            a.Enqueue(this);
            b.Enqueue(cdTree);

            while (a.Count > 0)
            {
                AABBTreeNode nodea = a.Dequeue();
                //AABBTreeNode nodeb = b.Dequeue();

                // a/b anainst each b/a , if overlaps add a/b
                // a or b depends on Queue.Count

            }

            throw new Exception("The method or operation is not implemented.");
        }

        public override void Update()
        {
            throw new NotImplementedException();
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
            return sizeof(int) * 2 + sizeof(BoundingBox) + chSize;
        }
    }
}
