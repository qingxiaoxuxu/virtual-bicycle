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
                if ((tx is TextBox) && ((TextBox)tx).Text == "" )
                {
                    MessageBox.Show("信息不完整！");
                    return;
                }
            }
                    
        }

    }
}
