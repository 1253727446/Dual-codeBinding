using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (var mutex = new Mutex(true, "WindowsFormsApp_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("程序已在运行中，不能重复打开。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 防止连续 HTTP 请求导致 ServicePoint 连接池耗尽，引发卡死
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.MaxServicePointIdleTime = 5000;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
