using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool
{
    static class AnyIDReader
    {
        public delegate void onSuccess(Object obj);
        public delegate void onFail(string msg);
        private static int hSerial = -1;
        private static byte[] uid_15693 = new byte[8];
        private static byte[] uid_14443A=new byte[10];
        public const int TAGTYPE_ISO15693 = 0x00;
        public const int TAGTYPE_NTAG = 0x01;

        private static string[] NDEF_URI_PREFIXS = {
                                        "",
                                        "http://www.",
                                        "https://www.",
                                        "http://",
                                        "https://",
                                        "tel:",
                                        "mailto:",
                                        "ftp://anonymous:anonymous@",
                                        "ftp://ftp.",
                                        "ftps://",
                                        "sftp://",
                                        "smb://",
                                        "nfs://",
                                        "ftp://",
                                        "dav://",
                                        "news:",
                                        "telnet://",
                                        "imap:",
                                        "rtsp://",
                                        "urn:",
                                        "pop:",
                                        "sip:",
                                        "sips:",
                                        "tftp:",
                                        "btspp://",
                                        "btl2cap://",
                                        "btgoep://",
                                        "tcpobex://",
                                        "irdaobex://",
                                        "file://",
                                        "urn:epc:id:",
                                        "urn:epc:tag:",
                                        "urn:epc:pat:",
                                        "urn:epc:raw:",
                                        "urn:epc:",
                                        "urn:nfc:"
        };

        public static void initReader(onSuccess fun1, onFail fun2)
        {
            int hHFREADERDLLModule = 0;
            hHFREADERDLLModule = hfReaderDll.LoadLibrary("HFREADER.dll");
            if (hHFREADERDLLModule > 0)
            {
                fun1("初始化设备成功");
            }
            else
            {
                fun2("缺少文件HFREADER.dll");
            }
        }

        public static void openReader(onSuccess fun1, onFail fun2)
        {
            hSerial = hfReaderDll.hfReaderOpenUsb(0x0505, 0x5050);
            if (hSerial > 0)
            {
                fun1("设备连接成功");
            }
            else
            {
                fun2("设备连接失败");
            }
        }

        public static void getReaderVersion(onSuccess fun1, onFail fun2)
        {
            Byte[] buffer = new Byte[255];
            ushort[] addrArray = new ushort[2];
            HFREADER_VERSION pVersion = new HFREADER_VERSION();
            pVersion.type = new byte[hfReaderDll.HFREADER_VERSION_SIZE];
            pVersion.sv = new byte[hfReaderDll.HFREADER_VERSION_SIZE];
            pVersion.hv = new byte[hfReaderDll.HFREADER_VERSION_SIZE];
            int rlt = hfReaderDll.hfReaderGetVersion(hSerial, 0x0000, 0xFFFF, ref pVersion, null, null);
            if (rlt > 0)
            {
                if (pVersion.result.flag == 0)
                {
                    ReaderVersion readerVersion=new ReaderVersion();
                    readerVersion.Model = System.Text.Encoding.Default.GetString(pVersion.type).Replace("\0", "");
                    readerVersion.HardwareVersion = System.Text.Encoding.Default.GetString(pVersion.hv).Replace("\0", "");
                    readerVersion.SoftwareVersion = System.Text.Encoding.Default.GetString(pVersion.sv).Replace("\0", "");
                    fun1(readerVersion);
                }
            }
            else
            {
                fun2("获取版本失败");
            }

        }
        /// <summary>
        /// 配置读写器参数
        /// </summary>
        /// <param name="tagType"></param>
        /// <param name="fun1"></param>
        /// <param name="fun2"></param>
        public static void setReaderConfig(int tagType, onSuccess fun1, onFail fun2)
        {
            ushort[] addrArray = { 0x0000, 0x0001 };
            Byte WorkMode = 0;
            HFREADER_CONFIG pReaderConfig = new HFREADER_CONFIG();
            if (tagType == TAGTYPE_ISO15693)
            {
                WorkMode |= hfReaderDll.HFREADER_CFG_TYPE_ISO15693 | hfReaderDll.HFREADER_CFG_WM_INVENTORY;
            }
            else
            {
                WorkMode |= hfReaderDll.HFREADER_CFG_TYPE_ISO14443A | hfReaderDll.HFREADER_CFG_WM_INVENTORY;
            }
            pReaderConfig.workMode = WorkMode;
            pReaderConfig.cmdMode = hfReaderDll.HFREADER_CFG_INVENTORY_TRIGGER;
            pReaderConfig.uidSendMode = hfReaderDll.HFREADER_CFG_UID_POSITIVE;
            pReaderConfig.beepStatus = hfReaderDll.HFREADER_CFG_BUZZER_DISABLE;
            pReaderConfig.afiCtrl = hfReaderDll.HFREADER_CFG_AFI_DISABLE;
            pReaderConfig.tagStatus = hfReaderDll.HFREADER_CFG_TAG_NOQUIET;
            pReaderConfig.baudrate = hfReaderDll.HFREADER_CFG_BAUDRATE38400;
            pReaderConfig.afi = 0x00;
            pReaderConfig.readerAddr = (ushort)(addrArray[1]);
            int rlt = hfReaderDll.hfReaderSetConfig(hSerial, 0, 1, ref pReaderConfig, null, null);
            if (rlt > 0)
            {
                fun1("设置设备成功");
            }
            else
            {
                fun2("设置设备失败");
            }
        }


        public static void resetRf()
        {
            HFREADER_OPRESULT pResult = new HFREADER_OPRESULT();
            hfReaderDll.hfReaderCtrlRf(hSerial, 0, 1, 2, ref pResult, null, null);
        }

        /// <summary>
        /// 文本转txtNDEF
        /// </summary>
        /// <param name="txtMsg">输入文本</param>
        /// <returns>返回转换的byte数组</returns>
        private static byte[] formatTxtNdef(string ccData,string txtMsg)
        {
            int index = 0;
            byte[] block = new byte[1024];
            byte[] cc=TranfUtil.strToHexByte(ccData);
            Array.Copy(cc, 0, block, index, cc.Length);
            index += cc.Length;
            block[index++] = 0x03;
            block[index++] = (byte)(7 + txtMsg.Length);//L
            block[index++] = 0xD1;//Ndef StatusByte
            block[index++] = 0x01;//Type_Length
            block[index++] = (byte)(txtMsg.Length + 3);//Payload_Length
            block[index++] = 0x54;//Type
            block[index++] = 0x02;//缩写
            block[index++] = 0x65;//'e'
            block[index++] = 0x6E;//'n'
            byte[] data= System.Text.Encoding.ASCII.GetBytes(txtMsg);
            Array.Copy(data, 0, block, index, data.Length);
            index+=data.Length;
           // block[index++] = 0xFE;
            byte[] buffer = new byte[index];
            Array.Copy(block, 0, buffer, 0, index);
            return buffer;
        }

        /// <summary>
        /// URL转urlNDEF
        /// </summary>
        /// <param name="ndefStr">输入URL</param>
        /// <returns>返回转换的byte数组</returns>
        private static byte[] formatUrlNdef(string ccData,string ndefStr)
        {
            int i = 0;
            int uriType = 0;
            for (i = 1; i < NDEF_URI_PREFIXS.Length; i++)       //第一个是空的
            {
                if (ndefStr.IndexOf(NDEF_URI_PREFIXS[i]) == 0)
                {
                    uriType = i;
                    ndefStr = ndefStr.Replace(NDEF_URI_PREFIXS[i], "");
                    break;
                }
            }
           // ndefStr = ndefStr.Remove(0, NDEF_URI_PREFIXS[uriType].Length);
            int index = 0;
            byte[] block = new byte[1024];
            byte[] cc = TranfUtil.strToHexByte(ccData);
            Array.Copy(cc, 0, block, index, cc.Length);
            index += cc.Length;
            block[index++] = 0x03;
            block[index++] = (byte)(5 + ndefStr.Length);//L
            block[index++] = 0xD1;//Ndef StatusByte
            block[index++] = 0x01;//Type_Length
            block[index++] = (byte)(ndefStr.Length + 1);//Payload_Length
            block[index++] = 0x55;//TYPE
            block[index++] = (byte)uriType;//ID
            byte[] urlArray = System.Text.Encoding.ASCII.GetBytes(ndefStr);   //string转换的字母.Substring(NDEF_URI_PREFIXS[uriType].Length)
            Array.Copy(urlArray, 0, block, index, urlArray.Length);
            index += urlArray.Length;
            byte[] data = new byte[index];
            Array.Copy(block, 0, data, 0, index);
            return data;
        }

        private static byte[] formatWifiNdef(string ccData,string wifiName,string passwordStr)
        {
            int index = 0;
            byte[] block = new byte[1024];
            byte[] cc = TranfUtil.strToHexByte(ccData);
            Array.Copy(cc, 0, block, index, cc.Length);
            index += cc.Length;
            block[index++] = 0x03; //T
            byte[] typeValue = TranfUtil.asciiToByte("application/vnd.wfa.wsc");
            block[index++] = (byte)(wifiName.Length + 67 + passwordStr.Length);//L
            //V
            block[index++] = 0xDA;
            block[index++] = 0x17;
            block[index++] = (byte)(wifiName.Length + 39 + passwordStr.Length);
            block[index++] = 0x01;
            Array.Copy(typeValue, 0, block, index, typeValue.Length);
            index += typeValue.Length;
            block[index++] = 0x31;
            block[index++] = 0x10;
            block[index++] = 0x0E;
            block[index++] = 0x00;
            block[index++] = (byte)(wifiName.Length + 35 + passwordStr.Length);
            byte[] fixedValue = TranfUtil.strToHexByte("10260001011045");
            Array.Copy(fixedValue,0,block, index, fixedValue.Length);
            index+=fixedValue.Length;
            block[index++] = 0x00;
            block[index++] = (byte)wifiName.Length;
            byte[] wifiArray = TranfUtil.asciiToByte(wifiName);
            Array.Copy(wifiArray,0,block, index, wifiArray.Length);
            index+=wifiArray.Length;
            byte[] fixedValue2 = TranfUtil.strToHexByte("100300020001100F000200011027");
            Array.Copy (fixedValue2,0,block, index, fixedValue2.Length);
            index+=fixedValue2.Length;
            block[index++] = 0x00;
            block[index++] = (byte)passwordStr.Length;
            byte[] passwordArray= TranfUtil.asciiToByte(passwordStr);
            Array.Copy(passwordArray,0,block, index, passwordArray.Length);
            index+=passwordArray.Length;
            byte[] fixedValue3 = TranfUtil.strToHexByte("10200006FFFFFFFFFFFF");
            Array.Copy(fixedValue3, 0, block, index, fixedValue3.Length);
            index += fixedValue3.Length;
            byte[] Buffer = new byte[index];
            Array.Copy(block, 0, Buffer, 0, index);
            return Buffer;
        }
   
        private static byte[] formatBleNdef(string ccData,string mac)
        {
            int index = 0;
            byte[] block = new byte[1024];
            byte[] cc = TranfUtil.strToHexByte(ccData);
            Array.Copy(cc, 0, block, index, cc.Length);
            index += cc.Length;
            block[index++] = 0x03; //T
            byte[] typeValue = TranfUtil.asciiToByte("application/vnd.bluetooth.ep.oob");
            mac = mac.Replace(":", "");
            byte[] payloadValue=TranfUtil.strToHexByte(mac);
            block[index++] = (byte)(typeValue.Length + payloadValue.Length + 5);
            block[index++] = 0xD2;
            block[index++] = 0x20;
            block[index++] = 0x08;
            Array.Copy(typeValue, 0, block, index, typeValue.Length);
            index+=typeValue.Length;
            block[index++] = 0x08;
            block[index++] = 0x00;
            Array.Reverse(payloadValue);
            Array.Copy(payloadValue, 0, block, index, payloadValue.Length);
            index+=payloadValue.Length;
            byte[] Buffer = new byte[index];
            Array.Copy(block, 0, Buffer, 0, index);
            return Buffer;
        }

        public static byte[] formatNdef(string ccData,byte type,string ndefStr1,string ndefStr2)
        {
            byte[] buffer = null;
            switch (type)
            {
                case 0:
                    buffer = formatUrlNdef(ccData, ndefStr1);
                    break;
                case 1:
                    buffer = formatTxtNdef(ccData, ndefStr1);
                    break;
                case 2:
                    buffer=formatWifiNdef(ccData,ndefStr1,ndefStr2);   
                    break;
                case 3:
                    buffer=formatBleNdef(ccData,ndefStr1);
                    break;
            }
            return buffer;

        }

        public static string parseNdef(byte[] valueData)
        {
            try
            {
                int index = 0;
                int idLen = 0;
                int header = valueData[index++];
                int idExited = (header >> 3) & 0x01;
                int tnf = header & 0x07;
                int typeLen = valueData[index++];
                int payloadLen = valueData[index++];
                if (idExited == 1)
                {
                    idLen = valueData[index++];
                }
                if (payloadLen > 0)
                {
                    byte[] type = new byte[typeLen];
                    Array.Copy(valueData, index, type, 0, typeLen);
                    string typeStr = TranfUtil.byteToAscii(type);
                    index += typeLen;
                    if (idLen > 0)
                    {
                        byte[] id = new byte[idLen];
                        Array.Copy(valueData, index, id, 0, idLen);
                        index += idLen;
                    }
                    byte[] payload = new byte[payloadLen];
                    Array.Copy(valueData, index, payload, 0, payloadLen);
                    switch (tnf)
                    {
                        case 1://通用类型:txt.url
                            if (typeStr == "T")
                            {
                                int languageCode = valueData[index++];
                                byte[] ndefData = new byte[payloadLen - languageCode - 1];
                                index += languageCode;
                                Array.Copy(valueData, index, ndefData, 0, ndefData.Length);
                                return TranfUtil.byteToAscii(ndefData);
                            }
                            else if (typeStr == "U")
                            {
                                byte[] ndefData = new byte[payloadLen - 1];
                                index++;
                                Array.Copy(valueData, index, ndefData, 0, ndefData.Length);
                                Console.WriteLine("ndefData:" + TranfUtil.HexToString(ndefData, 0, (uint)ndefData.Length));
                                string headerStr = NDEF_URI_PREFIXS[payload[0]];
                                return headerStr + Encoding.ASCII.GetString(ndefData);
                            }
                            break;
                        case 2://媒体类型:wifi ble
                            if (typeStr == "application/vnd.wfa.wsc")
                            {
                                int payloadIndex = 11;
                                int wifiNameLen = payload[payloadIndex++] * 256 + payload[payloadIndex++] & 0xFF;
                                byte[] wifiName = new byte[wifiNameLen];
                                Array.Copy(payload, payloadIndex, wifiName, 0, wifiNameLen);
                                payloadIndex += wifiNameLen;
                                payloadIndex += 14;
                                int wifiPasswordLen = payload[payloadIndex++] * 256 + payload[payloadIndex++] & 0xFF;
                                byte[] wifiPassword = new byte[wifiPasswordLen];
                                Array.Copy(payload, payloadIndex, wifiPassword, 0, wifiPasswordLen);
                                return TranfUtil.byteToAscii(wifiName) + "#" + TranfUtil.byteToAscii(wifiPassword);

                            }
                            else if (typeStr == "application/vnd.bluetooth.ep.oob")
                            {
                                byte[] mac = new byte[6];
                                Array.Copy(payload, 2, mac, 0, mac.Length);
                                Array.Reverse(mac);
                                return TranfUtil.HexToString(mac, 0, (uint)mac.Length);
                            }
                            break;
                        case 3://绝对URL
                            break;
                        case 4://外部类型:app
                            break;

                    }
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
           
        }

        #region 15693
        public static void getUidFor15693(onSuccess fun1, onFail fun2)
        {
            string uidStr = "";
            int rlt = 0;
            HFREADER_OPRESULT pResult = new HFREADER_OPRESULT();
            ISO15693_UIDPARAM iso15693Uid = new ISO15693_UIDPARAM();
            iso15693Uid.uid = new Byte[25 * 8];
            hfReaderDll.hfReaderCtrlRf(hSerial, 0, 1, hfReaderDll.HFREADER_RF_OPEN, ref pResult, null, null);       //RF复位                                                                                                                      //Thread.Sleep(10);
            rlt = hfReaderDll.iso15693GetUid(hSerial, 0, 1, 0, ref iso15693Uid, null, null);                  //获取UID
            if (rlt > 0 && iso15693Uid.result.flag == 0 && iso15693Uid.num > 0)
            {
                uidStr = TranfUtil.HexToString(iso15693Uid.uid, 0, hfReaderDll.HFREADER_ISO15693_SIZE_UID);
                Array.Copy(iso15693Uid.uid, 0, uid_15693, 0, 8);
                fun1(uidStr);
            }
            else
            {
                fun2("没有读取到标签");
            }
        }

        public static void readNDEFFor15693(onSuccess fun1,onFail fun2)
        {
            //先确认CC数据
            byte[] ccData = readBlockFor15693(0, 1);
            if (ccData!=null)
            {
                if (ccData[0] == 0xE1)//表示标签存在NDEF数据
                {
                    byte[] tlvData = readBlockFor15693(1,1);
                    if(tlvData == null || tlvData[0] != 0x03)
                    {
                        fun2?.Invoke("无NDEF数据");
                        return;
                    }
                    int len = tlvData[1];
                    len -= 2;
                    int blockNum = len / 4;
                    if (len % 4 > 0)
                    {
                        blockNum++;
                    }
                    byte[] valueData = readBlockFor15693(2, blockNum);
                    byte[] ndefData = new byte[valueData.Length + 2];
                    Array.Copy(valueData, 0, ndefData, 2, len);
                    ndefData[0] = tlvData[2];
                    ndefData[1] = tlvData[3];
                    if (valueData != null)
                    {
                        string ndefStr = parseNdef(ndefData);
                        if (ndefStr != null)
                        {
                            fun1?.Invoke(new NdefInfo(TranfUtil.HexToString(uid_15693, 0, (uint)uid_15693.Length),
                                TranfUtil.HexToString(ccData, 0, (uint)ccData.Length),
                                ndefStr));
                        }
                        else
                        {
                            fun2?.Invoke("无NDEF数据");
                        }
                    }
                    else
                    {
                        fun2?.Invoke("读卡失败");
                    }


                }
                else
                {
                    fun2?.Invoke("无NDEF数据");
                }
            }
            else
            {
                fun2?.Invoke("无NDEF数据");
            }          
        }
      
        private static byte[] readBlockFor15693(int addr,int blockNum)
        {
            byte[] blockBuffer=new byte[blockNum * 4];
            ISO15693_BLOCKPARAM iso15693BlockParams = new ISO15693_BLOCKPARAM();
            iso15693BlockParams.block = new Byte[4 * 32];
            iso15693BlockParams.addr = (uint)addr;
            iso15693BlockParams.num = (uint)blockNum;
            int rlt = hfReaderDll.iso15693ReadBlock(hSerial, 0, 1, uid_15693, ref iso15693BlockParams, null, null);
            if (rlt > 0 && iso15693BlockParams.result.flag == 0)
            {
                Array.Copy(iso15693BlockParams.block, 0, blockBuffer, 0, blockNum * 4);
                return blockBuffer;
            }         
            return null;
        }

        public static void writeNDEFFor15693(string ccData,int ndefType,string ndefMsg1,string ndefMsg2,onSuccess fun1,onFail fun2)
        {
            ISO15693_BLOCKPARAM iso15693BlockParams = new ISO15693_BLOCKPARAM();
            iso15693BlockParams.block = new Byte[1024];           
            byte[] ndefData = formatNdef(ccData, (byte)ndefType, ndefMsg1,ndefMsg2);        
            int blockNum=ndefData.Length/4;
            if (ndefData.Length % 4 > 0)
            {
                blockNum++;
            }
            iso15693BlockParams.addr = 0;//ISO15693从0地址开始写
            iso15693BlockParams.num = (uint)blockNum;
            Array.Copy(ndefData, 0, iso15693BlockParams.block, 0, ndefData.Length);
            int rlt = hfReaderDll.iso15693WriteBlock(hSerial, 0, 1, uid_15693, ref iso15693BlockParams, null, null);
            if (rlt > 0 && iso15693BlockParams.result.flag == 0)
            {
                checkBlock(ndefData, obj =>
                {
                    //写成功
                    fun1?.Invoke(TranfUtil.HexToString(uid_15693, 0,(uint)uid_15693.Length));
                }, msg =>
                {
                    //写失败
                    fun2?.Invoke("写数据失败");
                });
            }
            else
            {
                //写失败
                fun2?.Invoke("通信超时");
            }

        }


        public static void imWriteBlock(string dataStr, onSuccess fun1, onFail fun2)
        {
            int rlt = -1;
            byte[] data = TranfUtil.strToHexByte(dataStr);
            // getUid();
            int len = data.Length / 4;
            if (data.Length % 4 > 0)
            {
                len += 1;//不足一个块数目的数据补零
            }
            ISO15693_BLOCKPARAM iso15693BlockParams = new ISO15693_BLOCKPARAM();
            iso15693BlockParams.block = new Byte[4 * 32];
            iso15693BlockParams.addr = (uint)0;
            iso15693BlockParams.num = (uint)len;
            Array.Copy(data, 0, iso15693BlockParams.block, 0, data.Length);
            rlt = hfReaderDll.iso15693ImWriteBlock(hSerial, 0, 1, uid_15693, ref iso15693BlockParams, null, null);
            if (rlt > 0 && iso15693BlockParams.result.flag == 0)
            {
                fun1("写块成功");
            }
            else
            {
                fun2("写块失败");
            }
        }

        public static void checkBlock(byte[] data, onSuccess fun1, onFail fun2)
        {
            int rlt = -1;
            int len = data.Length / 4;
            if (data.Length % 4 > 0)
            {
                len += 1;
            }
            byte[] blockBuffer = new byte[len * 4];
            for (int i = 0; i < len; i++)
            {
                ISO15693_BLOCKPARAM iso15693BlockParams = new ISO15693_BLOCKPARAM();
                iso15693BlockParams.block = new Byte[4 * 32];
                iso15693BlockParams.addr = (uint)i;
                iso15693BlockParams.num = 1;
                rlt = hfReaderDll.iso15693ReadBlock(hSerial, 0, 1, uid_15693, ref iso15693BlockParams, null, null);
                if (rlt > 0 && iso15693BlockParams.result.flag == 0)
                {
                    Array.Copy(iso15693BlockParams.block, 0, blockBuffer, i * 4, 4);
                }
                else
                {
                    fun2("读块失败");
                    return;
                }
            }
            if (checkBytes(data, blockBuffer, data.Length))
            {
                fun1("校验成功");
            }
            else
            {
                fun2("校验失败");
            }


        }

        #endregion

        #region Ntag
        public static void getUidFor14443A(onSuccess fun1, onFail fun2)
        {
            string uidStr = "";
            int rlt = 0;
            HFREADER_OPRESULT pResult = new HFREADER_OPRESULT();
            ISO14443A_UIDPARAM iso14443aUid = new ISO14443A_UIDPARAM();
            iso14443aUid.uid = new ISO14443A_UID[15];
            hfReaderDll.hfReaderCtrlRf(hSerial, 0, 1, hfReaderDll.HFREADER_RF_OPEN, ref pResult, null, null);       //RF复位                                                                                                                      //Thread.Sleep(10);
            rlt = hfReaderDll.iso14443AGetUID(hSerial, 0, 1, 0, ref iso14443aUid, null, null);                 //获取UID
            if (rlt > 0 && iso14443aUid.result.flag == 0 && iso14443aUid.num > 0)
            {
                uidStr = TranfUtil.HexToString(iso14443aUid.uid[0].uid, 0, iso14443aUid.uid[0].len);
                Array.Copy(iso14443aUid.uid[0].uid, 0, uid_14443A, 0, 8);
                fun1(uidStr);
            }
            else
            {
                fun2("没有读取到标签");
            }
        }

        public static void writeNDEFForM0(string ccData, int ndefType, string ndefMsg1, string ndefMsg2, onSuccess fun1, onFail fun2)
        {
            byte[] ndefData = formatNdef(ccData, (byte)ndefType, ndefMsg1, ndefMsg2);
            if (ndefData != null)
            {
                ndefData = ndefData.Skip(4).ToArray();
                writePage(ndefData, obj =>
                {
                    checkPage(ndefData, sender =>
                    {
                        fun1?.Invoke(TranfUtil.HexToString(uid_14443A, 0, (uint)uid_14443A.Length));
                    }, msg =>
                    {
                        fun2?.Invoke("写数据失败");
                    });

                }, msg =>
                {
                    fun2?.Invoke("写数据失败");
                });
            }
            else
            {
                fun2?.Invoke("写数据失败");
            }
            
        }

        public static void readNDEFForM0(onSuccess fun1, onFail fun2)
        {
            byte[] ccData = readPage(3, 1);
            if (ccData != null)
            {
                if (ccData[0] == 0xE1)//表示标签存在NDEF数据
                {
                    byte[] tlvData = readPage(4, 1);
                    if (tlvData[0] == 0x03)
                    {                      
                        int len = tlvData[1];
                        len -= 2;                                                                                                                                                           
                        int blockNum = len / 4;
                        if (len % 4 > 0)
                        {
                            blockNum++;
                        }
                        byte[] valueData = readPage(5, blockNum);
                        byte[] ndefData = new byte[valueData.Length + 2];
                        Array.Copy(valueData, 0, ndefData, 2, len);
                        ndefData[0] = tlvData[2];
                        ndefData[1] = tlvData[3];
                        if (valueData != null)
                        {
                            string ndefStr = parseNdef(ndefData);
                            if (ndefStr != null)
                            {
                                fun1?.Invoke(new NdefInfo(TranfUtil.HexToString(uid_14443A, 0, (uint)uid_14443A.Length),
                                    TranfUtil.HexToString(ccData, 0, (uint)ccData.Length),
                                    ndefStr));
                            }
                            else
                            {
                                fun2?.Invoke("无NDEF数据");
                            }
                        }
                        else
                        {
                            fun2?.Invoke("读卡失败");
                        }

                    }
                    else
                    {
                        fun2?.Invoke("无NDEF数据");
                    }

                }
                else
                {
                    fun2?.Invoke("无NDEF数据");
                }
            }
            else
            {
                fun2?.Invoke("无NDEF数据");
            }
        }
        private static void writePage(byte[] data, onSuccess fun1, onFail fun2)
        {
            int rlt = -1;           
            int len = data.Length / 4;
            if (data.Length % 4 > 0)
            {
                len += 1;//不足一个块数目的数据补零
            }
            ISO14443A_BLOCKPARAM iso14443ABlock = new ISO14443A_BLOCKPARAM();
            iso14443ABlock.block = new Byte[1024];
            iso14443ABlock.key = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1_KEY];
            iso14443ABlock.uid = new ISO14443A_UID();
            iso14443ABlock.uid.uid = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_MAX_UID];
            iso14443ABlock.keyType = 0;
            iso14443ABlock.uid.len = 0;
            iso14443ABlock.addr = 0x04;
            iso14443ABlock.num = (uint)len;
            Array.Copy(data, 0, iso14443ABlock.block, 0, data.Length);
            rlt = hfReaderDll.iso14443AWriteM0Block(hSerial, 0, 1, ref iso14443ABlock, null, null);
            if (rlt > 0 && iso14443ABlock.result.flag == 0)
            {
                fun1("写Page成功");
            }
            else
            {
                fun2("写Page失败");
            }
        }

        private static void checkPage(byte[] data, onSuccess fun1, onFail fun2)
        {
            int rlt = -1;
            int len = data.Length / 4;
            if (data.Length % 4 > 0)
            {
                len += 1;
            }
            byte[] blockBuffer = new byte[len * 4];
            ISO14443A_BLOCKPARAM iso14443ABlock = new ISO14443A_BLOCKPARAM();
            iso14443ABlock.block = new Byte[13 * hfReaderDll.HFREADER_ISO14443A_LEN_M1BLOCK];
            iso14443ABlock.key = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1_KEY];
            iso14443ABlock.uid = new ISO14443A_UID();
            iso14443ABlock.uid.uid = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_MAX_UID];
            iso14443ABlock.keyType = 0;
            iso14443ABlock.uid.len = 0;
            iso14443ABlock.addr = 0x04;
            iso14443ABlock.num = (uint)len;
            rlt = hfReaderDll.iso14443AReadM0Block(hSerial, 0, 1, ref iso14443ABlock, null, null);
            if (rlt > 0 && iso14443ABlock.result.flag == 0)
            {
                Array.Copy(iso14443ABlock.block, 0, blockBuffer, 0, len * 4);
            }
            else
            {
                fun2("读Page失败");
                return;
            }
            if (checkBytes(data, blockBuffer, data.Length))
            {
                fun1("校验成功");
            }
            else
            {
                fun2("校验失败");
            }
        }

        private static byte[] readPage(int addr,int pageNum)
        {
            byte[] blockBuffer = new byte[pageNum * 4];
            ISO14443A_BLOCKPARAM iso14443ABlock = new ISO14443A_BLOCKPARAM();
            iso14443ABlock.block = new Byte[1024];
            iso14443ABlock.key = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1_KEY];
            iso14443ABlock.uid = new ISO14443A_UID();
            iso14443ABlock.uid.uid = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_MAX_UID];
            iso14443ABlock.keyType = 0;
            iso14443ABlock.uid.len = 0;
            //M0卡一次最多读取52个page iso14443ABlock.block的长度最大208个字节
            for (int i = 0; i < pageNum; i++)
            {
                iso14443ABlock.addr = (uint)addr;
                iso14443ABlock.num = 1;
                int rlt = hfReaderDll.iso14443AReadM0Block(hSerial, 0, 1, ref iso14443ABlock, null, null);
                if (rlt > 0 && iso14443ABlock.result.flag == 0)
                {
                    Array.Copy(iso14443ABlock.block, 0, blockBuffer, i*4, 4);
                }
                else
                {
                    return null;
                }
                addr++;
            }           
            return blockBuffer;
        }

        #endregion

        #region M1


        public static void readNDEFForM1(onSuccess fun1, onFail fun2)
        {
            byte[] tlvData = readM1Block(4, 1);
            if (tlvData[2] == 0x03)
            {
                int len = tlvData[3];
                len += 4;
                int blockNum = len / 16;
                if (len % 16 > 0)
                {
                    blockNum++;
                }
                byte[] valueData = readM1Block(4, blockNum);
                if (valueData != null&&valueData.Length>4)
                {
                    byte[] ndefData=new byte[valueData.Length-4];
                    Array.Copy(valueData, 4, ndefData, 0, ndefData.Length);
                    string ndefStr = parseNdef(ndefData);
                    if (ndefStr != null)
                    {
                        fun1?.Invoke(new NdefInfo(TranfUtil.HexToString(uid_14443A, 0, (uint)uid_14443A.Length),
                            "0000",
                            ndefStr));
                    }
                    else
                    {
                        fun2?.Invoke("无NDEF数据");
                    }
                }
                else
                {
                    fun2?.Invoke("读卡失败");
                }

            }
            else
            {
                fun2?.Invoke("无NDEF数据");
            }
        }

        private readonly static byte[] _fixedPasswordBlock = TranfUtil.strToHexByte("D3F7D3F7D3F77F078840FFFFFFFFFFFF");
        public static void writeNDEFForM1(string ccData, int ndefType, string ndefMsg1, string ndefMsg2, onSuccess fun1, onFail fun2)
        {
            byte[] ndefData = formatNdef("0000", (byte)ndefType, ndefMsg1, ndefMsg2);
            if (ndefData!=null)
            {
                int blockAddr = 0;
                int dataOffset = 0;
                for (int sector = 0; sector < 16; sector++)
                {
                    for (int block = 0; block < 4; block++)
                    {
                        if (sector == 0)
                        {
                            if (block == 0)
                            {
                                blockAddr++;
                                continue;
                            }
                            else if (block == 1)
                            {
                                writeM1Block(blockAddr, TranfUtil.strToHexByte("140103E103E103E103E103E103E103E1"), obj =>
                                {
                                    blockAddr++;
                                }, msg =>
                                {
                                    fun2?.Invoke("写数据失败");
                                    return;
                                });
                            }
                            else if (block == 2)
                            {
                                writeM1Block(blockAddr, TranfUtil.strToHexByte("03E103E103E103E103E103E103E103E1"), obj =>
                                {
                                    blockAddr++;
                                }, msg =>
                                {
                                    fun2?.Invoke("写数据失败");
                                    return;
                                });
                            }
                            else if (block == 3)
                            {
                                writeM1Block(blockAddr, TranfUtil.strToHexByte("A0A1A2A3A4A5787788C1FFFFFFFFFFFF"), obj =>
                                {
                                    blockAddr++;
                                }, msg =>
                                {
                                    fun2?.Invoke("写数据失败");
                                    return;
                                });
                            }
                        }
                        else
                        {
                            if (block == 3)
                            {
                                writeM1Block(blockAddr, _fixedPasswordBlock, obj =>
                                {
                                    blockAddr++;
                                }, msg =>
                                {
                                    fun2?.Invoke("写数据失败");
                                    return;
                                });
                            }
                            else
                            {
                                byte[] buffer = new byte[16];
                                if (dataOffset < ndefData.Length)
                                {
                                    int copyLen = Math.Min(16, ndefData.Length - dataOffset);
                                    Array.Copy(ndefData, dataOffset, buffer, 0, copyLen);
                                    dataOffset += copyLen;
                                }
                                else
                                {
                                    // 不足补 0x00
                                    for (int i = 0; i < 16; i++) buffer[i] = 0x00;
                                }
                                writeM1Block(blockAddr, buffer, obj =>
                                {
                                    blockAddr++;
                                }, msg =>
                                {
                                    fun2?.Invoke("写数据失败");
                                    return;
                                });
                            }
                        }


                    }
                }
                fun1?.Invoke(TranfUtil.HexToString(uid_14443A,0,(uint)uid_14443A.Length));
            }
            else
            {
                fun2?.Invoke("写数据失败");
            }

        }

        private static void writeM1Block(int addr, byte[] data, onSuccess fun1, onFail fun2)
        {
            ISO14443A_BLOCKPARAM pBlock = new ISO14443A_BLOCKPARAM();
            pBlock.uid.uid = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_MAX_UID];
            pBlock.block = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1BLOCK * hfReaderDll.HFREADER_ISO14443A_M1BLOCKNUM_MAX];
            pBlock.key = TranfUtil.strToHexByte("FFFFFFFFFFFF");
            pBlock.keyType = hfReaderDll.HFREADER_ISO14443A_KEY_B;
            pBlock.addr = (uint)addr;
            Array.Copy(data, 0, pBlock.block, 0, data.Length);
            pBlock.num = 1;
            int rlt = hfReaderDll.iso14443AAuthWriteM1Block(hSerial, 0, 1, ref pBlock, null, null);
            if (rlt > 0 && pBlock.result.flag == 0)
            {
                fun1?.Invoke(TranfUtil.HexToString(uid_14443A, 0, (uint)uid_14443A.Length));
            }
            else
            {
                fun2?.Invoke("写失败");
            }
        }

        private static byte[] readM1Block(int addr, int blockNum)
        {
            byte[] buffer = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1BLOCK * blockNum];
            ISO14443A_BLOCKPARAM pBlock = new ISO14443A_BLOCKPARAM();
            pBlock.uid.uid = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_MAX_UID];
            pBlock.key = TranfUtil.strToHexByte("FFFFFFFFFFFF");
            pBlock.keyType = hfReaderDll.HFREADER_ISO14443A_KEY_B;            
            for (int i = 0; i < blockNum; i++)
            {
                pBlock.block = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1BLOCK * hfReaderDll.HFREADER_ISO14443A_M1BLOCKNUM_MAX];
                pBlock.num = 1;
                pBlock.addr = (uint)addr;
                int rlt = hfReaderDll.iso14443AAuthReadM1Block(hSerial, 0, 1, ref pBlock, null, null);
                if (rlt > 0 && pBlock.result.flag == 0)
                {
                    Array.Copy(pBlock.block, 0, buffer, i*16, 16);
                }
                else
                {
                    return null;
                }
                addr++;
                if (addr%4==3)
                {
                    addr++;
                }
            }
            return buffer;
        }

        private static void checkM1Block(byte[] data, onSuccess fun1, onFail fun2)
        {
            ISO14443A_BLOCKPARAM pBlock = new ISO14443A_BLOCKPARAM();
            pBlock.uid.uid = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_MAX_UID];
            pBlock.block = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1BLOCK * hfReaderDll.HFREADER_ISO14443A_M1BLOCKNUM_MAX];
            pBlock.key = TranfUtil.strToHexByte("FFFFFFFFFFFF");
            pBlock.keyType = hfReaderDll.HFREADER_ISO14443A_KEY_B;
            byte[] block = new Byte[hfReaderDll.HFREADER_ISO14443A_LEN_M1BLOCK];
            int len = data.Length / 16;
            if (len % 16 > 0)
            {
                len++;
            }
            byte[] buffer = readM1Block(0x04, len);
            if (buffer!=null)
            {
                if (checkBytes(data,buffer,data.Length))
                {
                    fun1?.Invoke("校验成功");
                }
                else
                {
                    fun2?.Invoke("校验失败");
                }
            }
            else
            {
                fun2?.Invoke("校验失败");
            }

        }
        #endregion

        private static bool checkBytes(byte[] scrArray, byte[] destArray, int len)
        {
            bool result = false;
            for (int i = 0; i < len; i++)
            {
                if (scrArray[i].Equals(destArray[i]) == false)
                {
                    return result;
                }
            }
            return true;
        }
        public static void closeReader(int connect_type)
        {
            if (connect_type==0)
            {
                hfReaderDll.hfReaderCloseUsb(hSerial);
            }
            else
            {
                hfReaderDll.hfReaderClosePort(hSerial);
            }
        }

    }
}
