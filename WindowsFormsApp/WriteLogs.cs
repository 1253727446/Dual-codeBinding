using System;
using System.IO;

namespace WindowsFormsApp
{
    public static class WriteLogs
    {



        // 1. 定义静态锁对象 (必须 static，确保所有线程共用同一个锁)
        private static readonly object locker = new object();

        /// <summary>日志文件上限 1MB</summary>
        private const long MaxLogSize = 1 * 1024 * 1024;

        /// <summary>
        /// 获取可写入的日志文件路径：优先返回未超 1MB 的文件，否则新建序号文件
        /// </summary>
        private static string GetLogFilePath(string dir, string baseName)
        {
            // 先尝试基础文件名
            string path = Path.Combine(dir, $"{baseName}.txt");
            if (!File.Exists(path) || new FileInfo(path).Length < MaxLogSize)
                return path;

            // 超过 1MB，找下一个可用序号
            for (int i = 1; ; i++)
            {
                path = Path.Combine(dir, $"{baseName}_{i}.txt");
                if (!File.Exists(path) || new FileInfo(path).Length < MaxLogSize)
                    return path;
            }
        }

        public static void WriteLog(string msg)
        {
            // 2. 加锁：确保同一时间只有一个线程能执行写入逻辑
            lock (locker)
            {
                try
                {
                    // 生成路径：Logs/yyyyMM/dd/
                    string mm = DateTime.Now.ToString("yyyy-MM").Replace("-", "");
                    string dd = DateTime.Now.ToString("dd");
                    string dirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", mm, dd);

                    // 确保目录存在
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // 生成文件路径，超过 1MB 则加序号后缀（_1, _2, ...）
                    string baseName = $"{DateTime.Now:yyyy-MM-dd}_log";
                    string logPath = GetLogFilePath(dirPath, baseName);

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
