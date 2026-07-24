using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// 등록된 시험 준비물의 분류, 이름, 수량을 표시하고 삭제 요청을 전달한다.
    /// </summary>
    public partial class CableCard : UserControl
    {
        // 2px 외곽선 안쪽에 3px 빈 영역이 남도록 전체 Margin을 5px로 확보한다.
        private const int ExportMargin = 5;
        private ContextMenuStrip? editMenu;
        public event Action<CableInfo>? DeleteRequested;
        public event Action<CableInfo>? EditRequested;
        public CableInfo Info { get; }
        public Size ExportSize => new Size(
            TlpCableCard.Width + ExportMargin * 2,
            TlpCableCard.Height + ExportMargin * 2);
        public CableCard(CableInfo info)
        {
            InitializeComponent();

            Info = info;

            lblCategory.Text = info.Category;
            lblName.Text = info.Name;
            lblCount.Text = info.Count.ToString();
            AdjustWidthToContent();
            ApplyTheme();
            ConfigureContextMenu();
        }

        private void ConfigureContextMenu()
        {
            editMenu = new ContextMenuStrip
            {
                BackColor = AppTheme.Background,
                ForeColor = AppTheme.Accent,
                ShowImageMargin = false
            };
            ToolStripMenuItem editItem = new ToolStripMenuItem("수정")
            {
                ForeColor = AppTheme.Accent
            };
            editItem.Click += (_, _) => EditRequested?.Invoke(Info);
            editMenu.Items.Add(editItem);

            // 카드 내부의 어느 셀을 우클릭해도 동일한 수정 메뉴가 표시된다.
            ApplyContextMenu(this, editMenu);
        }

        private static void ApplyContextMenu(
            Control control,
            ContextMenuStrip menu)
        {
            control.ContextMenuStrip = menu;
            foreach (Control child in control.Controls)
                ApplyContextMenu(child, menu);
        }

        private void AdjustWidthToContent()
        {
            int nameWidth = TextRenderer.MeasureText(
                Info.Name,
                lblName.Font,
                new Size(int.MaxValue, lblName.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
            int categoryWidth = TextRenderer.MeasureText(
                Info.Category,
                lblCategory.Font,
                new Size(int.MaxValue, lblCategory.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;

            const int headerColumnWidth = 54;
            int valueColumnWidth = Math.Clamp(Math.Max(nameWidth, categoryWidth) + 20, 116, 220);
            int tableWidth = headerColumnWidth + valueColumnWidth;
            int cardWidth = tableWidth + 20;

            TlpCableCard.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, headerColumnWidth);
            TlpCableCard.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100F);
            TlpCableCard.Width = tableWidth;
            Width = cardWidth;
        }

        /// <summary>
        /// 현재 테마의 강조색과 카테고리 색상을 준비물 카드에 적용한다.
        /// </summary>
        public void ApplyTheme()
        {
            // 화면 상단 준비물 표는 카테고리와 관계없이 기존의 공통 강조색을 사용한다.
            lblCategory.ForeColor = AppTheme.Accent;
            lblName.ForeColor = AppTheme.Accent;
            lblCount.ForeColor = AppTheme.Accent;

            foreach (Label header in new[] { lblCategoryHeader, lblNameHeader, lblCountHeader })
                header.BackColor = AppTheme.DarkAccent;

            btnDelete.BackgroundImage = AppTheme.GetImage("delete", "delete_defense");
            btnDelete.BackColor = AppTheme.Background;
            btnDelete.FlatAppearance.BorderColor = AppTheme.BackgroundEnd;
            btnDelete.FlatAppearance.MouseOverBackColor = AppTheme.DarkAccent;
            btnDelete.FlatAppearance.MouseDownBackColor = AppTheme.BackgroundEnd;

            if (editMenu != null)
            {
                editMenu.BackColor = AppTheme.Background;
                editMenu.ForeColor = AppTheme.Accent;
                foreach (ToolStripItem item in editMenu.Items)
                    item.ForeColor = AppTheme.Accent;
            }

            TlpCableCard.Invalidate(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 실제 데이터 삭제는 화면 전체를 관리하는 Main 폼에서 수행한다.
            DeleteRequested?.Invoke(Info);
        }

        public Bitmap RenderForExport()
        {
            // 삭제 버튼과 바깥 여백을 제외하고 실제 정보 표 영역만 출력한다.
            Bitmap bitmap = new Bitmap(ExportSize.Width, ExportSize.Height);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using Bitmap tableBitmap = new Bitmap(TlpCableCard.Width, TlpCableCard.Height);
            TlpCableCard.DrawToBitmap(
                tableBitmap,
                new Rectangle(Point.Empty, TlpCableCard.Size));
            graphics.DrawImageUnscaled(tableBitmap, ExportMargin, ExportMargin);

            // 카테고리 헤더와 같은 색상의 얇은 테두리로 출력 카드 종류를 구분한다.
            using Pen categoryPen = new Pen(
                ColorHelper.GetMapHeaderColor(Info.Category),
                2F);
            graphics.DrawRectangle(
                categoryPen,
                1,
                1,
                bitmap.Width - 3,
                bitmap.Height - 3);
            return bitmap;
        }

        private void tlpCard_Paint(object sender, PaintEventArgs e)
        {
            TableLayoutPanel? tlp = sender as TableLayoutPanel;

            if (tlp == null)
                return;

            // 화면 카드의 구분선은 카테고리별 색상 대신 공통 테마 강조색을 사용한다.
            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                tlp.Height - 2,
                2,
                AppTheme.Accent);
        }
    }
}
