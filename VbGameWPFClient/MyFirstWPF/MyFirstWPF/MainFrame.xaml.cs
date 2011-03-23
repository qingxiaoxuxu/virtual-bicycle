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

namespace MyFirstWPF
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class MainFrame : Window
    {
		const int PageCount = 4;			//界面数量
        UserControl[] pages = new UserControl[PageCount];

        public MainFrame()
        {
            InitializeComponent();
            #region 界面初始化
            pages[0] = InfoControl.Main_Page;
            pages[1] = InfoControl.Single_Select_Map_Page;
            pages[2] = InfoControl.Multi_Select_Map_Page;
            pages[3] = InfoControl.Select_Room_Page;
            this.Content = pages[0];
            #endregion
            this.KeyDown +=new KeyEventHandler(MainFrame_KeyDown);
        }

        private void MainFrame_KeyDown(object sender, KeyEventArgs e)       //处理键盘事件响应
        {
            int index = -1;                                                 //界面跳转界面的索引

            if (e.Key != Key.Enter && e.Key != Key.Escape)                  //无需进行界面跳转
            {
                ((IKeyDown)this.Content).KeyboardDown(sender, e);           //具体的界面内变化由各自界面实现
                return;
            }

            if (e.Key == Key.Enter)                                         //敲回车，即跳转界面
                index = ((IKeyDown)this.Content).Choose();                  //获得新界面的索引
            else if (e.Key == Key.Escape)                                   //敲Esc，界面返回
                index = ((IKeyDown)this.Content).MoveBack();                //获得返回的界面索引
                
            if (index >= 0)                                                 //索引合法
                this.Content = pages[index];                                //显示新界面
        }

        
    }
}
