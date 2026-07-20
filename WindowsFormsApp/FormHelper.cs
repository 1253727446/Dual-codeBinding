using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WindowsFormsApp
{
    public class FormHelper
    {
        /// <summary>产品入站</summary>
        public static bool Start(string url, string loginId, string clientId, string sfc, string stationName, string line, string shoporder, string schingId, out string msg)
        {
            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                STATION_NAME = stationName,
                LINE = line,
                SHOPORDER = shoporder,
                SCHEDULING_ID = schingId
            };
            string content = "?method=Start&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls utel = new HttpUitls();
                WriteLogs.WriteLog("Start Send:" + url + content);
                string result_v = utel.Get(url + content, 2000);
                WriteLogs.WriteLog("Start Receive:" + result_v);
                msg = result_v;
                return result_v.Contains("\"RESULT\":\"PASS\"");
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
                WriteLogs.WriteLog("Start Error:" + msg);
                return false;
            }
        }

        /// <summary>良品过站完成</summary>
        public static bool Complete(string url, string loginId, string clientId, string sfc, string stationid, string schingId, string remark, string time, out string msg)
        {
            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                STATION_ID = stationid,
                SCHEDULING_ID = schingId,
                REMARK = remark,
                time = time
            };
            string content = "?method=Complete&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls utel = new HttpUitls();
                WriteLogs.WriteLog("Complete Send:" + url + content);
                string result_v = utel.Get(url + content, 2000);
                WriteLogs.WriteLog("Complete Receive:" + result_v);
                msg = result_v;
                return result_v.Contains("\"RESULT\":\"PASS\"");
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
                WriteLogs.WriteLog("Complete Error:" + msg);
                return false;
            }
        }

        /// <summary>获取自定义数据（SFCRule / SubSFCRule / NumberStore 等）</summary>
        public static bool GetCustomData(string url, string loginId, string clientId, string productId, out List<GetCustomDataItem> dataList, out string msg)
        {
            dataList = null;
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                PRODUCT_ID = productId
            };
            string content = "?method=GetCustomData&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("GetCustomData Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("GetCustomData Receive:" + str_result);

                JObject json = JObject.Parse(str_result);
                string result = json["RESULT"]?.ToString();
                if (result == "PASS")
                {
                    JArray arr = json["DATA"] as JArray;
                    if (arr != null)
                    {
                        dataList = arr.ToObject<List<GetCustomDataItem>>();
                    }
                    return true;
                }
                else
                {
                    msg = json["MESSAGE"]?.ToString() ?? "接口返回失败";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("GetCustomData Error:" + msg);
                return false;
            }
        }

        /// <summary>
        /// 查询纸码是否已被使用。
        /// 返回 isUsed：NEWSFC_DATA 或 OLDSFC_DATA 任一不为 null 即为已使用。
        /// </summary>
        public static bool GetSerializeData(string url, string loginId, string clientId, string sfc, out bool isUsed, out string msg)
        {
            isUsed = true;
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc
            };
            string content = "?method=GetSerializeData&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("GetSerializeData Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("GetSerializeData Receive:" + str_result);

                JObject json = JObject.Parse(str_result);
                string result = json["RESULT"]?.ToString();
                if (result == "PASS")
                {
                    JToken newSfc = json["NEWSFC_DATA"];
                    JToken oldSfc = json["OLDSFC_DATA"];
                    // 两者都为 null 才算未使用
                    bool newIsNull = newSfc == null || newSfc.Type == JTokenType.Null;
                    bool oldIsNull = oldSfc == null || oldSfc.Type == JTokenType.Null;
                    isUsed = !(newIsNull && oldIsNull);
                    return true;
                }
                else
                {
                    msg = json["MESSAGE"]?.ToString() ?? "接口返回失败";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("GetSerializeData Error:" + msg);
                return false;
            }
        }

        /// <summary>SFC 序列化绑定（镭雕码 + 纸码一对一绑定）</summary>
        public static bool Serializable(string url, string loginId, string clientId, string sfc, string schedulingId, string boardCount, string stationId, List<string> newSfcList, string sfcState, out string msg)
        {
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                SCHEDULING_ID = schedulingId,
                BOARD_COUNT = boardCount,
                STATION_ID = stationId,
                NEW_SFC_LIST = newSfcList,
                SFC_STATE = sfcState
            };
            string content = "?method=Serializable&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("Serializable Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("Serializable Receive:" + str_result);

                JObject json = JObject.Parse(str_result);
                string result = json["RESULT"]?.ToString();
                if (result == "PASS")
                {
                    return true;
                }
                else
                {
                    msg = json["MESSAGE"]?.ToString() ?? "接口返回失败";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("Serializable Error:" + msg);
                return false;
            }
        }
    }
}
