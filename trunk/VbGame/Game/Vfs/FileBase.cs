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
using System.IO;

namespace Apoc3D.Vfs
{
    public abstract class FileBase
    {

        protected bool isInArchive;
        protected int fileSize;
        protected string fileName;
        protected string filePath;

        protected FileBase(string file, int size, bool isinMix)
        {
            isInArchive = isinMix;
            fileName = Path.GetFileName(file);
            filePath = file;
            fileSize = size;
        }

        public bool IsInArchive
        {
            get { return isInArchive; }
        }
        public int FileSize
        {
            get { return fileSize; }
        }
        public string FileName
        {
            get { return fileName; }
        }
        public string FilePath
        {
            get { return filePath; }
        }

    }

}
