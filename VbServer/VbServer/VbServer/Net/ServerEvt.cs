using System;
using System.Collections.Generic;
 
using System.Text;

namespace VbServer.Net
{
    public class ServerEvt
    {
        public OESServer Server = new OESServer();
        public List<Team> teamList = new List<Team>();
        public ServerEvt()
        {
            Server.ReceivedTxt += new ClientEventHandel(Server_ReceivedTxt);
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
        void Server_ReceivedTxt(Client client, string msg)
        {
            string[] msgs = msg.Split('$');
            switch (msgs[0])
            {
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
                case "getlist":
                    {
                        string genmsg = "list";
                        foreach (Team t in teamList)
                        {
                            genmsg += "$" + t.teamName + "$" + t.mapName;
                        }
                        client.SendTxt(genmsg);
                        break;
                    }
                case "add":
                    {
                        Team t=FindTeamByName(teamList, msgs[1]);
                        if (t != null)
                        {
                            t.AddUser(client);
                            client.SendTxt("add$ok$" + t.mapName);
                        }
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
                case "vb":
                    {
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        foreach (User u in t.userList)
                        {
                            u.client.SendTxt(msg);
                        }
                        break;
                    }
                    
                case "obj":
                    {
                        Team t = FindTeamByUser(teamList, FindUserByClient(User.allLoginUser, client));
                        foreach (User u in t.userList)
                        {
                            u.client.SendTxt(msg);
                        }
                        break;
                    }
            }
        }
    }
}
