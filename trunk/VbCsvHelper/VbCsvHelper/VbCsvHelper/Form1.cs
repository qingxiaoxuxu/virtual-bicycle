using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VbCsvHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            foreach (Object tx in this.Controls)
            {
                if ((tx is TextBox) && ((TextBox)tx).Text == "")
                {
                    MessageBox.Show("信息不完整！");
                    return;
                }
            }
            try
            {
                CsvHelper csv = new CsvHelper();
                double speed = double.Parse(textSpeed.Text);
                double slope = double.Parse(textSlope.Text);
                double height = double.Parse(textHeight.Text);
                int rank = int.Parse(textRank.Text);
                string time = textTime.Text;
                string filePath = textPos.Text;
                #region 保存文件名修改
                int pos = filePath.LastIndexOf('.');
                string tmp = filePath.Substring(pos + 1);
                if (tmp != "csv")
                    filePath += ".csv";
                #endregion
                csv.CsvWriter(speed, slope, height, rank, time, filePath);
            }
            catch
            {
                MessageBox.Show("添加失败，请检查参数！");
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "csv逗号分隔文件(*.csv)|*.csv";
            if (of.ShowDialog() == DialogResult.OK)
                textPos.Text = of.FileName;
        }



    }
}
