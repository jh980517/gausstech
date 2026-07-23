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
        private const int WmSetRedraw = 0x000B;
        private const int WmSysCommand = 0x0112;
        private const int WmNcLButtonDown = 0x00A1;
        private const int HtCaption = 2;
        private const int ScMaximize = 0xF030;
        private const int ScRestore = 0xF120;
        private const uint RdwInvalidate = 0x0001;
        private const uint RdwErase = 0x0004;
        private const uint RdwAllChildren = 0x0080;
        private const uint RdwUpdateNow = 0x0100;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr windowHandle,
            int message,
            IntPtr wParam,
            IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool RedrawWindow(
            IntPtr windowHandle,
            IntPtr updateRectangle,
            IntPtr updateRegion,
            uint flags);

        // CableManager
        // MapManager
        // ConnectionManager
        private CableManager cableManager;
        private MapManager mapManager = null!;
        private readonly Button btnDefaultTheme;
        private readonly Button btnDefenseTheme;
        private readonly Button btnNewProject;
        private readonly Button btnOpenProject;
        private readonly Button btnSaveProject;
        private readonly FlowLayoutPanel pnlThemeButtons;
        private readonly Label lblThemeGroup;
        private readonly ToolTip themeToolTip;
        private readonly Label lblProgramVersion;
        private string? currentProjectPath;
        private bool hasUnsavedChanges;
        private bool isLoadingProject;
        private bool wasMinimized;
        private bool? compactLayoutApplied;
        private int windowStateTransitionVersion;

        public Main()
        {
            InitializeComponent();

            // 실행 파일에 포함된 공식 아이콘을 메인 창과 작업표시줄에도 동일하게 표시한다.
            using (Icon? applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath))
            {
                if (applicationIcon != null)
                    Icon = (Icon)applicationIcon.Clone();
            }

            lblProgramVersion = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("맑은 고딕", 9F, FontStyle.Regular),
                Margin = new Padding(3, 0, 12, 8),
                // ProductVersion 뒤에 자동으로 붙는 Git 커밋 식별자는 사용자 화면에서 제외한다.
                Text = $"v{Application.ProductVersion.Split('+')[0]}",
                TextAlign = ContentAlignment.BottomRight
            };
            TlpFoot.Controls.Add(lblProgramVersion, 1, 0);

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            DoubleBuffered = true;
            BackColor = AppTheme.Background;
            EnableLayoutDoubleBuffering();
            ApplyInitialWindowSize();
            ApplyResponsiveLayout();

            TlpHead1.SuspendLayout();
            TlpHead1.ColumnCount = 4;
            TlpHead1.ColumnStyles.Clear();
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 141F));
            TlpHead1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
            TlpHead1.SetColumn(LblTitle1, 0);
            TlpHead1.SetColumn(TlpHeadBtn, 2);
            TlpHeadBtn.Dock = DockStyle.Fill;
            // 헤더의 상·하단 그라데이션 선을 가리지 않도록 작업 버튼 영역을 안쪽으로 제한한다.
            TlpHeadBtn.Margin = new Padding(0, 5, 0, 5);
            TlpHeadBtn.Padding = Padding.Empty;
            foreach (Button historyButton in new[] { btnBack, btnForward, btnReset })
            {
                historyButton.Anchor = AnchorStyles.None;
                historyButton.Margin = Padding.Empty;
            }

            FlowLayoutPanel projectButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = new Padding(7, 7, 0, 0),
                WrapContents = false,
                BackColor = Color.Transparent
            };

            pnlThemeButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty,
                Padding = new Padding(13, 10, 0, 0),
                WrapContents = false,
                BackColor = Color.Transparent
            };
            pnlThemeButtons.Paint += (_, paintEvent) =>
            {
                using Pen separator = new Pen(Color.FromArgb(110, AppTheme.Accent), 1F);
                paintEvent.Graphics.DrawLine(separator, 3, 11, 3, pnlThemeButtons.Height - 11);
            };

            lblThemeGroup = new Label
            {
                AutoSize = false,
                Font = new Font("맑은 고딕", 8F, FontStyle.Bold),
                Margin = new Padding(0, 5, 3, 0),
                Size = new Size(34, 20),
                Text = "테마",
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnDefenseTheme = CreateThemeColorButton(
                "국방 테마",
                Color.FromArgb(213, 198, 133));
            btnDefaultTheme = CreateThemeColorButton(
                "기본 테마",
                Color.FromArgb(145, 223, 251));
            btnDefaultTheme.Click += (_, _) => SelectTheme(false);
            btnDefenseTheme.Click += (_, _) => SelectTheme(true);
            btnNewProject = CreateProjectFileButton("새 프로젝트");
            btnOpenProject = CreateProjectFileButton("프로젝트 열기");
            btnSaveProject = CreateProjectFileButton("프로젝트 저장");
            btnNewProject.Click += btnNewProject_Click;
            btnOpenProject.Click += btnOpenProject_Click;
            btnSaveProject.Click += btnSaveProject_Click;

            projectButtons.Controls.Add(btnNewProject);
            projectButtons.Controls.Add(btnOpenProject);
            projectButtons.Controls.Add(btnSaveProject);
            pnlThemeButtons.Controls.Add(lblThemeGroup);
            pnlThemeButtons.Controls.Add(btnDefaultTheme);
            pnlThemeButtons.Controls.Add(btnDefenseTheme);
            TlpHead1.Controls.Add(projectButtons, 1, 0);
            TlpHead1.Controls.Add(pnlThemeButtons, 3, 0);
            TlpHead1.ResumeLayout(true);

            themeToolTip = new ToolTip();
            themeToolTip.SetToolTip(btnDefaultTheme, "기본 테마");
            themeToolTip.SetToolTip(btnDefenseTheme, "국방 테마");
            themeToolTip.SetToolTip(btnNewProject, "새 프로젝트 (Ctrl+N)");
            themeToolTip.SetToolTip(btnOpenProject, "프로젝트 열기 (Ctrl+O)");
            themeToolTip.SetToolTip(btnSaveProject, "프로젝트 저장 (Ctrl+S)");
            themeToolTip.SetToolTip(btnSupAdd, "시험 준비물 추가");
            themeToolTip.SetToolTip(btnBack, "이전 작업으로 되돌리기 (Ctrl+Z)");
            themeToolTip.SetToolTip(btnForward, "다음 작업으로 다시 실행 (Ctrl+Shift+Z)");
            themeToolTip.SetToolTip(btnReset, "모든 설정 초기화");
            themeToolTip.SetToolTip(btnDone, "출력 미리보기 (Ctrl+P)");

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
            // 최초 컨트롤 생성과 레이아웃 이벤트는 사용자 변경으로 기록하지 않는다.
            isLoadingProject = true;
            // 디자이너가 모든 부모 컨트롤을 만든 뒤 MapManager를 생성해야 한다.
            mapManager = new MapManager(
                TlpMap,
                () => cableManager.Cables.ToList());
            mapManager.SuppliesRestoreRequested += RestoreSupplies;

            mapManager.Create();
            mapManager.HistoryChanged += MapManager_HistoryChanged;
            UpdateHistoryButtons();
            ApplyCurrentTheme();

            // 초기 레이아웃에서 지연 실행된 이벤트까지 끝난 다음 새 프로젝트를 저장된 상태로 확정한다.
            BeginInvoke(new Action(() =>
            {
                currentProjectPath = null;
                hasUnsavedChanges = false;
                isLoadingProject = false;
                UpdateWindowTitle();
            }));
        }

        private void ApplyInitialWindowSize()
        {
            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            Size = new Size(
                Math.Max(MinimumSize.Width, Math.Min(1400, workingArea.Width - 30)),
                Math.Max(MinimumSize.Height, Math.Min(900, workingArea.Height - 30)));
        }

        private void ApplyResponsiveLayout()
        {
            bool useCompactLayout = ClientSize.Height < 850 || ClientSize.Width < 1400;
            if (compactLayoutApplied == useCompactLayout)
                return;

            compactLayoutApplied = useCompactLayout;
            SuspendLayout();
            TlpBg.SuspendLayout();
            TlpTestSetting.SuspendLayout();
            try
            {
                TlpBg.RowStyles[0].Height = useCompactLayout ? 175F : 210F;
                TlpBg.RowStyles[2].Height = useCompactLayout ? 58F : 70F;
                TlpTestSupplies.RowStyles[0].Height = useCompactLayout ? 52F : 60F;
                TlpTestSetting.RowStyles[0].Height = useCompactLayout ? 52F : 60F;
                TlpSettingBody.ColumnStyles[0].Width = useCompactLayout ? 180F : 240F;
                TlpCable1.RowStyles[0].Height = useCompactLayout ? 42F : 50F;
                TlpCable2.RowStyles[0].Height = useCompactLayout ? 42F : 50F;
                TlpCable3.RowStyles[0].Height = useCompactLayout ? 42F : 50F;
                TlpMap.RowStyles[0].Height = useCompactLayout ? 44F : 50F;

                TlpCableBody.Margin = useCompactLayout
                    ? new Padding(8, 8, 8, 8)
                    : new Padding(20, 10, 20, 10);
                TlpCableBody.Padding = useCompactLayout ? new Padding(6) : new Padding(10);
                TlpMapBody.Margin = useCompactLayout
                    ? new Padding(0, 8, 6, 8)
                    : new Padding(0, 10, 20, 10);
                PnlMap.Margin = useCompactLayout
                    ? new Padding(8)
                    : new Padding(15, 15, 25, 15);
                PnlMap.Padding = useCompactLayout
                    // 커스텀 세로 스크롤바(12px)와 오른쪽 여백(4px)을 항상 확보한다.
                    ? new Padding(0, 0, 16, 0)
                    : new Padding(0, 0, 15, 0);

                float cableHeaderFontSize = useCompactLayout ? 11.5F : 13F;
                foreach (Label header in new[] { JigCable, AdapterCable, TestCable })
                    header.Font = new Font("맑은 고딕", cableHeaderFontSize, FontStyle.Bold);

                btnSupAdd.Size = useCompactLayout ? new Size(68, 68) : new Size(80, 80);
                btnSupAdd.Margin = useCompactLayout
                    ? new Padding(24, 14, 16, 0)
                    : new Padding(30, 18, 20, 0);
            }
            finally
            {
                TlpTestSetting.ResumeLayout(true);
                TlpBg.ResumeLayout(true);
                ResumeLayout(true);
            }
        }

        private void EnableLayoutDoubleBuffering()
        {
            // 디자이너 기본 레이아웃 컨트롤도 한 프레임에 그려 리사이즈 중 흰 배경 노출을 막는다.
            System.Reflection.PropertyInfo? doubleBufferedProperty = typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            if (doubleBufferedProperty == null)
                return;

            foreach (Control control in new Control[]
            {
                TlpBg,
                TlpTestSupplies,
                TlpCardArea,
                TlpTestSetting,
                TlpMap,
                PnlMap,
                FlpSupplies
            })
            {
                doubleBufferedProperty.SetValue(control, true);
            }
        }

        private static Button CreateThemeColorButton(string accessibleName, Color color)
        {
            Button button = new Button
            {
                AccessibleName = accessibleName,
                BackColor = color,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 38, 35),
                Margin = new Padding(6, 5, 0, 0),
                Size = new Size(24, 24),
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(100, 110, 115);
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.12F);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.08F);
            return button;
        }

        private static Button CreateProjectFileButton(string accessibleName)
        {
            Button button = new Button
            {
                AccessibleName = accessibleName,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(6, 0, 0, 0),
                Size = new Size(30, 30),
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 255, 255, 255);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(75, 255, 255, 255);
            return button;
        }

        private void SelectTheme(bool useDefenseTheme)
        {
            // 테마는 화면 표시 환경일 뿐 연결도 프로젝트 데이터의 편집 상태에는 포함하지 않는다.
            // 따라서 테마만 변경한 경우에는 닫기·새 작업·열기 시 저장 여부를 묻지 않는다.
            AppTheme.SetDefense(useDefenseTheme);
            ApplyCurrentTheme();
        }

        private void ApplyCurrentTheme()
        {
            UpdateThemeButtonState(btnDefaultTheme, !AppTheme.IsDefense);
            UpdateThemeButtonState(btnDefenseTheme, AppTheme.IsDefense);
            BackColor = AppTheme.Background;
            btnCancel.ForeColor = AppTheme.Accent;
            btnCreate.ForeColor = AppTheme.Accent;
            btnDone.ForeColor = AppTheme.Accent;
            ApplyThemeButtonImages();
            LblTitle1.ForeColor = AppTheme.Accent;
            LblTitle2.ForeColor = AppTheme.Accent;
            lblProgramVersion.ForeColor = ControlPaint.Dark(AppTheme.Accent, 0.15F);
            lblThemeGroup.ForeColor = AppTheme.Accent;
            pnlThemeButtons.Invalidate();
            UpdateProjectButtonIcons();
            TlpHead1.BackColor = AppTheme.Background;
            TlpHead2.BackColor = AppTheme.Background;
            TlpMap.BackColor = AppTheme.Background;

            JigCable.BackColor = ColorHelper.GetMapHeaderColor("지그 케이블");
            AdapterCable.BackColor = ColorHelper.GetMapHeaderColor("어댑터 케이블");
            TestCable.BackColor = ColorHelper.GetMapHeaderColor("시험 대상 케이블");
            LblJigHead.BackColor = JigCable.BackColor;
            LblAdapterHead.BackColor = AdapterCable.BackColor;
            LblTestHead.BackColor = TestCable.BackColor;
            LblTJHead.BackColor = AppTheme.ControllerHeader;

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
                    TJControl tj = mapManager.GetTJ(tjNumber);
                    tj.ApplyTheme();
                    bool isOn = tj.IsOn;
                    foreach (ConnectorType type in Enum.GetValues<ConnectorType>())
                    {
                        CablePanel panel = mapManager.GetPanel(tjNumber, type);
                        panel.ApplyTheme();
                        panel.SetActive(isOn);
                    }
                }

                mapManager.RefreshView();
            }

            // 테마 전환 전에 생성된 목록 항목과 배치 항목도 새 카테고리 색으로 갱신한다.
            ApplyThemeToCableItems(TlpBg);

            TlpBg.Invalidate(true);
            FlpSupplies.Invalidate(true);
            PnlMap.Invalidate(true);
        }

        private void UpdateProjectButtonIcons()
        {
            ReplaceButtonImage(btnNewProject, CreateNewProjectIcon(AppTheme.Accent));
            ReplaceButtonImage(btnOpenProject, CreateOpenProjectIcon(AppTheme.Accent));
            ReplaceButtonImage(btnSaveProject, CreateSaveProjectIcon(AppTheme.Accent));
        }

        private static void ReplaceButtonImage(Button button, Image image)
        {
            Image? previous = button.Image;
            button.Image = image;
            previous?.Dispose();
        }

        private static Bitmap CreateOpenProjectIcon(Color accent)
        {
            Bitmap bitmap = new Bitmap(28, 28);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new Pen(accent, 2F) { LineJoin = LineJoin.Round };
            using SolidBrush fill = new SolidBrush(Color.FromArgb(55, accent));
            Point[] folder =
            {
                new Point(3, 8), new Point(11, 8), new Point(13, 11),
                new Point(25, 11), new Point(22, 23), new Point(3, 23)
            };
            graphics.FillPolygon(fill, folder);
            graphics.DrawPolygon(pen, folder);
            graphics.DrawLine(pen, 6, 5, 14, 5);
            graphics.DrawLine(pen, 14, 5, 17, 8);
            return bitmap;
        }

        private static Bitmap CreateNewProjectIcon(Color accent)
        {
            Bitmap bitmap = new Bitmap(28, 28);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new Pen(accent, 2F) { LineJoin = LineJoin.Round };
            using SolidBrush fill = new SolidBrush(Color.FromArgb(55, accent));
            Point[] page =
            {
                new Point(5, 3), new Point(17, 3), new Point(23, 9),
                new Point(23, 25), new Point(5, 25)
            };
            graphics.FillPolygon(fill, page);
            graphics.DrawPolygon(pen, page);
            graphics.DrawLine(pen, 17, 3, 17, 9);
            graphics.DrawLine(pen, 17, 9, 23, 9);
            graphics.DrawLine(pen, 9, 17, 19, 17);
            graphics.DrawLine(pen, 14, 12, 14, 22);
            return bitmap;
        }

        private static Bitmap CreateSaveProjectIcon(Color accent)
        {
            Bitmap bitmap = new Bitmap(28, 28);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new Pen(accent, 2F) { LineJoin = LineJoin.Round };
            using SolidBrush fill = new SolidBrush(Color.FromArgb(55, accent));
            Rectangle body = new Rectangle(4, 3, 20, 22);
            graphics.FillRectangle(fill, body);
            graphics.DrawRectangle(pen, body);
            graphics.DrawRectangle(pen, 8, 4, 11, 7);
            graphics.DrawRectangle(pen, 8, 16, 12, 9);
            return bitmap;
        }

        private void ApplyThemeButtonImages()
        {
            ButtonHelper.ApplyButtonEffect(
                btnBack,
                AppTheme.GetImage("back", "back_defense"),
                AppTheme.GetImage("back_push", "back_defense_push"));
            ButtonHelper.ApplyButtonEffect(
                btnForward,
                AppTheme.GetImage("forward", "forward_defense"),
                AppTheme.GetImage("forward_push", "forward_defense_push"));
            ButtonHelper.ApplyButtonEffect(
                btnReset,
                AppTheme.GetImage("reset", "reset_defense"),
                AppTheme.GetImage("reset_push", "reset_defense_push"));
            ButtonHelper.ApplyButtonEffect(
                btnSupAdd,
                AppTheme.GetSupplyAddImage(false),
                AppTheme.GetSupplyAddImage(true));

            foreach (Button button in new[] { btnCancel, btnCreate, btnDone })
            {
                ButtonHelper.ApplyButtonEffect(
                    button,
                    AppTheme.GetStandardButtonImage(false),
                    AppTheme.GetStandardButtonImage(true));
            }

            UpdateHistoryButtons();
        }

        private static void UpdateThemeButtonState(Button button, bool isSelected)
        {
            button.Text = string.Empty;
            button.FlatAppearance.BorderSize = isSelected ? 3 : 1;
            button.FlatAppearance.BorderColor = isSelected
                ? Color.White
                : Color.FromArgb(100, 110, 115);
            button.Invalidate();
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

        private void btnNewProject_Click(object? sender, EventArgs e)
        {
            if (!ConfirmSaveBeforeReplacingProject())
                return;

            isLoadingProject = true;
            try
            {
                mapManager.StartNewProject();
            }
            finally
            {
                isLoadingProject = false;
            }

            currentProjectPath = null;
            hasUnsavedChanges = false;
            UpdateWindowTitle();
            RefreshMapAfterSupplyChange();
        }

        private bool ConfirmSaveBeforeReplacingProject()
        {
            if (!hasUnsavedChanges)
                return true;

            DialogResult result = ProjectMessageBox.Show(
                "현재 프로젝트의 변경 사항을 저장하시겠습니까?",
                "변경 사항 저장",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);
            return result switch
            {
                DialogResult.Yes => SaveProject(false),
                DialogResult.No => true,
                _ => false
            };
        }

        private void btnOpenProject_Click(object? sender, EventArgs e)
        {
            if (!ConfirmSaveBeforeReplacingProject())
                return;

            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = ProjectFileService.DialogFilter,
                DefaultExt = ProjectFileService.Extension.TrimStart('.'),
                CheckFileExists = true,
                InitialDirectory = DialogDirectoryStore.GetProjectDirectory(),
                Multiselect = false,
                RestoreDirectory = true,
                Title = "시험 연결 프로젝트 열기"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                ProjectFileData data = ProjectFileService.Load(dialog.FileName);
                isLoadingProject = true;
                try
                {
                    mapManager.ImportProjectData(data);
                    SelectTheme(data.UseDefenseTheme);
                }
                finally
                {
                    isLoadingProject = false;
                }
                currentProjectPath = dialog.FileName;
                DialogDirectoryStore.RememberProjectPath(dialog.FileName);
                hasUnsavedChanges = false;
                UpdateWindowTitle();
                RefreshMapAfterSupplyChange();
            }
            catch (Exception exception)
            {
                ErrorLogger.Write(exception, "ProjectOpen");
                ProjectMessageBox.Show(
                    $"프로젝트 파일을 열 수 없습니다.\n{exception.Message}",
                    "열기 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnSaveProject_Click(object? sender, EventArgs e)
        {
            SaveProject(true);
        }

        private bool SaveProject(bool showCompletionMessage)
        {
            string? path = currentProjectPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                using SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = ProjectFileService.DialogFilter,
                    DefaultExt = ProjectFileService.Extension.TrimStart('.'),
                    AddExtension = true,
                    FileName = GetDefaultProjectFileName(),
                    InitialDirectory = DialogDirectoryStore.GetProjectDirectory(),
                    OverwritePrompt = true,
                    RestoreDirectory = true,
                    Title = "시험 연결 프로젝트 저장"
                };
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return false;
                path = dialog.FileName;
            }

            try
            {
                ProjectFileData data = mapManager.ExportProjectData(AppTheme.IsDefense);
                ProjectFileService.Save(path, data);
                currentProjectPath = path;
                DialogDirectoryStore.RememberProjectPath(path);
                hasUnsavedChanges = false;
                UpdateWindowTitle();
                if (showCompletionMessage)
                {
                    ProjectMessageBox.Show(
                        "프로젝트를 저장했습니다.",
                        "저장 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                return true;
            }
            catch (Exception exception)
            {
                ErrorLogger.Write(exception, "ProjectSave");
                ProjectMessageBox.Show(
                    $"프로젝트를 저장할 수 없습니다.\n{exception.Message}",
                    "저장 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetDefaultProjectFileName()
        {
            string fileName = cableManager.Cables
                .FirstOrDefault(cable => cable.Category == "시험 대상 케이블")?.Name
                ?? $"시험연결프로젝트_{DateTime.Now:yyyyMMdd}";

            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidCharacter, '_');

            fileName = fileName.Trim().TrimEnd('.');
            return string.IsNullOrWhiteSpace(fileName)
                ? $"시험연결프로젝트_{DateTime.Now:yyyyMMdd}"
                : fileName;
        }

        private void MarkProjectChanged()
        {
            if (isLoadingProject)
                return;

            hasUnsavedChanges = true;
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            string projectName = string.IsNullOrWhiteSpace(currentProjectPath)
                ? string.Empty
                : $" - {Path.GetFileNameWithoutExtension(currentProjectPath)}";
            Text = $"Test Cable Connection Manager{projectName}{(hasUnsavedChanges ? " *" : string.Empty)}";
        }

        private void btnBack_Click(object? sender, EventArgs e)
        {
            mapManager.Undo();
        }

        /// <summary>메뉴가 없는 메인 화면에서도 표준 편집·파일 단축키를 전역 처리한다.</summary>
        protected override bool ProcessCmdKey(ref Message message, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    btnNewProject.PerformClick();
                    return true;
                case Keys.Control | Keys.O:
                    btnOpenProject.PerformClick();
                    return true;
                case Keys.Control | Keys.S:
                    btnSaveProject.PerformClick();
                    return true;
                case Keys.Control | Keys.Z:
                    if (mapManager.CanUndo)
                        mapManager.Undo();
                    return true;
                case Keys.Control | Keys.Shift | Keys.Z:
                case Keys.Control | Keys.Y:
                    if (mapManager.CanRedo)
                        mapManager.Redo();
                    return true;
                case Keys.Control | Keys.P:
                    btnDone.PerformClick();
                    return true;
                default:
                    return base.ProcessCmdKey(ref message, keyData);
            }
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
            using PdfPreviewForm previewForm = new PdfPreviewForm(
                title,
                pages,
                SaveProjectBeforeExport);
            previewForm.ShowDialog(this);
        }

        private bool SaveProjectBeforeExport()
        {
            if (!hasUnsavedChanges && !string.IsNullOrWhiteSpace(currentProjectPath))
                return true;

            return SaveProject(false);
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

            if (hasUnsavedChanges)
            {
                DialogResult saveResult = ProjectMessageBox.Show(
                    "변경된 연결도 프로젝트를 저장하시겠습니까?",
                    "변경 사항 저장",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                if (saveResult == DialogResult.Cancel ||
                    saveResult == DialogResult.Yes && !SaveProject(false))
                {
                    e.Cancel = true;
                }
            }
            else
            {
                DialogResult closeResult = ProjectMessageBox.Show(
                    "프로그램을 종료하고 창을 닫으시겠습니까?",
                    "프로그램 종료",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (closeResult != DialogResult.Yes)
                    e.Cancel = true;
            }

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
            MarkProjectChanged();
        }

        private void UpdateHistoryButtons()
        {
            // 이동할 이력이 없는 버튼은 비활성화하여 현재 상태를 명확히 보여준다.
            btnBack.Enabled = mapManager.CanUndo;
            btnForward.Enabled = mapManager.CanRedo;
            btnReset.Enabled = mapManager.CanReset || cableManager.Cables.Count > 0;

            // 활성 상태에 맞는 전용 리소스를 버튼에 직접 지정한다.
            btnBack.BackgroundImage = btnBack.Enabled
                ? AppTheme.GetImage("back", "back_defense")
                : AppTheme.GetImage("back_off", "back_defense_off");
            btnForward.BackgroundImage = btnForward.Enabled
                ? AppTheme.GetImage("forward", "forward_defense")
                : AppTheme.GetImage("forward_off", "forward_defense_off");
            btnReset.BackgroundImage = btnReset.Enabled
                ? AppTheme.GetImage("reset", "reset_defense")
                : AppTheme.GetImage("reset_off", "reset_defense_off");
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
                    VerticalFlowPanel itemPanel = GetPanel(info.Category);

                    TlpBg.SuspendLayout();
                    TlpTestSupplies.SuspendLayout();
                    TlpTestSetting.SuspendLayout();
                    PnlMap.SuspendLayout();
                    TlpMap.SuspendLayout();
                    FlpSupplies.SuspendLayout();
                    itemPanel.SuspendLayout();
                    try
                    {
                        cableManager.Add(info);

                        CableCard card = new CableCard(info);
                        card.DeleteRequested += DeleteCable;
                        FlpSupplies.Controls.Add(card);
                        CreateItems(info);
                    }
                    finally
                    {
                        itemPanel.ResumeLayout(true);
                        FlpSupplies.ResumeLayout(true);
                        TlpMap.ResumeLayout(true);
                        PnlMap.ResumeLayout(true);
                        TlpTestSetting.ResumeLayout(true);
                        TlpTestSupplies.ResumeLayout(true);
                        TlpBg.ResumeLayout(true);
                    }

                    mapManager.RecordExternalChange();
                    RefreshMapAfterSupplyChange();
                }
            }
        }

        private void RefreshMapAfterSupplyChange()
        {
            BeginInvoke(new Action(() =>
            {
                // 상단 카드와 좌측 아이템 추가로 발생한 중첩 레이아웃을 먼저 확정한다.
                TlpBg.PerformLayout();
                TlpTestSetting.PerformLayout();
                PnlMap.PerformLayout();
                TlpMap.PerformLayout();
                mapManager.RefreshView();

                // 예약된 스크롤 패널 배치가 끝난 다음 오버레이를 최종 상태로 즉시 표시한다.
                BeginInvoke(new Action(mapManager.ForceRefreshView));
            }));
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

        protected override void WndProc(ref Message message)
        {
            int systemCommand = (int)message.WParam & 0xFFF0;
            bool isMaximizeCommand = message.Msg == WmSysCommand &&
                systemCommand == ScMaximize;
            bool isRestoreFromMaximizedCommand = message.Msg == WmSysCommand &&
                systemCommand == ScRestore &&
                WindowState == FormWindowState.Maximized;
            // 최대화된 제목 표시줄을 아래로 끌어 복원하면 SC_RESTORE보다 먼저
            // WM_NCLBUTTONDOWN(HTCAPTION) 경로에서 창 크기 변경이 시작된다.
            bool isDragRestoreFromMaximized =
                message.Msg == WmNcLButtonDown &&
                message.WParam.ToInt32() == HtCaption &&
                WindowState == FormWindowState.Maximized;
            bool isWindowStateCommand =
                isMaximizeCommand ||
                isRestoreFromMaximizedCommand ||
                isDragRestoreFromMaximized;

            if (!isWindowStateCommand)
            {
                base.WndProc(ref message);
                return;
            }

            int transitionVersion = ++windowStateTransitionVersion;
            SetRedrawForControlTree(this, false);
            mapManager?.BeginViewportResize();

            try
            {
                base.WndProc(ref message);

                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke(new Action(() => CompleteWindowStateTransition(transitionVersion)));
            }
            catch
            {
                EnableWindowRedraw();
                throw;
            }
        }

        private void CompleteWindowStateTransition(int transitionVersion)
        {
            if (transitionVersion != windowStateTransitionVersion || IsDisposed)
                return;

            try
            {
                ApplyResponsiveLayout();
                PerformLayout();
                TlpBg.PerformLayout();
                TlpTestSupplies.PerformLayout();
                TlpTestSetting.PerformLayout();
                TlpSettingBody.PerformLayout();
                TlpMapBody.PerformLayout();
                PnlMap.PerformLayout();
                TlpMap.PerformLayout();
                mapManager?.EndViewportResize();
                mapManager?.ForceRefreshView();
            }
            finally
            {
                EnableWindowRedraw();
            }
        }

        /// <summary>
        /// 잠가 둔 창과 모든 자식 컨트롤의 그리기를 다시 활성화하고 완성된 화면을 한 번에 표시한다.
        /// </summary>
        private void EnableWindowRedraw()
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            SetRedrawForControlTree(this, true);
            RedrawWindow(
                Handle,
                IntPtr.Zero,
                IntPtr.Zero,
                RdwInvalidate | RdwErase | RdwAllChildren | RdwUpdateNow);
        }

        /// <summary>
        /// 창 상태가 바뀌는 동안 자식 컨트롤이 각자 중간 배치 상태를 그리지 않도록
        /// 전체 컨트롤 트리의 WM_SETREDRAW 상태를 일괄 변경한다.
        /// </summary>
        private static void SetRedrawForControlTree(Control root, bool enabled)
        {
            if (root.IsDisposed)
                return;

            // 다시 그리기를 켤 때는 자식부터 활성화해야 부모의 최종 합성 시 모두 포함된다.
            if (enabled)
            {
                foreach (Control child in root.Controls)
                    SetRedrawForControlTree(child, true);

                if (root.IsHandleCreated)
                    SendMessage(root.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                return;
            }

            // 잠글 때는 부모를 먼저 막아 이후 자식 상태 변경이 화면에 노출되지 않게 한다.
            if (root.IsHandleCreated)
                SendMessage(root.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);

            foreach (Control child in root.Controls)
                SetRedrawForControlTree(child, false);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            mapManager?.EndViewportResize();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState != FormWindowState.Minimized)
                ApplyResponsiveLayout();

            if (mapManager == null)
                return;

            if (WindowState == FormWindowState.Minimized)
            {
                if (!wasMinimized)
                {
                    wasMinimized = true;
                    mapManager.BeginViewportResize();
                }
                return;
            }

            if (!wasMinimized || !IsHandleCreated)
                return;

            wasMinimized = false;
            BeginInvoke(new Action(() =>
            {
                // 복원 직후의 0px 임시 크기가 사라진 뒤 정상 뷰포트로 다시 계산한다.
                mapManager.EndViewportResize();
                mapManager.ForceRefreshView();
            }));
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
