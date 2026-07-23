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
    /// <summary>
    /// P 번호 선택 UI와 클릭 가능한 연결점을 하나의 커넥터로 표현한다.
    /// </summary>
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

        public event Action<Connector>? RightClicked;
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
            ConfigureNamePrefix("P");
        }

        /// <summary>
        /// 커넥터 용도에 맞춰 선택 가능한 명칭을 P1~P30 또는 J1~J30으로 구성한다.
        /// </summary>
        public void ConfigureNamePrefix(string prefix)
        {
            CmbPin.BeginUpdate();
            try
            {
                CmbPin.Items.Clear();
                for (int i = 1; i <= 30; i++)
                    CmbPin.Items.Add($"{prefix}{i}");

                CmbPin.MaxDropDownItems = 10;
                CmbPin.IntegralHeight = false;
                CmbPin.SelectedIndex = 0;
            }
            finally
            {
                CmbPin.EndUpdate();
            }
        }

        public Point GetConnectionPoint(Control parent)
        {
            // 중첩된 컨트롤 좌표를 화면 좌표를 거쳐 그리기 대상 좌표로 변환한다.
            Point p = PnlPoint.PointToScreen(
                new Point(PnlPoint.Width / 2, PnlPoint.Height / 2));

            return parent.PointToClient(p);
        }

        public Color ConnectionPointColor => PnlPoint.BackColor;

        public Size ConnectionPointSize => PnlPoint.Size;

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
            if (!Enabled)
            {
                PnlPoint.BackColor = Color.FromArgb(105, 110, 120);
                return;
            }

            // 선택 대기 상태가 연결 완료 상태보다 우선하여 표시된다.
            PnlPoint.BackColor = isConnectionPending
                ? Color.DodgerBlue
                : isConnected
                    ? Color.LimeGreen
                    : Color.Red;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            // TJ OFF 상태에서는 빨간 미연결점 대신 회색 연결점과 흐린 입력란으로 표시한다.
            CmbPin.BackColor = Enabled
                ? Color.White
                : Color.FromArgb(205, 208, 214);
            CmbPin.ForeColor = Enabled
                ? Color.Black
                : Color.FromArgb(115, 120, 130);
            UpdateConnectionPointColor();
        }

        private void UpdateSide()
        {
            // 연결점이 패널 내부를 향하도록 핀 이미지와 점의 열 위치를 교환한다.
            TlpPin.SuspendLayout();

            TlpPin.Controls.Remove(PnlPin);
            TlpPin.Controls.Remove(PnlPoint);

            if (Side == ConnectorSide.Left)
            {
                PnlPoint.Anchor = AnchorStyles.Right;
                TlpPin.ColumnStyles[0].Width = 70;
                TlpPin.ColumnStyles[1].Width = 12;
                CmbPin.Location = new Point(18, 6); // 기존 위치

                TlpPin.Controls.Add(PnlPin, 0, 0);
                TlpPin.Controls.Add(PnlPoint, 1, 0);

                PnlPin.BackgroundImage = Properties.Resources.connectorIcon_left;
            }
            else
            {
                PnlPoint.Anchor = AnchorStyles.Left;
                TlpPin.ColumnStyles[0].Width = 12;
                TlpPin.ColumnStyles[1].Width = 70;
                CmbPin.Location = new Point(0, 6);

                TlpPin.Controls.Add(PnlPoint, 0, 0);
                TlpPin.Controls.Add(PnlPin, 1, 0);

                PnlPin.BackgroundImage = Properties.Resources.connectorIcon_right;
            }

            PnlPin.BackgroundImageLayout = ImageLayout.Stretch;

            TlpPin.ResumeLayout();
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
            // 콤보박스와 이미지 위의 우클릭도 커넥터 우클릭으로 취급한다.
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
