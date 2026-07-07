

namespace WindowsFormsApp
{
    /// <summary>
    /// 小件定义
    /// </summary>
    public class SubMaterialDef
    {
        public string Bydpn { get; set; }      // 小件名称
        public string Did { get; set; }        // 绑定的小件码
        public string DidRule { get; set; }    // 条码匹配规则（分号分隔）
        public double Qty { get; set; }        // 每产品用量
        public string Remarks { get; set; }    // 默认上料数量（初始剩余数量）
        public string Location { get; set; }   // 小件位置号
        public int MinSurplus { get; set; }    // 最小剩余数量
        public string StopQty { get; set; }    // 停机数量
        public string ClientNo { get; set; }   // 料号
    }
    /// <summary>
    /// TestDataCollect2MainChild 接口中 TEST_DATA_LIST 的单项
    /// </summary>
    public class TestDataItem
    {
        public string NAME { get; set; }
        public string VALUE { get; set; }
        public string TEST_RESULT { get; set; }
        public string MAX_VALUE { get; set; }
        public string MIN_VALUE { get; set; }
        public string STANDARD_VALUE { get; set; }
    }

    /// <summary>
    /// GetLoadUpByParams 接口返回结果
    /// </summary>
    public class LoadUpResult
    {
        /// <summary>调用是否成功（网络+解析）</summary>
        public bool Ok { get; set; }
        /// <summary>RESULT=FAIL 时的错误消息，非空表示应停止流程</summary>
        public string FailMessage { get; set; }
        /// <summary>最新剩余数量</summary>
        public double QtyResidual { get; set; }
        /// <summary>LoadUps 数组是否有数据</summary>
        public bool Found { get; set; }
        /// <summary>接口返回的小件码</summary>
        public string Did { get; set; }
    }

    /// <summary>
    /// 用来给下拉框展示”哪些行可以和当前输入的小件码匹配”。
    /// </summary>
    public class MatchCandidate
    {
        public int RowIndex { get; set; }
        public string DisplayText { get; set; }
        /// <summary>该行是否已绑定 Did（已绑定时选中需要走"上新小件"流程）</summary>
        public bool IsBound { get; set; }
    }
}
