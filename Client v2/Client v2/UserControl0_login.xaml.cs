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
using Client_v2.DAO;

namespace Client_v2
{
    /// <summary>
    /// UserControl0_login.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl0_login : UserControl
    {
        public UserControl0_login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string userid = textUser.Text;
            string pw = textPw.Password;
            string res;
            res = DataManage.findUser(userid, pw);
            if (res == null)
            {
                MessageBox.Show("用户名或密码错误，请重试！", "Login Error");
                textUser.Text = "";
                textPw.Password = "";
            }
            else
            {
                //res = (userid == "11113" ? "lkq" : "test");
                MessageBox.Show("欢迎您，" + res + "!", "Login Success");
                InfoControl.User = res;
                InfoControl.UserId = userid;
                InfoControl.LoginTime = DateTime.Now;
                InfoControl.Mw.LoginAction();
            }
        }
    }
}
