using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool
{

    internal interface IReaderService
    {
        event Action<ReaderVersion> onReaderVersionReturn;
        Task<bool> ReaderInitAsync(ConnectParam param,int tagType);
        Task<bool> GetReaderVersionAsync();
        Task<bool> SetReaderConfigAsync(int type);
        Task<OperationResult> GetTagUidAsync();
        Task<OperationResult> GetTagCcDataAsync();
        Task<OperationResult> ReadNdefDataAsync();
        Task<OperationResult> InitTagAsync(int len);
        Task<OperationResult> WriteNdefDataAsync(int ndefType, string[] ndefList);
        void CloseReader(int type);

       
    }
}
