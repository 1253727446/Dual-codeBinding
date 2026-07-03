using ReadWriteLogIni_NameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WindowsFormsApp
{
    public class ConfigService
    {
        private readonly ReadWriteLogIni iniRead = new ReadWriteLogIni();

        public Dictionary<string, Dictionary<string, string>> LoadAllConfig()
        {
            var config = new Dictionary<string, Dictionary<string, string>>();
            config["Config"] = ReadMesConfig();
            config["Setting"] = ReadSetting();
            config["SOFTWARE"] = ReadSoftware();
            return config;
        }

        private Dictionary<string, string> ReadMesConfig()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\mes_config.ini");

            TryReadValue("SYSTEM", "JSONURL", "JSONURL", dic);
            TryReadValue("Config", "LoginID", "LoginID", dic);
            TryReadValue("Config", "Load_ID", "Load_ID", dic);
            TryReadValue("Config", "ClientID", "ClientID", dic);
            TryReadValue("Config", "PROJECT_ID", "PROJECT_ID", dic);
            TryReadValue("Config", "PRODUCT_ID", "PRODUCT_ID", dic);
            TryReadValue("Config", "StationID", "StationID", dic);
            TryReadValue("Config", "Line", "Line", dic);
            TryReadValue("Config", "Operation", "Operation", dic);
            TryReadValue("Config", "PRODUCT", "PRODUCT", dic);
            TryReadValue("Config", "SapShoporder", "SapShoporder", dic);
            TryReadValue("Config", "Resource", "Resource", dic);
            TryReadValue("Config", "Remark", "Remark", dic);
            TryReadValue("Config", "SchedulingID", "SchedulingID", dic);
            TryReadValue("Config", "StationName", "StationName", dic);
            TryReadValue("Config", "PROJECT", "PROJECT", dic);
            TryReadValue("Config", "TraceStationId", "TraceStationId", dic);

            return dic;
        }

        private Dictionary<string, string> ReadSetting()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\setting.ini");

            TryReadValue("PLC", "IP", "plcip", dic);
            TryReadValue("PLC", "PORT", "plcport", dic);
            TryReadValue("PLC", "startscan", "startscan", dic);
            TryReadValue("PLC", "timeout", "timeout", dic);
            TryReadValue("PLC", "mesresult", "mesresult", dic);
            TryReadValue("PLC", "heartbeat", "heartbeat", dic);
            TryReadValue("SCAN", "IP", "scanip", dic);
            TryReadValue("SCAN", "port", "scanport", dic);
            TryReadValue("SCAN", "startorder", "startorder", dic);
            TryReadValue("PARAM", "MACHINE_NO", "MACHINE_NO", dic);
            TryReadValue("PARAM", "FIXTURE_NO", "FIXTURE_NO", dic);

            return dic;
        }

        private Dictionary<string, string> ReadSoftware()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\setting.ini");

            TryReadValue("SOFTWARE", "startscanswitch", "startscanswitch", dic);
            TryReadValue("SOFTWARE", "glueCheck", "glueCheck", dic);
            TryReadValue("SOFTWARE", "plcmode", "plcmode", dic);
            TryReadValue("SOFTWARE", "Config", "Config", dic);
            TryReadValue("SOFTWARE", "selectedConfig", "selectedConfig", dic);
            TryReadValue("SOFTWARE", "addSfcKeyDataName", "addSfcKeyDataName", dic);
            TryReadValue("SOFTWARE", "machineNoswitch", "machineNoswitch", dic);

            return dic;
        }

        /// <summary>将 PLC 连接模式写回 setting.ini 的 [SOFTWARE] 节</summary>
        public void SavePlcMode(string mode)
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cfg", "setting.ini");
            if (!File.Exists(iniPath)) return;

            var lines = File.ReadAllLines(iniPath).ToList();
            int softIdx = lines.FindIndex(l => l.Trim().Equals("[SOFTWARE]", StringComparison.OrdinalIgnoreCase));
            if (softIdx < 0)
            {
                lines.Add("");
                lines.Add("[SOFTWARE]");
                lines.Add("plcmode=" + mode);
            }
            else
            {
                int valIdx = lines.FindIndex(softIdx + 1, l => l.Trim().StartsWith("plcmode=", StringComparison.OrdinalIgnoreCase));
                if (valIdx >= 0)
                    lines[valIdx] = "plcmode=" + mode;
                else
                    lines.Insert(softIdx + 1, "plcmode=" + mode);
            }
            File.WriteAllLines(iniPath, lines);
        }

        /// <summary>将选中的 Config 值写回 setting.ini 的 [SOFTWARE] 节</summary>
        public void SaveSelectedConfig(string configValue)
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cfg", "setting.ini");
            if (!File.Exists(iniPath)) return;

            var lines = File.ReadAllLines(iniPath).ToList();
            int softIdx = lines.FindIndex(l => l.Trim().Equals("[SOFTWARE]", StringComparison.OrdinalIgnoreCase));
            if (softIdx < 0)
            {
                lines.Add("");
                lines.Add("[SOFTWARE]");
                lines.Add("selectedConfig=" + configValue);
            }
            else
            {
                int valIdx = lines.FindIndex(softIdx + 1, l => l.Trim().StartsWith("selectedConfig=", StringComparison.OrdinalIgnoreCase));
                if (valIdx >= 0)
                    lines[valIdx] = "selectedConfig=" + configValue;
                else
                    lines.Insert(softIdx + 1, "selectedConfig=" + configValue);
            }
            File.WriteAllLines(iniPath, lines);
        }

        private void TryReadValue(string section, string key, string dictKey, Dictionary<string, string> dic)
        {
            string value = "";
            int state = iniRead.IniReadValue(section, key, ref value);
            if (state != 0)
            {
                dic[dictKey] = value;
            }
        }
    }
}
