using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Managers;
using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;
using System.Net.Http.Headers;
namespace PinConnectionDiagram
{
    public partial class Main : Form
    {
        // CableManager
        // MapManager
        // ConnectionManager
        private CableManager cableManager;
        private MapManager mapManager;

        public Main()
        {
            InitializeComponent();

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

            /****************************************************************************/

            cableManager = new CableManager();

            //mapManager = new MapManager(TlpMap);

            //CreateCablePanels();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            mapManager = new MapManager(TlpMap);

            mapManager.Create();
            mapManager.HistoryChanged += MapManager_HistoryChanged;
            UpdateHistoryButtons();
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
        }

        private void ResetSupplies()
        {
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
        }

        private void ClearItemPanel(FlowLayoutPanel panel)
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
            btnBack.Enabled = mapManager.CanUndo;
            btnForward.Enabled = mapManager.CanRedo;
        }

        private void TlpTJ_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not TableLayoutPanel tlp)
                return;

            Graphics g = e.Graphics;

            Color borderColor = Color.FromArgb(30, 46, 69);

            using (Pen pen = new Pen(borderColor, 2))
            {
                //---------------------------------
                // 바깥 Border
                //---------------------------------
                g.DrawRectangle(
                    pen,
                    0,
                    0,
                    tlp.Width,
                    tlp.Height);

                //---------------------------------
                // Row Border
                //---------------------------------

                int rowHeight = tlp.Height / tlp.RowCount;

                for (int i = 1; i < tlp.RowCount; i++)
                {
                    int y = rowHeight * i;

                    g.DrawLine(
                        pen,
                        0,
                        y,
                        tlp.Width,
                        y);
                }
            }
        }

        // 배경 그라디언트 효과 함수
        private void TlpBg_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not Control ctrl)
                return;

            if (ctrl.Width <= 0 || ctrl.Height <= 0)
                return;

            Color startColor = ColorTranslator.FromHtml("#252525");
            Color endColor = ColorTranslator.FromHtml("#3E4377");

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

            DrawHelper.DrawGlowLine(e.Graphics, ctrl.Width, 0, 4, Color.FromArgb(180, 100, 220, 255));
            DrawHelper.DrawGlowLine(e.Graphics, ctrl.Width, ctrl.Height - 4, 4, Color.FromArgb(180, 100, 220, 255));
            DrawHelper.DrawGlowLine(e.Graphics, ctrl.Width, 0, 4, Color.FromArgb(180, 100, 220, 255));
            DrawHelper.DrawGlowLine(e.Graphics, ctrl.Width, ctrl.Height - 4, 4, Color.FromArgb(180, 100, 220, 255));
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
                Color.FromArgb(255, 145, 223, 251)
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
                Color.FromArgb(255, 63, 202, 255)
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
                Color.FromArgb(255, 30, 46, 69)
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
                Color.FromArgb(255, 56, 60, 98)
            );
        }

        // 시험 대상 준비물 추가 /////////////////////////////////////////////////////////////////////////////
        private void btnSupAdd_MouseUp(object sender, MouseEventArgs e)
        {
            using (AddCableForm form = new AddCableForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    CableInfo info = form.CableInfo;

                    cableManager.Add(info);

                    CableCard card = new CableCard(info);
                    card.DeleteRequested += DeleteCable;
                    FlpSupplies.Controls.Add(card);

                    CreateItems(info);
                }
            }
        }

        // 시험 준비물 추가 시 해당 카테고리에 아이템 생성 함수
        private void CreateItems(CableInfo info)
        {
            FlowLayoutPanel panel = GetPanel(info.Category);

            CableItem item = new CableItem(info);
            panel.Controls.Add(item);

        }

        // 삭제 부분 ///////////////////////////////////////////////////////////////////////////////
        // 추가된 시험 준비물 삭제 여부 확인 및 삭제함수들 실행
        private void DeleteCable(CableInfo info)
        {
            DialogResult result = MessageBox.Show(
                "삭제하시겠습니까?",
                "확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            cableManager.Remove(info);

            DeleteCard(info);
            DeleteItem(info);
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
        private FlowLayoutPanel GetPanel(string category)
        {
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

        ////////////////////////////////////////////////////////////////////////////////////
        //private void CreateCablePanels() 
        //{
        //    CablePanel jig = new CablePanel(1, ConnectorType.Jig);
        //    CablePanel adapter = new CablePanel(1, ConnectorType.Adapter);
        //    CablePanel test = new CablePanel(1, ConnectorType.Test);

        //    mapManager.RegisterCablePanel(1, ConnectorType.Jig, jig);
        //    mapManager.RegisterCablePanel(1, ConnectorType.Adapter, adapter);
        //    mapManager.RegisterCablePanel(1, ConnectorType.Test, test);
        //}

        //private void PnlLine_Paint(object sender, PaintEventArgs e)
        //{
        //    Graphics g = e.Graphics;

        //    g.SmoothingMode = SmoothingMode.AntiAlias;

        //    foreach (ConnectionInfo connection in connectionManager.Connections)
        //    {
        //        Point start = PnlLine.PointToClient(connection.Start.ConnectionPoint);

        //        Point end = PnlLine.PointToClient(connection.End.ConnectionPoint);

        //        DrawConnection(g, start, end);
        //    }
        //}

        //private void DrawConnection(Graphics g, Point start, Point end)
        //{
        //    int middleX = (start.X + end.X) / 2;

        //    using Pen pen = new Pen(Color.Black, 4);

        //    g.DrawLine(pen, start, new Point(middleX, start.Y));

        //    g.DrawLine(pen, new Point(middleX, start.Y), new Point(middleX, end.Y));

        //    g.DrawLine(pen, new Point(middleX, end.Y), end);
        //}
    }
}
