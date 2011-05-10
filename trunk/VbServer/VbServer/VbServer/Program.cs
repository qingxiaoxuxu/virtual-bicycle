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
            string cmd="";
            //server.Server.ip=System.Net.IPAddress.Parse("222.20.59.63");
            server.Server.StartServer();
            server.Server.ReceivedMsg += new ClientEventHandel(Server_ReceivedMsg);
            while ( cmd!= "exit")
            {
                cmd = Console.ReadLine();
                //Console.WriteLine(Environment.CurrentDirectory);
                if (cmd == "team")
                {
                    foreach(Team t in server.teamList)
                    {
                        Console.WriteLine(t.teamName + " " + t.mapName);
                        foreach (User u in t.userList)
                        {
                            Console.WriteLine("\t" + u.userName);
                        }
                    }
                }
                else if (cmd == "user")
                {
                    foreach (User u in User.allLoginUser)
                    {
                        Console.WriteLine(u.userName);
                    }
                }
            }
        }

        static void Server_ReceivedMsg(Client client, string msg)
        {
            Console.WriteLine("Receiving("+client.ClientIp+"):"+msg);
        }
    }
}
