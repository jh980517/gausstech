namespace PinConnectionDiagram
{
    partial class DescriptionForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblSection;
        private Label lblGuide;
        private Controls.VerticalScrollPanel pnlProcedure;
        private RichTextBox txtProcedure;
        private Button btnCopy;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblSection = new Label();
            lblGuide = new Label();
            pnlProcedure = new PinConnectionDiagram.Controls.VerticalScrollPanel();
            txtProcedure = new RichTextBox();
            btnCopy = new Button();
            btnClose = new Button();
            pnlProcedure.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.FromArgb(30, 46, 69);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("맑은 고딕", 22F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(145, 223, 251);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1000, 72);
            lblTitle.TabIndex = 5;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSection
            // 
            lblSection.AutoSize = true;
            lblSection.Font = new Font("맑은 고딕", 17F, FontStyle.Bold);
            lblSection.ForeColor = Color.White;
            lblSection.Location = new Point(34, 94);
            lblSection.Name = "lblSection";
            lblSection.Size = new Size(114, 31);
            lblSection.TabIndex = 4;
            lblSection.Text = "시험 절차";
            // 
            // lblGuide
            // 
            lblGuide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblGuide.Font = new Font("맑은 고딕", 9.5F);
            lblGuide.ForeColor = Color.FromArgb(160, 170, 184);
            lblGuide.Location = new Point(559, 104);
            lblGuide.Name = "lblGuide";
            lblGuide.Size = new Size(384, 22);
            lblGuide.TabIndex = 3;
            lblGuide.Text = "현재 연결도와 배치된 시험 준비물을 기준으로 생성되었습니다.";
            lblGuide.TextAlign = ContentAlignment.MiddleRight;
            // 
            // pnlProcedure
            // 
            pnlProcedure.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlProcedure.AutoScroll = true;
            pnlProcedure.BackColor = Color.FromArgb(38, 38, 38);
            pnlProcedure.Controls.Add(txtProcedure);
            pnlProcedure.Location = new Point(34, 139);
            pnlProcedure.Name = "pnlProcedure";
            pnlProcedure.Padding = new Padding(18, 12, 18, 12);
            pnlProcedure.Size = new Size(909, 365);
            pnlProcedure.TabIndex = 2;
            // 
            // txtProcedure
            // 
            txtProcedure.BackColor = Color.FromArgb(38, 38, 38);
            txtProcedure.BorderStyle = BorderStyle.None;
            txtProcedure.Dock = DockStyle.Top;
            txtProcedure.Font = new Font("맑은 고딕", 12.5F);
            txtProcedure.ForeColor = Color.WhiteSmoke;
            txtProcedure.Location = new Point(18, 12);
            txtProcedure.Margin = new Padding(0);
            txtProcedure.Name = "txtProcedure";
            txtProcedure.ReadOnly = true;
            txtProcedure.ScrollBars = RichTextBoxScrollBars.None;
            txtProcedure.Size = new Size(873, 341);
            txtProcedure.TabIndex = 0;
            txtProcedure.Text = "";
            // 
            // btnCopy
            // 
            btnCopy.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCopy.BackgroundImage = Properties.Resources.Button;
            btnCopy.BackgroundImageLayout = ImageLayout.Zoom;
            btnCopy.Cursor = Cursors.Hand;
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.FlatStyle = FlatStyle.Flat;
            btnCopy.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            btnCopy.ForeColor = Color.FromArgb(145, 223, 251);
            btnCopy.Location = new Point(775, 529);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(80, 35);
            btnCopy.TabIndex = 1;
            btnCopy.Text = "복사";
            btnCopy.Click += btnCopy_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.BackgroundImage = Properties.Resources.Button;
            btnClose.BackgroundImageLayout = ImageLayout.Zoom;
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(145, 223, 251);
            btnClose.Location = new Point(863, 529);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 35);
            btnClose.TabIndex = 0;
            btnClose.Text = "닫기";
            btnClose.Click += btnClose_Click;
            // 
            // DescriptionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(56, 60, 98);
            ClientSize = new Size(1000, 585);
            Controls.Add(btnClose);
            Controls.Add(btnCopy);
            Controls.Add(pnlProcedure);
            Controls.Add(lblGuide);
            Controls.Add(lblSection);
            Controls.Add(lblTitle);
            MinimumSize = new Size(760, 480);
            Name = "DescriptionForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "시험 절차 설명문";
            pnlProcedure.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
