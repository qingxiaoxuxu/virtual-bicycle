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
	/// UserInfoBlock.xaml 的交互逻辑
	/// </summary>
	public partial class UserInfoBlock : UserControl
	{
		public UserInfoBlock()
		{
			this.InitializeComponent();
			SolidColorBrush solidBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			ReadyCanvas.Background = solidBrush;
				
		}
	}
}