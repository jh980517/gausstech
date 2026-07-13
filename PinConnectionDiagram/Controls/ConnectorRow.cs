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
    public partial class ConnectorRow : UserControl
    {
        // Left PinConnector
        // Right PinConnector
        public int TJNumber { get; }
        public ConnectorType Type { get; }
        public PinConnector LeftConnector { get; private set; }
        public PinConnector? RightConnector { get; private set; }
        public Panel DropZone => PnlCenter;
        public CableInfo? CableInfo { get; private set; }
        public ConnectorRow(int tjNumber, ConnectorType type)
        {
            InitializeComponent();

            TJNumber = TJNumber;
            Type = type;

            CreateConnectors();
        }

        // 커넥터 생성
        private void CreateConnectors()
        {
            LeftConnector = new PinConnector();
            LeftConnector.SetSide(ConnectorSide.Left);

            LeftConnector.Anchor = AnchorStyles.None;
            LeftConnector.Margin = new Padding(0);
            LeftConnector.Size = new Size(80, 45);

            TlpRow.Controls.Add(LeftConnector, 0, 0);

            if (Type != ConnectorType.Test)
            {
                RightConnector = new PinConnector();
                RightConnector.SetSide(ConnectorSide.Right);

                RightConnector.Anchor = AnchorStyles.None;
                RightConnector.Margin = new Padding(0);
                RightConnector.Size = new Size(80, 45);

                TlpRow.Controls.Add(RightConnector, 2, 0);
            }
        }

        public void SetConnectorVisible(bool visible)
        {
            LeftConnector.Visible = visible;

            if (RightConnector != null)
            {
                RightConnector.Visible = visible;
            }
            DropZone.Visible = visible;
        }
    }
}
