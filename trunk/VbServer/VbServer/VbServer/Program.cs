using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VbServer.Net;

namespace VbServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            ServerEvt server = new ServerEvt();
            server.Server.ip=System.Net.IPAddress.Parse("222.20.59.63");
            server.Server.StartServer();
            server.Server.ReceivedMsg += new ClientEventHandel(Server_ReceivedMsg);
            while (Console.ReadLine() != "exit")
            {
            }
        }

        static void Server_ReceivedMsg(Client client, string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
