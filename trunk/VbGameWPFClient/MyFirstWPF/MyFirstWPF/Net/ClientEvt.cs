using System;
using System.Collections.Generic;

using System.Text;

namespace VbClient.Net
{
    public class ClientEvt
    {
        public OESClient Client = new OESClient();

        public event EventHandler LoginSuccess;
        public event EventHandler LoginFailure;

        public event EventHandler CreateSuccess;
        public event EventHandler CreateFailure;

        public delegate void UpdateMapHandler(object sender, string map);
        public event UpdateMapHandler SettingMap;

        public delegate void TeamMapList(object sender, List<string> team, List<string> map, List<int> counts);
        public event TeamMapList GotTeamMapList;

        public event UpdateMapHandler AddSuccess;
        public event EventHandler AddFailure;

        public event EventHandler BeginLoadGame;
        public event EventHandler BeginGame;

        public event UpdateMapHandler GetOtherBike;
        public event UpdateMapHandler GetObject;

        public event Action BecomeHost;
        public ClientEvt(string serverIp)
        {
            //Client.server = serverIp;
            Client.ReceivedTxt += new EventHandler(Client_ReceivedTxt);
            Client.InitializeClient();
        }

        public void Login(string name, string id)
        {
            Client.SendTxt("login$" + name + "$" + id);
        }

        public void CreateTeam(string teamName)
        {
            Client.SendTxt("create$" + teamName);
        }

        public void SetMap(string mapName)
        {
            Client.SendTxt("setmap$" + mapName);
        }

        public void GetTeamList()
        {
            Client.SendTxt("getlist");
        }

        public void AddTeam(string teamName)
        {
            Client.SendTxt("add$" + teamName);
        }

        public void LeaveTeam(string teamName)
        {
            Client.SendTxt("leave$" + teamName);
        }

        public void Begin(int i)
        {
            Client.SendTxt("begin$" + i.ToString());
        }

        public void SendBikeState(string msg)
        {
            Client.SendTxt("vb$" + msg);
        }

        public void SendObjectState(string msg)
        {
            Client.SendTxt("obj" + msg);
        }

        public void Logout()
        {
            Client.SendTxt("logout");
        }
        void Client_ReceivedTxt(object sender, EventArgs e)
        {
            string[] msgs = sender.ToString().Split('$');
            switch (msgs[0])
            {
                case "login":
                    {
                        switch (msgs[1])
                        {
                            case "ok":
                                if (LoginSuccess != null)
                                    LoginSuccess(this, null);
                                break;
                            case "error":
                                if (LoginFailure != null)
                                    LoginFailure(this, null);
                                break;
                        }
                        break;
                    }
                case "create":
                    {
                        switch (msgs[1])
                        {
                            case "ok":
                                if (CreateSuccess != null)
                                    CreateSuccess(this, null);
                                break;
                            case "error":
                                if (CreateFailure != null)
                                    CreateFailure(this, null);
                                break;
                        }
                        break;
                    }
                case "setmap":
                    {
                        if (SettingMap != null)
                            SettingMap(this, msgs[1]);
                        break;
                    }
                case "list":
                    {
                        List<string> team = new List<string>();
                        List<string> map = new List<string>();
                        List<int> counts = new List<int>();
                        for (int i = 1; i < msgs.Length; )
                        {
                            team.Add(msgs[i++]);
                            map.Add(msgs[i++]);
                            counts.Add(Int32.Parse(msgs[i++]));
                        }
                        if (GotTeamMapList != null)
                            GotTeamMapList(this, team, map, counts);
                        break;
                    }
                case "add":
                    {
                        switch (msgs[1])
                        {
                            case "ok":
                                if (AddSuccess != null)
                                    AddSuccess(this, msgs[2]);
                                break;
                            case "error":
                                if (AddFailure != null)
                                    AddFailure(this, null);
                                break;
                        }
                        break;
                    }
                case "begin":
                    {
                        switch (msgs[1])
                        {
                            case "1":
                                if (BeginLoadGame != null)
                                    BeginLoadGame(this, null);
                                break;
                            case "3":
                                if (BeginGame != null)
                                    BeginGame(this, null);
                                break;
                        }
                        break;
                    }
                case "host":
                    {
                        if (BecomeHost != null)
                        {
                            BecomeHost();
                        }
                        break;
                    }
                case "vb":
                    {
                        if (GetOtherBike != null)
                            GetOtherBike(this, sender.ToString());
                        break;
                    }
                case "obj":
                    {
                        if (GetObject != null)
                            GetObject(this, sender.ToString());
                        break;
                    }
            }
        }
    }
}
