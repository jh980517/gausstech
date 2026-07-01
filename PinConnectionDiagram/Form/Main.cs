using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Managers;
using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;
namespace PinConnectionDiagram
{
    public partial class Main : Form
    {
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
        }

        private CableManager cableManager = new CableManager();

        // 배경 그라디언트 효과 함수
        private void TlpBg_Paint(object sender, PaintEventArgs e)
        {
            Color startColor = ColorTranslator.FromHtml("#252525");
            Color endColor = ColorTranslator.FromHtml("#3E4377");
            float angle = 90F;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.TlpBg.ClientRectangle, startColor, endColor, angle))
            {
                e.Graphics.FillRectangle(brush, this.TlpBg.ClientRectangle);
            }
        }


        // TableLayoutPanel에 맞춰 상,하단으로 광선 그리기 함수
        public void GlowLine_paint(object sender, PaintEventArgs e)
        {
            TableLayoutPanel? tlp = sender as TableLayoutPanel;

            if (tlp == null)
                return;

            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, 0, 4);
            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, tlp.Height - 4, 4);
            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, 0, 4);
            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, tlp.Height - 4, 4);
        }

        // 시험 대상 준비물 추가 버튼 이벤트
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

        private void CreateItems(CableInfo info)
        {
            FlowLayoutPanel panel = GetPanel(info.Category);

            for (int i = 0; i < info.Count; i++)
            {
                CableItem item = new CableItem(info);
                panel.Controls.Add(item);
            }
        }

        private void DeleteCable(CableInfo info)
        {
            cableManager.Remove(info);
            DeleteCard(info);
            DeleteItem(info);
        }

        private void DeleteCard(CableInfo info)
        {
            foreach (CableCard card in FlpSupplies.Controls.OfType<CableCard>().ToList())
            {
                if (card.Info.Id == info.Id)
                {
                    DialogResult result = MessageBox.Show(
                        "삭제하시겠습니까?",
                        "확인",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        FlpSupplies.Controls.Remove(card);
                        card.Dispose();
                    }
                    break;
                }
            }
        }

        private void DeleteItem(CableInfo info)
        {
            FlowLayoutPanel panel = GetPanel(info.Category);

            foreach (CableItem item in panel.Controls.OfType<CableItem>().ToList())
            {
                if (item.Info.Id == info.Id)
                {
                    panel.Controls.Remove(item);
                    item.Dispose();
                }
            }
        }

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

        private void PnlMap_DragDrop(object sender,  DragEventArgs e)
        {
            CableItem item = (CableItem)e.Data.GetData(typeof(CableItem));


            Point p = PnlMap.PointToClient(new Point (e.X, e.Y));

            if (item.Parent == PnlMap)
            {
                item.Location = p;
            }
            else
            {
                CableItem newItem = new CableItem(item.Info);
                newItem.Location = p;
                PnlMap.Controls.Add(newItem);
            }            

        }
        
        private void PnlMap_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(typeof(CableItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
    }
}
