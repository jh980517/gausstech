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
    public partial class Connector : UserControl
    {
        // ComboBox(P1~)
        // 연결점(Point)
        private ConnectorSide side;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectorSide Side
        {
            get => side;

            set
            {
                side = value;

                UpdateSide();
            }
        }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ConnectorName
        {
            get => CmbPin.Text;
            set => CmbPin.Text = value;
        }

        public event Action<Connector>? PointClicked;
        public event Action<Connector>? HoverEnterd;
        public event Action<Connector>? HoverLeaved;

        public Connector()
        {
            InitializeComponent();

            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            CmbPin.Items.Clear();

            // [변경예정]CablePanel이 Pin 목록을 관리
            for (int i = 1; i <= 30; i++)
            {
                CmbPin.Items.Add($"P{i}");
            }

            CmbPin.MaxDropDownItems = 10;
            CmbPin.IntegralHeight = false;

            CmbPin.SelectedIndex = 0;
        }

        public Point GetConnectionPoint(Control parent)
        {
            Point p = PnlPoint.PointToScreen(
                new Point(PnlPoint.Width / 2, PnlPoint.Height / 2));

            return parent.PointToClient(p);
        }

        private void UpdateSide()
        {
            if (Side == ConnectorSide.Left)
            {
                PnlPoint.BackgroundImage = Properties.Resources.connectorIcon_left;

                PnlPin.Dock = DockStyle.Fill;
                PnlPoint.Dock = DockStyle.Right;
            }
            else
            {
                PnlPin.BackgroundImage = Properties.Resources.connectorIcon_right;

                PnlPin.Dock = DockStyle.Fill;
                PnlPoint.Dock = DockStyle.Left;
            }

            PnlPin.BackgroundImageLayout = ImageLayout.Stretch;
        }


        private void PnlPoint_click(object sender, EventArgs e)
        {
            PointClicked?.Invoke(this);
        }
    }
}
