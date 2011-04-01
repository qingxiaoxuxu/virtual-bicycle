using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Net.NetworkInformation;
using System.Threading;

namespace VbServer.Net
{
    public class OESServer
    {
        //服务端监听
        private TcpListener listener;
        //服务端Ip
        public IPAddress ip = null;
        //数据端口列表
        private List<DataPort> ports = new List<DataPort>();
        //客户端列表
        private List<Client> clients = new List<Client>();
        //数据端口队列
        private Queue<DataPort> PortQueue = new Queue<DataPort>();
        //等待请求文件的客户端队列
        private Queue<Client> RequestingQueue = new Queue<Client>();
        //等待上传文件的客户端队列
        private Queue<Client> SubmitingQueue = new Queue<Client>();
        //可用端口号列表
        private Queue<int> availablePorts = new Queue<int>();
        //线程同步锁
        private object syncLock = new object();
        //数据端口预定个数
        private int portsRequest = ServerConfig.DataPortNum;      


        #region 运行事件定义
        /// <summary>
        /// 接受客户端连接前
        /// </summary>
        public event EventHandler AcceptingClient;
        /// <summary>
        /// 接受客户端连接后
        /// </summary>
        public event EventHandler AcceptedClient;
        /// <summary>
        /// 接收到消息
        /// </summary>
        public event ClientEventHandel ReceivedMsg;
        /// <summary>
        /// 接收到数据发送消息 服务端--->客户端
        /// </summary>
        public event ClientEventHandel ReceivedDataRequest;
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
        /// 准备发送数据（设置filePath）
        /// </summary>
        public event ClientEventHandel SendDataReady;
        /// <summary>
        /// 准备接收数据（设置filePath）
        /// </summary>
        public event ClientEventHandel ReceiveDataReady;
        /// <summary>
        /// 文件传输开始
        /// 注意：本事件在StartServer()前定义有效
        /// </summary>
        public event DataPortEventHandler FileSendBegin;
        /// <summary>
        /// 文件传输结束
        /// 注意：本事件在StartServer()前定义有效
        /// </summary>
        public event DataPortEventHandler FileSendEnd;
        /// <summary>
        /// 文件接收开始
        /// 注意：本事件在StartServer()前定义有效
        /// </summary>
        public event DataPortEventHandler FileReceiveBegin;
        /// <summary>
        /// 文件接收结束
        /// 注意：本事件在StartServer()前定义有效
        /// </summary>
        public event DataPortEventHandler FileReceiveEnd;
        /// <summary>
        /// 接收客户端数据端口连接请求
        /// 注意：本事件在StartServer()前定义有效
        /// </summary>
        public event DataPortEventHandler AcceptedDataPort;

        #endregion
        #region 出错事件定义
        
        /// <summary>
        /// 数据端口出错
        /// </summary>
        /// <param name="msg"></param>
        public delegate void DataPortError(string msg);
        public event DataPortError OnDataPortError;

        #endregion

        /// <summary>
        /// 服务端构造函数,获取本机Ip
        /// </summary>
        public OESServer()
        {
            //RetrieveHostIpv4Address();
        }
        /// <summary>
        /// 开启监听(开启服务端)
        /// </summary>
        public void StartServer()
        {
            if(ip==null)
                ip = ServerConfig.HostIp;
            if (ip != null)
            {
                listener = new TcpListener(ip, ServerConfig.HostPort);     //固定服务端监听端口           
                listener.Start();
                listener.BeginAcceptTcpClient(new AsyncCallback(accept_callBack), listener);
                InitializeDataPorts();
            }
            //else 通知出错处理程序
        }
        /// <summary>
        /// 获取本机IP,有时会出错,尤其是有多个Ip
        /// </summary>
        private void RetrieveHostIpv4Address()
        {
            //获得所有的ip地址，包括ipv6和ipv4
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress tip in ips)
            {
                //ipv4的最大长度为15，ipv6为39
                if (tip.ToString().Length <= 15)
                {
                    ip = tip;
                    return;
                }
            }
        }

        /// <summary>
        /// 初始化数据端口
        /// </summary>
        private void InitializeDataPorts()
        {
            SearchSparePort();
            for (int i = availablePorts.Count - 1; i >= 0; i--)
            {
                DataPort dport = new DataPort(ip, availablePorts.Dequeue());
                dport.portRecycle += PortRecycler;  //注册回收端口事件
                if (this.AcceptedDataPort != null)
                    dport.AcceptedDataPort += this.AcceptedDataPort;
                if (this.FileReceiveBegin != null)
                    dport.FileReceiveBegin += this.FileReceiveBegin;
                if (this.FileReceiveEnd != null)
                    dport.FileReceiveEnd += this.FileReceiveEnd;
                if (this.FileSendBegin != null)
                    dport.FileSendBegin += this.FileSendBegin;
                if (this.FileSendEnd != null)
                    dport.FileSendEnd += this.FileSendEnd;
                
                ports.Add(dport);
                PortQueue.Enqueue(dport);
            }
        }

        /// <summary>
        /// 搜索当前空闲端口号
        /// </summary>
        private void SearchSparePort()
        {
            int start_port = 10005;
            IPGlobalProperties ipgloabalprops = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpListInfos = ipgloabalprops.GetActiveTcpListeners();
            TcpConnectionInformation[] tcpConnInfos = ipgloabalprops.GetActiveTcpConnections();
            IPEndPoint[] udpInfos = ipgloabalprops.GetActiveUdpListeners();

            List<int> portNumbers = new List<int>();
            foreach (IPEndPoint tcpl in tcpListInfos)
            {
                portNumbers.Add(tcpl.Port);
            }
            foreach (TcpConnectionInformation tcpi in tcpConnInfos)
            {
                portNumbers.Add(tcpi.LocalEndPoint.Port);
            }
            foreach (IPEndPoint udpl in udpInfos)
            {
                portNumbers.Add(udpl.Port);
            }

            portNumbers.Sort();
            int port = start_port;
            for (int i = 0; i < portNumbers.Count; i++)
            {
                if (portNumbers[i] <= port)
                    continue;

                port++;
                while (port != portNumbers[i])
                {
                    availablePorts.Enqueue(port);
                    port++;
                    if (--portsRequest == 0)
                        return;
                }
            }
            if (portsRequest != 0)
            {
                if(OnDataPortError!=null)
                    OnDataPortError("可用端口数量不足！");
            }
        }

        /// <summary>
        /// 回收数据端口
        /// </summary>
        /// <param name="port"></param>
        private void PortRecycler(DataPort port)
        {
            PortQueue.Enqueue(port);
            Client tclient;
            while ((RequestingQueue.Count != 0) && (PortQueue.Count != 0))
            {
                tclient = RequestingQueue.Dequeue();
                tclient.Port = PortQueue.Dequeue();
                tclient.Port.IsSend = true;
                tclient.sendData();
            }
            while ((SubmitingQueue.Count != 0) && (PortQueue.Count != 0))
            {
                tclient = SubmitingQueue.Dequeue();
                tclient.Port = PortQueue.Dequeue();
                tclient.Port.IsSend = false;
                tclient.fetchData();
            }
        }

        /// <summary>
        /// Accept回调函数
        /// </summary>
        /// <param name="asy"></param>
        public void accept_callBack(IAsyncResult asy)
        {
            if (AcceptingClient != null)
            {
                AcceptingClient(this, null);
            }
            TcpListener tlistener = (TcpListener)asy.AsyncState;
            TcpClient tclient = tlistener.EndAcceptTcpClient(asy);

            Client client = new Client(tclient);
            client.MessageScheduler += MessageScheduler;
            if(this.ReceivedDataRequest!=null)
                client.ReceivedDataRequest += this.ReceivedDataRequest;
            if (this.ReceivedDataSubmit != null)
                client.ReceivedDataSubmit += this.ReceivedDataSubmit;
            if (this.ReceivedMsg != null)
                client.ReceivedMsg += this.ReceivedMsg;
            if (this.ReceivedTxt != null)
                client.ReceivedTxt += this.ReceivedTxt;
            if (this.WrittenMsg != null)
                client.WrittenMsg += this.WrittenMsg;
            if (this.SendDataReady != null)
                client.SendDataReady += this.SendDataReady;
            if (this.ReceiveDataReady != null)
                client.ReceiveDataReady += this.ReceiveDataReady;
            clients.Add(client);

            listener.BeginAcceptTcpClient(new AsyncCallback(accept_callBack), listener);
            if (AcceptedClient != null)
            {
                AcceptedClient(client, null);
            }
        }
        /// <summary>
        /// 服务端消息处理函数
        /// </summary>
        /// <param name="client">活动的客户端</param>
        /// <param name="type">消息类型</param>
        private void MessageScheduler(Client client,int type)
        {
            lock(syncLock)
            {
                switch (type)
                {
                    
                    case 0:
                    #region 搜索空闲数据端口，准备传送数据
                        if (PortQueue.Count != 0)
                        {
                            client.Port = PortQueue.Dequeue();
                            client.Port.IsSend = false;
                            client.fetchData();
                        }
                        else
                        {
                            RequestingQueue.Enqueue(client);
                        }
                        break;
                    #endregion

                    case 1:
                    #region 搜索空闲数据端口，准备接受数据
                        if (PortQueue.Count != 0)
                        {
                            client.Port = PortQueue.Dequeue();
                            client.Port.IsSend = true;
                            client.sendData();
                        }
                        else
                        {
                            SubmitingQueue.Enqueue(client);
                        }
                        break;
                    #endregion
                    case -1:
                        PortRecycler(client.Port);
                        break;
                    default:
                        //网络出错程序
                        break;
                }
            }
        }
        /// <summary>
        /// 请求文件的客户端队列清空
        /// </summary>
        public void ClearRequestingQueue()
        {
            RequestingQueue.Clear();
        }
        /// <summary>
        /// 上传文件的客户端队列清空
        /// </summary>
        public void ClearSubmitingQueue()
        {
            SubmitingQueue.Clear();
        }

    }
}
