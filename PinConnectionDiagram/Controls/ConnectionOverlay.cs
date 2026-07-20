using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;

namespace PinConnectionDiagram.Controls
{
    internal sealed class ConnectionOverlay : Control
    {
        private readonly Func<IEnumerable<ConnectionLine>> getConnectionLines;

        public ConnectionOverlay(Func<IEnumerable<ConnectionLine>> getConnectionLines)
        {
            this.getConnectionLines = getConnectionLines;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WsExTransparent = 0x20;
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= WsExTransparent;
                return parameters;
            }
        }

        protected override void WndProc(ref Message message)
        {
            const int WmNcHitTest = 0x0084;
            const int HtTransparent = -1;

            if (message.Msg == WmNcHitTest)
            {
                message.Result = (IntPtr)HtTransparent;
                return;
            }

            base.WndProc(ref message);
        }

        public void RefreshConnections()
        {
            using(GraphicsPath visibleArea = new GraphicsPath())
            {
                foreach (ConnectionLine line in getConnectionLines())
                {
                    if (line.StartConnector.IsDisposed || line.EndConnector.IsDisposed)
                    {
                        continue;
                    }

                    // 각 연결선을 별도의 도형으로 추가해야 서로 대각선으로 이어지지 않는다.
                    visibleArea.StartFigure();
                    visibleArea.AddLines(GetLinePoints(line));
                }

                using(Pen regionPen = new Pen(Color.Black, 7))
                {
                    regionPen.StartCap = LineCap.Round;
                    regionPen.EndCap = LineCap.Round;
                    regionPen.LineJoin = LineJoin.Round;

                    if (visibleArea.PointCount > 0)
                    {
                        visibleArea.Widen(regionPen);
                        SetVisibleRegion(new Region(visibleArea));
                    }
                    else
                    {
                        Region emptyRegion = new Region();
                        emptyRegion.MakeEmpty();
                        SetVisibleRegion(emptyRegion);
                    }
                }
            }

            Invalidate();
        }

        private void SetVisibleRegion(Region newRegion)
        {
            Region? oldRegion = Region;
            Region = newRegion;
            oldRegion?.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen pen = new Pen(Color.LimeGreen, 3))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                foreach (ConnectionLine line in getConnectionLines())
                {
                    if (line.StartConnector.IsDisposed || line.EndConnector.IsDisposed)
                    {
                        continue;
                    }

                    e.Graphics.DrawLines(pen, GetLinePoints(line));
                }
            }
        }

        private Point[] GetLinePoints(ConnectionLine line)
        {
            Point start = line.StartConnector.GetConnectionPoint(this);
            Point end = line.EndConnector.GetConnectionPoint(this);
            int middleX = GetMiddleX(line, start, end);

            return new[]
            {
                start,
                new Point(middleX, start.Y),
                new Point(middleX, end.Y),
                end
            };
        }

        private int GetMiddleX(ConnectionLine line, Point start, Point end)
        {
            if (line.StartConnector.ConnectorType == ConnectorType.Test &&
                line.StartConnector.Side == ConnectorSide.Left &&
                line.EndConnector.Side == ConnectorSide.Left)
            {
                CablePanel? panel = FindCablePanel(line.StartConnector);

                if (panel != null)
                {
                    Point panelCenter = panel.PointToScreen(
                        new Point(panel.ClientSize.Width / 2, 0));

                    return PointToClient(panelCenter).X;
                }
            }

            return (start.X + end.X) / 2;
        }

        private CablePanel? FindCablePanel(Control control)
        {
            Control? parent = control.Parent;

            while (parent != null)
            {
                if (parent is CablePanel panel)
                {
                    return panel;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }
}
