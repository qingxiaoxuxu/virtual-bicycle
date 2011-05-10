using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO;

namespace VbServer.Net
{
    public class Client
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
        //得到的消息
        public string msg;
        //消息末尾字符
        public char EndOfMsg = '`';

        #region 事件定义
        /// <summary>
        /// 返回到Server处理的信息
        /// </summary>
        /// <param name="client">活动的Client</param>
        /// <param name="type">消息类型</param>
        public delegate void MsgFun(Client client, int type);
        public MsgFun MessageScheduler;
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event ClientEventHandel ReceivedMsg;
        /// <summary>
        /// 接收到数据发送消息 服务端--->客户端
        /// </summary>
        public event ClientEventHandel ReceivedDataRequest;
        /// <summary>
        /// 准备发送数据（设置filePath）
        /// </summary>
        public event ClientEventHandel SendDataReady;
        /// <summary>
        /// 准备接收数据（设置filePath）
        /// </summary>
        public event ClientEventHandel ReceiveDataReady;
        /// <summary>
        /// 接收到数据请求消息 客户端--->服务端
        /// </summary>
        public event ClientEventHandel ReceivedDataSubmit;
        /// <summary>
        /// 接收到文字消息.第一个参数为String类型,表示收到的消息
        /// </summary>
        public event ClientEventHandel ReceivedTxt;
        /// <summary>
        /// 消息发送完成.第一个参数为String类型,表示发送出去的消息内容
        /// </summary>
        public event ClientEventHandel WrittenMsg;
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        public event EventHandler DisConnect;
        #endregion
        /// <summary>
        /// 客户端数据端口
        /// </summary>
        public DataPort port;

        public DataPort Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
                port.InitDataPort(this.client.Client.RemoteEndPoint);
                port.FileSizeError += new ErrorEventHandler(port_FileSizeError);
            }
        }

        public string ClientIp
        {
            get
            {
                return client.Client.RemoteEndPoint.ToString();
            }
        }

        /// <summary>
        /// Client构造函数
        /// </summary>
        /// <param name="client">以连接好的Socket</param>
        public Client(TcpClient client)
        {
            this.client = client;
            ns = client.GetStream();
            ns.BeginRead(buffer, 0, bufferSize, new AsyncCallback(receive_callBack), client);
        }

        /// <summary>
        /// 文件大小出错,重传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void port_FileSizeError(object sender, ErrorEventArgs e)
        {
            SendResend();
        }

        /// <summary>
        /// Receive回调函数
        /// </summary>
        /// <param name="asy"></param>
        private void receive_callBack(IAsyncResult asy)
        {
            try
            {
                TcpClient tclient = (TcpClient)asy.AsyncState;
                int result = ns.EndRead(asy);
                DispatchMessage();
                ns.BeginRead(buffer, 0, bufferSize, new AsyncCallback(receive_callBack), client);
            }
            catch
            {
                if (DisConnect != null)
                    DisConnect(this, null);
            }
        }

        /// <summary>
        /// 内部消息处理函数
        /// </summary>
        private void DispatchMessage()
        {
            string raw_msgs = System.Text.Encoding.Default.GetString(buffer, 0, buffer.Length).Trim('\0');
            foreach (string onemsg in raw_msgs.Split(EndOfMsg))
            {
                if (!String.IsNullOrEmpty(onemsg))
                {
                    string[] messages;
                    char[] sparator = new char[] { '#' };

                    raw_msg = onemsg;
                    Array.Clear(buffer, 0, bufferSize);

                    if (ReceivedMsg != null)
                    {
                        ReceivedMsg(this, raw_msg);
                    }

                    messages = raw_msg.Split(sparator, StringSplitOptions.RemoveEmptyEntries);

                    switch (messages[0])
                    {
                        case "cmd":                             //命令
                            switch (messages[1])
                            {
                                case "0":                       //申请数据端口
                                    switch (messages[2])
                                    {
                                        case "0":               //上传文件
                                            if (ReceivedDataSubmit != null)
                                            {
                                                ReceivedDataSubmit(this, raw_msg);
                                            }
                                            MessageScheduler(this, 0);
                                            break;
                                        case "1":               //下载文件
                                            if (ReceivedDataRequest != null)
                                            {
                                                ReceivedDataRequest(this, raw_msg);
                                            }
                                            MessageScheduler(this, 1);
                                            break;
                                    }
                                    break;
                                case "2":
                                    port.fileLength = Int64.Parse(messages[2]);
                                    break;
                                case "-1":
                                    MessageScheduler(this, -1);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "txt":
                            if (ReceivedTxt != null)
                            {
                                ReceivedTxt(this, messages[1]);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 发出发送文件消息
        /// </summary>
        public void sendData()
        {
            if (SendDataReady != null)
                SendDataReady(this, null);
            WriteMsg(SendFileMsg(port.FilePath));
        }

        /// <summary>
        /// 发出接收文件消息
        /// </summary>
        public void fetchData()
        {
            if (ReceiveDataReady != null)
                ReceiveDataReady(this, null);
            WriteMsg(RecieveFileMsg());
        }

        /// <summary>
        /// Write回调函数
        /// </summary>
        /// <param name="asy"></param>
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

        /// <summary>
        /// 传送文件消息
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string SendFileMsg(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            return "cmd#1#1#" + port.ip.ToString() + "#" + port.localPort.ToString() + "#" + fi.Name.ToString() + "#" + fi.Length.ToString();
        }

        /// <summary>
        /// 接收文件消息
        /// </summary>
        /// <returns></returns>
        private string RecieveFileMsg()
        {
            return "cmd#1#0#" + port.ip.ToString() + "#" + port.localPort.ToString();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        private void WriteMsg(String msg)
        {
            byte[] tBuffer = System.Text.Encoding.Default.GetBytes(msg + EndOfMsg);
            try
            {
                ns.BeginWrite(tBuffer, 0, tBuffer.Length, new AsyncCallback(write_callBack), client);
                if (WrittenMsg != null)
                {
                    WrittenMsg(this, msg);
                }
            }
            catch (Exception e)
            {
                //网络出错处理程序
            }
        }

        /// <summary>
        /// 文字消息
        /// </summary>
        /// <param name="content"></param>
        public void SendTxt(String content)
        {
            string tmsg = "txt#" + content;
            WriteMsg(tmsg);
        }

        /// <summary>
        /// 重传消息
        /// </summary>
        public void SendResend()
        {
            string tmsg = "cmd#-2";
            WriteMsg(tmsg);
        }
    }
    public delegate void ClientEventHandel(Client client, String msg);
}
