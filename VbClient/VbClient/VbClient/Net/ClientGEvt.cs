using System;
using System.Collections.Generic;

using System.Text;

namespace VbClient.Net
{
    public class ClientGEvt
    {
        public OESClient Client = new OESClient();

        /// <summary>
        /// 获取当前房间所有信息
        /// </summary>
        /// <param name="teamName">房间名称</param>
        /// <param name="mapName">地图名称</param>
        /// <param name="userId">玩家ID列表</param>
        /// <param name="userName">玩家名称列表</param>
        /// <param name="carId">玩家车型列表</param>
        public delegate void RoomInfo(string teamName, string mapName, List<string> userId, List<string> userName, List<string> carId);
        public event RoomInfo RoomDetail;
        
        /// <summary>
        /// 开始游戏
        /// </summary>
        public event Action BeginGame;


        public ClientGEvt(string serverIp)
        {
            //Client.server = serverIp;
            Client.ReceivedTxt += new EventHandler(Client_ReceivedTxt);
            Client.InitializeClient();
        }

        public void ConnectToServer(string userId)
        {
            Client.SendTxt("connect$" + userId);
        }

        public void BeginMyGame()
        {
            Client.SendTxt("begingame");
        }

        public void RequestRoomInfo()
        {
            Client.SendTxt("info");
        }

        public void SendBikeState(string msg)
        {
            Client.SendTxt(msg);
        }

        void Client_ReceivedTxt(object sender, EventArgs e)
        {
            string[] msgs = sender.ToString().Split('$');
            switch (msgs[0])
            {
                case "begingame":
                    {
                        BeginGame();
                        break;
                    }
                case "vb":
                    {
                        
                        break;
                    }
                case "obj":
                    {
                        
                        break;
                    }
                case "info":
                    {
                        List<string> userId = new List<string>();
                        List<string> userName = new List<string>();
                        List<string> carId = new List<string>();
                        int pointer = 4;
                        for (int i = 0; i < Convert.ToInt32(msgs[3]); i++)
                        {
                            userId.Add(msgs[pointer++]);
                            userName.Add(msgs[pointer++]);
                            carId.Add(msgs[pointer++]);
                        }
                        RoomDetail(msgs[1], msgs[2], userId, userName, carId);
                        break;
                    }
                
            }
        }
    }
}
