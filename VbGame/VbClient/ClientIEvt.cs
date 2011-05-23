using System;
using System.Collections.Generic;

using System.Text;
using RacingGame;

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
    }
}
