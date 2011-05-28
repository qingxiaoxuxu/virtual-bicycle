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
using System.Diagnostics;
using System.IO;

namespace Client_v2
{
    /// <summary>
    /// UserControl2_game.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl2_game : UserControl
    {
        public UserControl2_game()
        {
            InitializeComponent();
            InitialButton();
        }
        List<Game> gameList = new List<Game>();
        public void InitialButton()
        {
            gameList.Clear();
            using (StreamReader sr = new StreamReader("path.ini",Encoding.Default))
            {
                int count=0;
                while (!sr.EndOfStream)
                {
                    string gameName = sr.ReadLine();
                    string gamePath = sr.ReadLine();
                    string gamePic = sr.ReadLine();
                    Image img = new Image();
                    img.Tag = count;
                    img.ToolTip = gameName.ToString();
                    img.Height=140;
                    img.Width=140;
                    img.Margin = new Thickness(20);
                    if (count == 0) 
                        img.MouseDown +=new MouseButtonEventHandler(btnRacingGame_Click); 
                    else
                        img.MouseDown+=new MouseButtonEventHandler(img_MouseDown);
                    //if (gameName != "" && gamePath != "")
                    {
                        gameList.Add(new Game(gameName, gamePath, gamePic));
                        // 创建一个源
                        BitmapImage myBitmapImage = new BitmapImage();
                        // BitmapImage.UriSource必须使用BeginInit/EndInit块
                        myBitmapImage.BeginInit();
                        myBitmapImage.UriSource = new Uri(gamePic, UriKind.Absolute);
                        myBitmapImage.EndInit();
                        //把源赋给Image控件
                        img.Source = myBitmapImage;

                    }
                    count++;
                    wrapPanel.Children.Add(img);
                }
            }
        }

        void img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            InfoControl.IsRacingGame = false;
            ProcessStartInfo gameInfo = new ProcessStartInfo();
            gameInfo.FileName = gameList[Convert.ToInt32((sender as Button).Tag)].Name + ".exe";
            gameInfo.WorkingDirectory = gameList[Convert.ToInt32((sender as Button).Tag)].Path;
            gameInfo.WindowStyle = ProcessWindowStyle.Normal;
            try
            {
                Process gameProcess = Process.Start(gameInfo);
                System.Threading.Thread.Sleep(500);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("File Not Found!");
            }
        }

        private void btnRacingGame_Click(object sender, RoutedEventArgs e)
        {
            InfoControl.IsRacingGame = true;
            string userId, user;
            Random rd = new Random(DateTime.Now.Millisecond);
            userId = rd.Next(100000).ToString();
            user = "LKQ" + userId;
            ProcessStartInfo gameInfo = new ProcessStartInfo();
            gameInfo.FileName = gameList[Convert.ToInt32((sender as Button).Tag)].Name + ".exe";
            gameInfo.WorkingDirectory = gameList[Convert.ToInt32((sender as Button).Tag)].Path;
            gameInfo.WindowStyle = ProcessWindowStyle.Normal;
            gameInfo.Arguments = user + " " + userId;
            try
            {
                Process gameProcess = Process.Start(gameInfo);
                System.Threading.Thread.Sleep(500);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("File Not Found!");
            }
        }
        public class Game
        {
            public string Name = "";
            public string Path = "";
            public string Pic = "";
            public Game(string name ,string path,string pic)
            {
                this.Name = name;
                this.Path = path;
                this.Pic = pic;
            }
        }
    }
}
