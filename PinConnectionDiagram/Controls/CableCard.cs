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
    public partial class CableCard : UserControl
    {
        public event Action<CableInfo>? DeleteRequested;
        public CableInfo Info { get; }
        public CableCard(CableInfo info)
        {
            InitializeComponent();

            Info = info;

            lblCategory.Text = info.Category;
            lblName.Text = info.Name;
            lblCount.Text = info.Count.ToString();

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteRequested?.Invoke(Info);
        }

        private void tlpCard_Paint(object sender, PaintEventArgs e)
        {
            TableLayoutPanel? tlp = sender as TableLayoutPanel;

            if (tlp == null)
                return;

            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                tlp.Height - 2,
                2,
                Color.FromArgb(180, 100, 220, 255));

            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                tlp.Height - 2,
                2,
                Color.FromArgb(180, 100, 220, 255));
        }
    }
}