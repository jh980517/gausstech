using System.Runtime.InteropServices;
using PinConnectionDiagram.Helpers;

namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// AutoScroll을 사용하면서 세로 스크롤바만 표시하는 패널이다.
    /// </summary>
    public partial class VerticalScrollPanel : Panel
    {
        private const int SbHorz = 0;
        private const int SbVert = 1;
        private const int GwlStyle = -16;
        private const int WmStyleChanging = 0x007C;
        private const int WsHScroll = 0x00100000;
        private const int WsVScroll = 0x00200000;
        private const int ScrollBarWidth = 12;
        private const int ScrollMargin = 4;
        private const int MinimumThumbHeight = 30;

        private bool isUpdatingNativeScrollBars;
        private bool isDraggingThumb;
        private bool isThumbHovered;
        private int thumbDragOffset;

        private Color ScrollTrackColor => AppTheme.DarkAccent;
        private Color ScrollThumbColor => AppTheme.Accent;
        private Color ScrollThumbHoverColor => AppTheme.AccentHover;

        public VerticalScrollPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StyleChange
        {
            public int OldStyle;
            public int NewStyle;
        }

        /// <summary>
        /// 핸들이 처음 생성될 때부터 Win32 기본 스크롤바 스타일을 제외해
        /// 커스텀 스크롤바로 교체되기 전 기본 UI가 잠깐 노출되지 않게 한다.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.Style &= ~(WsHScroll | WsVScroll);
                return parameters;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(
            IntPtr windowHandle,
            int scrollBar,
            bool show);

        // 네이티브 메시지 처리 후 기본 스크롤바가 다시 나타나지 않게 즉시 숨긴다.
        protected override void WndProc(ref Message message)
        {
            // AutoScroll이 콘텐츠 크기 변경 중 WS_VSCROLL/WS_HSCROLL을 다시 추가하려는
            // 순간에도 스타일 적용 전에 제거해 네이티브 스크롤바 페인트 자체를 막는다.
            if (message.Msg == WmStyleChanging &&
                message.WParam.ToInt32() == GwlStyle &&
                message.LParam != IntPtr.Zero)
            {
                StyleChange styleChange =
                    Marshal.PtrToStructure<StyleChange>(message.LParam);
                styleChange.NewStyle &= ~(WsHScroll | WsVScroll);
                Marshal.StructureToPtr(styleChange, message.LParam, false);
            }

            base.WndProc(ref message);
            HideNativeScrollBars();
        }

        // 자식 레이아웃이 달라지면 사용자 정의 스크롤바의 길이와 위치를 다시 계산한다.
        protected override void OnLayout(LayoutEventArgs layoutEvent)
        {
            base.OnLayout(layoutEvent);
            HideNativeScrollBars();
            InvalidateScrollBar();
        }

        // 가로 스크롤 입력은 무시하고 세로 위치만 유지한다.
        protected override void OnScroll(ScrollEventArgs scrollEvent)
        {
            base.OnScroll(scrollEvent);

            if (scrollEvent.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                int verticalPosition = Math.Abs(AutoScrollPosition.Y);
                AutoScrollPosition = new Point(0, verticalPosition);
            }

            HideNativeScrollBars();
            InvalidateScrollBar();
        }

        // 스크롤이 필요한 경우에만 트랙과 손잡이를 프로젝트 색상으로 그린다.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle thumbRectangle = GetThumbRectangle();
            if (thumbRectangle.IsEmpty)
                return;

            Rectangle trackRectangle = GetTrackRectangle();

            using(SolidBrush trackBrush = new SolidBrush(ScrollTrackColor))
            using(SolidBrush thumbBrush = new SolidBrush(
                isThumbHovered || isDraggingThumb
                    ? ScrollThumbHoverColor
                    : ScrollThumbColor))
            {
                DrawPill(e.Graphics, trackBrush, trackRectangle);
                DrawPill(e.Graphics, thumbBrush, thumbRectangle);
            }
        }

        // 휠 한 칸을 일정한 픽셀 이동량으로 변환한다.
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int direction = e.Delta > 0 ? -1 : 1;
            ScrollTo(GetScrollPosition() + direction * 45);
        }

        // 자식 RichTextBox에서 전달된 휠 입력도 패널 스크롤에 반영한다.
        public void ScrollByWheelDelta(int delta)
        {
            // 자식 컨트롤이 휠 이벤트를 받은 경우에도 동일한 스크롤 동작을 제공한다.
            int direction = delta > 0 ? -1 : 1;
            ScrollTo(GetScrollPosition() + direction * 45);
        }

        // 손잡이 드래그를 시작하거나 트랙 클릭 시 한 화면 단위로 이동한다.
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Rectangle thumbRectangle = GetThumbRectangle();
            Rectangle trackRectangle = GetTrackRectangle();

            if (e.Button == MouseButtons.Left && thumbRectangle.Contains(e.Location))
            {
                isDraggingThumb = true;
                thumbDragOffset = e.Y - thumbRectangle.Y;
                Capture = true;
                InvalidateScrollBar();
                return;
            }

            if (e.Button == MouseButtons.Left && trackRectangle.Contains(e.Location))
            {
                int pageDirection = e.Y < thumbRectangle.Y ? -1 : 1;
                ScrollTo(GetScrollPosition() + pageDirection * ClientSize.Height);
                return;
            }

            base.OnMouseDown(e);
        }

        // 마우스 위치에 따라 호버 상태와 드래그 중인 스크롤 위치를 갱신한다.
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Rectangle thumbRectangle = GetThumbRectangle();
            bool wasHovered = isThumbHovered;
            isThumbHovered = thumbRectangle.Contains(e.Location);

            if (wasHovered != isThumbHovered)
            {
                InvalidateScrollBar();
            }

            if (!isDraggingThumb)
            {
                base.OnMouseMove(e);
                return;
            }

            Rectangle trackRectangle = GetTrackRectangle();
            int availableTrack = trackRectangle.Height - thumbRectangle.Height;
            int thumbY = Math.Clamp(
                e.Y - thumbDragOffset,
                trackRectangle.Top,
                trackRectangle.Bottom - thumbRectangle.Height);

            int scrollMaximum = GetScrollMaximum();
            int scrollPosition = availableTrack <= 0
                ? 0
                : (int)Math.Round(
                    (thumbY - trackRectangle.Top) /
                    (double)availableTrack * scrollMaximum);

            ScrollTo(scrollPosition);
        }

        // 마우스 버튼을 놓으면 손잡이 캡처와 드래그 상태를 해제한다.
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

        // 포인터가 패널을 벗어나면 손잡이 호버 강조를 제거한다.
        protected override void OnMouseLeave(EventArgs e)
        {
            isThumbHovered = false;
            InvalidateScrollBar();
            base.OnMouseLeave(e);
        }

        // 패널 우측에 표시할 세로 트랙 영역을 계산한다.
        private Rectangle GetTrackRectangle()
        {
            return new Rectangle(
                ClientSize.Width - ScrollBarWidth,
                ScrollMargin,
                ScrollBarWidth - ScrollMargin,
                Math.Max(0, ClientSize.Height - ScrollMargin * 2));
        }

        // 콘텐츠 대비 화면 비율과 현재 위치를 이용해 손잡이 사각형을 계산한다.
        private Rectangle GetThumbRectangle()
        {
            int contentHeight = GetContentHeight();
            int viewportHeight = ClientSize.Height;

            if (contentHeight <= viewportHeight || viewportHeight <= 0)
                return Rectangle.Empty;

            Rectangle trackRectangle = GetTrackRectangle();
            int thumbHeight = Math.Max(
                MinimumThumbHeight,
                (int)Math.Round(
                    trackRectangle.Height * viewportHeight / (double)contentHeight));
            thumbHeight = Math.Min(thumbHeight, trackRectangle.Height);

            int availableTrack = trackRectangle.Height - thumbHeight;
            int scrollMaximum = contentHeight - viewportHeight;
            int thumbY = trackRectangle.Top + (int)Math.Round(
                GetScrollPosition() / (double)scrollMaximum * availableTrack);

            return new Rectangle(
                trackRectangle.X,
                thumbY,
                trackRectangle.Width,
                thumbHeight);
        }

        // 자동 스크롤 좌표와 최소 크기를 모두 고려해 실제 콘텐츠 높이를 구한다.
        private int GetContentHeight()
        {
            if (Controls.Count == 0)
                return AutoScrollMinSize.Height;

            // AutoScroll은 이동한 만큼 자식의 화면 좌표를 음수로 바꾸므로 현재 위치를 복원해 계산한다.
            int childContentHeight = Controls.Cast<Control>()
                .Max(control => control.Bottom + GetScrollPosition()) + Padding.Bottom;
            return Math.Max(AutoScrollMinSize.Height, childContentHeight);
        }

        // 현재 화면에서 이동할 수 있는 최대 세로 거리를 반환한다.
        private int GetScrollMaximum()
        {
            return Math.Max(0, GetContentHeight() - ClientSize.Height);
        }

        // WinForms가 음수로 보관하는 AutoScrollPosition을 양수 거리로 변환한다.
        private int GetScrollPosition()
        {
            return Math.Abs(AutoScrollPosition.Y);
        }

        // 요청 위치를 유효 범위로 제한한 뒤 네이티브 스크롤 상태와 표시를 동기화한다.
        private void ScrollTo(int position)
        {
            int clampedPosition = Math.Clamp(position, 0, GetScrollMaximum());
            AutoScrollPosition = new Point(0, clampedPosition);
            HideNativeScrollBars();
            InvalidateScrollBar();
        }

        // 트랙과 손잡이를 양끝이 둥근 막대 형태로 그린다.
        private void DrawPill(Graphics graphics, Brush brush, Rectangle rectangle)
        {
            if (rectangle.Width <= 0 || rectangle.Height <= 0)
                return;

            int radius = rectangle.Width;
            graphics.FillRectangle(
                brush,
                rectangle.X,
                rectangle.Y + radius / 2,
                rectangle.Width,
                Math.Max(0, rectangle.Height - radius));
            graphics.FillEllipse(
                brush,
                rectangle.X,
                rectangle.Y,
                rectangle.Width,
                radius);
            graphics.FillEllipse(
                brush,
                rectangle.X,
                rectangle.Bottom - radius,
                rectangle.Width,
                radius);
        }

        // 전체 패널 대신 스크롤바가 위치한 우측 영역만 다시 그리도록 요청한다.
        private void InvalidateScrollBar()
        {
            Invalidate(new Rectangle(
                Math.Max(0, ClientSize.Width - ScrollBarWidth - 1),
                0,
                ScrollBarWidth + 1,
                ClientSize.Height));
        }

        // Win32 기본 가로·세로 스크롤바를 숨기고 재진입을 방지한다.
        private void HideNativeScrollBars()
        {
            if (!IsHandleCreated || isUpdatingNativeScrollBars)
                return;

            try
            {
                isUpdatingNativeScrollBars = true;
                HorizontalScroll.Enabled = false;
                HorizontalScroll.Visible = false;
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
