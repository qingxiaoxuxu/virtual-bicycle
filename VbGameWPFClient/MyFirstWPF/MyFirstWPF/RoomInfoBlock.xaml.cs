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
	/// RoomInfoBlockxaml.xaml 的交互逻辑
	/// </summary>
	public partial class RoomInfoBlock : UserControl
	{
		public RoomInfoBlock()
		{
			this.InitializeComponent();
		}

        public string GetRoomName()
        {
            return RoomName.Content.ToString();
        }

		public RoomInfoBlock(string teamName, string mapName, int people, int max)
		{
            this.InitializeComponent();
            RoomName.Content = teamName;
            RoomMap.Content = mapName;
            string str = people.ToString() + "/" + max.ToString();
            PeopleLabel.Content = str;
		}
	}
}