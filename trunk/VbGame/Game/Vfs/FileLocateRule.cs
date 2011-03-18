/*
-----------------------------------------------------------------------------
This source file is part of Apoc3D Engine

Copyright (c) 2009+ Tao Games

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  if not, write to the Free Software Foundation, 
Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/copyleft/gpl.txt.

-----------------------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Apoc3D.Vfs
{
    /// <summary>
    ///  检查点包括若干路径
    ///  
    ///  查找文件时逐项检查
    /// </summary>
    public class LocateCheckPoint
    {
        struct Entry
        {
            public string Path;
            public string ArchivePath;

            public Entry(string path, string ap)
            {
                Path = path;
                ArchivePath = ap;
            }
        }

        List<Entry> pathList = new List<Entry>();

        /// <summary>
        ///  添加检查路径
        /// </summary>
        /// <param name="path"></param>
        public void AddPath(string path)
        {
            List<string> fullPath;
            List<string> arcPath;

            if (FileSystem.Instance.Exists(path, out fullPath, out arcPath))
            {
                for (int i = 0; i < fullPath.Count; i++)
                {
                    pathList.Add(new Entry(fullPath[i], arcPath[i]));
                }
            }
        }

        public void Clear()
        {
            pathList.Clear();
        }

        /// <summary>
        ///  索引为index的检查路径是否指向一个文件包
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HasArchivePath(int index)
        {
            return !string.IsNullOrEmpty(pathList[index].ArchivePath);
        }
        /// <summary>
        ///  获得索引为index的检查路径
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetPath(int index)        
        {
            return pathList[index].Path;
        }

        public string GetArchivePath(int index)
        {
            return pathList[index].ArchivePath;
        }

        /// <summary>
        ///  获得路径的数量
        /// </summary>
        public int Count
        {
            get { return pathList.Count; }
        }


        public bool SearchesCurrectArchiveSet
        {
            get;
            set;
        }
    }


    /// <summary>
    ///  定义查找文件的规则，就是检查目录或者文件包的次序
    /// </summary>
    public class FileLocateRule
    {
        static readonly string textures = "texture";
        static readonly string effects = "effect";

        public static FileLocateRule Textures
        {
            get;
            set;
        }
        public static FileLocateRule Effects
        {
            get;
            set;
        }
        public static FileLocateRule Default
        {
            get;
            private set;
        }

        static FileLocateRule() 
        {
            LocateCheckPoint[] pts = new LocateCheckPoint[1];
            pts[0] = new LocateCheckPoint();
            pts[0].AddPath(string.Empty);

            Default = new FileLocateRule(pts);

            /********************************************************************************/

            pts = new LocateCheckPoint[1];
            pts[0] = new LocateCheckPoint();
            pts[0].AddPath(textures);

            Textures = new FileLocateRule(pts);

            /********************************************************************************/

            pts = new LocateCheckPoint[1];
            pts[0] = new LocateCheckPoint();
            pts[0].AddPath(effects);

            Effects = new FileLocateRule(pts);

        }



        List<LocateCheckPoint> pathChkPt;

        public FileLocateRule()
        {
            pathChkPt = new List<LocateCheckPoint>();
        }


        public FileLocateRule(LocateCheckPoint[] checkPoints)
        {
            pathChkPt = new List<LocateCheckPoint>(checkPoints.Length + 1);
            for (int i = 0; i < checkPoints.Length; i++)
            {
                pathChkPt.Add(checkPoints[i]);
            }
        }

        public void AddCheckPoint(LocateCheckPoint coll)
        {
            pathChkPt.Add(coll);
        }

        public int Count
        {
            get { return pathChkPt.Count; }
        }
        public LocateCheckPoint this[int index]
        {
            get { return pathChkPt[index]; }
        }
    }
}
