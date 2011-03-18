namespace VbCsvHelper
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textSpeed = new System.Windows.Forms.TextBox();
            this.textSlope = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textHeight = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textTime = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textRank = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textPos = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(94, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "速度";
            // 
            // textSpeed
            // 
            this.textSpeed.Location = new System.Drawing.Point(139, 68);
            this.textSpeed.Name = "textSpeed";
            this.textSpeed.Size = new System.Drawing.Size(151, 21);
            this.textSpeed.TabIndex = 1;
            // 
            // textSlope
            // 
            this.textSlope.Location = new System.Drawing.Point(381, 68);
            this.textSlope.Name = "textSlope";
            this.textSlope.Size = new System.Drawing.Size(151, 21);
            this.textSlope.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(336, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "坡度";
            // 
            // textHeight
            // 
            this.textHeight.Location = new System.Drawing.Point(139, 122);
            this.textHeight.Name = "textHeight";
            this.textHeight.Size = new System.Drawing.Size(151, 21);
            this.textHeight.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(94, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "高度";
            // 
            // textTime
            // 
            this.textTime.Location = new System.Drawing.Point(139, 176);
            this.textTime.Name = "textTime";
            this.textTime.Size = new System.Drawing.Size(151, 21);
            this.textTime.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(94, 179);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "时间";
            // 
            // textRank
            // 
            this.textRank.Location = new System.Drawing.Point(381, 122);
            this.textRank.Name = "textRank";
            this.textRank.Size = new System.Drawing.Size(151, 21);
            this.textRank.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(336, 125);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "名次";
            // 
            // textPos
            // 
            this.textPos.Location = new System.Drawing.Point(139, 248);
            this.textPos.Name = "textPos";
            this.textPos.Size = new System.Drawing.Size(293, 21);
            this.textPos.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(52, 248);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "csv文件位置";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(464, 247);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(68, 21);
            this.buttonBrowse.TabIndex = 12;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(242, 300);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(123, 49);
            this.buttonAdd.TabIndex = 13;
            this.buttonAdd.Text = "添加记录";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 385);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textPos);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textRank);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textHeight);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textSlope);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textSpeed);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textSpeed;
        private System.Windows.Forms.TextBox textSlope;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textHeight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textRank;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textPos;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonAdd;
    }
}

