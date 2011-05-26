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
            slider1.Value = 128;
            slider2.Value = 128;
            slider3.Value = 128;
            bt_setting.Text = "--单击此处--\r\n开始校准转向";
        }

        private const byte BEGIN_SETTING = 0x00;
        private const byte LEFT_POINT = 0x01;
        private const byte CENTER_POINT = 0x02;
        private const byte RIGHT_POINT = 0x03;
        private byte Step = 0x00;

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
            //InfoControl.device.MyConfig.DirectionRate = 128;
            //InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            //InfoControl.device.MyConfig.load = (Byte)slider3.Value;
            //InfoControl.device.SendConfigData();
            slider1.Value = 128;
        }
        //设置速度灵敏度的默认值
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            //InfoControl.device.MyConfig.SpeedRate = 128;
            //InfoControl.device.MyConfig.load = (Byte)slider3.Value;
            //InfoControl.device.SendConfigData();
            slider2.Value = 128;
        }
        //设置阻尼大小的默认值
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //InfoControl.device.MyConfig.DirectionRate = (Byte)slider1.Value;
            //InfoControl.device.MyConfig.SpeedRate = (Byte)slider2.Value;
            //InfoControl.device.MyConfig.load = 128;
            //InfoControl.device.SendConfigData();
            slider3.Value = 128;
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            switch (Step)
            {
                case BEGIN_SETTING:
                    {
                        InfoControl.device.SendSettingData(BEGIN_SETTING);
                        bt_setting.Text = "请转到左极限位置\r\n然后单击此按键";
                        Step = LEFT_POINT;
                        break;
                    }
                case LEFT_POINT:
                    {
                        InfoControl.device.SendSettingData(LEFT_POINT);
                        bt_setting.Text = "请转到中间位置\r\n然后单击此按键";
                        Step = CENTER_POINT;
                        break;

                    }
                case CENTER_POINT:
                    {
                        InfoControl.device.SendSettingData(CENTER_POINT);
                        bt_setting.Text = "请转到右极限位置\r\n然后单击此按键";
                        Step = RIGHT_POINT;

                        break;
                    }
                case RIGHT_POINT:
                    {
                        InfoControl.device.SendSettingData(RIGHT_POINT);
                        MessageBox.Show("转向校准完成！");
                        bt_setting.Text = "--单击此处--\r\n开始校准转向";
                        Step = BEGIN_SETTING;

                        break;
                    }
                default:
                    {
                        break;
                    }




            }
        }
    }
}
