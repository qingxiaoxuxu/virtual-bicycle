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
        }

        private void btnRacingGame_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
