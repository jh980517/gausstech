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
        public event EventHandler DeleteRequested;
        public CableCard(CableInfo info)
        {
            InitializeComponent();

            lblCategory.Text = info.Category;
            lblName.Text = info.Name;
            lblCount.Text = info.Count.ToString();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}