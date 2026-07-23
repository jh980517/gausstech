using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Models;

namespace PinConnectionDiagram.Managers
{
    /// <summary>
    /// TJ별 케이블 패널, 커넥터 연결, 연결선 및 실행 취소 이력을 통합 관리한다.
    /// </summary>
    public class MapManager
    {
        private const int MinimumMapWidth = 1050;
        // 화면 컨트롤 및 현재 연결 상태
        private readonly TableLayoutPanel tlpMap;
        private readonly Func<IReadOnlyList<CableInfo>> getSupplies;
        private readonly Dictionary<int, TJControl> tjControls = new();
        private readonly Dictionary<(int, ConnectorType), CablePanel> cablePanels = new();
        private readonly List<ConnectionLine> connectionLines = new();
        private readonly ConnectionOverlay connectionOverlay;
        private readonly System.Windows.Forms.Timer settledOverlayRefreshTimer;
        private readonly List<MapSnapshot> history = new();
        private MapSnapshot? initialSnapshot;
        // historyIndex는 현재 화면에 적용된 스냅샷 위치를 가리킨다.
        private int historyIndex = -1;
        private bool isRestoringHistory;
        private bool isOverlayRefreshQueued;
        private bool isOverlayRefreshPending;
        private bool isViewportResizing;
        private bool isSynchronizingMapWidth;
        private Connector? selectedConnector;

        public bool CanUndo => historyIndex > 0;
        public bool CanRedo => historyIndex >= 0 && historyIndex < history.Count - 1;
        public bool HasConfiguredData => connectionLines.Count > 0 ||
            tjControls.Values.Any(tj => tj.IsOn);
        public bool CanReset => connectionLines.Count > 0 ||
            Enumerable.Range(1, 5).Any(tjNumber =>
                GetTJ(tjNumber).IsOn ||
                Enum.GetValues<ConnectorType>().Any(type =>
                    GetPanel(tjNumber, type).LeftConnectors.Count > 0 ||
                    GetPanel(tjNumber, type).RightConnectors.Count > 0));
        public event Action? HistoryChanged;
        public event Action<IReadOnlyList<CableInfo>>? SuppliesRestoreRequested;
        public MapManager(
            TableLayoutPanel table,
            Func<IReadOnlyList<CableInfo>>? getSupplies = null)
        {
            tlpMap = table;
            this.getSupplies = getSupplies ?? (() => Array.Empty<CableInfo>());

            connectionOverlay = new ConnectionOverlay(() => connectionLines);
            connectionOverlay.ConnectionDeleteRequested += DeleteConnection;

            // 중첩 레이아웃이 모두 끝난 시점에 오버레이를 한 번 더 검증한다.
            // 즉시 갱신 때 임시 좌표가 들어와 선과 DropZone이 사라지는 경합을 방지한다.
            settledOverlayRefreshTimer = new System.Windows.Forms.Timer
            {
                Interval = 60
            };
            settledOverlayRefreshTimer.Tick += (_, _) =>
            {
                settledOverlayRefreshTimer.Stop();
                RefreshSettledConnectionOverlay();
            };
            connectionOverlay.CableAssignmentChanged += () =>
            {
                RecordHistory();
                QueueConnectionOverlayRefresh();
            };
            tlpMap.Parent?.Controls.Add(connectionOverlay);
            SyncConnectionOverlayBounds();
            connectionOverlay.BringToFront();

            tlpMap.LocationChanged += (_, _) => QueueConnectionOverlayRefresh();
            tlpMap.SizeChanged += (_, _) => QueueConnectionOverlayRefresh();
            tlpMap.Layout += (_, _) => QueueConnectionOverlayRefresh();

            if (tlpMap.Parent is ScrollableControl scrollParent)
            {
                scrollParent.SizeChanged += (_, _) =>
                {
                    SyncMapWidthToViewport();
                    QueueConnectionOverlayRefresh();
                };
                scrollParent.Scroll += (_, _) => SyncConnectionOverlayBounds();
                scrollParent.Layout += (_, _) =>
                {
                    SyncMapWidthToViewport();
                    QueueConnectionOverlayRefresh();
                };
            }

            SyncMapWidthToViewport();
        }

        /// <summary>
        /// 스크롤 영역의 과거 가상 폭과 무관하게 연결도를 현재 보이는 영역 폭에 맞춘다.
        /// </summary>
        private void SyncMapWidthToViewport()
        {
            if (isSynchronizingMapWidth || tlpMap.Parent is not ScrollableControl scrollParent)
                return;

            if (IsViewportUnavailable(scrollParent))
            {
                return;
            }

            int viewportWidth = Math.Max(
                MinimumMapWidth,
                scrollParent.ClientSize.Width - scrollParent.Padding.Horizontal);
            if (tlpMap.Width == viewportWidth)
                return;

            try
            {
                isSynchronizingMapWidth = true;
                tlpMap.Width = viewportWidth;
            }
            finally
            {
                isSynchronizingMapWidth = false;
            }
        }

        private void SyncConnectionOverlayBounds()
        {
            if (isViewportResizing ||
                tlpMap.Parent is ScrollableControl scrollParent && IsViewportUnavailable(scrollParent))
            {
                return;
            }

            // 스크롤 또는 행 높이 변경 후에도 선 좌표계가 TlpMap과 일치해야 한다.
            SyncMapWidthToViewport();
            HideHorizontalScrollBar();
            connectionOverlay.Bounds = tlpMap.Bounds;
            connectionOverlay.BringToFront();
            connectionOverlay.RefreshConnections();
        }

        private static bool IsViewportUnavailable(ScrollableControl viewport)
        {
            Form? form = viewport.FindForm();
            return form?.WindowState == FormWindowState.Minimized ||
                viewport.ClientSize.Width <= viewport.Padding.Horizontal ||
                viewport.ClientSize.Height <= viewport.Padding.Vertical;
        }

        public void RefreshView()
        {
            // 준비물 추가로 중첩 레이아웃이 진행 중이어도 현재 상태를 먼저 표시한다.
            SyncConnectionOverlayBounds();
            QueueConnectionOverlayRefresh();
        }

        /// <summary>
        /// 지연된 자식 컨트롤 페인트가 끝난 뒤 연결 오버레이를 즉시 다시 표시한다.
        /// </summary>
        public void ForceRefreshView()
        {
            if (isViewportResizing || connectionOverlay.IsDisposed)
            {
                return;
            }

            SyncConnectionOverlayBounds();
            connectionOverlay.Visible = true;
            connectionOverlay.BringToFront();
            connectionOverlay.Invalidate(true);
            connectionOverlay.Refresh();
        }

        public void BeginViewportResize()
        {
            if (connectionOverlay.IsDisposed)
                return;

            // 사용자가 창 테두리를 드래그하는 동안에는 비용이 큰 Region 재생성을 중단한다.
            isViewportResizing = true;
            connectionOverlay.Visible = false;
        }

        public void EndViewportResize()
        {
            isViewportResizing = false;
            if (connectionOverlay.IsDisposed)
                return;

            if (isOverlayRefreshPending)
                isOverlayRefreshPending = false;
            SyncConnectionOverlayBounds();
            connectionOverlay.Visible = true;
            // 크기 전환 중 누락된 요청이 있든 없든 최종 좌표를 한 번 검증한다.
            QueueConnectionOverlayRefresh();
        }

        private void QueueConnectionOverlayRefresh()
        {
            if (isViewportResizing ||
                tlpMap.Parent is ScrollableControl viewport && IsViewportUnavailable(viewport))
            {
                // 최소화·크기 전환 중 요청을 버리지 않고 정상 뷰포트가 돌아왔을 때 처리한다.
                isOverlayRefreshPending = true;
                return;
            }

            isOverlayRefreshPending = false;
            // 연속 Layout/SizeChanged가 발생하면 마지막 변경 후 60ms 시점으로 미룬다.
            settledOverlayRefreshTimer.Stop();
            settledOverlayRefreshTimer.Start();

            if (isOverlayRefreshQueued || connectionOverlay.IsDisposed)
                return;

            isOverlayRefreshQueued = true;
            Control invokeControl = connectionOverlay.Parent ?? connectionOverlay;
            if (!invokeControl.IsHandleCreated)
            {
                isOverlayRefreshQueued = false;
                return;
            }

            // 모든 하위 컨트롤의 위치 계산이 끝난 다음 선의 Region과 좌표를 다시 만든다.
            invokeControl.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (isViewportResizing || connectionOverlay.IsDisposed)
                        return;

                    SyncConnectionOverlayBounds();
                    connectionOverlay.Visible = true;
                    connectionOverlay.BringToFront();
                    connectionOverlay.Refresh();
                }
                finally
                {
                    isOverlayRefreshQueued = false;
                }
            }));
        }

        /// <summary>
        /// 마지막 레이아웃 이벤트 이후 확정된 좌표로 선, 표시 영역, DropZone을 다시 만든다.
        /// </summary>
        private void RefreshSettledConnectionOverlay()
        {
            if (isViewportResizing || connectionOverlay.IsDisposed ||
                tlpMap.Parent is ScrollableControl viewport && IsViewportUnavailable(viewport))
            {
                isOverlayRefreshPending = true;
                return;
            }

            SyncConnectionOverlayBounds();
            connectionOverlay.Visible = true;
            connectionOverlay.BringToFront();
            connectionOverlay.Invalidate(true);
            connectionOverlay.Update();
        }

        public Bitmap RenderConnectionDiagram()
        {
            Dictionary<CablePanel, bool> addButtonVisibility = cablePanels.Values
                .Distinct()
                .ToDictionary(panel => panel, panel => panel.AddButtonVisible);
            foreach (CablePanel panel in addButtonVisibility.Keys)
                panel.AddButtonVisible = false;

            try
            {
            // 마지막으로 켜진 TJ까지만 포함하여 출력물의 불필요한 OFF 행을 제거한다.
            int lastEnabledTJ = tjControls.Values
                .Where(tj => tj.IsOn)
                .Select(tj => tj.TJNumber)
                .DefaultIfEmpty(0)
                .Max();
            int outputHeight = tlpMap.RowStyles
                .Cast<RowStyle>()
                .Take(lastEnabledTJ + 1)
                .Sum(row => (int)Math.Ceiling(row.Height));
            outputHeight = Math.Clamp(outputHeight, 1, tlpMap.Height);

            // WinForms 컨트롤은 실제 화면 좌표로 캡처해야 자식 배치와 연결선 좌표가 어긋나지 않는다.
            using Bitmap fullBitmap = new Bitmap(tlpMap.Width, tlpMap.Height);
            tlpMap.DrawToBitmap(
                fullBitmap,
                new Rectangle(Point.Empty, tlpMap.Size));
            using Graphics graphics = Graphics.FromImage(fullBitmap);
            connectionOverlay.DrawConnections(graphics, true);

            return fullBitmap.Clone(
                new Rectangle(0, 0, fullBitmap.Width, outputHeight),
                fullBitmap.PixelFormat);
            }
            finally
            {
                // 출력 캡처가 끝나면 화면의 추가 버튼 표시 상태를 정확히 복원한다.
                foreach ((CablePanel panel, bool wasVisible) in addButtonVisibility)
                    panel.AddButtonVisible = wasVisible;
            }
        }

        public void RemoveCableAssignments(CableInfo cableInfo)
        {
            // 현재 화면의 배정만 해제하고 과거 스냅샷은 보존해 Undo 시 복원할 수 있게 한다.
            foreach (ConnectionLine line in connectionLines.Where(line =>
                line.CableInfo?.Id == cableInfo.Id))
            {
                line.CableInfo = null;
            }

            connectionOverlay.RefreshConnections();
            HistoryChanged?.Invoke();
        }

        private void HideHorizontalScrollBar()
        {
            if (tlpMap.Parent is not ScrollableControl scrollParent)
                return;

            int verticalPosition = Math.Abs(scrollParent.AutoScrollPosition.Y);
            scrollParent.AutoScrollPosition = new Point(0, verticalPosition);
            scrollParent.HorizontalScroll.Maximum = 0;
            scrollParent.HorizontalScroll.Enabled = false;
            scrollParent.HorizontalScroll.Visible = false;
        }

        public void RegisterCablePanel(int tjNumber, ConnectorType type, CablePanel panel)
        {
            cablePanels[(tjNumber, type)] = panel;

            panel.ConnectorPointClicked -= Panel_ConnectorPointClicked;
            panel.ConnectorPointClicked += Panel_ConnectorPointClicked;
            panel.ConnectorNameChanged -= Panel_ConnectorNameChanged;
            panel.ConnectorNameChanged += Panel_ConnectorNameChanged;


        }
        public CablePanel GetPanel(int tj, ConnectorType type)
        {
            return cablePanels[(tj, type)];
        }

        public void AddJigRightConnector(int tj)
        {
            CablePanel jig = GetPanel(tj, ConnectorType.Jig);

            jig.AddConnector(ConnectorSide.Right);

            SyncAdapterLeft(tj);
            UpdateRowHeight(tj);
            RecordHistory();
        }

        public void AddAdapterRightConnector(int tj)
        {
            CablePanel adapter = GetPanel(tj, ConnectorType.Adapter);

            adapter.AddConnector(ConnectorSide.Right);

            SyncTestLeft(tj);
            UpdateRowHeight(tj);
            RecordHistory();
        }

        // 지그 케이블 커넥터와 어댑터 케이블 커넥터 갯수 맞추기
        private void SyncAdapterLeft(int tj)
        {
            CablePanel jig = GetPanel(tj, ConnectorType.Jig);
            CablePanel adapter = GetPanel(tj, ConnectorType.Adapter);

            while (adapter.LeftConnectors.Count < jig.RightConnectors.Count)
            {
                // Jig 출력 하나당 대응하는 Adapter 입력 하나를 자동 생성한다.
                adapter.AddConnector(ConnectorSide.Left);
            }

            while (adapter.LeftConnectors.Count > jig.RightConnectors.Count)
            {
                RemoveConnector(adapter, adapter.LeftConnectors.Last());
            }
        }

        // 어댑터 케이블 커넥터와 시험 대상 케이블 커넥터 갯수 맞추기
        private void SyncTestLeft(int tj)
        {
            CablePanel adapter = GetPanel(tj, ConnectorType.Adapter);
            CablePanel test = GetPanel(tj, ConnectorType.Test);

            while (test.LeftConnectors.Count < adapter.RightConnectors.Count)
            {
                // Adapter 출력 하나당 대응하는 Test 입력 하나를 자동 생성한다.
                test.AddConnector(ConnectorSide.Left);
            }

            while (test.LeftConnectors.Count > adapter.RightConnectors.Count)
            {
                RemoveConnector(test, test.LeftConnectors.Last());
            }

            RebuildAutomaticTestConnections();
        }

        /// <summary>
        /// 시험 대상 케이블의 모든 활성 커넥터를 하나의 공통 연결 그룹으로 자동 구성한다.
        /// 선은 첫 커넥터를 중심으로 한 최소 개수의 분기만 저장하지만, 연결 판정상 모두 연결된다.
        /// </summary>
        private void RebuildAutomaticTestConnections()
        {
            CableInfo? assignedCable = connectionLines
                .Where(line =>
                    line.StartConnector.ConnectorType == ConnectorType.Test &&
                    line.EndConnector.ConnectorType == ConnectorType.Test)
                .Select(line => line.CableInfo)
                .FirstOrDefault(cable => cable != null);

            connectionLines.RemoveAll(line =>
                line.StartConnector.ConnectorType == ConnectorType.Test &&
                line.EndConnector.ConnectorType == ConnectorType.Test);

            List<Connector> testConnectors = tjControls.Values
                .Where(tj => tj.IsOn)
                .OrderBy(tj => tj.TJNumber)
                .SelectMany(tj => GetPanel(tj.TJNumber, ConnectorType.Test).LeftConnectors)
                .ToList();
            if (testConnectors.Count > 1)
            {
                Connector hub = testConnectors[0];
                foreach (Connector connector in testConnectors.Skip(1))
                {
                    connectionLines.Add(new ConnectionLine
                    {
                        StartConnector = hub,
                        EndConnector = connector,
                        CableInfo = assignedCable
                    });
                }
            }

            RefreshConnectedStates();
            connectionOverlay.RefreshConnections();
        }

        public void Create()
        {
            CreateRows();

            InitializeDefaultConnector();
            initialSnapshot = CaptureSnapshot();
            RecordHistory();
        }

        /// <summary>프로그램 최초 실행 상태로 복원하고 Undo/Redo 이력을 새로 시작한다.</summary>
        public void StartNewProject()
        {
            if (initialSnapshot == null)
                throw new InvalidOperationException("초기 프로젝트 상태가 준비되지 않았습니다.");

            RestoreSnapshot(initialSnapshot);
            history.Clear();
            history.Add(CaptureSnapshot());
            historyIndex = 0;
            HistoryChanged?.Invoke();
            QueueConnectionOverlayRefresh();
        }

        private void InitializeDefaultConnector()
        {
            for (int tj = 1; tj <= 5; tj++)
            {
                CablePanel jig = GetPanel(tj, ConnectorType.Jig);

                if (jig.LeftConnectors.Count == 0)
                {
                    jig.AddConnector(ConnectorSide.Left, "P1");
                }
            }
        }

        private void CreateRows()
        {
            for (int row = 1; row <= 5; row++)
            {
                CreateTJ(row);
                CreatePanel(row, ConnectorType.Jig);
                CreatePanel(row, ConnectorType.Adapter);
                CreatePanel(row, ConnectorType.Test);
            }
        }

        private void CreateTJ(int row)
        {
            TJControl tj = new TJControl(row);
            tj.Dock = DockStyle.None;
            tj.Anchor = AnchorStyles.None;
            tj.Margin = new Padding(50, 30, 0, 30);
            tlpMap.Controls.Add(tj, 0, row);
            tjControls.Add(row, tj);
            tj.StateChanged += TJ_StateChanged;
        }

        private void CreatePanel(int row, ConnectorType type)
        {
            CablePanel panel = new CablePanel(row, type);

            panel.AddRightConnectorRequested += Panel_AddRightConnectorRequested;
            panel.ConnectorDeleteRequested += Panel_ConnectorDeleteRequested;

            panel.Dock = DockStyle.Fill;
            panel.Margin = new Padding(3, 0, 3, 0);

            int column = type switch
            {
                ConnectorType.Jig => 1,
                ConnectorType.Adapter => 2,
                ConnectorType.Test => 3,
                _ => 1
            };

            tlpMap.Controls.Add(panel, column, row);
            //cablePanels.Add((row, type), panel);
            RegisterCablePanel(row, type, panel);

            if (type == ConnectorType.Jig)
            {
                panel.AddConnector(ConnectorSide.Left);
            }

            if (type is ConnectorType.Jig or ConnectorType.Test)
            {
                panel.ShowAddButton = false;
            }
        }

        public TJControl GetTJ(int tj)
        {
            return tjControls[tj];
        }

        private void TJ_StateChanged(TJControl tj, bool isOn)
        {
            // OFF는 설정 손실을 동반하므로 사용자 확인 후에만 초기화한다.
            if (!isOn)
            {
                DialogResult result = PinConnectionDiagram.Helpers.ProjectMessageBox.Show(
                    $"TJ{tj.TJNumber}을 끄면 해당 TJ의 커넥터 및 연결 설정이 " +
                    "모두 초기화됩니다.\n계속하시겠습니까?",
                    "TJ 설정 초기화",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    tj.IsOn = true;
                    return;
                }

                ResetTJ(tj.TJNumber);
            }
            else
            {
                EnsureDefaultJigConnection(tj.TJNumber);
            }

            GetPanel(tj.TJNumber, ConnectorType.Jig).SetActive(isOn);

            GetPanel(tj.TJNumber, ConnectorType.Adapter).SetActive(isOn);

            GetPanel(tj.TJNumber, ConnectorType.Test).SetActive(isOn);
            RecordHistory();
        }

        private void Panel_ConnectorNameChanged(Connector connector)
        {
            RecordHistory();
        }

        private void ResetTJ(int tjNumber)
        {
            // 다른 TJ와 연결된 선도 해당 TJ 끝점을 포함하면 함께 제거한다.
            if (selectedConnector?.TJNumber == tjNumber)
            {
                ClearSelectedConnector();
            }

            connectionLines.RemoveAll(line =>
                line.StartConnector.TJNumber == tjNumber ||
                line.EndConnector.TJNumber == tjNumber);

            CablePanel jig = GetPanel(tjNumber, ConnectorType.Jig);
            CablePanel adapter = GetPanel(tjNumber, ConnectorType.Adapter);
            CablePanel test = GetPanel(tjNumber, ConnectorType.Test);

            jig.ClearConnector();
            adapter.ClearConnector();
            test.ClearConnector();

            jig.AddConnector(ConnectorSide.Left, "P1");

            RebuildAutomaticTestConnections();
            RefreshConnectedStates();
            UpdateRowHeight(tjNumber);
            connectionOverlay.RefreshConnections();
        }

        private void Panel_AddRightConnectorRequested(CablePanel panel)
        {
            switch (panel.PanelType)
            {
                case ConnectorType.Jig:
                    AddJigRightConnector(panel.TJNumber);
                    break;
                case ConnectorType.Adapter:
                    AddAdapterRightConnector(panel.TJNumber);
                    break;
            }
        }
        private void Panel_ConnectorDeleteRequested(CablePanel panel, Connector connector)
        {
            DialogResult result = PinConnectionDiagram.Helpers.ProjectMessageBox.Show(
                $"{connector.ConnectorName} 커넥터를 삭제하시겠습니까?",
                "커넥터 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            RemoveConnector(panel, connector);

            switch (panel.PanelType)
            {
                case ConnectorType.Jig:
                    SyncAdapterLeft(panel.TJNumber);
                    break;
                case ConnectorType.Adapter:
                    SyncTestLeft(panel.TJNumber);
                    break;
            }

            UpdateRowHeight(panel.TJNumber);
            RecordHistory();
        }

        private void RemoveConnector(CablePanel panel, Connector connector)
        {
            // 폐기될 컨트롤을 참조하는 연결이 남지 않도록 먼저 목록에서 제거한다.
            if (ReferenceEquals(selectedConnector, connector))
            {
                ClearSelectedConnector();
            }

            connectionLines.RemoveAll(line =>
                ReferenceEquals(line.StartConnector, connector) ||
                ReferenceEquals(line.EndConnector, connector));

            panel.RemoveConnector(connector);
            RefreshConnectedStates();
            connectionOverlay.RefreshConnections();
        }

        private void RefreshConnectedStates()
        {
            // 연결 개수를 별도 캐시하지 않고 실제 목록을 기준으로 색상을 재계산한다.
            foreach (CablePanel panel in cablePanels.Values)
            {
                foreach (Connector connector in
                    panel.LeftConnectors.Concat(panel.RightConnectors))
                {
                    bool isConnected = connectionLines.Any(line =>
                        ReferenceEquals(line.StartConnector, connector) ||
                        ReferenceEquals(line.EndConnector, connector));

                    connector.SetConnected(isConnected);
                }
            }
        }

        public void UpdateRowHeight(int tjNumber)
        {
            CablePanel jig = GetPanel(tjNumber, ConnectorType.Jig);
            CablePanel adapter = GetPanel(tjNumber, ConnectorType.Adapter);
            CablePanel test = GetPanel(tjNumber, ConnectorType.Test);

            int maxConnector =
                Math.Max(
                    jig.MaxConnectorCount,
                    Math.Max(
                        adapter.MaxConnectorCount,
                        test.MaxConnectorCount));

            maxConnector = Math.Max(maxConnector, 1);

            int rowHeight = 90 + (maxConnector - 1) * 45;

            tlpMap.RowStyles[tjNumber].Height = rowHeight;

            UpdateTableHeight();
        }

        // 커넥터 갯수에 따라 높이 조절
        private void UpdateTableHeight()
        {
            int totalHeight = 0;

            foreach (RowStyle row in tlpMap.RowStyles)
            {
                totalHeight += (int)row.Height;
            }

            tlpMap.Height = totalHeight;

            // 현재 폭을 최소 폭으로 저장하면 최대화 후 창을 복원할 때 지도가 줄어들지 않는다.
            // 세로 스크롤 계산에 필요한 높이만 고정하고 폭은 부모 패널에 맡겨 둔다.
            tlpMap.MinimumSize = new Size(0, totalHeight);
        }

        private void Panel_ConnectorPointClicked(Connector connector)
        {
            // 첫 클릭은 시작점 선택, 두 번째 클릭은 검증 후 연결 생성으로 처리한다.
            if (selectedConnector == null)
            {
                selectedConnector = connector;
                selectedConnector.SetConnectionPending(true);

                return;
            }

            if (ReferenceEquals(selectedConnector, connector))
            {
                ClearSelectedConnector();

                return;
            }

            if (IsDuplicateConnection(selectedConnector, connector))
            {
                PinConnectionDiagram.Helpers.ProjectMessageBox.Show(
                    "이미 서로 연결된 연결점입니다.",
                    "중복 연결",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ClearSelectedConnector();
                return;
            }

            if (!CanConnect(selectedConnector, connector))
            {
                PinConnectionDiagram.Helpers.ProjectMessageBox.Show(
                    "연결 할 수 없는 커넥터입니다.");

                ClearSelectedConnector();
                return;
            }

            ConnectionLine line = new()
            {
                StartConnector = selectedConnector,
                EndConnector = connector
            };

            connectionLines.Add(line);
            RefreshConnectedStates();
            connectionOverlay.RefreshConnections();
            RecordHistory();

            ClearSelectedConnector();
        }

        private bool IsDuplicateConnection(Connector first, Connector second)
        {
            // 시작 방향과 관계없이 동일한 두 연결점을 잇는 선이 이미 있으면 중복으로 판단한다.
            bool isDirectlyConnected = connectionLines.Any(line =>
                (ReferenceEquals(line.StartConnector, first) &&
                 ReferenceEquals(line.EndConnector, second)) ||
                (ReferenceEquals(line.StartConnector, second) &&
                 ReferenceEquals(line.EndConnector, first)));

            if (isDirectlyConnected)
                return true;

            // 시험 대상 케이블은 같은 연결 그룹에 속한 모든 점이 전기적으로 연결된 것으로 본다.
            // P1-P2, P3-P1이 있으면 별도의 P3-P2 선 없이도 이미 연결된 상태다.
            return first.ConnectorType == ConnectorType.Test &&
                   second.ConnectorType == ConnectorType.Test &&
                   AreInSameTestConnectionGroup(first, second);
        }

        private bool AreInSameTestConnectionGroup(Connector first, Connector second)
        {
            HashSet<Connector> visited = new HashSet<Connector> { first };
            Queue<Connector> pending = new Queue<Connector>();
            pending.Enqueue(first);

            while (pending.Count > 0)
            {
                Connector current = pending.Dequeue();
                foreach (ConnectionLine line in connectionLines.Where(line =>
                    line.StartConnector.ConnectorType == ConnectorType.Test &&
                    line.EndConnector.ConnectorType == ConnectorType.Test &&
                    (ReferenceEquals(line.StartConnector, current) ||
                     ReferenceEquals(line.EndConnector, current))))
                {
                    Connector adjacent = ReferenceEquals(line.StartConnector, current)
                        ? line.EndConnector
                        : line.StartConnector;
                    if (ReferenceEquals(adjacent, second))
                        return true;

                    if (visited.Add(adjacent))
                        pending.Enqueue(adjacent);
                }
            }

            return false;
        }

        private void ClearSelectedConnector()
        {
            selectedConnector?.SetConnectionPending(false);
            selectedConnector = null;
        }

        private bool CanConnect(Connector first, Connector second)
        {
            if (ReferenceEquals(first, second))
                return false;

            if (first.ConnectorType != second.ConnectorType)
                return false;

            if (first.ConnectorType == ConnectorType.Test)
            {
                // 시험 대상 케이블은 구조상 자신의 Left 연결점끼리 연결한다.
                return first.Side == ConnectorSide.Left &&
                       second.Side == ConnectorSide.Left;
            }

            return first.Side != second.Side;
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            historyIndex--;
            RestoreSnapshot(history[historyIndex]);
            HistoryChanged?.Invoke();
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            historyIndex++;
            RestoreSnapshot(history[historyIndex]);
            HistoryChanged?.Invoke();
        }

        private void DeleteConnection(ConnectionLine line)
        {
            // 연결 생성 클릭 순서와 관계없이 어댑터는 항상 P(Left) → J(Right) 순서로 안내한다.
            Connector firstConnector = line.StartConnector;
            Connector secondConnector = line.EndConnector;
            if (line.StartConnector.ConnectorType == ConnectorType.Adapter &&
                firstConnector.Side == ConnectorSide.Right)
            {
                (firstConnector, secondConnector) = (secondConnector, firstConnector);
            }

            DialogResult result = PinConnectionDiagram.Helpers.ProjectMessageBox.Show(
                $"{firstConnector.ConnectorName}과 {secondConnector.ConnectorName} 사이의 연결선을 삭제하시겠습니까?",
                "연결선 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            connectionLines.Remove(line);
            RefreshConnectedStates();
            connectionOverlay.RefreshConnections();
            RecordHistory();
        }

        /// <summary>
        /// TJ를 켜면 지그 P2와 대응 어댑터 Left를 만들고 지그 P1-P2 선을 즉시 연결한다.
        /// 이 작업은 TJ ON 이력 하나에 포함되도록 내부에서 별도 이력을 기록하지 않는다.
        /// </summary>
        private void EnsureDefaultJigConnection(int tjNumber)
        {
            CablePanel jig = GetPanel(tjNumber, ConnectorType.Jig);
            if (jig.LeftConnectors.Count == 0)
                jig.AddConnector(ConnectorSide.Left, "P1");
            if (jig.RightConnectors.Count == 0)
                jig.AddConnector(ConnectorSide.Right, "P2");

            Connector left = jig.LeftConnectors[0];
            Connector right = jig.RightConnectors[0];
            bool alreadyConnected = connectionLines.Any(line =>
                ReferenceEquals(line.StartConnector, left) &&
                ReferenceEquals(line.EndConnector, right) ||
                ReferenceEquals(line.StartConnector, right) &&
                ReferenceEquals(line.EndConnector, left));
            if (!alreadyConnected)
            {
                connectionLines.Add(new ConnectionLine
                {
                    StartConnector = left,
                    EndConnector = right
                });
            }

            SyncAdapterLeft(tjNumber);
            RefreshConnectedStates();
            UpdateRowHeight(tjNumber);
            connectionOverlay.RefreshConnections();
        }

        public bool TryBuildTestProcedure(
            out string title,
            out string procedure,
            out string errorMessage)
        {
            title = string.Empty;
            procedure = string.Empty;
            errorMessage = string.Empty;

            if (connectionLines.Count == 0)
            {
                errorMessage = "설명문을 생성하려면 먼저 커넥터 사이에 연결선을 만들어 주세요.";
                return false;
            }

            // 다른 영역의 선만 존재해도 전체 연결이 완료된 것으로 오인하지 않도록,
            // 켜진 TJ의 모든 커넥터가 각 케이블 영역의 연결망에 포함됐는지 검사한다.
            foreach (ConnectorType connectorType in new[]
            {
                ConnectorType.Jig,
                ConnectorType.Adapter,
                ConnectorType.Test
            })
            {
                List<Connector> requiredConnectors = tjControls.Values
                    .Where(tj => tj.IsOn)
                    .OrderBy(tj => tj.TJNumber)
                    .SelectMany(tj =>
                    {
                        CablePanel panel = GetPanel(tj.TJNumber, connectorType);
                        return panel.LeftConnectors.Concat(panel.RightConnectors);
                    })
                    .ToList();
                List<ConnectionLine> typeConnectionLines = connectionLines
                    .Where(line =>
                        line.StartConnector.ConnectorType == connectorType &&
                        line.EndConnector.ConnectorType == connectorType)
                    .ToList();
                string cableTypeName = GetCableTypeName(connectorType);

                if (requiredConnectors.Count > 0 && typeConnectionLines.Count == 0)
                {
                    errorMessage = $"{cableTypeName} 영역의 연결선이 없습니다.\n" +
                        $"{cableTypeName} 커넥터를 연결한 후 다시 완료해 주세요.";
                    return false;
                }

                List<Connector> unconnectedConnectors = requiredConnectors
                    .Where(connector => !typeConnectionLines.Any(line =>
                        ReferenceEquals(line.StartConnector, connector) ||
                        ReferenceEquals(line.EndConnector, connector)))
                    .ToList();
                if (unconnectedConnectors.Count > 0)
                {
                    string connectorNames = string.Join(", ",
                        unconnectedConnectors.Select(connector =>
                            $"TJ{connector.TJNumber} {connector.ConnectorName}"));
                    errorMessage = $"{cableTypeName} 영역에 연결되지 않은 커넥터가 있습니다.\n" +
                        $"{connectorNames}\n모든 커넥터를 연결한 후 다시 완료해 주세요.";
                    return false;
                }
            }

            if (connectionLines.Any(line => line.CableInfo == null))
            {
                errorMessage = "모든 케이블 연결 영역에 시험 준비물을 배치한 후 다시 생성해 주세요.";
                return false;
            }

            // 선을 공유 커넥터 기준으로 묶어 실제 하나의 분기 케이블 단위로 설명한다.
            Dictionary<ConnectorType, List<string>> sectionParagraphs =
                Enum.GetValues<ConnectorType>()
                    .ToDictionary(type => type, _ => new List<string>());
            List<ConnectionLine> assignedLines = connectionLines.ToList();
            List<List<ConnectionLine>> connectionGroups = BuildProcedureConnectionGroups(assignedLines);

            foreach (List<ConnectionLine> group in connectionGroups
                .OrderBy(group => GetTypeOrder(group[0].StartConnector.ConnectorType))
                .ThenBy(group => group.Min(line =>
                    Math.Min(line.StartConnector.TJNumber, line.EndConnector.TJNumber))))
            {
                CableInfo cable = group.First().CableInfo!;
                ConnectorType connectorType = group[0].StartConnector.ConnectorType;

                // 주제어장치에 직접 닿는 것은 지그 케이블의 Left 끝단뿐이다.
                // 어댑터와 시험 대상 케이블은 아래의 맞댐 연결 문장에서 설명한다.
                if (connectorType != ConnectorType.Jig)
                    continue;

                List<Connector> connectors = group
                    .SelectMany(line => new[] { line.StartConnector, line.EndConnector })
                    .Where(connector => connector.Side == ConnectorSide.Left)
                    .Distinct()
                    .OrderBy(connector => connector.TJNumber)
                    .ThenBy(connector => connector.ConnectorName)
                    .ToList();

                // 같은 P 번호가 연결된 TJ는 범위로 줄여 반복적인 커넥터 나열을 피한다.
                foreach (IGrouping<string, Connector> pinGroup in connectors
                    .GroupBy(connector => connector.ConnectorName)
                    .OrderBy(pinGroup => GetPinNumber(pinGroup.Key)))
                {
                    List<int> tjNumbers = pinGroup
                        .Select(connector => connector.TJNumber)
                        .Distinct()
                        .OrderBy(number => number)
                        .ToList();
                    string tjRange = FormatTJRanges(tjNumbers);
                    string eachText = tjNumbers.Count > 1 ? "각각 " : string.Empty;

                    sectionParagraphs[connectorType].Add(
                        $"- 주제어장치의 {tjRange}에 {GetCableTypeName(connectorType)} {cable.Name} “{pinGroup.Key}”을 {eachText}연결한다.");
                }
            }

            // 같은 TJ에 위치한 서로 다른 종류의 케이블 끝단을 실제 조립 순서대로 설명한다.
            for (int tjNumber = 1; tjNumber <= 5; tjNumber++)
            {
                AddJunctionInstructions(sectionParagraphs[ConnectorType.Adapter], assignedLines, tjNumber,
                    ConnectorType.Jig, ConnectorType.Adapter);
                AddJunctionInstructions(sectionParagraphs[ConnectorType.Test], assignedLines, tjNumber,
                    ConnectorType.Adapter, ConnectorType.Test);
            }

            string? testCableName = assignedLines
                .Where(line => line.StartConnector.ConnectorType == ConnectorType.Test)
                .Select(line => line.CableInfo?.Name)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));

            title = string.IsNullOrWhiteSpace(testCableName)
                ? "시험 연결도"
                : $"{testCableName} 시험 연결도";

            List<string> paragraphs = new List<string>();
            foreach (ConnectorType type in new[]
            {
                ConnectorType.Jig,
                ConnectorType.Adapter,
                ConnectorType.Test
            })
            {
                if (sectionParagraphs[type].Count == 0)
                    continue;

                paragraphs.Add($"[{GetCableTypeName(type)} 영역]");
                paragraphs.AddRange(sectionParagraphs[type]);
            }

            procedure = string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
            return true;
        }

        private static List<List<ConnectionLine>> BuildProcedureConnectionGroups(
            IReadOnlyCollection<ConnectionLine> lines)
        {
            List<List<ConnectionLine>> groups = new List<List<ConnectionLine>>();
            HashSet<ConnectionLine> remaining = new HashSet<ConnectionLine>(lines);

            while (remaining.Count > 0)
            {
                ConnectionLine first = remaining.First();
                Queue<ConnectionLine> pending = new Queue<ConnectionLine>();
                List<ConnectionLine> group = new List<ConnectionLine>();
                pending.Enqueue(first);
                remaining.Remove(first);

                while (pending.Count > 0)
                {
                    ConnectionLine current = pending.Dequeue();
                    group.Add(current);

                    List<ConnectionLine> connectedLines = remaining
                        .Where(candidate => SharesConnector(current, candidate))
                        .ToList();

                    foreach (ConnectionLine connectedLine in connectedLines)
                    {
                        remaining.Remove(connectedLine);
                        pending.Enqueue(connectedLine);
                    }
                }

                groups.Add(group);
            }

            return groups;
        }

        private static bool SharesConnector(ConnectionLine first, ConnectionLine second)
        {
            return first.StartConnector == second.StartConnector ||
                first.StartConnector == second.EndConnector ||
                first.EndConnector == second.StartConnector ||
                first.EndConnector == second.EndConnector;
        }

        private void AddJunctionInstructions(
            List<string> paragraphs,
            List<ConnectionLine> lines,
            int tjNumber,
            ConnectorType sourceType,
            ConnectorType targetType)
        {
            ConnectorSide sourceSide = ConnectorSide.Right;
            ConnectorSide targetSide = ConnectorSide.Left;
            List<Connector> sourceConnectors = GetConnectorsFacingBoundary(
                tjNumber, sourceType, sourceSide);
            List<Connector> targetConnectors = GetConnectorsFacingBoundary(
                tjNumber, targetType, targetSide);
            int pairCount = Math.Min(sourceConnectors.Count, targetConnectors.Count);

            for (int index = 0; index < pairCount; index++)
            {
                Connector sourceConnector = sourceConnectors[index];
                Connector targetConnector = targetConnectors[index];
                CableInfo? sourceCable = FindAssignedCable(lines, sourceConnector);
                CableInfo? targetCable = FindAssignedCable(lines, targetConnector);

                // 양쪽 커넥터 모두 실제 연결선과 DropItem이 있을 때만 접속 절차로 만든다.
                if (sourceCable == null || targetCable == null)
                    continue;

                string sourcePrefix = targetType == ConnectorType.Test
                    ? string.Empty
                    : $"TJ{tjNumber}에 연결된 ";

                // 시험 대상 케이블 단계에서는 조립에 필요한 케이블과 핀 정보만 간결하게 표시한다.
                paragraphs.Add(
                    $"- {sourcePrefix}{GetCableTypeName(sourceType)} {sourceCable.Name} “{sourceConnector.ConnectorName}”에 " +
                    $"{GetCableTypeName(targetType)} {targetCable.Name} “{targetConnector.ConnectorName}”을 연결한다.");
            }
        }

        private List<Connector> GetConnectorsFacingBoundary(
            int tjNumber,
            ConnectorType type,
            ConnectorSide side)
        {
            CablePanel panel = GetPanel(tjNumber, type);
            IReadOnlyList<Connector> connectors = side == ConnectorSide.Left
                ? panel.LeftConnectors
                : panel.RightConnectors;
            return connectors.ToList();
        }

        private static CableInfo? FindAssignedCable(
            IEnumerable<ConnectionLine> lines,
            Connector connector)
        {
            return lines.FirstOrDefault(line =>
                line.StartConnector == connector ||
                line.EndConnector == connector)?.CableInfo;
        }

        private static int GetTypeOrder(ConnectorType type) => type switch
        {
            ConnectorType.Jig => 0,
            ConnectorType.Adapter => 1,
            ConnectorType.Test => 2,
            _ => 3
        };

        private static int GetPinNumber(string connectorName)
        {
            return int.TryParse(connectorName.TrimStart('P', 'p'), out int number)
                ? number
                : int.MaxValue;
        }

        private static string FormatTJRanges(IReadOnlyList<int> tjNumbers)
        {
            List<string> ranges = new List<string>();

            for (int index = 0; index < tjNumbers.Count; index++)
            {
                int start = tjNumbers[index];
                int end = start;

                while (index + 1 < tjNumbers.Count && tjNumbers[index + 1] == end + 1)
                {
                    index++;
                    end = tjNumbers[index];
                }

                ranges.Add(start == end ? $"TJ{start}" : $"TJ{start}~TJ{end}");
            }

            return string.Join(", ", ranges);
        }

        private static string GetCableTypeName(ConnectorType type) => type switch
        {
            ConnectorType.Jig => "지그 케이블",
            ConnectorType.Adapter => "어댑터 케이블",
            ConnectorType.Test => "시험 대상 케이블",
            _ => "케이블"
        };

        public bool ResetAll()
        {
            if (history.Count == 0)
                return false;

            DialogResult result = PinConnectionDiagram.Helpers.ProjectMessageBox.Show(
                "모든 TJ의 커넥터 및 연결 설정을 초기화하시겠습니까?",
                "전체 설정 초기화",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return false;

            RestoreSnapshot(history[0]);
            return true;
        }

        public void RecordExternalChange()
        {
            // 준비물 추가·삭제처럼 지도 밖에서 발생한 작업도 동일한 Undo 이력에 기록한다.
            RecordHistory();
        }

        /// <summary>현재 편집 상태를 파일 저장용 참조 데이터로 변환한다.</summary>
        public ProjectFileData ExportProjectData(bool useDefenseTheme)
        {
            MapSnapshot snapshot = CaptureSnapshot();
            ProjectFileData data = new ProjectFileData
            {
                ApplicationVersion = Application.ProductVersion.Split('+')[0],
                SavedAt = DateTime.Now,
                UseDefenseTheme = useDefenseTheme,
                Supplies = snapshot.Supplies.ToList()
            };

            foreach (TJState tj in snapshot.TJs)
            {
                data.TJs.Add(new ProjectTJState
                {
                    TJNumber = tj.TJNumber,
                    IsOn = tj.IsOn,
                    Panels = tj.Panels.Select(panel => new ProjectPanelState
                    {
                        ConnectorType = panel.ConnectorType,
                        Connectors = panel.Connectors.Select(connector => new ProjectConnectorState
                        {
                            Side = connector.Side,
                            ConnectorName = connector.ConnectorName
                        }).ToList()
                    }).ToList()
                });
            }

            foreach (ConnectionState connection in snapshot.Connections)
            {
                data.Connections.Add(new ProjectConnectionState
                {
                    Start = ToProjectKey(connection.Start),
                    End = ToProjectKey(connection.End),
                    CableId = connection.CableInfo?.Id
                });
            }

            return data;
        }

        /// <summary>파일에서 읽은 프로젝트를 화면에 복원하고 새 Undo 기준점으로 설정한다.</summary>
        public void ImportProjectData(ProjectFileData data)
        {
            ValidateProjectData(data);
            Dictionary<Guid, CableInfo> suppliesById = data.Supplies.ToDictionary(item => item.Id);
            MapSnapshot snapshot = new MapSnapshot();
            snapshot.Supplies.AddRange(data.Supplies);

            foreach (ProjectTJState tj in data.TJs.OrderBy(item => item.TJNumber))
            {
                TJState tjState = new TJState { TJNumber = tj.TJNumber, IsOn = tj.IsOn };
                foreach (ProjectPanelState panel in tj.Panels)
                {
                    PanelState panelState = new PanelState { ConnectorType = panel.ConnectorType };
                    panelState.Connectors.AddRange(panel.Connectors.Select(connector => new ConnectorState
                    {
                        Side = connector.Side,
                        ConnectorName = connector.ConnectorName
                    }));
                    tjState.Panels.Add(panelState);
                }
                snapshot.TJs.Add(tjState);
            }

            foreach (ProjectConnectionState connection in data.Connections)
            {
                CableInfo? cable = connection.CableId.HasValue
                    ? suppliesById[connection.CableId.Value]
                    : null;
                snapshot.Connections.Add(new ConnectionState
                {
                    Start = FromProjectKey(connection.Start),
                    End = FromProjectKey(connection.End),
                    CableInfo = cable
                });
            }

            RestoreSnapshot(snapshot);
            history.Clear();
            history.Add(CaptureSnapshot());
            historyIndex = 0;
            HistoryChanged?.Invoke();
            QueueConnectionOverlayRefresh();
        }

        private static ProjectConnectorKey ToProjectKey(ConnectorKey key) => new()
        {
            TJNumber = key.TJNumber,
            ConnectorType = key.ConnectorType,
            Side = key.Side,
            Index = key.Index
        };

        private static ConnectorKey FromProjectKey(ProjectConnectorKey key) => new()
        {
            TJNumber = key.TJNumber,
            ConnectorType = key.ConnectorType,
            Side = key.Side,
            Index = key.Index
        };

        private static void ValidateProjectData(ProjectFileData data)
        {
            if (data.TJs.Count != 5 ||
                data.TJs.Select(tj => tj.TJNumber).Distinct().Count() != 5 ||
                data.TJs.Any(tj => tj.TJNumber is < 1 or > 5))
            {
                throw new InvalidDataException("TJ 구성이 올바르지 않습니다.");
            }

            if (data.Supplies.Any(item =>
                    item.Id == Guid.Empty ||
                    string.IsNullOrWhiteSpace(item.Category) ||
                    string.IsNullOrWhiteSpace(item.Name) ||
                    item.Count < 1) ||
                data.Supplies.Select(item => item.Id).Distinct().Count() != data.Supplies.Count)
            {
                throw new InvalidDataException("시험 준비물 데이터가 올바르지 않습니다.");
            }

            foreach (ProjectTJState tj in data.TJs)
            {
                if (tj.Panels.Count != Enum.GetValues<ConnectorType>().Length ||
                    tj.Panels.Select(panel => panel.ConnectorType).Distinct().Count() !=
                    Enum.GetValues<ConnectorType>().Length ||
                    tj.Panels.Any(panel =>
                        !Enum.IsDefined(panel.ConnectorType) ||
                        panel.Connectors.Any(connector =>
                            !Enum.IsDefined(connector.Side) ||
                            string.IsNullOrWhiteSpace(connector.ConnectorName))))
                {
                    throw new InvalidDataException($"TJ{tj.TJNumber} 커넥터 구성이 올바르지 않습니다.");
                }
            }

            HashSet<Guid> supplyIds = data.Supplies.Select(item => item.Id).ToHashSet();
            if (data.Connections.Any(connection =>
                connection.CableId.HasValue && !supplyIds.Contains(connection.CableId.Value)))
            {
                throw new InvalidDataException("연결선이 존재하지 않는 준비물을 참조합니다.");
            }


            foreach (ProjectConnectionState connection in data.Connections)
            {
                ValidateProjectConnectorKey(data, connection.Start);
                ValidateProjectConnectorKey(data, connection.End);
            }
        }

        private static void ValidateProjectConnectorKey(
            ProjectFileData data,
            ProjectConnectorKey key)
        {
            ProjectTJState? tj = data.TJs.FirstOrDefault(item => item.TJNumber == key.TJNumber);
            ProjectPanelState? panel = tj?.Panels.FirstOrDefault(
                item => item.ConnectorType == key.ConnectorType);
            int connectorCount = panel?.Connectors.Count(
                connector => connector.Side == key.Side) ?? 0;
            if (key.Index < 0 || key.Index >= connectorCount)
                throw new InvalidDataException("연결선의 커넥터 참조가 올바르지 않습니다.");
        }

        private void RecordHistory()
        {
            // Undo 이후 새 작업이 시작되면 더 이상 유효하지 않은 Redo 분기를 버린다.
            if (isRestoringHistory)
                return;

            if (historyIndex < history.Count - 1)
            {
                history.RemoveRange(
                    historyIndex + 1,
                    history.Count - historyIndex - 1);
            }

            history.Add(CaptureSnapshot());
            historyIndex = history.Count - 1;
            HistoryChanged?.Invoke();

            // 모든 모델 변경은 이 공통 지점을 통과한다. 개별 기능이 즉시 Refresh를 빠뜨려도
            // 최종 레이아웃 좌표가 확정된 뒤 선과 DropZone을 반드시 다시 생성한다.
            QueueConnectionOverlayRefresh();
        }

        private MapSnapshot CaptureSnapshot()
        {
            // UI 컨트롤 참조 대신 복원 가능한 값과 위치만 저장한다.
            MapSnapshot snapshot = new MapSnapshot();
            snapshot.Supplies.AddRange(getSupplies());

            for (int tjNumber = 1; tjNumber <= 5; tjNumber++)
            {
                TJState tjState = new TJState
                {
                    TJNumber = tjNumber,
                    IsOn = GetTJ(tjNumber).IsOn
                };

                foreach (ConnectorType type in Enum.GetValues<ConnectorType>())
                {
                    CablePanel panel = GetPanel(tjNumber, type);
                    PanelState panelState = new PanelState
                    {
                        ConnectorType = type
                    };

                    foreach (Connector connector in
                        panel.LeftConnectors.Concat(panel.RightConnectors))
                    {
                        panelState.Connectors.Add(new ConnectorState
                        {
                            Side = connector.Side,
                            ConnectorName = connector.ConnectorName
                        });
                    }

                    tjState.Panels.Add(panelState);
                }

                snapshot.TJs.Add(tjState);
            }

            foreach (ConnectionLine line in connectionLines)
            {
                snapshot.Connections.Add(new ConnectionState
                {
                    Start = GetConnectorKey(line.StartConnector),
                    End = GetConnectorKey(line.EndConnector),
                    CableInfo = line.CableInfo
                });
            }

            return snapshot;
        }

        private ConnectorKey GetConnectorKey(Connector connector)
        {
            // 컨트롤 재생성 후에도 찾을 수 있도록 TJ/종류/방향/순번으로 식별한다.
            CablePanel panel = GetPanel(connector.TJNumber, connector.ConnectorType);
            IReadOnlyList<Connector> connectors = connector.Side == ConnectorSide.Left
                ? panel.LeftConnectors
                : panel.RightConnectors;

            return new ConnectorKey
            {
                TJNumber = connector.TJNumber,
                ConnectorType = connector.ConnectorType,
                Side = connector.Side,
                Index = connectors.ToList().IndexOf(connector)
            };
        }

        private void RestoreSnapshot(MapSnapshot snapshot)
        {
            // 복원 과정에서 발생하는 이벤트가 새 이력으로 기록되지 않게 차단한다.
            isRestoringHistory = true;

            try
            {
                ClearSelectedConnector();
                connectionLines.Clear();
                SuppliesRestoreRequested?.Invoke(snapshot.Supplies);

                foreach (TJState tjState in snapshot.TJs)
                {
                    foreach (PanelState panelState in tjState.Panels)
                    {
                        CablePanel panel = GetPanel(
                            tjState.TJNumber,
                            panelState.ConnectorType);

                        panel.ClearConnector();

                        foreach (ConnectorState connectorState in panelState.Connectors)
                        {
                            panel.AddConnector(
                                connectorState.Side,
                                connectorState.ConnectorName);
                        }
                    }

                    GetTJ(tjState.TJNumber).IsOn = tjState.IsOn;

                    foreach (ConnectorType type in Enum.GetValues<ConnectorType>())
                    {
                        GetPanel(tjState.TJNumber, type).SetActive(tjState.IsOn);
                    }

                    UpdateRowHeight(tjState.TJNumber);
                }

                foreach (ConnectionState connectionState in snapshot.Connections)
                {
                    connectionLines.Add(new ConnectionLine
                    {
                        StartConnector = FindConnector(connectionState.Start),
                        EndConnector = FindConnector(connectionState.End),
                        CableInfo = connectionState.CableInfo
                    });
                }

                RefreshConnectedStates();
                connectionOverlay.RefreshConnections();
            }
            finally
            {
                isRestoringHistory = false;
            }

            // Undo/Redo 복원 중 준비물·커넥터 UI가 재생성되면서 레이아웃이 여러 번 바뀐다.
            // 즉시 그린 결과만 사용하면 이전 좌표가 남을 수 있으므로, 레이아웃이 안정된 뒤
            // 연결선과 DropZone을 확정 좌표로 한 번 더 그린다.
            QueueConnectionOverlayRefresh();
        }

        private Connector FindConnector(ConnectorKey key)
        {
            CablePanel panel = GetPanel(key.TJNumber, key.ConnectorType);
            IReadOnlyList<Connector> connectors = key.Side == ConnectorSide.Left
                ? panel.LeftConnectors
                : panel.RightConnectors;

            return connectors[key.Index];
        }

        private class MapSnapshot
        {
            public List<CableInfo> Supplies { get; } = new List<CableInfo>();
            public List<TJState> TJs { get; } = new List<TJState>();
            public List<ConnectionState> Connections { get; } = new List<ConnectionState>();
        }

        private class TJState
        {
            public int TJNumber { get; set; }
            public bool IsOn { get; set; }
            public List<PanelState> Panels { get; } = new List<PanelState>();
        }

        private class PanelState
        {
            public ConnectorType ConnectorType { get; set; }
            public List<ConnectorState> Connectors { get; } = new List<ConnectorState>();
        }

        private class ConnectorState
        {
            public ConnectorSide Side { get; set; }
            public string ConnectorName { get; set; } = string.Empty;
        }

        private class ConnectionState
        {
            public ConnectorKey Start { get; set; } = new ConnectorKey();
            public ConnectorKey End { get; set; } = new ConnectorKey();
            public CableInfo? CableInfo { get; set; }
        }

        private class ConnectorKey
        {
            public int TJNumber { get; set; }
            public ConnectorType ConnectorType { get; set; }
            public ConnectorSide Side { get; set; }
            public int Index { get; set; }
        }

    }
}
