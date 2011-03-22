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
		const int PageCount = 1;			//界面数量
        UserControl[] pages = new UserControl[PageCount];
        public MainFrame()
        {
            InitializeComponent();
            pages[0] = mainPage;
			this.KeyDown +=new KeyEventHandler(MainFrame_KeyDown);
        }

        private void MainFrame_KeyDown(object sender, KeyEventArgs e)       //处理键盘事件响应
        {
            for (int i = 0; i < PageCount; i++)
                if (pages[i].Visibility == Visibility.Visible)
                {
                    ((IKeyDown)pages[i]).KeyboardDown(sender, e);
                    break;
                }
        }

        
    }
}
