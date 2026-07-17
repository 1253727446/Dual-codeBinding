using ReadWriteLogIni_NameSpace;
using System.Collections.Generic;

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
            config["CCD"] = ReadCcd();
            return config;
        }

        private Dictionary<string, string> ReadMesConfig()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\mes_config.ini");

            TryReadValue("SYSTEM", "JSONURL", "JSONURL", dic);
            TryReadValue("Config", "LoginID", "LoginID", dic);
            TryReadValue("Config", "ClientID", "ClientID", dic);
            TryReadValue("Config", "PROJECT_ID", "PROJECT_ID", dic);
            TryReadValue("Config", "PRODUCT_ID", "PRODUCT_ID", dic);
            TryReadValue("Config", "StationID", "StationID", dic);
            TryReadValue("Config", "Line", "Line", dic);
            TryReadValue("Config", "Operation", "Operation", dic);
            TryReadValue("Config", "SapShoporder", "SapShoporder", dic);
            TryReadValue("Config", "Remark", "Remark", dic);
            TryReadValue("Config", "SchedulingID", "SchedulingID", dic);

            return dic;
        }

        private Dictionary<string, string> ReadSetting()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\setting.ini");

            TryReadValue("SCAN", "IP", "scanip", dic);
            TryReadValue("SCAN", "port", "scanport", dic);
            TryReadValue("PARAM", "ccdDataName", "ccdDataName", dic);

            return dic;
        }

        private Dictionary<string, string> ReadCcd()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\setting.ini");

            TryReadValue("CCD", "IP", "ip", dic);
            TryReadValue("CCD", "port", "port", dic);

            return dic;
        }

        private Dictionary<string, string> ReadSoftware()
        {
            var dic = new Dictionary<string, string>();
            iniRead.IniFile(@"\cfg\setting.ini");

            TryReadValue("SOFTWARE", "addSfcKeyDataName", "addSfcKeyDataName", dic);
            TryReadValue("SOFTWARE", "logStation", "logStation", dic);

            return dic;
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
