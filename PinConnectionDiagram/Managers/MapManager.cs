using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Models;

namespace PinConnectionDiagram.Managers
{
    public class MapManager
    {
        private readonly Panel pnlJig;
        private readonly Panel pnlAdapter;
        private readonly Panel pnlTest;

        private readonly List<PinConnector> connectors = new();
        public MapManager(
            Panel jig,
            Panel adapter,
            Panel test)
        {
            pnlJig = jig;
            pnlAdapter = adapter;
            pnlTest = test;
        }

        public void RegisterPanels()
        {
            Panel[] panels =
            {
                pnlJig,
                pnlAdapter,
                pnlTest
            };

            foreach (Panel panel in panels)
            {
                panel.AllowDrop = true;
                panel.DragEnter += Panel_DragEnter;
                panel.DragDrop += Panel_DragDrop;
            }
        }

        // 우클릭 한 해당 아이템 삭제 함수
        private void DeleteDiagramCable(DiagramCable cable)
        {
            if (cable.Parent is Panel panel)
            {
                panel.Controls.Remove(cable);
            }

            cable.Dispose();
        }

        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(CableItem)))
                return;

            CableItem item = (CableItem)e.Data.GetData(typeof(CableItem));

            Panel panel = (Panel)sender;

            Point p = panel.PointToClient(new Point(e.X, e.Y));

            if (item != null)
            {
                if (item.Parent == panel)
                {
                    item.Location = p;
                }
                else
                {
                    DiagramCable newItem = new DiagramCable(item.Info);
                    newItem.Location = p;

                    newItem.DeleteRequested += DeleteDiagramCable;

                    panel.Controls.Add(newItem);
                }
            }
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(CableItem)))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            CableItem item = (CableItem)e.Data.GetData(typeof(CableItem));

            Panel panel = (Panel)sender;

            if (panel.Tag?.ToString() == item.Info.Category.ToString())
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        public void CreateConnectors()
        {
            CreateCategory(pnlJig, "지그 케이블", true);
            CreateCategory(pnlAdapter, "어댑터 케이블", true);
            CreateCategory(pnlTest, "시험 대상 케이블", false);
        }

        private void CreateCategory(
            Panel panel,
            string category,
            bool hasRight)
        {
            panel.Controls.Clear();

            for (int tj = 1; tj <= 5; tj++)
            {
                int y = 40 + (tj - 1) * 110;

                PinConnector left =
                    new PinConnector(
                        tj,
                        category,
                        ConnectorSide.Left);

                left.Location = new Point(10, y);
                left.Visible = false;

                panel.Controls.Add(left);

                connectors.Add(left);

                if (hasRight)
                {
                    PinConnector right =
                        new PinConnector(
                            tj,
                            category,
                            ConnectorSide.Right);

                    right.Location =
                        new Point(
                            panel.Width - right.Width - 10,
                            y);

                    right.Visible = false;

                    panel.Controls.Add(right);

                    connectors.Add(right);
                }
            }
        }

        public void SetTJVisible(int tjNumber, bool visible)
        {
            foreach (PinConnector connector in connectors)
            {
                if (connector.TJNumber == tjNumber)
                {
                    connector.Visible = visible;
                }
            }
        }
    }
}