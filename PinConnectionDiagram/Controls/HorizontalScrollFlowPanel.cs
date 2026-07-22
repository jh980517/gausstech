using System.Runtime.InteropServices;
using PinConnectionDiagram.Helpers;

namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// 준비물 카드를 한 줄로 배치하고 프로젝트 색상의 가로 스크롤바를 표시한다.
    /// </summary>
    public partial class HorizontalScrollFlowPanel : FlowLayoutPanel
    {
        private const int SbHorz = 0;
        private const int SbVert = 1;
        private const int ScrollBarHeight = 12;
        private const int ScrollMargin = 4;
        private const int MinimumThumbWidth = 30;
        private bool isUpdatingNativeScrollBars;
        private bool isDraggingThumb;
        private bool isThumbHovered;
        private int thumbDragOffset;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr windowHandle, int scrollBar, bool show);

        public HorizontalScrollFlowPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            HideNativeScrollBars();
        }

        protected override void OnLayout(LayoutEventArgs layoutEvent)
        {
            base.OnLayout(layoutEvent);
            HideNativeScrollBars();
            InvalidateScrollBar();
        }

        protected override void OnScroll(ScrollEventArgs scrollEvent)
        {
            base.OnScroll(scrollEvent);
            if (scrollEvent.ScrollOrientation == ScrollOrientation.VerticalScroll)
                AutoScrollPosition = new Point(GetScrollPosition(), 0);

            HideNativeScrollBars();
            InvalidateScrollBar();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int direction = e.Delta > 0 ? -1 : 1;
            ScrollTo(GetScrollPosition() + direction * 70);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle thumb = GetThumbRectangle();
            if (thumb.IsEmpty)
                return;

            using SolidBrush trackBrush = new SolidBrush(AppTheme.DarkAccent);
            using SolidBrush thumbBrush = new SolidBrush(
                isThumbHovered || isDraggingThumb
                    ? AppTheme.AccentHover
                    : AppTheme.Accent);
            DrawPill(e.Graphics, trackBrush, GetTrackRectangle());
            DrawPill(e.Graphics, thumbBrush, thumb);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Rectangle thumb = GetThumbRectangle();
            Rectangle track = GetTrackRectangle();
            if (e.Button == MouseButtons.Left && thumb.Contains(e.Location))
            {
                isDraggingThumb = true;
                thumbDragOffset = e.X - thumb.X;
                Capture = true;
                InvalidateScrollBar();
                return;
            }

            if (e.Button == MouseButtons.Left && track.Contains(e.Location))
            {
                ScrollTo(GetScrollPosition() + (e.X < thumb.X ? -ClientSize.Width : ClientSize.Width));
                return;
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Rectangle thumb = GetThumbRectangle();
            bool hovered = thumb.Contains(e.Location);
            if (hovered != isThumbHovered)
            {
                isThumbHovered = hovered;
                InvalidateScrollBar();
            }

            if (isDraggingThumb)
            {
                Rectangle track = GetTrackRectangle();
                int availableTrack = track.Width - thumb.Width;
                int thumbX = Math.Clamp(
                    e.X - thumbDragOffset,
                    track.Left,
                    track.Right - thumb.Width);
                int position = availableTrack <= 0
                    ? 0
                    : (int)Math.Round(
                        (thumbX - track.Left) / (double)availableTrack * GetScrollMaximum());
                ScrollTo(position);
                return;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDraggingThumb)
            {
                isDraggingThumb = false;
                Capture = false;
                InvalidateScrollBar();
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isThumbHovered = false;
            InvalidateScrollBar();
            base.OnMouseLeave(e);
        }

        private Rectangle GetTrackRectangle() => new Rectangle(
            ScrollMargin,
            ClientSize.Height - ScrollBarHeight,
            Math.Max(0, ClientSize.Width - ScrollMargin * 2),
            ScrollBarHeight - ScrollMargin);

        private Rectangle GetThumbRectangle()
        {
            int contentWidth = GetContentWidth();
            if (contentWidth <= ClientSize.Width || ClientSize.Width <= 0)
                return Rectangle.Empty;

            Rectangle track = GetTrackRectangle();
            if (track.Width <= 0 || track.Height <= 0)
                return Rectangle.Empty;

            int minimumThumbWidth = Math.Min(MinimumThumbWidth, track.Width);
            int thumbWidth = Math.Clamp(
                (int)Math.Round(track.Width * ClientSize.Width / (double)contentWidth),
                minimumThumbWidth,
                track.Width);
            int availableTrack = track.Width - thumbWidth;
            int scrollMaximum = GetScrollMaximum();
            if (scrollMaximum <= 0)
                return Rectangle.Empty;

            int thumbX = track.Left + (int)Math.Round(
                GetScrollPosition() / (double)scrollMaximum * availableTrack);
            return new Rectangle(thumbX, track.Y, thumbWidth, track.Height);
        }

        private int GetContentWidth()
        {
            if (Controls.Count == 0)
                return AutoScrollMinSize.Width;

            int childWidth = Controls.Cast<Control>()
                .Max(control => control.Right + GetScrollPosition()) + Padding.Right;
            return Math.Max(AutoScrollMinSize.Width, childWidth);
        }

        private int GetScrollMaximum() => Math.Max(0, GetContentWidth() - ClientSize.Width);
        private int GetScrollPosition() => Math.Abs(AutoScrollPosition.X);

        private void ScrollTo(int position)
        {
            AutoScrollPosition = new Point(Math.Clamp(position, 0, GetScrollMaximum()), 0);
            HideNativeScrollBars();
            InvalidateScrollBar();
        }

        private static void DrawPill(Graphics graphics, Brush brush, Rectangle rectangle)
        {
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
                return;

            int radius = rectangle.Height;
            graphics.FillRectangle(
                brush,
                rectangle.X + radius / 2,
                rectangle.Y,
                Math.Max(0, rectangle.Width - radius),
                rectangle.Height);
            graphics.FillEllipse(brush, rectangle.X, rectangle.Y, radius, radius);
            graphics.FillEllipse(brush, rectangle.Right - radius, rectangle.Y, radius, radius);
        }

        private void InvalidateScrollBar() => Invalidate(new Rectangle(
            0,
            Math.Max(0, ClientSize.Height - ScrollBarHeight - 1),
            ClientSize.Width,
            ScrollBarHeight + 1));

        private void HideNativeScrollBars()
        {
            if (!IsHandleCreated || isUpdatingNativeScrollBars)
                return;

            try
            {
                isUpdatingNativeScrollBars = true;
                VerticalScroll.Enabled = false;
                VerticalScroll.Visible = false;
                ShowScrollBar(Handle, SbHorz, false);
                ShowScrollBar(Handle, SbVert, false);
            }
            finally
            {
                isUpdatingNativeScrollBars = false;
            }
        }
    }
}
