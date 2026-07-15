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
        private readonly List<Connector> leftConnectors = new();
        private readonly List<Connector> rightConnectors = new();
        public IReadOnlyList<Connector> LeftConnectors => leftConnectors;
        public IReadOnlyList<Connector> RightConnectors => rightConnectors;
        
        public CablePanel(int tjNumber, ConnectorType type)
        {
            InitializeComponent();

            TJNumber = tjNumber;
            Type = type;

            DoubleBuffered = true;
        }

        public Connector AddConnector(ConnectorSide side, string? connectorName = null)
        {
            Connector connector = new Connector();

            connector.Side = side;

            if (connectorName != null)
            {
                connector.ConnectorName = connectorName;
            }

            if (side == ConnectorSide.Left)
            {
                leftConnectors.Add(connector);
            }
            else
            {
                rightConnectors.Add(connector);
            }
            
            PnlCanvas.Controls.Add(connector);

            RefreshLayout();

            return connector;
        }

        public void RemoveConnector(Connector connector)
        {
            leftConnectors.Remove(connector);
            rightConnectors.Remove(connector);         

            PnlCanvas.Controls.Remove(connector);

            connector.Dispose();

            RefreshLayout();
        }

        public void ClearConnector()
        {
            foreach (Connector connector in leftConnectors)
            {
                connector.Dispose();
            }
            
            foreach (Connector connector in rightConnectors)
            {
                connector.Dispose();
            }

            leftConnectors.Clear();
            rightConnectors.Clear();

            PnlCanvas.Controls.Clear();
        }

        private void RefreshLayout()
        {
            int top = 10;
            int gap = 45;

            for (int i = 0; i < leftConnectors.Count; i++)
            {
                leftConnectors[i].Location = new Point(10, top + i * gap);
            }

            for (int i = 0; i < rightConnectors.Count; i++) 
            {
                rightConnectors[i].Location = new Point(PnlCanvas.Width - rightConnectors[i].Width - 10, top + i * gap);
            }
        }

        private void PnlCanvas_SizeChanged(object sender, EventArgs e)
        {
            RefreshLayout();
        }
    }
}
