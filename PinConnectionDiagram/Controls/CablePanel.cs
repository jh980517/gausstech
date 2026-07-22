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
    /// <summary>
    /// 하나의 TJ와 케이블 종류에 속한 좌우 커넥터를 생성, 배치, 삭제한다.
    /// </summary>
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AddButtonVisible
        {
            get => BtnAdd.Visible;
            set => BtnAdd.Visible = value;
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

        public void ApplyTheme()
        {
            ButtonHelper.ApplyButtonEffect(
                BtnAdd,
                AppTheme.GetImage("btnAdd_Up", "btnAdd_defense_Up"),
                AppTheme.GetImage("btnAdd_Down", "btnAdd_defense_Down"));
        }

        public Connector AddConnector(ConnectorSide side, string? connectorName = null)
        {
            // 이름을 지정하지 않으면 같은 방향에서 다음 순번의 P 번호를 사용한다.
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
            // 전달된 커넥터가 이 패널 소유가 아니면 아무 상태도 변경하지 않는다.
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
            // 삭제 후 화면에 P 번호가 연속되도록 방향별로 다시 부여한다.
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
            // BtnAdd는 PnlCanvas에 함께 있으므로 커넥터만 선택적으로 제거해야 한다.
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
            // 좌우 커넥터는 독립된 세로 목록으로 배치한다.
            const int top = 25;
            const int gap = 45;
            const int sideMargin = 0;

            if (PanelType == ConnectorType.Jig && leftConnectors.Count == 1)
            {
                // Jig의 고정 입력 커넥터는 행 높이와 관계없이 세로 중앙에 둔다.
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
            // TJ 상태에 맞춰 입력, 추가 버튼, 배경색을 한 번에 갱신한다.
            Enabled = active;

            PnlCanvas.Enabled = active;

            BtnAdd.Visible = active && showAddButton;

            BackColor = active
                ? AppTheme.ContentBackground
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

        private void BtnAdd_Click(object? sender, EventArgs e)
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
            // 자동 생성되는 왼쪽 및 Test 커넥터는 사용자가 직접 삭제할 수 없다.
            if (PanelType == ConnectorType.Test)
            {
                return;
            }

            if (connector.Side == ConnectorSide.Left)
            {
                return;
            }

            ConnectorDeleteRequested?.Invoke(this, connector);
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
