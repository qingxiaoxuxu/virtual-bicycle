using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VbClient.Net;

namespace MyFirstWPF
{
	/// <summary>
	/// WaitingRoomPage.xaml 的交互逻辑
	/// </summary>
	public partial class WaitingRoomPage : UserControl, IKeyDown, IReload
	{
        public const int MODE_HOST = 0;
        public const int MODE_JOIN = 1;
        public const int TOTAL_USERS = 4;
        UserInfoBlock[] users = new UserInfoBlock[6];
        string teamName;
        private ClientEvt client;

		public WaitingRoomPage()
		{
			this.InitializeComponent();
            client = InfoControl.Client;
            client.RoomDetail += new ClientEvt.RoomInfo(client_RoomDetail);
            client.RefreshRoomInfo += new Action(client_RefreshRoomInfo);
            #region 分配位置
            users[0] = User0;
            users[1] = User1;
            users[2] = User2;
            users[3] = User3;
            users[4] = User4;
            users[5] = User5;
            #endregion
        }

        #region client系列事件响应
        //获取房间信息
        void client_RoomDetail(string teamName, string mapName, string host,
            List<string> userId, List<string> userName, List<string> carId, List<bool> isReady)
        {
            this.Dispatcher.Invoke(new Action(() =>
                {
                    int i;
                    titleText.Text = teamName;
                    this.teamName = teamName;
                    for (i = 0; i < userId.Count; i++)
                    {
                        users[i].PlayerText.Text = userId[i] + userName[i] + carId[i];
                        users[0].ReadyCanvas.Background =
                            new ImageBrush(new BitmapImage(new Uri(@"level\scene1.png", UriKind.Relative)));
                    }
                    for (; i < TOTAL_USERS; i++)
                    {
                        users[i].PlayerText.Text = "";
                        users[0].ReadyCanvas.Background =
                            new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    }
                    for (i = 0; i < InfoControl.MapCount; i++)
                        if (InfoControl.MapTexts[i] == mapName)
                        {
                            MapCanvas.Background =
                                new ImageBrush(new BitmapImage(new Uri(InfoControl.MapPaths[i], UriKind.Relative)));
                            break;
                        }
                }
            ));
        }
        
        //提示需要更新房间信息
        void client_RefreshRoomInfo()
        {
            client.GetRoomInfo();               //请求重新获取房间信息
        }
        #endregion

        #region IReload 成员

        public void Reload()
        {
            teamName = "";
            client.GetRoomInfo();       //获取房间具体信息，消息返回方法是client_RoomDetail
        }

        #endregion

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)
        {
            
        }

        public int Choose()
        {
            return -1;
        }

        public int MoveBack()
        {
            client.LeaveTeam(teamName);
            return MainFrame.INDEX_MULTI_SELECT_ROOM_PAGE;
        }

        public int BtnFunction1()
        {
            return -1;
        }

        #endregion
    }
}