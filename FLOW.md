# 磁铁组装自动化 — 扫码过站软件流程文档

## 一、启动流程

```
MainForm()
  ├─ LoadAllConfig()          读取 mes_config.ini + setting.ini
  └─ 加载 Pass.wav / alert.wav

Form1_Load
  ├─ CacheConfig()            缓存 _mesUrl / _loginId / _clientId
  ├─ InitSettings()           读取 logStation / 扫码枪 IP:Port / CCD IP:Port
  ├─ LoadCounters()           从 counters.json 恢复 PASS/FAIL 计数
  ├─ ConnectToScanner()       TCP 连接扫码枪 (SimpleTCP)
  ├─ ConnectToCcd()           TCP 连接 CCD (SimpleTCP)
  ├─ StartScanHealthCheck()   每 3s 探测扫码枪，断线自动重连(最多 5 次)
  ├─ StartCcdHealthCheck()    每 3s 探测 CCD，断线自动重连(最多 5 次)
  └─ StartShiftTimer()        每 30s 检查 8:00/20:00 换班清零
```

---

## 二、核心过站流程

```
扫码枪 TCP 收到条码
  │
  ├─ scanclient_DataReceived
  │   清洗: 去 \r \n 空格
  │   BeginInvoke → UI 线程回显 SFC_UITextBox.Text
  │   Task.Run 异步执行 ↓
  │
  ├─ ① ValidateSfcLog(sfc)
  │   │
  │   │  [接口] GetSfcLogListByParam
  │   │    入参: LOGIN_ID, CLIENT_ID, SFC
  │   │    返回: DATA[] (每条含 SFC_LOG.{logStation, logAction, remark, ...})
  │   │
  │   │  处理:
  │   │    a. 过滤掉 logStation == 本地 _logStation 的条目
  │   │    b. 取剩余第一条
  │   │    c. 检查 logAction == "REWORK" && remark 包含 _logStation
  │   │
  │   ├─ ✅ 通过 → _sfcLogPassed = true,  记录 _firstLogStation
  │   └─ ❌ 不通过 → _sfcLogPassed = false（不影响流程，只跳过 ChangeSfcStation）
  │
  ├─ ② ruleSFC(sfc)
  │   │  若 _ccdArmed=true（CCD 忙）→ 拒绝，MarkFail
  │   │
  │   │  [接口] Start
  │   │    入参: LOGIN_ID, CLIENT_ID, SFC, STATION_NAME(=Operation),
  │   │          LINE, SHOPORDER(=SapShoporder), SCHEDULING_ID
  │   │
  │   ├─ 失败 → MarkFail() → return
  │   └─ 成功 → _ccdArmed=true, _ccdPendingSfc=sfc, 等待CCD
  │
  └─ ③ CCD TCP 收到判定结果
      │
      ├─ ccdClient_DataReceived
      │   清洗: 取第一行，去空格
      │   若 _ccdArmed=false → 忽略（未就绪）
      │   _ccdArmed=false → ProcessCcdResult(raw)
      │
      └─ ProcessCcdResult(raw)
          格式: "OK;..." 或 "NG;..."
          │
          ├─ [必执行] AddSfcKey (CCD 上报)
          │    入参: LOGIN_ID, CLIENT_ID, SFC, STATION_ID, STATION_NAME(=Operation),
          │          SHOPORDER(=SapShoporder), DATA_NAME(=ccdDataName, 默认"CCD"),
          │          DATA_VALUE(=CCD 原始字符串), PROJECT_ID, PRODUCT_ID
          │
          ├─ OK → Complete
          │    入参: LOGIN_ID, CLIENT_ID, SFC, STATION_ID,
          │          SCHEDULING_ID, REMARK
          │    → PASS+1, 绿色标签, 播放 Pass.wav
          │
          ├─ NG → NcComplete
          │    入参: LOGIN_ID, CLIENT_ID, SFC, STATION_ID,
          │          NC_CODE="CCD NG", NC_CONTEXT=raw, NC_TYPE="CCD",
          │          SCHEDULING_ID
          │    → FAIL+1, 红色标签, 播放 alert.wav
          │
          └─ [条件] 仅当 _sfcLogPassed=true:
              ├─ ChangeSfcStation
              │    入参: LOGIN_ID, CLIENT_ID, SFC,
              │          NEW_STATION_NAME = _firstLogStation
              │
              └─ AddSfcKey (补充业务数据上报)
                   入参: 同上，但 DATA_NAME = addSfcKeyDataName (如 "logo_sn"),
                         DATA_VALUE = CCD 原始字符串
```

---

## 三、MES 接口汇总

| # | 方法 | URL | 调用时机 | 入参 |
|---|------|-----|---------|------|
| 1 | **GetSfcLogListByParam** | `{JSONURL}?method=GetSfcLogListByParam` | 扫码后、Start 前 | `LOGIN_ID, CLIENT_ID, SFC` |
| 2 | **Start** | `{JSONURL}?method=Start` | SFC Log 校验后 | `LOGIN_ID, CLIENT_ID, SFC, STATION_NAME, LINE, SHOPORDER, SCHEDULING_ID` |
| 3 | **AddSfcKey** | `{JSONURL}?method=AddSfcKey` | CCD 结果到达（OK/NG 都调） | `LOGIN_ID, CLIENT_ID, SFC, STATION_ID, STATION_NAME, SHOPORDER, DATA_NAME, DATA_VALUE, PROJECT_ID, PRODUCT_ID` |
| 4a | **Complete** | `{JSONURL}?method=Complete` | CCD OK | `LOGIN_ID, CLIENT_ID, SFC, STATION_ID, SCHEDULING_ID, REMARK` |
| 4b | **NcComplete** | `{JSONURL}?method=NcComplete` | CCD NG | `LOGIN_ID, CLIENT_ID, SFC, STATION_ID, NC_CODE, NC_CONTEXT, NC_TYPE, SCHEDULING_ID` |
| 5 | **ChangeSfcStation** | `{JSONURL}?method=ChangeSfcStation` | CCD 判定后，仅 Log 校验通过 | `LOGIN_ID, CLIENT_ID, SFC, NEW_STATION_NAME` |
| 6 | **AddSfcKey** (补充) | `{JSONURL}?method=AddSfcKey` | CCD 判定后，仅 Log 校验通过 + addSfcKeyDataName 非空 | 同 #3，DATA_NAME 取 addSfcKeyDataName |

---

## 四、配置文件

### mes_config.ini

```ini
[SYSTEM]
JSONURL=http://10.6.78.14/Service.action       # MES 接口根地址

[Config]
LoginID=-1                                       # 登录 ID
ClientID=1                                       # 客户端 ID
PROJECT_ID=1481                                  # 项目 ID
PRODUCT_ID=3868                                  # 产品 ID
StationID=13374                                  # 当前工站 ID
Line=F1-1F-Roll-A                                # 产线
Operation=磁铁组装1                               # 工站名 (STATION_NAME)
SapShoporder=Polaris_CAP_Rolling_EVT_MAIN        # 工单号 (SHOPORDER)
SchedulingID=9821                                # 排程 ID
Remark=bezel-height                              # 备注 (可选)
```

### setting.ini

```ini
[SCAN]
ip=127.0.0.1        # 扫码枪 IP
port=502            # 扫码枪端口

[CCD]
ip=127.0.0.1        # CCD IP
port=999            # CCD 端口

[SOFTWARE]
logStation=喇叭网组装               # 本地工站名（SFC Log 过滤/校验基准）
addSfcKeyDataName=logo_sn          # 补充 AddSfcKey 的 DATA_NAME

[PARAM]
ccdDataName=CCD                    # CCD AddSfcKey 的 DATA_NAME（默认 "CCD"）
```

---

## 五、UI 标签

| 控件 | 内容 | 说明 |
|------|------|------|
| `SFC_UITextBox` | 条码回显 | 扫码枪收到后自动填入 |
| `uiLabel2` | PASS / FAIL | 72pt 大字，绿底白字 / 红底白字 |
| `PassCount` | 数字 | 当班 PASS 计数 |
| `FailCount` | 数字 | 当班 FAIL 计数 |
| `OutPut` | 数字 | 上一班产出 |
| `uiLabel5` | "扫码枪状态:" | 静态文字 |
| `uiLabel7` | 已连接 / 未连接 | 扫码枪连接状态 |
| `uiLabel3` | "CCD状态:" | 静态文字 |
| `uiLabel4` | 已连接 / 未连接 | CCD 连接状态 |
| `uiRichTextBox1` | 日志区域 | 带时间戳，超100行自动清屏 |

---

## 六、项目结构

```
WindowsFormsApp/
├── MainForm.cs              # 主窗体：启动、过站、设备连接、CCD处理
├── MainForm.Designer.cs     # 窗体控件布局（自动生成）
├── FormHelper.cs            # MES 接口封装（6 个方法）
├── ConfigService.cs         # INI 配置文件读取
├── HttpUitls.cs             # HTTP GET/POST 工具
├── WriteLogs.cs             # 文件日志（按日期分目录）
├── Models.cs                # 数据模型（待扩展）
├── Program.cs               # 入口，单实例检测
├── WindowsFormsApp.csproj   # 项目文件 (.NET 4.7.2)
├── App.config               # 运行时绑定重定向
├── Pass.wav                 # 过站成功音效
└── alert.wav                # 过站失败音效
```

### 依赖

| 包 | 版本 | 用途 |
|----|------|------|
| SunnyUI | 3.9.6 | WinForms 现代化 UI |
| Newtonsoft.Json | 12.0.3 | JSON 序列化 |
| SimpleTCP | 1.0.24 | TCP 客户端（扫码枪/CCD） |
| JSON_RW_DLL | — | MES 响应反序列化 |
| ReadWriteLogIni | — | INI 文件读写 |
