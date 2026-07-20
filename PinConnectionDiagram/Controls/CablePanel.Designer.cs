namespace PinConnectionDiagram.Controls
{
    partial class CablePanel
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            PnlCanvas = new Panel();
            BtnAdd = new Button();
            PnlCanvas.SuspendLayout();
            SuspendLayout();
            // 
            // PnlCanvas
            // 
            PnlCanvas.AutoSize = true;
            PnlCanvas.BackColor = Color.Transparent;
            PnlCanvas.Controls.Add(BtnAdd);
            PnlCanvas.Dock = DockStyle.Fill;
            PnlCanvas.Location = new Point(0, 0);
            PnlCanvas.Margin = new Padding(0);
            PnlCanvas.Name = "PnlCanvas";
            PnlCanvas.Size = new Size(425, 150);
            PnlCanvas.TabIndex = 0;
            PnlCanvas.SizeChanged += PnlCanvas_SizeChanged;
            // 
            // BtnAdd
            // 
            BtnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnAdd.BackgroundImage = Properties.Resources.btnAdd_Up;
            BtnAdd.BackgroundImageLayout = ImageLayout.Zoom;
            BtnAdd.FlatAppearance.BorderSize = 0;
            BtnAdd.FlatStyle = FlatStyle.Flat;
            BtnAdd.Location = new Point(400, 0);
            BtnAdd.Name = "BtnAdd";
            BtnAdd.Size = new Size(25, 25);
            BtnAdd.TabIndex = 0;
            BtnAdd.UseVisualStyleBackColor = true;
            // 
            // CablePanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(PnlCanvas);
            Name = "CablePanel";
            Size = new Size(425, 150);
            PnlCanvas.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel PnlCanvas;
        private Button BtnAdd;
    }
}
