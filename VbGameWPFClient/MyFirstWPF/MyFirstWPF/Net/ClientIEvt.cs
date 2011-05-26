using System;
using System.Collections.Generic;

using System.Text;

namespace VbClient.Net
{
    public class ClientIEvt
    {
        public OESClient Client = new OESClient();

        public event Action Reset;
        public event Action ViewChanged;
        public event Action Enter;
        public event Action Escape;
        public event HandlebarRotatedHandler HandlebarRotated;
        public event WheelSpeedChangedHandler WheelSpeedChanged;
        public event HeartPulseChangedHandler HeartPulse;

        public ClientIEvt(string serverIp)
        {
            Client.server = "127.0.0.1";
            Client.portNum = 30000;
            Client.ReceivedTxt += new EventHandler(Client_ReceivedTxt);
            Client.InitializeClient();
        }

        public void ForceFeedBack(float f)
        {
            Client.SendTxt("forceback$" + f.ToString());
        }

        void Client_ReceivedTxt(object sender, EventArgs e)
        {
            string[] msgs = sender.ToString().Split('$');
            switch (msgs[0])
            {
                case "reset":
                    {
                        Reset();
                        break;
                    }
                case "viewchange":
                    {
                        ViewChanged();
                        break;
                    }
                case "enter":
                    {
                        Enter();
                        break;
                    }
                case "escape":
                    {
                        Escape();
                        break;
                    }
                case "rotate":
                    {
                        HandlebarRotatedEventArgs arg = new HandlebarRotatedEventArgs(Single.Parse(msgs[1]));
                        HandlebarRotated(arg);
                        break;
                    }
                case "speed":
                    {
                        WheelSpeedChangedEventArgs arg = new WheelSpeedChangedEventArgs(Single.Parse(msgs[1]), Single.Parse(msgs[2]));
                        WheelSpeedChanged(arg);
                        break;
                    }
                case "heart":
                    {
                        HeartPulseChangedEventArgs arg = new HeartPulseChangedEventArgs(Int32.Parse(msgs[1]));
                        HeartPulse(arg);
                        break;
                    }

            }
        }
        public class HeartPulseChangedEventArgs : EventArgs
        {

            public HeartPulseChangedEventArgs(int value)
            {
                this.Value = value;
            }
            public float Value
            {
                get;
                private set;
            }

        }
        /// <summary>
        ///  定义车把转向发生变化时的事件的参数
        /// </summary>
        public class HandlebarRotatedEventArgs : EventArgs
        {
            /// <summary>
            ///  
            /// </summary>
            /// <param name="angle">新的车把转向程度，0为中心，
            /// 根据手感调整输入参数的大小，不是角度</param>
            public HandlebarRotatedEventArgs(float angle)
            {
                this.Angle = angle;
            }
            public float Angle
            {
                get;
                private set;
            }

        }
        /// <summary>
        ///  定义轮子速度发生变化时的事件的参数
        /// </summary>
        public class WheelSpeedChangedEventArgs : EventArgs
        {
            /// <summary>
            ///  
            /// </summary>
            /// <param name="speed">当前速度[暂时未使用]</param>
            /// <param name="change">速度变化量</param>
            public WheelSpeedChangedEventArgs(float speed, float change)
            {
                this.Speed = speed;
                this.SpeedChange = change;
            }

            public float Speed
            {
                get;
                set;
            }
            public float SpeedChange
            {
                get;
                set;
            }

        }
        public delegate void HandlebarRotatedHandler(HandlebarRotatedEventArgs e);
        public delegate void WheelSpeedChangedHandler(WheelSpeedChangedEventArgs e);
        public delegate void HeartPulseChangedHandler(HeartPulseChangedEventArgs e);
    }
}
