using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Reflection;

namespace ProtocolDecoder
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            string dllPath = Path.Combine(Application.StartupPath, "Interop.DMCoreAutomation.dll");
            if (!File.Exists(dllPath))
            {
                Debug.WriteLine("create dll file");
                FileStream fs = new FileStream(dllPath, FileMode.CreateNew, FileAccess.Write);
                byte[] buffer = ProtocolDecoder.Properties.Resources.Interop_DMCoreAutomation;
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            DataTable datatable = TableOutputController.GetDataTable();
            datatable.Columns.Add(new DataColumn("Source Byte"));
            datatable.Columns.Add(new DataColumn("Decoded Result"));
            dataGridView1.DataSource = datatable;

            dataGridView1.Columns[0].Width = 120;
            dataGridView1.Columns[1].MinimumWidth = dataGridView1.Width - dataGridView1.Columns[0].Width - 3;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
        }

        private void buttonDecodeStk_Click(object sender, EventArgs e)
        {
            TableOutputController.Clear();

            byte[] bytes = Utils.ConvertInputToByteArray(textBoxInput.Text);

            if (bytes == null)
            {
                TableOutputController.Format("Please input valid hex string");
            }
            else
            {
                textBoxInput.Text = BitConverter.ToString(bytes).Replace("-", " ");
                CatDecoder.Decode(bytes);
            }
            dataGridView1.ClearSelection();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxInput.Clear();
            TableOutputController.Clear();
        }


        private void textBoxInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                //ctrl+a
                case (char)1:
                    textBoxInput.SelectAll();
                    e.Handled = true;      // 不再发出“噔”的声音
                    break;
                //enter键
                case '\r':
                    buttonDecodeStk_Click(null, null);
                    e.Handled = true;
                    break;
            }

        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Feel free to share!"
                           + "\nAuthor: Liu Meng（刘猛）"
                           + "\nE-Mail: veryliumeng@qq.com"
                           + "\nWebsite: http://blog.csdn.net/veryliumeng"
                           , "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBoxPath.Text = openFileDialog1.FileName;
            }

        }

        private void buttonMessage_Click(object sender, EventArgs e)
        {
            buttonApdu.Enabled = false;
            backgroundWorker1.RunWorkerAsync(new string[] { "msg", textBoxPath.Text });
        }


        private void buttonApdu_Click(object sender, EventArgs e)
        {
            buttonApdu.Enabled = false;
            backgroundWorker1.RunWorkerAsync(textBoxPath.Text);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = e.Argument.ToString();
            IsfDecoder.DecodeIsf(path, (BackgroundWorker)sender, checkBoxTime.Checked, checkBoxSummary.Checked, checkBoxOta.Checked, checkBoxMsg.Checked);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel2.Text = (string)e.UserState;
            if (e.ProgressPercentage == 0)
            {
                toolStripStatusLabel2.ForeColor = Color.Red;
            }
            else
            {
                toolStripStatusLabel2.ForeColor = Color.Green;
            }
        }

        private void backgroundWorker1_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonApdu.Enabled = true;
            //buttonMessage.Enabled = true;
        }


        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (buttonConnect.Text == "Connect" && comboBox1.SelectedItem != null)
            {
                serialPort1.PortName = comboBox1.SelectedItem.ToString();
                serialPort1.BaudRate = 115200;
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Parity = Parity.None;
                serialPort1.RtsEnable = true;
                serialPort1.DtrEnable = false;
                try
                {
                    serialPort1.Open();
                    buttonConnect.Text = "Disconnect";
                    comboBox1.Enabled = false;
                    buttonSend.Enabled = true;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception.ToString());
                }

            }
            else if (buttonConnect.Text == "Disconnect" && serialPort1.IsOpen)
            {
                serialPort1.Close();
                buttonSend.Enabled = false;
                buttonConnect.Text = "Connect";
                comboBox1.Enabled = true;
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.WriteLine(textBoxCommandLine.Text + "\r");
                textBoxCommandLine.AutoCompleteCustomSource.Insert(0, textBoxCommandLine.Text);

            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Debug.WriteLine("serialPort1_DataReceived");
            textBox3.Invoke(
                new MethodInvoker(
                    delegate
                    {
                        textBox3.AppendText(serialPort1.ReadExisting());
                    }
                    )
                );
        }

        private void textBoxCommandLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && buttonSend.Enabled == true)
            {
                buttonSend_Click(null, null);
            }
        }

        private void combox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage3)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(SerialPort.GetPortNames());
            }
        }
    }
}
