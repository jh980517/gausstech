using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Models;

namespace PinConnectionDiagram
{
    /// <summary>
    /// 새로운 시험 준비물의 분류, 이름, 수량을 입력받는 대화상자다.
    /// </summary>
    public partial class AddCableForm : Form
    {
        private readonly HashSet<string> existingNames;
        public CableInfo CableInfo { get; private set; } = null!;

        public AddCableForm(IEnumerable<string>? existingNames = null)
        {
            InitializeComponent();
            ApplyFormStyle();
            this.existingNames = new HashSet<string>(
                existingNames ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);

            // 디자이너 설정과 관계없이 준비물 수량은 항상 1부터 시작한다.
            numCount.Minimum = 1;
            numCount.Value = 1;

            this.AcceptButton = btnOk;

            ButtonHelper.ApplyButtonEffect(
                btnOk,
                AppTheme.GetStandardButtonImage(false),
                AppTheme.GetStandardButtonImage(true));

            ButtonHelper.ApplyButtonEffect(
                btnCancel,
                AppTheme.GetStandardButtonImage(false),
                AppTheme.GetStandardButtonImage(true));

            ButtonHelper.CancelButtonFunction(btnCancel, this);
        }

        private void ApplyFormStyle()
        {
            // 프로젝트의 남색·하늘색 포인트를 사용해 입력 폼을 카드형 대화상자로 정리한다.
            ClientSize = new Size(450, 330);
            MinimumSize = new Size(466, 369);
            MaximumSize = new Size(466, 369);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            BackColor = AppTheme.Background;

            Label header = new Label
            {
                BackColor = AppTheme.DarkAccent,
                ForeColor = AppTheme.Accent,
                Font = new Font("맑은 고딕", 16F, FontStyle.Bold),
                Location = Point.Empty,
                Size = new Size(ClientSize.Width, 68),
                Text = "시험 준비물 추가",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(24, 0, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(header);
            header.BringToFront();

            TlpInput.Dock = DockStyle.None;
            TlpInput.Location = new Point(0, 68);
            TlpInput.Size = new Size(ClientSize.Width, ClientSize.Height - 68);
            TlpInput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TlpInput.BackColor = AppTheme.Background;
            TlpInput.Padding = new Padding(22, 14, 22, 12);
            TlpInput.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 105F);
            TlpInput.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100F);
            TlpInput.RowStyles[0] = new RowStyle(SizeType.Absolute, 58F);
            TlpInput.RowStyles[1] = new RowStyle(SizeType.Absolute, 58F);
            TlpInput.RowStyles[2] = new RowStyle(SizeType.Absolute, 58F);
            TlpInput.RowStyles[3] = new RowStyle(SizeType.Percent, 100F);

            foreach (Label label in new[] { lblCategory, lblName, lblCount })
            {
                label.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
                label.ForeColor = Color.WhiteSmoke;
                label.Margin = new Padding(0, 8, 12, 8);
            }
            ApplyInputLabelStyle(
                lblCategory,
                AppTheme.Accent,
                AppTheme.DarkAccent);
            ApplyInputLabelStyle(
                lblName,
                ColorHelper.GetMapHeaderColor("지그 케이블"),
                AppTheme.DarkAccent);
            ApplyInputLabelStyle(
                lblCount,
                ColorHelper.GetMapHeaderColor("시험 대상 케이블"),
                AppTheme.DarkAccent);

            foreach (Control input in new Control[] { cbCategory, txtName, numCount })
            {
                input.BackColor = AppTheme.Background;
                input.ForeColor = AppTheme.Accent;
            }

            cbCategory.Margin = new Padding(10, 10, 0, 8);
            txtName.Margin = new Padding(10, 10, 0, 8);
            numCount.Margin = new Padding(10, 10, 170, 8);
            cbCategory.Font = new Font("맑은 고딕", 11.5F, FontStyle.Bold);
            txtName.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            numCount.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);

            TlpButtons.Dock = DockStyle.Fill;
            TlpButtons.Margin = new Padding(55, 8, 0, 0);
            TlpButtons.BackColor = Color.Transparent;
            foreach (Button button in new[] { btnOk, btnCancel })
            {
                button.Dock = DockStyle.Fill;
                button.Margin = new Padding(6, 4, 6, 4);
                button.Font = new Font("맑은 고딕", 10.5F, FontStyle.Bold);
                button.ForeColor = AppTheme.Accent;
                button.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }

        private static void ApplyInputLabelStyle(
            Label label,
            Color accentColor,
            Color backgroundColor)
        {
            label.BackColor = backgroundColor;
            label.BorderStyle = BorderStyle.None;
            label.Paint += (_, e) =>
            {
                // 좌측 포인트 바와 얇은 외곽선으로 입력 항목을 카드처럼 구분한다.
                using SolidBrush accentBrush = new SolidBrush(accentColor);
                using Pen borderPen = new Pen(Color.FromArgb(90, accentColor));
                e.Graphics.FillRectangle(accentBrush, 0, 0, 5, label.Height);
                e.Graphics.DrawRectangle(
                    borderPen,
                    0,
                    0,
                    Math.Max(0, label.Width - 1),
                    Math.Max(0, label.Height - 1));
            };
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // 모든 값이 유효한 경우에만 호출 측에 CableInfo를 반환한다.
            if (string.IsNullOrWhiteSpace(cbCategory.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                (numCount.Value <= 0))
            {
                ProjectMessageBox.Show(
                    "항목을 모두 입력해주세요.",
                    "알림",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            string cableName = txtName.Text.Trim();
            if (existingNames.Contains(cableName))
            {
                ProjectMessageBox.Show(
                    "이미 등록된 준비물 명칭입니다.",
                    "중복 명칭",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtName.Focus();
                txtName.SelectAll();
                return;
            }

            CableInfo = new CableInfo
            {
                Category = cbCategory.Text,
                Name = cableName,
                Count = (int)numCount.Value
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
