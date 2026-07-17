using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleTCP;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
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
        /// <summary>本地工站名（用于 SFC Log 过滤和校验）</summary>
        private string _logStation;
        /// <summary>第一条符合规则的 SFC Log 的 logStation（用于 ChangeSfcStation）</summary>
        private string _firstLogStation;
        /// <summary>SFC Log 校验是否通过（通过才调 ChangeSfcStation）</summary>
        private bool _sfcLogPassed;

        // ========== 过站计数器 ==========
        /// <summary>当班过站成功计数</summary>
        private int _passCount = 0;
        /// <summary>当班过站失败计数</summary>
        private int _failCount = 0;
        /// <summary>上次换班日期（防止同一班次重复触发）</summary>
        private DateTime _lastShiftDate = DateTime.MinValue;
        /// <summary>换班检查定时器</summary>
        private System.Windows.Forms.Timer _shiftTimer;
        /// <summary>过站成功音效</summary>
        private SoundPlayer _passSound;
        /// <summary>过站失败音效</summary>
        private SoundPlayer _failSound;

        // ========== 扫码枪 / CCD 连接 ==========
        /// <summary>扫码枪 TCP 客户端</summary>
        private SimpleTcpClient scanclient;
        /// <summary>扫码枪是否已连接</summary>
        private bool _scanConnected;
        /// <summary>扫码枪是否正在重连中</summary>
        private bool _scanReconnecting;
        /// <summary>扫码枪 IP / 端口（缓存）</summary>
        private string _scanIp;
        private int _scanPort;
        /// <summary>扫码枪健康检查定时器</summary>
        private System.Timers.Timer _scanHealthTimer;
        /// <summary>CCD TCP 客户端</summary>
        private SimpleTcpClient _ccdClient;
        /// <summary>CCD 是否已连接</summary>
        private bool _ccdConnected;
        /// <summary>CCD 是否正在重连中</summary>
        private bool _ccdReconnecting;
        /// <summary>CCD IP / 端口（缓存）</summary>
        private string _ccdIp;
        private int _ccdPort;
        /// <summary>CCD 健康检查定时器</summary>
        private System.Windows.Forms.Timer _ccdHealthTimer;
        /// <summary>CCD 是否在等待结果</summary>
        private volatile bool _ccdArmed;
        /// <summary>当前等待 CCD 结果的 SFC</summary>
        private string _ccdPendingSfc;
        /// <summary>后台任务取消令牌</summary>
        private CancellationTokenSource _cts;

        /// <summary></summary>
        /// 构造函数：初始化组件、加载全部配置
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            g_DicMESConfig = new ConfigService().LoadAllConfig();
            _cts = new CancellationTokenSource();
            _passSound = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pass.wav"));
            _failSound = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alert.wav"));
        }

        /// <summary>
        /// 窗体加载：缓存配置 → 初始化参数 → 连接设备 → 启动健康检查/定时器 → 注册关闭事件
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            CacheConfig();               // 缓存 MES 接口 URL / LoginID / ClientID
            InitSettings();              // 读取设备、工站等本地配置
            LoadCounters();              // 从 counters.json 恢复过站计数器
            ConnectToScanner();          // 连接扫码枪 TCP
            ConnectToCcd();              // 连接 CCD TCP（始终启用）
            StartScanHealthCheck();      // 启动扫码枪健康检查/自动重连
            StartCcdHealthCheck();       // 启动 CCD 健康检查/自动重连
            StartShiftTimer();           // 启动换班检查定时器（8:00 / 20:00）
            this.FormClosing += MainForm_FormClosing;  // 注册关闭事件释放资源
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
        /// 读取设备连接和工站配置
        /// </summary>
        private void InitSettings()
        {
            // SFC Log 校验用的本地工站名
            _logStation = g_DicMESConfig["SOFTWARE"].ContainsKey("logStation")
                ? g_DicMESConfig["SOFTWARE"]["logStation"] : "";
            // 扫码枪配置
            _scanIp = g_DicMESConfig["Setting"]["scanip"];
            int.TryParse(g_DicMESConfig["Setting"]["scanport"], out _scanPort);
            // CCD 配置
            _ccdIp = g_DicMESConfig["CCD"]["ip"];
            int.TryParse(g_DicMESConfig["CCD"]["port"], out _ccdPort);
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
                    _passCount = (int)data.pass;
                    _failCount = (int)data.fail;
                    PassCount.Text = _passCount.ToString();
                    FailCount.Text = _failCount.ToString();
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
                var data = new { pass = _passCount, fail = _failCount };
                File.WriteAllText(_countersPath, JsonConvert.SerializeObject(data));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 过站失败计数递增并落盘，提取重复逻辑
        /// </summary>
        private void MarkFail()
        {
            _failCount++;
            SaveCounters();
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    FailCount.Text = _failCount.ToString();
                    uiLabel2.Text = "FAIL";
                    uiLabel2.BackColor = Color.Red;
                    try { _failSound?.Play(); } catch { }
                }));
                return;
            }
            FailCount.Text = _failCount.ToString();
            uiLabel2.Text = "FAIL";
            uiLabel2.BackColor = Color.Red;
            try { _failSound?.Play(); } catch { }
        }

        /// <summary>
        /// 设置过站结果标签为 PASS（绿色背景白色字体）
        /// </summary>
        private void SetPassLabel()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(SetPassLabel));
                return;
            }
            uiLabel2.Text = "PASS";
            uiLabel2.BackColor = Color.Green;
            try { _passSound?.Play(); } catch { }
        }

        /// <summary>
        /// 启动换班检查定时器，每 30 秒检查一次是否到达 8:00 或 20:00 换班时间
        /// </summary>
        private void StartShiftTimer()
        {
            _shiftTimer = new System.Windows.Forms.Timer { Interval = 30000 };
            _shiftTimer.Tick += (s, e) => CheckShiftChange();
            _shiftTimer.Start();
        }

        /// <summary>
        /// 检查当前时间是否到达换班点（8:00 或 20:00），
        /// 到达时将 PassCount 值赋给 OutPut，然后清零 PassCount 和 FailCount
        /// </summary>
        private void CheckShiftChange()
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            // 判断是否在换班时间窗口内（当前小时为 8 或 20，且今天尚未换班）
            bool isShiftTime = (now.Hour == 8 || now.Hour == 20);

            if (isShiftTime && _lastShiftDate != today)
            {
                _lastShiftDate = today;
                if (this.InvokeRequired)
                    this.BeginInvoke(new Action(DoShiftChange));
                else
                    DoShiftChange();
            }
        }

        private void DoShiftChange()
        {
            OutPut.Text = _passCount.ToString();
            _passCount = 0;
            _failCount = 0;
            PassCount.Text = "0";
            FailCount.Text = "0";
            SaveCounters();
            AddLogMessage($"换班：本班产出 {OutPut.Text}，计数器已清零。");
        }

        /// <summary>
        /// 窗体关闭时释放扫码枪和 CCD 连接及定时器
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { _cts?.Cancel(); } catch (ObjectDisposedException) { }
            try { _cts?.Dispose(); } catch (ObjectDisposedException) { }
            _shiftTimer?.Stop();
            _shiftTimer?.Dispose();
            _scanHealthTimer?.Stop();
            _scanHealthTimer?.Dispose();
            _ccdHealthTimer?.Stop();
            _ccdHealthTimer?.Dispose();
            try { scanclient?.Disconnect(); } catch (Exception) { }
            try { scanclient?.Dispose(); } catch (Exception) { }
            try { _ccdClient?.Disconnect(); } catch (Exception) { }
            try { _ccdClient?.Dispose(); } catch (Exception) { }
        }

        // ============================================================
        // 扫码过站页
        // ============================================================

        /// <summary>
        /// SFC 过站主流程：
        /// 1. MES Start（产品入站）
        /// 2. 提交 CCD 等待判定结果
        /// （Complete / NcComplete 由 CCD 回调异步完成）
        /// 若 CCD 还在处理上一个条码，拒绝新条码防止覆盖
        /// </summary>
        /// <param name="sfcValue">扫描到的 SFC 条码值</param>
        private bool ruleSFC(string sfcValue)
        {
            sfcValue = sfcValue.Trim();
            // 清空结果标签
            BeginInvoke(new Action(() => { uiLabel2.Text = ""; }));

            // 防止覆盖：CCD 还在等待上一个条码的结果
            if (_ccdArmed)
            {
                AddLogMessage($"CCD 忙：正在等待 [{_ccdPendingSfc}] 的判定结果，拒绝 [{sfcValue}]", Color.Red);
                BeginInvoke(new Action(() => SFC_UITextBox.Text = ""));
                MarkFail();
                return false;
            }

            // 第一步：MES Start（产品入站）
            if (!Start(sfcValue))
            {
                AddLogMessage("Start失败", Color.Red);
                MarkFail();
                return false;
            }
            AddLogMessage("start成功", Color.Green);

            // 第二步：提交 CCD 等待判定结果
            _ccdPendingSfc = sfcValue;
            _ccdArmed = true;
            AddLogMessage($"等待CCD判定结果（SFC={sfcValue}）", Color.Blue);
            return true;
        }

        // ============================================================
        // MES 接口调用
        // ============================================================

        /// <summary>
        /// 调用 MES Start 接口，产品入站
        /// </summary>
        /// <param name="SFC">条码值</param>
        /// <returns>成功返回 true</returns>
        private bool Start(string SFC)
        {
            try
            {
                string StationName = g_DicMESConfig["Config"]["Operation"];
                string Line = g_DicMESConfig["Config"]["Line"];
                string ShopOrder = g_DicMESConfig["Config"]["SapShoporder"];
                string SchingID = g_DicMESConfig["Config"]["SchedulingID"];

                bool flag = FormHelper.Start(_mesUrl, _loginId, _clientId, SFC, StationName, Line, ShopOrder, SchingID, out string msg);
                if (!flag)
                    AddLogMessage(msg, Color.Red);
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
            this.uiRichTextBox1.SelectionColor = useColor;
            this.uiRichTextBox1.AppendText(fe + message + "\r\n");
            this.uiRichTextBox1.ScrollToCaret();
            WriteLogs.WriteLog(message);
        }

        // ============================================================
        // SFC Log 校验
        // ============================================================

        /// <summary>
        /// 扫码后调用 GetSfcLogListByParam 校验条码流向：
        /// 1. 获取 SFC Log 列表
        /// 2. 删去 logStation 与本地 logStation 一致的条目
        /// 3. 取剩余第一条，检查 logAction=="REWORK" 且 remark 包含本地 logStation
        /// 通过 → 继续过站 / 不通过 → 记录失败
        /// </summary>
        private bool ValidateSfcLog(string sfc)
        {
            if (string.IsNullOrEmpty(_logStation))
            {
                AddLogMessage("SFC Log校验跳过：未配置 logStation", Color.Orange);
                return true; // 未配置则跳过校验
            }

            bool ok = FormHelper.GetSfcLogListByParam(
                _mesUrl, _loginId, _clientId, sfc,
                out JArray dataList, out string apiMsg);

            if (!ok || dataList == null || dataList.Count == 0)
            {
                AddLogMessage($"SFC Log校验失败：GetSfcLogListByParam 无数据 — {apiMsg}", Color.Red);
                BeginInvoke(new Action(() => MarkFail()));
                return false;
            }

            // 过滤：删去 logStation 与本地一致的条目
            var filtered = new List<JToken>();
            foreach (var item in dataList)
            {
                string itemStation = item["SFC_LOG"]?["logStation"]?.ToString() ?? "";
                if (!string.Equals(itemStation.Trim(), _logStation.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    filtered.Add(item);
                }
            }

            if (filtered.Count == 0)
            {
                AddLogMessage($"SFC Log校验失败：过滤后无剩余条目（所有 logStation 均为 [{_logStation}]）", Color.Red);
                BeginInvoke(new Action(() => MarkFail()));
                return false;
            }

            AddLogMessage($"SFC Log：原始 {dataList.Count} 条，过滤后剩余 {filtered.Count} 条", Color.Blue);

            // 取第一条剩余的
            var firstLog = filtered[0]["SFC_LOG"];
            string logAction = firstLog["logAction"]?.ToString() ?? "";
            string remark = firstLog["remark"]?.ToString() ?? "";

            // 检查：logAction 必须为 REWORK，remark 必须包含本地 logStation
            bool actionOk = string.Equals(logAction.Trim(), "REWORK", StringComparison.OrdinalIgnoreCase);
            bool remarkOk = remark.Contains(_logStation);

            // 记录第一条符合规则的 logStation，供 ChangeSfcStation 使用
            _firstLogStation = firstLog["logStation"]?.ToString() ?? "";

            if (actionOk && remarkOk)
            {
                _sfcLogPassed = true;
                AddLogMessage($"SFC Log校验通过：logAction=REWORK, remark 包含 [{_logStation}]", Color.Green);
            }
            else
            {
                _sfcLogPassed = false;
                string failReason = "";
                if (!actionOk) failReason += $"logAction=[{logAction}]（期望REWORK）";
                if (!remarkOk) failReason += (failReason.Length > 0 ? "；" : "") + $"remark 不包含 [{_logStation}]";
                AddLogMessage($"SFC Log校验未通过：{failReason}（跳过ChangeSfcStation）", Color.Orange);
            }
            return true; // 无论是否符合，都继续后续流程
        }

        // ============================================================
        // 扫码枪 TCP 连接管理
        // ============================================================

        /// <summary>从配置读取扫码枪 IP/端口并建立连接</summary>
        private void ConnectToScanner()
        {
            _scanIp = g_DicMESConfig["Setting"]["scanip"];
            int.TryParse(g_DicMESConfig["Setting"]["scanport"], out _scanPort);
            Connect_Scan(uiLabel7);
        }

        /// <summary>创建 SimpleTcpClient 连接扫码枪，注册 DataReceived 回调，更新状态标签</summary>
        private void Connect_Scan(UILabel uiLabel)
        {
            scanclient?.Disconnect();
            scanclient = new SimpleTcpClient
            {
                StringEncoder = Encoding.UTF8,
                Delimiter = Encoding.UTF8.GetBytes("\r")[0]
            };
            SetSocketTimeout(scanclient);
            scanclient.DataReceived += scanclient_DataReceived;
            try
            {
                scanclient.Connect(_scanIp, _scanPort);
                _scanConnected = true;
                uiLabel.Text = "已连接";
                uiLabel.BackColor = Color.DodgerBlue;
                AddLogMessage("扫码枪连接成功", Color.Green);
            }
            catch
            {
                _scanConnected = false;
                uiLabel.Text = "未连接";
                uiLabel.BackColor = Color.Red;
                AddLogMessage("扫码枪连接失败", Color.Red);
            }
        }

        /// <summary>启动扫码枪健康检查定时器：每 3 秒探测一次，断线自动重连</summary>
        private void StartScanHealthCheck()
        {
            _scanHealthTimer = new System.Timers.Timer(3000);
            _scanHealthTimer.Elapsed += (s, ev) =>
            {
                if (_scanReconnecting) return;

                bool reachable = ProbeScanner();
                if (_scanConnected && !reachable)
                {
                    BeginInvoke(new Action(() =>
                    {
                        AddLogMessage("检测到扫码枪已断开", Color.Red);
                        SetScanStatus(false);
                    }));
                }
                else if (!_scanConnected && reachable)
                {
                    BeginInvoke(new Action(() => AddLogMessage("检测到扫码枪端口可达，开始重连")));
                    _ = ReconnectScanner();
                }
            };
            _scanHealthTimer.Start();
        }

        /// <summary>用短超时 TcpClient 探测扫码枪端口是否可达</summary>
        private bool ProbeScanner()
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var ar = client.BeginConnect(_scanIp, _scanPort, null, null);
                    bool connected = ar.AsyncWaitHandle.WaitOne(1000);
                    try { client.EndConnect(ar); } catch { }
                    if (connected && client.Connected) return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>扫码枪重连：断开旧连接 → 新建客户端 → 最多 5 次尝试，每次间隔 3 秒</summary>
        private async Task ReconnectScanner()
        {
            _scanReconnecting = true;
            var token = _cts?.Token ?? CancellationToken.None;

            for (int i = 1; i <= 5; i++)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    await Task.Run(() =>
                    {
                        try { scanclient?.Disconnect(); } catch (Exception) { }
                        try { scanclient?.Dispose(); } catch (Exception) { }
                        scanclient = new SimpleTcpClient
                        {
                            StringEncoder = Encoding.UTF8,
                            Delimiter = Encoding.UTF8.GetBytes("\r")[0]
                        };
                        SetSocketTimeout(scanclient);
                        scanclient.DataReceived += scanclient_DataReceived;
                        scanclient.Connect(_scanIp, _scanPort);
                    }, token);
                    _scanReconnecting = false;
                    AddLogMessage($"扫码枪重连成功（第{i}次尝试）", Color.Green);
                    SetScanStatus(true);
                    return;
                }
                catch (Exception) { }
                if (i < 5)
                {
                    try { await Task.Delay(3000, token); }
                    catch (TaskCanceledException) { break; }
                }
            }

            _scanReconnecting = false;
            AddLogMessage("扫码枪重连失败，已尝试5次", Color.Red);
            SetScanStatus(false);
        }

        /// <summary>
        /// 扫码枪数据接收回调（TCP 子线程）：
        /// 清洗条码 → 回显 SFC 输入框 → SFC Log 校验 → 触发过站流程
        /// </summary>
        private void scanclient_DataReceived(object sender, SimpleTCP.Message e)
        {
            string sfc = e.MessageString.Replace("\r", "").Replace("\n", "").Replace(" ", "");

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    SFC_UITextBox.Text = sfc;
                    Task.Run(() =>
                    {
                        ValidateSfcLog(sfc);
                        ruleSFC(sfc);
                    });
                }));
            }
        }

        /// <summary>更新扫码枪状态标签（线程安全）</summary>
        private void SetScanStatus(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetScanStatus(connected)));
                return;
            }
            _scanConnected = connected;
            uiLabel7.Text = connected ? "已连接" : "未连接";
            uiLabel7.BackColor = connected ? Color.DodgerBlue : Color.Red;
        }

        // ============================================================
        // CCD TCP 连接管理（始终启用，无开关）
        // ============================================================

        /// <summary>从配置读取 CCD IP/端口并建立连接</summary>
        private void ConnectToCcd()
        {
            _ccdIp = g_DicMESConfig["CCD"]["ip"];
            int.TryParse(g_DicMESConfig["CCD"]["port"], out _ccdPort);
            Connect_Ccd(uiLabel4);
        }

        /// <summary>创建 SimpleTcpClient 连接 CCD 服务端，注册 DataReceived 回调（预留），更新状态标签</summary>
        private void Connect_Ccd(UILabel uiLabel)
        {
            try { _ccdClient?.Disconnect(); } catch (Exception) { }
            try { _ccdClient?.Dispose(); } catch (Exception) { }
            _ccdClient = new SimpleTcpClient
            {
                StringEncoder = Encoding.UTF8,
                Delimiter = Encoding.UTF8.GetBytes("\n")[0]
            };
            _ccdClient.DataReceived += ccdClient_DataReceived;
            try
            {
                _ccdClient.Connect(_ccdIp, _ccdPort);
                _ccdConnected = true;
                uiLabel.Text = "已连接";
                uiLabel.BackColor = Color.DodgerBlue;
                AddLogMessage("CCD连接成功", Color.Green);
            }
            catch
            {
                _ccdConnected = false;
                uiLabel.Text = "未连接";
                uiLabel.BackColor = Color.Red;
                AddLogMessage("CCD连接失败", Color.Red);
            }
        }

        /// <summary>启动 CCD 健康检查定时器：每 3 秒探测一次，断线自动重连</summary>
        private void StartCcdHealthCheck()
        {
            _ccdHealthTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _ccdHealthTimer.Tick += (s, ev) =>
            {
                if (_ccdReconnecting) return;

                bool reachable = ProbeCcd();
                if (_ccdConnected && !reachable)
                {
                    AddLogMessage("检测到CCD已断开", Color.Red);
                    SetCcdStatus(false);
                }
                else if (!_ccdConnected && reachable)
                {
                    AddLogMessage("检测到CCD端口可达，开始重连");
                    _ = ReconnectCcd();
                }
            };
            _ccdHealthTimer.Start();
        }

        /// <summary>用短超时 TcpClient 探测 CCD 端口是否可达</summary>
        private bool ProbeCcd()
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var ar = client.BeginConnect(_ccdIp, _ccdPort, null, null);
                    if (ar.AsyncWaitHandle.WaitOne(1000))
                    {
                        client.EndConnect(ar);
                        return true;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>CCD 重连：断开旧连接 → 新建客户端 → 最多 5 次尝试，每次间隔 3 秒</summary>
        private async Task ReconnectCcd()
        {
            _ccdReconnecting = true;
            var token = _cts?.Token ?? CancellationToken.None;

            for (int i = 1; i <= 5; i++)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    try { _ccdClient?.Disconnect(); } catch (Exception) { }
                    try { _ccdClient?.Dispose(); } catch (Exception) { }
                    _ccdClient = new SimpleTcpClient
                    {
                        StringEncoder = Encoding.UTF8,
                        Delimiter = Encoding.UTF8.GetBytes("\n")[0]
                    };
                    _ccdClient.DataReceived += ccdClient_DataReceived;
                    _ccdClient.Connect(_ccdIp, _ccdPort);
                    _ccdReconnecting = false;
                    AddLogMessage($"CCD重连成功（第{i}次尝试）", Color.Green);
                    SetCcdStatus(true);
                    return;
                }
                catch (Exception) { }
                if (i < 5)
                {
                    try { await Task.Delay(3000, token); }
                    catch (TaskCanceledException) { break; }
                }
            }

            _ccdReconnecting = false;
            AddLogMessage("CCD重连失败，已尝试5次", Color.Red);
            SetCcdStatus(false);
        }

        /// <summary>
        /// CCD 数据接收回调（TCP 子线程）：
        /// 解析结果 → OK: AddSfcKey + Complete → NG: AddSfcKey + NcComplete
        /// </summary>
        private void ccdClient_DataReceived(object sender, SimpleTCP.Message e)
        {
            string raw = e.MessageString
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?.Replace(" ", "") ?? "";

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (!_ccdArmed)
                    {
                        AddLogMessage($"收到CCD数据但未就绪，忽略：{raw}", Color.Gray);
                        return;
                    }

                    _ccdArmed = false;
                    ProcessCcdResult(raw);
                }));
            }
        }

        /// <summary>
        /// 解析 CCD 结果（格式 OK;... 或 NG;...）
        /// OK → AddSfcKey + Complete → PASS
        /// NG → AddSfcKey + NcComplete → FAIL
        /// </summary>
        private void ProcessCcdResult(string raw)
        {
            string[] parts = raw.Split(';');
            string judgement = parts.Length > 0 ? parts[0].ToUpper() : "";
            string sfc = _ccdPendingSfc;

            string dataName = g_DicMESConfig["Setting"].ContainsKey("ccdDataName")
                ? g_DicMESConfig["Setting"]["ccdDataName"] : "CCD";

            // AddSfcKey 公共参数
            string stationId = g_DicMESConfig["Config"]["StationID"];
            string operation = g_DicMESConfig["Config"]["Operation"];
            string shopOrder = g_DicMESConfig["Config"]["SapShoporder"];
            string projectId = g_DicMESConfig["Config"]["PROJECT_ID"];
            string productId = g_DicMESConfig["Config"]["PRODUCT_ID"];
            string schedulingId = g_DicMESConfig["Config"]["SchedulingID"];

            // 先上报 CCD 结果到 MES
            FormHelper.AddSfcKey(
                _mesUrl, _loginId, _clientId,
                sfc, stationId, operation, shopOrder,
                dataName, raw,
                projectId, productId,
                out string addSfcMsg);
            AddLogMessage($"AddSfcKey CCD结果上报：{addSfcMsg}", Color.Blue);

            if (judgement == "OK")
            {
                AddLogMessage($"CCD判定 OK：{raw}", Color.Green);

                string remark = g_DicMESConfig["Config"].ContainsKey("Remark")
                    ? g_DicMESConfig["Config"]["Remark"] : "";
                bool cplOk = FormHelper.Complete(
                    _mesUrl, _loginId, _clientId, sfc,
                    stationId, schedulingId, remark, out string cplMsg);

                if (cplOk)
                {
                    AddLogMessage("Complete成功", Color.Green);
                    _passCount++;
                    SaveCounters();
                    SetPassLabel();
                    PassCount.Text = _passCount.ToString();
                }
                else
                {
                    AddLogMessage($"Complete失败：{cplMsg}", Color.Red);
                    MarkFail();
                }
            }
            else // NG 或其他
            {
                AddLogMessage($"CCD判定 NG：{raw}", Color.Red);

                bool ncOk = FormHelper.NcComplete(
                    _mesUrl, _loginId, _clientId, sfc,
                    stationId, "CCD NG", raw, "CCD",
                    schedulingId, out string ncMsg);

                if (ncOk)
                    AddLogMessage("NcComplete成功", Color.Green);
                else
                    AddLogMessage($"NcComplete失败：{ncMsg}", Color.Red);

                MarkFail();
            }

            // 仅当 SFC Log 校验通过时：变更工站 + 补充 AddSfcKey
            if (_sfcLogPassed)
            {
                // ChangeSfcStation：工站名传第一条符合规则的 logStation
                bool changeOk = FormHelper.ChangeSfcStation(
                    _mesUrl, _loginId, _clientId, sfc,
                    _firstLogStation, out string changeMsg);
                if (changeOk)
                    AddLogMessage($"ChangeSfcStation成功：→ [{_firstLogStation}]", Color.Green);
                else
                    AddLogMessage($"ChangeSfcStation失败：{changeMsg}", Color.Red);

                // 补充 AddSfcKey：DATA_NAME 本地维护，DATA_VALUE 传 CCD 信息
                string extraDataName = g_DicMESConfig["SOFTWARE"].ContainsKey("addSfcKeyDataName")
                    ? g_DicMESConfig["SOFTWARE"]["addSfcKeyDataName"] : "";
                if (!string.IsNullOrEmpty(extraDataName))
                {
                    FormHelper.AddSfcKey(
                        _mesUrl, _loginId, _clientId,
                        sfc, stationId, operation, shopOrder,
                        extraDataName, raw,
                        projectId, productId,
                        out string extraMsg);
                    AddLogMessage($"补充AddSfcKey（{extraDataName}）：{extraMsg}", Color.Blue);
                }
            }
        }

        /// <summary>更新 CCD 状态标签（线程安全）</summary>
        private void SetCcdStatus(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetCcdStatus(connected)));
                return;
            }
            _ccdConnected = connected;
            uiLabel4.Text = connected ? "已连接" : "未连接";
            uiLabel4.BackColor = connected ? Color.DodgerBlue : Color.Red;
        }

        /// <summary>
        /// 利用反射设置 SimpleTcpClient 内部 TcpClient 的 SendTimeout / ReceiveTimeout
        /// 防止断开连接时卡住 UI 线程
        /// </summary>
        private void SetSocketTimeout(SimpleTcpClient client, int timeoutMs = 2000)
        {
            try
            {
                var t = client.GetType();
                var prop = t.GetProperty("Client",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? t.GetProperty("client",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? t.GetProperty("_client",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? t.GetProperty("tcpClient",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prop != null && prop.GetValue(client) is System.Net.Sockets.TcpClient tcp)
                {
                    tcp.SendTimeout = timeoutMs;
                    tcp.ReceiveTimeout = timeoutMs;
                }
            }
            catch (Exception) { }
        }
    }
}
