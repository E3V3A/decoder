namespace ProtocolDecoder
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
            this.components = new System.ComponentModel.Container();
            this.buttonDecodeStk = new System.Windows.Forms.Button();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.buttonAbout = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.checkBoxMsg = new System.Windows.Forms.CheckBox();
            this.checkBoxSummary = new System.Windows.Forms.CheckBox();
            this.checkBoxTime = new System.Windows.Forms.CheckBox();
            this.buttonApdu = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.textBoxCommandLine = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonDecodeStk
            // 
            this.buttonDecodeStk.Location = new System.Drawing.Point(620, 29);
            this.buttonDecodeStk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonDecodeStk.Name = "buttonDecodeStk";
            this.buttonDecodeStk.Size = new System.Drawing.Size(94, 52);
            this.buttonDecodeStk.TabIndex = 0;
            this.buttonDecodeStk.Text = "Decode STK";
            this.buttonDecodeStk.UseVisualStyleBackColor = true;
            this.buttonDecodeStk.Click += new System.EventHandler(this.buttonDecodeStk_Click);
            // 
            // textBoxInput
            // 
            this.textBoxInput.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxInput.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxInput.Location = new System.Drawing.Point(7, 29);
            this.textBoxInput.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxInput.Multiline = true;
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(579, 67);
            this.textBoxInput.TabIndex = 1;
            this.textBoxInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxInput_KeyPress);
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(620, 101);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(94, 29);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 21);
            this.label1.TabIndex = 4;
            this.label1.Text = "Input String";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 101);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 21);
            this.label2.TabIndex = 5;
            this.label2.Text = "Decode Result";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(7, 127);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.ReadOnly = true;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.Size = new System.Drawing.Size(579, 459);
            this.dataGridView1.TabIndex = 6;
            // 
            // buttonAbout
            // 
            this.buttonAbout.Location = new System.Drawing.Point(620, 194);
            this.buttonAbout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonAbout.Name = "buttonAbout";
            this.buttonAbout.Size = new System.Drawing.Size(94, 29);
            this.buttonAbout.TabIndex = 7;
            this.buttonAbout.Text = "About";
            this.buttonAbout.UseVisualStyleBackColor = true;
            this.buttonAbout.Click += new System.EventHandler(this.buttonAbout_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(620, 241);
            this.buttonExit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(94, 29);
            this.buttonExit.TabIndex = 8;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(1, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(737, 623);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.textBoxInput);
            this.tabPage1.Controls.Add(this.buttonExit);
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Controls.Add(this.buttonAbout);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.buttonClear);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.buttonDecodeStk);
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(729, 589);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Decoder";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.checkBoxMsg);
            this.tabPage2.Controls.Add(this.checkBoxSummary);
            this.tabPage2.Controls.Add(this.checkBoxTime);
            this.tabPage2.Controls.Add(this.buttonApdu);
            this.tabPage2.Controls.Add(this.statusStrip1);
            this.tabPage2.Controls.Add(this.textBoxPath);
            this.tabPage2.Controls.Add(this.buttonLoad);
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(729, 589);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "QXDM Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBoxMsg
            // 
            this.checkBoxMsg.AutoSize = true;
            this.checkBoxMsg.Location = new System.Drawing.Point(513, 84);
            this.checkBoxMsg.Name = "checkBoxMsg";
            this.checkBoxMsg.Size = new System.Drawing.Size(134, 25);
            this.checkBoxMsg.TabIndex = 10;
            this.checkBoxMsg.Text = "UIM Message";
            this.checkBoxMsg.UseVisualStyleBackColor = true;
            this.checkBoxMsg.CheckedChanged += new System.EventHandler(this.checkBoxMsg_CheckedChanged);
            // 
            // checkBoxSummary
            // 
            this.checkBoxSummary.AutoSize = true;
            this.checkBoxSummary.Checked = true;
            this.checkBoxSummary.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSummary.Location = new System.Drawing.Point(213, 84);
            this.checkBoxSummary.Name = "checkBoxSummary";
            this.checkBoxSummary.Size = new System.Drawing.Size(151, 25);
            this.checkBoxSummary.TabIndex = 8;
            this.checkBoxSummary.Text = "APDU Summary";
            this.checkBoxSummary.UseVisualStyleBackColor = true;
            // 
            // checkBoxTime
            // 
            this.checkBoxTime.AutoSize = true;
            this.checkBoxTime.Location = new System.Drawing.Point(367, 84);
            this.checkBoxTime.Name = "checkBoxTime";
            this.checkBoxTime.Size = new System.Drawing.Size(125, 25);
            this.checkBoxTime.TabIndex = 7;
            this.checkBoxTime.Text = "Use PC Time";
            this.checkBoxTime.UseVisualStyleBackColor = true;
            // 
            // buttonApdu
            // 
            this.buttonApdu.Location = new System.Drawing.Point(11, 80);
            this.buttonApdu.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonApdu.Name = "buttonApdu";
            this.buttonApdu.Size = new System.Drawing.Size(123, 29);
            this.buttonApdu.TabIndex = 4;
            this.buttonApdu.Text = "Extract APDU";
            this.buttonApdu.UseVisualStyleBackColor = true;
            this.buttonApdu.Click += new System.EventHandler(this.buttonApdu_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(3, 564);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(723, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(0, 17);
            // 
            // textBoxPath
            // 
            this.textBoxPath.AutoCompleteCustomSource.AddRange(new string[] {
            "*.isf"});
            this.textBoxPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.textBoxPath.Location = new System.Drawing.Point(112, 20);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(601, 29);
            this.textBoxPath.TabIndex = 2;
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(11, 19);
            this.buttonLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(94, 29);
            this.buttonLoad.TabIndex = 1;
            this.buttonLoad.Text = "Load ISF";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.buttonConnect);
            this.tabPage3.Controls.Add(this.comboBox1);
            this.tabPage3.Controls.Add(this.textBoxCommandLine);
            this.tabPage3.Controls.Add(this.textBox3);
            this.tabPage3.Controls.Add(this.buttonSend);
            this.tabPage3.Location = new System.Drawing.Point(4, 30);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(729, 589);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "AT Command";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(591, 66);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(103, 31);
            this.buttonConnect.TabIndex = 5;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(591, 19);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(103, 29);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.combox1_MouseClick);
            // 
            // textBoxCommandLine
            // 
            this.textBoxCommandLine.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxCommandLine.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxCommandLine.Location = new System.Drawing.Point(19, 421);
            this.textBoxCommandLine.Name = "textBoxCommandLine";
            this.textBoxCommandLine.Size = new System.Drawing.Size(522, 29);
            this.textBoxCommandLine.TabIndex = 2;
            this.textBoxCommandLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxCommandLine_KeyDown);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(19, 19);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox3.Size = new System.Drawing.Size(522, 369);
            this.textBox3.TabIndex = 1;
            // 
            // buttonSend
            // 
            this.buttonSend.Enabled = false;
            this.buttonSend.Location = new System.Drawing.Point(591, 421);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(103, 31);
            this.buttonSend.TabIndex = 0;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Log Files(*.isf)|*.isf";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_Completed);
            // 
            // serialPort1
            // 
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(737, 621);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Protocol Decoder";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonDecodeStk;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button buttonAbout;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonApdu;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.TextBox textBoxCommandLine;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.CheckBox checkBoxTime;
        private System.Windows.Forms.CheckBox checkBoxSummary;
        private System.Windows.Forms.CheckBox checkBoxMsg;
    }
}

