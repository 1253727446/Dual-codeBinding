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
                WriteLogs.WriteLog("Complate Receive:" + result_v);
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

        public static bool GetAuxmStockList(string url, string loginId, string clientId, string line, string machineNo, out string did, out string msg)
        {
            did = "";
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                LINE = line,
                STATUS = "3",
                MACHINE_NO = machineNo
            };
            string content = "?method=GetAuxmStockList&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("GetAuxmStockList Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("GetAuxmStockList Receive:" + str_result);

                JObject json = JObject.Parse(str_result);
                string result = json["RESULT"]?.ToString();
                if (result == "PASS")
                {
                    JArray dataList = json["DATA_LIST"] as JArray;
                    if (dataList != null && dataList.Count > 0)
                    {
                        did = dataList[0]["did"]?.ToString() ?? "";
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
                WriteLogs.WriteLog("GetAuxmStockList Error:" + msg);
                return false;
            }
        }

        public static bool TestDataCollect2MainChild(
            string url, string loginId, string clientId,
            string lineNo, string productName, string projectName,
            string shoporderNo, string sn, string testStation,
            string fixtureNo, string version, string tdsName,
            List<TestDataItem> testDataList,
            out string msg)
        {
            msg = "";

            var param = new
            {
                CLIENT_ID = clientId,
                FIXTURE_NO = fixtureNo,
                TEST_RESULT = "PASS",
                VERSION = version,
                HW_VERSION = "",
                LINE_NO = lineNo,
                LOGIN_ID = loginId,
                PRODUCT_NAME = productName,
                PROJECT_NAME = projectName,
                SHOPORDER_NO = shoporderNo,
                SN = sn,
                STARTIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TEST_STATION = testStation,
                TDS_NAME = tdsName,
                TEST_DATA_LIST = testDataList
            };
            string content = "?method=TestDataCollect2MainChild&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("TestDataCollect2MainChild Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("TestDataCollect2MainChild Receive:" + str_result);

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
                WriteLogs.WriteLog("TestDataCollect2MainChild Error:" + msg);
                return false;
            }
        }

        public static bool GetCustomData(string url, string loginId, string clientId, string projectId, string productId, string stationId, string line, string projectName, string productName, out string ruler, out string config, out string msg)
        {
            ruler = "";
            config = "";
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                PROJECT_ID = projectId,
                PRODUCT_ID = productId,
                STATION_ID = stationId,
                LINE = line,
                PROJECT_NAME = projectName,
                PRODUCT_NAME = productName
            };
            string content = "?method=GetCustomData&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();

                WriteLogs.WriteLog("GetCustomData Send:" + url + content);
                string str_result = ht.Get(url + content, 1000);
                WriteLogs.WriteLog("GetCustomData Receive:" + str_result);

                JSON_RW_DLL.JSON.RET.GetCustomData gh = JsonConvert.DeserializeObject<JSON_RW_DLL.JSON.RET.GetCustomData>(str_result);

                if (gh.RESULT == "PASS")
                {
                    foreach (var item in gh.DATA)
                    {
                        if (item.NAME == "SFCRule")
                        {
                            ruler = item.VALUE;
                        }
                        else if (item.NAME == "PolarisConfig")
                        {
                            config = item.VALUE;
                        }
                    }
                    return true;
                }

                msg = str_result;

                return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("GetCustomData Error:" + msg);
                return false;
            }
        }

        public static List<SubMaterialDef> GetLoadSubsByLoadId(string url, string loginId, string clientId, string load_id, string station)
        {
            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                LOAD_ID = load_id,
                STATION = station
            };
            string content = "?method=GetLoadSubsByLoadId&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();

                WriteLogs.WriteLog("GetLoadSubsByLoadId Send:" + url + content);
                string str_result = ht.Get(url + content, 1000);
                WriteLogs.WriteLog("GetLoadSubsByLoadId Receive:" + str_result);

                JSON_RW_DLL.JSON.RET.GetLoadSubsByLoadId gh = JsonConvert.DeserializeObject<JSON_RW_DLL.JSON.RET.GetLoadSubsByLoadId>(str_result);

                var list = new List<SubMaterialDef>();
                if (gh.RESULT == "PASS")
                {
                    foreach (var item in gh.DATA_LIST)
                    {
                        list.Add(new SubMaterialDef
                        {
                            Bydpn = item.bydpn?.ToString(),
                            DidRule = item.didRule?.ToString(),
                            Remarks = item.remarks?.ToString(),
                            Qty = !string.IsNullOrEmpty(item.qty) ? Convert.ToDouble(item.qty) : 0.0,
                            Did = "",
                            Location = item.location?.ToString(),
                            MinSurplus = item.minSurplus != null ? Convert.ToInt32(item.minSurplus) : 0,
                            StopQty = item.stopQty?.ToString(),
                            ClientNo = item.clientNo?.ToString()
                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                WriteLogs.WriteLog("GetLoadSubsByLoadId Error:" + ex.Message);
                return new List<SubMaterialDef>();
            }
        }

        public static bool GetLoadSubsByLoadId(string url, string loginId, string clientId, string load_id, string station, string product, out string byd_pn, out string location, out string remarks, out string msg)
        {
            byd_pn = "";
            location = "";
            remarks = "";
            msg = "";

            var list = GetLoadSubsByLoadId(url, loginId, clientId, load_id, station);
            if (list.Count > 0)
            {
                byd_pn = list[0].Bydpn;
                location = "";
                remarks = list[0].Remarks;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取小件上料信息（最新剩余数量）
        /// </summary>
        public static LoadUpResult GetLoadUpByParams(
            string url, string loginId, string clientId,
            string shoporder, string location, string line)
        {
            var result = new LoadUpResult();
            try
            {
                var param = new
                {
                    LOGIN_ID = loginId,
                    CLIENT_ID = clientId,
                    SHOPORDER = shoporder,
                    LOCATION = location,
                    LINE = line
                };
                string content = "?method=GetLoadUpByParams&param=" + JsonConvert.SerializeObject(param);

                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("GetLoadUpByParams Send:" + url + content);
                string str_result = ht.Get(url + content, 1000);
                WriteLogs.WriteLog("GetLoadUpByParams Receive:" + str_result);

                var json = JObject.Parse(str_result);
                string res = json["RESULT"]?.ToString();
                if (res == "FAIL")
                {
                    result.FailMessage = json["MESSAGE"]?.ToString() ?? "未知错误";
                    return result;
                }

                result.Ok = true;
                var loadUps = json["LoadUps"] as JArray;
                if (loadUps != null && loadUps.Count > 0)
                {
                    result.Found = true;
                    result.QtyResidual = Convert.ToDouble(loadUps[0]["qtyResidual"] ?? 0);
                    result.Did = loadUps[0]["did"]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                WriteLogs.WriteLog("GetLoadUpByParams Error:" + ex.Message);
                result.Ok = false;
            }
            return result;
        }

        /// <summary>
        /// 修改 DID 上料信息（下旧小件）
        /// </summary>
        public static bool UpdateLoadDidInfo(
            string url, string loginId, string clientId,
            string did, string station, string shoporder, string line,
            string state, string loadId, string location, out string msg)
        {
            msg = "";
            try
            {
                var param = new
                {
                    LOGIN_ID = loginId,
                    CLIENT_ID = clientId,
                    DID = did,
                    STATION = station,
                    SHOPORDER = shoporder,
                    LINE = line,
                    STATE = state,
                    LOAD_ID = loadId,
                    LOCATION = location
                };
                string content = "?method=UpdateLoadDidInfo&param=" + JsonConvert.SerializeObject(param);

                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("UpdateLoadDidInfo Send:" + url + content);
                string str_result = ht.Get(url + content, 1000);
                WriteLogs.WriteLog("UpdateLoadDidInfo Receive:" + str_result);

                var json = JObject.Parse(str_result);
                msg = str_result;
                return json["RESULT"]?.ToString() == "PASS";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("UpdateLoadDidInfo Error:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 站位表上料（上新小件码）
        /// </summary>
        public static bool LoadMaterialUp(
            string url, string loginId, string clientId,
            string loadId, string location, string bydpn, string station,
            string line, string shoporder, string did, string remarks,
            string dateCode, out string msg)
        {
            msg = "";
            try
            {
                var param = new
                {
                    LOGIN_ID = loginId,
                    CLIENT_ID = clientId,
                    LOAD_ID = loadId,
                    LOCATION = location,
                    BYDPN = bydpn,
                    STATION = station,
                    LINE = line,
                    SHOPORDER = shoporder,
                    DID = did,
                    QTY = remarks,
                    DATE_CODE = dateCode,
                    REMARK = "小件上料",
                    qty = remarks,
                    QtyResidual = remarks,
                    LOADUP_TYPE = "0"
                };
                string content = "?method=LoadMaterialUp&param=" + JsonConvert.SerializeObject(param);

                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("LoadMaterialUp Send:" + url + content);
                string str_result = ht.Get(url + content, 1000);
                WriteLogs.WriteLog("LoadMaterialUp Receive:" + str_result);

                var json = JObject.Parse(str_result);
                msg = str_result;
                return json["RESULT"]?.ToString() == "PASS";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("LoadMaterialUp Error:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 扣料，扣除小件数量
        /// </summary>
        public static bool Binding(
            string url, string loginId, string clientId,
            string sfc, string shopOrder, string station, string loadId,
            string line, string projectName, string productName,
            string multiple, string schedulingId, string allowNegative,
            out string msg)
        {
            msg = "";
            try
            {
                var param = new
                {
                    LOGIN_ID = loginId,
                    CLIENT_ID = clientId,
                    SFC = sfc,
                    SHOP_ORDER = shopOrder,
                    STATION = station,
                    LOAD_ID = loadId,
                    LINE = line,
                    PROJECT_NAME = projectName,
                    PRODUCT_NAME = productName,
                    MULTIPLE = multiple,
                    SCHEDULING_ID = schedulingId,
                    ALLOW_NEGATIVE = allowNegative
                };
                string content = "?method=Binding&param=" + JsonConvert.SerializeObject(param);

                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("Binding Send:" + url + content);
                string str_result = ht.Get(url + content, 1000);
                WriteLogs.WriteLog("Binding Receive:" + str_result);

                var json = JObject.Parse(str_result);
                msg = str_result;
                return json["RESULT"]?.ToString() == "PASS";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                WriteLogs.WriteLog("Binding Error:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 上传 Config 绑定结果到 MES
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
        /// 根据当前 StationID 反查上一工站信息
        /// </summary>
        public static bool GetAboutStationByStationId(
            string url, string loginId, string clientId, string stationId,
            out string prevName, out string prevId, out string msg)
        {
            prevName = "";
            prevId = "";
            msg = "";

            var param = new
            {
                LOGIN_ID = loginId,
                CLIENT_ID = clientId,
                STATION_ID = stationId
            };
            string content = "?method=GetAboutStationByStationId&param=" + JsonConvert.SerializeObject(param);

            try
            {
                HttpUitls ht = new HttpUitls();
                WriteLogs.WriteLog("GetAboutStationByStationId Send:" + url + content);
                string str_result = ht.Get(url + content, 2000);
                WriteLogs.WriteLog("GetAboutStationByStationId Receive:" + str_result);

                JObject json = JObject.Parse(str_result);
                string result = json["RESULT"]?.ToString();
                if (result == "PASS")
                {
                    JToken prev = json["PREVIOUS"];
                    if (prev != null)
                    {
                        prevName = prev["name"]?.ToString() ?? "";
                        prevId = prev["id"]?.ToString() ?? "";
                    }
                    return !string.IsNullOrEmpty(prevName);
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
                WriteLogs.WriteLog("GetAboutStationByStationId Error:" + msg);
                return false;
            }
        }

    }
}