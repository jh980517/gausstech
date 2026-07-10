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
    public partial class PinConnector : UserControl
    {
        public int TJNumber { get; }

        public string Category { get; }

        public ConnectorSide Side { get; }
        public PinConnector(int tjNumber, string category, ConnectorSide side)
        {
            InitializeComponent();

            TJNumber = tjNumber;
            Category = category;
        }

        private void PnlPoint_Click(object sender, EventArgs e)
        {

        }
    }
}
