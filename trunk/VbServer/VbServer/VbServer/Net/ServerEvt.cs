#define DEBUG
using System;
using System.Collections.Generic;
 
using System.Text;

namespace VbServer.Net
{
    public class ServerEvt
    {
        public const int MAX_USER = 4;
        public OESServer Server = new OESServer();
        public List<Team> teamList = new List<Team>();
        public ServerEvt()
        {
            Server.ReceivedTxt += new ClientEventHandel(Server_ReceivedTxt);
            Server.WrittenMsg += new ClientEventHandel(Server_WrittenMsg);
            Server.AcceptedClient += new EventHandler(Server_AcceptedClient);
#if DEBUG
            Team t=new Team("Team1");
            t.mapName = "Beginner";
            teamList.Add(t);
            t.userList.Add(new User("lkq","1",null));
            t.userList.Add(new User("pl","2",null));
            //t.userList.Add(new User("xt","3",null));
            t.userList[0].isAdmin = true;       //
            User.allLoginUser.AddRange(t.userList);
#endif
        }

        void Server_WrittenMsg(Client client, string msg)
        {
            Console.WriteLine("Sending:" + msg);
        }

        void Server_AcceptedClient(object sender, EventArgs e)
        {
            (sender as Client).DisConnect += new EventHandler(ServerEvt_DisConnect);
        }

        void ServerEvt_DisConnect(object sender, EventArgs e)
        {
        
            for (int i = teamList.Count - 1; i >= 0; i--)
            {
                teamList[i].DelUser(sender as Client);
                if (teamList[i].userList.Count == 0)
                {
                    teamList.RemoveAt(i);
                }
                else
                {
                    if(teamList[i].userList[0].client!=null)
                        teamList[i].userList[0].client.SendTxt("host");
                }

            }
     
            User u = FindUserByClient(User.allLoginUser, sender as Client);
            User.allLoginUser.Remove(u);

        }
        //通过房间名找房间
        public static Team FindTeamByName(List<Team> list, string name)
        {
            foreach (Team t in list)
            {
                if (t.teamName == name)
                {
                    return t;
                }
            }
            return null;
        }
        //通过用户找房间
        public static Team FindTeamByUser(List<Team> list, User u)
        {
            foreach (Team t in list)
            {
                if (t.userList.Contains(u))
                {
                    return t;
                }
                if (t.playerList.Contains(u))
                {
                    return t;
                }
            }

            return null;
        }
         //通过Client寻找用户
        public static User FindUserByClient(List<User> list, Client c)
        {
            foreach (User u in list)
            {
                if (u.client == c)
                {
                    return u;
                }
            }
            return null;
        }
        //通过Id寻找用户
        public static User FindUserById(List<User> list, string id)
        {
            foreach (User u in list)
            {
                if (u.userId == id)
                {
                    return u;
                }
            }
            return null;
        }
        void Server_ReceivedTxt(Client client, string msg)
        {
            string[] msgs = msg.Split('$');
            switch (msgs[0])
            {
                #region 登录界面消息
                case "login":
                    {
                        User u = new User(msgs[1], msgs[2], client);
                        User.allLoginUser.Add(u);
                        u.client.SendTxt("login$ok");
                        break;
                    }
                
                case "create":
                    {
                        Team t = new Team(msgs[1]);
                        teamList.Add(t);
                        t.AddUser(client);
                        t.userList[0].isAdmin = true;
                        t.mapName = msgs[2];                //添加地图信息
                        client.SendTxt("create$ok");
                        break;
                    }
                case "setmap":
                    {
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        t.mapName = msgs[1];
                        foreach (User u in t.userList)
                        {
                            u.client.SendTxt("setmap$" + t.mapName);
                        }
                        break;
                    }
                case "setcar":
                    {
                        User temp=FindUserByClient(User.allLoginUser, client);
                        Team t = FindTeamByUser(teamList, temp);
                        temp.carId = msgs[1];
                        foreach (User u in t.userList)
                        {
                            if(u.client!=client)
                               u.client.SendTxt("setcar$"+temp.userId+"$" + temp.carId);
                        }
                        break;
                    }
                case "getroominfo":
                    {
                        string allInfo = "getroominfo";             //漏了标签
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        allInfo += "$" + t.teamName + "$" + t.mapName + "$";
                        foreach (User u in t.userList)
                            if (u.isAdmin)
                            {
                                allInfo += u.userName;            //
                                break;
                            }
                        allInfo += "$" + t.userList.Count.ToString();
                        foreach (User u in t.userList)
                        {
                            allInfo += "$" + u.userId + "$" + u.userName+"$"+u.carId+"$"+u.isReady.ToString();
                        }
                        client.SendTxt(allInfo);
                        break;
                        
                    }
                case "getlist":
                    {
                        string genmsg = "list";
                        #region test
                        teamList.Clear();
                        for (int i = 0; i < 6; i++)
                        {
                            Team t = new Team("lkq" + i.ToString());
                            t.mapName = "forest" + i.ToString();
                            teamList.Add(t);
                        }
                        Team p = new Team("Hello World!");
                        p.mapName = "看到我就对了~";
                        teamList.Add(p);
                        #endregion
                        foreach (Team t in teamList)
                        {
                            genmsg += "$" + t.teamName + "$" + t.mapName+ "$" + t.userList.Count.ToString();
                        }
                        client.SendTxt(genmsg);
                        break;
                    }
                case "add":
                    {
                        Team t=FindTeamByName(teamList, msgs[1]);
                        if (t != null && t.userList.Count < MAX_USER)
                        {
                            t.AddUser(client);
                            client.SendTxt("add$ok$" + t.mapName);
                            for (int i = 0; i < t.userList.Count - 1; i++)
                                if (t.userList[i].client != null)
                                    t.userList[i].client.SendTxt("update");         //
                        }
                        else
                            client.SendTxt("add$error");
                        break;
                    }
                case "leave":
                    {
                        Team t=FindTeamByName(teamList, msgs[1]);
                        t.DelUser(client);
                        if (t.userList.Count == 0)
                        {
                            teamList.Remove(t);
                        }
                        else
                        {
                            t.userList[0].isAdmin = true;           //
                            //if (t.userList[0].client != null)
                            //    t.userList[0].client.SendTxt("host");
                            foreach (User u in t.userList)
                            {
                                if (u.client != null)
                                    u.client.SendTxt("update");
                            }
                        }
                        break;
                    }
                case "ready":
                    {
                        FindUserByClient(User.allLoginUser,client).isReady = !FindUserByClient(User.allLoginUser,client).isReady;
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        foreach (User u in t.userList)
                        {
                            u.client.SendTxt("update");
                        }
                        break;
                    }
                case "begin":
                    {
                        switch (msgs[1])
                        {
                            case "0":
                                {
                                    Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                                    foreach (User u in t.userList)
                                    {
                                        u.client.SendTxt("begin$1");
                                    }
                                    break;
                                }
                            case "2":
                                {
                                    Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                                    t.readyCount++;
                                    if (t.readyCount == t.userList.Count)
                                    {
                                        foreach (User u in t.userList)
                                        {
                                            u.client.SendTxt("begin$3");
                                        }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case "logout":
                    {
                        User u = FindUserByClient(User.allLoginUser,client);
                        User.allLoginUser.Remove(u);
                        break;
                    }
                #endregion 登录界面消息

                #region 游戏中消息
                case "connect":
                    {
                        User temp = FindUserById(User.allLoginUser, msgs[1]);
                        User newTemp = new User(temp.userName, temp.userId, client);
                        FindTeamByUser(teamList, temp).playerList.Add(newTemp);
                        User.allLoginUser.Add(newTemp);
                        client.DisConnect += new EventHandler(client_DisConnect);
                        break;
                    }
                case "info":
                    {
                        string allInfo = "info$";
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        allInfo+=t.teamName+"$"+t.mapName+"$"+t.userList.Count.ToString();
                        foreach(User u in t.userList)
                        {
                            allInfo += "$" + u.userId + "$" + u.userName+"$"+u.carId;
                        }
                        client.SendTxt(allInfo);
                        break;
                    }
                case "begingame":
                    {
                        
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        t.loadingRightCount++;
                        if (t.loadingRightCount == t.playerList.Count && t.playerList.Count == t.userList.Count)
                        {
                            foreach (User u in t.playerList)
                            {
                                u.client.SendTxt("begingame");

                            }
                            t.loadingRightCount = 0;
                        }
                        break;
                        
                    }
                case "vb":
                    {
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        User current = FindUserByClient(User.allLoginUser, client);
                        msg=msg.Replace("vb$", "");
                        foreach (User u in t.playerList)
                        {
                            if(u.client!=client)
                                u.client.SendTxt("vb$"+msg);
                        }
                        break;
                    }
                    
                case "obj":
                    {
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        User current = FindUserByClient(User.allLoginUser, client);
                        msg = msg.Replace("obj$", "");
                        foreach (User u in t.playerList)
                        {
                            if (u.client != client)
                                u.client.SendTxt("obj$"+current.userId+"$"+msg);
                        }
                        break;
                    }
                #endregion 游戏中消息
            }
        }

        void client_DisConnect(object sender, EventArgs e)
        {
            User u = FindUserByClient(User.allLoginUser, sender as Client);
            if (u != null)
            {
                FindTeamByUser(teamList, u).playerList.Remove(u);
                User.allLoginUser.Remove(u);
            }
        }
    }
}