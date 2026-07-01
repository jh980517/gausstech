using PinConnectionDiagram.Models;
using PinConnectionDiagram.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PinConnectionDiagram.Controls
{
    public partial class CableItem : UserControl
    {
        public CableInfo Info { get; }

        private readonly Color _categoryColor;
        public CableItem(CableInfo info)
        {
            InitializeComponent();

            RegisterDrag(this);

            Info = info;

            LblCableName.Text = info.Name;

            _categoryColor = ColorHelper.GetCategoryColor(info.Category);

            PbConnector.BackColor = _categoryColor;

            TlpCableItem.Paint += TlpCableItem_Paint;
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

        private void RegisterDrag(Control control)
        {
            control.MouseDown += CableItem_MouseDown;

            foreach (Control child in control.Controls)
            {
                RegisterDrag(child);
            }
        }

        private void CableItem_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(this, DragDropEffects.Copy);
        }
    }
}
