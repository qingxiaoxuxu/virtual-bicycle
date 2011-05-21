using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using RacingGame;
using VbClient.Net;
using System.Threading;

namespace VbClient
{
    public class InterfacePlugin:INetInterface,IInputInterface
    {
        static InterfacePlugin netInterface=new InterfacePlugin();
        
        public ClientGEvt client;
        public ClientIEvt inputClient;

        string uid="";
        Dictionary<string, BikeState> playersState=new Dictionary<string,BikeState>();
        public static void Load() 
        {
            netInterface.client = new ClientGEvt("");
            netInterface.client.Client.ConnectedServer += new EventHandler(Client_ConnectedServer);
            netInterface.client.BeginGame += new Action(client_BeginGame);
            netInterface.client.GetBikeState += new ClientGEvt.BikeState(client_GetBikeState);
            netInterface.client.RoomDetail += new ClientGEvt.RoomInfo(client_RoomDetail);
            InterfaceFactory.Instance.RegisterNewNetwork(netInterface);

            netInterface.inputClient = new ClientIEvt("127.0.0.1");
            netInterface.inputClient.Enter += new Action(inputClient_Enter);
            netInterface.inputClient.Escape += new Action(inputClient_Escape);
            netInterface.inputClient.HandlebarRotated += new HandlebarRotatedHandler(inputClient_HandlebarRotated);
            netInterface.inputClient.Reset += new Action(inputClient_Reset);
            netInterface.inputClient.ViewChanged += new Action(inputClient_ViewChanged);
            netInterface.inputClient.WheelSpeedChanged += new WheelSpeedChangedHandler(inputClient_WheelSpeedChanged);

            //netInterface.client.Client.ConnectedServer += new EventHandler(Client_ConnectedServer);
            //netInterface.client.BeginGame += new Action(client_BeginGame);
            //netInterface.client.GetBikeState += new ClientGEvt.BikeState(client_GetBikeState);
            //netInterface.client.RoomDetail += new ClientGEvt.RoomInfo(client_RoomDetail);
            //InterfaceFactory.Instance.RegisterNewInput(netInterface);
        }

        
        static bool IsGetRoomInfo = false;
        StartUpParameters startUpPara = new StartUpParameters();
        static void client_RoomDetail(string teamName, string mapName, List<string> userId, List<string> userName, List<string> carId)
        {
            IsGetRoomInfo = false;
            netInterface.startUpPara.MapName = mapName;
            netInterface.startUpPara.TeamName = teamName;
            netInterface.startUpPara.Players = new StartUpParameters.PlayerInfo[userId.Count];
            for (int i = 0; i < userId.Count; i++)
            {
                netInterface.startUpPara.Players[i].ID = userId[i];
                netInterface.startUpPara.Players[i].Name = userName[i];
                netInterface.startUpPara.Players[i].CarID = carId[i];
                netInterface.playersState.Add(userId[i], new BikeState());
            }
            IsGetRoomInfo = true;
        }
        
        static EventWaitHandle myEvent = new System.Threading.EventWaitHandle(true, EventResetMode.ManualReset);

        static void client_GetBikeState(string userId, List<float> states)
        {
            if (myEvent.WaitOne())
            {
                myEvent.Reset();
                BikeState bikeState = new BikeState();
                CoMatrix(ref bikeState.Transform, states);
                bikeState.CompletionProgress = states[16];

                bikeState.Velocity.X = states[17];
                bikeState.Velocity.Y = states[18];
                bikeState.Velocity.Z = states[19];

                bikeState.ID = userId;

                netInterface.playersState[userId] = bikeState;
                myEvent.Set();

            }
        }

        static void client_BeginGame()
        {
            canStartGame = true;
        }
        static bool IsConnected = false;
        static void Client_ConnectedServer(object sender, EventArgs e)
        {
            IsConnected = true;
        }

        #region INetInterface 成员

        public bool Connect(string uid)
        {
            while (!IsConnected) 
                Thread.Sleep(10);
            client.ConnectToServer(uid);
            this.uid = uid;
            return true;
        }

        public StartUpParameters DownloadStartUpParameters()
        {
            client.RequestRoomInfo();
            while (!IsGetRoomInfo)
                Thread.Sleep(10);
            return startUpPara;
        }

        public void SendBikeState(BikeState[] state)
        {
            foreach(BikeState bs in state)
            {
                if(bs.ID==uid)
                {
                    playersState[uid]=bs;
                    break;
                }
            }
            List<float> para;
            para=DeMatrix(playersState[uid].Transform);
            para.Add(playersState[uid].CompletionProgress);
            para.Add(playersState[uid].Velocity.X);
            para.Add(playersState[uid].Velocity.Y);
            para.Add(playersState[uid].Velocity.Z);
            client.SendBikeState(uid, para);
        }

        public static void CoMatrix(ref Matrix ma, List<float> list)
        {
            ma.M11 = list[0];
            ma.M12 = list[1];
            ma.M13 = list[2];
            ma.M14 = list[3];
            ma.M21 = list[4];
            ma.M22 = list[5];
            ma.M23 = list[6];
            ma.M24 = list[7];
            ma.M31 = list[8];
            ma.M32 = list[9];
            ma.M33 = list[10];
            ma.M34 = list[11];
            ma.M41 = list[12];
            ma.M42 = list[13];
            ma.M43 = list[14];
            ma.M44 = list[15];
        }

        public List<float> DeMatrix(Matrix ma)
        {
            List<float> list = new List<float>();
            list.Add(ma.M11);
            list.Add(ma.M12);
            list.Add(ma.M13);
            list.Add(ma.M14);
            list.Add(ma.M21);
            list.Add(ma.M22);
            list.Add(ma.M23);
            list.Add(ma.M24);
            list.Add(ma.M31);
            list.Add(ma.M32);
            list.Add(ma.M33);
            list.Add(ma.M34);
            list.Add(ma.M41);
            list.Add(ma.M42);
            list.Add(ma.M43);
            list.Add(ma.M44);
            return list;

        }

        public BikeState[] DownloadBikeState()
        {
            if (myEvent.WaitOne())
            {
                myEvent.Reset();
                BikeState[] listbs = new BikeState[playersState.Count];
                int i = 0;
                foreach (BikeState bs in playersState.Values)
                {
                    listbs[i++] = bs;
                }
                myEvent.Set();
                if (listbs.Length <= 0) return null;
                return listbs;
            }
            
            return null;
        }

        public void TellReady()
        {
            client.BeginMyGame();
        }
        static bool canStartGame = false;
        public bool CanStartGame()
        {
            return canStartGame;
        }

        public void Disconnect()
        {
            
        }

        #endregion

        #region IInputInterface 成员

        public void Connect()
        {
            
        }

        public void ForceFeedBack(float f)
        {
            inputClient.ForceFeedBack(f);
        }

        public event EventHandler Reset;

        public event EventHandler ViewChanged;

        public event EventHandler Enter;

        public event EventHandler Escape;

        public event HandlebarRotatedHandler HandlebarRotated;

        public event WheelSpeedChangedHandler WheelSpeedChanged;

        public event EventHandler HeartPulse;

        public void Update(GameTime time)
        {
            //throw new NotImplementedException();
        }

        #endregion

        static void inputClient_WheelSpeedChanged(WheelSpeedChangedEventArgs e)
        {
            if(netInterface.WheelSpeedChanged!=null)
                netInterface.WheelSpeedChanged(e);
        }

        static void inputClient_ViewChanged()
        {
            if(netInterface.ViewChanged!=null)
                netInterface.ViewChanged(null, null);
        }

        static void inputClient_Reset()
        {
            if(netInterface.Reset!=null)
                netInterface.Reset(null, null);
        }

        static void inputClient_HandlebarRotated(HandlebarRotatedEventArgs e)
        {
            if(netInterface.HandlebarRotated!=null)
                netInterface.HandlebarRotated(e);
        }

        static void inputClient_Escape()
        {
            if(netInterface.Escape!=null)
                netInterface.Escape(null, null);
        }

        static void inputClient_Enter()
        {
            if(netInterface.Enter!=null)
                netInterface.Enter(null, null);
        }
    }
}
