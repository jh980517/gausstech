namespace PinConnectionDiagram
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TlpBg = new TableLayoutPanel();
            TestSuppliesTlp = new TableLayoutPanel();
            TlpHead1 = new TableLayoutPanel();
            LblTitle1 = new Label();
            TlpBtn1 = new TableLayoutPanel();
            btnReset = new Button();
            btnForward = new Button();
            btnBack = new Button();
            TlpCardArea = new TableLayoutPanel();
            FlpSupplies = new FlowLayoutPanel();
            btnSupAdd = new Button();
            TestSettingTlp = new TableLayoutPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            LblTitle2 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            FlpItemBox3 = new FlowLayoutPanel();
            FlpItemBox2 = new FlowLayoutPanel();
            FlpItemBox1 = new FlowLayoutPanel();
            PnlMap = new Panel();
            TestCable = new Label();
            JigCable = new Label();
            AdapterCable = new Label();
            TlpBg.SuspendLayout();
            TestSuppliesTlp.SuspendLayout();
            TlpHead1.SuspendLayout();
            TlpBtn1.SuspendLayout();
            TlpCardArea.SuspendLayout();
            FlpSupplies.SuspendLayout();
            TestSettingTlp.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            // 
            // TlpBg
            // 
            TlpBg.BackColor = Color.Transparent;
            TlpBg.BackgroundImageLayout = ImageLayout.Zoom;
            TlpBg.ColumnCount = 1;
            TlpBg.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TlpBg.Controls.Add(TestSuppliesTlp, 0, 0);
            TlpBg.Controls.Add(TestSettingTlp, 0, 1);
            TlpBg.Dock = DockStyle.Fill;
            TlpBg.Location = new Point(0, 0);
            TlpBg.Margin = new Padding(0, 3, 0, 3);
            TlpBg.Name = "TlpBg";
            TlpBg.Padding = new Padding(0, 10, 0, 10);
            TlpBg.RowCount = 3;
            TlpBg.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));
            TlpBg.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpBg.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            TlpBg.Size = new Size(984, 961);
            TlpBg.TabIndex = 0;
            TlpBg.Paint += TlpBg_Paint;
            // 
            // TestSuppliesTlp
            // 
            TestSuppliesTlp.BackColor = Color.Transparent;
            TestSuppliesTlp.ColumnCount = 1;
            TestSuppliesTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TestSuppliesTlp.Controls.Add(TlpHead1, 0, 0);
            TestSuppliesTlp.Controls.Add(TlpCardArea, 0, 1);
            TestSuppliesTlp.Dock = DockStyle.Fill;
            TestSuppliesTlp.Location = new Point(0, 13);
            TestSuppliesTlp.Margin = new Padding(0, 3, 0, 3);
            TestSuppliesTlp.Name = "TestSuppliesTlp";
            TestSuppliesTlp.Padding = new Padding(3);
            TestSuppliesTlp.RowCount = 2;
            TestSuppliesTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            TestSuppliesTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TestSuppliesTlp.Size = new Size(984, 204);
            TestSuppliesTlp.TabIndex = 0;
            // 
            // TlpHead1
            // 
            TlpHead1.BackColor = Color.FromArgb(38, 38, 38);
            TlpHead1.ColumnCount = 3;
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195F));
            TlpHead1.Controls.Add(LblTitle1, 0, 0);
            TlpHead1.Controls.Add(TlpBtn1, 2, 0);
            TlpHead1.Dock = DockStyle.Fill;
            TlpHead1.Location = new Point(3, 6);
            TlpHead1.Margin = new Padding(0, 3, 0, 3);
            TlpHead1.Name = "TlpHead1";
            TlpHead1.RowCount = 1;
            TlpHead1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TlpHead1.Size = new Size(978, 54);
            TlpHead1.TabIndex = 2;
            TlpHead1.Paint += GlowLine_paint;
            // 
            // LblTitle1
            // 
            LblTitle1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            LblTitle1.AutoSize = true;
            LblTitle1.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            LblTitle1.ForeColor = Color.FromArgb(145, 223, 251);
            LblTitle1.Location = new Point(15, 10);
            LblTitle1.Margin = new Padding(15, 10, 5, 5);
            LblTitle1.Name = "LblTitle1";
            LblTitle1.Size = new Size(142, 39);
            LblTitle1.TabIndex = 0;
            LblTitle1.Text = "시험 준비물";
            // 
            // TlpBtn1
            // 
            TlpBtn1.ColumnCount = 3;
            TlpBtn1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            TlpBtn1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            TlpBtn1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            TlpBtn1.Controls.Add(btnReset, 2, 0);
            TlpBtn1.Controls.Add(btnForward, 1, 0);
            TlpBtn1.Controls.Add(btnBack, 0, 0);
            TlpBtn1.Location = new Point(785, 5);
            TlpBtn1.Margin = new Padding(3, 5, 3, 5);
            TlpBtn1.Name = "TlpBtn1";
            TlpBtn1.RowCount = 1;
            TlpBtn1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpBtn1.Size = new Size(178, 44);
            TlpBtn1.TabIndex = 4;
            // 
            // btnReset
            // 
            btnReset.BackColor = Color.Transparent;
            btnReset.BackgroundImage = Properties.Resources.reset;
            btnReset.BackgroundImageLayout = ImageLayout.Zoom;
            btnReset.Cursor = Cursors.Hand;
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(62, 67, 119);
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.Location = new Point(123, 5);
            btnReset.Margin = new Padding(5, 5, 0, 0);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(35, 35);
            btnReset.TabIndex = 2;
            btnReset.UseVisualStyleBackColor = false;
            // 
            // btnForward
            // 
            btnForward.BackColor = Color.Transparent;
            btnForward.BackgroundImage = Properties.Resources.forward;
            btnForward.BackgroundImageLayout = ImageLayout.Zoom;
            btnForward.Cursor = Cursors.Hand;
            btnForward.FlatAppearance.BorderColor = Color.FromArgb(62, 67, 119);
            btnForward.FlatAppearance.BorderSize = 0;
            btnForward.FlatStyle = FlatStyle.Flat;
            btnForward.Location = new Point(64, 5);
            btnForward.Margin = new Padding(5, 5, 0, 0);
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(35, 35);
            btnForward.TabIndex = 1;
            btnForward.UseVisualStyleBackColor = false;
            // 
            // btnBack
            // 
            btnBack.BackColor = Color.Transparent;
            btnBack.BackgroundImage = Properties.Resources.back;
            btnBack.BackgroundImageLayout = ImageLayout.Zoom;
            btnBack.Cursor = Cursors.Hand;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.ForeColor = Color.FromArgb(62, 67, 119);
            btnBack.Location = new Point(5, 5);
            btnBack.Margin = new Padding(5, 5, 0, 0);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(35, 35);
            btnBack.TabIndex = 0;
            btnBack.UseVisualStyleBackColor = false;
            // 
            // TlpCardArea
            // 
            TlpCardArea.ColumnCount = 1;
            TlpCardArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TlpCardArea.Controls.Add(FlpSupplies, 0, 0);
            TlpCardArea.Dock = DockStyle.Fill;
            TlpCardArea.Location = new Point(6, 66);
            TlpCardArea.Name = "TlpCardArea";
            TlpCardArea.RowCount = 1;
            TlpCardArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpCardArea.Size = new Size(972, 132);
            TlpCardArea.TabIndex = 3;
            TlpCardArea.Paint += GlowLine_paint;
            // 
            // FlpSupplies
            // 
            FlpSupplies.AutoScroll = true;
            FlpSupplies.Controls.Add(btnSupAdd);
            FlpSupplies.Dock = DockStyle.Fill;
            FlpSupplies.Location = new Point(5, 5);
            FlpSupplies.Margin = new Padding(5);
            FlpSupplies.Name = "FlpSupplies";
            FlpSupplies.Padding = new Padding(0, 3, 0, 0);
            FlpSupplies.Size = new Size(962, 122);
            FlpSupplies.TabIndex = 3;
            FlpSupplies.WrapContents = false;
            // 
            // btnSupAdd
            // 
            btnSupAdd.BackgroundImage = Properties.Resources.추가;
            btnSupAdd.BackgroundImageLayout = ImageLayout.Zoom;
            btnSupAdd.Cursor = Cursors.Hand;
            btnSupAdd.FlatAppearance.BorderSize = 0;
            btnSupAdd.FlatStyle = FlatStyle.Flat;
            btnSupAdd.Location = new Point(30, 21);
            btnSupAdd.Margin = new Padding(30, 18, 20, 0);
            btnSupAdd.Name = "btnSupAdd";
            btnSupAdd.Size = new Size(80, 80);
            btnSupAdd.TabIndex = 1;
            btnSupAdd.UseVisualStyleBackColor = true;
            btnSupAdd.MouseUp += btnSupAdd_MouseUp;
            // 
            // TestSettingTlp
            // 
            TestSettingTlp.ColumnCount = 1;
            TestSettingTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TestSettingTlp.Controls.Add(tableLayoutPanel1, 0, 0);
            TestSettingTlp.Controls.Add(tableLayoutPanel2, 0, 1);
            TestSettingTlp.Controls.Add(PnlMap, 0, 2);
            TestSettingTlp.Dock = DockStyle.Fill;
            TestSettingTlp.Location = new Point(0, 230);
            TestSettingTlp.Margin = new Padding(0, 10, 0, 10);
            TestSettingTlp.Name = "TestSettingTlp";
            TestSettingTlp.RowCount = 3;
            TestSettingTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            TestSettingTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            TestSettingTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TestSettingTlp.Size = new Size(984, 675);
            TestSettingTlp.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.FromArgb(38, 38, 38);
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(LblTitle2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 3);
            tableLayoutPanel1.Margin = new Padding(0, 3, 0, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(984, 54);
            tableLayoutPanel1.TabIndex = 2;
            tableLayoutPanel1.Paint += GlowLine_paint;
            // 
            // LblTitle2
            // 
            LblTitle2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            LblTitle2.AutoSize = true;
            LblTitle2.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            LblTitle2.ForeColor = Color.FromArgb(145, 223, 251);
            LblTitle2.Location = new Point(15, 10);
            LblTitle2.Margin = new Padding(15, 10, 5, 5);
            LblTitle2.Name = "LblTitle2";
            LblTitle2.Size = new Size(142, 39);
            LblTitle2.TabIndex = 1;
            LblTitle2.Text = "시험 연결도";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel2.Controls.Add(AdapterCable, 2, 0);
            tableLayoutPanel2.Controls.Add(JigCable, 1, 0);
            tableLayoutPanel2.Controls.Add(FlpItemBox1, 0, 1);
            tableLayoutPanel2.Controls.Add(FlpItemBox2, 1, 1);
            tableLayoutPanel2.Controls.Add(FlpItemBox3, 2, 1);
            tableLayoutPanel2.Controls.Add(TestCable, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(20, 70);
            tableLayoutPanel2.Margin = new Padding(20, 10, 20, 10);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(944, 180);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // FlpItemBox3
            // 
            FlpItemBox3.AutoScroll = true;
            FlpItemBox3.BackColor = Color.FromArgb(45, 75, 125);
            FlpItemBox3.BackgroundImageLayout = ImageLayout.Zoom;
            FlpItemBox3.BorderStyle = BorderStyle.Fixed3D;
            FlpItemBox3.Location = new Point(633, 35);
            FlpItemBox3.Margin = new Padding(5, 0, 5, 5);
            FlpItemBox3.Name = "FlpItemBox3";
            FlpItemBox3.Padding = new Padding(5);
            FlpItemBox3.Size = new Size(306, 140);
            FlpItemBox3.TabIndex = 2;
            // 
            // FlpItemBox2
            // 
            FlpItemBox2.AutoScroll = true;
            FlpItemBox2.BackColor = Color.FromArgb(33, 90, 85);
            FlpItemBox2.BackgroundImageLayout = ImageLayout.Zoom;
            FlpItemBox2.BorderStyle = BorderStyle.Fixed3D;
            FlpItemBox2.Location = new Point(319, 35);
            FlpItemBox2.Margin = new Padding(5, 0, 5, 5);
            FlpItemBox2.Name = "FlpItemBox2";
            FlpItemBox2.Padding = new Padding(5);
            FlpItemBox2.Size = new Size(304, 140);
            FlpItemBox2.TabIndex = 1;
            // 
            // FlpItemBox1
            // 
            FlpItemBox1.AutoScroll = true;
            FlpItemBox1.BackColor = Color.FromArgb(73, 52, 122);
            FlpItemBox1.BackgroundImageLayout = ImageLayout.Zoom;
            FlpItemBox1.BorderStyle = BorderStyle.Fixed3D;
            FlpItemBox1.Location = new Point(5, 35);
            FlpItemBox1.Margin = new Padding(5, 0, 5, 5);
            FlpItemBox1.Name = "FlpItemBox1";
            FlpItemBox1.Padding = new Padding(5);
            FlpItemBox1.Size = new Size(304, 140);
            FlpItemBox1.TabIndex = 0;
            // 
            // PnlMap
            // 
            PnlMap.AllowDrop = true;
            PnlMap.BackColor = Color.White;
            PnlMap.Dock = DockStyle.Fill;
            PnlMap.Location = new Point(25, 265);
            PnlMap.Margin = new Padding(25, 5, 25, 5);
            PnlMap.Name = "PnlMap";
            PnlMap.Size = new Size(934, 405);
            PnlMap.TabIndex = 4;
            PnlMap.DragDrop += PnlMap_DragDrop;
            PnlMap.DragEnter += PnlMap_DragEnter;
            // 
            // TestCable
            // 
            TestCable.AutoSize = true;
            TestCable.BackColor = Color.FromArgb(73, 52, 122);
            TestCable.BorderStyle = BorderStyle.Fixed3D;
            TestCable.Dock = DockStyle.Fill;
            TestCable.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            TestCable.ForeColor = Color.White;
            TestCable.Location = new Point(5, 5);
            TestCable.Margin = new Padding(5, 5, 5, 0);
            TestCable.Name = "TestCable";
            TestCable.Size = new Size(304, 30);
            TestCable.TabIndex = 3;
            TestCable.Text = "시험 대상 케이블";
            TestCable.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // JigCable
            // 
            JigCable.AutoSize = true;
            JigCable.BackColor = Color.FromArgb(33, 90, 85);
            JigCable.BorderStyle = BorderStyle.Fixed3D;
            JigCable.Dock = DockStyle.Fill;
            JigCable.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            JigCable.ForeColor = Color.White;
            JigCable.Location = new Point(319, 5);
            JigCable.Margin = new Padding(5, 5, 5, 0);
            JigCable.Name = "JigCable";
            JigCable.Size = new Size(304, 30);
            JigCable.TabIndex = 4;
            JigCable.Text = "지그 케이블";
            JigCable.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AdapterCable
            // 
            AdapterCable.AutoSize = true;
            AdapterCable.BackColor = Color.FromArgb(45, 75, 125);
            AdapterCable.BorderStyle = BorderStyle.Fixed3D;
            AdapterCable.Dock = DockStyle.Fill;
            AdapterCable.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            AdapterCable.ForeColor = Color.White;
            AdapterCable.Location = new Point(633, 5);
            AdapterCable.Margin = new Padding(5, 5, 5, 0);
            AdapterCable.Name = "AdapterCable";
            AdapterCable.Size = new Size(306, 30);
            AdapterCable.TabIndex = 5;
            AdapterCable.Text = "어댑터 케이블";
            AdapterCable.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 961);
            Controls.Add(TlpBg);
            MaximumSize = new Size(1000, 1000);
            MinimumSize = new Size(1000, 1000);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "핀 연결도 생성기";
            TlpBg.ResumeLayout(false);
            TestSuppliesTlp.ResumeLayout(false);
            TlpHead1.ResumeLayout(false);
            TlpHead1.PerformLayout();
            TlpBtn1.ResumeLayout(false);
            TlpCardArea.ResumeLayout(false);
            FlpSupplies.ResumeLayout(false);
            TestSettingTlp.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TlpBg;
        private TableLayoutPanel TestSuppliesTlp;
        private Label LblTitle1;
        private Button btnBack;
        private Button btnForward;
        private Button btnReset;
        private TableLayoutPanel TlpHead1;
        private FlowLayoutPanel FlpSupplies;
        private TableLayoutPanel TlpCardArea;
        private Button btnSupAdd;
        private TableLayoutPanel TlpBtn1;
        private TableLayoutPanel TestSettingTlp;
        private Label LblTitle2;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private FlowLayoutPanel FlpItemBox1;
        private FlowLayoutPanel FlpItemBox3;
        private FlowLayoutPanel FlpItemBox2;
        private Panel PnlMap;
        private Label TestCable;
        private Label AdapterCable;
        private Label JigCable;
    }
}
