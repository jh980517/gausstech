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
            LblTitle = new Label();
            TlpBtn1 = new TableLayoutPanel();
            btnReset = new Button();
            btnForward = new Button();
            btnBack = new Button();
            TlpCardArea = new TableLayoutPanel();
            FlpSupplies = new FlowLayoutPanel();
            btnSupAdd = new Button();
            TlpBg.SuspendLayout();
            TestSuppliesTlp.SuspendLayout();
            TlpHead1.SuspendLayout();
            TlpBtn1.SuspendLayout();
            TlpCardArea.SuspendLayout();
            FlpSupplies.SuspendLayout();
            SuspendLayout();
            // 
            // TlpBg
            // 
            TlpBg.BackColor = Color.White;
            TlpBg.ColumnCount = 1;
            TlpBg.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TlpBg.Controls.Add(TestSuppliesTlp, 0, 0);
            TlpBg.Dock = DockStyle.Fill;
            TlpBg.Location = new Point(0, 0);
            TlpBg.Name = "TlpBg";
            TlpBg.Padding = new Padding(10);
            TlpBg.RowCount = 3;
            TlpBg.RowStyles.Add(new RowStyle(SizeType.Absolute, 240F));
            TlpBg.RowStyles.Add(new RowStyle(SizeType.Absolute, 480F));
            TlpBg.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            TlpBg.Size = new Size(984, 861);
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
            TestSuppliesTlp.Location = new Point(13, 13);
            TestSuppliesTlp.Name = "TestSuppliesTlp";
            TestSuppliesTlp.Padding = new Padding(3);
            TestSuppliesTlp.RowCount = 2;
            TestSuppliesTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 65F));
            TestSuppliesTlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 160F));
            TestSuppliesTlp.Size = new Size(958, 234);
            TestSuppliesTlp.TabIndex = 0;
            // 
            // TlpHead1
            // 
            TlpHead1.BackColor = Color.Transparent;
            TlpHead1.ColumnCount = 3;
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 195F));
            TlpHead1.Controls.Add(LblTitle, 0, 0);
            TlpHead1.Controls.Add(TlpBtn1, 2, 0);
            TlpHead1.Dock = DockStyle.Fill;
            TlpHead1.Location = new Point(6, 6);
            TlpHead1.Name = "TlpHead1";
            TlpHead1.RowCount = 1;
            TlpHead1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TlpHead1.Size = new Size(946, 59);
            TlpHead1.TabIndex = 2;
            TlpHead1.Paint += GlowLine_paint;
            // 
            // LblTitle
            // 
            LblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            LblTitle.AutoSize = true;
            LblTitle.Font = new Font("맑은 고딕", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            LblTitle.ForeColor = Color.FromArgb(145, 223, 251);
            LblTitle.Location = new Point(10, 10);
            LblTitle.Margin = new Padding(10);
            LblTitle.Name = "LblTitle";
            LblTitle.Size = new Size(161, 39);
            LblTitle.TabIndex = 0;
            LblTitle.Text = "시험 준비물";
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
            TlpBtn1.Location = new Point(753, 3);
            TlpBtn1.Name = "TlpBtn1";
            TlpBtn1.RowCount = 1;
            TlpBtn1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpBtn1.Size = new Size(178, 53);
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
            btnReset.Location = new Point(128, 10);
            btnReset.Margin = new Padding(10, 10, 0, 0);
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
            btnForward.Location = new Point(69, 10);
            btnForward.Margin = new Padding(10, 10, 0, 0);
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
            btnBack.Location = new Point(10, 10);
            btnBack.Margin = new Padding(10, 10, 0, 0);
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
            TlpCardArea.Location = new Point(6, 71);
            TlpCardArea.Name = "TlpCardArea";
            TlpCardArea.Padding = new Padding(5);
            TlpCardArea.RowCount = 1;
            TlpCardArea.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            TlpCardArea.Size = new Size(946, 157);
            TlpCardArea.TabIndex = 3;
            TlpCardArea.Paint += GlowLine_paint;
            // 
            // FlpSupplies
            // 
            FlpSupplies.AutoScroll = true;
            FlpSupplies.Controls.Add(btnSupAdd);
            FlpSupplies.Dock = DockStyle.Fill;
            FlpSupplies.Location = new Point(8, 8);
            FlpSupplies.Name = "FlpSupplies";
            FlpSupplies.Padding = new Padding(0, 20, 0, 0);
            FlpSupplies.Size = new Size(930, 141);
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
            btnSupAdd.Location = new Point(20, 30);
            btnSupAdd.Margin = new Padding(20, 10, 20, 0);
            btnSupAdd.Name = "btnSupAdd";
            btnSupAdd.Size = new Size(80, 80);
            btnSupAdd.TabIndex = 1;
            btnSupAdd.UseVisualStyleBackColor = true;
            btnSupAdd.MouseUp += btnSupAdd_MouseUp;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 861);
            Controls.Add(TlpBg);
            MaximumSize = new Size(1000, 900);
            MinimumSize = new Size(1000, 900);
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
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TlpBg;
        private TableLayoutPanel TestSuppliesTlp;
        private Label LblTitle;
        private Button btnBack;
        private Button btnForward;
        private Button btnReset;
        private TableLayoutPanel TlpHead1;
        private FlowLayoutPanel FlpSupplies;
        private TableLayoutPanel TlpCardArea;
        private Button btnSupAdd;
        private TableLayoutPanel TlpBtn1;
    }
}
