using System;
using System.IO;

namespace WindowsFormsApp
{
    public static class WriteLogs
    {



        // 1. 定义静态锁对象 (必须 static，确保所有线程共用同一个锁)
        private static readonly object locker = new object();

        public static void WriteLog(string msg)
        {
            // 2. 加锁：确保同一时间只有一个线程能执行写入逻辑
            lock (locker)
            {
                try
                {
                    // 生成路径
                    string mm = DateTime.Now.ToString("yyyy-MM").Replace("-", "");
                    string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", mm);

                    // 确保目录存在
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // 生成文件路径
                    string logPath = Path.Combine(dirPath, $"{DateTime.Now:yyyy-MM-dd}_log.txt");

                    // 3. 直接写入，无需先读取文件
                    // File.AppendText 会自动打开文件，追加内容，并在 using 块结束时自动关闭
                    using (StreamWriter sw = File.AppendText(logPath))
                    {
                        string timestr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
                        sw.WriteLine($"[{timestr}]: {msg}");
                        sw.Flush(); // 确保数据立即写入磁盘
                    }
                }
                catch (Exception ex)
                {
                    // 4. 捕获异常，防止日志写入失败导致主程序崩溃
                    // 可以在这里记录到控制台或内存日志
                    Console.WriteLine($"日志写入失败：{ex.Message}");
                }
            }
        }



    }
}
