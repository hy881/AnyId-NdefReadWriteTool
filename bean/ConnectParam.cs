using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool.bean
{
    public class ConnectParam
    {
        public ConnectParam()
        {
        }

        public ConnectParam(string ipStr, int port, int connectType)
        {
            IpStr = ipStr;
            Port = port;
            ConnectType = connectType;
        }

        public ConnectParam(string comStr, string baudrate, int connectType)
        {
            ComStr = comStr;
            Baudrate = baudrate;
            ConnectType = connectType;
        }

        public ConnectParam(int pID, int vID, int connectType)
        {
            PID = pID;
            VID = vID;
            ConnectType = connectType;
        }

        public string IpStr { get; set; }
        public int Port { get; set; }
        public string ComStr { get; set; }
        public string Baudrate { get; set; }
        public int PID { get; set; }
        public int VID { get; set; }
        public int ConnectType { get; set; }

    }
}
