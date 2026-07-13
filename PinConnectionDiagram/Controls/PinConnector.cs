using PinConnectionDiagram.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace PinConnectionDiagram.Controls
{
    public partial class PinConnector : UserControl
    {
        // ComboBox(P1~)
        // 연결점(Point)
        public string Category { get; }

        public ConnectorSide Side { get; private set; }
        public event Action<PinConnector>? PointClicked;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedPin
        {
            get => CmbPin.Text;
            set => CmbPin.Text = value;
        }

        public Point ConnectionPoint
        {
            get
            {
                return PnlPoint.PointToScreen(
                    new Point(
                        PnlPoint.Width / 2,
                        PnlPoint.Height / 2));
            }
        }
        public PinConnector()
        {
            InitializeComponent();

            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            CmbPin.Items.Clear();

            for (int i= 1; i <= 30; i++)
            {
                CmbPin.Items.Add($"P{i}");
            }

            CmbPin.MaxDropDownItems = 10;
            CmbPin.IntegralHeight = false;

            CmbPin.SelectedIndex = 0;
        }

        public void SetSide(ConnectorSide side)
        {
            if (Side == side)
                return;

            Side = side;

            ArrangeConnector();
            UpdateConnectorImage();
        }

        private void ArrangeConnector()
        {
            TlpPin.Controls.Clear();

            TlpPin.ColumnStyles[0].SizeType = SizeType.Absolute;
            TlpPin.ColumnStyles[1].SizeType = SizeType.Absolute;
            if (Side == ConnectorSide.Left)
            {
                TlpPin.ColumnStyles[0].Width = 68;
                TlpPin.ColumnStyles[1].Width = 12;

                TlpPin.Controls.Add(PnlPin, 0, 0);
                TlpPin.Controls.Add(PnlPoint, 1, 0);

                CmbPin.Location = new Point(14, 8);
            }
            else
            {
                TlpPin.ColumnStyles[0].Width = 12;
                TlpPin.ColumnStyles[1].Width = 68;

                TlpPin.Controls.Add(PnlPoint, 0, 0);
                TlpPin.Controls.Add(PnlPin, 1, 0);

                CmbPin.Location = new Point(3, 8);
            }
        }

        private void UpdateConnectorImage()
        {
            if (Side == ConnectorSide.Left)
            {
                PnlPin.BackgroundImage =
                    Properties.Resources.connectorIcon_left;
            }
            else
            {
                PnlPin.BackgroundImage =
                    Properties.Resources.connectorIcon_right;
            }

            PnlPin.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void PnlPoint_Click(object sender, EventArgs e)
        {
            PointClicked?.Invoke(this);
        }

        public void Highlight(bool selected)
        {
            PnlPoint.BackColor = selected
                ? Color.LimeGreen
                : Color.Red;
        }
    }
}
