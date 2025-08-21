using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool.Model
{
    internal interface ICsvService
    {
        Task CreateCsvFile();

        Task WriteCsvFile(NdefInfo ndefInfo);
    }
}
