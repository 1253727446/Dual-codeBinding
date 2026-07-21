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
            this.uiTabControl1 = new Sunny.UI.UITabControl();
            this.pageScan = new System.Windows.Forms.TabPage();
            this.uiPanel2 = new Sunny.UI.UIPanel();
            this.uiLabel15 = new Sunny.UI.UILabel();
            this.stationName = new Sunny.UI.UILabel();
            this.uiLabel13 = new Sunny.UI.UILabel();
            this.shopOrder = new Sunny.UI.UILabel();
            this.uiLabel11 = new Sunny.UI.UILabel();
            this.line = new Sunny.UI.UILabel();
            this.uiLabel9 = new Sunny.UI.UILabel();
            this.projectName = new Sunny.UI.UILabel();
            this.uiButton1 = new Sunny.UI.UIButton();
            this.uiPanel1 = new Sunny.UI.UIPanel();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.uiLabel5 = new Sunny.UI.UILabel();
            this.OutPut = new Sunny.UI.UILabel();
            this.uiLabel10 = new Sunny.UI.UILabel();
            this.FailCount = new Sunny.UI.UILabel();
            this.uiLabel8 = new Sunny.UI.UILabel();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.uiTextBox1 = new Sunny.UI.UITextBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiRichTextBox1 = new Sunny.UI.UIRichTextBox();
            this.SFC_UITextBox = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiTabControl1.SuspendLayout();
            this.pageScan.SuspendLayout();
            this.uiPanel2.SuspendLayout();
            this.uiPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiTabControl1
            // 
            this.uiTabControl1.Controls.Add(this.pageScan);
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
            this.pageScan.Controls.Add(this.uiPanel2);
            this.pageScan.Controls.Add(this.uiButton1);
            this.pageScan.Controls.Add(this.uiPanel1);
            this.pageScan.Controls.Add(this.uiLabel3);
            this.pageScan.Controls.Add(this.uiTextBox1);
            this.pageScan.Controls.Add(this.uiLabel2);
            this.pageScan.Controls.Add(this.uiRichTextBox1);
            this.pageScan.Controls.Add(this.SFC_UITextBox);
            this.pageScan.Controls.Add(this.uiLabel1);
            this.pageScan.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.pageScan.Location = new System.Drawing.Point(0, 20);
            this.pageScan.Name = "pageScan";
            this.pageScan.Size = new System.Drawing.Size(809, 464);
            this.pageScan.TabIndex = 0;
            this.pageScan.Text = "追溯模块";
            // 
            // uiPanel2
            // 
            this.uiPanel2.Controls.Add(this.uiLabel15);
            this.uiPanel2.Controls.Add(this.stationName);
            this.uiPanel2.Controls.Add(this.uiLabel13);
            this.uiPanel2.Controls.Add(this.shopOrder);
            this.uiPanel2.Controls.Add(this.uiLabel11);
            this.uiPanel2.Controls.Add(this.line);
            this.uiPanel2.Controls.Add(this.uiLabel9);
            this.uiPanel2.Controls.Add(this.projectName);
            this.uiPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiPanel2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel2.Location = new System.Drawing.Point(0, 0);
            this.uiPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPanel2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel2.Name = "uiPanel2";
            this.uiPanel2.Size = new System.Drawing.Size(809, 32);
            this.uiPanel2.TabIndex = 14;
            this.uiPanel2.Text = null;
            this.uiPanel2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel15
            // 
            this.uiLabel15.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel15.Location = new System.Drawing.Point(689, 7);
            this.uiLabel15.Name = "uiLabel15";
            this.uiLabel15.Size = new System.Drawing.Size(109, 19);
            this.uiLabel15.TabIndex = 20;
            this.uiLabel15.Text = "NULL";
            this.uiLabel15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stationName
            // 
            this.stationName.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.stationName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.stationName.Location = new System.Drawing.Point(620, 7);
            this.stationName.Name = "stationName";
            this.stationName.Size = new System.Drawing.Size(64, 19);
            this.stationName.TabIndex = 19;
            this.stationName.Text = "工站:";
            this.stationName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel13
            // 
            this.uiLabel13.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel13.Location = new System.Drawing.Point(427, 7);
            this.uiLabel13.Name = "uiLabel13";
            this.uiLabel13.Size = new System.Drawing.Size(187, 19);
            this.uiLabel13.TabIndex = 18;
            this.uiLabel13.Text = "NULL";
            this.uiLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // shopOrder
            // 
            this.shopOrder.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.shopOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.shopOrder.Location = new System.Drawing.Point(372, 7);
            this.shopOrder.Name = "shopOrder";
            this.shopOrder.Size = new System.Drawing.Size(64, 19);
            this.shopOrder.TabIndex = 17;
            this.shopOrder.Text = "工单:";
            this.shopOrder.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel11
            // 
            this.uiLabel11.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel11.Location = new System.Drawing.Point(245, 7);
            this.uiLabel11.Name = "uiLabel11";
            this.uiLabel11.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.uiLabel11.Size = new System.Drawing.Size(109, 19);
            this.uiLabel11.TabIndex = 16;
            this.uiLabel11.Text = "NULL";
            this.uiLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // line
            // 
            this.line.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.line.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.line.Location = new System.Drawing.Point(202, 7);
            this.line.Name = "line";
            this.line.Size = new System.Drawing.Size(51, 19);
            this.line.TabIndex = 15;
            this.line.Text = "线体:";
            this.line.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel9
            // 
            this.uiLabel9.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel9.Location = new System.Drawing.Point(61, 7);
            this.uiLabel9.Name = "uiLabel9";
            this.uiLabel9.Size = new System.Drawing.Size(109, 19);
            this.uiLabel9.TabIndex = 14;
            this.uiLabel9.Text = "NULL";
            this.uiLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // projectName
            // 
            this.projectName.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.projectName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.projectName.Location = new System.Drawing.Point(4, 7);
            this.projectName.Name = "projectName";
            this.projectName.Size = new System.Drawing.Size(63, 19);
            this.projectName.TabIndex = 13;
            this.projectName.Text = "项目:";
            this.projectName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiButton1
            // 
            this.uiButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButton1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Location = new System.Drawing.Point(572, 323);
            this.uiButton1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButton1.Name = "uiButton1";
            this.uiButton1.Size = new System.Drawing.Size(65, 27);
            this.uiButton1.TabIndex = 12;
            this.uiButton1.Text = "重置";
            this.uiButton1.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // uiPanel1
            // 
            this.uiPanel1.Controls.Add(this.uiLabel4);
            this.uiPanel1.Controls.Add(this.uiLabel5);
            this.uiPanel1.Controls.Add(this.OutPut);
            this.uiPanel1.Controls.Add(this.uiLabel10);
            this.uiPanel1.Controls.Add(this.FailCount);
            this.uiPanel1.Controls.Add(this.uiLabel8);
            this.uiPanel1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPanel1.Location = new System.Drawing.Point(362, 415);
            this.uiPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPanel1.Name = "uiPanel1";
            this.uiPanel1.Size = new System.Drawing.Size(443, 44);
            this.uiPanel1.TabIndex = 3;
            this.uiPanel1.Text = null;
            this.uiPanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel4
            // 
            this.uiLabel4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel4.BackColor = System.Drawing.Color.DodgerBlue;
            this.uiLabel4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel4.Location = new System.Drawing.Point(216, 10);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(59, 23);
            this.uiLabel4.TabIndex = 15;
            this.uiLabel4.Text = "0";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel5
            // 
            this.uiLabel5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel5.Font = new System.Drawing.Font("宋体", 10F);
            this.uiLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel5.Location = new System.Drawing.Point(140, 11);
            this.uiLabel5.Name = "uiLabel5";
            this.uiLabel5.Size = new System.Drawing.Size(70, 23);
            this.uiLabel5.TabIndex = 14;
            this.uiLabel5.Text = "当班产出:";
            this.uiLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OutPut
            // 
            this.OutPut.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.OutPut.BackColor = System.Drawing.Color.DodgerBlue;
            this.OutPut.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.OutPut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.OutPut.Location = new System.Drawing.Point(369, 11);
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
            this.uiLabel10.Location = new System.Drawing.Point(293, 11);
            this.uiLabel10.Name = "uiLabel10";
            this.uiLabel10.Size = new System.Drawing.Size(70, 23);
            this.uiLabel10.TabIndex = 12;
            this.uiLabel10.Text = "上班产出:";
            this.uiLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FailCount
            // 
            this.FailCount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.FailCount.BackColor = System.Drawing.Color.DodgerBlue;
            this.FailCount.Font = new System.Drawing.Font("宋体", 10F);
            this.FailCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.FailCount.Location = new System.Drawing.Point(65, 10);
            this.FailCount.Name = "FailCount";
            this.FailCount.Size = new System.Drawing.Size(54, 23);
            this.FailCount.TabIndex = 11;
            this.FailCount.Text = "0";
            this.FailCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel8
            // 
            this.uiLabel8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel8.Font = new System.Drawing.Font("宋体", 10F);
            this.uiLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel8.Location = new System.Drawing.Point(10, 10);
            this.uiLabel8.Name = "uiLabel8";
            this.uiLabel8.Size = new System.Drawing.Size(42, 23);
            this.uiLabel8.TabIndex = 10;
            this.uiLabel8.Text = "FAIL:";
            this.uiLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel3
            // 
            this.uiLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.uiLabel3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiLabel3.Location = new System.Drawing.Point(399, 262);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(48, 35);
            this.uiLabel3.TabIndex = 11;
            this.uiLabel3.Text = "纸码";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // uiTextBox1
            // 
            this.uiTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox1.Location = new System.Drawing.Point(452, 262);
            this.uiTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox1.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox1.Name = "uiTextBox1";
            this.uiTextBox1.Padding = new System.Windows.Forms.Padding(5);
            this.uiTextBox1.ShowText = false;
            this.uiTextBox1.Size = new System.Drawing.Size(338, 35);
            this.uiTextBox1.TabIndex = 3;
            this.uiTextBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiTextBox1.Watermark = "";
            // 
            // uiLabel2
            // 
            this.uiLabel2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uiLabel2.BackColor = System.Drawing.Color.DodgerBlue;
            this.uiLabel2.Font = new System.Drawing.Font("宋体", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.ForeColor = System.Drawing.Color.White;
            this.uiLabel2.Location = new System.Drawing.Point(0, 33);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(809, 92);
            this.uiLabel2.TabIndex = 10;
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiRichTextBox1
            // 
            this.uiRichTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.uiRichTextBox1.FillColor = System.Drawing.Color.White;
            this.uiRichTextBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiRichTextBox1.Location = new System.Drawing.Point(0, 130);
            this.uiRichTextBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiRichTextBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiRichTextBox1.Name = "uiRichTextBox1";
            this.uiRichTextBox1.Padding = new System.Windows.Forms.Padding(2);
            this.uiRichTextBox1.ShowText = false;
            this.uiRichTextBox1.Size = new System.Drawing.Size(354, 329);
            this.uiRichTextBox1.TabIndex = 3;
            this.uiRichTextBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SFC_UITextBox
            // 
            this.SFC_UITextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.SFC_UITextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.SFC_UITextBox.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.SFC_UITextBox.Location = new System.Drawing.Point(454, 194);
            this.SFC_UITextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SFC_UITextBox.MinimumSize = new System.Drawing.Size(1, 16);
            this.SFC_UITextBox.Name = "SFC_UITextBox";
            this.SFC_UITextBox.Padding = new System.Windows.Forms.Padding(5);
            this.SFC_UITextBox.ShowText = false;
            this.SFC_UITextBox.Size = new System.Drawing.Size(338, 35);
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
            this.uiLabel1.Location = new System.Drawing.Point(352, 194);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(95, 35);
            this.uiLabel1.TabIndex = 1;
            this.uiLabel1.Text = "镭雕二维码";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(809, 519);
            this.Controls.Add(this.uiTabControl1);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "双码绑定";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.uiTabControl1.ResumeLayout(false);
            this.pageScan.ResumeLayout(false);
            this.uiPanel2.ResumeLayout(false);
            this.uiPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UILabel uiLabel5;

        private Sunny.UI.UILabel uiLabel15;
        private Sunny.UI.UILabel stationName;

        private Sunny.UI.UILabel uiLabel9;
        private Sunny.UI.UILabel uiLabel11;
        private Sunny.UI.UILabel line;
        private Sunny.UI.UILabel uiLabel13;
        private Sunny.UI.UILabel shopOrder;

        private Sunny.UI.UIPanel uiPanel2;

        private Sunny.UI.UILabel projectName;

        private Sunny.UI.UIButton uiButton1;

        private Sunny.UI.UITextBox uiTextBox1;
        private Sunny.UI.UILabel uiLabel3;

        private Sunny.UI.UILabel uiLabel2;

        #endregion

        private Sunny.UI.UITabControl uiTabControl1;
        private System.Windows.Forms.TabPage pageScan;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UITextBox SFC_UITextBox;
        private Sunny.UI.UIRichTextBox uiRichTextBox1;
        private Sunny.UI.UILabel uiLabel8;
        private Sunny.UI.UILabel FailCount;
        private Sunny.UI.UILabel uiLabel10;
        private Sunny.UI.UILabel OutPut;
        private Sunny.UI.UIPanel uiPanel1;
    }
}

