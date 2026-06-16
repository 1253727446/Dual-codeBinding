using WindowsFormsApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsFormsApp;

namespace MagnetAssemblyClient.Services
{
    public class SubMaterialService
    {
        private readonly string _dataFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "sub_materials.json");
        private readonly string _historyFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "history.csv");

        public List<ActiveSubMaterial> ActiveSubs { get; private set; } = new List<ActiveSubMaterial>();

        public SubMaterialService()
        {
            LoadFromFile();
        }

        /// <summary>
        /// 从本地文件加载已保存的小件信息
        /// </summary>
        public void LoadFromFile()
        {
            if (File.Exists(_dataFile))
            {
                var json = File.ReadAllText(_dataFile);
                ActiveSubs = JsonConvert.DeserializeObject<List<ActiveSubMaterial>>(json) ?? new List<ActiveSubMaterial>();
            }
        }

        /// <summary>
        /// 保存到本地文件
        /// </summary>
        public void SaveToFile()
        {
            var json = JsonConvert.SerializeObject(ActiveSubs, Formatting.Indented);
            File.WriteAllText(_dataFile, json);
        }

        /// <summary>
        /// 记录上料历史
        /// </summary>
        public void AddHistory(string bydpn, string did)
        {
            var line = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss},{bydpn},{did}";
            File.AppendAllText(_historyFile, line + System.Environment.NewLine);
        }

        /// <summary>
        /// 判断小件码是否已被绑定（可用于重复检测）
        /// </summary>
        public bool IsDidAlreadyBound(string did)
        {
            return ActiveSubs.Any(s => s.Did == did);
        }

        /// <summary>
        /// 根据接口返回的定义初始化小件列表（仅首次，或保留已有绑定）
        /// </summary>
        public void InitializeFromDefs(List<SubMaterialDef> defs)
        {
            // 如果本地已有数据，则保留 Did，但更新用量和默认数量（如果接口有变化）
            foreach (var def in defs)
            {
                var existing = ActiveSubs.FirstOrDefault(a => a.Bydpn == def.Bydpn);
                if (existing == null)
                {
                    // 新小件，数量使用 remarks，Did 为空
                    ActiveSubs.Add(new ActiveSubMaterial
                    {
                        Bydpn = def.Bydpn,
                        Did = "",
                        RemainingQty = double.TryParse(def.Remarks, out double qty) ? qty : 2000,
                        UsageQty = def.Qty
                    });
                }
                else
                {
                    // 更新可能变化的用量和默认数量（但保留已绑定的 Did 和剩余数量）
                    existing.UsageQty = def.Qty;
                    // 如果剩余数量为0或负数，可以重新填充默认数量，但保留手动修改
                    if (existing.RemainingQty <= 0)
                        existing.RemainingQty = double.TryParse(def.Remarks, out double d) ? d : 2000;
                }
            }

            // 移除接口中已不存在的小件
            ActiveSubs.RemoveAll(a => !defs.Any(d => d.Bydpn == a.Bydpn));
            SaveToFile();
        }

        /// <summary>
        /// 绑定小件码到指定小件名称
        /// </summary>
        public void BindDid(string bydpn, string did)
        {
            var sub = ActiveSubs.FirstOrDefault(a => a.Bydpn == bydpn);
            if (sub != null)
            {
                sub.Did = did;
                SaveToFile();
                AddHistory(bydpn, did);
            }
        }
    }
}