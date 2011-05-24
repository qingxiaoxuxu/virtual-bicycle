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

namespace Client_v2
{
    /// <summary>
    /// UserControl3_set.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl3_set : UserControl
    {
        public UserControl3_set()
        {
            InitializeComponent();
        }
     
        //转向灵敏度
        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            InfoControl.device.MyConfig.load = (Byte)slider3.Value;

            InfoControl.device.SendConfigData();

        }
        //设置速度灵敏度
        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            InfoControl.device.MyConfig.load = (Byte)slider3.Value;

            InfoControl.device.SendConfigData();
        }
        //设置阻尼
        private void slider3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            InfoControl.device.MyConfig.load = (Byte)slider3.Value;

            InfoControl.device.SendConfigData();
        }
        //转向灵敏度的默认值
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InfoControl.device.MyConfig.DirectionRate = 128;
            InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            InfoControl.device.MyConfig.load = (Byte)slider3.Value;

            InfoControl.device.SendConfigData();
        }
        //设置速度灵敏度的默认值
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            InfoControl.device.MyConfig.SpeedRate = 128;
            InfoControl.device.MyConfig.load = (Byte)slider3.Value;

            InfoControl.device.SendConfigData();
        }
        //设置阻尼大小的默认值
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            InfoControl.device.MyConfig.load = 128;

            InfoControl.device.SendConfigData();
        }
    }
}
