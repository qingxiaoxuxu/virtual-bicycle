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
using System.Threading;
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
        private int currentIndex;           //当前处于哪个界面
        #region 房间索引
        public const int INDEX_MAIN_PAGE = 0;
        public const int INDEX_SINGLE_SELECT_MAP_PAGE = 1;
        public const int INDEX_MULTI_SELECT_ROOM_PAGE = 2;
        public const int INDEX_MULTI_SELECT_MAP_PAGE = 3;
        public const int INDEX_WAITING_ROOM_PAGE = 4;
        #endregion
        #region 健身车按钮索引
        public const int BTN_CHOOSE = 0;
        public const int BTN_ESCAPE = 1;
        public const int BTN_FUN1 = 2;
        public const int BTN_FUN2 = 3;
        public const int BTN_ROTATE_LEFT = 4;
        public const int BTN_ROTATE_RIGHT = 5;
        public const int BTN_ROTATE_UP = 6;
        public const int BTN_ROTATE_DOWN = 7;
        #endregion

        Random rd = new Random(DateTime.Now.Millisecond);
        private string user;
        private string userId;
        
        private ClientEvt client;
        private ClientIEvt iClient;
        
        //private EventWaitHandle pressEvent = new EventWaitHandle(true, EventResetMode.ManualReset);

        static int a = 0, b = 0, c = 0, d = 0;

        const int PRESS_DELAY = 1000;        //按键冷却时间
        #region 判断按钮是否被按下，以防多次触发按钮响应
        private bool isEscapePressed = false;
        private bool isChoosePressed = false;
        private bool isFun1Pressed = false;
        private bool isFun2Pressed = false;
        private bool isRotated = false;
        private bool isRiding = false;
        #endregion


        public MainFrame(string usr, string usrId)
        {
            InitializeComponent();
            this.KeyDown +=new KeyEventHandler(MainFrame_KeyDown);
            client = InfoControl.Client;                        //获取客户端
            iClient = new ClientIEvt("127.0.0.1");
            #region 订阅client事件
            client.Client.ConnectError += new System.IO.ErrorEventHandler(Client_ConnectError);
            client.LoginSuccess += new EventHandler(client_LoginSuccess);
            client.LoginFailure += new EventHandler(client_LoginFailure);
            #endregion
            #region 订阅iClient事件
            iClient.Escape += new Action(iClient_Escape);                       //对应ESC
            iClient.ViewChanged += new Action(iClient_Choose);                  //对应CHOOSE
            iClient.Enter += new Action(iClient_Function1);                     //对应FUN1
            iClient.Reset += new Action(iClient_Function2);                     //对应FUN2
            iClient.HandlebarRotated += new ClientIEvt.HandlebarRotatedHandler(iClient_HandlebarRotated);
            iClient.WheelSpeedChanged += new ClientIEvt.WheelSpeedChangedHandler(iClient_WheelSpeedChanged);
            #endregion
            userId = usrId;
            user = usr;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client.Login(user, userId);
            #region 界面初始化
            pages[0] = InfoControl.Main_Page;
            pages[1] = InfoControl.Single_Select_Map_Page;
            pages[2] = InfoControl.Select_Room_Page;
            pages[3] = InfoControl.Multi_Select_Map_Page;
            pages[4] = InfoControl.Waiting_Room_Page;
            this.Content = pages[0];
            currentIndex = INDEX_MAIN_PAGE;
            #endregion
        }

        #region 健身车事件
        void iClient_HandlebarRotated(ClientIEvt.HandlebarRotatedEventArgs e)
        {  
            if (e.Angle < -0.02)
            {
                #region 健身车右转
                Thread t = new Thread(new ThreadStart(() =>
                    {
                        if (!isRotated)
                        {
                            isRotated = true;
                            MainFrame_KeyDown_Bicycle(BTN_ROTATE_RIGHT);
                            Thread.Sleep(PRESS_DELAY);
                            isRotated = false;
                        }
                    }
                ));
                t.Start();
                #endregion
            }
            else if (e.Angle > 0.02)
            {
                #region 健身车左转
                Thread t = new Thread(new ThreadStart(() =>
                    {
                        if (!isRotated)
                        {
                            isRotated = true;
                            MainFrame_KeyDown_Bicycle(BTN_ROTATE_LEFT);
                            Thread.Sleep(PRESS_DELAY);
                            isRotated = false;
                        }
                    }
                ));
                t.Start();
                #endregion
            }
        }

        void iClient_WheelSpeedChanged(ClientIEvt.WheelSpeedChangedEventArgs e)
        {
            //if (a % 100 == 0)
            //    Console.WriteLine(a + " " + e.Speed + " " + e.SpeedChange);
            //a++;
            if (e.Speed > 64)
            {
                #region 向前踩
                Thread t = new Thread(new ThreadStart(() =>
                    {
                        if (!isRiding)
                        {
                            isRiding = true;
                            MainFrame_KeyDown_Bicycle(BTN_ROTATE_DOWN);
                            Thread.Sleep(PRESS_DELAY);
                            isRiding = false;
                        }
                    }
                ));
                t.Start();
                #endregion
            }
            else if (e.Speed < -64)
            {
                #region 向后踩
                #endregion
            }
        }

        void iClient_Escape()
        {
            #region unused
            //if (pressEvent.WaitOne())
            //{
            //    MessageBox.Show("ESC:" + (b++).ToString());
            //    pressEvent.Reset();
            //    MainFrame_KeyDown_Bicycle(BTN_ESCAPE);
            //    Thread.Sleep(1000);
            //    pressEvent.Set();
            //}
            #endregion
            Thread t = new Thread(new ThreadStart(() =>
            {
                if (!isEscapePressed)
                {
                    isEscapePressed = true;
                    //Console.WriteLine(c++ + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
                    MainFrame_KeyDown_Bicycle(BTN_ESCAPE);
                    Thread.Sleep(PRESS_DELAY);
                    isEscapePressed = false;
                }
                //else
                //{
                //    Console.WriteLine(c++ + " Oops...");
                //}
            }
            ));
            t.Start();
        }

        void iClient_Choose()
        {
            #region unused
            //if (pressEvent.WaitOne())
            //{
            //    MessageBox.Show("CHS:" + (c++).ToString());
            //    pressEvent.Reset();
            //    MainFrame_KeyDown_Bicycle(BTN_CHOOSE);
            //    Thread.Sleep(2000);
            //    pressEvent.Set();
            //}
            #endregion
            Thread t = new Thread(new ThreadStart(() =>
            {
                if (!isChoosePressed)
                {
                    isChoosePressed = true;
                    //Console.WriteLine(b++ + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
                    MainFrame_KeyDown_Bicycle(BTN_CHOOSE);
                    Thread.Sleep(PRESS_DELAY);
                    isChoosePressed = false;
                }
                //else
                //{
                //    Console.WriteLine(b++ + " Oops...");
                //}
            }
            ));
            t.Start();
        }

        void iClient_Function1()
        {
            #region unused
            //if (!watchDog)
            //{
            //    watchDog = true;
            //    //MessageBox.Show("FUN1:" + (a++).ToString());
            //    //pressEvent.Reset();
            //    MainFrame_KeyDown_Bicycle(BTN_FUN1);
            //    //Thread.Sleep(1000);
            //    watchDog = false;
            //}
            #endregion
            Thread t = new Thread(new ThreadStart(() =>
            {
                if (!isFun1Pressed)
                {
                    isFun1Pressed = true;
                    //Console.WriteLine(a++ + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
                    MainFrame_KeyDown_Bicycle(BTN_FUN1);
                    Thread.Sleep(PRESS_DELAY);
                    isFun1Pressed = false;
                }
                //else
                //{
                //    Console.WriteLine(a++ + " Oops...");
                //}
            }
            ));
            t.Start();
        }

        void iClient_Function2()
        {
            //MessageBox.Show("FUN2:" + (d++).ToString());
            //MainFrame_KeyDown_Bicycle(BTN_FUN2);
            Thread t = new Thread(new ThreadStart(() =>
            {
                if (!isFun2Pressed)
                {
                    isFun2Pressed = true;
                    //Console.WriteLine(d++ + " " + DateTime.Now.Second + " " + DateTime.Now.Millisecond);
                    MainFrame_KeyDown_Bicycle(BTN_FUN2);
                    Thread.Sleep(PRESS_DELAY);
                    isFun2Pressed = false;
                }
                //else
                //{
                //    Console.WriteLine(d++ + " Oops...");
                //}
            }
            ));
            t.Start();
        }
        #endregion

        #region client系列事件响应
        //连接错误
        void Client_ConnectError(object sender, System.IO.ErrorEventArgs e)
        {
            MessageBox.Show("Connect Err.");
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();                                               //匿名委托，支线程调用主线程函数
            }));
        }

        //登录成功
        void client_LoginSuccess(object sender, EventArgs e)
        {
            InfoControl.UserName = user;
            InfoControl.UserId = userId;
            //MessageBox.Show("Login Success!");
        }

        //连接失败
        void client_LoginFailure(object sender, EventArgs e)
        {
            MessageBox.Show("Login Failure!");
            this.Dispatcher.Invoke(new Action(() => {
                this.Close();                                               //匿名委托，支线程调用主线程函数
            }));
        }
        #endregion

        #region 按键操作响应
        private void MainFrame_KeyDown_Bicycle(int mode)
        {
            this.Dispatcher.Invoke(new Action(() =>
                {
                    #region 界面控制
                    int index = -1;
                    if (mode <= 3)
                    {
                        if (mode == BTN_CHOOSE)
                            index = ((IKeyDown)this.Content).Choose();                  //获得新界面的索引
                        else if (mode == BTN_ESCAPE)
                            index = ((IKeyDown)this.Content).MoveBack();                //获得返回的界面索引
                        else if (mode == BTN_FUN1)
                            index = ((IKeyDown)this.Content).BtnFunction1();            //获得第一个功能键的界面索引
                    }
                    else if (mode >= 4 &&
                        currentIndex != INDEX_WAITING_ROOM_PAGE)
                    {
                        if (mode == BTN_ROTATE_UP)
                            ((IMove)this.Content).moveUp();
                        else if (mode == BTN_ROTATE_DOWN)
                            ((IMove)this.Content).moveDown();
                        else if (mode == BTN_ROTATE_LEFT)
                            ((IMove)this.Content).moveLeft();
                        else if (mode == BTN_ROTATE_RIGHT)
                            ((IMove)this.Content).moveRight();
                    }
                    if (index >= 0)                                                 //索引合法
                    {
                        if (index == INDEX_MULTI_SELECT_ROOM_PAGE ||
                            index == INDEX_WAITING_ROOM_PAGE)
                            ((IReload)pages[index]).Reload();
                        System.Threading.Thread.Sleep(50);                          //延迟0.05秒，进行界面变换
                        this.Content = pages[index];                                //显示新界面
                        currentIndex = index;
                    }
                    else if (index == -2)                                           //退出程序
                        this.Close();
                    #endregion
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
                if (index == INDEX_MULTI_SELECT_ROOM_PAGE ||
                    index == INDEX_WAITING_ROOM_PAGE)
                    ((IReload)pages[index]).Reload();
                System.Threading.Thread.Sleep(50);                          //延迟0.05秒，进行界面变换
                this.Content = pages[index];                                //显示新界面
                currentIndex = index;
            }
            else if (index == -2)                                           //退出程序
                this.Close();
        }
        #endregion

    }
}
