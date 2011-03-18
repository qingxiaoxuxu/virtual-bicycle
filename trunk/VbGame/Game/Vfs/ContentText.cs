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

    public class ContentStreamReader : StreamReader
    {
        //bool closeStream = true;

        //public override void Close()
        //{
        //    base.Close();
        //    //base.Dispose(closeStream);
        //}


        public ContentStreamReader(ResourceLocation rl)
            : this(rl.GetStream, Encoding.Default)
        {
            //closeStream = rl.AutoClose;
        }

        public ContentStreamReader(Stream stream)
            : base(stream, true)
        { }

        public ContentStreamReader(string path)
            : base(path, true)
        { }

        public ContentStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks)
            : base(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024)
        { }

        public ContentStreamReader(Stream stream, Encoding encoding)
            : base(stream, encoding, true, 1024)
        { }

        public ContentStreamReader(string path, bool detectEncodingFromByteOrderMarks)
            : base(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, 1024)
        { }

        public ContentStreamReader(string path, Encoding encoding)
            : base(path, encoding, true, 1024)
        { }

        public ContentStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, 1024)
        { }

        public ContentStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : base(path, encoding, detectEncodingFromByteOrderMarks, 1024)
        { }

        public ContentStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        { }

        public ContentStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        { }
    }

    public class ContentStreamWriter : StreamWriter
    {
        //bool closeStream = true;

        //public override void Close()
        //{
        //    base.Dispose(closeStream);
        //}

        //public bool AutoCloseStream
        //{
        //    get { return closeStream; }
        //    set { closeStream = value; }
        //}

        public ContentStreamWriter(ResourceLocation rl)
            : this(rl.GetStream, Encoding.Default)
        {
            //closeStream = rl.AutoClose;
        }

        public ContentStreamWriter(Stream stream)
            : base(stream)
        {
        }

        public ContentStreamWriter(string path)
            : base(path)
        {
        }

        public ContentStreamWriter(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
        }

        public ContentStreamWriter(string path, bool append)
            : base(path, append)
        {
        }

        public ContentStreamWriter(Stream stream, Encoding encoding, int bufferSize)
            : base(stream, encoding, bufferSize)
        {
        }

        public ContentStreamWriter(string path, bool append, Encoding encoding)
            : base(path, append, encoding)
        {
        }


        public ContentStreamWriter(string path, bool append, Encoding encoding, int bufferSize)
            : base(path, append, encoding, bufferSize)
        {
        }


    }
}
