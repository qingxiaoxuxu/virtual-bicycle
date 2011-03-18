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
    public unsafe class MemoryStream : Stream
    {
        int length;
        byte* data;
        int position;

        protected IntPtr InternalPointer
        {
            get { return new IntPtr(data); }
        }

        public MemoryStream(void* data, int len)
        {
            this.data = (byte*)data;
            length = len;

        }

        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return true; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        { }

        public override long Length
        {
            get { return length; }
        }

        public override long Position
        {
            get { return position; }
            set { position = (int)value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset + count > length)
            {
                count = length - offset;
            }

            fixed (byte* dst = &buffer[0])
            {
                Memory.Copy(data + position, dst, count);
            }
            position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = (int)offset;
                    break;
                case SeekOrigin.Current:
                    position += (int)offset;
                    break;
                case SeekOrigin.End:
                    position = length + (int)offset;
                    break;
            }
            if (position < 0)
                position = 0;
            if (position > length)
                position = length;
            return position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset + count > length)
            {
                throw new EndOfStreamException();
            }
            fixed (byte* src = &buffer[0])
            {
                Memory.Copy(src, data + position, count);
            }
            position += count;
        }
    }

    /// <summary>
    ///  虚拟流，通常用来读取其他流之中的一段数据而不影响那个流。
    /// </summary>
    public class VirtualStream : Stream
    {
        Stream stream;

        long length;
        long baseOffset;

        bool isOutput;


        public Stream BaseStream
        {
            get { return stream; }
        }

        public VirtualStream(Stream stream)
        {
            isOutput = true;
            this.stream = stream;
            this.length = stream.Length;
            this.baseOffset = 0;
            stream.Position = 0;
        }
        public VirtualStream(Stream stream, long baseOffset)
        {
            isOutput = true;
            this.stream = stream;
            this.baseOffset = 0;
            stream.Position = baseOffset;
        }
        public VirtualStream(Stream stream, long baseOffset, long length)
        {
            stream.Position = baseOffset;

            this.stream = stream;
            this.length = length;
            this.baseOffset = baseOffset;
            stream.Position = baseOffset;
        }


        public bool IsOutput
        {
            get { return isOutput; }
        }
        public long BaseOffset
        {
            get { return baseOffset; }
        }
        public override bool CanRead
        {
            get { return stream.CanRead; }
        }
        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }
        public override bool CanWrite
        {
            get { return stream.CanWrite; }
        }
        public override bool CanTimeout
        {
            get { return stream.CanTimeout; }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Length
        {
            get { return isOutput ? stream.Length : length; }
        }

        public long AbsolutePosition
        {
            get { return stream.Position; }
        }
        public override long Position
        {
            get
            {
                return stream.Position - baseOffset;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                if (value > Length)
                    throw new EndOfStreamException();
                stream.Position = value + baseOffset;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position + count > length)
            {
                count = (int)(length - Position);
            }
            if (count > 0)
            {
                return stream.Read(buffer, offset, count);
            }
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset > length)
                    {
                        offset = length;
                    }
                    if (offset < 0)
                    {
                        offset = 0;
                    }
                    break;
                case SeekOrigin.Current:
                    if (stream.Position + offset > baseOffset + length)
                    {
                        offset = baseOffset + length - stream.Position;
                    }
                    if (stream.Position + offset < baseOffset)
                    {
                        offset = baseOffset - stream.Position;
                    }
                    break;
                case SeekOrigin.End:
                    if (offset > 0)
                    {
                        offset = 0;
                    }
                    if (offset < -length)
                    {
                        offset = -length;
                    }
                    break;
            }
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
            if (isOutput)
                length += count;
        }
        public override void WriteByte(byte value)
        {
            stream.WriteByte(value);
            if (isOutput)
                length++;
        }

        public override void Close() { }
    }
}
