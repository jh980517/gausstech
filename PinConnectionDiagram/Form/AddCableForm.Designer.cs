namespace PinConnectionDiagram
{
    partial class AddCableForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TlpInput = new TableLayoutPanel();
            lblCategory = new Label();
            lblName = new Label();
            lblCount = new Label();
            numCount = new NumericUpDown();
            txtName = new TextBox();
            cbCategory = new ComboBox();
            TlpButtons = new TableLayoutPanel();
            btnOk = new Button();
            btnCancel = new Button();
            TlpInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).BeginInit();
            TlpButtons.SuspendLayout();
            SuspendLayout();
            // 
            // TlpInput
            // 
            TlpInput.BackColor = Color.Transparent;
            TlpInput.ColumnCount = 2;
            TlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.38853F));
            TlpInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72.6114655F));
            TlpInput.Controls.Add(lblCategory, 0, 0);
            TlpInput.Controls.Add(lblName, 0, 1);
            TlpInput.Controls.Add(lblCount, 0, 2);
            TlpInput.Controls.Add(numCount, 1, 2);
            TlpInput.Controls.Add(txtName, 1, 1);
            TlpInput.Controls.Add(cbCategory, 1, 0);
            TlpInput.Controls.Add(TlpButtons, 1, 3);
            TlpInput.Dock = DockStyle.Fill;
            TlpInput.Location = new Point(0, 0);
            TlpInput.Name = "TlpInput";
            TlpInput.Padding = new Padding(10);
            TlpInput.RowCount = 4;
            TlpInput.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333F));
            TlpInput.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333359F));
            TlpInput.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33334F));
            TlpInput.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            TlpInput.Size = new Size(384, 261);
            TlpInput.TabIndex = 0;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.BackColor = Color.FromArgb(30, 46, 69);
            lblCategory.BorderStyle = BorderStyle.FixedSingle;
            lblCategory.Dock = DockStyle.Fill;
            lblCategory.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblCategory.ForeColor = Color.White;
            lblCategory.Location = new Point(18, 25);
            lblCategory.Margin = new Padding(8, 15, 8, 15);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(83, 36);
            lblCategory.TabIndex = 9;
            lblCategory.Text = "구분";
            lblCategory.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.BackColor = Color.FromArgb(30, 46, 69);
            lblName.BorderStyle = BorderStyle.FixedSingle;
            lblName.Dock = DockStyle.Fill;
            lblName.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblName.ForeColor = Color.White;
            lblName.Location = new Point(18, 91);
            lblName.Margin = new Padding(8, 15, 8, 15);
            lblName.Name = "lblName";
            lblName.Size = new Size(83, 37);
            lblName.TabIndex = 10;
            lblName.Text = "명칭";
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.BackColor = Color.FromArgb(30, 46, 69);
            lblCount.BorderStyle = BorderStyle.FixedSingle;
            lblCount.Dock = DockStyle.Fill;
            lblCount.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblCount.ForeColor = Color.White;
            lblCount.Location = new Point(18, 158);
            lblCount.Margin = new Padding(8, 15, 8, 15);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(83, 37);
            lblCount.TabIndex = 11;
            lblCount.Text = "수량";
            lblCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // numCount
            // 
            numCount.BackColor = Color.FromArgb(38, 38, 38);
            numCount.BorderStyle = BorderStyle.FixedSingle;
            numCount.Cursor = Cursors.Hand;
            numCount.Dock = DockStyle.Fill;
            numCount.Font = new Font("맑은 고딕", 15F);
            numCount.ForeColor = Color.FromArgb(145, 223, 251);
            numCount.Location = new Point(129, 158);
            numCount.Margin = new Padding(20, 15, 130, 10);
            numCount.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            numCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.ReadOnly = true;
            numCount.Size = new Size(115, 34);
            numCount.TabIndex = 8;
            numCount.TextAlign = HorizontalAlignment.Center;
            numCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // txtName
            // 
            txtName.BackColor = Color.FromArgb(38, 38, 38);
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtName.Dock = DockStyle.Fill;
            txtName.Font = new Font("맑은 고딕", 15F, FontStyle.Regular, GraphicsUnit.Point, 129);
            txtName.ForeColor = Color.FromArgb(145, 223, 251);
            txtName.Location = new Point(129, 91);
            txtName.Margin = new Padding(20, 15, 20, 10);
            txtName.MaxLength = 18;
            txtName.Name = "txtName";
            txtName.Size = new Size(225, 34);
            txtName.TabIndex = 7;
            txtName.TextAlign = HorizontalAlignment.Center;
            // 
            // cbCategory
            // 
            cbCategory.BackColor = Color.FromArgb(38, 38, 38);
            cbCategory.Cursor = Cursors.Hand;
            cbCategory.Dock = DockStyle.Fill;
            cbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCategory.Font = new Font("맑은 고딕", 13F);
            cbCategory.ForeColor = Color.FromArgb(145, 223, 251);
            cbCategory.FormattingEnabled = true;
            cbCategory.Items.AddRange(new object[] { "시험 대상 케이블", "지그 케이블", "어댑터 케이블" });
            cbCategory.Location = new Point(129, 25);
            cbCategory.Margin = new Padding(20, 15, 20, 10);
            cbCategory.Name = "cbCategory";
            cbCategory.Size = new Size(225, 31);
            cbCategory.TabIndex = 15;
            // 
            // TlpButtons
            // 
            TlpButtons.ColumnCount = 2;
            TlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TlpButtons.Controls.Add(btnOk, 0, 0);
            TlpButtons.Controls.Add(btnCancel, 1, 0);
            TlpButtons.Location = new Point(112, 213);
            TlpButtons.Name = "TlpButtons";
            TlpButtons.RowCount = 1;
            TlpButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TlpButtons.Size = new Size(181, 35);
            TlpButtons.TabIndex = 16;
            // 
            // btnOk
            // 
            btnOk.BackgroundImage = Properties.Resources.Button;
            btnOk.BackgroundImageLayout = ImageLayout.Zoom;
            btnOk.Cursor = Cursors.Hand;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnOk.ForeColor = Color.FromArgb(145, 223, 251);
            btnOk.Location = new Point(3, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(60, 29);
            btnOk.TabIndex = 13;
            btnOk.Text = "확인";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackgroundImage = Properties.Resources.Button;
            btnCancel.BackgroundImageLayout = ImageLayout.Zoom;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnCancel.ForeColor = Color.FromArgb(145, 223, 251);
            btnCancel.Location = new Point(93, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(60, 29);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "취소";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AddCableForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(38, 38, 38);
            ClientSize = new Size(384, 261);
            Controls.Add(TlpInput);
            MaximumSize = new Size(400, 300);
            MinimumSize = new Size(400, 300);
            Name = "AddCableForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "시험 준비물 추가";
            TlpInput.ResumeLayout(false);
            TlpInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).EndInit();
            TlpButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TlpInput;
        private NumericUpDown numCount;
        private TextBox txtName;
        private Label lblCategory;
        private Label lblName;
        private Label lblCount;
        private Button btnCancel;
        private ComboBox cbCategory;
        private TableLayoutPanel TlpButtons;
        private Button btnOk;
    }
}
