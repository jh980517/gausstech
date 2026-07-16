using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Models;
using System.Text.Json.Serialization;

namespace PinConnectionDiagram.Managers
{
    public class MapManager
    {
        // TJControl 생성
        // ConnectorRow 생성
        // PinConnector 관리
        private readonly TableLayoutPanel tlpMap;
        private readonly Dictionary<int, TJControl> tjControls = new();
        private readonly Dictionary<(int, ConnectorType), CablePanel> cablePanels = new();
        public MapManager(TableLayoutPanel table)
        {
            tlpMap = table;
        }

        public void RegisterCablePanel(int tjNumber, ConnectorType type, CablePanel panel)
        {
            cablePanels[(tjNumber, type)] = panel;
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
        }

        public void RemoveJigRightConnector(int tj)
        {
            CablePanel jig = GetPanel(tj, ConnectorType.Jig);

            if (jig.RightConnectors.Count == 0)
                return;

            jig.RemoveConnector(jig.RightConnectors.Last());

            SyncAdapterLeft(tj);
            UpdateRowHeight(tj);
        }

        public void AddAdapterRightConnector(int tj)
        {
            CablePanel adapter = GetPanel(tj, ConnectorType.Adapter);

            adapter.AddConnector(ConnectorSide.Right);

            SyncTestLeft(tj);
            UpdateRowHeight(tj);
        }

        public void RemoveAdapterRightConnector(int tj)
        {
            CablePanel adapter = GetPanel(tj, ConnectorType.Adapter);

            if (adapter.RightConnectors.Count == 0)
                return;

            adapter.RemoveConnector(adapter.RightConnectors.Last());

            SyncTestLeft(tj);
            UpdateRowHeight(tj);
        }

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
                adapter.RemoveConnector(adapter.LeftConnectors.Last());
            }
        }

        private void SyncTestLeft(int tj)
        {
            CablePanel adapter = GetPanel(tj, ConnectorType.Adapter);
            CablePanel test = GetPanel(tj, ConnectorType.Test);

            while (test.LeftConnectors.Count <  adapter.RightConnectors.Count)
            {
                test.AddConnector(ConnectorSide.Left);
            }

            while (test.LeftConnectors.Count > adapter.RightConnectors.Count)
            {
                test.RemoveConnector(test.LeftConnectors.Last());
            }
        }

        public void Create()
        {
            CreateRows();

            InitializeDefaultConnector();
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
            //MessageBox.Show($"TJ{tj.TJNumber} : {isOn}");
            GetPanel(tj.TJNumber, ConnectorType.Jig).SetActive(isOn);

            GetPanel(tj.TJNumber, ConnectorType.Adapter).SetActive(isOn);

            GetPanel(tj.TJNumber, ConnectorType.Test).SetActive(isOn);
        }

        private void Panel_AddRightConnectorRequested(CablePanel panel)
        {
            switch (panel.Type)
            {
                case ConnectorType.Jig:
                    AddJigRightConnector(panel.TJNumber);
                    break;
                case ConnectorType.Adapter:
                    AddAdapterRightConnector(panel.TJNumber);
                    break;
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