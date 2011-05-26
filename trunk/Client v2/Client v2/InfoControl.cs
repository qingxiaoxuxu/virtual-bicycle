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

        private static string user;
        
        public static string User
        {
            get { return user; }
            set { user = value; }
        }

        private static int userId;

        public static int UserId
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

        private static bool isRacingGame=false;

        public static bool IsRacingGame
        {
            get { return InfoControl.isRacingGame; }
            set { InfoControl.isRacingGame = value; }
        }
        
    }
}
