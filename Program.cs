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

            SplashForm splash = new SplashForm();

            Task.Run(async () =>
            {
                await Task.Delay(3000);

                if (!splash.IsDisposed)
                {
                    splash.Invoke(new Action(() =>
                    {
                        if (!splash.IsDisposed)
                            splash.Close();
                    }));
                }
            });

            Application.Run(splash);

            // Splash 关闭后才执行
            Form1 form = new Form1();
            ReaderService readerService = new ReaderService();
            ReaderPersenter readerPersenter = new ReaderPersenter(readerService, form);
            Application.Run(form);
      
        }

        static void ShowSplash()
        {
            splash = new SplashForm();
            Application.Run(splash);
        }
    }
}
