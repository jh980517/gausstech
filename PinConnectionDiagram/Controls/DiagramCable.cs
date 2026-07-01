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
    public partial class DiagramCable : UserControl
    {
        public CableInfo Info { get; }
        private bool dragging = false;
        private Point offset;
        private readonly Color _categoryColor;
        public event Action<DiagramCable>? DeleteRequested;
        public DiagramCable(CableInfo info)
        {
            InitializeComponent();

            RegisterMoveEvent(this);

            Info = info;

            LblCableName.Text = info.Name;

            _categoryColor = ColorHelper.GetCategoryColor(info.Category);

            PbConnector.BackColor = _categoryColor;

            TlpCableItem.Paint += TlpCableItem_Paint;
        }

        private void RegisterMoveEvent(Control control)
        {
            control.MouseDown += DiagramCable_MouseDown;
            control.MouseMove += DiagramCable_MouseMove;
            control.MouseUp += DiagramCable_MouseUp;

            foreach (Control child in control.Controls)
            {
                RegisterMoveEvent(child);
            }
        }

        private void DiagramCable_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DeleteRequested?.Invoke(this);
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            dragging = true;

            offset = PointToClient(Cursor.Position);
        }


        private void DiagramCable_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragging)
                return;
            Point p = Parent.PointToClient(Cursor.Position);

            int newX = p.X - offset.X;
            int newY = p.Y - offset.Y;

            if (newX < 0) 
                newX = 0;

            if (newY < 0)
                newY = 0;

            if (newX + Width > Parent.ClientSize.Width)
                newX = Parent.ClientSize.Width - Width;

            if (newY + Height > Parent.ClientSize.Height) 
                newY = Parent.ClientSize.Height - Height;

            Location = new Point(newX, newY);
        }

        private void DiagramCable_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void TlpCableItem_Paint(object sender, PaintEventArgs e)
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
