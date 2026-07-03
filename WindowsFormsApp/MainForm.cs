using HslCommunication;
using HslCommunication.Profinet.Omron;
using Newtonsoft.Json;

using SimpleTCP;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms; 

namespace WindowsFormsApp
{
    public partial class MainForm : UIForm
    {
        // ========== 表格列名常量 ==========
        private const string ColumnBydpn = "Bydpn";
        private const string ColumnDid = "Did";
        private const string ColumnRemarks = "Remarks";
        private const string ColumnRemaining = "Remaining";
        private const string ColumnQty = "Qty";
        private const string ColumnDidRule = "DidRule";
        private const string ColumnLocation = "Location";
        private const string ColumnMinSurplus = "MinSurplus";
        private const string ColumnStopQty = "StopQty";
        private const string ColumnClientNo = "ClientNo";

        // ========== 小件数据相关字段 ==========
        /// <summary>承载表格数据的 DataTable，直接绑定到 DataGridView</summary>
        private DataTable _partsTable;
        /// <summary>小件数据本地持久化路径</summary>
        private readonly string _xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "parts-data.xml");
        /// <summary>过站计数器持久化路径</summary>
        private readonly string _countersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "counters.json");
        /// <summary>所有小件定义（从 MES 接口获取）</summary>
        private List<SubMaterialDef> _allSubDefs = new List<SubMaterialDef>();

        // ========== 配置与运行参数 ==========
        /// <summary>配置参数字典（Config / Setting 两节）</summary>
        private Dictionary<string, Dictionary<string, string>> g_DicMESConfig;
        /// <summary>缓存的 MES 接口地址 / 登录 ID / 客户端 ID，避免反复查字典</summary>
        private string _mesUrl;
        private string _loginId;
        private string _clientId;
        /// <summary>PLC 通信结果地址 / 启动指令</summary>
        private string mesresult, startscan, _startOrder, _heartbeat;
        /// <summary>扫码超时时间（秒），支持小数如 2.5</summary>
        private double timeout = 0;
        /// <summary>扫码开关：false 时忽略 D3000 扫码信号，不发送 start 指令，关闭超时检测</summary>
        private bool _startScanSwitchEnabled;
        /// <summary>胶水校验是否启用</summary>
        private bool _glueCheckEnabled;
        /// <summary>胶水 DID（校验通过后缓存，供 TestDataCollect 使用）</summary>
        private string _glueDid;

        // ========== 外部设备连接 ==========
        /// <summary>欧姆龙 PLC 通讯客户端（TCP 模式）</summary>
        private OmronFinsNet _plcTcp;
        /// <summary>欧姆龙 PLC 通讯客户端（UDP 模式）</summary>
        private OmronFinsUdp _plcUdp;
        /// <summary>当前 PLC 通讯模式：true=TCP, false=UDP</summary>
        private bool _plcIsTcp = true;
        /// <summary>PLC 端口号（TCP/UDP 共用）</summary>
        private int _plcPort = 9600;
        /// <summary>PLC 连接状态（由本类自行维护）</summary>
        private volatile bool _plcConnected;
        /// <summary>防止监控定时器重入</summary>
        private int _monitorBusy;
        /// <summary>扫码枪 TCP 客户端</summary>
        private SimpleTcpClient scanclient;
        /// <summary>后台心跳任务的取消令牌（同时用于重连取消）</summary>
        private CancellationTokenSource _cts;
        /// <summary>扫码枪当前是否已连接（避免依赖 UI 标签判断状态）</summary>
        private bool _scanConnected;
        /// <summary>扫码枪 IP（缓存，避免反复查字典）</summary>
        private string _scanIp;
        /// <summary>扫码枪端口（缓存）</summary>
        private int _scanPort;
        /// <summary>防止下拉框填充时触发 SelectedIndexChanged</summary>
        private bool _populatingMatches;

        // ========== 条码规则 ==========
        /// <summary>从 MES 获取的 SFC 通配符规则（? 匹配任意字符）</summary>
        private string SFCRule = "";

        // ========== 过站计数器 ==========
        /// <summary>当班过站成功计数</summary>
        private int _passCount = 0;
        /// <summary>当班过站失败计数</summary>
        private int _failCount = 0;
        /// <summary>上次换班日期（防止同一班次重复触发）</summary>
        private DateTime _lastShiftDate = DateTime.MinValue;
        /// <summary>换班检查定时器</summary>
        private System.Windows.Forms.Timer _shiftTimer;
        /// <summary>PLC 地址监听定时器</summary>
        private System.Timers.Timer _plcMonitorTimer;
        /// <summary>扫码枪健康检查定时器</summary>
        private System.Windows.Forms.Timer _scanHealthTimer;
        /// <summary>PLC 是否正在重连中（防止重复触发）</summary>
        private bool _plcReconnecting;
        /// <summary>PLC 连续读取失败计数（UDP 模式下容忍偶发丢包，连续失败 N 次才断开）</summary>
        private int _plcReadFailCount;
        /// <summary>UDP 模式下连续读取失败多少次才判定断开</summary>
        private const int UdpFailThreshold = 5;
        /// <summary>扫码枪是否正在重连中（防止重复触发）</summary>
        private bool _scanReconnecting;
        /// <summary>D3000 扫码信号是否已就绪（1=允许扫码）</summary>
        private volatile bool _scanArmed;
        /// <summary>D3000 变为 1 的时间戳，用于超时校验</summary>
        private DateTime _scanArmedTime;
        /// <summary>上一次 D3000 的值，用于检测上升沿</summary>
        private short _prevD3000Value;
        private readonly object _scanCycleLock = new object();
        private bool _scanCycleActive;
        private CancellationTokenSource _scanTimeoutCts;

        // ========== PLC 通讯助手方法 ==========
        /// <summary>当前 PLC 是否已连接（由本类自行维护状态）</summary>
        private bool PlcConnected => _plcConnected;

        /// <summary>读取 PLC 单字（Int16）</summary>
        private OperateResult<short> PlcReadInt16(string addr)
        {
            if (_plcIsTcp)
                return _plcTcp != null ? _plcTcp.ReadInt16(addr) : new OperateResult<short>("PLC 未连接");
            return _plcUdp != null ? _plcUdp.ReadInt16(addr) : new OperateResult<short>("PLC 未连接");
        }

        /// <summary>写入 PLC 单字（short）</summary>
        private OperateResult PlcWrite(string addr, short val)
        {
            if (_plcIsTcp)
                return _plcTcp != null ? _plcTcp.Write(addr, val) : new OperateResult("PLC 未连接");
            return _plcUdp != null ? _plcUdp.Write(addr, val) : new OperateResult("PLC 未连接");
        }

        /// <summary>写入 PLC 单字（int → short）</summary>
        private OperateResult PlcWrite(string addr, int val) =>
            PlcWrite(addr, (short)val);

        /// <summary>连接 PLC（根据当前模式创建 TCP 或 UDP 客户端）</summary>
        private OperateResult PlcConnect(string ip, int port)
        {
            _plcConnected = false;
            if (_plcIsTcp)
            {
                _plcTcp = new OmronFinsNet(ip, port);
                _plcTcp.ConnectTimeOut = 2000;
                _plcTcp.ReceiveTimeOut = 2000;
                var result = _plcTcp.ConnectServer();
                if (result.IsSuccess) _plcConnected = true;
                return result;
            }
            else
            {
                _plcUdp = new OmronFinsUdp(ip, port);
                _plcUdp.ReceiveTimeOut = 2000;
                // UDP 无连接，不标记已连接——由后续 Read 验证通过后再标记
                return new OperateResult { IsSuccess = true, Message = "UDP ready" };
            }
        }

        /// <summary>断开 PLC 连接（同时关闭 TCP 和 UDP 客户端）</summary>
        private void PlcClose()
        {
            _plcConnected = false;
            _plcTcp?.ConnectClose();
            _plcTcp = null;
            _plcUdp = null;
        }

        /// <summary></summary>
        /// 构造函数：初始化组件、加载全部配置
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            g_DicMESConfig = new ConfigService().LoadAllConfig();
        }

        /// <summary>
        /// 窗体加载：按顺序缓存配置 → 初始化参数 → 获取规则 → 加载表格 →
        /// 绑定网格 → 连接 PLC → 连接扫码枪 → 启用心跳 → 启动监听 → 注册关闭事件
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            CacheConfig();               // 缓存 MES 接口 URL / LoginID / ClientID
            InitSettings();              // 读取 PLC 超时 / 结果地址 / 启动指令
            GetCustomData();             // 从 MES 获取 SFC 条码匹配规则
            LoadOrCreateTable();         // 从本地 XML 恢复或从 MES 拉取小件表格
            BindGrid();                  // 小件 DataTable 绑定到 DataGridView
            InitPlcMonitorGrid();        // 初始化 PLC 地址监听表格（D3000/D3001/D5000）
            LoadCounters();              // 从 counters.json 恢复过站计数器
            InitPlcModeCombo();          // 初始化 PLC 连接模式下拉框（TCP/UDP）
            ConnectToPlc();              // 连接欧姆龙 PLC
            ConnectToScanner();          // 连接扫码枪 TCP 客户端
            StartHeartbeat();            // 启动后台心跳：每 2s 向 D5000 写 1
            StartPlcMonitor();           // 启动 PLC 地址监听：每 500ms 读取并刷新
            StartScanHealthCheck();      // 启动扫码枪健康检查：每 3s 探测并自动重连
            StartShiftTimer();           // 启动换班检查定时器（8:00 / 20:00）
            this.FormClosing += MainForm_FormClosing;  // 注册关闭事件释放资源
        }

        /// <summary>
        /// 重新加载全部配置文件，使修改实时生效（每个扫码周期开始时调用）
        /// </summary>
        private void ReloadConfig()
        {
            g_DicMESConfig = new ConfigService().LoadAllConfig();
            CacheConfig();
            InitSettings();
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
        /// 读取 Setting 节中的运行参数到成员变量
        /// </summary>
        private void InitSettings()
        {
            startscan = g_DicMESConfig["Setting"]["startscan"];
            _startOrder = g_DicMESConfig["Setting"]["startorder"];
            timeout = double.Parse(g_DicMESConfig["Setting"]["timeout"]);
            mesresult = g_DicMESConfig["Setting"]["mesresult"];
            _heartbeat = g_DicMESConfig["Setting"]["heartbeat"];
            int.TryParse(g_DicMESConfig["Setting"]["plcport"], out _plcPort);
            bool.TryParse(g_DicMESConfig["SOFTWARE"]["startscanswitch"], out _startScanSwitchEnabled);
        }

        /// <summary>
        /// 从配置读取 PLC IP 并建立连接
        /// </summary>
        /// <summary>
        /// 初始化 PLC 连接模式下拉框：TCP / UDP，默认 TCP
        /// </summary>
        private void InitPlcModeCombo()
        {
            if (uiComboBox1 == null) return;
            uiComboBox1.Items.Clear();
            uiComboBox1.Items.Add("TCP");
            uiComboBox1.Items.Add("UDP");

            // 读取上次保存的连接模式，默认 TCP
            string savedMode = g_DicMESConfig["SOFTWARE"].ContainsKey("plcmode")
                ? g_DicMESConfig["SOFTWARE"]["plcmode"] : "TCP";
            _plcIsTcp = !string.Equals(savedMode, "UDP", StringComparison.OrdinalIgnoreCase);
            uiComboBox1.SelectedIndex = _plcIsTcp ? 0 : 1;

            uiComboBox1.SelectedIndexChanged += (s, ev) =>
            {
                bool newMode = uiComboBox1.SelectedIndex == 0; // 0=TCP, 1=UDP
                if (newMode == _plcIsTcp) return; // 未变化

                // 1. 停掉旧的 heartbeat / reconnect
                try { _cts?.Cancel(); } catch (ObjectDisposedException) { }

                // 2. 断开旧连接，更新 UI 状态
                PlcClose();
                _plcIsTcp = newMode;
                _plcReconnecting = true; // 临时阻止 SetPlcStatus 触发重连
                SetPlcStatus(false);
                _plcReconnecting = false;
                AddLogMessage(_plcIsTcp ? "已切换为 TCP 模式，重新连接PLC..." : "已切换为 UDP 模式，重新连接PLC...", Color.Blue);

                // 3. 持久化模式到 setting.ini
                new ConfigService().SavePlcMode(_plcIsTcp ? "TCP" : "UDP");

                // 4. 新建令牌 → 心跳 + 重连共用同一 CTS
                StartHeartbeat();
                _ = ReconnectPlc();
            };
        }

        private async void ConnectToPlc()
        {
            try
            {
                string plcip = g_DicMESConfig["Setting"]["plcip"];
                await Connect_PLC(plcip, uiLabel3).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                AddLogMessage("PLC连接异常：" + ex.Message, Color.Red);
                WriteLogs.WriteLog("PLC连接异常: " + ex);
            }
        }

        /// <summary>
        /// 异步连接 PLC：I/O 在线程池执行，避免阻塞 UI
        /// </summary>
        private async Task Connect_PLC(string plcip, UILabel uiLabel)
        {
            try
            {
                // 所有阻塞 I/O 放在后台线程
                var (closeOk, connectOk, connectMsg, readOk) = await Task.Run(() =>
                {
                    PlcClose();
                    var cr = PlcConnect(plcip, _plcPort);
                    if (!cr.IsSuccess) return (true, false, cr.Message, false);
                    var rd = PlcReadInt16(startscan);
                    return (true, true, "", rd.IsSuccess);
                });

                if (!connectOk)
                {
                    uiLabel.Text = "未连接";
                    uiLabel.BackColor = Color.Red;
                    AddLogMessage($"PLC连接失败：{connectMsg}", Color.Red);
                    return;
                }
                if (readOk)
                {
                    _plcConnected = true;
                    uiLabel.Text = "已连接";
                    uiLabel.BackColor = Color.DodgerBlue;
                    AddLogMessage($"PLC连接成功（{(_plcIsTcp ? "TCP" : "UDP")}）", Color.Green);
                }
                else
                {
                    uiLabel.Text = "未连接";
                    uiLabel.BackColor = Color.Red;
                    AddLogMessage("PLC连接失败：读取D3000无响应", Color.Red);
                }
            }
            catch
            {
                uiLabel.Text = "未连接";
                uiLabel.BackColor = Color.Red;
                AddLogMessage("PLC连接失败", Color.Red);
            }
        }

        /// <summary>
        /// 从配置读取扫码枪 IP/端口并建立连接
        /// </summary>
        private void ConnectToScanner()
        {
            _scanIp = g_DicMESConfig["Setting"]["scanip"];
            _scanPort = int.Parse(g_DicMESConfig["Setting"]["scanport"]);
            Connect_Scan(uiLabel5);
        }

        /// <summary>
        /// 启动后台心跳任务，每 2 秒向 PLC 心跳地址 写入 1（不记日志）
        /// </summary>
        private void StartHeartbeat()
        {
            try { _cts?.Dispose(); } catch (ObjectDisposedException) { }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_plcConnected)
                    {
                        try
                        {
                            if (_plcIsTcp)
                                _plcTcp?.Write(_heartbeat, (short)1);
                            else
                                _plcUdp?.Write(_heartbeat, (short)1);
                        }
                        catch (Exception) { }
                    }
                    try
                    {
                        await Task.Delay(2000, token);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, token);
        }

        /// <summary>
        /// 初始化 PLC 地址监听表格：三行 扫码启动 / MES结果 / 心跳
        /// </summary>
        private void InitPlcMonitorGrid()
        {
            uiDataGridView1.Rows.Clear();
            uiDataGridView1.Rows.Add(startscan, startscan + " 扫码启动信号", "0");
            uiDataGridView1.Rows.Add(mesresult, mesresult + " MES结果信号", "0");
            uiDataGridView1.Rows.Add(_heartbeat, _heartbeat + "心跳信号", "0");
        }

        /// <summary>
        /// 启动 PLC 地址监听定时器，每 500ms 读取一次值并刷新表格
        /// </summary>
        private void StartPlcMonitor()
        {
            _plcMonitorTimer = new System.Timers.Timer { Interval = 500, AutoReset = true };
            _plcMonitorTimer.Elapsed += (s, ev) =>
            {
                // 防止重入：上一次读取还在阻塞中时跳过本轮
                if (Interlocked.CompareExchange(ref _monitorBusy, 1, 0) != 0)
                    return;

                try
                {
                    if (!PlcConnected)
                    {
                        SetPlcStatus(false);
                        return;
                    }

                    // D3000：读取值并刷新表格，同时处理扫码信号 + 超时
                    var d3k = PlcReadInt16(startscan);
                    bool d3000Ok = d3k.IsSuccess;
                    if (!d3000Ok)
                    {
                        if (!_plcIsTcp)
                        {
                            // UDP 无连接，偶发丢包正常；连续失败 N 次才判定断开
                            _plcReadFailCount++;
                            if (_plcReadFailCount >= UdpFailThreshold)
                            {
                                _plcConnected = false;
                                _plcReadFailCount = 0;
                                SetPlcStatus(false);
                                // 断开后跳过本轮，等待重连
                                Interlocked.Exchange(ref _monitorBusy, 0);
                                return;
                            }
                            // 未达阈值：跳过 D3000 信号处理（不更新 _prevD3000Value/_scanArmed），
                            // 但仍刷新 mesresult/heartbeat，避免状态僵死导致下次误判
                        }
                        else
                        {
                            // TCP 面向连接，一次失败即断开
                            _plcConnected = false;
                            SetPlcStatus(false);
                            Interlocked.Exchange(ref _monitorBusy, 0);
                            return;
                        }
                    }
                    else
                    {
                        // 读取成功 → 清零连续失败计数
                        _plcReadFailCount = 0;
                    }

                    if (d3000Ok)
                    {
                        UpdateMonitorCell(0, d3k.Content.ToString());
                        short d3000Val = d3k.Content;

                        // 仅当 startscanswitch=true 时才监听 D3000 扫码信号、发送 start 指令、检测超时
                        if (_startScanSwitchEnabled)
                        {
                            if (d3000Val == 1)
                            {
                                bool isRisingEdge = _prevD3000Value != 1;
                                bool started = false;

                                if (isRisingEdge)
                                {
                                    ReloadConfig();
                                    started = BeginScanCycle();
                                    if (started)
                                    {
                                        AddLogMessage($"{startscan}=1 检测到扫码信号", Color.Blue);
                                    }
                                    else
                                    {
                                        AddLogMessage("扫码周期未结束，忽略重复D3000=1", Color.Orange);
                                    }
                                }

                                Task.Run(() => PlcWrite(startscan, (short)0));
                                BeginInvoke(new Action(() => AddLogMessage($"上位机写 [{startscan}] = 0", Color.Green)));

                                if (started && _scanConnected && !string.IsNullOrEmpty(_startOrder))
                                {
                                    var order = _startOrder;
                                    Task.Run(() => SendScannerStartOrder(order));
                                }
                            }
                            _prevD3000Value = d3000Val;
                        }
                    }

                    bool ok2 = RefreshPlcCell(1, mesresult);
                    RefreshPlcCell(2, _heartbeat);
                    SetPlcStatus(true);
                }
                catch (Exception ex)
                {
                    WriteLogs.WriteLog("PLC监控定时器异常: " + ex);
                }
                finally
                {
                    Interlocked.Exchange(ref _monitorBusy, 0);
                }
            };
            _plcMonitorTimer.Start();
        }

        /// <summary>
        /// 读取单个 PLC 地址并刷新监控表格对应行，返回是否读取成功
        /// </summary>
        private bool RefreshPlcCell(int rowIndex, string address)
        {
            try
            {
                var result = PlcReadInt16(address);
                if (result.IsSuccess)
                {
                    UpdateMonitorCell(rowIndex, result.Content.ToString());
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 更新 PLC 状态标签，并在从已连接变为未连接时触发自动重连
        /// </summary>
        private void SetPlcStatus(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetPlcStatus(connected)));
                return;
            }
            bool wasConnected = uiLabel3.Text == "已连接";
            uiLabel3.Text = connected ? "已连接" : "未连接";
            uiLabel3.BackColor = connected ? Color.DodgerBlue : Color.Red;
            if (!connected && wasConnected && !_plcReconnecting)
            {
                AddLogMessage("检测到PLC已断开，开始重连", Color.Red);
                _ = ReconnectPlc();
            }
        }

        /// <summary>
        /// PLC 重连：关闭旧连接 → 创建新客户端 → Open → 验证读取，
        /// 最多 5 次，每次间隔 3 秒，窗体关闭时通过 _cts 取消
        /// </summary>
        private async Task ReconnectPlc()
        {
            if (_plcReconnecting) return;
            try
            {
                _plcReconnecting = true;
                _plcReadFailCount = 0;  // 重置连续失败计数
                string plcip = g_DicMESConfig["Setting"]["plcip"];
                var token = _cts?.Token ?? CancellationToken.None;

                for (int i = 1; i <= 5; i++)
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        AddLogMessage($"PLC重连第{i}/5次尝试...");
                        var result = await Task.Run(() =>
                        {
                            PlcClose();
                            PlcConnect(plcip, _plcPort);
                            return PlcReadInt16(startscan);
                        }, token);
                        if (result.IsSuccess)
                        {
                            _plcConnected = true;
                            _plcReconnecting = false;
                            AddLogMessage($"PLC重连成功（第{i}次尝试）", Color.Green);
                            SetPlcStatus(true);
                            return;
                        }
                        AddLogMessage($"PLC重连第{i}次失败：{result.Message}", Color.Red);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        AddLogMessage($"PLC重连第{i}次异常：{ex.Message}", Color.Red);
                    }
                    if (i < 5)
                    {
                        try { await Task.Delay(3000, token); }
                        catch (TaskCanceledException) { break; }
                    }
                }
                _plcReconnecting = false;
                AddLogMessage("PLC重连失败，已尝试5次，请重启软件", Color.Red);
                SetPlcStatus(false);
            }
            catch (Exception ex)
            {
                _plcReconnecting = false;
                WriteLogs.WriteLog("PLC重连未预期异常: " + ex);
            }
        }

        private void UpdateMonitorCell(int rowIndex, string value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateMonitorCell(rowIndex, value)));
                return;
            }
            if (uiDataGridView1.Rows.Count > rowIndex)
            {
                uiDataGridView1.Rows[rowIndex].Cells[2].Value = value;
            }
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
                this.BeginInvoke(new Action(() => FailCount.Text = _failCount.ToString()));
                return;
            }
            FailCount.Text = _failCount.ToString();
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
        /// 窗体关闭时取消心跳任务，释放 PLC 连接和扫码枪连接
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { _cts?.Cancel(); } catch (ObjectDisposedException) { }
            try { _cts?.Dispose(); } catch (ObjectDisposedException) { }
            try { _scanTimeoutCts?.Cancel(); } catch (ObjectDisposedException) { }
            try { _scanTimeoutCts?.Dispose(); } catch (ObjectDisposedException) { }
            _shiftTimer?.Stop();
            _shiftTimer?.Dispose();
            _plcMonitorTimer?.Stop();
            _plcMonitorTimer?.Dispose();
            _scanHealthTimer?.Stop();
            _scanHealthTimer?.Dispose();
            try { PlcClose(); } catch (Exception) { }
            try { scanclient?.Disconnect(); } catch (Exception) { }
            try { scanclient?.Dispose(); } catch (Exception) { }
        }

        // ============================================================
        // 扫码过站页
        // ============================================================

        /// <summary>
        /// SFC 扫码过站主流程：
        /// 1. 校验 SFC 是否符合规则
        /// 2. 胶水库存校验
        /// 3. 小件绑定及数量校验
        /// 4. MES Start（产品入站）
        /// 5. TestDataCollect2MainChild（上报测试数据）
        /// 6. AddSfcKey（上报 Config 绑定，Config=false 时跳过）
        /// 7. MES Complete（过站完成）
        /// 每步失败向 PLC 写对应状态码，并更新过站计数器
        /// </summary>
        /// <param name="sfcValue">扫描到的 SFC 条码值</param>
        private bool ruleSFC(string sfcValue)
        {
            sfcValue = sfcValue.Trim();
            // 第一步：SFC 规则校验
            if (!ValidateSFC(sfcValue, SFCRule))
            {
                AddLogMessage($"SFC规则校验失败：SFC = {sfcValue} 不符合规则 {SFCRule}", Color.Red);
                this.BeginInvoke(new Action(() => SFC_UITextBox.Text = ""));
                MarkFail();
                return false;
            }
            AddLogMessage($"SFC规则校验成功：SFC = {sfcValue}", Color.Green);

            // 第二步：胶水库存校验（GetAuxmStockList）
            if (!ValidateGlueStock())
            {
                Task.Run(() => PlcWrite(mesresult, 11)); AddLogMessage($"上位机写 [{mesresult}] = 11", Color.Green);
                MarkFail();
                return false;
            }

            // 第三步：小件绑定及数量校验
            if (!ValidatePartsBeforePass())
            {
                Task.Run(() => PlcWrite(mesresult, 12)); AddLogMessage($"上位机写 [{mesresult}] = 12", Color.Red);
                AddLogMessage("过站前小件信息校验失败", Color.Red);
                MarkFail();
                return false;
            }
            AddLogMessage("小件校验成功", Color.Green);

            // 第四步：MES Start（产品入站）
            if (!Start(sfcValue))
            {
                Task.Run(() => PlcWrite(mesresult, 14)); AddLogMessage($"上位机写 [{mesresult}] = 14", Color.Red);
                AddLogMessage("Start失败", Color.Red);
                MarkFail();
                return false;
            }
            AddLogMessage("start成功", Color.Green);

            // 第五步：TestDataCollect2MainChild（上报测试数据）
            if (!TestDataCollect(sfcValue))
            {
                AddLogMessage("TestDataCollect2MainChild 失败", Color.Red);
                MarkFail();
                return false;
            }
            AddLogMessage("TestDataCollect2MainChild上传成功", Color.Green);

            // 第五步半：Binding 扣料
            if (!BindingDeduction(sfcValue))
            {
                AddLogMessage("Binding 扣料失败", Color.Red);
                MarkFail();
                return false;
            }
            AddLogMessage("Binding 扣料成功", Color.Green);

            // 第六步：AddSfcKey 上报 Config 绑定结果（Config=false 时跳过）
            if (!UploadConfigBinding(sfcValue))
            {
                AddLogMessage("AddSfcKey 上报失败", Color.Red);
                MarkFail();
                return false;
            }
            AddLogMessage("AddSfcKey成功", Color.Green);

            // 第七步：MES Complete（过站完成）
            if (!Complete(sfcValue))
            {
                AddLogMessage("Complete失败", Color.Red);
                MarkFail();
                return false;
            }
            Task.Run(() => PlcWrite(mesresult, 27)); AddLogMessage($"上位机写 [{mesresult}] = 27", Color.Green);
            AddLogMessage("Complete成功", Color.Green);
            _passCount++;
            SaveCounters();
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => PassCount.Text = _passCount.ToString()));
            }
            else
            {
                PassCount.Text = _passCount.ToString();
            }
            return true;
        }

        /// <summary>
        /// 过站前校验所有小件：必须全部绑定 Did，且每次扣减数不大于剩余数量
        /// </summary>
        /// <returns>校验通过返回 true</returns>
        private bool ValidatePartsBeforePass()
        {
            if (_partsTable == null)
            {
                AddLogMessage("小件表格还没有初始化。", Color.Red);
                return false;
            }

            if (_partsTable.Rows.Count == 0)
            {
                AddLogMessage("当前没有小件数据，请先加载小件。", Color.Red);
                return false;
            }

            // 收集未绑定和数量不足的小件
            List<string> unboundParts = new List<string>();
            List<string> notEnoughParts = new List<string>();

            foreach (DataRow row in _partsTable.Rows)
            {
                string bydpn = Convert.ToString(row[ColumnBydpn]).Trim();
                string did = Convert.ToString(row[ColumnDid]).Trim();

                if (string.IsNullOrWhiteSpace(did))
                {
                    unboundParts.Add(bydpn);
                    continue;
                }

                double remaining = Convert.ToDouble(row[ColumnRemaining]);
                double deductCount = Convert.ToDouble(row[ColumnQty]);

                if (remaining < deductCount)
                {
                    notEnoughParts.Add(
                        bydpn + "：剩余数量 " + remaining + "，每次扣减 " + deductCount);
                }
            }

            // UI日志警告
            if (unboundParts.Count > 0)
            {
                AddLogMessage(
                    "以下小件还没有绑定小件码，不能过站：" +
                    string.Join("；", unboundParts),
                    Color.Red);
                return false;
            }

            if (notEnoughParts.Count > 0)
            {
                AddLogMessage(
                    "以下小件数量不够扣减，不能过站：" +
                    string.Join("；", notEnoughParts),
                    Color.Red);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证 SFC 值是否符合通配符规则（? 代表匹配任意单字符，非 ? 必须严格相等）
        /// </summary>
        /// <param name="value">实际输入的 SFC 值</param>
        /// <param name="rule">规则字符串，? 代表任意字符</param>
        /// <returns>true 表示匹配</returns>
        private bool ValidateSFC(string value, string rule)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(rule))
                return false;
            if (value.Length != rule.Length)
                return false;

            for (int i = 0; i < rule.Length; i++)
            {
                char ruleChar = rule[i];
                char valueChar = value[i];

                if (ruleChar == '?')
                    continue;           // ? 匹配任意字符
                if (ruleChar != valueChar)
                    return false;       // 非 ? 必须严格相等
            }
            return true;
        }

        // ============================================================
        // MES 接口调用
        // ============================================================

        /// <summary>
        /// 调用 MES GetCustomData 接口获取 SFC 条码匹配规则
        /// </summary>
        private void GetCustomData()
        {
            string ProjectID = g_DicMESConfig["Config"]["PROJECT_ID"];
            string ProductID = g_DicMESConfig["Config"]["PRODUCT_ID"];
            string StationID = g_DicMESConfig["Config"]["StationID"];
            string Line = g_DicMESConfig["Config"]["Line"];
            string ProjectName = g_DicMESConfig["Config"]["PROJECT"];
            string ProductName = g_DicMESConfig["Config"]["PRODUCT"];
            bool flag = FormHelper.GetCustomData(_mesUrl, _loginId, _clientId, ProjectID, ProductID, StationID, Line, ProjectName, ProductName, out string rule, out string config, out string msg);
            if (!flag)
            {
                AddLogMessage(msg, Color.Red);
            }
            else
            {
                AddLogMessage("GetCustomData成功");
                AddLogMessage("已加载条码规则：" + rule);
                SFCRule = rule;

                // 填充 ConfigCombox（分号分隔）
                if (!string.IsNullOrWhiteSpace(config))
                {
                    string[] configItems = config.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    ConfigCombox.SelectedIndexChanged -= ConfigCombox_SelectedIndexChanged;
                    ConfigCombox.DataSource = configItems;

                    string selectedConfig = g_DicMESConfig["SOFTWARE"].ContainsKey("selectedConfig")
                        ? g_DicMESConfig["SOFTWARE"]["selectedConfig"] : "";
                    bool configEditable = g_DicMESConfig["SOFTWARE"].ContainsKey("Config")
                        && bool.TryParse(g_DicMESConfig["SOFTWARE"]["Config"], out bool ce) && ce;

                    ConfigCombox.Enabled = configEditable;
                    ConfigCombox.Visible = configEditable;
                    uiLabel7.Visible = configEditable;

                    if (!string.IsNullOrWhiteSpace(selectedConfig) && configItems.Contains(selectedConfig))
                    {
                        ConfigCombox.SelectedItem = selectedConfig;
                    }
                    else
                    {
                        ConfigCombox.SelectedIndex = 0;
                    }

                    ConfigCombox.SelectedIndexChanged += ConfigCombox_SelectedIndexChanged;
                }
            }
        }

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

        /// <summary>
        /// 调用 MES Complete 接口，产品过站完成
        /// </summary>
        /// <param name="SFC">条码值</param>
        /// <returns>成功返回 true</returns>
        private bool Complete(string SFC)
        {
            try
            {
                string StationID = g_DicMESConfig["Config"]["StationID"];
                string SchingID = g_DicMESConfig["Config"]["SchedulingID"];
                string Remark = g_DicMESConfig["Config"]["Remark"];

                bool flag = FormHelper.Complete(_mesUrl, _loginId, _clientId, SFC, StationID, SchingID, Remark, out string msg);
                if (!flag)
                    AddLogMessage(msg, Color.Red);
                return flag;
            }
            catch (Exception ex)
            {
                AddLogMessage("Complete异常：" + ex.Message, Color.Red);
                return false;
            }
        }

        /// <summary>
        /// 调用 MES TestDataCollect2MainChild 接口上报测试数据，
        /// 成功后从每行 Remaining（剩余数量）中减去 Qty（每次扣减数），并保存到本地 XML
        /// </summary>
        /// <param name="sfcValue">当前 SFC 条码</param>
        /// <returns>成功返回 true</returns>
        private bool TestDataCollect(string sfcValue)
        {
            try
            {
                string lineNo = g_DicMESConfig["Config"]["Line"];
                string productName = g_DicMESConfig["Config"]["PRODUCT"];
                string projectName = g_DicMESConfig["Config"]["PROJECT"];
                string shoporderNo = g_DicMESConfig["Config"]["SapShoporder"];
                string testStation = g_DicMESConfig["Config"]["Operation"];
                string fixtureNo = g_DicMESConfig["Config"]["TraceStationId"];
                // 构建 TEST_DATA_LIST：遍历所有小件行
                var testDataList = new List<TestDataItem>();
                foreach (DataRow row in _partsTable.Rows)
                {
                    string bydpn = Convert.ToString(row[ColumnBydpn]);
                    string did = Convert.ToString(row[ColumnDid]);
                    testDataList.Add(new TestDataItem
                    {
                        NAME = bydpn,
                        VALUE = did,
                        TEST_RESULT = "PASS",
                        MAX_VALUE = "",
                        MIN_VALUE = "",
                        STANDARD_VALUE = ""
                    });
                }

                // 胶水校验启用且 DID 存在时，追加胶水信息
                if (_glueCheckEnabled && !string.IsNullOrEmpty(_glueDid))
                {
                    testDataList.Add(new TestDataItem
                    {
                        NAME = "胶水DID",
                        VALUE = _glueDid,
                        TEST_RESULT = "PASS",
                        MAX_VALUE = "",
                        MIN_VALUE = "",
                        STANDARD_VALUE = ""
                    });
                }

                // 追加机台号（machineNoswitch=false 时跳过）
                bool machineNoswitch = false;
                bool.TryParse(g_DicMESConfig["SOFTWARE"]["machineNoswitch"], out machineNoswitch);
                if (machineNoswitch)
                {
                    string machineNo = g_DicMESConfig["Setting"].ContainsKey("MACHINE_NO")
                        ? g_DicMESConfig["Setting"]["MACHINE_NO"] : "";
                    testDataList.Add(new TestDataItem
                    {
                        NAME = machineNo,
                        VALUE = fixtureNo,
                        TEST_RESULT = "PASS",
                        MAX_VALUE = "",
                        MIN_VALUE = "",
                        STANDARD_VALUE = ""
                    });
                }

                bool flag = FormHelper.TestDataCollect2MainChild(
                    _mesUrl, _loginId, _clientId,
                    lineNo, productName, projectName,
                    shoporderNo, sfcValue, testStation,
                    fixtureNo, "", "LOKI",
                    testDataList,
                    out string msg);

                if (!flag)
                {
                    AddLogMessage(msg, Color.Red);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AddLogMessage("TestDataCollect异常：" + ex.Message, Color.Red);
                return false;
            }
        }

        /// <summary>
        /// 第五步半：调用 MES Binding 接口扣料，成功后更新本地剩余数量
        /// </summary>
        private bool BindingDeduction(string sfcValue)
        {
            try
            {
                string shopOrder = g_DicMESConfig["Config"]["Resource"];
                string station = g_DicMESConfig["Config"]["Operation"];
                string loadId = g_DicMESConfig["Config"]["Load_ID"];
                string line = g_DicMESConfig["Config"]["Line"];
                string projectName = g_DicMESConfig["Config"]["PROJECT"];
                string productName = g_DicMESConfig["Config"]["PRODUCT"];
                string schedulingId = g_DicMESConfig["Config"]["SchedulingID"];

                string msg;
                bool ok = FormHelper.Binding(
                    _mesUrl, _loginId, _clientId, sfcValue, shopOrder, station,
                    loadId, line, projectName, productName, "1", schedulingId,
                    "true", out msg);

                if (!ok)
                {
                    AddLogMessage("Binding 扣料失败：" + msg, Color.Red);
                    return false;
                }

                // 接口成功后：每行剩余数量减去每次扣减数
                foreach (DataRow row in _partsTable.Rows)
                {
                    double remaining = Convert.ToDouble(row[ColumnRemaining]);
                    double deduct = Convert.ToDouble(row[ColumnQty]);
                    row[ColumnRemaining] = Math.Max(0.0, remaining - deduct);
                }
                SaveTable();

                return true;
            }
            catch (Exception ex)
            {
                AddLogMessage("Binding 扣料异常：" + ex.Message, Color.Red);
                return false;
            }
        }

        /// <summary>
        /// 第六步：调用 MES AddSfcKey 接口上报 Config 绑定结果，
        /// 仅当 setting.ini [SOFTWARE] Config=true 时执行
        /// </summary>
        /// <param name="sfcValue">当前 SFC 条码</param>
        /// <returns>成功返回 true</returns>
        private bool UploadConfigBinding(string sfcValue)
        {
            bool configEnabled = g_DicMESConfig["SOFTWARE"].ContainsKey("Config")
                && bool.TryParse(g_DicMESConfig["SOFTWARE"]["Config"], out bool ce) && ce;
            if (!configEnabled) return true;

            string dataName = g_DicMESConfig["SOFTWARE"].ContainsKey("addSfcKeyDataName")
                ? g_DicMESConfig["SOFTWARE"]["addSfcKeyDataName"] : "";
            string dataValue = ConfigCombox.SelectedItem?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(dataName) || string.IsNullOrWhiteSpace(dataValue))
            {
                AddLogMessage("AddSfcKey跳过：DATA_NAME或DATA_VALUE为空", Color.Red);
                return false;
            }

            bool flag = FormHelper.AddSfcKey(
                _mesUrl, _loginId, _clientId,
                sfcValue,
                g_DicMESConfig["Config"]["StationID"],
                g_DicMESConfig["Config"]["Operation"],
                g_DicMESConfig["Config"]["SapShoporder"],
                dataName,
                dataValue,
                g_DicMESConfig["Config"]["PROJECT_ID"],
                g_DicMESConfig["Config"]["PRODUCT_ID"],
                out string msg);

            if (!flag)
            {
                AddLogMessage("AddSfcKey失败：" + msg, Color.Red);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 第二步：胶水库存校验（GetAuxmStockList）
        /// </summary>
        /// <returns>校验成功返回 true，did 非空时缓存到 _glueDid</returns>
        private bool ValidateGlueStock()
        {
            _glueCheckEnabled = true;
            if (g_DicMESConfig.ContainsKey("SOFTWARE") && g_DicMESConfig["SOFTWARE"].ContainsKey("glueCheck"))
            {
                bool.TryParse(g_DicMESConfig["SOFTWARE"]["glueCheck"], out _glueCheckEnabled);
            }
            if (!_glueCheckEnabled) return true;

            string machineNo = g_DicMESConfig["Setting"].ContainsKey("MACHINE_NO") ? g_DicMESConfig["Setting"]["MACHINE_NO"] : "";
            string line = g_DicMESConfig["Config"]["Line"];
            bool glueOk = FormHelper.GetAuxmStockList(_mesUrl, _loginId, _clientId, line, machineNo, out string did, out string glueMsg);

            if (!glueOk || string.IsNullOrEmpty(did))
            {
                AddLogMessage("胶水校验失败：" + (string.IsNullOrEmpty(glueMsg) ? "did为空，无可用胶水" : glueMsg), Color.Red);
                return false;
            }

            _glueDid = did;
            AddLogMessage("胶水校验通过：did = " + did, Color.Green);
            return true;
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
        // 设备连接
        // ============================================================

        /// <summary>

        /// <summary>
        /// 创建 SimpleTcpClient 连接扫码枪，注册 DataReceived 回调，更新状态标签
        /// </summary>
        private void Connect_Scan(UILabel uiLabel)
        {
            scanclient?.Disconnect();
            scanclient = new SimpleTcpClient
            {
                StringEncoder = Encoding.UTF8,
                Delimiter = Encoding.UTF8.GetBytes("\r")[0]
            };
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

        /// <summary>
        /// 启动扫码枪健康检查定时器：每 3 秒探测一次，
        /// 若当前未连接且端口可达则触发重连
        /// </summary>
        private void StartScanHealthCheck()
        {
            _scanHealthTimer = new System.Windows.Forms.Timer { Interval = 3000 };
            _scanHealthTimer.Tick += (s, ev) =>
            {
                if (_scanReconnecting) return;

                bool reachable = ProbeScanner();
                if (_scanConnected)
                {
                    // 已连接但端口不可达 → 标记断开，下次 tick 自动重连
                    if (!reachable)
                    {
                        SetScanStatus(false);
                        AddLogMessage("扫码枪连接已断开", Color.Orange);
                    }
                    return;
                }

                if (reachable)
                {
                    AddLogMessage("检测到扫码枪端口可达，开始重连");
                    _ = ReconnectScanner();
                }
            };
            _scanHealthTimer.Start();
        }

        /// <summary>
        /// 用短超时 TcpClient 探测扫码枪端口是否可达，
        /// 使用独立连接，不干扰 SimpleTCP 主连接
        /// </summary>
        private bool BeginScanCycle()
        {
            lock (_scanCycleLock)
            {
                if (_scanCycleActive)
                    return false;

                _scanCycleActive = true;
                _scanArmed = true;
                _scanArmedTime = DateTime.Now;
            }

            StartScanTimeoutWatch();
            return true;
        }

        private bool TryConsumeScanCycle()
        {
            lock (_scanCycleLock)
            {
                if (!_scanCycleActive)
                    return false;

                _scanCycleActive = false;
                _scanArmed = false;
            }

            try { _scanTimeoutCts?.Cancel(); } catch (ObjectDisposedException) { }
            return true;
        }

        private void StartScanTimeoutWatch()
        {
            try { _scanTimeoutCts?.Cancel(); } catch (ObjectDisposedException) { }
            try { _scanTimeoutCts?.Dispose(); } catch (ObjectDisposedException) { }

            _scanTimeoutCts = new CancellationTokenSource();
            var token = _scanTimeoutCts.Token;
            double waitSeconds = timeout > 0 ? timeout : 2.0;

            Task.Run(async () =>
            {
                try
                {
                    double elapsed = 0;
                    const double retryInterval = 0.5;

                    while (elapsed < waitSeconds)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(retryInterval), token);
                        elapsed += retryInterval;

                        bool cycleActive;
                        lock (_scanCycleLock)
                        {
                            cycleActive = _scanCycleActive;
                        }

                        if (!cycleActive)
                            return; // 扫码数据已收到，正常退出

                        // 重发 start 指令
                        if (_scanConnected && !string.IsNullOrEmpty(_startOrder))
                        {
                            SendScannerStartOrder(_startOrder);
                        }
                    }

                    // 超时：扫码周期仍未结束
                    lock (_scanCycleLock)
                    {
                        if (!_scanCycleActive)
                            return;

                        _scanCycleActive = false;
                        _scanArmed = false;
                    }

                    var writeResult = PlcWrite(mesresult, 13);
                    BeginInvoke(new Action(() =>
                    {
                        AddLogMessage($"扫码超时：超过 {waitSeconds:F1}秒 未收到扫码数据", Color.Red);
                        AddLogMessage($"上位机写 [{mesresult}] = 13" + (writeResult.IsSuccess ? "" : " 失败：" + writeResult.Message), writeResult.IsSuccess ? Color.Green : Color.Red);
                        MarkFail();
                    }));
                }
                catch (TaskCanceledException) { }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    WriteLogs.WriteLog("扫码超时任务异常: " + ex);
                }
            });
        }

        private bool SendScannerStartOrder(string order)
        {
            try
            {
                var client = scanclient;
                if (client == null)
                    throw new InvalidOperationException("扫码枪客户端未初始化");

                client.Write(order);
                return true;
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    AddLogMessage("发送startorder失败：" + ex.Message, Color.Red);
                    SetScanStatus(false);
                }));
                if (!_scanReconnecting)
                    _ = ReconnectScanner();
                return false;
            }
        }

        private bool ProbeScanner()
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var ar = client.BeginConnect(_scanIp, _scanPort, null, null);
                    bool connected = ar.AsyncWaitHandle.WaitOne(1000);
                    // 无论成功与否都必须调用 EndConnect，否则 AsyncWaitHandle 泄漏
                    try { client.EndConnect(ar); } catch { }
                    if (connected && client.Connected)
                        return true;
                }
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 扫码枪重连：断开旧连接 → 创建新客户端 → 重订阅事件 → 尝试连接，
        /// 最多 5 次，每次间隔 3 秒，窗体关闭时通过 _cts 取消
        /// </summary>
        private async Task ReconnectScanner()
        {
            _scanReconnecting = true;
            var token = _cts?.Token ?? CancellationToken.None;

            for (int i = 1; i <= 5; i++)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    try { scanclient?.Disconnect(); } catch (Exception) { }
                    try { scanclient?.Dispose(); } catch (Exception) { }
                    scanclient = new SimpleTcpClient
                    {
                        StringEncoder = Encoding.UTF8,
                        Delimiter = Encoding.UTF8.GetBytes("\r")[0]
                    };
                    scanclient.DataReceived += scanclient_DataReceived;
                    scanclient.Connect(_scanIp, _scanPort);
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
        /// 更新扫码枪状态标签和内部状态（线程安全）
        /// </summary>
        private void SetScanStatus(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetScanStatus(connected)));
                return;
            }
            _scanConnected = connected;
            uiLabel5.Text = connected ? "已连接" : "未连接";
            uiLabel5.BackColor = connected ? Color.DodgerBlue : Color.Red;
        }

        /// <summary>
        /// 扫码枪数据接收回调（TCP 子线程）：
        /// 清洗条码 → 投递到 UI 线程 → 判断当前 Tab → 填入对应文本框
        /// </summary>
        private void scanclient_DataReceived(object sender, SimpleTCP.Message e)
        {
            // 清洗条码：去掉回车、换行、空格
            string sfc = e.MessageString.Replace("\r", "").Replace("\n", "").Replace(" ", "");

            if (this.InvokeRequired)
            {
                // UI 线程：信号验证 + UI 更新（轻量操作，不阻塞）
                this.BeginInvoke(new Action(() =>
                {
                    if (_startScanSwitchEnabled)
                    {
                        if (!TryConsumeScanCycle())
                        {
                            AddLogMessage("未处于扫码周期，忽略本次扫码", Color.Red);
                            SFC_UITextBox.Text = "";
                            return;
                        }
                    }

                    SFC_UITextBox.Text = sfc;

                    // 后台线程：MES 通信（HTTP 请求，不阻塞 UI 线程和定时器）
                    Task.Run(() => ruleSFC(sfc));
                }));

            }
        }

        // ============================================================
        // 小件数据管理
        // ============================================================

        /// <summary>
        /// 从 MES 接口重新加载小件定义，清空并重建表格数据（原有绑定将丢失）
        /// </summary>
        private void RefreshSubDefs()
        {
            try
            {
                string StationName = g_DicMESConfig["Config"]["Operation"];
                string loadId = g_DicMESConfig["Config"]["Load_ID"];

                // 调 MES 接口获取小件定义列表
                _allSubDefs = FormHelper.GetLoadSubsByLoadId(_mesUrl, _loginId, _clientId, loadId, StationName);

                // 清空现有行，重新填充
                _partsTable.Rows.Clear();
                if (_allSubDefs != null)
                {
                    foreach (var item in _allSubDefs)
                    {
                        _partsTable.Rows.Add(
                            item.Bydpn,      // 小件名称
                            item.Did,         // 绑定的小件码（初始为空）
                            item.Remarks,     // 总数（静态，不扣减）
                            item.Remarks,     // 剩余数量（动态，会扣减）
                            item.Qty,         // 每产品用量（每次扣减数）
                            item.DidRule,     // 条码匹配规则
                            item.Location,    // 小件位置号
                            item.MinSurplus,  // 最小剩余数量
                            item.StopQty,     // 停机数量
                            item.ClientNo     // 料号
                        );
                    }
                    AddLogMessage("GetLoadSubsByLoadId获取小件信息成功",Color.Green);
                }
            }
            catch (Exception ex)
            {
                AddLogMessage("加载小件定义失败：" + ex.Message, Color.Red);
            }
        }

        /// <summary>
        /// 启动时：始终先调接口获取最新小件定义，与本地 XML 比对后按规则合并
        /// </summary>
        private void LoadOrCreateTable()
        {
            _partsTable = new DataTable("Parts");
            CreateTableSchema();

            string StationName = g_DicMESConfig["Config"]["Operation"];
            string loadId = g_DicMESConfig["Config"]["Load_ID"];

            // 1. 尝试调接口获取最新小件定义
            List<SubMaterialDef> apiDefs = null;
            bool apiOk = false;
            try
            {
                apiDefs = FormHelper.GetLoadSubsByLoadId(_mesUrl, _loginId, _clientId, loadId, StationName);
                apiOk = true;
                _allSubDefs = apiDefs ?? new List<SubMaterialDef>();
            }
            catch (Exception ex)
            {
                AddLogMessage("启动时获取小件定义失败：" + ex.Message, Color.Red);
            }

            // 2. 接口成功
            if (apiOk && apiDefs != null)
            {
                if (File.Exists(_xmlPath))
                {
                    // 加载本地 XML 到临时表
                    var localTable = new DataTable("Parts");
                    try { localTable.ReadXml(_xmlPath); }
                    catch (Exception ex)
                    {
                        AddLogMessage("读取本地小件XML失败：" + ex.Message, Color.Red);
                    }

                    // 比对合并
                    MergeFromApi(apiDefs, localTable);
                }
                else
                {
                    // 无本地文件 → 首次运行，直接从接口填充
                    PopulateFromApiDefs(apiDefs);
                    SaveTable();
                    AddLogMessage("首次运行：已从接口获取小件数据并保存到本地。", Color.Green);
                }

                // 逐个调用 GetLoadUpByParams 更新剩余数量
                FetchAndUpdateQtyResiduals();
                return;
            }

            // 3. 接口失败 → 兜底用本地 XML
            if (File.Exists(_xmlPath))
            {
                try
                {
                    _partsTable.ReadXml(_xmlPath);
                    EnsureRequiredColumns();
                    AddLogMessage("接口获取失败，使用本地缓存的小件数据。", Color.Orange);
                }
                catch (Exception ex)
                {
                    AddLogMessage("读取本地小件XML失败：" + ex.Message, Color.Red);
                }
            }
            else
            {
                AddLogMessage("无法获取小件数据：接口不可达且无本地缓存。", Color.Red);
                SetStatus("小件数据加载失败");
            }
        }

        /// <summary>
        /// 以接口返回的 apiDefs 为准，与本地 localTable 逐条比对合并：
        /// - bydpn 相同 + DidRule/Remarks 都未变 → 保留 Did
        /// - bydpn 相同 + DidRule/Remarks 有变化 → 清空 Did
        /// - 仅接口有 → 新行，Did 为空
        /// - 仅本地有 → 移除
        /// </summary>
        private void MergeFromApi(List<SubMaterialDef> apiDefs, DataTable localTable)
        {
            // 先检查是否完全一致（条目数 + bydpn + DidRule 全部相同）
            bool fullyConsistent = true;
            if (apiDefs.Count != localTable.Rows.Count)
            {
                fullyConsistent = false;
            }
            else
            {
                foreach (var def in apiDefs)
                {
                    var localRows = localTable.Select($"Bydpn = '{def.Bydpn.Replace("'", "''")}'");
                    if (localRows.Length != 1)
                    {
                        fullyConsistent = false;
                        break;
                    }
                    var localDidRule = Convert.ToString(localRows[0][ColumnDidRule]) ?? "";
                    if (!string.Equals(def.DidRule?.Trim(), localDidRule.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        fullyConsistent = false;
                        break;
                    }
                }
            }

            if (fullyConsistent)
            {
                // 完全一致：直接使用本地数据，不写不覆盖
                _partsTable = localTable;
                _partsTable.TableName = "Parts";
                EnsureRequiredColumns();
                AddLogMessage("小件信息一致，无需更新。", Color.Green);
                return;
            }

            // 不一致 → 合并
            int keptCount = 0, clearedCount = 0, addedCount = 0, removedCount = 0;

            _partsTable.Rows.Clear();

            foreach (var def in apiDefs)
            {
                var localRows = localTable.Select($"Bydpn = '{def.Bydpn.Replace("'", "''")}'");

                if (localRows.Length == 1)
                {
                    var localRow = localRows[0];
                    var localDidRule = Convert.ToString(localRow[ColumnDidRule]) ?? "";
                    var localRemarks = Convert.ToDouble(localRow[ColumnRemarks]);
                    double apiRemarks;
                    double.TryParse(def.Remarks ?? "0", out apiRemarks);

                    bool ruleSame = string.Equals(def.DidRule?.Trim(), localDidRule.Trim(), StringComparison.OrdinalIgnoreCase);
                    bool remarksSame = Math.Abs(localRemarks - apiRemarks) < 0.001;

                    if (ruleSame && remarksSame)
                    {
                        // 保留本地行（含 Did 绑定），但更新可能变化的字段
                        var did = Convert.ToString(localRow[ColumnDid]) ?? "";
                        double remaining = Convert.ToDouble(localRow[ColumnRemaining]);
                        _partsTable.Rows.Add(
                            def.Bydpn, did, def.Remarks, remaining, def.Qty,
                            def.DidRule, def.Location, def.MinSurplus, def.StopQty, def.ClientNo);
                        keptCount++;
                    }
                    else
                    {
                        // 规则或数量变了 → 清空 Did
                        string reason = "";
                        if (!ruleSame) reason += "规则变更";
                        if (!remarksSame) reason += (reason.Length > 0 ? "、" : "") + "数量变更";
                        AddLogMessage($"[{def.Bydpn}] {reason}，已清空绑定的小件码。", Color.Orange);
                        _partsTable.Rows.Add(
                            def.Bydpn, "", def.Remarks, def.Remarks, def.Qty,
                            def.DidRule, def.Location, def.MinSurplus, def.StopQty, def.ClientNo);
                        clearedCount++;
                    }
                }
                else
                {
                    // 接口新增的小件
                    _partsTable.Rows.Add(
                        def.Bydpn, "", def.Remarks, def.Remarks, def.Qty,
                        def.DidRule, def.Location, def.MinSurplus, def.StopQty, def.ClientNo);
                    AddLogMessage($"[{def.Bydpn}] 接口新增小件，需重新绑定。", Color.Blue);
                    addedCount++;
                }
            }

            // 统计被删除的（本地有、接口无）
            foreach (DataRow localRow in localTable.Rows)
            {
                var localBydpn = Convert.ToString(localRow[ColumnBydpn]) ?? "";
                if (!apiDefs.Any(d => string.Equals(d.Bydpn, localBydpn, StringComparison.OrdinalIgnoreCase)))
                {
                    var did = Convert.ToString(localRow[ColumnDid]) ?? "";
                    AddLogMessage($"[{localBydpn}] 接口已删除此小件" + (string.IsNullOrWhiteSpace(did) ? "" : $"，绑定码 {did} 已失效") + "。", Color.Orange);
                    removedCount++;
                }
            }

            SaveTable();
            AddLogMessage($"小件合并完成：保留 {keptCount} | 规则/数量变更清空 {clearedCount} | 新增 {addedCount} | 删除 {removedCount}", Color.Green);
        }

        /// <summary>
        /// 从接口定义列表直接填充 _partsTable（不比对本地）
        /// </summary>
        private void PopulateFromApiDefs(List<SubMaterialDef> defs)
        {
            _partsTable.Rows.Clear();
            foreach (var item in defs)
            {
                _partsTable.Rows.Add(
                    item.Bydpn,
                    item.Did,
                    item.Remarks,
                    item.Remarks,
                    item.Qty,
                    item.DidRule,
                    item.Location,
                    item.MinSurplus,
                    item.StopQty,
                    item.ClientNo
                );
            }
        }

        /// <summary>
        /// 逐个调用 GetLoadUpByParams 获取每个小件的最新剩余数量，
        /// 覆盖本地剩余数量，并在数量不足时告警
        /// </summary>
        private void FetchAndUpdateQtyResiduals()
        {
            string shoporder = g_DicMESConfig["Config"]["Resource"];
            string line = g_DicMESConfig["Config"]["Line"];
            bool stopped = false;
            int updatedCount = 0;

            foreach (DataRow row in _partsTable.Rows)
            {
                string location = Convert.ToString(row[ColumnLocation]) ?? "";
                string bydpn = Convert.ToString(row[ColumnBydpn]) ?? "";
                int stopQty;
                int.TryParse(Convert.ToString(row[ColumnStopQty]) ?? "0", out stopQty);

                if (string.IsNullOrWhiteSpace(location))
                {
                    AddLogMessage($"[{bydpn}] 位置号为空，跳过获取剩余数量。", Color.Orange);
                    continue;
                }

                var result = FormHelper.GetLoadUpByParams(
                    _mesUrl, _loginId, _clientId, shoporder, location, line);

                if (!result.Ok)
                {
                    AddLogMessage($"[{bydpn}] GetLoadUpByParams 网络异常，跳过。", Color.Orange);
                    continue;
                }

                if (!string.IsNullOrEmpty(result.FailMessage))
                {
                    // RESULT=FAIL → 立即停止
                    AddLogMessage($"接口返回失败：{result.FailMessage}", Color.Red);
                    stopped = true;
                    break;
                }

                if (!result.Found)
                {
                    // LoadUps 为空 → 告警并写 D3056=1
                    AddLogMessage($"[{bydpn}] 位置 {location} 小件数量不足，请及时上料！", Color.Red);
                    Task.Run(() => PlcWrite("D3056", (short)1));
                    BeginInvoke(new Action(() => AddLogMessage("上位机写 [D3056] = 1", Color.Green)));
                    continue;
                }

                // 更新剩余数量
                row[ColumnRemaining] = result.QtyResidual;
                updatedCount++;

                if (result.QtyResidual <= stopQty)
                {
                    AddLogMessage($"[{bydpn}] 位置 {location} 剩余 {result.QtyResidual}（停机数量 {stopQty}），小件数量不足，请及时上料！", Color.Red);
                    Task.Run(() => PlcWrite("D3056", (short)1));
                    BeginInvoke(new Action(() => AddLogMessage("上位机写 [D3056] = 1", Color.Green)));
                }
                else
                {
                    AddLogMessage($"[{bydpn}] 位置 {location} 剩余数量已更新：{result.QtyResidual}", Color.Green);
                }
            }

            if (!stopped)
            {
                SaveTable();
                AddLogMessage($"剩余数量更新完成：共更新 {updatedCount} 个小件。", Color.Green);
            }
            else
            {
                AddLogMessage("因接口返回失败，已停止剩余数量更新流程。", Color.Red);
            }
        }

        /// <summary>
        /// 如果 XML 文件列结构不完整（如旧版本文件），补齐必需列
        /// </summary>
        private void EnsureRequiredColumns()
        {
            AddColumnIfMissing(ColumnBydpn, typeof(string));
            AddColumnIfMissing(ColumnDid, typeof(string));
            AddColumnIfMissing(ColumnRemarks, typeof(double));
            AddColumnIfMissing(ColumnRemaining, typeof(double));
            AddColumnIfMissing(ColumnQty, typeof(double));
            AddColumnIfMissing(ColumnDidRule, typeof(string));
            AddColumnIfMissing(ColumnLocation, typeof(string));
            AddColumnIfMissing(ColumnMinSurplus, typeof(int));
            AddColumnIfMissing(ColumnStopQty, typeof(string));
            AddColumnIfMissing(ColumnClientNo, typeof(string));
        }

        /// <summary>
        /// 如果指定列不存在则添加到 DataTable
        /// </summary>
        private void AddColumnIfMissing(string columnName, Type type)
        {
            if (!_partsTable.Columns.Contains(columnName))
            {
                _partsTable.Columns.Add(columnName, type);
            }
        }

        /// <summary>
        /// DataTable 数据每次变更后立即写回本地 XML
        /// </summary>
        private void SaveTable()
        {
            try
            {
                _partsTable.WriteXml(_xmlPath, XmlWriteMode.WriteSchema);
            }
            catch (Exception ex)
            {
                AddLogMessage("保存小件XML失败：" + ex.Message, Color.Red);
            }
        }

        /// <summary>
        /// 创建 DataTable 的列结构（5 列）
        /// </summary>
        private void CreateTableSchema()
        {
            _partsTable.Columns.Add(ColumnBydpn, typeof(string));
            _partsTable.Columns.Add(ColumnDid, typeof(string));
            _partsTable.Columns.Add(ColumnRemarks, typeof(double));
            _partsTable.Columns.Add(ColumnRemaining, typeof(double));
            _partsTable.Columns.Add(ColumnQty, typeof(double));
            _partsTable.Columns.Add(ColumnDidRule, typeof(string));
            _partsTable.Columns.Add(ColumnLocation, typeof(string));
            _partsTable.Columns.Add(ColumnMinSurplus, typeof(int));
            _partsTable.Columns.Add(ColumnStopQty, typeof(string));
            _partsTable.Columns.Add(ColumnClientNo, typeof(string));
        }

        /// <summary>
        /// 设置底部状态栏文字
        /// </summary>
        private void SetStatus(string message)
        {
            lblStatus.Text = "状态：" + message;
        }

        /// <summary>
        /// 将 DataTable 绑定到 DataGridView，设置中文列头，自动调整列宽和行高
        /// </summary>
        private void BindGrid()
        {
            dataGridViewParts.DataSource = _partsTable;

            // 设置中文列头
            dataGridViewParts.Columns[ColumnBydpn].HeaderText = "小件名称";
            dataGridViewParts.Columns[ColumnDid].HeaderText = "小件码（待绑定）";
            dataGridViewParts.Columns[ColumnRemarks].HeaderText = "总数";
            dataGridViewParts.Columns[ColumnRemaining].HeaderText = "剩余数量";
            dataGridViewParts.Columns[ColumnQty].HeaderText = "每次扣的数量";
            dataGridViewParts.Columns[ColumnDidRule].HeaderText = "规则";
            dataGridViewParts.Columns[ColumnLocation].HeaderText = "位置号";
            dataGridViewParts.Columns[ColumnMinSurplus].HeaderText = "最小剩余";
            dataGridViewParts.Columns[ColumnStopQty].HeaderText = "停机数量";
            dataGridViewParts.Columns[ColumnClientNo].HeaderText = "料号";

            // 添加操作按钮列（重置单条绑定）
            if (!dataGridViewParts.Columns.Contains("btnResetRow"))
            {
                var btnCol = new DataGridViewButtonColumn
                {
                    Name = "btnResetRow",
                    HeaderText = "操作",
                    Text = "重置",
                    UseColumnTextForButtonValue = true,
                    FlatStyle = FlatStyle.Flat
                };
                dataGridViewParts.Columns.Add(btnCol);
            }

            // 只注册一次事件
            dataGridViewParts.CellContentClick -= DataGridViewParts_CellContentClick;
            dataGridViewParts.CellContentClick += DataGridViewParts_CellContentClick;

            // 自动调整列宽和行高以适应内容
            dataGridViewParts.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            dataGridViewParts.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
        }

        /// <summary>
        /// 单行重置：清空该行 Did，恢复剩余数量为总数，保存并刷新
        /// </summary>
        private void DataGridViewParts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dataGridViewParts.Columns[e.ColumnIndex].Name != "btnResetRow") return;

            DataRow row = _partsTable.Rows[e.RowIndex];
            string bydpn = Convert.ToString(row[ColumnBydpn]);
            string currentDid = Convert.ToString(row[ColumnDid]);

            if (string.IsNullOrWhiteSpace(currentDid))
            {
                MessageBox.Show($"第 {e.RowIndex + 1} 行 [{bydpn}] 当前未绑定，无需重置。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            row[ColumnDid] = DBNull.Value;
            row[ColumnRemaining] = row[ColumnRemarks];
            SaveTable();
            dataGridViewParts.Refresh();
            AddLogMessage($"已重置第 {e.RowIndex + 1} 行 [{bydpn}] 的绑定记录。");
        }

        // ============================================================
        // 小件上料页
        // ============================================================

        /// <summary>
        /// 清空所有已绑定的小件码（重置第二列 Did）；
        /// 操作前统计并确认，清空后立即保存到本地 XML
        /// </summary>
        private void btnResetBindings_Click(object sender, EventArgs e)
        {
            // 统计已绑定数量
            int boundCount = _partsTable.Rows.Cast<DataRow>()
                .Count(row => !string.IsNullOrWhiteSpace(Convert.ToString(row[ColumnDid])));

            if (boundCount == 0)
            {
                MessageBox.Show("当前没有已绑定的小件码可重置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 二次确认
            DialogResult result = MessageBox.Show(
                string.Format("确认要清空全部 {0} 个已绑定的小件码吗？", boundCount),
                "确认重置绑定",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // 清空所有 Did 列
            foreach (DataRow row in _partsTable.Rows)
            {
                row[ColumnDid] = string.Empty;
                row[ColumnRemaining] = row[ColumnRemarks];
            }

            SaveTable();
            dataGridViewParts.Refresh();
            RefreshMatchCandidates();
            SetStatus(string.Format("已重置 {0} 个已绑定的小件码，并同步保存到本地 XML。", boundCount));
        }

        /// <summary>
        /// 根据输入的小件码，匹配所有"未绑定且规则命中"的行，填充下拉框供用户选择
        /// </summary>
        private void RefreshMatchCandidates()
        {
            string inputCode = txtInputCode.Text.Trim();
            var candidates = new List<MatchCandidate>();

            cmbMatches.DataSource = null;

            if (string.IsNullOrWhiteSpace(inputCode))
            {
                SetStatus("请输入小件码后再匹配。");
                return;
            }

            // 遍历所有行，筛选出规则匹配的行（含已绑定）
            for (int i = 0; i < _partsTable.Rows.Count; i++)
            {
                DataRow row = _partsTable.Rows[i];
                string existingCode = Convert.ToString(row[ColumnDid]).Trim();
                string rule = Convert.ToString(row[ColumnDidRule]).Trim();
                bool isBound = !string.IsNullOrEmpty(existingCode);

                // 无规则则跳过
                if (string.IsNullOrEmpty(rule))
                    continue;
                // 规则不匹配则跳过
                if (!IsRuleMatch(inputCode, rule))
                    continue;

                string bydpn = Convert.ToString(row[ColumnBydpn]);

                string prefix = isBound ? "⚠ 已绑定 | " : "";
                candidates.Add(new MatchCandidate
                {
                    RowIndex = i,
                    IsBound = isBound,
                    DisplayText = string.Format(
                        "{0}第 {1} 行 | 名称: {2} | 规则: {3}" + (isBound ? " | 已绑定: {4}" : ""),
                        prefix, i + 1, bydpn, rule, existingCode)
                });
            }

            // 绑定到下拉框
            _populatingMatches = true;
            cmbMatches.DisplayMember = nameof(MatchCandidate.DisplayText);
            cmbMatches.ValueMember = nameof(MatchCandidate.RowIndex);
            cmbMatches.DataSource = candidates;
            cmbMatches.SelectedIndex = -1;
            _populatingMatches = false;

            if (candidates.Count == 0)
            {
                SetStatus("没有找到匹配项。请检查输入的小件码是否符合规则。");
                return;
            }

            if (candidates.Count == 1 && !candidates[0].IsBound)
            {
                // 唯一匹配且未绑定 → 自动绑定
                ExecuteBind(inputCode, candidates[0]);
                return;
            }

            if (candidates.Count == 1 && candidates[0].IsBound)
            {
                // 唯一匹配且已绑定 → 弹窗确认上新小件
                var single = candidates[0];
                DataRow boundRow = _partsTable.Rows[single.RowIndex];
                string oldDid = Convert.ToString(boundRow[ColumnDid]);
                string boundBydpn = Convert.ToString(boundRow[ColumnBydpn]);
                var dlgResult = MessageBox.Show(
                    string.Format("该位置已绑定小件码 [{0}]（{1}）。\n是否更换为新小件码 [{2}]？\n\n点击[是]将先卸下旧小件，再上新的小件码。",
                        oldDid, boundBydpn, inputCode),
                    "上新小件确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dlgResult == DialogResult.Yes)
                {
                    ReplaceAndLoadNew(inputCode, boundRow);
                    txtInputCode.Clear();
                    cmbMatches.DataSource = null;
                }
                else
                {
                    SetStatus("已取消上新小件。");
                }
                return;
            }

            BeginInvoke(new Action(() =>
            {
                cmbMatches.ShowDropDown();
            }));
            SetStatus(string.Format("找到 {0} 个匹配项，请从下拉框中选择。", candidates.Count));
        }

        /// <summary>
        /// 判断输入小件码是否命中规则组中的任意一个模板（分号分隔的多个规则）
        /// </summary>
        private bool IsRuleMatch(string inputCode, string ruleGroup)
        {
            if (string.IsNullOrWhiteSpace(inputCode) || string.IsNullOrWhiteSpace(ruleGroup))
                return false;

            string[] ruleItems = ruleGroup
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string ruleItem in ruleItems)
            {
                if (IsSinglePatternMatch(inputCode, ruleItem.Trim()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断输入码是否匹配单条模板规则：
        /// 长度必须一致，? 匹配任意字符，非 ? 字符忽略大小写严格相等
        /// </summary>
        private bool IsSinglePatternMatch(string inputCode, string pattern)
        {
            if (string.IsNullOrWhiteSpace(inputCode) || string.IsNullOrWhiteSpace(pattern))
                return false;
            if (inputCode.Length != pattern.Length)
                return false;

            for (int i = 0; i < pattern.Length; i++)
            {
                char ruleChar = pattern[i];
                char inputChar = inputCode[i];

                if (ruleChar == '?')
                    continue;
                if (char.ToUpperInvariant(ruleChar) != char.ToUpperInvariant(inputChar))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 执行绑定：将用户输入的小件码写入对应 DataRow 的 Did 列；
        /// 绑定成功后立即落盘并刷新界面
        /// </summary>
        /// <summary>
        /// 执行绑定：将小件码写入目标行，落盘并清空输入
        /// </summary>
        private void ExecuteBind(string inputCode, MatchCandidate candidate)
        {
            // 防止同一个小件码重复绑定到多行
            bool alreadyBound = _partsTable.Rows.Cast<DataRow>()
                .Any(row => string.Equals(
                    Convert.ToString(row[ColumnDid]).Trim(),
                    inputCode,
                    StringComparison.OrdinalIgnoreCase));

            if (alreadyBound)
            {
                MessageBox.Show("该小件码已经绑定过，不能重复使用。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetStatus("绑定失败：输入的小件码已存在于表格第二列。");
                return;
            }

            DataRow targetRow = _partsTable.Rows[candidate.RowIndex];

            // 已绑定行 → 弹窗确认上新小件
            if (candidate.IsBound)
            {
                string oldDid = Convert.ToString(targetRow[ColumnDid]);
                string boundBydpn = Convert.ToString(targetRow[ColumnBydpn]);
                var dlgResult = MessageBox.Show(
                    string.Format("该位置已绑定小件码 [{0}]（{1}）。\n是否更换为新小件码 [{2}]？\n\n点击[是]将先卸下旧小件，再上新的小件码。",
                        oldDid, boundBydpn, inputCode),
                    "上新小件确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dlgResult == DialogResult.Yes)
                {
                    ReplaceAndLoadNew(inputCode, targetRow);
                }
                else
                {
                    SetStatus("已取消上新小件。");
                }
                txtInputCode.Clear();
                cmbMatches.DataSource = null;
                return;
            }

            // 未绑定的新行 → 先调 LoadMaterialUp 上料，再绑定
            string location = Convert.ToString(targetRow[ColumnLocation]) ?? "";
            string bydpn = Convert.ToString(targetRow[ColumnBydpn]) ?? "";
            string remarks = Convert.ToString(targetRow[ColumnRemarks]) ?? "0";
            string station = g_DicMESConfig["Config"]["Operation"];
            string shoporder = g_DicMESConfig["Config"]["Resource"];
            string line = g_DicMESConfig["Config"]["Line"];
            string loadId = g_DicMESConfig["Config"]["Load_ID"];
            string dateCode = g_DicMESConfig["Config"]["LoginID"];

            string loadUpMsg;
            bool loadUpOk = FormHelper.LoadMaterialUp(
                _mesUrl, _loginId, _clientId, loadId, location, bydpn, station,
                line, shoporder, inputCode, remarks, dateCode, out loadUpMsg);

            if (!loadUpOk)
            {
                AddLogMessage($"LoadMaterialUp 失败：{loadUpMsg}", Color.Red);
                MessageBox.Show($"上料失败：{loadUpMsg}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("上料失败，请重试。");
                return;
            }
            AddLogMessage($"LoadMaterialUp 成功：小件码 [{inputCode}] 已上到 {bydpn}（{location}）", Color.Green);

            targetRow[ColumnDid] = inputCode;
            SaveTable();
            dataGridViewParts.Refresh();

            string boundPartName = Convert.ToString(targetRow[ColumnBydpn]);
            SetStatus(string.Format("绑定成功：小件码 {0} 已写入 [{1}] 这一行。", inputCode, boundPartName));

            txtInputCode.Clear();
            cmbMatches.DataSource = null;
        }

        /// <summary>
        /// 上新小件：先调 UpdateLoadDidInfo 下旧小件，再调 LoadMaterialUp 上新的小件码
        /// </summary>
        private void ReplaceAndLoadNew(string newDid, DataRow row)
        {
            string oldDid = Convert.ToString(row[ColumnDid]) ?? "";
            string location = Convert.ToString(row[ColumnLocation]) ?? "";
            string bydpn = Convert.ToString(row[ColumnBydpn]) ?? "";
            int stopQty;
            int.TryParse(Convert.ToString(row[ColumnStopQty]) ?? "0", out stopQty);
            double remaining = Convert.ToDouble(row[ColumnRemaining]);

            string station = g_DicMESConfig["Config"]["Operation"];
            string shoporder = g_DicMESConfig["Config"]["Resource"];
            string line = g_DicMESConfig["Config"]["Line"];
            string loadId = g_DicMESConfig["Config"]["Load_ID"];
            string dateCode = g_DicMESConfig["Config"]["LoginID"];

            // 判断 STATE：≤stopQty 或 ==0 → 用完(1)，否则 → 未完卸下(2)
            string state = (remaining <= stopQty || remaining == 0) ? "1" : "2";
            string stateDesc = state == "1" ? "用完" : "未完卸下";

            AddLogMessage($"开始上新小件：旧码 [{oldDid}] → 新码 [{newDid}]（{bydpn}，STATE={state}（{stateDesc}））", Color.Blue);

            // 1. 下旧小件
            string msg1;
            bool ok1 = FormHelper.UpdateLoadDidInfo(
                _mesUrl, _loginId, _clientId, oldDid, station, shoporder, line,
                state, loadId, location, out msg1);

            if (!ok1)
            {
                AddLogMessage($"UpdateLoadDidInfo 失败：{msg1}", Color.Red);
                MessageBox.Show($"下旧小件失败：{msg1}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AddLogMessage($"UpdateLoadDidInfo 成功：旧小件 [{oldDid}] STATE={state}（{stateDesc}）", Color.Green);

            // 2. 上新的小件码
            string remarks = Convert.ToString(row[ColumnRemarks]) ?? "0";
            string msg2;
            bool ok2 = FormHelper.LoadMaterialUp(
                _mesUrl, _loginId, _clientId, loadId, location, bydpn, station,
                line, shoporder, newDid, remarks, dateCode, out msg2);

            if (!ok2)
            {
                AddLogMessage($"LoadMaterialUp 失败：{msg2}", Color.Red);
                MessageBox.Show($"上新小件失败：{msg2}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AddLogMessage($"LoadMaterialUp 成功：新小件 [{newDid}] 已上到 {bydpn}（{location}）", Color.Green);

            // 3. 更新本地行
            row[ColumnDid] = newDid;
            row[ColumnRemaining] = Convert.ToDouble(remarks);
            SaveTable();
            dataGridViewParts.Refresh();

            SetStatus($"上新小件成功：{bydpn} [{oldDid}] → [{newDid}]，剩余数量重置为 {remarks}");
        }

        private void cmbMatches_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_populatingMatches)
                return;

            string inputCode = txtInputCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(inputCode))
                return;

            if (cmbMatches.SelectedItem == null)
                return;

            var selectedCandidate = (MatchCandidate)cmbMatches.SelectedItem;
            ExecuteBind(inputCode, selectedCandidate);
        }

        /// <summary>
        /// ConfigCombox 选择变更时，保存到 setting.ini
        /// </summary>
        private void ConfigCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ConfigCombox.SelectedItem != null)
            {
                new ConfigService().SaveSelectedConfig(ConfigCombox.SelectedItem.ToString());
            }
        }

        /// <summary>
        /// 小件码输入框内容变化时，实时刷新匹配候选项
        /// </summary>
        private void txtInputCode_TextChanged(object sender, EventArgs e)
        {
            RefreshMatchCandidates();
        }
    }
}
