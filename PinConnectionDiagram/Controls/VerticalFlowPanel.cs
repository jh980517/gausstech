namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// 프로젝트 전용 세로 스크롤 디자인을 사용하며 자식 항목을 위에서 아래로 정렬한다.
    /// </summary>
    public partial class VerticalFlowPanel : VerticalScrollPanel
    {
        private const int ItemSpacing = 6;
        private bool isArrangingItems;
        private bool isFullRepaintQueued;

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            ArrangeItems();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            ArrangeItems();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ArrangeItems();
        }

        private void ArrangeItems()
        {
            if (!IsHandleCreated)
                return;

            if (isArrangingItems)
                return;

            isArrangingItems = true;
            try
            {
                int y = Padding.Top - Math.Abs(AutoScrollPosition.Y);
                int availableWidth = Math.Max(0, ClientSize.Width - Padding.Horizontal - 12);

                foreach (Control item in Controls.Cast<Control>().Where(control => control.Visible))
                {
                    int x = Padding.Left + Math.Max(0, (availableWidth - item.Width) / 2);
                    item.Location = new Point(x, y);
                    y += item.Height + ItemSpacing;
                }

                int contentHeight = Math.Max(
                    0,
                    y + Math.Abs(AutoScrollPosition.Y) + Padding.Bottom - ItemSpacing);
                if (AutoScrollMinSize.Height != contentHeight)
                    AutoScrollMinSize = new Size(0, contentHeight);

                Invalidate(true);
            }
            finally
            {
                isArrangingItems = false;
            }
        }

        protected override void OnScroll(ScrollEventArgs scrollEvent)
        {
            base.OnScroll(scrollEvent);

            QueueFullRepaint();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 네이티브 스크롤이 복사한 이전 강조선을 남기지 않고 배경 전체를 먼저 지운다.
            e.Graphics.Clear(BackColor);
        }

        private void QueueFullRepaint()
        {
            if (isFullRepaintQueued || !IsHandleCreated || IsDisposed)
                return;

            isFullRepaintQueued = true;
            BeginInvoke(new Action(() =>
            {
                isFullRepaintQueued = false;
                if (IsDisposed)
                    return;

                Invalidate(true);
                Update();
            }));
        }
    }
}
