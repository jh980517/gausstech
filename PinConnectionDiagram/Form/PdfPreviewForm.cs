using PinConnectionDiagram.Helpers;
using System.Drawing.Imaging;

namespace PinConnectionDiagram
{
    /// <summary>출력 페이지를 확인하고 PDF, JPG 또는 PNG 형식으로 저장한다.</summary>
    public partial class PdfPreviewForm : Form
    {
        private readonly string documentTitle;
        private readonly List<Bitmap> pages;
        private int pageIndex;

        public PdfPreviewForm(string title, List<Bitmap> pages)
        {
            InitializeComponent();
            documentTitle = title;
            this.pages = pages;
            lblTitle.Text = $"{title} 출력 미리보기";
            ApplyTheme();
            ApplyButtonStyles();
            ApplyNavigationButtonTextStyle();
            ShowPage(0);
        }

        private void ApplyTheme()
        {
            BackColor = AppTheme.Background;
            lblTitle.BackColor = AppTheme.DarkAccent;
            lblTitle.ForeColor = AppTheme.Accent;
            pnlPreview.BackColor = AppTheme.BackgroundEnd;
            lblPage.ForeColor = AppTheme.Accent;

            foreach (Button button in new[] { btnPrevious, btnNext, btnSave, btnClose })
                button.ForeColor = AppTheme.Accent;
        }

        // 폼이 닫힐 때 미리보기 페이지 비트맵까지 해제해 GDI 리소스 누수를 방지한다.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                picturePreview.Image = null;
                foreach (Bitmap page in pages)
                    page.Dispose();
            }
            base.Dispose(disposing);
        }

        // 미리보기 창의 모든 버튼에 프로젝트 공통 이미지 효과를 적용한다.
        private void ApplyButtonStyles()
        {
            foreach (Button button in new[] { btnSave, btnClose })
            {
                ButtonHelper.ApplyButtonEffect(
                    button,
                    Properties.Resources.Button,
                    Properties.Resources.Button_push);
            }
        }

        // 평상시에는 기존 청록색을 사용하고 누르는 동안에만 흰색으로 바꿔 대비를 유지한다.
        private void ApplyNavigationButtonTextStyle()
        {
            foreach (Button button in new[] { btnPrevious, btnNext })
            {
                string navigationText = button.Text;
                button.Cursor = Cursors.Hand;
                button.UseVisualStyleBackColor = false;
                button.BackgroundImage = Properties.Resources.Button;
                button.ForeColor = AppTheme.Accent;
                button.FlatAppearance.MouseOverBackColor = Color.Transparent;
                button.FlatAppearance.MouseDownBackColor = Color.Transparent;
                button.MouseDown += (_, _) =>
                {
                    if (!button.Enabled)
                        return;

                    button.BackgroundImage = Properties.Resources.Button_push;
                    button.ForeColor = AppTheme.Accent;
                };
                button.MouseUp += (_, _) => RestoreNavigationButton(button);
                button.MouseLeave += (_, _) => RestoreNavigationButton(button);

                // 이동할 페이지가 없으면 누른 상태 이미지와 청록색 글자로 비활성을 표현한다.
                button.EnabledChanged += (_, _) =>
                {
                    button.Text = button.Enabled ? navigationText : string.Empty;
                    button.BackgroundImage = button.Enabled
                        ? Properties.Resources.Button
                        : Properties.Resources.Button_push;
                    button.BackgroundImageLayout = ImageLayout.Stretch;
                    button.Cursor = button.Enabled ? Cursors.Hand : Cursors.Default;
                    button.Invalidate();
                };
                button.Paint += (_, e) =>
                {
                    if (button.Enabled)
                        return;

                    TextRenderer.DrawText(
                        e.Graphics,
                        navigationText,
                        button.Font,
                        button.ClientRectangle,
                        AppTheme.Accent,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.SingleLine);
                };
            }
        }

        // 이동 버튼을 기본 이미지와 기존 청록색 글자 상태로 되돌린다.
        private static void RestoreNavigationButton(Button button)
        {
            if (!button.Enabled)
                return;

            button.BackgroundImage = Properties.Resources.Button;
            button.ForeColor = AppTheme.Accent;
        }

        // 요청한 페이지 번호를 유효 범위로 제한하고 이동 버튼의 활성 상태를 갱신한다.
        private void ShowPage(int index)
        {
            pageIndex = Math.Clamp(index, 0, pages.Count - 1);
            picturePreview.Image = pages[pageIndex];
            lblPage.Text = $"{pageIndex + 1} / {pages.Count}";
            btnPrevious.Enabled = pageIndex > 0;
            btnNext.Enabled = pageIndex < pages.Count - 1;
        }

        private void btnPrevious_Click(object? sender, EventArgs e) => ShowPage(pageIndex - 1);
        private void btnNext_Click(object? sender, EventArgs e) => ShowPage(pageIndex + 1);
        private void btnClose_Click(object? sender, EventArgs e) => Close();

        // 선택한 확장자에 따라 PDF 또는 페이지별 이미지 저장 기능을 호출한다.
        private void btnSave_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog dialog = new SaveFileDialog
            {
                AddExtension = true,
                FileName = SanitizeFileName(GetDefaultFileName(documentTitle)),
                Filter = "PNG 이미지 (*.png)|*.png|PDF 문서 (*.pdf)|*.pdf|JPEG 이미지 (*.jpg)|*.jpg",
                FilterIndex = 1,
                OverwritePrompt = true,
                Title = "연결도 및 시험 절차 저장"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                string extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                if (extension == ".pdf")
                    PdfExportService.Save(dialog.FileName, pages);
                else if (extension is ".jpg" or ".jpeg")
                    PdfExportService.SaveImages(dialog.FileName, pages, ImageFormat.Jpeg);
                else
                    PdfExportService.SaveImages(dialog.FileName, pages, ImageFormat.Png);

                ProjectMessageBox.Show(
                    "연결도와 시험 절차가 저장되었습니다.",
                    "출력 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                ProjectMessageBox.Show(
                    $"파일을 저장하지 못했습니다.\n{exception.Message}",
                    "출력 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // 문서 제목에 파일명으로 사용할 수 없는 문자가 있으면 안전한 문자로 치환한다.
        private static string SanitizeFileName(string value)
        {
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                value = value.Replace(invalidCharacter, '_');
            return value;
        }

        private static string GetDefaultFileName(string title)
        {
            const string diagramTitleSuffix = " 시험 연결도";
            return title.EndsWith(diagramTitleSuffix, StringComparison.Ordinal)
                ? title[..^diagramTitleSuffix.Length]
                : title;
        }
    }
}
