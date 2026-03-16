using NDEFReadWriteTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NDEFReadWriteTool
{
    internal static class Program
    {
        private static SplashForm splash;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //启动广告线程
            Thread splashThread = new Thread(new ThreadStart(ShowSplash));
            splashThread.Start();

            // 模拟程序加载
            Thread.Sleep(3000);

            // 关闭广告
            splash?.Invoke(new Action(() =>
            {
                splash.Close();
            }));

            Form1 form = new Form1();
            ReaderService readerService = new ReaderService();
            ReaderPersenter readerPersenter = new ReaderPersenter(readerService,form);
            Application.Run(form);
        }

        static void ShowSplash()
        {
            splash = new SplashForm();
            Application.Run(splash);
        }
    }
}
