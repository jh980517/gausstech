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
        public CableItem(CableInfo info)
        {
            InitializeComponent();

            RegisterDrag(this);


            Info = info;

            LblCableName.Text = info.Name;
        }

        private void TlpCableItem_Paint(object sender, PaintEventArgs e)
        {
            TableLayoutPanel? tlp = sender as TableLayoutPanel;

            if (tlp == null)
                return;

            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, tlp.Height - 2, 2);
            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, 0, 2);
            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, tlp.Height - 2, 2);
            DrawHelper.DrawGlowLine(e.Graphics, tlp.Width, 0, 2);
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
