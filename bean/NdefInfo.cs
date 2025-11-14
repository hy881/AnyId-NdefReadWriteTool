using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool.bean
{
    public class NdefInfo
    {
        public NdefInfo()
        {
        }

        public NdefInfo(string uid, string cc, string[] ndefData)
        {
            Uid = uid;
            Cc = cc;
            NdefData = ndefData;
        }


        public string Uid { get; set; }
        public string Cc { get; set; }
        public string[] NdefData { get; set; }
    }
}
