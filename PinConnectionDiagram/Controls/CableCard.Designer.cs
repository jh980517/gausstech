namespace PinConnectionDiagram.Controls
{
    partial class CableCard
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
            TlpCableCard = new TableLayoutPanel();
            TlpCount = new TableLayoutPanel();
            lblCount = new Label();
            TlpName = new TableLayoutPanel();
            lblName = new Label();
            TlpCategory = new TableLayoutPanel();
            lblCategory = new Label();
            lblCategoryHeader = new Label();
            lblNameHeader = new Label();
            lblCountHeader = new Label();
            pnlCardContainer = new Panel();
            btnDelete = new Button();
            TlpCableCard.SuspendLayout();
            TlpCount.SuspendLayout();
            TlpName.SuspendLayout();
            TlpCategory.SuspendLayout();
            pnlCardContainer.SuspendLayout();
            SuspendLayout();
            // 
            // TlpCableCard
            // 
            TlpCableCard.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            TlpCableCard.BackColor = Color.FromArgb(38, 38, 38);
            TlpCableCard.ColumnCount = 2;
            TlpCableCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32.0388336F));
            TlpCableCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67.96116F));
            TlpCableCard.Controls.Add(TlpCount, 1, 2);
            TlpCableCard.Controls.Add(TlpName, 1, 1);
            TlpCableCard.Controls.Add(TlpCategory, 1, 0);
            TlpCableCard.Controls.Add(lblCategoryHeader, 0, 0);
            TlpCableCard.Controls.Add(lblNameHeader, 0, 1);
            TlpCableCard.Controls.Add(lblCountHeader, 0, 2);
            TlpCableCard.Location = new Point(0, 10);
            TlpCableCard.Margin = new Padding(0);
            TlpCableCard.Name = "TlpCableCard";
            TlpCableCard.Padding = new Padding(0, 3, 0, 3);
            TlpCableCard.RowCount = 3;
            TlpCableCard.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            TlpCableCard.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            TlpCableCard.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            TlpCableCard.Size = new Size(170, 90);
            TlpCableCard.TabIndex = 0;
            // 
            // TlpCount
            // 
            TlpCount.ColumnCount = 1;
            TlpCount.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpCount.Controls.Add(lblCount, 0, 0);
            TlpCount.Dock = DockStyle.Fill;
            TlpCount.Location = new Point(54, 59);
            TlpCount.Margin = new Padding(0);
            TlpCount.Name = "TlpCount";
            TlpCount.RowCount = 1;
            TlpCount.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TlpCount.Size = new Size(116, 28);
            TlpCount.TabIndex = 3;
            TlpCount.Paint += tlpCard_Paint;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.BackColor = Color.Transparent;
            lblCount.Dock = DockStyle.Fill;
            lblCount.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblCount.ForeColor = Color.FromArgb(145, 223, 251);
            lblCount.Location = new Point(0, 0);
            lblCount.Margin = new Padding(0);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(116, 28);
            lblCount.TabIndex = 2;
            lblCount.Text = "수량";
            lblCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TlpName
            // 
            TlpName.ColumnCount = 1;
            TlpName.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpName.Controls.Add(lblName, 0, 0);
            TlpName.Dock = DockStyle.Fill;
            TlpName.Location = new Point(54, 31);
            TlpName.Margin = new Padding(0);
            TlpName.Name = "TlpName";
            TlpName.RowCount = 1;
            TlpName.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TlpName.Size = new Size(116, 28);
            TlpName.TabIndex = 3;
            TlpName.Paint += tlpCard_Paint;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.BackColor = Color.Transparent;
            lblName.Dock = DockStyle.Fill;
            lblName.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblName.ForeColor = Color.FromArgb(145, 223, 251);
            lblName.Location = new Point(0, 0);
            lblName.Margin = new Padding(0);
            lblName.Name = "lblName";
            lblName.Size = new Size(116, 28);
            lblName.TabIndex = 1;
            lblName.Text = "명칭";
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TlpCategory
            // 
            TlpCategory.ColumnCount = 1;
            TlpCategory.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpCategory.Controls.Add(lblCategory, 0, 0);
            TlpCategory.Dock = DockStyle.Fill;
            TlpCategory.Location = new Point(54, 3);
            TlpCategory.Margin = new Padding(0);
            TlpCategory.Name = "TlpCategory";
            TlpCategory.RowCount = 1;
            TlpCategory.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TlpCategory.Size = new Size(116, 28);
            TlpCategory.TabIndex = 2;
            TlpCategory.Paint += tlpCard_Paint;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.BackColor = Color.Transparent;
            lblCategory.Dock = DockStyle.Fill;
            lblCategory.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblCategory.ForeColor = Color.FromArgb(145, 223, 251);
            lblCategory.Location = new Point(0, 0);
            lblCategory.Margin = new Padding(0);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(116, 28);
            lblCategory.TabIndex = 0;
            lblCategory.Text = "구분";
            lblCategory.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCategoryHeader
            // 
            lblCategoryHeader.AutoSize = true;
            lblCategoryHeader.BackColor = Color.FromArgb(30, 46, 69);
            lblCategoryHeader.BorderStyle = BorderStyle.FixedSingle;
            lblCategoryHeader.Dock = DockStyle.Fill;
            lblCategoryHeader.ForeColor = Color.White;
            lblCategoryHeader.Location = new Point(3, 6);
            lblCategoryHeader.Margin = new Padding(3);
            lblCategoryHeader.Name = "lblCategoryHeader";
            lblCategoryHeader.Size = new Size(48, 22);
            lblCategoryHeader.TabIndex = 3;
            lblCategoryHeader.Text = "구분";
            lblCategoryHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblNameHeader
            // 
            lblNameHeader.AutoSize = true;
            lblNameHeader.BackColor = Color.FromArgb(30, 46, 69);
            lblNameHeader.BorderStyle = BorderStyle.FixedSingle;
            lblNameHeader.Dock = DockStyle.Fill;
            lblNameHeader.ForeColor = Color.White;
            lblNameHeader.Location = new Point(3, 34);
            lblNameHeader.Margin = new Padding(3);
            lblNameHeader.Name = "lblNameHeader";
            lblNameHeader.Size = new Size(48, 22);
            lblNameHeader.TabIndex = 4;
            lblNameHeader.Text = "명칭";
            lblNameHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCountHeader
            // 
            lblCountHeader.AutoSize = true;
            lblCountHeader.BackColor = Color.FromArgb(30, 46, 69);
            lblCountHeader.BorderStyle = BorderStyle.FixedSingle;
            lblCountHeader.Dock = DockStyle.Fill;
            lblCountHeader.ForeColor = Color.White;
            lblCountHeader.Location = new Point(3, 62);
            lblCountHeader.Margin = new Padding(3);
            lblCountHeader.Name = "lblCountHeader";
            lblCountHeader.Size = new Size(48, 22);
            lblCountHeader.TabIndex = 5;
            lblCountHeader.Text = "수량";
            lblCountHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlCardContainer
            // 
            pnlCardContainer.Controls.Add(btnDelete);
            pnlCardContainer.Controls.Add(TlpCableCard);
            pnlCardContainer.Dock = DockStyle.Fill;
            pnlCardContainer.Location = new Point(0, 0);
            pnlCardContainer.Margin = new Padding(0);
            pnlCardContainer.Name = "pnlCardContainer";
            pnlCardContainer.Size = new Size(190, 100);
            pnlCardContainer.TabIndex = 1;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.BackColor = Color.FromArgb(38, 38, 38);
            btnDelete.BackgroundImage = Properties.Resources.delete;
            btnDelete.BackgroundImageLayout = ImageLayout.Zoom;
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(62, 67, 119);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Location = new Point(163, 0);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(24, 24);
            btnDelete.TabIndex = 1;
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // CableCard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Transparent;
            Controls.Add(pnlCardContainer);
            Margin = new Padding(0, 0, 20, 0);
            Name = "CableCard";
            Size = new Size(190, 100);
            TlpCableCard.ResumeLayout(false);
            TlpCableCard.PerformLayout();
            TlpCount.ResumeLayout(false);
            TlpCount.PerformLayout();
            TlpName.ResumeLayout(false);
            TlpName.PerformLayout();
            TlpCategory.ResumeLayout(false);
            TlpCategory.PerformLayout();
            pnlCardContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TlpCableCard;
        private Label lblCategory;
        private Label lblName;
        private Label lblCount;
        private Label lblCategoryHeader;
        private Label lblNameHeader;
        private Label lblCountHeader;
        private Panel pnlCardContainer;
        private Button btnDelete;
        private TableLayoutPanel TlpCategory;
        private TableLayoutPanel TlpName;
        private TableLayoutPanel TlpCount;
    }
}
