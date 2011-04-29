using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VbClient.Net;

namespace MyFirstWPF
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class MainFrame : Window
    {
		const int PageCount = 5;			//界面数量
        UserControl[] pages = new UserControl[PageCount];

        public const int INDEX_MAIN_PAGE = 0;
        public const int INDEX_SINGLE_SELECT_MAP_PAGE = 1;
        public const int INDEX_MULTI_SELECT_ROOM_PAGE = 2;
        public const int INDEX_MULTI_SELECT_MAP_PAGE = 3;
        public const int INDEX_WAITING_ROOM_PAGE = 4;
        private string user = "LKQ";

        private ClientEvt client;

        public MainFrame()
        {
            InitializeComponent();
            this.KeyDown +=new KeyEventHandler(MainFrame_KeyDown);
            client = InfoControl.Client;                        //获取客户端
            client.Client.ConnectError+=new System.IO.ErrorEventHandler(Client_ConnectError);
            client.LoginSuccess += new EventHandler(client_LoginSuccess);
            client.LoginFailure += new EventHandler(client_LoginFailure);
        }

        void Client_ConnectError(object sender, System.IO.ErrorEventArgs e)
        {
            MessageBox.Show("Connect Err.");
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();                                               //匿名委托，支线程调用主线程函数
            }));
        }

        void client_LoginSuccess(object sender, EventArgs e)
        {
            InfoControl.UserName = user;
            //MessageBox.Show("Login Success!");
        }

        void client_LoginFailure(object sender, EventArgs e)
        {
            MessageBox.Show("Login Failure!");
            this.Dispatcher.Invoke(new Action(() => {
                this.Close();                                               //匿名委托，支线程调用主线程函数
            }));
        }

        private void MainFrame_KeyDown(object sender, KeyEventArgs e)       //处理键盘事件响应
        {
            int index = -1;                                                 //界面跳转界面的索引

            if (e.Key != Key.Enter && e.Key != Key.Escape 
                && e.Key != Key.Space)                                      //无需进行界面跳转
            {
                ((IKeyDown)this.Content).KeyboardDown(sender, e);           //具体的界面内变化由各自界面实现
                return;
            }

            if (e.Key == Key.Enter)                                         //敲回车，即跳转界面
                index = ((IKeyDown)this.Content).Choose();                  //获得新界面的索引
            else if (e.Key == Key.Escape)                                   //敲Esc，界面返回
                index = ((IKeyDown)this.Content).MoveBack();                //获得返回的界面索引
            else if (e.Key == Key.Space)
                index = ((IKeyDown)this.Content).BtnFunction1();            //获得第一个功能键的界面索引
            if (index >= 0)                                                 //索引合法
            {
                if (index == INDEX_MULTI_SELECT_ROOM_PAGE)
                    ((IReload)pages[index]).Reload();
                this.Content = pages[index];                                //显示新界面
            }
            else if (index == -2)                                           //退出程序
                this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.Login(user, "123456");
            #region 界面初始化
            pages[0] = InfoControl.Main_Page;
            pages[1] = InfoControl.Single_Select_Map_Page;
            pages[2] = InfoControl.Select_Room_Page;
            pages[3] = InfoControl.Multi_Select_Map_Page;
            pages[4] = InfoControl.Waiting_Room_Page;
            this.Content = pages[0];
            #endregion
        }

        
    }
}
