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
    public partial class CablePanel : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectorType PanelType { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TJNumber { get; set; }
        private readonly List<Connector> leftConnectors = new();
        private readonly List<Connector> rightConnectors = new();
        public IReadOnlyList<Connector> LeftConnectors => leftConnectors;
        public IReadOnlyList<Connector> RightConnectors => rightConnectors;
        public event Action<CablePanel>? AddRightConnectorRequested;
        public event Action<CablePanel>? RemoveRightConnectorRequested;
        public event Action<CablePanel, Connector>? ConnectorDeleteRequested;
        public event Action<Connector>? ConnectorPointClicked;
        public event Action<Connector>? ConnectorNameChanged;

        private bool showAddButton = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAddButton
        {
            get => showAddButton;
            set
            {
                showAddButton = value;
                BtnAdd.Visible = Enabled && showAddButton;
            }
        }
        
        public CablePanel(int tjNumber, ConnectorType type)
        {

            InitializeComponent();

            TJNumber = tjNumber;
            PanelType = type;

            DoubleBuffered = true;

            Enabled = false;

            SetActive(false);

            BtnAdd.Visible = false;

            BtnAdd.Click += BtnAdd_Click;

            ButtonHelper.ApplyButtonEffect(
                BtnAdd,
                Properties.Resources.btnAdd_Up,
                Properties.Resources.btnAdd_Down);
        }

        public Connector AddConnector(ConnectorSide side, string? connectorName = null)
        {
            Connector connector = new Connector();

            if (string.IsNullOrEmpty(connectorName))
            {
                int number = 
                    side == ConnectorSide.Left
                    ? leftConnectors.Count + 1
                    : rightConnectors.Count + 1;

                connectorName = $"P{number}";
            }

            connector.Side = side;
            connector.ConnectorType = PanelType;
            connector.TJNumber = TJNumber;
            connector.ConnectorName = connectorName;

            connector.RightClicked += Connector_RightClicked;
            connector.PointClicked += Connector_PointClicked;
            connector.ConnectorNameChanged += Connector_ConnectorNameChanged;

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

        public bool RemoveConnector(Connector connector)
        {
            bool removed = leftConnectors.Remove(connector);

            if (!removed)
            {
                removed = rightConnectors.Remove(connector);
            }

            if (!removed)
            {
                return false;
            }

            connector.RightClicked -= Connector_RightClicked;


            PnlCanvas.Controls.Remove(connector);

            connector.Dispose();

            RenumberConnectors();
            RefreshLayout();

            return true;
        }

        private void RenumberConnectors()
        {
            for (int i = 0; i < leftConnectors.Count; i++)
            {
                leftConnectors[i].ConnectorName = $"P{i + 1}";
            }

            for (int i = 0; i < rightConnectors.Count; i++)
            {
                rightConnectors[i].ConnectorName = $"P{i + 1}";
            }
        }

        public void ClearConnector()
        {
            foreach (Connector connector in leftConnectors.Concat(rightConnectors).ToList())
            {
                connector.RightClicked -= Connector_RightClicked;
                connector.PointClicked -= Connector_PointClicked;
                connector.ConnectorNameChanged -= Connector_ConnectorNameChanged;
                PnlCanvas.Controls.Remove(connector);
                connector.Dispose();
            }

            leftConnectors.Clear();
            rightConnectors.Clear();

            RefreshLayout();
        }

        private void RefreshLayout()
        {
            const int top = 25;
            const int gap = 45;
            const int sideMargin = 0;

            if (PanelType == ConnectorType.Jig && leftConnectors.Count == 1)
            {
                Connector leftConnector = leftConnectors[0];

                int centerY = (PnlCanvas.ClientSize.Height - leftConnector.Height) / 2;

                leftConnector.Location = new Point(sideMargin, Math.Max(0, centerY));
            }
            else
            {
                for (int i = 0; i < leftConnectors.Count; i++)
                {
                    Connector connector = leftConnectors[i];

                    connector.Location = new Point(sideMargin, top + i * gap);
                }
            }

            for (int i = 0; i < rightConnectors.Count; i ++)
            {
                Connector connector = rightConnectors[i];

                int x = PnlCanvas.ClientSize.Width - connector.Width - sideMargin;

                connector.Location = new Point(Math.Max(0, x), top + i * gap);
            }

            BtnAdd.BringToFront();
        }

        private void PnlCanvas_SizeChanged(object sender, EventArgs e)
        {
            RefreshLayout();
        }

        public void SetActive(bool active)
        {
            //MessageBox.Show($"{Type} / TJ{TJNumber} / {active}");
            Enabled = active;

            PnlCanvas.Enabled = active;

            BtnAdd.Visible = active && showAddButton;
            //btnRemove.Visible = active;

            BackColor = active
                ? Color.FromArgb(212, 219, 230)
                : Color.Transparent;

            foreach (Connector connector in leftConnectors)
            {
                connector.Enabled = active;
            }

            foreach (Connector connector in rightConnectors)
            {
                connector.Enabled = active;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddRightConnectorRequested?.Invoke(this);
        }

        public int MaxConnectorCount
        {
            get
            {
                return Math.Max(
                    leftConnectors.Count,
                    rightConnectors.Count);
            }
        }

        private void Connector_RightClicked(Connector connector)
        {
            if (PanelType == ConnectorType.Test)
            {
                return;
            }

            if (connector.Side == ConnectorSide.Left)
            {
                return;
            }

            ConnectorDeleteRequested?.Invoke(this, connector);
            //MessageBox.Show($"{Type} / {connector.Side} / {connector.ConnectorName}");
        }

        private void Connector_PointClicked(Connector connector)
        {
            ConnectorPointClicked?.Invoke(connector);
        }

        private void Connector_ConnectorNameChanged(Connector connector)
        {
            ConnectorNameChanged?.Invoke(connector);
        }
    }
}
