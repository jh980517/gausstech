namespace PinConnectionDiagram
{
    partial class ProjectMessageForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private Label lblIcon;
        private Label lblMessage;
        private FlowLayoutPanel flpButtons;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblIcon = new Label();
            lblMessage = new Label();
            flpButtons = new FlowLayoutPanel();
            SuspendLayout();
            lblTitle.BackColor = Color.FromArgb(30, 46, 69);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("맑은 고딕", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(145, 223, 251);
            lblTitle.Height = 52;
            lblTitle.Padding = new Padding(18, 0, 0, 0);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblIcon.Font = new Font("맑은 고딕", 22F, FontStyle.Bold);
            lblIcon.Location = new Point(24, 76);
            lblIcon.Size = new Size(48, 48);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblMessage.Font = new Font("맑은 고딕", 11.5F);
            lblMessage.ForeColor = Color.WhiteSmoke;
            lblMessage.Location = new Point(84, 70);
            lblMessage.Size = new Size(390, 72);
            lblMessage.TextAlign = ContentAlignment.MiddleLeft;
            flpButtons.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            flpButtons.FlowDirection = FlowDirection.RightToLeft;
            flpButtons.Location = new Point(264, 157);
            flpButtons.Size = new Size(210, 42);
            flpButtons.WrapContents = false;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(500, 220);
            Controls.Add(flpButtons);
            Controls.Add(lblMessage);
            Controls.Add(lblIcon);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProjectMessageForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "알림";
            ResumeLayout(false);
        }
    }
}
