using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Models;
using System.Text.Json.Serialization;

namespace PinConnectionDiagram.Managers
{
    public class MapManager
    {
        private readonly TableLayoutPanel tlpMap;
        private readonly Dictionary<int, TJControl> tjControls = new();
        private readonly Dictionary<(int, ConnectorType), CablePanel> cablePanels = new();
        private readonly List<ConnectionLine> connectionLines = new();
        private readonly ConnectionOverlay connectionOverlay;
        private readonly List<MapSnapshot> history = new();
        private int historyIndex = -1;
        private bool isRestoringHistory;
        private Connector? selectedConnector;

        public bool CanUndo => historyIndex > 0;
        public bool CanRedo => historyIndex >= 0 && historyIndex < history.Count - 1;
        public event Action? HistoryChanged;
        public MapManager(TableLayoutPanel table)
        {
            tlpMap = table;

            connectionOverlay = new ConnectionOverlay(() => connectionLines);
            tlpMap.Parent?.Controls.Add(connectionOverlay);
            SyncConnectionOverlayBounds();
            connectionOverlay.BringToFront();

            tlpMap.LocationChanged += (_, _) => SyncConnectionOverlayBounds();
            tlpMap.SizeChanged += (_, _) => SyncConnectionOverlayBounds();
        }

        private void SyncConnectionOverlayBounds()
        {
            connectionOverlay.Bounds = tlpMap.Bounds;
            connectionOverlay.RefreshConnections();
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
                test.AddConnector(ConnectorSide.Left);
            }

            while (test.LeftConnectors.Count > adapter.RightConnectors.Count)
            {
                RemoveConnector(test, test.LeftConnectors.Last());
            }
        }

        public void Create()
        {
            CreateRows();

            InitializeDefaultConnector();
            RecordHistory();
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

            if (type == ConnectorType.Test)
            {
                panel.ShowAddButton = false;
            }
        }

        public TJControl GetTJ(int tj)
        {
            return tjControls[tj];
        }

        private void SyncConnector(int tj)
        {
            // jig right 개수 확인

            // adapter left 개수 맞춤

            // adapter right 개수 확인

            // test left 개수 맞춤
        }

        private void TJ_StateChanged(TJControl tj, bool isOn)
        {
            if (!isOn)
            {
                DialogResult result = MessageBox.Show(
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
            DialogResult result = MessageBox.Show(
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
            tlpMap.MinimumSize = new Size(tlpMap.Width, totalHeight);
        }

        private void Panel_ConnectorPointClicked(Connector connector)
        {
            if (selectedConnector == null)
            {
                selectedConnector = connector;
                selectedConnector.SetConnectionPending(true);

                MessageBox.Show(
                    $"{connector.ConnectorName} 커넥터가 선택되었습니다\n" +
                    "연결할 두 번째 커넥터의 연결점을 선택하세요.",
                    "연결 시작",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (ReferenceEquals(selectedConnector, connector))
            {
                ClearSelectedConnector();

                MessageBox.Show(
                    "연결 선택이 취소되었습니다.",
                    "연결 취소",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            //MessageBox.Show(
            //    $"첫 번째: TJ{selectedConnector.TJNumber}, " +
            //    $"{selectedConnector.ConnectorType}, " +
            //    $"{selectedConnector.Side}, " +
            //    $"{selectedConnector.ConnectorName}\n\n" +
            //    $"두 번째: TJ{connector.TJNumber}, " +
            //    $"{connector.ConnectorType}, " +
            //    $"{connector.Side}, " +
            //    $"{connector.ConnectorName}",
            //    "커넥터 정보 확인");

            if (!CanConnect(selectedConnector, connector))
            {
                MessageBox.Show(
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

            MessageBox.Show("연결이 생성되었습니다.");

            ClearSelectedConnector();
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

        public bool ResetAll()
        {
            if (history.Count == 0)
                return false;

            DialogResult result = MessageBox.Show(
                "모든 TJ의 커넥터 및 연결 설정을 초기화하시겠습니까?",
                "전체 설정 초기화",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return false;

            RestoreSnapshot(history[0]);
            RecordHistory();
            return true;
        }

        private void RecordHistory()
        {
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
        }

        private MapSnapshot CaptureSnapshot()
        {
            MapSnapshot snapshot = new MapSnapshot();

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
                    End = GetConnectorKey(line.EndConnector)
                });
            }

            return snapshot;
        }

        private ConnectorKey GetConnectorKey(Connector connector)
        {
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
            isRestoringHistory = true;

            try
            {
                ClearSelectedConnector();
                connectionLines.Clear();

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
                        EndConnector = FindConnector(connectionState.End)
                    });
                }

                RefreshConnectedStates();
                connectionOverlay.RefreshConnections();
            }
            finally
            {
                isRestoringHistory = false;
            }
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
        }

        private class ConnectorKey
        {
            public int TJNumber { get; set; }
            public ConnectorType ConnectorType { get; set; }
            public ConnectorSide Side { get; set; }
            public int Index { get; set; }
        }

        // 우클릭 한 해당 아이템 삭제 함수
        private void DeleteDiagramCable(Control parent, CableInfo info)
        {
            foreach (DropItem cable in parent.Controls.OfType<DropItem>().ToList())
            {
                if (cable.Info.Id == info.Id)
                {
                    parent.Controls.Remove(cable);
                    cable.Dispose();
                }
            }
        }

        private void Panel_DragDrop(object? sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(CableItem)))
                return;

            CableItem item = (CableItem)e.Data.GetData(typeof(CableItem));

            Panel panel = (Panel)sender;

            //ConnectorRow row = (ConnectorRow)panel.Parent;

            panel.Controls.Clear();

            DropItem cable = new DropItem(item.Info);
            cable.Dock = DockStyle.Fill;
            panel.Controls.Add(cable);
        }
    }
}
