
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Threading;

namespace WindowsFormsApp
{
    public class HttpUitls
    {
        public string Get(string Url, int TimeOut)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = null;
            request.KeepAlive = false;
            request.Method = "GET";
            request.ContentType = "application/json; charset=UTF-8";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Timeout = TimeOut;
            request.ServicePoint.ConnectionLimit = 100;
            request.ServicePoint.ConnectionLeaseTimeout = 5000;

            // 兜底定时器：强制覆盖 DNS / TCP 连接阶段，确保总超时 ≤ TimeOut
            Timer abortTimer = null;
            if (TimeOut > 0)
            {
                abortTimer = new Timer(_ =>
                {
                    try { request.Abort(); }
                    catch { }
                }, null, TimeOut, Timeout.Infinite);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream myResponseStream = response.GetResponseStream())
                using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8))
                {
                    string retString = myStreamReader.ReadToEnd();
                    retString = retString.Replace(@"\", "");
                    return retString;
                }
            }
            finally
            {
                if (abortTimer != null)
                {
                    abortTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    abortTimer.Dispose();
                }
                try { request.Abort(); } catch { }
            }
        }
        public string Post(string Url, string Data, string Referer)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.Referer = Referer;
            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;
            Stream myResponseStream = request.GetRequestStream();
            myResponseStream.Write(bytes, 0, bytes.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            // retString.Replace("\\", "");
            myStreamReader.Close();
            myResponseStream.Close();

            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }
            //NlogHelper.WriteLog("PlcConn_Tcp", "Trace", retString, "Post", "");
            return retString;
        }
    }
}
