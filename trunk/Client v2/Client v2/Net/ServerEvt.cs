using System;
using System.Collections.Generic;
using System.Text;

namespace VbServer.Net
{
    public class ServerEvt
    {
        public const int MAX_USER = 4;
        public OESServer Server = new OESServer();
        public ServerEvt()
        {
            Server.ReceivedTxt += new ClientEventHandel(Server_ReceivedTxt);
            Server.WrittenMsg += new ClientEventHandel(Server_WrittenMsg);
            Server.AcceptedClient += new EventHandler(Server_AcceptedClient);
        }
        public delegate void ForceBackHandler(float f);
        public event ForceBackHandler ForceBack;
        public void Reset()
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("reset");
        }
        public void ViewChanged()
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("viewchange");
        }
        public void Enter()
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("enter");
        }
        public void Escape()
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("escape");
        }
        public void HandlebarRotated(float angle)
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("rotate$" + angle.ToString());
        }
        public void WheelSpeedChanged(float speed, float change)
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("speed$" + speed.ToString() + "$" + change.ToString());
        }
        public void WheelSpeedChangedRaw(float speed)
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("speedraw$" + speed.ToString() );
        }
        public void HeartPulse(int pulse)
        {
            for (int i = 0; i < Server.Clients.Count; i++)
                Server.Clients[i].SendTxt("heart$" + pulse.ToString());
        }

        void Server_WrittenMsg(Client client, string msg)
        {
            //Console.WriteLine("Sending:" + msg);
        }

        void Server_AcceptedClient(object sender, EventArgs e)
        {
            (sender as Client).DisConnect += new EventHandler(ServerEvt_DisConnect);
        }

        void ServerEvt_DisConnect(object sender, EventArgs e)
        {
        
            

        }
        
        void Server_ReceivedTxt(Client client, string msg)
        {
            string[] msgs = msg.Split('$');
            switch (msgs[0])
            {
                case "forceback":
                    {
                        ForceBack(Single.Parse(msgs[1]));
                        break;
                    }
            }
        }

        void client_DisConnect(object sender, EventArgs e)
        {
            
        }
    }
}