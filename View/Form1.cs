using NDEFReadWriteTool.bean;
using NDEFReadWriteTool.Infrastructure;
using NDEFReadWriteTool.View;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            switch_connect.ValueChanged +=(s,e)=> ConnectSwitchValueChange?.Invoke(this,e);
            btn_refresh.Click += (s, e) => RefreshButtonClick?.Invoke(this,e);
            uiRadioButtonGroup1.ValueChanged+=(s,i,e)=>RadioButtonChange?.Invoke(this,i,e);
            btn_url_read.Click += (s, e) => ReadURLButtonClick?.Invoke(this, e);
            btn_url_write.Click += (s, e) => WriteURLButtonClick?.Invoke(this, e);
            btn_txt_read.Click += (s, e) => ReadTXTButtonClick?.Invoke(this, e);
            btn_txt_write.Click += (s, e) => WriteTXTButtonClick?.Invoke(this, e);
            btn_wifi_read.Click += (s, e) => ReadWifiButtonClick?.Invoke(this, e);
            btn_wifi_write.Click+=(s, e) => WriteWifiButtonClick?.Invoke(this, e);
            btn_ble_write.Click += (s, e) => WriteBleButtonClick?.Invoke(this, e);
            btn_ble_read.Click += (s, e) => ReadBleButtonClick?.Invoke(this, e);
        }

        public event EventHandler<bool> ConnectSwitchValueChange;
        public event EventHandler RefreshButtonClick;
        public event UIRadioButtonGroup.OnValueChanged RadioButtonChange;
        public event EventHandler ReadURLButtonClick;
        public event EventHandler WriteURLButtonClick;
        public event EventHandler SaveCsvButtonClick;
       
        public event EventHandler ReadTXTButtonClick;
        public event EventHandler WriteTXTButtonClick;

        public event EventHandler WriteWifiButtonClick;
        public event EventHandler WriteBleButtonClick;
        public event EventHandler ReadWifiButtonClick;
        public event EventHandler ReadBleButtonClick;

        private void uiTabControlMenu1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
                comb_com.Visible = true;
                combo_baudrate.Visible = true;
                lab_title1.Text = "串口号:";
                lab_title2.Text = "波特率:";
                //增加com口刷新
            }
            else//TCP
            {
                txt_param1.Enabled = true;
                txt_param2.Enabled = true;
                txt_param1.Visible = true;
                txt_param2.Visible = true;
                comb_com.Visible = false;
                combo_baudrate.Visible = false;
                lab_title1.Text = "IP:";
                lab_title2.Text = "PORT:";
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
        public void showUrlInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_uid.Text = info.Uid;
                this.txt_cc.Text = info.Cc;
                this.txt_url.Text = info.NdefData;
                urlList.Add(info);
                url_table.Rows.Add(urlList.Count, info.Uid, info.NdefData);
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
                if (urlList.Count==0)
                {
                    MessageBox.Show("无表单数据");
                    return;
                }
                this.ShowStatusForm(urlList.Count, "文件正在保存中"+ "......", 0);
                CsvUtil csvUtil = new CsvUtil();
                csvUtil.setWriteDataList(urlList);
                csvUtil.startSaveCsvFile(filePath, value =>
                {
                    this.SetStatusFormDescription("进度："+ "(" + value + "%)......");
                    this.SetStatusFormStepIt();
                }, result =>
                {
                    this.HideStatusForm();
                    bool bResult=(bool)result;
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
        public void showTxtInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_txt_uid.Text = info.Uid;
                this.txt_txt_cc.Text = info.Cc;
                this.txt_txt.Text = info.NdefData;
                txtList.Add(info);
                txt_table.Rows.Add(txtList.Count, info.Uid, info.NdefData);
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
        public void showWifiInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                string[] ndefStr=info.NdefData.Split('#');
                if (ndefStr.Length>=2)
                {
                    this.txt_wifiName.Text = ndefStr[0];
                    this.txt_wifiPassword.Text = ndefStr[1];
                }          
                this.txt_wifi_uid.Text= info.Uid;
                this.txt_wifi_cc.Text = info.Cc;
            }));
        }
        #endregion

        #region 读写蓝牙
        public void showBleInfo(NdefInfo info)
        {
            this.Invoke((MethodInvoker)(() => {
                this.txt_ble_uid.Text = info.Uid;
                this.txt_ble_cc.Text = info.Cc;
                this.txt_mac.Text = info.NdefData;
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

        public NdefInfo GetNdefInfo(int writeType)
        {
            NdefInfo info = new NdefInfo();
            switch (writeType)
            {
                case 0:
                    info.Cc = this.txt_cc.Text;
                    info.NdefData = this.txt_url.Text;
                    break;
                case 1:
                    info.Cc = this.txt_txt_cc.Text;
                    info.NdefData = this.txt_txt.Text;
                    break;
                case 2:
                    info.Cc = this.txt_cc.Text;
                    info.NdefData=this.txt_wifiName.Text;
                    info.NdefData2=this.txt_wifiPassword.Text;
                    break;
                case 3:
                    info.Cc = this.txt_ble_cc.Text;
                    info.NdefData=this.txt_mac.Text;
                    break;
            }
            return info;
        }

        public void showNdefInfo(NdefInfo info, int type)
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
            }
        }
    }
}
