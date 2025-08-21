using NDEFReadWriteTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NDEFReadWriteTool
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            ReaderService readerService = new ReaderService();
            ReaderPersenter readerPersenter = new ReaderPersenter(readerService,form);
            Application.Run(form);
        }
    }
}
