using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace VbClient.Net
{
    public class ClientConfig
    {
        const string FileTemplate = 
@"RemoteIp=127.0.0.1
RemotePort=20000";
        const string ConfName = "ClientConfig.ini";
        private Dictionary<string, string> dirc = new Dictionary<string, string>();
        private static ClientConfig instence = null;
        private ClientConfig()
        {
            if (!File.Exists(ConfName))
            {
                using (StreamWriter sr = new StreamWriter(ConfName))
                {
                    dirc.Clear();
                    sr.Write(FileTemplate);
                }
            }
            using (StreamReader sr = new StreamReader(ConfName))
            {
                dirc.Clear();
                string[] eachLine = sr.ReadToEnd().Split('\n');
                foreach (string line in eachLine)
                {
                    string[] sep = line.Split('=');
                    dirc.Add(sep[0].Trim(), sep[1].Trim());
                }
            }
        }
        private static ClientConfig GetInstence()
        {
            if(instence==null)
            {
                return new ClientConfig();
            }
            else
            {
                return instence;
            }
        }
        public static IPAddress RemoteIp
        {
            get
            {
                return IPAddress.Parse(GetInstence().dirc["RemoteIp"]);
            }
        }
        public static int RemotePort
        {
            get
            {
                return Convert.ToInt32(GetInstence().dirc["RemotePort"]);
            }
        }
    }
}
