using NDEFReadWriteTool.bean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDEFReadWriteTool.Infrastructure
{
    internal class CsvUtil
    {
        private BackgroundWorker worker = null;

        public CsvUtil()
        {
            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            worker.DoWork += saveData;
           
        }


        public List<NdefInfo> writeDataList = new List<NdefInfo>();
        public void setWriteDataList(List<NdefInfo> ndefInfos)
        {
            writeDataList = ndefInfos;
        }

        public void startSaveCsvFile(string path, Action<int> onProgressChanged = null, Action<object> onComplete = null)
        {
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync(path);
                worker.RunWorkerCompleted += (s, e) => onComplete?.Invoke(e.Result);
                worker.ProgressChanged += (s, e) => onProgressChanged?.Invoke((int)e.ProgressPercentage);
            }
        }

        public void saveData(object sender, DoWorkEventArgs e)
        {
            try
            {
                string filePath = e.Argument.ToString();
                // 使用 StreamWriter 写入 CSV 文件
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // 写入 CSV 数据（第一行为标题行）
                    writer.WriteLine("序号,Uid,Url");
                    for (int i = 0; i < writeDataList.Count; i++)  
                    {
                        writer.WriteLine($"{i+1},{writeDataList[i].Uid},{writeDataList[i].NdefData}");
                        worker.ReportProgress(i);
                    }
                    e.Result = true;
                }
            }
            catch (Exception ex)
            {
                e.Result = false;
            }
        }
    }
}
