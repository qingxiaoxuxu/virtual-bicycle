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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BuzzWin;
using System.Windows.Threading;
using System.Threading;
using Client_v2.Model;
using System.IO;
using Client_v2.DampingAutoLearning;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Client_v2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer tm = new DispatcherTimer();
        List<ChartInfo> data;                   //当前要在屏幕上显示的数据
        List<ChartInfo> bufData;                //缓冲区里的数据
        int totalInfo;                          //当前获得的数据计数器
        bool isProcessing;                      //是否正在将内存中的数据转移到文件中
        private const int TOTAL = 5;
        private bool isLogin = false;
        //EventWaitHandle myEvent = new EventWaitHandle(true, EventResetMode.ManualReset);
        #region 常量
        public const string FILE_NAME = "history.csv";  //数据暂存文件名，放在exe目录下
        public const int MAX_POINT = 50;              //屏幕上最多显示的数据个数
        public const int MAX_BUFFER = 100;           //缓冲区中最多存放数据个数
        #endregion
        Random rd = new Random(DateTime.Now.Millisecond);
        VbCsvHelper.CsvHelper csv = new VbCsvHelper.CsvHelper();
        #region 界面
        UserControl0_login usrLogin;
        UserControl1_chart usrChart;
        UserControl2_game usrGame;
        UserControl3_set usrSet;
        UserControl4_auto usrAuto;
        #endregion

        #region 网络
        VbServer.Net.ServerEvt server;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InfoControl.device.OpenDevice(ref InfoControl.device.m_oBuzzDevice,0x8888,0x0006);
            InfoControl.device.GetSportStatus += new DeviceDataManager.F2(device_GetSportStatus);
            InfoControl.device.GetGameControl += new DeviceDataManager.F8(device_GetGameControl);
            #region 界面
            usrLogin = new UserControl0_login();
            usrChart = new UserControl1_chart();
            usrGame = new UserControl2_game();
            usrSet = new UserControl3_set();
            usrAuto = new UserControl4_auto();
            #endregion

            #region 网络
            server = new VbServer.Net.ServerEvt();
            #endregion
            data = new List<ChartInfo>();
            bufData = new List<ChartInfo>();
            totalInfo = 0;

            server.Server.StartServer();
            server.ForceBack += new VbServer.Net.ServerEvt.ForceBackHandler(server_ForceBack);

            #region 登陆信息
            //InfoControl.User = "黄婷";
            //InfoControl.UserId = 0;
            //InfoControl.LoginTime = DateTime.Now;
            #endregion
            #region 测试数据
            //for (totalInfo = 0; totalInfo < 10; totalInfo++)
            //    data.Add(new ChartInfo(0, totalInfo + 1, 300 + 0.5 * rd.Next(100), 150, 150, 5000));
            #endregion
            #region 添加界面
            canvas3.Children.Add(usrLogin);
            canvas3.Children.Add(usrChart);
            canvas3.Children.Add(usrGame);
            canvas3.Children.Add(usrSet);
            canvas3.Children.Add(usrAuto);
            usrChart.UpdateChartInfo(data);
            for (int i = 0; i < TOTAL; i++)
                canvas3.Children[i].Visibility =
                    (i == 0 ? Visibility.Visible : Visibility.Hidden);
            #endregion

            isProcessing = false;
            File.Create(FILE_NAME);
            tm.Interval = TimeSpan.FromSeconds(1);
            tm.Tick += new EventHandler(tm_Tick);
            tm.Start();

            isLogin = true;
        }

        static int preDamp = 0;
        void server_ForceBack(float f)
        {
            if (Math.Abs(f * 255 - preDamp) > 30)
            {
                DeviceDataManager.Damp d = new DeviceDataManager.Damp();
                d.value = (int)(-f * 127 + 128);
                InfoControl.device.SetDamp(d);
            }
            preDamp = (int)(f * 255);
        }


        static float preAngle = 0;
        static float preSpeed = 0;
        void device_GetGameControl(DeviceDataManager.GameControl gameControl)
        {
            if (InfoControl.IsRacingGame)
            {
                #region 网络

                if (gameControl.Btn1)
                {
                    server.Enter();
                }
                if (gameControl.Btn2)
                {
                    server.Escape();
                }
                if (gameControl.Btn3)
                {
                    server.Reset();
                }
                if (gameControl.Btn4)
                {
                    server.ViewChanged();
                }

                server.HandlebarRotated((float)((128 - gameControl.X)) / 5000);
                //preAngle = (float)((128 - gameControl.X) * 180 / 128);

                gameControl.Y = gameControl.Y - 128;

                server.WheelSpeedChangedRaw(gameControl.Y);
                if (preSpeed >= 0 && gameControl.Y >= 0 && gameControl.Y - preSpeed > -1)
                {
                    server.WheelSpeedChanged(gameControl.Y, (float)((gameControl.Y - preSpeed) + gameControl.Y * gameControl.Y * 0.00003));
                }
                //else if (preSpeed <= 0 && gameControl.Y <= 0 && gameControl.Y - preSpeed < 5)
                //{
                //    server.WheelSpeedChanged(gameControl.Y, (float)((gameControl.Y - preSpeed)));
                //}

                preSpeed = gameControl.Y;
                //Console.WriteLine(gameControl.Btn1);
                //Console.WriteLine(gameControl.Btn2);
                //Console.WriteLine(gameControl.Btn3);
                //Console.WriteLine(gameControl.Btn4);
                //Console.WriteLine(gameControl.X);
                //Console.WriteLine(gameControl.Y);
                #endregion
            }
            else
            {
                #region 键盘控制
                if (gameControl.Btn1)
                {
                    Demo.Keyboard.Press(Key.Space);
                    Demo.Keyboard.Release(Key.Space);
                }
                if (gameControl.Btn2)
                {
                    Demo.Keyboard.Press(Key.LeftCtrl);
                    Demo.Keyboard.Release(Key.LeftCtrl);
                }
                if (gameControl.Btn3)
                {
                    Demo.Keyboard.Press(Key.LeftShift);
                    Demo.Keyboard.Release(Key.LeftShift);
                }
                if (gameControl.Btn4)
                {
                    Demo.Keyboard.Press(Key.C);
                    Demo.Keyboard.Release(Key.C);
                }

                gameControl.Y = gameControl.Y - 128;

                gameControl.X = gameControl.X - 128;
                if (gameControl.X > 60)
                {
                    Console.WriteLine("Left");
                    Demo.Keyboard.Press(Key.Left);
                    Demo.Keyboard.Release(Key.Left);
                    
                }
                if (gameControl.X < -60)
                {
                    Console.WriteLine("Right");
                    Demo.Keyboard.Press(Key.Right);
                    Demo.Keyboard.Release(Key.Right);
                }
                if (gameControl.Y > 60)
                {
                    Console.WriteLine("Up");
                    Demo.Keyboard.Press(Key.Up);
                    Demo.Keyboard.Release(Key.Up);
                }
                if (gameControl.Y < -60)
                {
                    Console.WriteLine("Down");
                    Demo.Keyboard.Press(Key.Down);
                    Demo.Keyboard.Release(Key.Down);
                }
                #endregion
            }
        }

        void tm_Tick(object sender, EventArgs e)
        {   
            usrChart.UpdateChartInfo(data);
        }

        void device_GetSportStatus(DeviceDataManager.SportStatus sportStatus)
        {
            Console.WriteLine(sportStatus.Speed);
            totalInfo++;
            TimeSpan time = DateTime.Now - InfoControl.LoginTime;
            data.Add(new ChartInfo(
                totalInfo,
                Convert.ToInt32(time.TotalSeconds),
                sportStatus.Speed / 6.0 * 5.0,
                sportStatus.HeartRate,
                sportStatus.distance,
                sportStatus.distance * sportStatus.load
                ));
            if (data.Count > MAX_POINT)         //将以前的数据移出
            {
                bufData.Add(data[0]);
                data.RemoveAt(0);
            }
            if (bufData.Count >= MAX_BUFFER && !isProcessing)
            {
                isProcessing = true;                    //正在进行数据转移
                Thread t = new Thread(new ThreadStart(transferDataToFile));
                t.Start();
            }
        }

        /// <summary>
        /// 将缓存区域中的健身信息放入数据库中
        /// </summary>
        void transferDataToFile()
        {
            List<List<Object>> saveData = new List<List<object>>();
            for (int i = 0; i < MAX_BUFFER; i++)
            {
                ChartInfo info = bufData[0];
                List<Object> convert = csv.getOriginData
                    (info.CurrentTime, info.Speed, info.HeartBeat, info.Distance, info.Energy);
                saveData.Add(convert);
                bufData.RemoveAt(0);
            }
            csv.writeManyDataInCsv(saveData, FILE_NAME);
            isProcessing = false;
        }

        //点击用户登陆按钮
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < TOTAL; i++)
                canvas3.Children[i].Visibility = 
                    (i == 0 ? Visibility.Visible : Visibility.Hidden);
        }
        
        //点击设置按钮
        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            if (!isLogin) return;
            for (int i = 0; i < TOTAL; i++)
                canvas3.Children[i].Visibility =
                    (i == 3 ? Visibility.Visible : Visibility.Hidden);
            
        }
        
        //点击查看健身数据按钮
        private void btnChart_Click(object sender, RoutedEventArgs e)
        {
            if (!isLogin) return;
            for (int i = 0; i < TOTAL; i++)
                canvas3.Children[i].Visibility =
                    (i == 1 ? Visibility.Visible : Visibility.Hidden);
        }

        //点击选择游戏按钮
        private void btnGame_Click(object sender, RoutedEventArgs e)
        {
            if (!isLogin) return;
            for (int i = 0; i < TOTAL; i++)
                canvas3.Children[i].Visibility =
                    (i == 2 ? Visibility.Visible : Visibility.Hidden);
        }

        //智能调节阻尼
        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            if (!isLogin) return;
            for (int i = 0; i < TOTAL; i++)
                canvas3.Children[i].Visibility =
                    (i == 4 ? Visibility.Visible : Visibility.Hidden);
        }

        //用户退出
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            //transferDataToDB();
            //InfoControl.User = "";
            //InfoControl.UserId = -1;
            this.Close();
        }

        private void transferDataToDB()
        { }


    }

}
