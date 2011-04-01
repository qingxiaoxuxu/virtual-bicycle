using System;
using System.Collections.Generic;
 
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace VbClient.Net
{
    public class DataPort
    {
        //服务端数据端口端口号
        public int remotePort;
        //服务端数据端口IP
        public IPAddress remoteIp;
        //数据端Socket--用于和服务端数据端口通信
        private TcpClient dataTrans;
        //数据端Socket
        private TcpClient client;
        //网络流
        private NetworkStream data_ns;
        //文件路径
        private string filePath = "";
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
                if (File.Exists(filePath))
                {
                    fileLength=new FileInfo(filePath).Length;
                }
                else
                {
                    fileLength = 0;
                }
            }
        }
        //文件大小
        public long fileLength = 0;
        //是否用于传送数据
        //false 表示传出数据
        //true 表示接受数据
        public bool IsSend = false;

        #region 事件定义

        /// <summary>
        /// 端口收回事件
        /// </summary>
        /// <param name="port"></param>
        public delegate void portUsed(DataPort port);
        public event portUsed portRecycle;
        /// <summary>
        /// 文件传输开始
        /// </summary>
        public event EventHandler FileSendBegin;
        /// <summary>
        /// 文件传输结束
        /// </summary>
        public event EventHandler FileSendEnd;
        /// <summary>
        /// 文件接收开始
        /// </summary>
        public event EventHandler FileReceiveBegin;
        /// <summary>
        /// 文件接收结束
        /// </summary>
        public event EventHandler FileReceiveEnd;
        /// <summary>
        /// 数据端口连接上
        /// </summary>
        public event EventHandler ConnectedDataPort;
        /// <summary>
        /// 数据端口连接错误
        /// </summary>
        public event ErrorEventHandler ConnectError;
        /// <summary>
        /// 文件传输大小错误(一般为网络中丢包)
        /// </summary>
        public event ErrorEventHandler FileSizeError;
        /// <summary>
        /// 传送文件比例
        /// </summary>
        public event ReturnVal SendFileRate;
        /// <summary>
        /// 接收文件比例
        /// </summary>
        public event ReturnVal RecieveFileRate;
        #endregion


        /// <summary>
        /// 数据端口构造函数 
        /// </summary>
        public DataPort()
        {
        }
        /// <summary>
        /// 数据端口连接服务端
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool Connect()
        {
            try
            {
                client=  new TcpClient();
                client.BeginConnect(remoteIp, remotePort, new AsyncCallback(connect_callBack), client);
                return true;
            }
            catch (Exception e)
            {
                if (ConnectError != null)
                {
                    ConnectError(this, null);
                }
                return false;
            }
        }

        /// <summary>
        /// 数据Socket连接服务器的回调函数
        /// </summary>
        /// <param name="asy"></param>
        public void connect_callBack(IAsyncResult asy)
        {
            dataTrans = (TcpClient)asy.AsyncState;
            dataTrans.EndConnect(asy);
            data_ns = dataTrans.GetStream();
            data_ns = dataTrans.GetStream();
            if (ConnectedDataPort != null)
            {
                ConnectedDataPort(this, null);
            }
            if (IsSend)
            {
                SendData();
            }
            else
            {
                ReceiveData();
            }
        }
        /// <summary>
        /// 接收文件
        /// </summary>
        private void ReceiveData()
        {
            if (FileReceiveBegin != null)
            {
                FileReceiveBegin(this, null);
            }
            long total = fileLength;
            int byteRead;
            Byte[] buffer = new Byte[1024];
            FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            byteRead = data_ns.Read(buffer, 0, 1024);

            while (byteRead > 0)
            {
                total -= byteRead;
                file.Write(buffer, 0, byteRead);
                Array.Clear(buffer, 0, 1024);
                byteRead = data_ns.Read(buffer, 0, 1024);
                if (RecieveFileRate != null)
                {
                    RecieveFileRate(1.0 - total / fileLength);
                }
            }

            data_ns.Dispose();
            dataTrans.Close();
            file.Close();

            if (FileReceiveEnd != null)
            {
                FileReceiveEnd(filePath, null);
            }

            if (portRecycle != null)
            {
                portRecycle(this);
            }
            if (total != 0L)
            {
                if (FileSizeError != null)
                {
                    FileSizeError(this, null);
                }
            }
        }
        /// <summary>
        /// 传输文件
        /// </summary>
        private void SendData()
        {
            if (FileSendBegin != null)
            {
                FileSendBegin(this, null);
            }
            long totle=0L;
            int byteRead;
            Byte[] buffer = new Byte[1024];
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            byteRead = file.Read(buffer, 0, 1024);
            while (byteRead > 0)
            {
                data_ns.Write(buffer, 0, byteRead);
                Array.Clear(buffer, 0, 1024);
                byteRead = file.Read(buffer, 0, 1024);
                totle+=byteRead;
                if (SendFileRate != null)
                {
                    SendFileRate(totle / fileLength);
                }
            }

            data_ns.Dispose();
            dataTrans.Close();
            file.Close();

            if (FileSendEnd != null)
            {
                FileSendEnd(filePath, null);
            }

            if (portRecycle != null)
                portRecycle(this);
        }
    }
    public delegate void ReturnVal(double rate);
}
