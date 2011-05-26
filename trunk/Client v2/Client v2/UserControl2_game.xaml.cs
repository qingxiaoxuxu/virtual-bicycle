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
                    Button btn = new Button();
                    btn.Tag = count;
                    btn.Content = gameName.ToString();
                    btn.Height=140;
                    btn.Width=140;
                    btn.Margin = new Thickness(20);
                    if (count == 0) 
                        btn.Click += new RoutedEventHandler(btnRacingGame_Click);
                    else
                        btn.Click += new RoutedEventHandler(btn_Click);
                    //if (gameName != "" && gamePath != "")
                    {
                        gameList.Add(new Game(gameName, gamePath, ""));
                    }
                    count++;
                    wrapPanel.Children.Add(btn);
                }
            }
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            InfoControl.IsRacingGame = false;
            ProcessStartInfo gameInfo = new ProcessStartInfo();
            gameInfo.FileName = gameList[Convert.ToInt32((sender as Button).Tag)].Name+".exe";
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
            gameInfo.FileName = "MyFirstWPF.exe";
            gameInfo.WorkingDirectory = @"J:\VB\MyFirstWPF\MyFirstWPF\bin\Debug";
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
