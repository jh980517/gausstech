using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Managers;
using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;
namespace PinConnectionDiagram
{
    /// <summary>
    /// 시험 준비물과 핀 연결도 화면을 구성하고 사용자 작업을 각 관리자에 전달한다.
    /// </summary>
    public partial class Main : Form
    {
        // CableManager
        // MapManager
        // ConnectionManager
        private CableManager cableManager;
        private MapManager mapManager = null!;
        private readonly Button btnTheme;

        public Main()
        {
            InitializeComponent();

            btnTheme = new Button
            {
                Anchor = AnchorStyles.Right,
                BackgroundImage = Properties.Resources.Button,
                BackgroundImageLayout = ImageLayout.Stretch,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 9.5F, FontStyle.Bold),
                ForeColor = AppTheme.Accent,
                Margin = new Padding(0, 8, 18, 8),
                Size = new Size(110, 34),
                Text = "국방 테마",
                TabStop = false
            };
            btnTheme.FlatAppearance.BorderSize = 0;
            btnTheme.Click += btnTheme_Click;
            TlpHead1.Controls.Add(btnTheme, 1, 0);
            ButtonHelper.ApplyButtonEffect(
                btnTheme,
                Properties.Resources.Button,
                Properties.Resources.Button_push);

            ButtonHelper.ApplyButtonEffect(
            btnBack,
                Properties.Resources.back,
                Properties.Resources.back_push);
            ButtonHelper.ApplyButtonEffect(
            btnForward,
                Properties.Resources.forward,
                Properties.Resources.forward_push);
            ButtonHelper.ApplyButtonEffect(
            btnReset,
                Properties.Resources.reset,
                Properties.Resources.reset_push);
            ButtonHelper.ApplyButtonEffect(
            btnSupAdd,
                Properties.Resources.추가,
                Properties.Resources.추가_push);
            ButtonHelper.ApplyButtonEffect(
                btnCancel,
                Properties.Resources.Button,
                Properties.Resources.Button_push);
            ButtonHelper.ApplyButtonEffect(
                btnCreate,
                Properties.Resources.Button,
                Properties.Resources.Button_push);
            ButtonHelper.ApplyButtonEffect(
                btnDone,
                Properties.Resources.Button,
                Properties.Resources.Button_push);

            btnBack.Click += btnBack_Click;
            btnForward.Click += btnForward_Click;
            btnReset.Click += btnReset_Click;
            btnCreate.Click += btnCreate_Click;
            btnCancel.Click += btnCancel_Click;
            btnDone.Click += btnDone_Click;

            /****************************************************************************/

            cableManager = new CableManager();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // 디자이너가 모든 부모 컨트롤을 만든 뒤 MapManager를 생성해야 한다.
            mapManager = new MapManager(
                TlpMap,
                () => cableManager.Cables.ToList());
            mapManager.SuppliesRestoreRequested += RestoreSupplies;

            mapManager.Create();
            mapManager.HistoryChanged += MapManager_HistoryChanged;
            UpdateHistoryButtons();
            ApplyCurrentTheme();
        }

        private void btnTheme_Click(object? sender, EventArgs e)
        {
            AppTheme.Toggle();
            ApplyCurrentTheme();
        }

        private void ApplyCurrentTheme()
        {
            btnTheme.Text = AppTheme.IsDefense ? "기본 테마" : "국방 테마";
            btnTheme.ForeColor = AppTheme.Accent;
            btnCancel.ForeColor = AppTheme.Accent;
            btnCreate.ForeColor = AppTheme.Accent;
            btnDone.ForeColor = AppTheme.Accent;
            LblTitle1.ForeColor = AppTheme.Accent;
            LblTitle2.ForeColor = AppTheme.Accent;
            TlpHead1.BackColor = AppTheme.Background;
            TlpHead2.BackColor = AppTheme.Background;
            TlpMap.BackColor = AppTheme.Background;

            JigCable.BackColor = ColorHelper.GetMapHeaderColor("지그 케이블");
            AdapterCable.BackColor = ColorHelper.GetMapHeaderColor("어댑터 케이블");
            TestCable.BackColor = ColorHelper.GetMapHeaderColor("시험 대상 케이블");
            LblJigHead.BackColor = JigCable.BackColor;
            LblAdapterHead.BackColor = AdapterCable.BackColor;
            LblTestHead.BackColor = TestCable.BackColor;

            foreach (Control control in new Control[]
            {
                FlpItemBox1,
                FlpItemBox2,
                FlpItemBox3
            })
            {
                control.BackColor = AppTheme.ContentBackground;
            }

            if (mapManager != null)
            {
                for (int tjNumber = 1; tjNumber <= 5; tjNumber++)
                {
                    bool isOn = mapManager.GetTJ(tjNumber).IsOn;
                    foreach (ConnectorType type in Enum.GetValues<ConnectorType>())
                        mapManager.GetPanel(tjNumber, type).SetActive(isOn);
                }

                mapManager.RefreshView();
            }

            // 테마 전환 전에 생성된 목록 항목과 배치 항목도 새 카테고리 색으로 갱신한다.
            ApplyThemeToCableItems(TlpBg);

            TlpBg.Invalidate(true);
            FlpSupplies.Invalidate(true);
            PnlMap.Invalidate(true);
        }

        private static void ApplyThemeToCableItems(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is CableItem cableItem)
                    cableItem.ApplyTheme();
                else if (control is DropItem dropItem)
                    dropItem.ApplyTheme();
                else if (control is CableCard cableCard)
                    cableCard.ApplyTheme();

                if (control.HasChildren)
                    ApplyThemeToCableItems(control);
            }
        }

        private void btnBack_Click(object? sender, EventArgs e)
        {
            mapManager.Undo();
        }

        private void btnForward_Click(object? sender, EventArgs e)
        {
            mapManager.Redo();
        }

        private void btnReset_Click(object? sender, EventArgs e)
        {
            if (!mapManager.ResetAll())
                return;

            ResetSupplies();
            mapManager.RecordExternalChange();
        }

        private void btnCreate_Click(object? sender, EventArgs e)
        {
            if (!mapManager.TryBuildTestProcedure(
                out string title,
                out string procedure,
                out string errorMessage))
            {
                ProjectMessageBox.Show(
                    errorMessage,
                    "설명문 생성",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using DescriptionForm form = new DescriptionForm(title, procedure);
            form.ShowDialog(this);
        }

        private void btnDone_Click(object? sender, EventArgs e)
        {
            if (!mapManager.TryBuildTestProcedure(
                out string title,
                out string procedure,
                out string errorMessage))
            {
                ProjectMessageBox.Show(
                    errorMessage,
                    "PDF 생성",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using Bitmap diagram = mapManager.RenderConnectionDiagram();
            using Bitmap supplies = RenderSuppliesForExport();
            List<Bitmap> pages = Helpers.PdfExportService.CreatePages(
                title,
                diagram,
                procedure,
                supplies);
            using PdfPreviewForm previewForm = new PdfPreviewForm(title, pages);
            previewForm.ShowDialog(this);
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Windows 종료처럼 사용자가 선택할 수 없는 시스템 종료 과정은 방해하지 않는다.
            if (e.CloseReason != CloseReason.UserClosing)
            {
                base.OnFormClosing(e);
                return;
            }

            DialogResult result = ProjectMessageBox.Show(
                "현재 작업을 종료하고 창을 닫으시겠습니까?",
                "프로그램 종료",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                e.Cancel = true;

            base.OnFormClosing(e);
        }

        private Bitmap RenderSuppliesForExport()
        {
            List<CableCard> cards = FlpSupplies.Controls
                .OfType<CableCard>()
                .OrderBy(card => card.Info.Category switch
                {
                    "지그 케이블" => 0,
                    "어댑터 케이블" => 1,
                    "시험 대상 케이블" => 2,
                    _ => 3
                })
                .ThenBy(card => card.Info.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (cards.Count == 0)
                return new Bitmap(1, 1);

            const int columnsPerRow = 4;
            const int spacing = 5;
            int columnCount = Math.Min(columnsPerRow, cards.Count);
            int rowCount = (int)Math.Ceiling(cards.Count / (double)columnsPerRow);
            int cardWidth = cards.Max(card => card.ExportSize.Width);
            int cardHeight = cards.Max(card => card.ExportSize.Height);
            int width = columnCount * cardWidth + Math.Max(0, columnCount - 1) * spacing;
            int height = rowCount * cardHeight + Math.Max(0, rowCount - 1) * spacing;
            Bitmap result = new Bitmap(width, height);

            using Graphics graphics = Graphics.FromImage(result);
            graphics.Clear(Color.Transparent);
            for (int index = 0; index < cards.Count; index++)
            {
                CableCard card = cards[index];
                int column = index % columnsPerRow;
                int row = index / columnsPerRow;
                int x = column * (cardWidth + spacing);
                int y = row * (cardHeight + spacing);
                using Bitmap cardImage = card.RenderForExport();
                graphics.DrawImageUnscaled(cardImage, x, y);
            }

            return result;
        }

        private void ResetSupplies()
        {
            // 최초 실행 화면처럼 추가 버튼을 제외한 준비물 UI와 데이터를 모두 비운다.
            cableManager.Clear();

            foreach (CableCard card in FlpSupplies.Controls.OfType<CableCard>().ToList())
            {
                FlpSupplies.Controls.Remove(card);
                card.Dispose();
            }

            ClearItemPanel(FlpItemBox1);
            ClearItemPanel(FlpItemBox2);
            ClearItemPanel(FlpItemBox3);

            FlpSupplies.AutoScrollPosition = Point.Empty;
            FlpItemBox1.AutoScrollPosition = Point.Empty;
            FlpItemBox2.AutoScrollPosition = Point.Empty;
            FlpItemBox3.AutoScrollPosition = Point.Empty;

            UpdateHistoryButtons();
        }

        private void ClearItemPanel(Control panel)
        {
            foreach (Control control in panel.Controls.Cast<Control>().ToList())
            {
                panel.Controls.Remove(control);
                control.Dispose();
            }
        }

        private void MapManager_HistoryChanged()
        {
            UpdateHistoryButtons();
        }

        private void UpdateHistoryButtons()
        {
            // 이동할 이력이 없는 버튼은 비활성화하여 현재 상태를 명확히 보여준다.
            btnBack.Enabled = mapManager.CanUndo;
            btnForward.Enabled = mapManager.CanRedo;
            btnReset.Enabled = mapManager.CanReset || cableManager.Cables.Count > 0;

            // 활성 상태에 맞는 전용 리소스를 버튼에 직접 지정한다.
            btnBack.BackgroundImage = btnBack.Enabled
                ? Properties.Resources.back
                : Properties.Resources.back_off;
            btnForward.BackgroundImage = btnForward.Enabled
                ? Properties.Resources.forward
                : Properties.Resources.forward_off;
            btnReset.BackgroundImage = btnReset.Enabled
                ? Properties.Resources.reset
                : Properties.Resources.reset_off;
        }

        // 배경 그라디언트 효과 함수
        private void TlpBg_Paint(object sender, PaintEventArgs e)
        {
            // 메인 배경을 위에서 아래로 이어지는 그라데이션으로 채운다.
            if (sender is not Control ctrl)
                return;

            if (ctrl.Width <= 0 || ctrl.Height <= 0)
                return;

            Color startColor = AppTheme.Background;
            Color endColor = AppTheme.BackgroundEnd;

            using (LinearGradientBrush brush =
                new LinearGradientBrush(
                    ctrl.ClientRectangle,
                    startColor,
                    endColor,
                    90f))
            {
                e.Graphics.FillRectangle(brush, ctrl.ClientRectangle);
            }
        }


        // Control에 맞춰 상,하단으로 그라디언트 선 그리기 함수
        public void GlowHorizontalLine_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Control ctrl)
                return;

            // 화면 구분선은 기본 테마의 기존 하늘색과 각 테마의 대표 강조색을 사용한다.
            DrawHelper.DrawGlowLine(e.Graphics, ctrl.Width, 0, 4, AppTheme.Accent);
            DrawHelper.DrawGlowLine(e.Graphics, ctrl.Width, ctrl.Height - 4, 4, AppTheme.Accent);
        }

        // Border 그리기 함수
        private void DrawBorderLine_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Control ctrl)
                return;

            DrawHelper.DrawBorderLine(
                e.Graphics,
                ctrl.Width,
                ctrl.Height,
                3,
                AppTheme.Accent
            );
        }
        private void DrawBorderLine_Paint_Skyblue(object sender, PaintEventArgs e)
        {
            if (sender is not Control ctrl)
                return;

            DrawHelper.DrawBorderLine(
                e.Graphics,
                ctrl.Width,
                ctrl.Height,
                3,
                ColorHelper.GetMapHeaderColor("어댑터 케이블")
            );
        }
        private void DrawBorderLine_Paint_Darkblue(object sender, PaintEventArgs e)
        {
            if (sender is not Control ctrl)
                return;

            DrawHelper.DrawBorderLine(
                e.Graphics,
                ctrl.Width,
                ctrl.Height,
                3,
                ColorHelper.GetMapHeaderColor("지그 케이블")
            );
        }
        private void DrawBorderLine_Paint_Darkpurple(object sender, PaintEventArgs e)
        {
            if (sender is not Control ctrl)
                return;

            DrawHelper.DrawBorderLine(
                e.Graphics,
                ctrl.Width,
                ctrl.Height,
                3,
                ColorHelper.GetMapHeaderColor("시험 대상 케이블")
            );
        }

        // 시험 대상 준비물 추가 /////////////////////////////////////////////////////////////////////////////
        private void btnSupAdd_MouseUp(object sender, MouseEventArgs e)
        {
            // 입력 완료 후 데이터, 카드, 드래그 항목을 같은 CableInfo로 생성한다.
            using (AddCableForm form = new AddCableForm(
                cableManager.Cables.Select(cable => cable.Name)))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    CableInfo info = form.CableInfo;

                    cableManager.Add(info);

                    CableCard card = new CableCard(info);
                    card.DeleteRequested += DeleteCable;
                    FlpSupplies.Controls.Add(card);

                    CreateItems(info);
                    mapManager.RecordExternalChange();

                    // 준비물 추가로 전체 레이아웃 계산이 끝난 다음 연결 화면을 다시 그린다.
                    BeginInvoke(new Action(mapManager.RefreshView));
                }
            }
        }

        // 시험 준비물 추가 시 해당 카테고리에 아이템 생성 함수
        private void CreateItems(CableInfo info)
        {
            VerticalFlowPanel panel = GetPanel(info.Category);

            CableItem item = new CableItem(info);
            panel.Controls.Add(item);

        }

        // 삭제 부분 ///////////////////////////////////////////////////////////////////////////////
        // 추가된 시험 준비물 삭제 여부 확인 및 삭제함수들 실행
        private void DeleteCable(CableInfo info)
        {
            // 하나의 준비물 삭제가 데이터와 두 종류의 UI에 함께 반영되게 한다.
            DialogResult result = ProjectMessageBox.Show(
                "삭제하시겠습니까?",
                "확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // 목록 데이터보다 먼저 연결선 배정을 해제해 DropZone의 표시도 동시에 제거한다.
            mapManager.RemoveCableAssignments(info);
            cableManager.Remove(info);

            DeleteCard(info);
            DeleteItem(info);
            mapManager.RecordExternalChange();
        }

        private void RestoreSupplies(IReadOnlyList<CableInfo> supplies)
        {
            // Undo/Redo 스냅샷의 준비물 데이터로 카드와 드래그 아이템을 함께 재구성한다.
            ResetSupplies();

            foreach (CableInfo info in supplies)
            {
                cableManager.Add(info);

                CableCard card = new CableCard(info);
                card.DeleteRequested += DeleteCable;
                FlpSupplies.Controls.Add(card);
                CreateItems(info);
            }
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            mapManager?.BeginViewportResize();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            mapManager?.EndViewportResize();
        }

        // 케이블 카드 삭제 함수
        private void DeleteCard(CableInfo info)
        {
            foreach (CableCard card in FlpSupplies.Controls.OfType<CableCard>().ToList())
            {
                if (card.Info.Id == info.Id)
                {

                    FlpSupplies.Controls.Remove(card);
                    card.Dispose();
                    break;
                }
            }
        }

        // 케이블 카드 삭제 시 해당 케이블 아이템 삭제 함수
        private void DeleteItem(CableInfo info)
        {
            Control panel = GetPanel(info.Category);

            foreach (CableItem item in panel.Controls.OfType<CableItem>().ToList())
            {
                if (item.Info.Id == info.Id)
                {
                    panel.Controls.Remove(item);
                    item.Dispose();
                }
            }
        }


        // 카테고리/////////////////////////////////////////////////////////////////////////
        private VerticalFlowPanel GetPanel(string category)
        {
            // 입력 폼의 분류명과 화면의 케이블 목록 영역을 연결한다.
            switch (category)
            {
                case "시험 대상 케이블":
                    return FlpItemBox1;
                case "지그 케이블":
                    return FlpItemBox2;
                case "어댑터 케이블":
                    return FlpItemBox3;
                default:
                    throw new Exception("알 수 없는 카테고리입니다.");
            }
        }
    }
}
