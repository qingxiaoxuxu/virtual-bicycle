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

namespace Client_v2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public DeviceDataManager device = new DeviceDataManager();
        DispatcherTimer tm = new DispatcherTimer();
        List<ChartInfo> data;                   //当前要在屏幕上显示的数据
        List<ChartInfo> bufData;                //缓冲区里的数据
        int totalInfo;                          //当前获得的数据计数器
        bool isProcessing;                      //是否正在将内存中的数据转移到文件中
        #region 常量
        public const string FILE_NAME = "history.csv";  //数据暂存文件名，放在exe目录下
        public const int MAX_POINT = 50;              //屏幕上最多显示的数据个数
        public const int MAX_BUFFER = 100;           //缓冲区中最多存放数据个数
        #endregion
        Random rd = new Random(DateTime.Now.Millisecond);
        VbCsvHelper.CsvHelper csv = new VbCsvHelper.CsvHelper();
        #region 界面
        UserControl0_login usrLogin = new UserControl0_login();
        UserControl1_chart usrChart = new UserControl1_chart();
        UserControl2_game usrGame = new UserControl2_game();
        UserControl3_set usrSet = new UserControl3_set();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            device.OpenDevice(ref device.m_oBuzzDevice,0x8888,0x0006);
            device.GetSportStatus += new DeviceDataManager.F2(device_GetSportStatus);
            device.GetGameControl += new DeviceDataManager.F8(device_GetGameControl);
            data = new List<ChartInfo>();
            bufData = new List<ChartInfo>();
            totalInfo = 0;

            #region 登陆信息
            InfoControl.User = "黄婷";
            InfoControl.UserId = 0;
            InfoControl.LoginTime = DateTime.Now;
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
            usrChart.UpdateChartInfo(data);
            for (int i = 0; i < 4; i++)
                canvas3.Children[i].Visibility =
                    (i == 0 ? Visibility.Visible : Visibility.Hidden);
            #endregion

            initExerciseFile();
            isProcessing = false;
            tm.Interval = TimeSpan.FromSeconds(1);
            tm.Tick += new EventHandler(tm_Tick);
            tm.Start();
        }

        void initExerciseFile()
        {
            File.Create(FILE_NAME);
        }

        void device_GetGameControl(DeviceDataManager.GameControl gameControl)
        {
            Console.WriteLine(totalInfo + " " + gameControl.Y);
            totalInfo++;
            TimeSpan time = DateTime.Now - InfoControl.LoginTime;
            data.Add(new ChartInfo(
                totalInfo,
                Convert.ToInt32(time.TotalSeconds),
                Convert.ToDouble(128 - gameControl.Y),
                0,
                0,
                0
                ));
            if (data.Count > MAX_POINT)         //将以前的数据移出
            {
                bufData.Add(data[0]);
                data.RemoveAt(0);
            }
            if (bufData.Count % MAX_BUFFER == 0)
            {
                //isProcessing = true;
                Thread t = new Thread(new ThreadStart(transferDataToFile));
                t.Start();
            }
        }

        void tm_Tick(object sender, EventArgs e)
        {   
            usrChart.UpdateChartInfo(data);
        }

        void device_GetSportStatus(DeviceDataManager.SportStatus sportStatus)
        {
            //Console.WriteLine(sportStatus.Speed);
            //totalInfo++;
            //TimeSpan time = DateTime.Now - InfoControl.LoginTime;
            //data.Add(new ChartInfo(
            //    totalInfo, 
            //    Convert.ToInt32(time.TotalSeconds),
            //    sportStatus.Speed, 
            //    sportStatus.HeartRate, 
            //    sportStatus.distance, 
            //    sportStatus.distance * sportStatus.load
            //    ));
            //if (data.Count > MAX_POINT)         //将以前的数据移出
            //{
            //    bufData.Add(data[0]);
            //    data.RemoveAt(0);
            //}
            //if (bufData.Count >= MAX_BUFFER)
            //{
            //    Thread t = new Thread(new ThreadStart(transferDataToFile));
            //    t.Start();
            //}
        }

        /// <summary>
        /// 将缓存区域中的健身信息放入数据库中
        /// </summary>
        void transferDataToFile()
        {
            List<List<Object>> saveData = new List<List<object>>();
            for (int i = 0; i < MAX_BUFFER; i++ )
            {
                ChartInfo info = bufData[0];
                List<Object> convert = csv.getOriginData
                    (info.CurrentTime, info.Speed, info.HeartBeat, info.Distance, info.Energy);
                saveData.Add(convert);
                bufData.RemoveAt(0);
            }
            csv.writeManyDataInCsv(saveData, FILE_NAME);
            //isProcessing = false;           //数据转移完毕
        }

        //点击设置按钮
        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                canvas3.Children[i].Visibility =
                    (i == 3 ? Visibility.Visible : Visibility.Hidden);
            
        }
        
        //点击用户登入按钮
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                canvas3.Children[i].Visibility = 
                    (i == 0 ? Visibility.Visible : Visibility.Hidden);
        }
        
        //点击查看健身数据按钮
        private void btnChart_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                canvas3.Children[i].Visibility =
                    (i == 1 ? Visibility.Visible : Visibility.Hidden);
        }

        //点击选择游戏按钮
        private void btnGame_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 4; i++)
                canvas3.Children[i].Visibility =
                    (i == 2 ? Visibility.Visible : Visibility.Hidden);
        }

        //用户退出
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            transferDataToDB();
            InfoControl.User = "";
            InfoControl.UserId = -1;
        }

        private void transferDataToDB()
        { }
    }

}
