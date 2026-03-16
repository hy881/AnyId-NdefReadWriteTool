using NDEFReadWriteTool.bean;
using NDEFReadWriteTool.Infrastructure;
using NDEFReadWriteTool.Properties;
using NDEFReadWriteTool.View;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using 控件缩放算法;

namespace NDEFReadWriteTool
{
    public partial class Form1 : UIForm, IReaderView
    {
        private string _ccData;
        private string _ndefData;

        public Form1()
        {
            InitializeComponent();
            initUrlDataGridView();
            initTxtDataGridView();
            comb_connect_type.SelectedIndex = 0;
            uiRadioButtonGroup1.SelectedIndex = 0;
            this.uiRadioButtonGroup1.SelectedIndex = Properties.Settings.Default.TagType;
            this.uiPanel2.Text += "  V" + Application.ProductVersion.ToString();
            // switch_connect.ValueChanged +=(s,e)=> ConnectSwitchValueChange?.Invoke(this,e);
            btn_refresh.Click += (s, e) => RefreshButtonClick?.Invoke(this, e);
            uiRadioButtonGroup1.ValueChanged += (s, i, e) => RadioButtonChange?.Invoke(this, i, e);
        }

        public event Action<bool, ConnectParam> ConnectSwitchValueChange;


        public event EventHandler RefreshButtonClick;
        public event UIRadioButtonGroup.OnValueChanged RadioButtonChange;

        public event EventHandler SaveCsvButtonClick;


        public event Action ReadNdefDataEvent;
        public event WriteNDEFEventHandler WriteNdefDataEvent;
        public event EventHandler InitTagButtonClick;

        private void uiTabControlMenu1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        WindowZoomer zoomer;
        private void Form1_Load(object sender, EventArgs e)
        {
            //控件屏幕自适应
            zoomer = new WindowZoomer(this, false);
            comb_connect_type.SelectedIndex = Settings.Default.通信类型;
        }

        #region 读写器配置
        private void comb_connect_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comb_connect_type.SelectedIndex == 0)//USB
            {
                txt_param1.Visible = true;
                txt_param2.Visible = true;
                comb_com.Visible = false;
                combo_baudrate.Visible = false;
                lab_title1.Text = "PID:";
                lab_title2.Text = "VID";
                txt_param1.Text = "0x0505";
                txt_param2.Text = "0x0505";
                txt_param1.Enabled = false;
                txt_param2.Enabled = false;
            }
            else if (comb_connect_type.SelectedIndex == 1)//COM
            {
                txt_param1.Visible = false;
                txt_param2.Visible = false;
                comb_com.Enabled = true;
                combo_baudrate.Enabled = true;
                comb_com.Visible = true;
                combo_baudrate.Visible = true;
                lab_title1.Text = "串口号:";
                lab_title2.Text = "波特率:";
                //增加com口刷新
                comb_com.Items.Clear();
                combo_baudrate.Items.Clear();
                combo_baudrate.Items.AddRange(new string[] { "9600", "38400", "115200" });
                comb_com.Items.AddRange(SerialPort.GetPortNames());
                string save_comStr = Settings.Default.串口号;
                if (SerialPort.GetPortNames().ToList().IndexOf(save_comStr)>=0)
                {
                    comb_com.Text = save_comStr;
                }
                combo_baudrate.Text = Settings.Default.波特率;
            }
            else//TCP
            {
                txt_param1.Enabled = true;
                txt_param2.Enabled = true;
                txt_param1.Visible = true;
                txt_param2.Visible = true;
                txt_param1.Text = "";
                txt_param2.Text = "";
                comb_com.Visible = false;
                combo_baudrate.Visible = false;
                lab_title1.Text = "IP:";
                lab_title2.Text = "PORT:";
                txt_param1.Text = Settings.Default.IP;
                txt_param2.Text = Settings.Default.PORT;

            }
        }
        public ConnectParam GetConnectParam()
        {
            ConnectParam param = new ConnectParam();
            if (comb_connect_type.SelectedIndex == 0)
            {
                param.VID = 0x0505;
                param.PID = 0x5050;
                param.ConnectType = 0;
            }
            else if (comb_connect_type.SelectedIndex == 1)
            {
                param.ComStr = comb_com.Text;
                param.Baudrate = combo_baudrate.Text;
                param.ConnectType = 1;
            }
            else
            {
                param.IpStr = txt_param1.Text;
                param.Port = txt_param2.Text.ToInt();
                param.ConnectType = 2;
            }
            return param;
        }
        public void showReaderVersion(ReaderVersion readerVersion)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                this.txt_model.Text = readerVersion.Model;
                this.txt_softwave.Text = readerVersion.SoftwareVersion;
                this.txt_hardwave.Text = readerVersion.HardwareVersion;
            }));

        }


        #endregion

        #region 读写URL
        private void btn_write_url_Click(object sender, EventArgs e)
        {
            string url = txt_url.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                showTips(2, "url不能为空!");
                return;
            }
            WriteNdefDataEvent?.Invoke(0, new string[] { url });
        }

        public void showUrlInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_uid.Text = info.Uid;
                this.txt_url.Text = info.NdefData[0];
                urlList.Add(info);
                url_table.Rows.Add(urlList.Count, info.Uid, info.NdefData[0]);
                dgv_url.FirstDisplayedScrollingRowIndex = dgv_url.Rows.Count - 1;
            }));
        }
        List<NdefInfo> urlList = new List<NdefInfo>();
        DataTable url_table = new DataTable();
        private void initUrlDataGridView()
        {
            url_table.Columns.Add("序号", typeof(int));
            url_table.Columns.Add("UID", typeof(string));
            url_table.Columns.Add("URL", typeof(string));

            dgv_url.DataSource = url_table;
        }

        private void clearUrlDataGridView()
        {
            url_table.Rows.Clear();
            urlList.Clear();
        }

        private void btn_url_save_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Save a CSV File",
                OverwritePrompt = false // 不自动提示覆盖
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                // 检查文件是否已存在
                if (File.Exists(filePath))
                {
                    MessageBox.Show("文件已存在，不能覆盖！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // 文件已存在，终止操作
                }
                if (urlList.Count == 0)
                {
                    MessageBox.Show("无表单数据");
                    return;
                }
                this.ShowStatusForm(urlList.Count, "文件正在保存中" + "......", 0);
                CsvUtil csvUtil = new CsvUtil();
                csvUtil.setWriteDataList(urlList);
                csvUtil.startSaveCsvFile(filePath, value =>
                {
                    this.SetStatusFormDescription("进度：" + "(" + value + "%)......");
                    this.SetStatusFormStepIt();
                }, result =>
                {
                    this.HideStatusForm();
                    bool bResult = (bool)result;
                    if (bResult)
                    {
                        this.ShowSuccessDialog2("操作成功");
                        clearUrlDataGridView();
                    }
                    else
                    {
                        this.ShowErrorDialog2("操作失败");
                    }

                });

            }
        }

        private void btn_url_clear_Click(object sender, EventArgs e)
        {
            clearUrlDataGridView();
        }
        #endregion

        #region 读写文本
        private void btn_write_txt_Click(object sender, EventArgs e)
        {
            string txt = txt_txt.Text.Trim();
            if (string.IsNullOrEmpty(txt))
            {
                showTips(2, "文本不能为空!");
                return;
            }
            WriteNdefDataEvent?.Invoke(1, new string[] { txt });
        }
        public void showTxtInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_txt_uid.Text = info.Uid;
                this.txt_txt.Text = info.NdefData[0];
                txtList.Add(info);
                txt_table.Rows.Add(txtList.Count, info.Uid, info.NdefData[0]);
                dgv_txt.FirstDisplayedScrollingRowIndex = dgv_txt.Rows.Count - 1;
            }));
        }
        List<NdefInfo> txtList = new List<NdefInfo>();
        DataTable txt_table = new DataTable();
        private void initTxtDataGridView()
        {
            txt_table.Columns.Add("序号", typeof(int));
            txt_table.Columns.Add("UID", typeof(string));
            txt_table.Columns.Add("TXT", typeof(string));

            dgv_txt.DataSource = txt_table;
        }

        private void clearTxtDataGridView()
        {
            txt_table.Rows.Clear();
            txtList.Clear();
        }

        private void btn_txt_save_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Save a CSV File",
                OverwritePrompt = false // 不自动提示覆盖
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                // 检查文件是否已存在
                if (File.Exists(filePath))
                {
                    MessageBox.Show("文件已存在，不能覆盖！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // 文件已存在，终止操作
                }
                if (txtList.Count == 0)
                {
                    MessageBox.Show("无表单数据");
                    return;
                }
                this.ShowStatusForm(txtList.Count, "文件正在保存中" + "......", 0);
                CsvUtil csvUtil = new CsvUtil();
                csvUtil.setWriteDataList(txtList);
                csvUtil.startSaveCsvFile(filePath, value =>
                {
                    this.SetStatusFormDescription("进度：" + "(" + value + "%)......");
                    this.SetStatusFormStepIt();
                }, result =>
                {
                    this.HideStatusForm();
                    bool bResult = (bool)result;
                    if (bResult)
                    {
                        this.ShowSuccessDialog2("保存成功");
                        clearTxtDataGridView();
                    }
                    else
                    {
                        this.ShowErrorDialog2("保存失败");
                    }
                });

            }
        }

        private void btn_txt_clear_Click(object sender, EventArgs e)
        {
            clearTxtDataGridView();
        }
        #endregion

        #region 读写WIFI
        private void btn_write_wifi_Click(object sender, EventArgs e)
        {
            string wifiName = txt_wifiName.Text.Trim();
            string wifiPassword = txt_wifiPassword.Text.Trim();
            if (string.IsNullOrEmpty(wifiName) || string.IsNullOrEmpty(wifiPassword))
            {
                showTips(2, "输入不能为空!");
                return;
            }
            WriteNdefDataEvent?.Invoke(2, new string[] { wifiName, wifiPassword });
        }
        public void showWifiInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                string[] ndefStr = info.NdefData;
                if (ndefStr.Length >= 2)
                {
                    this.txt_wifiName.Text = ndefStr[0];
                    this.txt_wifiPassword.Text = ndefStr[1];
                }
                this.txt_wifi_uid.Text = info.Uid;
            }));
        }
        #endregion

        #region 读写蓝牙
        private void btn_write_ble_Click(object sender, EventArgs e)
        {
            string mac = txt_mac.Text.Trim();
            if (string.IsNullOrEmpty(mac))
            {
                showTips(2, "mac不能为空!");
                return;
            }
            if (!TranfUtil.IsMacAddress(mac))
            {
                showTips(2, "MAC地址格式输入错误");
                return;
            }
            WriteNdefDataEvent?.Invoke(3, new string[] { mac });
        }
        public void showBleInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_ble_uid.Text = info.Uid;
                this.txt_mac.Text = info.NdefData[0];
            }));
        }
        #endregion

        #region 读写小程序
        private void btn_write_app_Click(object sender, EventArgs e)
        {
            string packageName = txt_app_package.Text.Trim();
            string urlScheme = txt_app_urlscheme.Text.Trim();
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(urlScheme))
            {
                showTips(2, "输入不能为空!");
                return;
            }
            WriteNdefDataEvent?.Invoke(4, new string[] { urlScheme, packageName });
        }

        public void showAppInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_app_uid.Text = info.Uid;
                this.txt_app_urlscheme.Text = info.NdefData[0];
                this.txt_app_package.Text = info.NdefData[1];
            }));
        }
        #endregion

        public void controlProgressDialog(bool bOpen)
        {
            if (bOpen)
            {
                this.ShowProcessForm(100);
            }
            else
            {
                this.HideProcessForm();
            }
        }

        public void showTips(int type, string message)
        {
            if (type == 0)
            {
                this.ShowSuccessTip(message);
            }
            else if (type == 1)
            {
                this.ShowWarningTip(message);
            }
            else
            {
                this.ShowErrorTip(message);
            }
        }

        public void showNdefInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txt_read_uid.Text = info.Uid;
                txt_read_cc.Text = info.Cc;
                if (info.NdefData?.Length > 0)
                {
                    for (int i = 0; i < info.NdefData.Length; i++)
                    {
                        txt_read_ndef.Text += $"{info.NdefData[i]}\r\n";
                    }
                }
                else
                {
                    txt_read_ndef.Text = "";
                }


            });
        }

        private void btn_read_ndef_Click(object sender, EventArgs e)
        {
            ReadNdefDataEvent?.Invoke();
        }

        public NdefInfo GetNdefInfo(int writeType)
        {
            throw new NotImplementedException();
        }

        public void updateDataGridView(int type, NdefInfo info)
        {
            switch (type)
            {
                case 0:
                    showUrlInfo(info);
                    break;
                case 1:
                    showTxtInfo(info);
                    break;
                case 2:
                    showWifiInfo(info);
                    break;
                case 3:
                    showBleInfo(info);
                    break;
                case 4:
                    showAppInfo(info);
                    break;
            }

        }

        private void btn_init_tag_Click(object sender, EventArgs e)
        {
            InitTagButtonClick?.Invoke(this, e);
        }

        private bool bOpen = false;
        private void switch_connect_ValueChanged(object sender, bool value)
        {
           
        }              
        
        public void ConnectViewEnable(bool enable)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                bOpen = enable;
                switch_connect.Active = enable;
                comb_connect_type.Enabled = !enable;
                txt_param1.Enabled = !enable && comb_connect_type.SelectedIndex == 2;
                txt_param2.Enabled = !enable && comb_connect_type.SelectedIndex == 2;
                comb_com.Enabled = !enable && comb_connect_type.SelectedIndex == 1;
                combo_baudrate.Enabled = !enable && comb_connect_type.SelectedIndex == 1;
                btn_refresh.Enabled = enable;
                uiRadioButtonGroup1.Enabled = enable;
                Settings.Default.通信类型 = comb_connect_type.SelectedIndex;
                if (comb_connect_type.SelectedIndex==1)
                {
                    Settings.Default.串口号 = comb_com.Text;
                    Settings.Default.波特率 = combo_baudrate.Text;
                }else if (comb_connect_type.SelectedIndex == 2)
                {
                    Settings.Default.IP = txt_param1.Text;
                    Settings.Default.PORT = txt_param2.Text;
                }
                Settings.Default.Save();
            }));
        }

        private void switch_connect_Click(object sender, EventArgs e)
        {
            ConnectParam param = new ConnectParam();
            if (!bOpen)
            {
                if (comb_connect_type.SelectedIndex == 0)
                {
                    param.VID = 0x0505;
                    param.PID = 0x5050;
                    param.ConnectType = 0;
                }
                else if (comb_connect_type.SelectedIndex == 1)
                {
                    if (string.IsNullOrEmpty(comb_com.Text) || string.IsNullOrEmpty(combo_baudrate.Text))
                    {
                        MessageBox.Show("通信参数不能为空");
                        switch_connect.Active = false;
                        return;
                    }
                    param.ComStr = comb_com.Text;
                    param.Baudrate = combo_baudrate.Text;
                    param.ConnectType = 1;
                }
                else
                {
                    if (string.IsNullOrEmpty(txt_param1.Text) || string.IsNullOrEmpty(txt_param2.Text))
                    {
                        MessageBox.Show("通信参数不能为空");
                        switch_connect.Active = false;
                        return;
                    }
                    param.IpStr = txt_param1.Text;
                    param.Port = txt_param2.Text.ToInt();
                    param.ConnectType = 2;
                }
                ConnectSwitchValueChange?.Invoke(true, param);
            }
            else
            {
                param.ConnectType = comb_connect_type.SelectedIndex;
                ConnectSwitchValueChange?.Invoke(false, param);
                //界面还原
            }
        }

        private void btn_contact_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.anyid.com.cn/lxwm");
        }

        private void btn_download_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.anyid.com.cn/xzzx");
        }
    }
}


