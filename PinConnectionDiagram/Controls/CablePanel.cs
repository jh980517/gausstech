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
        private bool addButtonDisplayRequested = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAddButton
        {
            get => showAddButton;
            set
            {
                showAddButton = value;
                UpdateAddButtonVisibility();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AddButtonVisible
        {
            get => BtnAdd.Visible;
            set
            {
                // 출력 캡처에서 사용하는 임시 표시 상태를 패널 활성 상태와 분리한다.
                addButtonDisplayRequested = value;
                UpdateAddButtonVisibility();
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
            BtnAdd.VisibleChanged += BtnAdd_VisibleChanged;

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
            // 어댑터 출력은 J, 나머지 커넥터는 P 접두사를 사용한다.
            Connector connector = new Connector();
            string namePrefix = GetConnectorNamePrefix(side);

            if (string.IsNullOrEmpty(connectorName))
            {
                int number = 
                    side == ConnectorSide.Left
                    ? leftConnectors.Count + 1
                    : PanelType == ConnectorType.Jig
                        ? leftConnectors.Count + rightConnectors.Count + 1
                        : rightConnectors.Count + 1;

                connectorName = $"{namePrefix}{number}";
            }
            else if (namePrefix == "J" && connectorName.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                // 이전 이력에 저장된 어댑터 출력 P 명칭도 새 J 규칙으로 복원한다.
                connectorName = $"J{connectorName[1..]}";
            }

            connector.Side = side;
            connector.ConnectorType = PanelType;
            connector.TJNumber = TJNumber;
            connector.ConfigureNamePrefix(namePrefix);
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
            // 삭제 후 각 방향의 명명 규칙에 맞춰 번호를 연속으로 다시 부여한다.
            for (int i = 0; i < leftConnectors.Count; i++)
            {
                leftConnectors[i].ConnectorName = $"{GetConnectorNamePrefix(ConnectorSide.Left)}{i + 1}";
            }

            for (int i = 0; i < rightConnectors.Count; i++)
            {
                int number = PanelType == ConnectorType.Jig
                    ? leftConnectors.Count + i + 1
                    : i + 1;
                rightConnectors[i].ConnectorName =
                    $"{GetConnectorNamePrefix(ConnectorSide.Right)}{number}";
            }
        }

        private string GetConnectorNamePrefix(ConnectorSide side) =>
            PanelType == ConnectorType.Adapter && side == ConnectorSide.Right
                ? "J"
                : "P";

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
            else if (PanelType == ConnectorType.Adapter && leftConnectors.Count > 0)
            {
                // Adapter Left는 맞닿는 Jig Right와 동일한 중앙 정렬 규칙을 사용한다.
                int startY = GetCenteredConnectorStartY(
                    leftConnectors.Count,
                    leftConnectors[0].Height,
                    gap);
                for (int i = 0; i < leftConnectors.Count; i++)
                {
                    leftConnectors[i].Location =
                        new Point(sideMargin, startY + i * gap);
                }
            }
            else
            {
                for (int i = 0; i < leftConnectors.Count; i++)
                {
                    Connector connector = leftConnectors[i];

                    connector.Location = new Point(sideMargin, top + i * gap);
                }
            }

            int rightStartY = PanelType == ConnectorType.Jig &&
                rightConnectors.Count > 0
                ? GetCenteredConnectorStartY(
                    rightConnectors.Count,
                    rightConnectors[0].Height,
                    gap)
                : top;
            for (int i = 0; i < rightConnectors.Count; i ++)
            {
                Connector connector = rightConnectors[i];

                int x = PnlCanvas.ClientSize.Width - connector.Width - sideMargin;

                connector.Location =
                    new Point(Math.Max(0, x), rightStartY + i * gap);
            }

            BtnAdd.BringToFront();
        }

        private int GetCenteredConnectorStartY(
            int connectorCount,
            int connectorHeight,
            int gap)
        {
            int listHeight = connectorHeight + Math.Max(0, connectorCount - 1) * gap;
            return Math.Max(0, (PnlCanvas.ClientSize.Height - listHeight) / 2);
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

            UpdateAddButtonVisibility();

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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            // 부모 컨트롤의 활성 상태나 창 재구성으로 Enabled가 바뀌어도
            // 추가 버튼의 표시 상태가 패널 상태와 어긋나지 않게 한다.
            if (BtnAdd != null)
                UpdateAddButtonVisibility();
        }

        private void BtnAdd_VisibleChanged(object? sender, EventArgs e)
        {
            // WinForms의 상태 복원 및 재부모화 과정에서 잘못 표시된 경우를
            // 즉시 차단한다. 비활성 버튼의 잔상도 다음 페인트 전에 제거된다.
            if (BtnAdd.Visible &&
                (!Enabled || !showAddButton || !addButtonDisplayRequested))
            {
                UpdateAddButtonVisibility();
            }
        }

        private void UpdateAddButtonVisibility()
        {
            bool canUseAddButton = Enabled && showAddButton;
            bool shouldDisplay = canUseAddButton && addButtonDisplayRequested;

            if (!canUseAddButton)
            {
                // 비활성 부모 안에 숨겨진 네이티브 버튼 창이 작업 창 전환 후
                // 다시 그려지는 잔상을 막기 위해 컨트롤 트리에서도 분리한다.
                if (BtnAdd.Parent == PnlCanvas)
                {
                    Rectangle previousBounds = BtnAdd.Bounds;
                    BtnAdd.Visible = false;
                    PnlCanvas.Controls.Remove(BtnAdd);
                    PnlCanvas.Invalidate(previousBounds, true);
                }

                return;
            }

            if (BtnAdd.Parent != PnlCanvas)
            {
                PnlCanvas.Controls.Add(BtnAdd);
                BtnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                BtnAdd.Location = new Point(
                    Math.Max(0, PnlCanvas.ClientSize.Width - BtnAdd.Width),
                    0);
            }

            BtnAdd.Visible = shouldDisplay;
            if (shouldDisplay)
                BtnAdd.BringToFront();
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
