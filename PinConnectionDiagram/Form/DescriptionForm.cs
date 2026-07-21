namespace PinConnectionDiagram
{
    /// <summary>
    /// 현재 연결도를 기반으로 생성한 시험 절차를 별도 창에 표시한다.
    /// </summary>
    public partial class DescriptionForm : Form
    {
        public DescriptionForm(string title, string procedure)
        {
            InitializeComponent();
            lblTitle.Text = title;
            ApplyButtonStyle();
            DisplayProcedure(procedure);
        }

        private void ApplyButtonStyle()
        {
            // 메인 화면과 동일한 이미지 버튼 효과를 적용해 디자인 흐름을 맞춘다.
            Helpers.ButtonHelper.ApplyButtonEffect(
                btnCopy,
                Properties.Resources.Button,
                Properties.Resources.Button_push);
            Helpers.ButtonHelper.ApplyButtonEffect(
                btnClose,
                Properties.Resources.Button,
                Properties.Resources.Button_push);
        }

        private void DisplayProcedure(string procedure)
        {
            txtProcedure.Clear();
            string[] steps = procedure.Split(
                new[] { Environment.NewLine + Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            int procedureNumber = 0;

            for (int index = 0; index < steps.Length; index++)
            {
                string step = steps[index].Trim();

                if (step.StartsWith("[") && step.EndsWith("]"))
                {
                    string sectionName = step.Trim('[', ']');
                    txtProcedure.SelectionColor = Color.FromArgb(183, 99, 255);
                    txtProcedure.SelectionFont = new Font("맑은 고딕", 14F, FontStyle.Bold);
                    txtProcedure.AppendText($"◆ {sectionName}");
                    // 영역 제목과 첫 절차 사이를 한 줄 더 띄워 내용 덩어리를 명확히 구분한다.
                    txtProcedure.AppendText(
                        Environment.NewLine + Environment.NewLine + Environment.NewLine);
                    continue;
                }

                if (step.StartsWith("- "))
                    step = step[2..];

                // 순번을 포인트 색상으로 분리해 긴 절차에서도 현재 항목을 쉽게 찾게 한다.
                txtProcedure.SelectionColor = Color.FromArgb(145, 223, 251);
                txtProcedure.SelectionFont = new Font("맑은 고딕", 13F, FontStyle.Bold);
                procedureNumber++;
                txtProcedure.AppendText($"{procedureNumber:00}  ");

                txtProcedure.SelectionColor = Color.WhiteSmoke;
                txtProcedure.SelectionFont = new Font("맑은 고딕", 12.5F, FontStyle.Regular);
                AppendHighlightedText(step);
                txtProcedure.AppendText(Environment.NewLine + Environment.NewLine);
            }

            // 실제 줄바꿈이 완료된 마지막 문자의 위치를 사용해 스크롤 가능한 전체 높이를 계산한다.
            txtProcedure.Width = Math.Max(200, pnlProcedure.ClientSize.Width - pnlProcedure.Padding.Horizontal);
            txtProcedure.PerformLayout();
            Point lastCharacter = txtProcedure.GetPositionFromCharIndex(Math.Max(0, txtProcedure.TextLength - 1));
            int actualContentHeight = lastCharacter.Y + txtProcedure.Font.Height + 70;
            txtProcedure.Height = Math.Max(
                pnlProcedure.ClientSize.Height - pnlProcedure.Padding.Vertical,
                actualContentHeight);
            pnlProcedure.AutoScrollMinSize = new Size(0, txtProcedure.Bottom + pnlProcedure.Padding.Bottom);

            txtProcedure.SelectionStart = 0;
            txtProcedure.ScrollToCaret();

            txtProcedure.MouseWheel += (_, e) =>
                pnlProcedure.ScrollByWheelDelta(e.Delta);
        }

        private void AppendHighlightedText(string text)
        {
            // TJ, 케이블 명칭, 커넥터 명칭을 굵게 표시해 긴 절차에서도 연결 대상을 빠르게 찾게 한다.
            const string pattern = @"(TJ\d+(?:\s*~\s*TJ\d+)?|(?:지그|어댑터|시험 대상) 케이블\s+\S+|“[^”]+”)";
            int currentIndex = 0;

            foreach (System.Text.RegularExpressions.Match match in
                System.Text.RegularExpressions.Regex.Matches(text, pattern))
            {
                txtProcedure.SelectionFont = new Font("맑은 고딕", 12.5F, FontStyle.Regular);
                txtProcedure.AppendText(text[currentIndex..match.Index]);
                txtProcedure.SelectionFont = new Font("맑은 고딕", 12.5F, FontStyle.Bold);
                txtProcedure.AppendText(match.Value);
                currentIndex = match.Index + match.Length;
            }

            txtProcedure.SelectionFont = new Font("맑은 고딕", 12.5F, FontStyle.Regular);
            txtProcedure.AppendText(text[currentIndex..]);
        }

        private void btnCopy_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtProcedure.Text))
                Clipboard.SetText(txtProcedure.Text);
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
