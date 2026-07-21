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
    /// 배치 영역에 놓인 케이블을 이동하거나 우클릭으로 삭제할 수 있게 한다.
    /// </summary>
    public partial class DropItem : UserControl
    {
        public CableInfo Info { get; }
        private bool dragging = false;
        private readonly bool allowMove;
        private Point offset;
        private Color _categoryColor;
        public event Action<CableInfo>? DeleteRequested;
        public DropItem(CableInfo info, bool allowMove = true)
        {
            InitializeComponent();

            this.allowMove = allowMove;
            RegisterMoveEvent(this);

            Info = info;

            LblCableName.Text = info.Name;

            ApplyTheme();

            // 목록의 CableItem과 동일하게 커넥터 영역과 강조선에만 색상을 표시한다.
            TlpCableItem.BackColor = Color.White;
            LblCableName.ForeColor = Color.Black;

            TlpCableItem.Paint += TlpCableItem_Paint;
        }

        /// <summary>
        /// 현재 테마의 카테고리 색상을 이미 배치된 항목에도 다시 적용한다.
        /// </summary>
        public void ApplyTheme()
        {
            _categoryColor = ColorHelper.GetMapHeaderColor(Info.Category);
            PbConnector.BackColor = _categoryColor;
            TlpCableItem.Invalidate();
        }

        private void RegisterMoveEvent(Control control)
        {
            // 항목 내부 어느 위치에서든 동일한 이동 동작을 시작할 수 있게 한다.
            control.MouseDown += DiagramCable_MouseDown;
            control.MouseMove += DiagramCable_MouseMove;
            control.MouseUp += DiagramCable_MouseUp;

            foreach (Control child in control.Controls)
            {
                RegisterMoveEvent(child);
            }
        }

        private void DiagramCable_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 삭제 확인 및 실제 제거는 상위 화면에서 처리한다.
                DeleteRequested?.Invoke(Info);
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            if (!allowMove)
                return;

            dragging = true;

            offset = PointToClient(Cursor.Position);
        }


        private void DiagramCable_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!dragging)
                return;
            Control? parent = Parent;
            if (parent == null)
                return;

            Point p = parent.PointToClient(Cursor.Position);

            int newX = p.X - offset.X;
            int newY = p.Y - offset.Y;

            // 항목이 부모 배치 영역 밖으로 벗어나지 않도록 좌표를 제한한다.
            if (newX < 0) 
                newX = 3;

            if (newY < 0)
                newY = 3;

            if (newX + Width > parent.ClientSize.Width)
                newX = parent.ClientSize.Width - Width-3;

            if (newY + Height > parent.ClientSize.Height) 
                newY = parent.ClientSize.Height - Height-3;

            Location = new Point(newX, newY);
        }

        private void DiagramCable_MouseUp(object? sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void TlpCableItem_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not TableLayoutPanel tlp)
                return;

            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                0,
                2,
                _categoryColor);
            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                tlp.Height - 2,
                2,
                _categoryColor);
        }
    }
}
