using PinConnectionDiagram.Helpers;
using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;

namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// 연결선에 실제 케이블을 할당하기 위한 드래그 앤 드롭 영역이다.
    /// </summary>
    public partial class ConnectionDropZone : Control
    {
        private IReadOnlyList<ConnectionLine> connectionLines;
        public event Action? CableAssignmentChanged;

        public ConnectionDropZone(IReadOnlyList<ConnectionLine> connectionLines)
        {
            this.connectionLines = connectionLines;

            AllowDrop = true;
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            Size = new Size(120, 34);

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            DragEnter += ConnectionDropZone_DragEnter;
            DragDrop += ConnectionDropZone_DragDrop;
            MouseUp += ConnectionDropZone_MouseUp;
        }

        // 분기선 구성이 바뀌면 DropZone이 관리할 선 목록과 표시 아이템을 동기화한다.
        public void UpdateConnectionLines(IReadOnlyList<ConnectionLine> lines)
        {
            connectionLines = lines;
            UpdateCableDisplay();
            Invalidate();
        }

        // 현재 선 종류와 같은 카테고리의 CableItem만 복사 드롭을 허용한다.
        private void ConnectionDropZone_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(typeof(CableItem)) is not CableItem cableItem)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = IsMatchingCategory(cableItem.Info)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        // 드롭한 준비물을 같은 분기 그룹의 모든 연결선에 함께 할당한다.
        private void ConnectionDropZone_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(typeof(CableItem)) is not CableItem cableItem)
                return;

            if (!IsMatchingCategory(cableItem.Info))
                return;

            foreach (ConnectionLine line in connectionLines)
            {
                line.CableInfo = cableItem.Info;
            }

            UpdateCableDisplay();
            Invalidate();
            CableAssignmentChanged?.Invoke();
            QueueFinalDisplayRefresh();
        }

        // 우클릭하면 해당 분기 그룹에 배정된 준비물을 해제한다.
        private void ConnectionDropZone_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || GetCableInfo() == null)
                return;

            foreach (ConnectionLine line in connectionLines)
            {
                line.CableInfo = null;
            }

            UpdateCableDisplay();
            Invalidate();
            CableAssignmentChanged?.Invoke();
        }

        // 현재 배정 데이터에 맞춰 DropItem을 생성·교체하고 긴 명칭에 맞게 너비를 조정한다.
        private void UpdateCableDisplay()
        {
            CableInfo? cableInfo = GetCableInfo();
            DropItem? currentItem = Controls.OfType<DropItem>().FirstOrDefault();

            if (currentItem != null &&
                cableInfo != null &&
                ReferenceEquals(currentItem.Info, cableInfo))
            {
                ApplyDisplaySize(currentItem);
                currentItem.Visible = true;
                currentItem.BringToFront();
                currentItem.Invalidate(true);
                return;
            }

            if (currentItem != null)
            {
                Controls.Remove(currentItem);
                currentItem.Dispose();
            }

            if (cableInfo == null)
            {
                Size = new Size(120, CableDisplayHelper.NormalItemHeight);
                return;
            }

            // 기존 DropItem을 이동 기능 없이 사용해 목록과 동일한 디자인을 유지한다.
            DropItem dropItem = new DropItem(cableInfo, false);
            ApplyDisplaySize(dropItem);
            dropItem.Dock = DockStyle.Fill;
            dropItem.DeleteRequested += _ => ClearCableAssignment();
            Controls.Add(dropItem);
            dropItem.Visible = true;
            dropItem.BringToFront();
            dropItem.Invalidate(true);
        }

        /// <summary>
        /// 오버레이 그룹 재구성 후에도 현재 배정 아이템이 표시되도록 최종 상태를 맞춘다.
        /// </summary>
        public void RefreshCableDisplay()
        {
            UpdateCableDisplay();
            Visible = true;

            DropItem? dropItem = Controls.OfType<DropItem>().FirstOrDefault();
            if (dropItem != null)
            {
                dropItem.Visible = true;
                dropItem.BringToFront();
                dropItem.Invalidate(true);
            }

            Invalidate(true);
        }

        private void QueueFinalDisplayRefresh()
        {
            if (IsDisposed || !IsHandleCreated)
                return;

            // DragDrop 이벤트 안에서 어댑터 그룹이 합쳐지면 DropZone이 교체될 수 있다.
            // 이벤트가 끝난 뒤 살아 있는 최종 컨트롤만 다시 그려 잔상·누락을 방지한다.
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                RefreshCableDisplay();
                BringToFront();
                Update();
            }));
        }

        private void ApplyDisplaySize(DropItem dropItem)
        {
            // DropZone은 실제로 표시되는 DropItem과 동일한 크기를 사용한다.
            Size = dropItem.Size;
        }

        // DropItem의 삭제 요청과 우클릭 해제를 동일한 데이터 정리 흐름으로 처리한다.
        private void ClearCableAssignment()
        {
            foreach (ConnectionLine line in connectionLines)
            {
                line.CableInfo = null;
            }

            UpdateCableDisplay();
            Invalidate();
            CableAssignmentChanged?.Invoke();
        }

        // 연결선 종류를 준비물 카테고리 문자열로 변환해 잘못된 드롭을 차단한다.
        private bool IsMatchingCategory(CableInfo cableInfo)
        {
            ConnectorType connectorType = connectionLines[0].StartConnector.ConnectorType;

            string requiredCategory = connectorType switch
            {
                ConnectorType.Jig => "지그 케이블",
                ConnectorType.Adapter => "어댑터 케이블",
                ConnectorType.Test => "시험 대상 케이블",
                _ => string.Empty
            };

            return cableInfo.Category == requiredCategory;
        }

        // 그룹에 할당된 첫 준비물을 반환한다. 그룹 내 선들은 항상 같은 준비물을 공유한다.
        private CableInfo? GetCableInfo()
        {
            return connectionLines
                .Select(line => line.CableInfo)
                .FirstOrDefault(cableInfo => cableInfo != null);
        }

        // 준비물이 없을 때만 점선 테두리와 안내 문구를 그린다.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            CableInfo? cableInfo = GetCableInfo();
            if (cableInfo != null)
                return;

            using(Pen borderPen = new Pen(Color.Gray, 2))
            {
                borderPen.Alignment = PenAlignment.Inset;
                borderPen.DashStyle = DashStyle.Dash;

                e.Graphics.DrawRectangle(
                    borderPen,
                    0,
                    0,
                    Width - 1,
                    Height - 1);
            }

            string text = "케이블 배치";
            TextRenderer.DrawText(
                e.Graphics,
                text,
                Font,
                new Rectangle(10, 0, Width - 12, Height),
                Color.Black,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis);
        }
    }
}
