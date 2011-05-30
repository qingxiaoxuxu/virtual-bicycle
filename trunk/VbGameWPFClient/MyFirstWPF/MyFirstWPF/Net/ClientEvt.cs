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

        public delegate void TeamMapList(object sender, 
            List<string> team, List<string> map, List<int> counts);
        public event TeamMapList GotTeamMapList;

        public event UpdateMapHandler AddSuccess;
        public event EventHandler AddFailure;

        public event EventHandler BeginLoadGame;
        public event EventHandler BeginGame;

        public event EventHandler EndGame;

        public event UpdateMapHandler GetOtherBike;
        public event UpdateMapHandler GetObject;

        public event Action BecomeHost;

        public delegate void RoomInfo(string teamName, string mapName, string host, 
            List<string> userId, List<string> userName, List<string> carId, List<bool> isReady);
        public event RoomInfo RoomDetail;

        public delegate void CarState(string userId, string carId);
        public event CarState ChangeCar;

        public event Action RefreshRoomInfo;

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

        public void CreateTeam(string teamName, string mapName)
        {
            Client.SendTxt("create$" + teamName + "$" + mapName);
        }

        public void SetMap(string mapName)
        {
            Client.SendTxt("setmap$" + mapName);
        }

        public void GetTeamList()
        {
            Client.SendTxt("getlist");
        }

        public void GetRoomInfo()
        {
            Client.SendTxt("getroominfo");
        }

        public void AddTeam(string teamName)
        {
            Client.SendTxt("add$" + teamName);
        }

        public void LeaveTeam(string teamName)
        {
            Client.SendTxt("leave$" + teamName);
        }

        public void Begin()
        {
            Client.SendTxt("begin");
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

        public void Ready()
        {
            Client.SendTxt("ready");
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
                        //switch (msgs[1])
                        //{
                        //    case "1":
                        //        if (BeginLoadGame != null)
                        //            BeginLoadGame(this, null);
                        //        break;
                        //    case "3":
                        //        if (BeginGame != null)
                        //            BeginGame(this, null);
                        //        break;
                        //}
                        //break;
                        if (BeginGame != null)
                            BeginGame(this, null);
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
                case "getroominfo":
                    {
                        List<string> userId = new List<string>();
                        List<string> userName = new List<string>();
                        List<string> carId = new List<string>(); 
                        List<bool> isReady = new List<bool>();
                        int pointer = 5;
                        for (int i = 0; i < Convert.ToInt32(msgs[4]); i++)
                        {
                            userId.Add(msgs[pointer++]);
                            userName.Add(msgs[pointer++]);
                            carId.Add(msgs[pointer++]);
                            isReady.Add(Boolean.Parse(msgs[pointer++]));
                        }
                        RoomDetail(msgs[1], msgs[2], msgs[3], userId, userName, carId, isReady);
                        break;
                    }
                case "setcar":
                    {
                        ChangeCar(msgs[1], msgs[2]);
                        break;
                    }
                case "update":          //更新房间情况
                    {
                        if (RefreshRoomInfo != null)
                            RefreshRoomInfo();
                        break;
                    }
                case "gameend":
                    {
                        EndGame(this, null);
                        break;
                    }
            }
        }
    }
}
