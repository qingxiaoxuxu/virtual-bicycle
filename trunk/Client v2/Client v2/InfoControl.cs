using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuzzWin;

namespace Client_v2
{
    public class InfoControl
    {
        public static DeviceDataManager device = new DeviceDataManager();

        private static MainWindow mw;

        public static MainWindow Mw
        {
            get { return InfoControl.mw; }
            set { InfoControl.mw = value; }
        }

        private static string user;
        
        public static string User
        {
            get { return user; }
            set { user = value; }
        }

        private static string userId;

        public static string UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        private static DateTime loginTime;

        public static DateTime LoginTime
        {
            get { return loginTime; }
            set { loginTime = value; }
        }

        private static bool isRacingGame=true;

        public static bool IsRacingGame
        {
            get { return InfoControl.isRacingGame; }
            set { InfoControl.isRacingGame = value; }
        }
        
    }
}
