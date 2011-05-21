using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VbServer.Net;

namespace VbServer
{
    public class Team
    {
        //房间名
        public string teamName = "";
        //地图名称
        public string mapName = "";
        //房间用户
        public List<User> userList = new List<User>();

        //就绪人数
        public int readyCount = 0;

        //进入游戏后的玩家用户
        public List<User> playerList = new List<User>();

        //游戏载入正确用户数
        public int loadingRightCount = 0;

        //添加一个用户
        public void AddUser(Client c)
        {
            User user=null;
            foreach (User u in User.allLoginUser)
            {
                if (u.client == c)
                {
                    user = u;
                    break;
                }
            }
            if (user != null)
            {
                AddUser(user);
            }
        }
        public void AddUser(User u)
        {
            userList.Add(u);
        }
        //删除一个用户
        public void DelUser(Client c)
        {
            User user = null;
            foreach (User u in User.allLoginUser)
            {
                if (u.client == c)
                {
                    user = u;
                    break;
                }
            }
            if (user != null)
            {
                DelUser(user);
            }
        }
        public void DelUser(User u)
        {
            userList.Remove(u);
            playerList.Remove(u);
        }

        public Team(string name)
        {
            teamName = name;
        }
        
    }
}
