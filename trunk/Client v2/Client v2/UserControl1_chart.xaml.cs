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
using ChartFX.WPF.Controls;

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
            separatePanel();
        }

        private void separatePanel()
        {
            StackPanePanel sPPanel = new StackPanePanel();
            sPPanel.Orientation = StackPaneOrientation.Vertical;
            chartSpeed.PanesPanel = sPPanel;

            chartSpeed.AxesY.Clear();

            Axis axisYSpeed = new Axis();
            Axis axisYHeart = new Axis();

            axisYSpeed.Max = new DataUnit(30);
            axisYSpeed.Min = new DataUnit(0);
            axisYSpeed.Step = new DataUnit(10);

            axisYHeart.Max = new DataUnit(180);
            axisYHeart.Min = new DataUnit(60);
            axisYHeart.Step = new DataUnit(40);

            chartSpeed.AxesY.Add(axisYSpeed);
            chartSpeed.AxesY.Add(axisYHeart);

            axisYSpeed.Title.Content = "当前速度";
            axisYHeart.Title.Content = "当前心率";

            chartSpeed.Series[0].AxisY = axisYSpeed;
            chartSpeed.Series[1].AxisY = axisYHeart;

            Pane paneSpeed = new Pane();
            Pane paneHeart = new Pane();
            paneSpeed.Series.Add(chartSpeed.Series[0]);
            paneHeart.Series.Add(chartSpeed.Series[1]);

            chartSpeed.Panes.Clear();

            chartSpeed.Panes.Add(paneSpeed);
            chartSpeed.Panes.Add(paneHeart);
        }

        public void UpdateChartInfo(List<ChartInfo> data)
        {
            displayData = new List<ChartInfo>(data);
            while (displayData.Count < MainWindow.MAX_POINT)
                displayData.Insert(0, new ChartInfo(-1, 0, 5, 80, 0, 0));
            chartSpeed.ItemsSource = displayData;
            chartSpeed.Refresh();
        }
    }


}
