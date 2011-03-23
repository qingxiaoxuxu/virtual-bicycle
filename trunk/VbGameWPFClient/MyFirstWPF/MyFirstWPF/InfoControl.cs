using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyFirstWPF
{
    public class InfoControl
    {
        private static MainPage mainPage = null;
        public static MainPage Main_Page
        {
            get 
            {
                if (mainPage == null)
                    mainPage = new MainPage();
                return InfoControl.mainPage;
            }
            set
            {
                InfoControl.mainPage = value;
            }
        }

        private static SelectRoomPage selectRoomPage = null;
        public static SelectRoomPage Select_Room_Page
        {
            get
            {
                if (selectRoomPage == null)
                    selectRoomPage = new SelectRoomPage();
                return InfoControl.selectRoomPage;
            }
            set
            {
                InfoControl.selectRoomPage = value;
            }
        }

    }
}
