using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class MainForm : UIForm
    {
        // ========== 过站计数器持久化路径 ==========
        private readonly string _countersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "counters.json");

        // ========== 配置与运行参数 ==========
        /// <summary>配置参数字典（Config / Setting 两节）</summary>
        private Dictionary<string, Dictionary<string, string>> g_DicMESConfig;
        /// <summary>缓存的 MES 接口地址 / 登录 ID / 客户端 ID，避免反复查字典</summary>
        private string _mesUrl;
        private string _loginId;
        private string _clientId;

        // ========== 过站计数器 ==========
        /// <summary>过站失败计数</summary>
        private int _failCount = 0;
        /// <summary>当班过站产出（只计PASS）</summary>
        private int _currentShiftOutput = 0;
        /// <summary>上班级过站产出</summary>
        private int _lastShiftOutput = 0;
        /// <summary>当前班次开始时间</summary>
        private DateTime _shiftStartTime = DateTime.MinValue;
        /// <summary>上次换班时间（防止同一班次重复触发）</summary>
        private DateTime _lastShiftChangeTime = DateTime.MinValue;
        /// <summary>白班开始小时（默认8）</summary>
        private int _dayShiftStartHour = 8;
        /// <summary>夜班开始小时（默认20）</summary>
        private int _nightShiftStartHour = 20;
        /// <summary>换班检查定时器</summary>
        private System.Windows.Forms.Timer _shiftTimer;
        /// <summary>过站成功音效</summary>
        private SoundPlayer _passSound;
        /// <summary>过站失败音效</summary>
        private SoundPlayer _failSound;

        /// <summary>后台任务取消令牌</summary>
        private CancellationTokenSource _cts;

        // ========== 双码绑定状态管理 ==========
        private enum BindState { Waiting, Processing }
        private BindState _state = BindState.Waiting;
        /// <summary>当前已扫描的镭雕二维码</summary>
        private string _currentLaserCode;
        /// <summary>GetCustomData 返回的规则缓存</summary>
        private List<GetCustomDataItem> _customData;
        /// <summary>镭雕二维码正则规则（SFCRule）</summary>
        private string _sfcRule;
        /// <summary>纸码正则规则（SubSFCRule）</summary>
        private string _subSfcRule;
        /// <summary>PASS/FAIL 结果展示 2 秒后自动清除</summary>
        private System.Windows.Forms.Timer _resultTimer;
        /// <summary>绑定流程中断恢复文件路径</summary>
        private readonly string _statePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "state.json");

        /// <summary>
        /// 构造函数：初始化组件、加载全部配置
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            g_DicMESConfig = new ConfigService().LoadAllConfig();
            _cts = new CancellationTokenSource();
            _passSound = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pass.wav"));
            _failSound = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alert.wav"));

            // 结果展示定时器：PASS/FAIL 显示 2 秒后自动回到等待状态
            _resultTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            _resultTimer.Tick += (s, e) => { _resultTimer.Stop(); EnterWaitingState(); };

            // 注册 USB 扫码枪回车事件（键盘模式）
            SFC_UITextBox.KeyDown += SFC_UITextBox_KeyDown;
            uiTextBox1.KeyDown += uiTextBox1_KeyDown;

            // 日志框不隐藏选区，保证 ScrollToCaret 始终有效
            uiRichTextBox1.HideSelection = false;

            // 注册重置按钮
            uiButton1.Click += uiButton1_Click;
        }

        /// <summary>
        /// 窗体加载：缓存配置 → 初始化参数 → 加载计数器 → 初始化班次 → 启动定时器 → 注册关闭事件
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            CacheConfig();               // 缓存 MES 接口 URL / LoginID / ClientID
            InitSettings();              // 读取设备、工站、班次等本地配置
            LoadCounters();              // 从 counters.json 恢复过站计数器
            InitializeShiftState();      // 初始化当前班次状态
            StartShiftTimer();           // 启动换班检查定时器（8:00 / 20:00）
            CheckRecoveryState();        // 检查是否有未完成的绑定流程需要恢复
            this.FormClosing += MainForm_FormClosing;  // 注册关闭事件释放资源
            this.Shown += MainForm_Shown;               // 窗体完全显示后确保焦点
        }

        /// <summary>窗体完全显示后确保 SFC_UITextBox 获得焦点</summary>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // 如果当前是等待状态，强制焦点到镭雕码输入框
            if (_state == BindState.Waiting)
            {
                SFC_UITextBox.Focus();
                SFC_UITextBox.Select();
            }
        }

        /// <summary>
        /// 缓存 MES 接口常用的三个配置值（URL / LoginID / ClientID）
        /// </summary>
        private void CacheConfig()
        {
            _mesUrl = g_DicMESConfig["Config"]["JSONURL"];
            _loginId = g_DicMESConfig["Config"]["LoginID"];
            _clientId = g_DicMESConfig["Config"]["ClientID"];
        }

        /// <summary>
        /// 读取设备连接配置，并填充顶部信息栏标签
        /// </summary>
        private void InitSettings()
        {
            var cfg = g_DicMESConfig["Config"];
            var setting = g_DicMESConfig["Setting"];

            // 顶部信息栏：静态标签保持设计器默认值，动态值标签填入配置
            uiLabel9.Text = cfg.ContainsKey("PROJECT") ? cfg["PROJECT"] : "";            // 项目
            uiLabel11.Text = cfg.ContainsKey("Line") ? cfg["Line"] : "";                 // 线体
            uiLabel13.Text = cfg.ContainsKey("Resource") ? cfg["Resource"] : "";         // 工单
            uiLabel15.Text = cfg.ContainsKey("Operation") ? cfg["Operation"] : "";       // 工站

            // 班次配置（默认白班8:00-20:00，夜班20:00-次日8:00）
            if (setting.ContainsKey("dayShiftStart"))
                int.TryParse(setting["dayShiftStart"], out _dayShiftStartHour);
            if (setting.ContainsKey("nightShiftStart"))
                int.TryParse(setting["nightShiftStart"], out _nightShiftStartHour);
        }

        /// <summary>
        /// 从 counters.json 恢复过站计数器
        /// </summary>
        private void LoadCounters()
        {
            try
            {
                if (File.Exists(_countersPath))
                {
                    var json = File.ReadAllText(_countersPath);
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    _failCount = (int?)data.fail ?? 0;
                    _currentShiftOutput = (int?)data.currentShiftOutput ?? 0;
                    _lastShiftOutput = (int?)data.lastShiftOutput ?? 0;
                    string savedTime = (string)data.shiftStartTime;
                    if (!string.IsNullOrEmpty(savedTime) && DateTime.TryParse(savedTime, out DateTime parsedTime))
                        _shiftStartTime = parsedTime;

                    FailCount.Text = _failCount.ToString();
                    uiLabel4.Text = _currentShiftOutput.ToString();
                    OutPut.Text = _lastShiftOutput.ToString();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 保存过站计数器到 counters.json
        /// </summary>
        private void SaveCounters()
        {
            try
            {
                var data = new
                {
                    fail = _failCount,
                    currentShiftOutput = _currentShiftOutput,
                    lastShiftOutput = _lastShiftOutput,
                    shiftStartTime = _shiftStartTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
                File.WriteAllText(_countersPath, JsonConvert.SerializeObject(data));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 启动换班检查定时器，每 30 秒检查一次是否到达白班/夜班换班时间
        /// </summary>
        private void StartShiftTimer()
        {
            _shiftTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _shiftTimer.Tick += (s, e) => CheckShiftChange();
            _shiftTimer.Start();
        }

        /// <summary>
        /// 检查当前时间是否到达换班点（白班开始 / 夜班开始），
        /// 到达时将当班产出转移到上班产出，清零当班产出
        /// </summary>
        private void CheckShiftChange()
        {
            DateTime now = DateTime.Now;
            int hour = now.Hour;

            bool isDayShiftStart = (hour == _dayShiftStartHour);
            bool isNightShiftStart = (hour == _nightShiftStartHour);

            if (!isDayShiftStart && !isNightShiftStart) return;

            // 防止同一班次重复触发（30分钟内只触发一次）
            if ((now - _lastShiftChangeTime).TotalMinutes < 30) return;

            _lastShiftChangeTime = now;

            if (this.InvokeRequired)
                this.BeginInvoke(new Action(DoShiftChange));
            else
                DoShiftChange();
        }

        private void DoShiftChange()
        {
            _lastShiftOutput = _currentShiftOutput;
            OutPut.Text = _lastShiftOutput.ToString();
            _currentShiftOutput = 0;
            uiLabel4.Text = "0";
            _shiftStartTime = DateTime.Now;
            SaveCounters();
            AddLogMessage($"换班：本班产出 {_lastShiftOutput}，当班产出已清零。");
        }

        /// <summary>
        /// 窗体关闭时释放定时器及保存状态
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 如果在加工中，保存恢复状态
            if (_state == BindState.Processing && !string.IsNullOrEmpty(_currentLaserCode))
                SaveRecoveryState();

            try { _cts?.Cancel(); } catch (ObjectDisposedException) { }
            try { _cts?.Dispose(); } catch (ObjectDisposedException) { }
            _shiftTimer?.Stop();
            _shiftTimer?.Dispose();
            _resultTimer?.Stop();
            _resultTimer?.Dispose();
        }

        // ============================================================
        // 双码绑定 — 状态管理
        // ============================================================

        /// <summary>进入《等待》状态：清空输入、镭雕码获得焦点、纸码置灰</summary>
        private void EnterWaitingState()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(EnterWaitingState));
                return;
            }

            _state = BindState.Waiting;
            _currentLaserCode = null;
            _customData = null;
            _sfcRule = null;
            _subSfcRule = null;
            _resultTimer.Stop();

            SFC_UITextBox.Text = "";
            SFC_UITextBox.Enabled = true;
            this.ActiveControl = SFC_UITextBox;
            SFC_UITextBox.Focus();
            SFC_UITextBox.Select();

            uiTextBox1.Text = "";
            uiTextBox1.Enabled = false;

            uiLabel2.Text = "等待中";
            uiLabel2.BackColor = Color.DodgerBlue;
            uiLabel2.ForeColor = Color.White;

            ClearRecoveryState();
        }

        /// <summary>进入《加工中》状态：锁定镭雕码、纸码获得焦点</summary>
        private void EnterProcessingState()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(EnterProcessingState));
                return;
            }

            _state = BindState.Processing;

            SFC_UITextBox.Enabled = false;          // 锁定镭雕码

            uiTextBox1.Text = "";
            uiTextBox1.Enabled = true;              // 启用纸码输入
            this.ActiveControl = uiTextBox1;
            uiTextBox1.Focus();
            uiTextBox1.Select();

            uiLabel2.Text = "请扫描纸码";
            uiLabel2.BackColor = Color.DodgerBlue;
            uiLabel2.ForeColor = Color.White;

            SaveRecoveryState();
        }

        /// <summary>展示 PASS 结果：绿色背景 + 音效，2 秒后回到等待</summary>
        private void ShowPass()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(ShowPass));
                return;
            }

            uiLabel2.Text = "PASS  ✓";
            uiLabel2.BackColor = Color.Green;
            uiLabel2.ForeColor = Color.White;
            try { _passSound?.Play(); } catch { }

            _currentShiftOutput++;
            uiLabel4.Text = _currentShiftOutput.ToString();
            SaveCounters();

            ClearRecoveryState();
            _resultTimer.Start();
        }

        /// <summary>展示 FAIL 结果：红色背景 + 错误信息 + 音效，2 秒后回到等待</summary>
        private void ShowFail(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ShowFail(message)));
                return;
            }

            uiLabel2.Text = "FAIL  ✗";
            uiLabel2.BackColor = Color.Red;
            uiLabel2.ForeColor = Color.White;
            try { _failSound?.Play(); } catch { }

            _failCount++;
            SaveCounters();
            FailCount.Text = _failCount.ToString();

            AddLogMessage(message, Color.Red);
            _resultTimer.Start();
        }

        // ============================================================
        // 双码绑定 — 业务流程
        // ============================================================

        /// <summary>
        /// 处理镭雕二维码扫描：
        /// GetCustomData 获取规则 → SFCRule 正则校验 → Start 入站 → 进入加工中
        /// </summary>
        private void HandleLaserCode(string laserCode)
        {
            try
            {
                // 1. 获取自定义数据（规则）
                string productId = g_DicMESConfig["Config"]["PRODUCT_ID"];
                bool dataOk = FormHelper.GetCustomData(
                    _mesUrl, _loginId, _clientId, productId,
                    out List<GetCustomDataItem> dataList, out string dataMsg);

                if (!dataOk || dataList == null || dataList.Count == 0)
                {
                    AddLogMessage($"GetCustomData失败：{dataMsg}", Color.Red);
                    ShowFail(dataMsg);
                    return;
                }

                _customData = dataList;

                // 提取规则
                _sfcRule = null;
                _subSfcRule = null;
                foreach (var item in dataList)
                {
                    if (item.NAME == "SFCRule") _sfcRule = item.VALUE;
                    else if (item.NAME == "SubSFCRule") _subSfcRule = item.VALUE;
                }

                if (string.IsNullOrEmpty(_sfcRule))
                {
                    ShowFail("未配置SFCRule规则");
                    return;
                }
                AddLogMessage($"SFCRule=[{_sfcRule}]", Color.Blue);

                // 2. 镭雕码格式校验（? → 正则 . 匹配任意单字符，^$ 全匹配）
                try
                {
                    // 将 SFCRule 中的 ? 转为正则 . , 转义已有正则特殊字符, 加全匹配锚点
                    string pattern = "^" + Regex.Escape(_sfcRule).Replace(@"\?", ".") + "$";
                    if (!Regex.IsMatch(laserCode, pattern))
                    {
                        ShowFail("镭雕码格式错误，请重新输入");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ShowFail("SFCRule正则解析异常：" + ex.Message);
                    return;
                }
                AddLogMessage("镭雕码格式校验通过", Color.Green);

                // 3. MES Start 入站
                _currentLaserCode = laserCode;
                if (!CallStart(laserCode))
                {
                    _currentLaserCode = null;
                    ShowFail("Start失败");
                    return;
                }
                AddLogMessage("Start成功", Color.Green);

                // 4. 进入加工中状态
                BeginInvoke(new Action(() => EnterProcessingState()));
            }
            catch (Exception ex)
            {
                AddLogMessage("镭雕码处理异常：" + ex.Message, Color.Red);
                ShowFail(ex.Message);
            }
        }

        /// <summary>
        /// 处理纸码扫描：
        /// SubSFCRule 正则校验 → GetSerializeData 查重 → Serializable 绑定 → Complete 出站
        /// </summary>
        private void HandlePaperCode(string paperCode)
        {
            try
            {
                if (string.IsNullOrEmpty(_subSfcRule))
                {
                    ShowFail("未配置SubSFCRule规则");
                    return;
                }

                // 1. 纸码格式校验（? → 正则 . 匹配任意单字符，^$ 全匹配）
                try
                {
                    string pattern = "^" + Regex.Escape(_subSfcRule).Replace(@"\?", ".") + "$";
                    if (!Regex.IsMatch(paperCode, pattern))
                    {
                        ShowFail("纸码格式错误，请重新输入");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ShowFail("SubSFCRule正则解析异常：" + ex.Message);
                    return;
                }
                AddLogMessage("纸码格式校验通过", Color.Green);

                // 2. GetSerializeData 查重
                bool chkOk = FormHelper.GetSerializeData(
                    _mesUrl, _loginId, _clientId, paperCode,
                    out bool isUsed, out string chkMsg);

                if (!chkOk)
                {
                    ShowFail(chkMsg);
                    return;
                }

                if (isUsed)
                {
                    ShowFail("该纸码已经使用，请重新输入新纸码");
                    return;
                }
                AddLogMessage("纸码未使用，可以绑定", Color.Green);

                // 3. Serializable 绑定
                string schedulingId = g_DicMESConfig["Config"]["SchedulingID"];
                string stationId = g_DicMESConfig["Config"]["StationID"];

                bool bindOk = FormHelper.Serializable(
                    _mesUrl, _loginId, _clientId,
                    paperCode,
                    schedulingId,
                    "1",                       // BOARD_COUNT
                    stationId,
                    new List<string> { _currentLaserCode },  // NEW_SFC_LIST
                    "C",                       // SFC_STATE
                    out string bindMsg);

                if (!bindOk)
                {
                    ShowFail(bindMsg);
                    return;
                }
                AddLogMessage("绑定成功", Color.Green);

                // 4. Complete 出站
                string remark = g_DicMESConfig["Config"].ContainsKey("remark1")
                    ? g_DicMESConfig["Config"]["remark1"] : "";
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                bool cplOk = FormHelper.Complete(
                    _mesUrl, _loginId, _clientId, paperCode,
                    stationId, schedulingId, remark, time,
                    out string cplMsg);

                if (cplOk)
                {
                    AddLogMessage("Complete成功", Color.Green);
                    BeginInvoke(new Action(() => ShowPass()));
                }
                else
                {
                    ShowFail(cplMsg);
                }
            }
            catch (Exception ex)
            {
                AddLogMessage("纸码处理异常：" + ex.Message, Color.Red);
                ShowFail(ex.Message);
            }
        }

        // ============================================================
        // MES 接口调用
        // ============================================================

        /// <summary>
        /// 调用 MES Start 接口，产品入站
        /// </summary>
        private bool CallStart(string SFC)
        {
            try
            {
                string StationName = g_DicMESConfig["Config"]["Operation"];
                string Line = g_DicMESConfig["Config"]["Line"];
                string ShopOrder = g_DicMESConfig["Config"]["Resource"];
                string SchingID = g_DicMESConfig["Config"]["SchedulingID"];

                bool flag = FormHelper.Start(_mesUrl, _loginId, _clientId, SFC, StationName, Line, ShopOrder, SchingID, out string msg);
                if (!flag)
                    AddLogMessage("Start失败：" + msg, Color.Red);
                return flag;
            }
            catch (Exception ex)
            {
                AddLogMessage("Start异常：" + ex.Message, Color.Red);
                return false;
            }
        }

        // ============================================================
        // 日志
        // ============================================================

        /// <summary>
        /// 向日志 RichTextBox 追加时间戳日志，自动处理跨线程调用；
        /// 超过 100 行时自动清屏
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="color">文字颜色，默认黑色（成功=Green，失败=Red）</param>
        private void AddLogMessage(string message, Color? color = null)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => AddLogMessage(message, color)));
                return;
            }

            Color useColor = color ?? Color.Black;
            if (this.uiRichTextBox1.Lines.Length > 100)
                this.uiRichTextBox1.Text = string.Empty;

            string fe = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]: ";
            this.uiRichTextBox1.SelectionStart = this.uiRichTextBox1.TextLength;
            this.uiRichTextBox1.SelectionColor = useColor;
            this.uiRichTextBox1.AppendText(fe + message + "\r\n");
            this.uiRichTextBox1.SelectionStart = this.uiRichTextBox1.TextLength;
            this.uiRichTextBox1.ScrollToCaret();
            WriteLogs.WriteLog(message);
        }

        // ============================================================
        // 中断恢复
        // ============================================================

        /// <summary>启动时检查是否有未完成的绑定流程</summary>
        private void CheckRecoveryState()
        {
            try
            {
                if (!File.Exists(_statePath))
                {
                    EnterWaitingState();
                    return;
                }

                string json = File.ReadAllText(_statePath);
                var data = JsonConvert.DeserializeObject<dynamic>(json);
                string savedState = data.state?.ToString() ?? "";
                string savedLaserCode = data.laserCode?.ToString() ?? "";

                if (savedState == "Processing" && !string.IsNullOrEmpty(savedLaserCode))
                {
                    var result = MessageBox.Show(
                        $"检测到上次未完成的绑定流程\n镭雕码：{savedLaserCode}\n\n是否继续绑定纸码？",
                        "恢复流程",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _currentLaserCode = savedLaserCode;
                        SFC_UITextBox.Text = savedLaserCode;

                        // 重新获取规则并进入加工中
                        Task.Run(() =>
                        {
                            string productId = g_DicMESConfig["Config"]["PRODUCT_ID"];
                            bool ok = FormHelper.GetCustomData(
                                _mesUrl, _loginId, _clientId, productId,
                                out List<GetCustomDataItem> dataList, out string _);

                            if (ok && dataList != null)
                            {
                                _customData = dataList;
                                foreach (var item in dataList)
                                {
                                    if (item.NAME == "SFCRule") _sfcRule = item.VALUE;
                                    else if (item.NAME == "SubSFCRule") _subSfcRule = item.VALUE;
                                }
                                BeginInvoke(new Action(() => EnterProcessingState()));
                            }
                            else
                            {
                                AddLogMessage("恢复失败：无法获取规则", Color.Red);
                                BeginInvoke(new Action(() => EnterWaitingState()));
                            }
                        });
                    }
                    else
                    {
                        EnterWaitingState();
                    }
                }
                else
                {
                    EnterWaitingState();
                }
            }
            catch (Exception ex)
            {
                AddLogMessage("恢复检测异常：" + ex.Message, Color.Red);
                EnterWaitingState();
            }
        }

        /// <summary>保存恢复状态到文件</summary>
        private void SaveRecoveryState()
        {
            try
            {
                var data = new { state = "Processing", laserCode = _currentLaserCode ?? "" };
                File.WriteAllText(_statePath, JsonConvert.SerializeObject(data));
            }
            catch (Exception) { }
        }

        /// <summary>清除恢复状态文件</summary>
        private void ClearRecoveryState()
        {
            try
            {
                if (File.Exists(_statePath))
                    File.Delete(_statePath);
            }
            catch (Exception) { }
        }

        // ============================================================
        // 重置按钮
        // ============================================================

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (_state == BindState.Processing && !string.IsNullOrEmpty(_currentLaserCode))
            {
                var result = MessageBox.Show(
                    "确定要重置当前绑定流程吗？\n已扫描的镭雕码将被清除。",
                    "确认重置",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
            }
            _resultTimer.Stop();
            EnterWaitingState();
        }

        // ============================================================
        // USB 扫码枪回车事件（键盘模式）
        // ============================================================

        /// <summary>镭雕二维码输入框回车事件：扫码枪输入后触发绑定流程</summary>
        private void SFC_UITextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _state == BindState.Waiting)
            {
                e.SuppressKeyPress = true;
                string code = SFC_UITextBox.Text.Trim();
                if (!string.IsNullOrEmpty(code))
                    Task.Run(() => HandleLaserCode(code));
            }
        }

        /// <summary>纸码输入框回车事件：扫码枪输入后触发绑定流程</summary>
        private void uiTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _state == BindState.Processing)
            {
                e.SuppressKeyPress = true;
                string code = uiTextBox1.Text.Trim();
                if (!string.IsNullOrEmpty(code))
                    Task.Run(() => HandlePaperCode(code));
            }
        }

        // ============================================================
        // 班次管理
        // ============================================================

        /// <summary>
        /// 启动时根据当前时间确定所属班次，初始化班次开始时间
        /// </summary>
        private void InitializeShiftState()
        {
            DateTime now = DateTime.Now;

            // 如果从 counters.json 恢复的班次开始时间仍在当前班次内，保留它
            if (_shiftStartTime != DateTime.MinValue)
            {
                DateTime currentShiftStart = GetCurrentShiftStart(now);
                if (_shiftStartTime == currentShiftStart)
                    return;
                // 班次已过，使用旧数据作为上班产出
                _lastShiftOutput = _currentShiftOutput;
                OutPut.Text = _lastShiftOutput.ToString();
                _currentShiftOutput = 0;
            }

            _shiftStartTime = GetCurrentShiftStart(now);
            SaveCounters();
        }

        /// <summary>
        /// 根据当前时间计算所属班次的开始时间
        /// 白班：dayShiftStart:00 ~ nightShiftStart:00
        /// 夜班：nightShiftStart:00 ~ 次日 dayShiftStart:00
        /// </summary>
        private DateTime GetCurrentShiftStart(DateTime now)
        {
            if (now.Hour >= _dayShiftStartHour && now.Hour < _nightShiftStartHour)
            {
                // 白班
                return now.Date.AddHours(_dayShiftStartHour);
            }
            else if (now.Hour >= _nightShiftStartHour)
            {
                // 夜班（当天开始）
                return now.Date.AddHours(_nightShiftStartHour);
            }
            else
            {
                // 夜班（前一天 20:00 开始，跨到次日凌晨）
                return now.Date.AddDays(-1).AddHours(_nightShiftStartHour);
            }
        }
    }
}
