namespace PinConnectionDiagram.Controls
{
    partial class PinConnector
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
            TlpPin = new TableLayoutPanel();
            PnlPin = new Panel();
            CbxPin = new ComboBox();
            PnlPoint = new Panel();
            TlpPin.SuspendLayout();
            PnlPin.SuspendLayout();
            SuspendLayout();
            // 
            // TlpPin
            // 
            TlpPin.ColumnCount = 2;
            TlpPin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TlpPin.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 18F));
            TlpPin.Controls.Add(PnlPin, 0, 0);
            TlpPin.Controls.Add(PnlPoint, 1, 0);
            TlpPin.Dock = DockStyle.Fill;
            TlpPin.Location = new Point(0, 0);
            TlpPin.Name = "TlpPin";
            TlpPin.RowCount = 1;
            TlpPin.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpPin.Size = new Size(70, 45);
            TlpPin.TabIndex = 0;
            // 
            // PnlPin
            // 
            PnlPin.BackgroundImage = Properties.Resources.connectorIcon_left;
            PnlPin.BackgroundImageLayout = ImageLayout.Stretch;
            PnlPin.Controls.Add(CbxPin);
            PnlPin.Dock = DockStyle.Fill;
            PnlPin.Location = new Point(0, 0);
            PnlPin.Margin = new Padding(0);
            PnlPin.Name = "PnlPin";
            PnlPin.Size = new Size(52, 45);
            PnlPin.TabIndex = 0;
            // 
            // CbxPin
            // 
            CbxPin.FormattingEnabled = true;
            CbxPin.Items.AddRange(new object[] { "P1", "P2", "P3", "P4", "P5", "P5", "P6", "P7", "P8", "P9", "P10" });
            CbxPin.Location = new Point(17, 11);
            CbxPin.Name = "CbxPin";
            CbxPin.Size = new Size(34, 23);
            CbxPin.TabIndex = 0;
            // 
            // PnlPoint
            // 
            PnlPoint.BackColor = Color.Red;
            PnlPoint.Location = new Point(55, 17);
            PnlPoint.Margin = new Padding(3, 17, 3, 3);
            PnlPoint.Name = "PnlPoint";
            PnlPoint.Size = new Size(12, 12);
            PnlPoint.TabIndex = 1;
            PnlPoint.Click += PnlPoint_Click;
            // 
            // PinConnector
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(TlpPin);
            Name = "PinConnector";
            Size = new Size(70, 45);
            TlpPin.ResumeLayout(false);
            PnlPin.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TlpPin;
        private Panel PnlPin;
        private ComboBox CbxPin;
        private Panel PnlPoint;
    }
}
