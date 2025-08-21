using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool
{
    public delegate void VersionReturnDelegate(ReaderVersion readerVersion);
    public delegate void OnMessageReturnDelegate(NdefInfo value, int index);
    internal interface IReaderService
    {
        event OnMessageReturnDelegate OnInfoReturn;
        event VersionReturnDelegate OnVersionReturn;
        Task<bool> ReaderInitAsync(ConnectParam param,int tagType);
        Task<bool> GetReaderVersionAsync();
        Task<bool> SetReaderConfigAsync(int type);
        Task<bool> ReadNdefDataAsync(int ndefType);
        Task<bool> WriteNdefDataAsync(int ndefType, string ccData, string ndefData1,string ndefData2);
        void CloseReader(int type);

       
    }
}
