using NDEFReadWriteTool.bean;
using NDEFReadWriteTool.Properties;
using NDEFReadWriteTool.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool
{
    internal class ReaderPersenter
    {
        private readonly IReaderService _readerService;
        private readonly IReaderView _readerView;

        public ReaderPersenter(IReaderService readerService, IReaderView readerView)
        {
            _readerService = readerService;
            this._readerView = readerView;
            this._readerView.ConnectSwitchValueChange += connectSwitchClick;
            _readerService.onReaderVersionReturn += readerVersionReturn;

            _readerView.RefreshButtonClick += refreshBtnClick;
            _readerView.RadioButtonChange += tagTypeChange;

            _readerView.ReadNdefDataEvent += readNdefData;
            _readerView.WriteNdefDataEvent += writeNdefData;
            _readerView.InitTagButtonClick += initNdefTag;
        }

        private  async void connectSwitchClick(bool value,ConnectParam connectParam)
        {
            //view需要弹出进度dialog
            _readerView.controlProgressDialog(true);
            if (value)
            {           
                bool result = await _readerService.ReaderInitAsync(connectParam, Properties.Settings.Default.TagType);
                if (result)
                {
                    _readerView.showTips(0, "设备连接成功");
                    _readerView.ConnectViewEnable(true);
                }
                else
                {
                    _readerView.showTips(2, "设备连接失败");
                    _readerView.ConnectViewEnable(false);
                }
            }
            else
            {
                _readerService.CloseReader(connectParam.ConnectType);
                _readerView.showTips(0, "设备关闭成功");
                _readerView.ConnectViewEnable(false);
            }
            //view关闭进度dialog
            _readerView.controlProgressDialog(false);
        }

        private async void refreshBtnClick(object e,EventArgs args)
        {
            bool result = await  _readerService.GetReaderVersionAsync();
            if (result)
            {
                _readerView.showTips(0, "版本获取成功");
            }
            else
            {
                _readerView.showTips(2, "通信超时");
            }
        }

        private void readerVersionReturn(ReaderVersion readerVersion)
        {
            _readerView.showReaderVersion(readerVersion);
        }

        private async void tagTypeChange(object obj,int index,string txt)
        {
            _readerView.controlProgressDialog(true);
            bool result = await _readerService.SetReaderConfigAsync(index);
            if (result)
            {
                _readerView.showTips(0, "协议选择成功");
                Properties.Settings.Default.TagType = index;
                Properties.Settings.Default.Save();
            }
            else
            {
                _readerView.showTips(2, "通信超时");
            }
            _readerView.controlProgressDialog(false);
        }

        private async void initNdefTag(object e, EventArgs args)
        {
            try
            {
                OperationResult uidRlt = await _readerService.GetTagUidAsync();
                if (!uidRlt.IsSuccess)
                {
                    _readerView.showTips(2, uidRlt.ErrorMessage);
                    return;
                }
                OperationResult ccRlt = await _readerService.InitTagAsync(144);
                if (!ccRlt.IsSuccess)
                {
                    _readerView.showTips(2, ccRlt.ErrorMessage);
                    return;
                }
                _readerView.showTips(0, "初始化成功");
            }
            catch (Exception)
            {

                _readerView.showTips(2, "初始化失败");
            }
        }
        private async void readNdefData()
        {
            try
            {
                NdefInfo ndefInfo = new NdefInfo();
                //读UID
                OperationResult uidRlt = await _readerService.GetTagUidAsync();
                if (!uidRlt.IsSuccess)
                {
                    _readerView.showTips(2, uidRlt.ErrorMessage);
                    return;
                }
                ndefInfo.Uid = uidRlt.Data.ToString();
                _readerView.showNdefInfo(ndefInfo);
                //读CCdata
                OperationResult ccRlt = await _readerService.GetTagCcDataAsync();
                if (!ccRlt.IsSuccess)
                {
                    _readerView.showTips(2, ccRlt.ErrorMessage);
                    return;
                }
                ndefInfo.Cc = ccRlt.Data.ToString();
                _readerView.showNdefInfo(ndefInfo);
                //读NDEF
                OperationResult ndefRlt = await _readerService.ReadNdefDataAsync();
                if (!ndefRlt.IsSuccess)
                {
                    _readerView.showTips(2, ndefRlt.ErrorMessage);
                    return;
                }

                ndefInfo.NdefData = ndefRlt.Data.ToString().Split('#');
                _readerView.showNdefInfo(ndefInfo);
                _readerView.showTips(0, "读取成功");
            }
            catch (Exception)
            {
                _readerView.showTips(2, "读取失败");
            }
          
        }

        private async void writeNdefData(int ndefType,string[] ndefList)
        {
            NdefInfo ndefInfo = new NdefInfo();
            try
            {
                //读UID
                OperationResult uidRlt = await _readerService.GetTagUidAsync();
                if (!uidRlt.IsSuccess)
                {
                    _readerView.showTips(2, uidRlt.ErrorMessage);
                    return;
                }
                ndefInfo.Uid = uidRlt.Data.ToString();
                //读CCdata
                OperationResult ccRlt = await _readerService.GetTagCcDataAsync();
                if (!ccRlt.IsSuccess)
                {
                    _readerView.showTips(2, ccRlt.ErrorMessage);
                    return;
                }
                if (ccRlt.Data.ToString().Length==0)
                {
                    int len = 0;
                    for (int i = 0; i < ndefList.Length; i++)
                    {
                        len += ndefList[i].Length;
                    }
                    OperationResult writeCcRlt = await _readerService.InitTagAsync(len);
                    if (!writeCcRlt.IsSuccess)
                    {
                        _readerView.showTips(2, writeCcRlt.ErrorMessage);
                        return;
                    }
                }
                ndefInfo.Cc = ccRlt.Data.ToString();
                OperationResult writeRlt = await _readerService.WriteNdefDataAsync(ndefType,ndefList);
                if (!writeRlt.IsSuccess)
                {
                    _readerView.showTips(2, writeRlt.ErrorMessage);
                    return;
                }
                ndefInfo.NdefData = ndefList;
                _readerView.showTips(0, "写入成功");
                _readerView.updateDataGridView(ndefType, ndefInfo);
            }
            catch (Exception ex)
            {
                _readerView.showTips(2, "写入失败");
            }
        }


    }
}
