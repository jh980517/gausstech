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
        private bool isConnectionPending;
        private bool isConnected;

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
        public ConnectorType ConnectorType { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TJNumber { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ConnectorName
        {
            get => CmbPin.Text;
            set => CmbPin.Text = value;
        }

        public event Action<Connector>? HoverEnterd;
        public event Action<Connector>? HoverLeaved;
        public event Action<Connector>? RightClicked;
        public event Action<Connector>? DeleteRequested;
        public event Action<Connector>? PointClicked;
        public event Action<Connector>? ConnectorNameChanged;

        public Connector()
        {
            InitializeComponent();

            InitializeComboBox();

            CmbPin.SelectedIndexChanged += CmbPin_SelectedIndexChanged;

            RegisterMouseEvents(this);
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

        public void SetConnectionPending(bool pending)
        {
            isConnectionPending = pending;
            UpdateConnectionPointColor();
        }

        public void SetConnected(bool connected)
        {
            isConnected = connected;
            UpdateConnectionPointColor();
        }

        private void UpdateConnectionPointColor()
        {
            PnlPoint.BackColor = isConnectionPending
                ? Color.DodgerBlue
                : isConnected
                    ? Color.LimeGreen
                    : Color.Red;
        }

        private void UpdateSide()
        {
            TlpPin.SuspendLayout();

            TlpPin.Controls.Remove(PnlPin);
            TlpPin.Controls.Remove(PnlPoint);

            if (Side == ConnectorSide.Left)
            {
                TlpPin.ColumnStyles[0].Width = 70;
                TlpPin.ColumnStyles[1].Width = 12;
                CmbPin.Location = new Point(18, 6); // 기존 위치

                TlpPin.Controls.Add(PnlPin, 0, 0);
                TlpPin.Controls.Add(PnlPoint, 1, 0);

                PnlPin.BackgroundImage = Properties.Resources.connectorIcon_left;
            }
            else
            {
                TlpPin.ColumnStyles[0].Width = 12;
                TlpPin.ColumnStyles[1].Width = 70;
                CmbPin.Location = new Point(0, 6);

                TlpPin.Controls.Add(PnlPoint, 0, 0);
                TlpPin.Controls.Add(PnlPin, 1, 0);

                PnlPin.BackgroundImage = Properties.Resources.connectorIcon_right;
            }

            PnlPin.BackgroundImageLayout = ImageLayout.Stretch;

            TlpPin.ResumeLayout();
            //if (Side == ConnectorSide.Left)
            //{
            //    PnlPoint.BackgroundImage = Properties.Resources.connectorIcon_left;

            //    PnlPin.Dock = DockStyle.Fill;

            //}
            //else
            //{
            //    PnlPin.BackgroundImage = Properties.Resources.connectorIcon_right;

            //    PnlPin.Dock = DockStyle.Fill;
            //}

            //PnlPin.BackgroundImageLayout = ImageLayout.Stretch;
        }


        private void PnlPoint_Click(object sender, EventArgs e)
        {
            PointClicked?.Invoke(this);
        }

        private void CmbPin_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ConnectorNameChanged?.Invoke(this);
        }

        private void RegisterMouseEvents(Control control)
        {
            control.MouseUp += Connector_MouseUp;

            foreach(Control child in control.Controls)
            {
                RegisterMouseEvents(child);
            }
        }

        private void Connector_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            RightClicked?.Invoke(this);
        }
    }
}
