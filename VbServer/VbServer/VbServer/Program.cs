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
            server.Server.StartServer();
            while (Console.ReadLine() != "exit")
            {
            }
        }
    }
}
