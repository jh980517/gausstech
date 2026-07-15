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

        public void Create()
        {
            //CreateHeader();
            CreateRows();
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
            tj.Dock = DockStyle.Fill;
            tj.Margin = new Padding(50, 30, 0, 30);
            tlpMap.Controls.Add(tj, 0, row);
            tjControls.Add(row, tj);
        }

        private void CreatePanel(int row, ConnectorType type)
        {
            CablePanel panel = new CablePanel(row, type);

            panel.Dock = DockStyle.Fill;
            panel.Margin = new Padding(0);

            int column = type switch
            {
                ConnectorType.Jig => 1,
                ConnectorType.Adapter => 2,
                ConnectorType.Test => 3,
                _ => 1
            };

            tlpMap.Controls.Add(panel, column, row);
            cablePanels.Add((row, type), panel);
        }

        public CablePanel GetPanel(int tj, ConnectorType type)
        {
            return cablePanels[(tj, type)];
        }

        public TJControl GetTJ(int tj)
        {
            return tjControls[tj];
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