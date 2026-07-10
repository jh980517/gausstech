using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Managers;
using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;
namespace PinConnectionDiagram
{
    public partial class Main : Form
    {
        private CableManager cableManager = new CableManager();
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

            mapManager = new MapManager(
                PnlJig,
                PnlAdapter,
                PnlTest);

            //mapManager.RegisterPanels();

            CreateTJControls();
            CreateConnectors();
        }

        private void CreateConnectors()
        {
            CreateCategory(PnlJig, "지그 케이블", true);
            CreateCategory(PnlAdapter, "어댑터 케이블", true);
            CreateCategory(PnlTest, "시험 대상 케이블", false);
        }

        private void CreateCategory(Panel panel, String category, bool hasRight)
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
                }
            }
        }

        private void TJ_StateChanged(TJControl sender, bool isOn)
        {
            UpdateConnectorVisible(PnlJig, sender.TJNumber, isOn);
            UpdateConnectorVisible(PnlAdapter, sender.TJNumber, isOn);
            UpdateConnectorVisible(PnlTest, sender.TJNumber, isOn);
        }

        private void UpdateConnectorVisible(Panel panel, int tj, bool visible)
        {
            foreach (PinConnector connector
                     in panel.Controls.OfType<PinConnector>())
            {
                if (connector.TJNumber == tj)
                {
                    connector.Visible = visible;
                }
            }
        }

        private void CreateTJControls()
        {
            TlpTJ.Controls.Clear();

            for (int i = 0; i < 5; i++)
            {
                TJControl tj = new TJControl(i + 1);

                tj.Anchor = AnchorStyles.Right;
                tj.Margin = new Padding(5);

                // ★ 이벤트 연결
                tj.StateChanged += TJ_StateChanged;

                TlpTJ.Controls.Add(tj, 0, i);
            }
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
            DeleteDiagramCableInfo(info);
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

        // 케이블 카드 삭제 시 관련 아이템 삭제 함수
        private void DeleteDiagramCableInfo(CableInfo info)
        {
            foreach (Panel panel in new Panel[]
            {
                PnlJig,
                PnlAdapter,
                PnlTest
            })
            {
                foreach (DiagramCable cable in panel.Controls.OfType<DiagramCable>().ToList())
                {
                    if (cable.Info == info)
                    {
                        panel.Controls.Remove(cable);
                        cable.Dispose();
                    }
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
    }
}
