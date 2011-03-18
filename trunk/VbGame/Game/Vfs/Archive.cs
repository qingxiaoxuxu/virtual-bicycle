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
using System.Runtime.InteropServices;
using System.Text;

namespace Apoc3D.Vfs
{
    /// <summary>
    ///  为文件包定义抽象基类
    /// </summary>
    public abstract class Archive : FileBase
    {
        protected Archive(string file, int size, bool isinArchive)
            : base(file, size, isinArchive)
        { }


        public abstract int FileCount
        {
            get;
        }

        public abstract Stream GetEntryStream(string file);

        public abstract void Dispose(bool disposing);

        public bool Disposed
        {
            get;
            private set;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Dispose(true);

                Disposed = true;
            }
            else
            {
                throw new ObjectDisposedException(ToString());
            }
        }
        
    }


    /// <summary>
    ///  为文件包定义抽象工厂
    /// </summary>
    public abstract class ArchiveFactory : IAbstractFactory<Archive, FileLocation>
    {

        #region IAbstractFactory<Archive,FileLocation> 成员

        public abstract Archive CreateInstance(string file);

        public abstract Archive CreateInstance(FileLocation fl);

        public abstract string Type
        {
            get;
        }

        #endregion
    }


    
}
