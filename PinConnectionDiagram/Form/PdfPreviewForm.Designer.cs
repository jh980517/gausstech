namespace PinConnectionDiagram
{
    partial class PdfPreviewForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Panel pnlPreview;
        private PictureBox picturePreview;
        private Label lblPage;
        private Button btnPrevious;
        private Button btnNext;
        private Button btnSave;
        private Button btnClose;

        private void InitializeComponent()
        {
            lblTitle = new Label();
            pnlPreview = new Panel();
            picturePreview = new PictureBox();
            lblPage = new Label();
            btnPrevious = CreatePreviewButton("이전", new Point(26, 838));
            btnNext = CreatePreviewButton("다음", new Point(120, 838));
            btnSave = CreatePreviewButton("출력 저장", new Point(892, 838), 105);
            btnClose = CreatePreviewButton("닫기", new Point(1005, 838));
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            pnlPreview.SuspendLayout();
            SuspendLayout();

            lblTitle.BackColor = Color.FromArgb(30, 46, 69);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("맑은 고딕", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(145, 223, 251);
            lblTitle.Height = 60;
            lblTitle.Padding = new Padding(20, 0, 0, 0);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            pnlPreview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlPreview.AutoScroll = true;
            pnlPreview.BackColor = Color.FromArgb(56, 60, 98);
            pnlPreview.Controls.Add(picturePreview);
            pnlPreview.Location = new Point(26, 78);
            pnlPreview.Size = new Size(1068, 742);

            picturePreview.BackColor = Color.White;
            picturePreview.Dock = DockStyle.Fill;
            picturePreview.SizeMode = PictureBoxSizeMode.Zoom;

            lblPage.Anchor = AnchorStyles.Bottom;
            lblPage.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            lblPage.ForeColor = Color.WhiteSmoke;
            lblPage.Location = new Point(500, 842);
            lblPage.Size = new Size(120, 30);
            lblPage.TextAlign = ContentAlignment.MiddleCenter;

            btnPrevious.Click += btnPrevious_Click;
            btnNext.Click += btnNext_Click;
            btnSave.Click += btnSave_Click;
            btnClose.Click += btnClose_Click;

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(1120, 890);
            Controls.Add(btnClose);
            Controls.Add(btnSave);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(lblPage);
            Controls.Add(pnlPreview);
            Controls.Add(lblTitle);
            MinimumSize = new Size(900, 720);
            Name = "PdfPreviewForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "출력 미리보기";
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            pnlPreview.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Button CreatePreviewButton(string text, Point location, int width = 88)
        {
            Button button = new Button
            {
                Anchor = AnchorStyles.Bottom | (location.X > 500 ? AnchorStyles.Right : AnchorStyles.Left),
                BackgroundImage = Properties.Resources.Button,
                BackgroundImageLayout = ImageLayout.Stretch,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(145, 223, 251),
                Location = location,
                Size = new Size(width, 38),
                Text = text,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }
    }
}
