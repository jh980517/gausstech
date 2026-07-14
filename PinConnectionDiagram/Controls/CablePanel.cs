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
    public partial class CablePanel : UserControl
    {
        public ConnectorType Type { get; }
        public int TJNumber { get; }
        private readonly List<PinConnector> connectors = new();
        public IReadOnlyList<PinConnector> Connectors => connectors;
        public CablePanel()
        {
            InitializeComponent();
        }
    }
}
