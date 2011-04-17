using System;
using System.Collections.Generic;
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

namespace MyFirstWPF
{
	/// <summary>
	/// WaitingRoomPage.xaml 的交互逻辑
	/// </summary>
	public partial class WaitingRoomPage : UserControl, IKeyDown
	{
		public WaitingRoomPage()
		{
			this.InitializeComponent();
		}

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)
        {
            
        }

        public int Choose()
        {
            return -1;
        }

        public int MoveBack()
        {
            return MainFrame.INDEX_SELECT_ROOM_PAGE;
        }

        #endregion
    }
}