using PinConnectionDiagram.Controls;
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
        private void GlowLine_paint(object sender, PaintEventArgs e)
        {
            TableLayoutPanel? tlp = sender as TableLayoutPanel;

            if (tlp == null)
                return;

            Graphics g = e.Graphics;

            Draw_glowLine(g, tlp, 0);    // 상단
            Draw_glowLine(g, tlp, tlp.Height - 3); // 하단
        }

        // 광선 그리기 함수
        private void Draw_glowLine(Graphics g, TableLayoutPanel tlp, int y)
        {
            Rectangle rect = new Rectangle(0, y, tlp.Width, 3);

            using (LinearGradientBrush brush =
                new LinearGradientBrush(
                    rect,
                    Color.Transparent,
                    Color.Transparent,
                    0f))
            {
                brush.InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.Transparent,
                        Color.FromArgb(180, 100, 220, 255),
                        Color.Transparent
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                g.FillRectangle(brush, rect);
            }
        }

        // 시험 대상 준비물 추가 버튼 이벤트
        private void btnSupAdd_MouseUp(object sender, MouseEventArgs e)
        {
            //btnSupAdd.BackgroundImage = Properties.Resources.추가;

            using (AddCableForm form = new AddCableForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    AddCard(form.CableInfo);
                }

            }
        }

        // 시험 대상 준비물 추가 함수
        private void AddCard(CableInfo info)
        {
            CableCard card = new CableCard(info);

            card.DeleteRequested += Card_DeleteRequested;

            FlpSupplies.Controls.Add(card);
        }

        private void Card_DeleteRequested(object sender, EventArgs e)
        {
            CableCard card = sender as CableCard;

            if (card == null)
                return;

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
        }
    }
}
