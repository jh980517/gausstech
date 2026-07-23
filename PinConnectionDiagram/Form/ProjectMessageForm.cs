using PinConnectionDiagram.Helpers;

namespace PinConnectionDiagram
{
    /// <summary>확인, 질문, 경고 및 안내 메시지를 프로젝트 디자인으로 표시한다.</summary>
    public partial class ProjectMessageForm : Form
    {
        public ProjectMessageForm(string message, string title, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            InitializeComponent();
            ApplyTheme();
            lblTitle.Text = title;
            lblMessage.Text = message;
            ApplyIcon(icon);
            CreateButtons(buttons, defaultButton);
        }

        private void ApplyTheme()
        {
            BackColor = AppTheme.Background;
            lblTitle.BackColor = AppTheme.DarkAccent;
            lblTitle.ForeColor = AppTheme.Accent;
            lblMessage.ForeColor = Color.WhiteSmoke;
            flpButtons.BackColor = AppTheme.Background;
        }

        // 메시지 종류별 기호와 강조색을 프로젝트 색상 체계에 맞춰 선택한다.
        private void ApplyIcon(MessageBoxIcon icon)
        {
            (lblIcon.Text, lblIcon.ForeColor) = icon switch
            {
                MessageBoxIcon.Warning => ("!", Color.FromArgb(255, 190, 80)),
                MessageBoxIcon.Question => ("?", AppTheme.AccentHover),
                MessageBoxIcon.Error => ("!", Color.FromArgb(255, 90, 100)),
                _ => ("i", AppTheme.Accent)
            };
        }

        // 요청된 버튼 조합과 기본 버튼을 동적으로 구성해 MessageBox와 같은 사용성을 제공한다.
        private void CreateButtons(MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            if (buttons == MessageBoxButtons.YesNoCancel)
            {
                Button saveButton = CreateButton("저장", DialogResult.Yes);
                Button discardButton = CreateButton("저장 안 함", DialogResult.No);
                Button cancelButton = CreateButton("취소", DialogResult.Cancel);
                flpButtons.Width = 294;
                flpButtons.Left = ClientSize.Width - flpButtons.Width - 26;
                flpButtons.Controls.Add(cancelButton);
                flpButtons.Controls.Add(discardButton);
                flpButtons.Controls.Add(saveButton);
                AcceptButton = defaultButton switch
                {
                    MessageBoxDefaultButton.Button2 => discardButton,
                    MessageBoxDefaultButton.Button3 => cancelButton,
                    _ => saveButton
                };
                CancelButton = cancelButton;
                ActiveControl = AcceptButton as Control;
                return;
            }

            if (buttons == MessageBoxButtons.YesNo)
            {
                Button yesButton = CreateButton("예", DialogResult.Yes);
                Button noButton = CreateButton("아니오", DialogResult.No);
                flpButtons.Controls.Add(noButton);
                flpButtons.Controls.Add(yesButton);
                AcceptButton = defaultButton == MessageBoxDefaultButton.Button2 ? noButton : yesButton;
                CancelButton = noButton;
                ActiveControl = AcceptButton as Control;
                return;
            }

            Button okButton = CreateButton("확인", DialogResult.OK);
            flpButtons.Controls.Add(okButton);
            AcceptButton = okButton;
            CancelButton = okButton;
            ActiveControl = okButton;
        }

        // 공통 크기와 이미지 효과를 가진 대화상자 버튼을 생성한다.
        private Button CreateButton(string text, DialogResult result)
        {
            Button button = new Button
            {
                BackgroundImageLayout = ImageLayout.Zoom,
                Cursor = Cursors.Hand,
                DialogResult = result,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 10.5F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                Margin = new Padding(6, 0, 0, 0),
                Size = new Size(88, 38),
                Text = text,
                UseVisualStyleBackColor = true
            };
            button.FlatAppearance.BorderSize = 0;
            ButtonHelper.ApplyButtonEffect(
                button,
                AppTheme.GetStandardButtonImage(false),
                AppTheme.GetStandardButtonImage(true));
            return button;
        }
    }
}
