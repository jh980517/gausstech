using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Models;

namespace PinConnectionDiagram.Managers
{
    public class MapManager
    {
        // TJControl 생성
        // ConnectorRow 생성
        // PinConnector 관리
        private readonly TableLayoutPanel TlpMap;

        private readonly Dictionary<int, TJControl> tjControls = new();

        //private readonly Dictionary<(int, ConnectorType), ConnectorRow> connectorRows = new();
        //private readonly ConnectionManager connectionManager = new();
        public MapManager(TableLayoutPanel table)
        {
            TlpMap = table;
        }

        //public ConnectionManager ConnectionManager
        //{
        //    get => connectionManager;
        //}

        //public void CreateRows()
        //{
        //    for (int row = 1; row <= 5; row++)
        //    {
        //        // TJ
        //        TJControl tj = new TJControl(row);

        //        tj.StateChanged += TJ_StateChanged;

        //        tj.Anchor = AnchorStyles.Right;
        //        tj.Size = new Size(60, 34);
        //        tj.Margin = new Padding(0, 0, 5, 0);

        //        TlpMap.Controls.Add(tj, 0, row);

        //        // Jig

        //        ConnectorRow jig = new ConnectorRow(row, ConnectorType.Jig);

        //        jig.Dock = DockStyle.Fill;
        //        jig.SetConnectorVisible(false);
        //        TlpMap.Controls.Add(jig, 1, row);

        //        jig.LeftConnector.PointClicked += OnPointClicked;

        //        if (jig.RightConnector != null)
        //        {
        //            jig.RightConnector.PointClicked += OnPointClicked;
        //        }

        //        // Adapter

        //        ConnectorRow adapter = new ConnectorRow(row, ConnectorType.Adapter);

        //        adapter.Dock = DockStyle.Fill;
        //        adapter.SetConnectorVisible(false);
        //        TlpMap.Controls.Add(adapter, 2, row);

        //        adapter.LeftConnector.PointClicked += OnPointClicked;

        //        if (adapter.RightConnector != null)
        //        {
        //            adapter.RightConnector.PointClicked += OnPointClicked;
        //        }

        //        // Test

        //        ConnectorRow test = new ConnectorRow(row, ConnectorType.Test);

        //        test.Dock = DockStyle.Fill;
        //        test.SetConnectorVisible(false);
        //        TlpMap.Controls.Add(test, 3, row);

        //        test.LeftConnector.PointClicked += OnPointClicked;
        //        if (test.RightConnector != null) 
        //        {
        //            test.RightConnector.PointClicked += OnPointClicked;
        //        }

        //        connectorRows.Add((row, ConnectorType.Jig), jig);
        //        connectorRows.Add((row, ConnectorType.Adapter), adapter);
        //        connectorRows.Add((row, ConnectorType.Test), test);

        //        RegisterDropEvent(jig);
        //        RegisterDropEvent(adapter);
        //        RegisterDropEvent(test);
        //    }
        ////}

        //private void OnPointClicked(PinConnector connector)
        //{
        //    connectionManager.SelectConnector(connector);
        //}

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
            //cable.DeleteRequested += DeleteDiagramCableInfo;

            //Point p = panel.PointToClient(new Point(e.X, e.Y));

            //if (item != null)
            //{
            //    if (item.Parent == panel)
            //    {
            //        item.Location = p;
            //    }
            //    else
            //    {
            //        DiagramCable newItem = new DiagramCable(item.Info);
            //        newItem.Location = p;

            //        //newItem.DeleteRequested += DeleteDiagramCable;

            //        panel.Controls.Add(newItem);
            //    }
            //}
        }

        //private void Panel_DragEnter(object sender, DragEventArgs e)
        //{
        //    if (!e.Data.GetDataPresent(typeof(CableItem)))
        //    {
        //        e.Effect = DragDropEffects.None;
        //        return;
        //    }

        //    CableItem item = (CableItem)e.Data.GetData(typeof(CableItem));

        //    Panel panel = (Panel)sender;

        //    //ConnectorRow row = (ConnectorRow)panel.Parent;

        //    bool allow = false;

        //    switch(row.Type)
        //    {
        //        case ConnectorType.Jig:
        //            allow = item.Info.Category == "지그 케이블";
        //            break;
        //        case ConnectorType.Adapter:
        //            allow = item.Info.Category == "어댑터 케이블";
        //            break;
        //        case ConnectorType.Test:
        //            allow = item.Info.Category == "시험 대상 케이블";
        //            break;
        //    }

        //    e.Effect = allow ? DragDropEffects.Copy : DragDropEffects.None;
        //}

        //private void TJ_StateChanged(TJControl sender, bool isOn)
        //{
        //    UpdateConnectorVisible(sender.TJNumber, isOn);
        //}

        //private void UpdateConnectorVisible(int tjNumber, bool visible)
        //{
        //    connectorRows[(tjNumber, ConnectorType.Jig)].SetConnectorVisible(visible);
        //    connectorRows[(tjNumber, ConnectorType.Adapter)].SetConnectorVisible(visible);
        //    connectorRows[(tjNumber, ConnectorType.Test)].SetConnectorVisible(visible);
        //}

        //// 케이블 카드 삭제 시 관련 아이템 삭제 함수
        //public void DeleteDiagramCableInfo(CableInfo info)
        //{

        //    foreach (ConnectorRow row in connectorRows.Values)
        //    {
        //        DeleteDiagramCable(row.DropZone, info);
        //    }

        //}

        //private void RegisterDropEvent(ConnectorRow row)
        //{
        //    row.DropZone.AllowDrop = true;

        //    row.DropZone.DragEnter += Panel_DragEnter;
        //    row.DropZone.DragDrop += Panel_DragDrop;
        //}

        //public void CreateConnectors()
        //{
        //    CreateCategory(pnlJig, "지그 케이블", true);
        //    CreateCategory(pnlAdapter, "어댑터 케이블", true);
        //    CreateCategory(pnlTest, "시험 대상 케이블", false);
        //}

        //private void CreateCategory(
        //    Panel panel,
        //    string category,
        //    bool hasRight)
        //{
        //    panel.Controls.Clear();

        //    for (int tj = 1; tj <= 5; tj++)
        //    {
        //        int y = 0 + (tj - 1) * 110;

        //        PinConnector left =
        //            new PinConnector(
        //                tj,
        //                category,
        //                ConnectorType.Left);

        //        left.Location = new Point(10, y);
        //        left.Visible = false;

        //        panel.Controls.Add(left);

        //        connectors.Add(left);

        //        if (hasRight)
        //        {
        //            PinConnector right =
        //                new PinConnector(
        //                    tj,
        //                    category,
        //                    ConnectorType.Right);

        //            right.Location =
        //                new Point(
        //                    panel.Width - right.Width - 10,
        //                    y);

        //            right.Visible = false;

        //            panel.Controls.Add(right);

        //            connectors.Add(right);
        //        }
        //    }
        //}

        //public void SetTJVisible(int tjNumber, bool visible)
        //{
        //    foreach (PinConnector connector in connectors)
        //    {
        //        if (connector.TJNumber == tjNumber)
        //        {
        //            connector.Visible = visible;
        //        }
        //    }
        //}
    }
}