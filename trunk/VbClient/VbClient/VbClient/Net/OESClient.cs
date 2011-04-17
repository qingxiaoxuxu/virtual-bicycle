using System;
using System.Collections.Generic;
 
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace VbClient.Net
{
    public class OESClient
    {
        //客户端Socket--用于和服务端通信
        private TcpClient client;
        //命令端口大小
        private static int bufferSize = 128;  
        //Byte数据数组
        private Byte[] buffer = new Byte[bufferSize];
        //网络流
        private NetworkStream ns;
        //字符串类型的原始消息
        private string raw_msg = String.Empty;
        //服务器地址
        public string server = ClientConfig.RemoteIp.ToString(); 
        //服务器端口
        public int portNum = ClientConfig.RemotePort;
        //客户端数据端口
        private DataPort port;
        /// <summary>
        /// 客户端数据端口
        /// </summary>
        public DataPort Port
        {
            get  
            {
                return port; 
            }
            set 
            {
                port = value;
            }
        }

        #region 事件定义
        /// <summary>
        /// 连接上服务器
        /// </summary>
        public event EventHandler ConnectedServer;
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event EventHandler ReceivedMsg;
        /// <summary>
        /// 接收到数据请求消息 客户端--->服务端
        /// </summary>
        public event EventHandler ReceivedDataRequest;
        /// <summary>
        /// 接收到数据发送消息 服务端--->客户端
        /// </summary>
        public event EventHandler ReceivedDataSubmit;
        /// <summary>
        /// 接收到文字消息.第一个参数为String类型,表示收到的消息
        /// </summary>
        public event EventHandler ReceivedTxt;
        /// <summary>
        /// 消息发送完成.第一个参数为String类型,表示发送出去的消息内容
        /// </summary>
        public event EventHandler WrittenMsg;
        /// <summary>
        /// 客户端连接服务端出现错误,第一个参数为错误信息
        /// </summary>
        public event ErrorEventHandler ConnectError;
        /// <summary>
        /// 接收时出现错误,第一个参数为错误信息(一般为文件路径不存在)
        /// </summary>
        public event ErrorEventHandler ReceiveError;
        /// <summary>
        /// 与服务端断开连接(一般为服务端网络出现问题)
        /// </summary>
        public event ErrorEventHandler DisConnectError;
        #endregion


        /// <summary>
        /// OESClient构造函数.会初始化命令端口和数据端口.
        /// </summary>
        public OESClient()
        {
            
        }
        /// <summary>
        /// 开始连接服务端
        /// </summary>
        /// <returns></returns>
        public bool InitializeClient()
        {
            try
            {
                client = new TcpClient();
                port = new DataPort();
                client.BeginConnect(IPAddress.Parse(server), portNum, new AsyncCallback(connect_callBack), client);
            }
            catch (Exception e)
            {
                if (ConnectError != null)
                {
                    ConnectError(e, null);
                }
                SendError("");
                return (false);
            }
            return (true);
        }

        /// <summary>
        /// 命令Socket连接服务器的回调函数
        /// </summary>
        /// <param name="asy">异步返回结果</param>
        private void connect_callBack(IAsyncResult asy)
        {
            try
            {
                client.EndConnect(asy);
                ns = client.GetStream();
                if (ConnectedServer != null)
                {
                    ConnectedServer(this, null);
                }
                ns.BeginRead(buffer, 0, bufferSize, new AsyncCallback(receive_callBack), client);
            }
            catch(Exception e)
            {
                if (ConnectError != null)
                {
                    ConnectError(e, null);
                }
                SendError("");
            }
        }
        /// <summary>
        /// 命令Socket接收信息的回调函数
        /// </summary>
        /// <param name="asy">异步返回结果</param>
        private void receive_callBack(IAsyncResult asy)
        {
            try
            {
                int result = ns.EndRead(asy);
                DispatchMessage();
            }
            catch (Exception e)
            {
                if (ReceiveError != null)
                {
                    ReceiveError(e, null);
                }
                SendError("");
            }
            try
            {
                ns.BeginRead(buffer, 0, bufferSize, new AsyncCallback(receive_callBack), client);
            }
            catch
            {
                client = new TcpClient();
                if (DisConnectError != null)
                {
                    DisConnectError(this, null);
                }
            }
        }
        /// <summary>
        /// 内部消息处理函数
        /// cmd#1#0#IP#Port 向客户端请求文件
        /// cmd#1#1#IP#Port#FileName#FileSize 向客户端发送文件
        /// cmd#-2 客户端上传文件失败,要求重传
        /// txt#content 服务端传来的文字消息
        /// </summary>
        private void DispatchMessage()
        {
            string[] messages;
            char[] sparator = new char[] { '#' };

            raw_msg = System.Text.Encoding.Default.GetString(buffer, 0, buffer.Length).Trim('\0');
            Array.Clear(buffer, 0, bufferSize);

            if (ReceivedMsg != null)
            {
                ReceivedMsg(raw_msg, null);
            }

            messages = raw_msg.Split(sparator, StringSplitOptions.RemoveEmptyEntries);
            switch (messages[0])
            {
                case "cmd":
                    switch (messages[1])
                    {
                        case "1":
                            port.remoteIp = IPAddress.Parse(messages[3]);
                            port.remotePort = Int32.Parse(messages[4]);
                            switch (messages[2])
                            {
                                case "0":
                                    if (ReceivedDataRequest != null)
                                    {
                                        ReceivedDataRequest(this, null);
                                    }
                                    SendFileMsg();
                                    port.IsSend = true;
                                    port.Connect();
                                    break;
                                case "1":
                                    if (ReceivedDataSubmit != null)
                                    {
                                        ReceivedDataSubmit(this, null);
                                    }
                                    port.IsSend = false;
                                    //port.filePath += messages[5];
                                    port.fileLength = Int64.Parse(messages[6]);
                                    port.Connect();
                                    break;
                            }
                            break;
                        case "-1":
                            break;
                        case "-2":
                            SendFile();
                            break;
                    }
                    break;
                case "txt":
                    if (ReceivedTxt != null)
                    {
                        ReceivedTxt(messages[1], null);
                    }
                    break;
            }
        }
        /// <summary>
        /// 当客户端需要上传文件时,发送文件大小信息
        /// </summary>
        public void SendFileMsg()
        {
            FileInfo fi=new FileInfo(port.FilePath);
            string tmsg = "cmd#2#"+fi.Length.ToString();
            WriteMsg(tmsg);
        }
        
        /// <summary>
        /// 向服务端请求上传文件
        /// </summary>
        public void SendFile()
        {
            string tmsg = "cmd#0#0";
            WriteMsg(tmsg);
        }
        /// <summary>
        /// 向服务端请求下载文件
        /// </summary>
        public void ReceiveFile()
        {
            string tmsg = "cmd#0#1";
            WriteMsg(tmsg);
        }
        /// <summary>
        /// 通知服务端客户端出错,请求回收数据端口
        /// </summary>
        /// <param name="error">错误消息</param>
        public void SendError(String error)
        {
            string tmsg = "cmd#-1#" + error;
            WriteMsg(tmsg);
        }
        /// <summary>
        /// 想服务端发送文字消息
        /// </summary>
        /// <param name="content"></param>
        public void SendTxt(String content)
        {
            string tmsg = "txt#" + content;
            WriteMsg(tmsg);
        }
        /// <summary>
        /// 发送命令Socket消息
        /// </summary>
        /// <param name="msg"></param>
        private void WriteMsg(String msg)
        {
            byte[] tBuffer = System.Text.Encoding.Default.GetBytes(msg);
            try
            {
                ns.BeginWrite(tBuffer, 0, tBuffer.Length, new AsyncCallback(write_callBack), client);
            }
            catch (Exception e)
            {
                //网络出错处理程序
            }
        }
        /// <summary>
        /// 命令Socket发送信息的回调函数
        /// </summary>
        /// <param name="asy">异步返回结果</param>
        private void write_callBack(IAsyncResult asy)
        {
            try
            {
                ns.EndWrite(asy);
            }
            catch (Exception e)
            {
            }
        }
    }
}
