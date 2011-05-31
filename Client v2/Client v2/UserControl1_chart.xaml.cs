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
using ChartFX.WPF;
using Client_v2.Model;

namespace Client_v2
{
    /// <summary>
    /// UserControl1_chart.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1_chart : UserControl
    {
        List<ChartInfo> displayData;

        public UserControl1_chart()
        {
            InitializeComponent();
        }

        public void UpdateChartInfo(List<ChartInfo> data)
        {
            displayData = new List<ChartInfo>(data);
            while (displayData.Count < MainWindow.MAX_POINT)
                displayData.Insert(0, new ChartInfo(-1, 0, 0, 0, 0, 0));
            chartSpeed.ItemsSource = displayData;
            chartSpeed.Refresh();
        }
    }


}
