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
using System.IO;
using System.Text;
using Apoc3D.Core;

namespace Apoc3D.Vfs
{
    /// <summary>
    ///  表示资源的位置
    /// </summary>
    public abstract class ResourceLocation
    {
        string name;
        protected int size;


        public ResourceLocation(string name, int s)
        {
            this.name = name;
            size = s;
        }

        public string Name
        {
            get { return name; }
        }

        public abstract bool IsReadOnly { get; }
        public abstract Stream GetStream { get; }

        public int Size
        {
            get { return size; }
        }

        public override string ToString()
        {
            return name;
        }
        public override int GetHashCode()
        {
            return Resource.GetHashCode(name);
        }
    }

    /// <summary>
    /// 记录一个文件的位置信息，FileSystem查询结果。
    /// </summary>
    public class FileLocation : ResourceLocation
    {
        Archive parent;
        string path;

        //int offset;

        Stream stm;

        public FileLocation(FileLocation fl)
            : base(fl.Name, fl.size)
        {
            this.parent = fl.parent;
            this.path = fl.path;
            //this.offset = fl.offset;
        }

        public FileLocation(string filePath)
            : this(filePath, (int)new FileInfo(filePath).Length)
        { }

        protected FileLocation(string filePath, int size)
            : base(filePath, size)
        {
            path = filePath;

            // 必须是绝对路径
            if (!System.IO.Path.IsPathRooted(filePath))
                throw new ArgumentException();
        }
        public FileLocation(Archive pack, string filePath, Stream stm)
            : base(filePath, (int)stm.Length)
        {
            parent = pack;
            path = filePath;

            //offset = 0;
            size = (int)stm.Length;
            this.stm = stm;

        }

        /// <summary>
        /// 获得文件流。对于资源包中的文件，以VirtualStream提供
        /// </summary>
        /// <remarks>资源包中的用的流</remarks>
        public override Stream GetStream
        {
            get
            {
                if (stm != null)
                {
                    return stm;
                }

                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }


        ///// <summary>
        /////  文件在资源包中的局部offset
        ///// </summary>
        //public int Offset
        //{
        //    get { return offset; }
        //}
        public bool IsInArchive
        {
            get { return parent != null; }
        }
        /// <summary>
        ///  文件完整路径（包括资源包）。
        /// </summary>
        public string Path
        {
            get { return path; }
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }
    }

    /// <summary>
    ///  表示内存中的资源的位置
    /// </summary>
    public unsafe class MemoryLocation : ResourceLocation
    {
        void* data;
        //[CLSCompliant(false)]
        public MemoryLocation(void* pos, int size)
            : base("Addr: " + ((int)pos).ToString(), size)
        {
            data = pos;
        }

        public MemoryLocation(IntPtr pos, int size)
            : base("Addr: " + ((int)pos).ToString(), size)
        {
            data = pos.ToPointer();
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Stream GetStream
        {
            get
            {
                return new MemoryStream(data, size);
            }
        }

        //public override bool AutoClose
        //{
        //    get { return true; }
        //}
    }

    /// <summary>
    ///  表示流中的资源的位置
    /// </summary>
    public class StreamedLocation : ResourceLocation
    {
        Stream stream;
        long oldPosition;

        public StreamedLocation(Stream stm)
            : base("Stream: " + new object().GetHashCode().ToString(), (int)stm.Length)
        {
            oldPosition = stm.Position;
            stream = stm;
        }

        public override bool IsReadOnly
        {
            get { return !stream.CanWrite; }
        }

        public override Stream GetStream
        {
            get
            {
                stream.Position = oldPosition;
                return stream;
            }
        }


    }
}
