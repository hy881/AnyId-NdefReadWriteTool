using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool.Model
{
    internal class ReaderService : IReaderService
    {
        
        public event OnMessageReturnDelegate OnInfoReturn;
        event OnMessageReturnDelegate OnFailReturn;
        public event VersionReturnDelegate OnVersionReturn;
        private int _tagType;


        public async Task<bool> GetReaderVersionAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    bool bResult = false;
                    AnyIDReader.getReaderVersion(obj =>
                    {
                        bResult = true;
                        OnVersionReturn?.Invoke((ReaderVersion)obj);
                    }, msg =>
                    {
                        bResult = false;
                    });
                    return bResult;
                }
                catch (Exception)
                {
                    return false;
                }
             
            });
        }   

        public async Task<bool> ReaderInitAsync(ConnectParam param,int tagType)
        {
            try
            {
                return await Task.Run(() =>
                {
                    bool bStart = true;
                    bool bResult=false;
                    int step = 0;
                    while (bStart)
                    {
                        switch (step)
                        {
                            case 0:
                                AnyIDReader.initReader(obj =>
                                {
                                    step++;
                                }, msg =>
                                {
                                    bStart = false;
                                    bResult = false;
                                });
                                break;
                            case 1:
                                AnyIDReader.openReader(obj =>
                                {
                                    step++;
                                }, msg =>
                                {
                                    bStart = false;
                                    bResult = false;
                                });
                                break;
                            case 2:
                                AnyIDReader.getReaderVersion(obj =>
                                {
                                    step++;
                                    OnVersionReturn?.Invoke((ReaderVersion)obj);
                                }, msg =>
                                {                                 
                                    
                                });
                                
                                break;
                            case 3:
                                AnyIDReader.setReaderConfig(tagType, obj =>
                                {
                                    bResult = true;
                                    _tagType = tagType;
                                }, msg =>
                                {
                                    bResult = false;
                                });
                                bStart = false;
                                break;
                        }
                    }
                    return bResult;
                });

            }
            catch (Exception)
            {

                return false;
            }
        }

        public void CloseReader(int type)
        {
            AnyIDReader.closeReader(type);
        }

        public async Task<bool> SetReaderConfigAsync(int type)
        {
           return await Task.Run(() =>
            {
                try
                {
                    bool bResult = false;
                    AnyIDReader.setReaderConfig(type, obj =>
                    {
                        bResult = true;
                        _tagType = type;
                    }, msg =>
                    {
                        bResult = false;
                    });
                    return bResult;
                }
                catch (Exception)
                {
                    return false;
                }
                             
            });
           
        }

        public async Task<bool> ReadNdefDataAsync(int tagType)
        {
           return  await Task.Run(() =>
            {
                try
                {
                    bool rlt = readUid();
                    if (rlt)
                    {
                        NdefInfo ndefInfo = readNdef();
                        if (ndefInfo != null)
                        {
                            OnInfoReturn?.Invoke(ndefInfo, tagType);
                            return true;
                        }
                    }

                }
                catch (Exception)
                {
                }             
                return false;
            });
        }

        public async Task<bool> WriteNdefDataAsync(int ndefType, string ccData, string ndefData1, string ndefData2)
        {
            return await Task.Run(() =>
            {
                try
                {
                    bool rlt = readUid();
                    if (rlt)
                    {
                        NdefInfo ndefInfo = writeNdef(_tagType, ndefType, ccData, ndefData1, ndefData2);
                        if (ndefInfo != null)
                        {
                            OnInfoReturn?.Invoke(ndefInfo, ndefType);
                            return true;
                        }
                    }
                }
                catch (Exception)
                {

                } 
                return false;
            });
        }

        private bool readUid()
        {
            bool bResult=false;
            AnyIDReader.resetRf();
            switch (_tagType)
            {
                case 0:
                    AnyIDReader.getUidFor15693(obj =>
                    {
                        bResult=true;
                    }, msg =>
                    {

                    });
                    break;
                case 1:
                    AnyIDReader.getUidFor14443A(obj =>
                    {
                        bResult = true;
                    }, msg =>
                    {

                    });
                    break;
                case 2:
                    AnyIDReader.getUidFor14443A(obj =>
                    {
                        bResult = true;
                    }, msg =>
                    {

                    });
                    break;
            }
            return bResult;
        }

        private NdefInfo writeNdef(int tagType,int ndefType, string ccData,string ndefData1,string ndefData2)
        {
            NdefInfo info=null;
           switch (tagType)
            {
                case 0://写15693
                    AnyIDReader.writeNDEFFor15693(ccData, ndefType, ndefData1, ndefData2, obj =>
                    {
                        info = new NdefInfo(obj.ToString(), ccData, ndefData1, ndefData2);
                    }, msg =>
                    {

                    });
                    break;
                case 1://写M1
                    AnyIDReader.writeNDEFForM1(ccData, ndefType, ndefData1, ndefData2, obj =>
                    {
                        info = new NdefInfo(obj.ToString(), ccData, ndefData1, ndefData2);
                    }, msg =>
                    {

                    });
                    break;
                case 2://写M0
                    AnyIDReader.writeNDEFForM0(ccData, ndefType, ndefData1, ndefData2, obj =>
                    {
                        info=new NdefInfo(obj.ToString(), ccData, ndefData1,ndefData2);
                    }, msg =>
                    {

                    });
                    break;
            }
            return info;
        }

        private NdefInfo readNdef()
        {
            NdefInfo ndefInfo = null;
            switch (_tagType)
            {
                case 0:
                    AnyIDReader.readNDEFFor15693(obj =>
                    {
                        ndefInfo = (NdefInfo)obj;
                    }, msg =>
                    {

                    });
                    break;
                case 1:
                    AnyIDReader.readNDEFForM1(obj =>
                    {
                        ndefInfo = (NdefInfo)obj;
                    }, msg =>
                    {

                    });
                    break;
                case 2:
                    AnyIDReader.readNDEFForM0(obj =>
                    {
                        ndefInfo = (NdefInfo)obj;
                    }, msg =>
                    {

                    });
                    break;
            }
            return ndefInfo;
        }



        //public async Task<bool> WriteUrlAsync(string ccData, string url)
        //{
        //    return await Task.Run(() =>
        //    {
        //        bool bResult=false;
        //        switch (tagType)
        //        {
        //            case 0:
        //                bResult = writeUrlByIso15693(ccData, url);
        //                break;
        //            case 1:
        //                break;
        //            case 2:
                        
        //                break;
        //        }
        //        return bResult;                
        //    });
        //}

        //private bool writeUrlByIso15693(string ccData,string ndefStr)
        //{
        //    int step = 0;
        //    bool bResult = false;
        //    bool bContinue = true;
        //    NdefInfo ndefInfo = new NdefInfo();
        //    ndefInfo.Cc = ccData;
        //    ndefInfo.NdefData = ndefStr;
        //    while (bContinue)
        //    {
        //        switch (step)
        //        {
        //            case 0://
        //                AnyIDReader.getUidFor15693(obj =>
        //                {
        //                    ndefInfo.Uid=obj.ToString();
        //                    step++;
        //                }, msg =>
        //                {
        //                    bContinue = false;
        //                });
        //                break;
        //            case 1:
        //                bContinue = false;
        //                AnyIDReader.writeNDEFFor15693(ccData, 0, ndefStr,"", obj =>
        //                {
        //                    bResult = true;
        //                    OnInfoReturn?.Invoke(ndefInfo, 0);
        //                }, msg =>
        //                {
                            
        //                });
        //                break;
        //        }
        //    }
        //    return bResult;
        //}

        //private bool writeVCardByIso15693(string ccData, string wifiName,string passwordStr)
        //{
        //    int step = 0;
        //    bool bResult = false;
        //    bool bContinue = true;
        //    NdefInfo ndefInfo = new NdefInfo();
        //    ndefInfo.Cc = ccData;
        //    ndefInfo.NdefData = wifiName;
        //    ndefInfo.NdefData2= passwordStr;
        //    while (bContinue)
        //    {
        //        switch (step)
        //        {
        //            case 0://
        //                AnyIDReader.getUidFor15693(obj =>
        //                {
        //                    ndefInfo.Uid = obj.ToString();
        //                    step++;
        //                }, msg =>
        //                {
        //                    bContinue = false;
        //                });
        //                break;
        //            case 1:
        //                bContinue = false;
        //                AnyIDReader.writeNDEFFor15693(ccData, 2, wifiName,passwordStr, obj =>
        //                {
        //                    bResult = true;
        //                    OnInfoReturn?.Invoke(ndefInfo, 0);
        //                }, msg =>
        //                {

        //                });
        //                break;
        //        }
        //    }
        //    return bResult;
        //}

        //private NdefInfo readUrlByIso15693()
        //{
        //    int step = 0;
        //    NdefInfo ndefInfo = null;
        //    bool bContinue = true;
        //    while (bContinue)
        //    {
        //        switch (step)
        //        {
        //            case 0://
        //                AnyIDReader.getUidFor15693(obj =>
        //                {
        //                    step++;
        //                }, msg =>
        //                {
        //                    bContinue = false;
        //                });
        //                break;
        //            case 1:
        //                bContinue = false;
        //                AnyIDReader.readNDEFFor15693(obj =>
        //                {
        //                    ndefInfo = (NdefInfo) obj;                           
        //                }, msg =>
        //                {

        //                });
        //                break;
        //        }
        //    }
        //    return ndefInfo;
        //}

        //public Task<bool> ReadUrlAsync()
        //{
        //    return Task.Run(() =>
        //    {
        //        NdefInfo ndefInfo = null;
        //        switch (tagType)
        //        {
        //            case 0:
        //                ndefInfo = readUrlByIso15693();
        //                break;
        //            case 1:
        //                break;
        //            case 2:
        //                break;
        //        }
        //        if (ndefInfo != null)
        //        {
        //            OnInfoReturn?.Invoke(ndefInfo, 0);
        //            return true;
        //        }
        //        return false;
        //    });
        //}

        //public async Task<bool> WriteVCardAsync(string ccData, string vCard)
        //{
        //    return await Task.Run(() =>
        //    {
        //        bool bResult = false;
        //        switch (tagType)
        //        {
        //            case 0:
        //             //   bResult = writeVCardByIso15693(ccData, vCard);
        //                break;
        //            case 1:
        //                break;
        //            case 2:

        //                break;
        //        }
        //        return bResult;
        //    });
        //}

    }
}
