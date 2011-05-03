using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VbServer.Net;

namespace VbServer
{
    public class User
    {
        //用户名
        public string userName = "";
        public string userId = "";
        //用户对应Socket
        public Client client;
        //是否是房主
        public bool isAdmin;
        //用户的自行车型号
        public string carId="0";
        //用户构造函数
        public User(string name ,string id, Client c)
        {
            userName = name;
            userId = id;
            client = c;
        }
        public static List<User> allLoginUser = new List<User>();
        
    }
}
