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

        private static SelectMapPage singleSelectMapPage = null;
        public static SelectMapPage Single_Select_Map_Page
        {
            get
            {
                if (singleSelectMapPage == null)
                    singleSelectMapPage = new SelectMapPage(0);
                return InfoControl.singleSelectMapPage;
            }
            set
            {
                InfoControl.singleSelectMapPage = value;
            }
        }

        private static SelectMapPage multiSelectMapPage = null;
        public static SelectMapPage Multi_Select_Map_Page
        {
            get
            {
                if (multiSelectMapPage == null)
                    multiSelectMapPage = new SelectMapPage(1);
                return InfoControl.multiSelectMapPage;
            }
            set
            {
                InfoControl.multiSelectMapPage = value;
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

        private static WaitingRoomPage waitingRoomPage = null;

        public static WaitingRoomPage Waiting_Room_Page
        {
            get 
            {
                if (waitingRoomPage == null)
                    waitingRoomPage = new WaitingRoomPage();
                return InfoControl.waitingRoomPage; 
            }
            set { InfoControl.waitingRoomPage = value; }
        }
        

    }
}
