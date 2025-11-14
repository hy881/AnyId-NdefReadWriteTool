using NDEFReadWriteTool.bean;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NDEFReadWriteTool.View
{

    public delegate void WriteNDEFEventHandler(int ndefType, string[] ndefList);
    internal interface IReaderView
    {      

        NdefInfo GetNdefInfo(int writeType);

        event Action<bool,ConnectParam> ConnectSwitchValueChange;

        event EventHandler RefreshButtonClick;

        event EventHandler InitTagButtonClick;

        event UIRadioButtonGroup.OnValueChanged RadioButtonChange;

        event Action ReadNdefDataEvent;

        event WriteNDEFEventHandler WriteNdefDataEvent;

        void showReaderVersion(ReaderVersion readerVersion);
        void controlProgressDialog(bool bOpen);
        void showTips(int type, string message);
        void showNdefInfo(NdefInfo info);
        void updateDataGridView(int type,NdefInfo info);
        void ConnectViewEnable(bool enable);
    }
}
