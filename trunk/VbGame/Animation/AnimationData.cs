#region File Description
//-----------------------------------------------------------------------------
// SkinningData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Text;


namespace Apoc3D.Graphics.Animation
{
    #region 必备
    public class ContentBinaryWriter : BinaryWriter
    {
        //bool closeStream = true;

        public ContentBinaryWriter(FileStream rl) :
            this(rl, Encoding.Default)
        { }
        public ContentBinaryWriter(Stream output) : base(output) { }

        public ContentBinaryWriter(Stream output, Encoding encoding) : base(output, encoding) { }

        //public bool AutoCloseStream
        //{
        //    get { return closeStream; }
        //    set { closeStream = value; }
        //}
        //public override void Close()
        //{
        //    base.Close();
        //}

        public void WriteStringUnicode(string str)
        {
            if (str == null)
                str = string.Empty;
            int len = str.Length;
            Write(len);

            for (int i = 0; i < len; i++)
            {
                Write((ushort)str[i]);
            }
        }

        public void Write(ref Matrix mat)
        {
            Write(mat.M11);
            Write(mat.M12);
            Write(mat.M13);
            Write(mat.M14);
            Write(mat.M21);
            Write(mat.M22);
            Write(mat.M23);
            Write(mat.M24);
            Write(mat.M31);
            Write(mat.M32);
            Write(mat.M33);
            Write(mat.M34);
            Write(mat.M41);
            Write(mat.M42);
            Write(mat.M43);
            Write(mat.M44);
        }
        public void Write(Matrix mat)
        {
            Write(mat.M11);
            Write(mat.M12);
            Write(mat.M13);
            Write(mat.M14);
            Write(mat.M21);
            Write(mat.M22);
            Write(mat.M23);
            Write(mat.M24);
            Write(mat.M31);
            Write(mat.M32);
            Write(mat.M33);
            Write(mat.M34);
            Write(mat.M41);
            Write(mat.M42);
            Write(mat.M43);
            Write(mat.M44);
        }



        /// <summary>
        ///  写入一个BinaryDataWriter数据块。
        /// </summary>
        /// <returns></returns>
        public void Write(BinaryDataWriter data)
        {
            Write(0); //占个位置
            Flush();

            long start = BaseStream.Position;

            data.Save(new VirtualStream(BaseStream, BaseStream.Position));

            long end = BaseStream.Position;
            int size = (int)(end - start);

            BaseStream.Position = start - 4;
            Write(size);
            BaseStream.Position = end;
        }
    }

    public class ContentBinaryReader : BinaryReader
    {
        //bool closeStream = true;


        public ContentBinaryReader(Stream src)
            : base(src)
        { }

        public ContentBinaryReader(Stream src, Encoding enc)
            : base(src, enc)
        { }

        //public bool AutoCloseStream
        //{
        //    get { return closeStream; }
        //    set { closeStream = value; }
        //}

        //public override void Close()
        //{
        //    base.Close();
        //}

        public void ReadMatrix(out Matrix mat)
        {
            mat.M11 = ReadSingle();
            mat.M12 = ReadSingle();
            mat.M13 = ReadSingle();
            mat.M14 = ReadSingle();
            mat.M21 = ReadSingle();
            mat.M22 = ReadSingle();
            mat.M23 = ReadSingle();
            mat.M24 = ReadSingle();
            mat.M31 = ReadSingle();
            mat.M32 = ReadSingle();
            mat.M33 = ReadSingle();
            mat.M34 = ReadSingle();
            mat.M41 = ReadSingle();
            mat.M42 = ReadSingle();
            mat.M43 = ReadSingle();
            mat.M44 = ReadSingle();
        }

        public string ReadStringUnicode()
        {
            int len = ReadInt32();
            char[] chars = new char[len];
            for (int i = 0; i < len; i++)
            {
                chars[i] = (char)ReadUInt16();
            }
            //char[] chars = ReadChars(len);
            return new string(chars);
        }

        public Matrix ReadMatrix()
        {
            Matrix mat;
            mat.M11 = ReadSingle();
            mat.M12 = ReadSingle();
            mat.M13 = ReadSingle();
            mat.M14 = ReadSingle();
            mat.M21 = ReadSingle();
            mat.M22 = ReadSingle();
            mat.M23 = ReadSingle();
            mat.M24 = ReadSingle();
            mat.M31 = ReadSingle();
            mat.M32 = ReadSingle();
            mat.M33 = ReadSingle();
            mat.M34 = ReadSingle();
            mat.M41 = ReadSingle();
            mat.M42 = ReadSingle();
            mat.M43 = ReadSingle();
            mat.M44 = ReadSingle();
            return mat;
        }


        /// <summary>
        ///  读取一个BinaryDataReader数据块。
        /// </summary>
        /// <returns></returns>
        public BinaryDataReader ReadBinaryData()
        {
            int size = ReadInt32();

            VirtualStream vs = new VirtualStream(BaseStream, BaseStream.Position, size);
            return new BinaryDataReader(vs);
        }

        public void Close(bool closeBaseStream)
        {
            base.Dispose(closeBaseStream);
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
    /// <summary>
    ///   定义一种由若干“键”—“值”组成的集合的存储方式  的写入器
    /// </summary>
    public unsafe class BinaryDataWriter : IDisposable
    {
        class Entry
        {
            public string name;
            public System.IO.MemoryStream buffer;

            public Entry(string name)
            {
                this.name = name;
                buffer = new System.IO.MemoryStream();
            }

        }

        bool disposed;
        Dictionary<string, Entry> positions = new Dictionary<string, Entry>();
        byte[] buffer = new byte[sizeof(decimal)];

        public ContentBinaryWriter AddEntry(string name)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);
            return new ContentBinaryWriter(new VirtualStream(ent.buffer, 0));
        }
        public Stream AddEntryStream(string name)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);
            return new VirtualStream(ent.buffer, 0);
        }

        //public int GetSize()
        //{
        //    int size = 0;
        //    Dictionary<string, Entry>.ValueCollection vals = positions.Values;
        //    foreach (Entry e in vals)
        //    {
        //        size += (int)e.buffer.Length;
        //    }
        //    return size;
        //}

        public void AddEntry(string name, int value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            ent.buffer.Write(buffer, 0, sizeof(int));
        }
        public void AddEntry(string name, uint value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            ent.buffer.Write(buffer, 0, sizeof(uint));
        }
        public void AddEntry(string name, short value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);

            ent.buffer.Write(buffer, 0, sizeof(short));
        }
        public void AddEntry(string name, ushort value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);

            ent.buffer.Write(buffer, 0, sizeof(ushort));
        }
        public void AddEntry(string name, long value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            ent.buffer.Write(buffer, 0, sizeof(long));
        }
        public void AddEntry(string name, ulong value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            ent.buffer.Write(buffer, 0, sizeof(ulong));
        }
        public void AddEntry(string name, float value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            uint num = *((uint*)&value);
            buffer[0] = (byte)num;
            buffer[1] = (byte)(num >> 8);
            buffer[2] = (byte)(num >> 16);
            buffer[3] = (byte)(num >> 24);

            ent.buffer.Write(buffer, 0, sizeof(float));
        }
        public void AddEntry(string name, double value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            ulong num = *((ulong*)&value);
            buffer[0] = (byte)num;
            buffer[1] = (byte)(num >> 8);
            buffer[2] = (byte)(num >> 16);
            buffer[3] = (byte)(num >> 24);
            buffer[4] = (byte)(num >> 32);
            buffer[5] = (byte)(num >> 40);
            buffer[6] = (byte)(num >> 48);
            buffer[7] = (byte)(num >> 56);

            ent.buffer.Write(buffer, 0, sizeof(float));
        }
        public void AddEntry(string name, bool value)
        {
            Entry ent = new Entry(name);
            positions.Add(name, ent);

            buffer[0] = value ? ((byte)1) : ((byte)0);

            ent.buffer.Write(buffer, 0, sizeof(bool));
        }

        public ContentBinaryWriter GetData(string name)
        {
            Entry ent = positions[name];
            return new ContentBinaryWriter(new VirtualStream(ent.buffer, 0));
        }

        public void SetData(string name, int value)
        {
            Entry ent = positions[name];

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(int));
        }
        public void SetData(string name, uint value)
        {
            Entry ent = positions[name];

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(uint));
        }
        public void SetData(string name, short value)
        {
            Entry ent = positions[name];

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(short));
        }
        public void SetData(string name, ushort value)
        {
            Entry ent = positions[name];

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(ushort));
        }
        public void SetData(string name, long value)
        {
            Entry ent = positions[name];

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(long));
        }
        public void SetData(string name, ulong value)
        {
            Entry ent = positions[name];

            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(ulong));
        }
        public void SetData(string name, float value)
        {
            Entry ent = positions[name];

            uint num = *((uint*)&value);
            buffer[0] = (byte)num;
            buffer[1] = (byte)(num >> 8);
            buffer[2] = (byte)(num >> 16);
            buffer[3] = (byte)(num >> 24);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(float));
        }
        public void SetData(string name, double value)
        {
            Entry ent = positions[name];

            ulong num = *((ulong*)&value);
            buffer[0] = (byte)num;
            buffer[1] = (byte)(num >> 8);
            buffer[2] = (byte)(num >> 16);
            buffer[3] = (byte)(num >> 24);
            buffer[4] = (byte)(num >> 32);
            buffer[5] = (byte)(num >> 40);
            buffer[6] = (byte)(num >> 48);
            buffer[7] = (byte)(num >> 56);


            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(float));
        }
        public void SetData(string name, bool value)
        {
            Entry ent = positions[name];

            buffer[0] = value ? ((byte)1) : ((byte)0);

            ent.buffer.Position = 0;
            ent.buffer.Write(buffer, 0, sizeof(bool));
        }


        public void Save(Stream stm)
        {
            ContentBinaryWriter bw = new ContentBinaryWriter(stm, Encoding.Default);

            bw.Write(positions.Count);

            foreach (KeyValuePair<string, Entry> e in positions)
            {
                bw.WriteStringUnicode(e.Key);
                bw.Write((int)e.Value.buffer.Length);
                bw.Flush();
                e.Value.buffer.WriteTo(stm);
            }
            bw.Close();
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (KeyValuePair<string, Entry> e in positions)
                {
                    e.Value.buffer.Dispose();
                }
                positions.Clear();
                disposed = true;
            }
            else
            {
                throw new ObjectDisposedException(this.ToString());
            }
        }

        #endregion

        ~BinaryDataWriter()
        {
            if (!disposed)
                Dispose();
        }
    }

    /// <summary>
    ///  定义一种由若干“键”—“值”组成的集合的存储方式  的读取器
    ///  “键”为字符串，“值”为二进制数据块。
    ///  
    ///  意义：可以不按先后顺序将数据存储，可以方便增添或减少存储的项目。
    /// </summary>
    public unsafe class BinaryDataReader
    {
        /// <summary>
        ///  定义一个“键”—“值”的存储项
        /// </summary>
        struct Entry
        {
            public string name;
            public int offset;
            public int size;

            public Entry(string name, int offset, int size)
            {
                this.name = name;
                this.offset = offset;
                this.size = size;
            }
        }


        int sectCount;
        Dictionary<string, Entry> positions;
        Stream stream;

        byte[] buffer;

        public BinaryDataReader(Stream stm)
        {
            stream = stm;
            buffer = new byte[sizeof(decimal)];

            ContentBinaryReader br = new ContentBinaryReader(stm, Encoding.Default);

            // 读出所有块
            sectCount = br.ReadInt32();
            positions = new Dictionary<string, Entry>(sectCount);

            for (int i = 0; i < sectCount; i++)
            {
                string name = br.ReadStringUnicode();
                int size = br.ReadInt32();

                positions.Add(name, new Entry(name, (int)br.BaseStream.Position, size));

                br.BaseStream.Position += size;
            }
            br.Close();
        }

        public bool Contains(string name)
        {
            return positions.ContainsKey(name);
        }

        public ContentBinaryReader TryGetData(string name)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                return new ContentBinaryReader(new VirtualStream(stream, ent.offset, ent.size));
            }
            return null;
        }
        public ContentBinaryReader GetData(string name)
        {
            Entry ent = positions[name];
            return new ContentBinaryReader(new VirtualStream(stream, ent.offset, ent.size));
        }
        public Stream GetDataStream(string name)
        {
            Entry ent = positions[name];
            return new VirtualStream(stream, ent.offset, ent.size);
        }
        public int GetDataInt32(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(int));

            return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
        }
        //[CLSCompliant(false)]
        public uint GetDataUInt32(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(uint));

            return (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
        }

        public short GetDataInt16(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(short));

            return (short)(buffer[0] | (buffer[1] << 8));
        }
        //[CLSCompliant(false)]
        public ushort GetDataUInt16(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(ushort));

            return (ushort)(buffer[0] | (buffer[1] << 8));
        }

        public long GetDataInt64(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(long));

            uint num = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
            uint num2 = (uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));
            return (long)((num2 << 32) | num);
        }
        //[CLSCompliant(false)]
        public ulong GetDataUInt64(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(ulong));

            uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
            uint num2 = (uint)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 16)) | (buffer[7] << 24));
            return ((num2 << 32) | num);
        }

        public bool GetDataBool(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(bool));

            return (buffer[0] != 0);
        }

        public float GetDataSingle(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(float));

            uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
            return *(((float*)&num));
        }
        public float GetDataDouble(string name)
        {
            Entry ent = positions[name];

            stream.Position = ent.offset;
            stream.Read(buffer, 0, sizeof(float));

            uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
            uint num2 = (uint)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 16)) | (buffer[7] << 24));
            ulong num3 = (num2 << 32) | num;
            return *(((float*)&num3));

        }




        public int GetDataInt32(string name, int def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {//= positions[name];

                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(int));

                return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
            }
            return def;
        }
        public uint GetDataUInt32(string name, uint def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(uint));

                return (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
            }
            return def;
        }

        public short GetDataInt16(string name, short def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(short));

                return (short)(buffer[0] | (buffer[1] << 8));
            }
            return def;
        }
        public ushort GetDataUInt16(string name, ushort def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {

                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(ushort));

                return (ushort)(buffer[0] | (buffer[1] << 8));
            }
            return def;
        }

        public long GetDataInt64(string name, long def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(long));

                uint num = (uint)(buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24));
                uint num2 = (uint)(buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24));
                return (long)((num2 << 32) | num);
            }
            return def;
        }
        public ulong GetDataUInt64(string name, ulong def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(ulong));

                uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
                uint num2 = (uint)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 16)) | (buffer[7] << 24));
                return ((num2 << 32) | num);
            }
            return def;
        }

        public bool GetDataBool(string name, bool def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(bool));

                return (buffer[0] != 0);
            }
            return def;
        }

        public float GetDataSingle(string name, float def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(float));

                uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
                return *(((float*)&num));
            }
            return def;
        }
        public float GetDataDouble(string name, float def)
        {
            Entry ent;
            if (positions.TryGetValue(name, out ent))
            {
                stream.Position = ent.offset;
                stream.Read(buffer, 0, sizeof(float));

                uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 16)) | (buffer[3] << 24));
                uint num2 = (uint)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 16)) | (buffer[7] << 24));
                ulong num3 = (num2 << 32) | num;
                return *(((float*)&num3));
            }
            return def;
        }



        public void Close()
        {
            stream.Close();
        }


        public int GetChunkOffset(string name)
        {
            Entry ent = positions[name];
            return ent.offset;
        }
        public Stream BaseStream
        {
            get { return stream; }
        }
    }
    #endregion

    public class AnimationDataReader : ContentTypeReader<AnimationData>
    {
        static readonly string BindPoseTag = "BindPose";
        static readonly string BindPoseCountTag = "BindPoseCount";

        static readonly string InvBindPoseTag = "InvBindPose";
        static readonly string InvBindPoseCountTag = "InvBindPoseCount";

        static readonly string ModelAnimationClipTag = "ModelAnimationClip";
        static readonly string ModelAnimationClipCountTag = "ModelAnimationClipCount";

        static readonly string RootAnimationClipTag = "RootAnimationClip";
        static readonly string RootAnimationClipCountTag = "RootAnimationClipCount";

        static readonly string BoneHierarchyTag = "BoneHierarchy";
        static readonly string BoneHierarchyCountTag = "BoneHierarchyCount";

        protected override AnimationData Read(ContentReader input, AnimationData existingInstance)
        {
            if (existingInstance != null) 
            {
                return existingInstance;
            }

            List<Matrix> bindPose = null;
            List<Matrix> invBindPose = null;
            Dictionary<string, ModelAnimationClip> modelAnim = null;
            Dictionary<string, ModelAnimationClip> rootAnim = null;
            List<int> skeleHierarchy = null;

            BinaryDataReader ad;
            int size = input.ReadInt32();

            VirtualStream vs = new VirtualStream(input.BaseStream, input.BaseStream.Position, size);
            ad =  new BinaryDataReader(vs);

            #region InvBindPoseTag

            
            if (ad.Contains(InvBindPoseCountTag))
            {
                int count = ad.GetDataInt32(InvBindPoseCountTag);
                invBindPose = new List<Matrix>(count);

                ContentBinaryReader br2 = ad.GetData(InvBindPoseTag);
                for (int i = 0; i < count; i++)
                {
                    invBindPose.Add(br2.ReadMatrix());
                }
                br2.Close();
            }

            #endregion

            #region AnimationClipTag

            if (ad.Contains(ModelAnimationClipCountTag))
            {
                int count = ad.GetDataInt32(ModelAnimationClipCountTag);

                modelAnim = new Dictionary<string, ModelAnimationClip>(count);

                ContentBinaryReader br2 = ad.GetData(ModelAnimationClipTag);

                for (int i = 0; i < count; i++)
                {
                    string key = br2.ReadStringUnicode();

                    TimeSpan duration = TimeSpan.FromSeconds(br2.ReadDouble());

                    int frameCount = br2.ReadInt32();
                    List<ModelKeyframe> frames = new List<ModelKeyframe>(frameCount);
                    for (int j = 0; j < frameCount; j++)
                    {
                        int bone = br2.ReadInt32();
                        TimeSpan totalSec = TimeSpan.FromSeconds(br2.ReadDouble());
                        Matrix transform = br2.ReadMatrix();

                        ModelKeyframe frame = new ModelKeyframe(bone, totalSec, transform);
                        frames.Add(frame);
                    }

                    ModelAnimationClip clip = new ModelAnimationClip(duration, frames);

                    modelAnim.Add(key, clip);
                }
                br2.Close();
            }


            #endregion

            #region RootAnimationClipTag

            if (ad.Contains(RootAnimationClipCountTag))
            {
                int count = ad.GetDataInt32(RootAnimationClipCountTag);

                rootAnim = new Dictionary<string, ModelAnimationClip>(count);

                ContentBinaryReader br2 = ad.GetData(RootAnimationClipTag);

                for (int i = 0; i < count; i++)
                {
                    string key = br2.ReadStringUnicode();

                    TimeSpan duration = TimeSpan.FromSeconds(br2.ReadDouble());

                    int frameCount = br2.ReadInt32();
                    List<ModelKeyframe> frames = new List<ModelKeyframe>(frameCount);
                    for (int j = 0; j < frameCount; j++)
                    {
                        int bone = br2.ReadInt32();
                        TimeSpan totalSec = TimeSpan.FromSeconds(br2.ReadDouble());
                        Matrix transform = br2.ReadMatrix();

                        ModelKeyframe frame = new ModelKeyframe(bone, totalSec, transform);
                        frames.Add(frame);
                    }

                    ModelAnimationClip clip = new ModelAnimationClip(duration, frames);
                    rootAnim.Add(key, clip);
                }
                br2.Close();
            }

            #endregion

            #region BoneHierarchyTag

            if (ad.Contains(BoneHierarchyCountTag))
            {
                int count = ad.GetDataInt32(BoneHierarchyCountTag);
                skeleHierarchy = new List<int>(count);


                ContentBinaryReader br2 = ad.GetData(BoneHierarchyTag);
                for (int i = 0; i < count; i++)
                {
                    skeleHierarchy.Add(br2.ReadInt32());
                }

                br2.Close();
            }


            #endregion

            return new AnimationData(modelAnim, rootAnim, bindPose, invBindPose, skeleHierarchy);
        }
    }

    /// <summary>
    /// Combines all the data needed to render and animate a skinned object.
    /// This is typically stored in the Tag property of the Model being animated.
    /// </summary>
    public class AnimationData
    {
        /// <summary>
        /// Gets a collection of animation clips that operate on the root of the object.
        /// These are stored by name in a dictionary, so there could for instance be 
        /// clips for "Walk", "Run", "JumpReallyHigh", etc.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, ModelAnimationClip> RootAnimationClips { get; private set; }

        /// <summary>
        /// Gets a collection of model animation clips. These are stored by name in a
        /// dictionary, so there could for instance be clips for "Walk", "Run",
        /// "JumpReallyHigh", etc.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, ModelAnimationClip> ModelAnimationClips { get; private set; }

        /// <summary>
        /// Bindpose matrices for each bone in the skeleton,
        /// relative to the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> BindPose { get; private set; }

        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; }

        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<int> SkeletonHierarchy { get; private set; }

        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public AnimationData(
            Dictionary<string, ModelAnimationClip> modelAnimationClips,
            Dictionary<string, ModelAnimationClip> rootAnimationClips,
            List<Matrix> bindPose,
            List<Matrix> inverseBindPose,
            List<int> skeletonHierarchy)
        {
            ModelAnimationClips = modelAnimationClips;
            RootAnimationClips = rootAnimationClips;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            SkeletonHierarchy = skeletonHierarchy;
        }
        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        public AnimationData(AnimationDataReader reader)
        {
        }
    }
}
