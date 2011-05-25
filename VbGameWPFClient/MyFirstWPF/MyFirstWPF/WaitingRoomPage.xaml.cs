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
using System.Diagnostics;
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
        private int index;                  //用户排在第几个位置
        private int currentMode;            //当前用户是否为房主，0为是，1为否
        private bool allReady;              //所有玩家是否都准备好（仅限于房主状态）
        private bool playerReady;           //该玩家是否准备好（仅限于加入游戏状态）

        private ClientEvt client;

        private ProcessStartInfo gameInfo;  //游戏的进程信息
        private Process gameProc;           //游戏进程

		public WaitingRoomPage()
		{
			this.InitializeComponent();
            client = InfoControl.Client;
            client.RoomDetail += new ClientEvt.RoomInfo(client_RoomDetail);
            client.RefreshRoomInfo += new Action(client_RefreshRoomInfo);
            client.BeginGame += new EventHandler(client_BeginGame);
            #region 分配位置
            users[0] = User0;
            users[1] = User1;
            users[2] = User2;
            users[3] = User3;
            users[4] = User4;
            users[5] = User5;
            #endregion
        }

        #region 启动游戏方法
        private void startGame()
        {
            gameInfo = new ProcessStartInfo();
            //gameInfo.FileName = "mspaint.exe";      //RacingGame.exe
            //gameInfo.WorkingDirectory = @"C:\WINDOWS\system32";     //Where?
            gameInfo.FileName = "RacingGame.exe";
            gameInfo.WorkingDirectory = @"J:\VB\VbGame\RacingGame\bin\x86\Debug\";
            gameInfo.WindowStyle = ProcessWindowStyle.Normal;
            gameInfo.Arguments = InfoControl.UserId;
            try
            {
                gameProc = Process.Start(gameInfo);
                System.Threading.Thread.Sleep(500);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("cannot find exe!");
            }
        }
        #endregion

        #region client系列事件响应
        //获取房间信息 host是房主名字
        void client_RoomDetail(string teamName, string mapName, string host,
            List<string> userId, List<string> userName, List<string> carId, List<bool> isReady)
        {
            this.Dispatcher.Invoke(new Action(() =>
                {
                    int i, totalReady;
                    titleText.Text = teamName;
                    this.teamName = teamName;
                    currentMode = (InfoControl.UserName == host ? MODE_HOST : MODE_JOIN);
                    #region 地图信息
                    for (i = 0; i < InfoControl.MapCount; i++)
                        if (InfoControl.MapTexts[i] == mapName)
                        {
                            MapCanvas.Background =
                                new ImageBrush(new BitmapImage(new Uri(InfoControl.MapPaths[i], UriKind.Relative)));
                            break;
                        }
                    #endregion
                    #region 找到用户位置
                    for (i = 0; i < userId.Count; i++ )
                        if (InfoControl.UserName == userName[i])
                        {
                            index = i;
                            playerReady = isReady[i];
                            break;
                        }
                    #endregion
                    #region 设置字体颜色
                    for (i = 0; i < userId.Count; i++)
                    {
                        users[i].PlayerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                        users[i].ReadyText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    }
                    users[index].PlayerText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    users[index].ReadyText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    #endregion
                    #region 改变准备状态
                    for (i = 0, totalReady = 0; i < userId.Count; i++)
                    {
                        if (userName[i] != host)
                            totalReady += (isReady[i] ? 1 : 0);     //统计除房主以外，准备的玩家个数
                        users[i].PlayerText.Text = userName[i];
                        users[i].Opacity = 0.95;
                        users[i].ReadyText.Text =
                            (isReady[i] ? "已准备" : (userName[i] == host ? "房主" : ""));
                        #region unused
                        //users[i].ReadyCanvas.Background =
                        //    new ImageBrush(new BitmapImage(new Uri(@"level\scene1.png", UriKind.Relative)));
                        //users[i].ReadyCanvas.Background =
                        //    new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                        #endregion
                    }
                    for (; i < TOTAL_USERS; i++)
                    {
                        users[i].PlayerText.Text = "";
						users[i].Opacity = 0.6;
                        users[i].ReadyText.Text = "";
                        #region unused
                        //users[i].ReadyCanvas.Background =
                        //    new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        //users[i].ReadyCanvas.Background = null;
                        #endregion
                    }
                    #endregion
                    #region 改变Choose按钮状态
                    if (totalReady == isReady.Count - 1 && totalReady != 0)
                        allReady = true;
                    else
                        allReady = false;
                    if (currentMode == MODE_JOIN)
                        StartText.Text = !playerReady ? "准备" : "取消准备";
                    else
                    {
                        StartText.Text = "开始游戏";
                        StartCanvas.Opacity = allReady ? 0.95 : 0.5;
                    }
                    #endregion
                }
            ));
        }
        
        //提示需要更新房间信息
        void client_RefreshRoomInfo()
        {
            client.GetRoomInfo();               //请求重新获取房间信息
        }

        //服务器发来开始游戏的命令
        void client_BeginGame(object sender, EventArgs e)
        {
            startGame();
            //MessageBox.Show("Game started! " + InfoControl.UserName);
        }
        #endregion

        #region IReload 成员

        public void Reload()
        {
            teamName = "";
            playerReady = allReady = false;
            client.GetRoomInfo();       //获取房间具体信息，消息返回方法是client_RoomDetail
        }

        #endregion

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)
        {
            
        }

        public int Choose()
        {
            if (currentMode == MODE_JOIN)
                client.Ready();
            else
            {
                if (allReady == true)
                {
                    client.Begin();
                }
            }
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