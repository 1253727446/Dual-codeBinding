namespace WindowsFormsApp
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.uiTabControl1 = new Sunny.UI.UITabControl();
            this.pageScan = new System.Windows.Forms.TabPage();
            this.ConfigCombox = new Sunny.UI.UIComboBox();
            this.uiLabel7 = new Sunny.UI.UILabel();
            this.uiRichTextBox1 = new Sunny.UI.UIRichTextBox();
            this.uiPanel1 = new Sunny.UI.UIPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.OutPut = new Sunny.UI.UILabel();
            this.uiLabel10 = new Sunny.UI.UILabel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.uiLabel8 = new Sunny.UI.UILabel();
            this.FailCount = new Sunny.UI.UILabel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.PassCount = new Sunny.UI.UILabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.SFC_UITextBox = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.pageLoad = new System.Windows.Forms.TabPage();
            this.btnResetBindings = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.uiLabel14 = new Sunny.UI.UILabel();
            this.cmbMatches = new Sunny.UI.UIComboBox();
            this.dataGridViewParts = new Sunny.UI.UIDataGridView();
            this.txtInputCode = new Sunny.UI.UITextBox();
            this.uiLabel12 = new Sunny.UI.UILabel();
            this.uiTabControl1.SuspendLayout();
            this.pageScan.SuspendLayout();
            this.uiPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.pageLoad.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParts)).BeginInit();
            this.SuspendLayout();
            // 
            // uiTabControl1
            // 
            this.uiTabControl1.Controls.Add(this.pageScan);
            this.uiTabControl1.Controls.Add(this.pageLoad);
            this.uiTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.uiTabControl1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTabControl1.ItemSize = new System.Drawing.Size(100, 20);
            this.uiTabControl1.Location = new System.Drawing.Point(0, 35);
            this.uiTabControl1.MainPage = "";
            this.uiTabControl1.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.uiTabControl1.Name = "uiTabControl1";
            this.uiTabControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.uiTabControl1.SelectedIndex = 0;
            this.uiTabControl1.Size = new System.Drawing.Size(809, 484);
            this.uiTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.uiTabControl1.TabBackColor = System.Drawing.Color.White;
            this.uiTabControl1.TabIndex = 0;
            this.uiTabControl1.TabPageTextAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            this.uiTabControl1.TabSelectedColor = System.Drawing.Color.DodgerBlue;
            this.uiTabControl1.TabSelectedForeColor = System.Drawing.Color.Black;
            this.uiTabControl1.TabUnSelectedColor = System.Drawing.Color.White;
            this.uiTabControl1.TabUnSelectedForeColor = System.Drawing.Color.Black;
            this.uiTabControl1.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // pageScan
            // 
            this.pageScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.pageScan.Controls.Add(this.ConfigCombox);
            this.pageScan.Controls.Add(this.uiLabel7);
            this.pageScan.Controls.Add(this.uiRichTextBox1);
            this.pageScan.Controls.Add(this.uiPanel1);
            this.pageScan.Controls.Add(this.SFC_UITextBox);
            this.pageScan.Controls.Add(this.uiLabel1);
            this.pageScan.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageScan.Location = new System.Drawing.Point(0, 20);
            this.pageScan.Name = "pageScan";
            this.pageScan.Size = new System.Drawing.Size(809, 464);
            this.pageScan.TabIndex = 0;
            this.pageScan.Text = "扫码过站";
            // 
            // ConfigCombox
            // 
            this.ConfigCombox.DataSource = null;
            this.ConfigCombox.FillColor = System.Drawing.Color.White;
            this.ConfigCombox.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ConfigCombox.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.ConfigCombox.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.ConfigCombox.Location = new System.Drawing.Point(653, 21);
            this.ConfigCombox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ConfigCombox.MinimumSize = new System.Drawing.Size(63, 0);
            this.ConfigCombox.Name = "ConfigCombox";
            this.ConfigCombox.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.ConfigCombox.Size = new System.Drawing.Size(149, 38);
            this.ConfigCombox.SymbolSize = 24;
            this.ConfigCombox.TabIndex = 9;
            this.ConfigCombox.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.ConfigCombox.Watermark = "";
            // 
            // uiLabel7
            // 
            this.uiLabel7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel7.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel7.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiLabel7.Location = new System.Drawing.Point(581, 23);
            this.uiLabel7.Name = "uiLabel7";
            this.uiLabel7.Size = new System.Drawing.Size(78, 35);
            this.uiLabel7.TabIndex = 4;
            this.uiLabel7.Text = "Config:";
            this.uiLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiRichTextBox1
            // 
            this.uiRichTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiRichTextBox1.FillColor = System.Drawing.Color.White;
            this.uiRichTextBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiRichTextBox1.Location = new System.Drawing.Point(14, 82);
            this.uiRichTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiRichTextBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiRichTextBox1.Name = "uiRichTextBox1";
            this.uiRichTextBox1.Padding = new System.Windows.Forms.Padding(2);
            this.uiRichTextBox1.ShowText = false;
            this.uiRichTextBox1.Size = new System.Drawing.Size(784, 328);
            this.uiRichTextBox1.TabIndex = 3;
            this.uiRichTextBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiPanel1
            // 
            this.uiPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiPanel1.Controls.Add(this.tableLayoutPanel1);
            this.uiPanel1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel1.Location = new System.Drawing.Point(8, 415);
            this.uiPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel1.Name = "uiPanel1";
            this.uiPanel1.Size = new System.Drawing.Size(801, 44);
            this.uiPanel1.TabIndex = 3;
            this.uiPanel1.Text = null;
            this.uiPanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.71036F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.09987F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.35581F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.35705F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.35206F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(801, 44);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54.19355F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.80645F));
            this.tableLayoutPanel6.Controls.Add(this.OutPut, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.uiLabel10, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(656, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(142, 38);
            this.tableLayoutPanel6.TabIndex = 4;
            // 
            // OutPut
            // 
            this.OutPut.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.OutPut.BackColor = System.Drawing.Color.DodgerBlue;
            this.OutPut.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.OutPut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.OutPut.Location = new System.Drawing.Point(79, 7);
            this.OutPut.Name = "OutPut";
            this.OutPut.Size = new System.Drawing.Size(59, 23);
            this.OutPut.TabIndex = 13;
            this.OutPut.Text = "0";
            this.OutPut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel10
            // 
            this.uiLabel10.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel10.Font = new System.Drawing.Font("宋体", 10F);
            this.uiLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel10.Location = new System.Drawing.Point(3, 7);
            this.uiLabel10.Name = "uiLabel10";
            this.uiLabel10.Size = new System.Drawing.Size(70, 23);
            this.uiLabel10.TabIndex = 12;
            this.uiLabel10.Text = "上班产出:";
            this.uiLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.80519F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.19481F));
            this.tableLayoutPanel5.Controls.Add(this.uiLabel8, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.FailCount, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(541, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(109, 38);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // uiLabel8
            // 
            this.uiLabel8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel8.Font = new System.Drawing.Font("宋体", 10F);
            this.uiLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel8.Location = new System.Drawing.Point(3, 7);
            this.uiLabel8.Name = "uiLabel8";
            this.uiLabel8.Size = new System.Drawing.Size(42, 23);
            this.uiLabel8.TabIndex = 10;
            this.uiLabel8.Text = "FAIL:";
            this.uiLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FailCount
            // 
            this.FailCount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.FailCount.BackColor = System.Drawing.Color.DodgerBlue;
            this.FailCount.Font = new System.Drawing.Font("宋体", 10F);
            this.FailCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.FailCount.Location = new System.Drawing.Point(51, 7);
            this.FailCount.Name = "FailCount";
            this.FailCount.Size = new System.Drawing.Size(54, 23);
            this.FailCount.TabIndex = 11;
            this.FailCount.Text = "0";
            this.FailCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.85714F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.14286F));
            this.tableLayoutPanel4.Controls.Add(this.uiLabel6, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.PassCount, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(418, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(117, 38);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // uiLabel6
            // 
            this.uiLabel6.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel6.Font = new System.Drawing.Font("宋体", 10F);
            this.uiLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel6.Location = new System.Drawing.Point(3, 7);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(44, 23);
            this.uiLabel6.TabIndex = 8;
            this.uiLabel6.Text = "PASS:";
            this.uiLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PassCount
            // 
            this.PassCount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.PassCount.BackColor = System.Drawing.Color.DodgerBlue;
            this.PassCount.Font = new System.Drawing.Font("宋体", 10F);
            this.PassCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.PassCount.Location = new System.Drawing.Point(58, 7);
            this.PassCount.Name = "PassCount";
            this.PassCount.Size = new System.Drawing.Size(50, 23);
            this.PassCount.TabIndex = 9;
            this.PassCount.Text = "0";
            this.PassCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.44156F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.55844F));
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(257, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(155, 38);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(248, 38);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // SFC_UITextBox
            // 
            this.SFC_UITextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.SFC_UITextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SFC_UITextBox.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SFC_UITextBox.Location = new System.Drawing.Point(71, 24);
            this.SFC_UITextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SFC_UITextBox.MinimumSize = new System.Drawing.Size(1, 16);
            this.SFC_UITextBox.Name = "SFC_UITextBox";
            this.SFC_UITextBox.Padding = new System.Windows.Forms.Padding(5);
            this.SFC_UITextBox.ShowText = false;
            this.SFC_UITextBox.Size = new System.Drawing.Size(502, 35);
            this.SFC_UITextBox.TabIndex = 2;
            this.SFC_UITextBox.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.SFC_UITextBox.Watermark = "";
            // 
            // uiLabel1
            // 
            this.uiLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiLabel1.Location = new System.Drawing.Point(16, 24);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(57, 35);
            this.uiLabel1.TabIndex = 1;
            this.uiLabel1.Text = "SFC:";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pageLoad
            // 
            this.pageLoad.Controls.Add(this.btnResetBindings);
            this.pageLoad.Controls.Add(this.lblStatus);
            this.pageLoad.Controls.Add(this.uiLabel14);
            this.pageLoad.Controls.Add(this.cmbMatches);
            this.pageLoad.Controls.Add(this.dataGridViewParts);
            this.pageLoad.Controls.Add(this.txtInputCode);
            this.pageLoad.Controls.Add(this.uiLabel12);
            this.pageLoad.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageLoad.Location = new System.Drawing.Point(0, 20);
            this.pageLoad.Name = "pageLoad";
            this.pageLoad.Size = new System.Drawing.Size(809, 464);
            this.pageLoad.TabIndex = 2;
            this.pageLoad.Text = "小件上料";
            this.pageLoad.UseVisualStyleBackColor = true;
            // 
            // btnResetBindings
            // 
            this.btnResetBindings.Location = new System.Drawing.Point(646, 158);
            this.btnResetBindings.Margin = new System.Windows.Forms.Padding(2);
            this.btnResetBindings.Name = "btnResetBindings";
            this.btnResetBindings.Size = new System.Drawing.Size(107, 23);
            this.btnResetBindings.TabIndex = 12;
            this.btnResetBindings.Text = "重置绑定";
            this.btnResetBindings.UseVisualStyleBackColor = true;
            this.btnResetBindings.Click += new System.EventHandler(this.btnResetBindings_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.Location = new System.Drawing.Point(27, 158);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(609, 23);
            this.lblStatus.TabIndex = 11;
            this.lblStatus.Text = "状态：等待输入小件码。";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel14
            // 
            this.uiLabel14.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel14.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel14.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiLabel14.Location = new System.Drawing.Point(18, 92);
            this.uiLabel14.Name = "uiLabel14";
            this.uiLabel14.Size = new System.Drawing.Size(81, 35);
            this.uiLabel14.TabIndex = 9;
            this.uiLabel14.Text = "匹配结果:";
            this.uiLabel14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbMatches
            // 
            this.cmbMatches.DataSource = null;
            this.cmbMatches.FillColor = System.Drawing.Color.White;
            this.cmbMatches.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbMatches.ItemHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.cmbMatches.ItemSelectForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.cmbMatches.Location = new System.Drawing.Point(106, 89);
            this.cmbMatches.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbMatches.MinimumSize = new System.Drawing.Size(63, 0);
            this.cmbMatches.Name = "cmbMatches";
            this.cmbMatches.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.cmbMatches.Size = new System.Drawing.Size(610, 38);
            this.cmbMatches.SymbolSize = 24;
            this.cmbMatches.TabIndex = 8;
            this.cmbMatches.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmbMatches.Watermark = "";
            this.cmbMatches.SelectedIndexChanged += new System.EventHandler(this.cmbMatches_SelectedIndexChanged);
            // 
            // dataGridViewParts
            // 
            this.dataGridViewParts.AllowUserToAddRows = false;
            this.dataGridViewParts.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.dataGridViewParts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewParts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewParts.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewParts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewParts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewParts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewParts.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewParts.EnableHeadersVisualStyles = false;
            this.dataGridViewParts.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridViewParts.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.dataGridViewParts.Location = new System.Drawing.Point(27, 209);
            this.dataGridViewParts.Name = "dataGridViewParts";
            this.dataGridViewParts.ReadOnly = true;
            this.dataGridViewParts.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewParts.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dataGridViewParts.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridViewParts.RowTemplate.Height = 23;
            this.dataGridViewParts.SelectedIndex = -1;
            this.dataGridViewParts.Size = new System.Drawing.Size(754, 227);
            this.dataGridViewParts.StripeOddColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(243)))), ((int)(((byte)(255)))));
            this.dataGridViewParts.TabIndex = 7;
            // 
            // txtInputCode
            // 
            this.txtInputCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputCode.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtInputCode.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtInputCode.Location = new System.Drawing.Point(106, 29);
            this.txtInputCode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtInputCode.MinimumSize = new System.Drawing.Size(1, 16);
            this.txtInputCode.Name = "txtInputCode";
            this.txtInputCode.Padding = new System.Windows.Forms.Padding(5);
            this.txtInputCode.ShowText = false;
            this.txtInputCode.Size = new System.Drawing.Size(610, 35);
            this.txtInputCode.TabIndex = 4;
            this.txtInputCode.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.txtInputCode.Watermark = "";
            this.txtInputCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInputCode_KeyDown);
            // 
            // uiLabel12
            // 
            this.uiLabel12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel12.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel12.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiLabel12.Location = new System.Drawing.Point(15, 29);
            this.uiLabel12.Name = "uiLabel12";
            this.uiLabel12.Size = new System.Drawing.Size(65, 35);
            this.uiLabel12.TabIndex = 3;
            this.uiLabel12.Text = "小件码:";
            this.uiLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(809, 519);
            this.Controls.Add(this.uiTabControl1);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "磁铁组装自动化";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.uiTabControl1.ResumeLayout(false);
            this.pageScan.ResumeLayout(false);
            this.uiPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.pageLoad.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParts)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Sunny.UI.UITabControl uiTabControl1;
        private System.Windows.Forms.TabPage pageScan;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UITextBox SFC_UITextBox;
        private Sunny.UI.UIRichTextBox uiRichTextBox1;
        private Sunny.UI.UILabel uiLabel6;
        private Sunny.UI.UILabel PassCount;
        private Sunny.UI.UILabel uiLabel8;
        private Sunny.UI.UILabel FailCount;
        private Sunny.UI.UILabel uiLabel10;
        private Sunny.UI.UILabel OutPut;
        private Sunny.UI.UIPanel uiPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TabPage pageLoad;
        private Sunny.UI.UIDataGridView dataGridViewParts;
        private Sunny.UI.UITextBox txtInputCode;
        private Sunny.UI.UILabel uiLabel12;
        private Sunny.UI.UIComboBox cmbMatches;
        private Sunny.UI.UILabel uiLabel14;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnResetBindings;
        private Sunny.UI.UIComboBox ConfigCombox;
        private Sunny.UI.UILabel uiLabel7;
    }
}

