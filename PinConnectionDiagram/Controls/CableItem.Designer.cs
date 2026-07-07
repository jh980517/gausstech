namespace PinConnectionDiagram.Controls
{
    partial class CableItem
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
            TlpCableItem = new TableLayoutPanel();
            PbConnector = new PictureBox();
            LblCableName = new Label();
            TlpCableItem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PbConnector).BeginInit();
            SuspendLayout();
            // 
            // TlpCableItem
            // 
            TlpCableItem.BackColor = Color.White;
            TlpCableItem.ColumnCount = 2;
            TlpCableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            TlpCableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TlpCableItem.Controls.Add(PbConnector, 0, 0);
            TlpCableItem.Controls.Add(LblCableName, 1, 0);
            TlpCableItem.Dock = DockStyle.Fill;
            TlpCableItem.Font = new Font("맑은 고딕", 9.5F);
            TlpCableItem.Location = new Point(0, 0);
            TlpCableItem.Margin = new Padding(0);
            TlpCableItem.Name = "TlpCableItem";
            TlpCableItem.RowCount = 1;
            TlpCableItem.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpCableItem.Size = new Size(90, 34);
            TlpCableItem.TabIndex = 0;
            TlpCableItem.Paint += TlpCableItem_Paint;
            // 
            // PbConnector
            // 
            PbConnector.BackgroundImage = Properties.Resources.connector;
            PbConnector.BackgroundImageLayout = ImageLayout.Zoom;
            PbConnector.Dock = DockStyle.Fill;
            PbConnector.Location = new Point(0, 1);
            PbConnector.Margin = new Padding(0, 1, 0, 1);
            PbConnector.Name = "PbConnector";
            PbConnector.Size = new Size(20, 32);
            PbConnector.TabIndex = 0;
            PbConnector.TabStop = false;
            // 
            // LblCableName
            // 
            LblCableName.AutoSize = true;
            LblCableName.Dock = DockStyle.Fill;
            LblCableName.Font = new Font("맑은 고딕", 7.5F, FontStyle.Bold);
            LblCableName.Location = new Point(23, 2);
            LblCableName.Margin = new Padding(3, 2, 3, 2);
            LblCableName.Name = "LblCableName";
            LblCableName.Size = new Size(64, 30);
            LblCableName.TabIndex = 1;
            LblCableName.Text = "케이블 명칭";
            LblCableName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CableItem
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(TlpCableItem);
            Cursor = Cursors.Hand;
            Font = new Font("맑은 고딕", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 129);
            Margin = new Padding(5, 0, 5, 11);
            Name = "CableItem";
            Size = new Size(90, 34);
            TlpCableItem.ResumeLayout(false);
            TlpCableItem.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PbConnector).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TlpCableItem;
        private PictureBox PbConnector;
        private Label LblCableName;
    }
}
