using NDEFReadWriteTool.bean;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NDEFReadWriteTool.View
{

    internal interface IReaderView
    {

      
        ConnectParam GetConnectParam();

        NdefInfo GetNdefInfo(int writeType);

        event EventHandler<bool> ConnectSwitchValueChange;

        event EventHandler RefreshButtonClick;

        event UIRadioButtonGroup.OnValueChanged RadioButtonChange;

        event EventHandler ReadURLButtonClick;

        event EventHandler WriteURLButtonClick;

        event EventHandler ReadTXTButtonClick;

        event EventHandler WriteTXTButtonClick;

        event EventHandler WriteWifiButtonClick;

        event EventHandler ReadWifiButtonClick;

        event EventHandler WriteBleButtonClick;

        event EventHandler ReadBleButtonClick;

        void showReaderVersion(ReaderVersion readerVersion);
        void controlProgressDialog(bool bOpen);
        void showTips(int type, string message);
        void showNdefInfo(NdefInfo info,int type);
    }
}
