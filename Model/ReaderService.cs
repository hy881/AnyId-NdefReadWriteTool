using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NDEFReadWriteTool.Model
{
    internal class ReaderService : IReaderService
    {
        

        private int _tagType;

        public event Action<ReaderVersion> onReaderVersionReturn;
     

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
                        onReaderVersionReturn?.Invoke((ReaderVersion)obj);
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
                                AnyIDReader.openReader(param,obj =>
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
                                    onReaderVersionReturn?.Invoke((ReaderVersion)obj);
                                }, msg =>
                                {
                                    bStart = false;
                                    bResult = false;
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

        

        public async Task<OperationResult> WriteNdefDataAsync(int ndefType, string[] ndefList)
        {
            return await Task.Run(() =>
            {
                string errMsg = "";
                bool bResult = false;
                switch (_tagType)
                {
                    case 0://写15693
                        AnyIDReader.writeNDEFFor15693(ndefType, ndefList, obj =>
                        {
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 1://写M1
                        AnyIDReader.writeNDEFForM1(ndefType, ndefList, obj =>
                        {
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 2://写M0
                        AnyIDReader.writeNDEFForM0(ndefType, ndefList, obj =>
                        {
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                }
                if (bResult)
                {
                    return OperationResult.Success(ndefList, "");
                }
                return OperationResult.Failure(-1, errMsg);

            });
        }

        public async Task<OperationResult> GetTagUidAsync()
        {
            return await Task.Run(() =>
            {
                bool bResult = false;
                string uid = "";
                string errMsg = "";
                AnyIDReader.resetRf();
                switch (_tagType)
                {
                    case 0:
                        AnyIDReader.getUidFor15693(obj =>
                        {
                            uid=obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 1:
                        AnyIDReader.getUidFor14443A(obj =>
                        {
                            uid = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 2:
                        AnyIDReader.getUidFor14443A(obj =>
                        {
                            uid = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                }
                if (bResult)
                {
                    return OperationResult.Success(uid,"");
                }
                return OperationResult.Failure(-1,errMsg);
            });
        }

        public async Task<OperationResult> InitTagAsync(int len)
        {
            return await Task.Run(() =>
            {
                bool bResult = false;
                string errMsg = "";
                switch (_tagType)
                {
                    case 0:
                        AnyIDReader.writeCcDataFor15693(len,obj =>
                        {
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 1:
                        return OperationResult.Success("", "");
                    case 2:
                        AnyIDReader.writeCcDataForM0(len, obj =>
                        {
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                }
                if (bResult)
                {
                    return OperationResult.Success("标签初始化成功");
                }
                return OperationResult.Failure(-1, errMsg);
            });
        }

        public async Task<OperationResult> GetTagCcDataAsync()
        {
            return await Task.Run(() =>
            {
                bool bResult = false;
                string ccdata = "";
                string errMsg = "";
                switch (_tagType)
                {
                    case 0:
                        AnyIDReader.readCcDataFor15693(obj =>
                        {
                            ccdata = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 1:
                        return OperationResult.Success("","");

                    case 2:
                        AnyIDReader.readCcDataForM0(obj =>
                        {
                            ccdata = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                }
                if (bResult)
                {
                    return OperationResult.Success(ccdata, "");
                }
                return OperationResult.Failure(-1, errMsg);
            });
           

        }

        public async Task<OperationResult> ReadNdefDataAsync()
        {
            return await Task.Run(() =>
            {
                string ndefInfo = "";
                bool bResult = false;
                string errMsg = "";
                switch (_tagType)
                {
                    case 0:
                        AnyIDReader.readNDEFFor15693(obj =>
                        {
                            ndefInfo = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 1:
                        AnyIDReader.readNDEFForM1(obj =>
                        {
                            ndefInfo = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                    case 2:
                        AnyIDReader.readNDEFForM0(obj =>
                        {
                            ndefInfo = obj.ToString();
                            bResult = true;
                        }, msg =>
                        {
                            errMsg = msg;
                        });
                        break;
                }
                if (bResult)
                {
                    return OperationResult.Success(ndefInfo, "");
                }
                return OperationResult.Failure(-1, errMsg);
            });
        }

    }
}
