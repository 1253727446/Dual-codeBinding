using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WindowsFormsApp
{
    public class FormHelper
    {
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

        public static bool Complete(string url, string loginId, string clientId, string sfc, string stationid, string schingId, string remark, out string msg)
        {
            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                STATION_ID = stationid,
                SCHEDULING_ID = schingId,
                REMARK = remark
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

        /// <summary>
        /// 根据 SFC 获取 SFC Log 列表，用于校验条码是否在正确工站流转
        /// </summary>
        public static bool GetSfcLogListByParam(
            string url, string loginId, string clientId, string sfc,
            out JArray dataList, out string msg)
        {
            dataList = null;
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc
            };
            string content = "?method=GetSfcLogListByParam&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("GetSfcLogListByParam Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("GetSfcLogListByParam Receive:" + str_result);

                JObject json = JObject.Parse(str_result);
                string result = json["RESULT"]?.ToString();
                if (result == "PASS")
                {
                    dataList = json["DATA"] as JArray;
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
                WriteLogs.WriteLog("GetSfcLogListByParam Error:" + msg);
                return false;
            }
        }

        /// <summary>
        /// 上报数据绑定结果到 MES（CCD 结果 / 业务数据）
        /// </summary>
        public static bool AddSfcKey(
            string url, string loginId, string clientId,
            string sfc, string stationId, string stationName,
            string shopOrder, string dataName, string dataValue,
            string projectId, string productId,
            out string msg)
        {
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                STATION_ID = stationId,
                STATION_NAME = stationName,
                SHOPORDER = shopOrder,
                DATA_NAME = dataName,
                DATA_VALUE = dataValue,
                PROJECT_ID = projectId,
                PRODUCT_ID = productId
            };
            string content = "?method=AddSfcKey&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("AddSfcKey Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("AddSfcKey Receive:" + str_result);

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
                WriteLogs.WriteLog("AddSfcKey Error:" + msg);
                return false;
            }
        }

        /// <summary>
        /// 不合格品过站完成（NC Complete）
        /// </summary>
        public static bool NcComplete(
            string url, string loginId, string clientId,
            string sfc, string stationId, string ncCode,
            string ncContext, string ncType, string schedulingId,
            out string msg)
        {
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                STATION_ID = stationId,
                NC_CODE = ncCode,
                NC_CONTEXT = ncContext,
                NC_TYPE = ncType,
                SCHEDULING_ID = schedulingId
            };
            string content = "?method=NcComplete&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("NcComplete Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("NcComplete Receive:" + str_result);

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
                WriteLogs.WriteLog("NcComplete Error:" + msg);
                return false;
            }
        }

        /// <summary>
        /// 变更 SFC 当前工站
        /// </summary>
        public static bool ChangeSfcStation(
            string url, string loginId, string clientId,
            string sfc, string newStationName, out string msg)
        {
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                SFC = sfc,
                NEW_STATION_NAME = newStationName
            };
            string content = "?method=ChangeSfcStation&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("ChangeSfcStation Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("ChangeSfcStation Receive:" + str_result);

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
                WriteLogs.WriteLog("ChangeSfcStation Error:" + msg);
                return false;
            }
        }

    }
}