using PinConnectionDiagram.Models;
using System.Drawing.Drawing2D;

namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// ConnectionLine 목록을 지도 위에 표시하되 기존 컨트롤의 입력은 통과시킨다.
    /// </summary>
    public partial class ConnectionOverlay : Control
    {
        private readonly Func<IEnumerable<ConnectionLine>> getConnectionLines;
        private readonly Dictionary<ConnectionLine, ConnectionDropZone> dropZones = new();
        private ConnectionLine? hoveredLine;
        public event Action? CableAssignmentChanged;
        public event Action<ConnectionLine>? ConnectionDeleteRequested;

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
                // 아래에 있는 WinForms 컨트롤이 먼저 그려진 뒤 선을 표시하게 한다.
                const int WsExTransparent = 0x20;
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= WsExTransparent;
                return parameters;
            }
        }

        protected override void WndProc(ref Message message)
        {
            // 오버레이가 연결점 및 버튼의 마우스 입력을 가로채지 않게 한다.
            const int WmNcHitTest = 0x0084;
            const int HtTransparent = -1;

            if (message.Msg == WmNcHitTest)
            {
                Point cursorPoint = PointToClient(Cursor.Position);

                if (dropZones.Values.Any(zone => zone.Bounds.Contains(cursorPoint)))
                {
                    base.WndProc(ref message);
                    return;
                }

                // 선의 시작/끝에서는 오버레이가 클릭을 받지 않고 실제 연결점으로 전달한다.
                if (IsConnectorPointAt(cursorPoint))
                {
                    message.Result = (IntPtr)HtTransparent;
                    return;
                }

                if (FindLineAt(cursorPoint) != null)
                {
                    base.WndProc(ref message);
                    return;
                }

                message.Result = (IntPtr)HtTransparent;
                return;
            }

            base.WndProc(ref message);
        }

        private bool IsConnectorPointAt(Point point)
        {
            foreach (Connector connector in getConnectionLines()
                .SelectMany(line => new[] { line.StartConnector, line.EndConnector })
                .Distinct())
            {
                Point center = connector.GetConnectionPoint(this);
                Size size = connector.ConnectionPointSize;
                Rectangle hitRectangle = new Rectangle(
                    center.X - size.Width / 2 - 3,
                    center.Y - size.Height / 2 - 3,
                    size.Width + 6,
                    size.Height + 6);

                if (hitRectangle.Contains(point))
                    return true;
            }

            return false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ConnectionLine? line = FindLineAt(e.Location);
                if (line != null)
                {
                    ConnectionDeleteRequested?.Invoke(line);
                    return;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ConnectionLine? line = FindLineAt(e.Location);
            if (!ReferenceEquals(hoveredLine, line))
            {
                hoveredLine = line;
                Cursor = line == null ? Cursors.Default : Cursors.Hand;
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (hoveredLine != null)
            {
                hoveredLine = null;
                Cursor = Cursors.Default;
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        private ConnectionLine? FindLineAt(Point point)
        {
            const double hitDistance = 8;

            // 위에 그려진 선부터 검사해 겹친 선에서도 사용자가 보는 선이 선택되게 한다.
            foreach (ConnectionLine line in getConnectionLines().Reverse())
            {
                if (line.StartConnector.IsDisposed || line.EndConnector.IsDisposed)
                    continue;

                Point[] points = GetLinePoints(line);
                for (int index = 0; index < points.Length - 1; index++)
                {
                    if (GetDistanceToSegment(point, points[index], points[index + 1]) <= hitDistance)
                        return line;
                }
            }

            return null;
        }

        private static double GetDistanceToSegment(Point point, Point start, Point end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            if (dx == 0 && dy == 0)
                return Math.Sqrt(Math.Pow(point.X - start.X, 2) + Math.Pow(point.Y - start.Y, 2));

            double ratio = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) /
                (dx * dx + dy * dy);
            ratio = Math.Clamp(ratio, 0, 1);
            double closestX = start.X + ratio * dx;
            double closestY = start.Y + ratio * dy;
            return Math.Sqrt(Math.Pow(point.X - closestX, 2) + Math.Pow(point.Y - closestY, 2));
        }

        public void RefreshConnections()
        {
            // 전체 지도 대신 선 주변만 오버레이 영역으로 만들어 화면 가림을 방지한다.
            using(GraphicsPath lineArea = new GraphicsPath())
            {
                SyncDropZones();

                foreach (ConnectionLine line in getConnectionLines())
                {
                    if (line.StartConnector.IsDisposed || line.EndConnector.IsDisposed)
                    {
                        continue;
                    }

                    // 각 연결선을 별도의 도형으로 추가해야 서로 대각선으로 이어지지 않는다.
                    lineArea.StartFigure();
                    lineArea.AddLines(GetLinePoints(line));
                }

                // 투명 오버레이 영역을 실제 선 두께와 가깝게 제한해 배경 띠가 보이지 않게 한다.
                // 입력 영역을 과도하게 넓히면 투명 오버레이 배경이 짙은 외곽선처럼 보인다.
                // 기본 선(4px)보다 조금만 넓혀 클릭과 Hover 여유만 유지한다.
                using(Pen regionPen = new Pen(Color.Black, 8))
                {
                    regionPen.StartCap = LineCap.Round;
                    regionPen.EndCap = LineCap.Round;
                    regionPen.LineJoin = LineJoin.Round;

                    Region visibleRegion = new Region();
                    visibleRegion.MakeEmpty();

                    if (lineArea.PointCount > 0)
                    {
                        lineArea.Widen(regionPen);
                        visibleRegion.Union(lineArea);
                    }

                    // 드롭 영역은 테두리가 아닌 사각형 전체를 표시 영역에 포함한다.
                    foreach (ConnectionDropZone dropZone in dropZones.Values)
                    {
                        visibleRegion.Union(dropZone.Bounds);
                    }

                    SetVisibleRegion(visibleRegion);
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

            DrawConnections(e.Graphics, true, true);
        }

        public void DrawConnections(
            Graphics graphics,
            bool includeDropZones,
            bool includeHover = false)
        {
            // 화면과 출력 이미지가 동일한 선 및 배치 항목을 사용하도록 그리기 코드를 공유한다.
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using(Pen outlinePen = new Pen(Color.FromArgb(155, 160, 170), 8))
            using(Pen linePen = new Pen(Color.Black, 4))
            using(Pen hoverPen = new Pen(Color.FromArgb(145, 223, 251), 9))
            {
                outlinePen.StartCap = LineCap.Round;
                outlinePen.EndCap = LineCap.Round;
                outlinePen.LineJoin = LineJoin.Round;
                linePen.StartCap = LineCap.Round;
                linePen.EndCap = LineCap.Round;
                linePen.LineJoin = LineJoin.Round;
                hoverPen.StartCap = LineCap.Round;
                hoverPen.EndCap = LineCap.Round;
                hoverPen.LineJoin = LineJoin.Round;

                foreach (ConnectionLine line in getConnectionLines())
                {
                    if (line.StartConnector.IsDisposed || line.EndConnector.IsDisposed)
                    {
                        continue;
                    }

                    Point[] points = GetLinePoints(line);
                    graphics.DrawLines(outlinePen, points);
                    if (includeHover && ReferenceEquals(line, hoveredLine))
                        graphics.DrawLines(hoverPen, points);
                    graphics.DrawLines(linePen, points);
                }
            }

            // 오버레이가 앞에 있어도 연결점이 선보다 위에 보이도록 점을 마지막에 다시 그린다.
            foreach (Connector connector in getConnectionLines()
                .SelectMany(line => new[] { line.StartConnector, line.EndConnector })
                .Distinct())
            {
                Point center = connector.GetConnectionPoint(this);
                Size pointSize = connector.ConnectionPointSize;
                Rectangle pointRectangle = new Rectangle(
                    center.X - pointSize.Width / 2,
                    center.Y - pointSize.Height / 2,
                    pointSize.Width,
                    pointSize.Height);
                using SolidBrush pointBrush = new SolidBrush(connector.ConnectionPointColor);
                graphics.FillRectangle(pointBrush, pointRectangle);
            }

            if (!includeDropZones)
                return;

            foreach (ConnectionDropZone dropZone in dropZones.Values)
            {
                using Bitmap zoneBitmap = new Bitmap(dropZone.Width, dropZone.Height);
                dropZone.DrawToBitmap(zoneBitmap, new Rectangle(Point.Empty, dropZone.Size));
                graphics.DrawImageUnscaled(zoneBitmap, dropZone.Location);
            }
        }

        private void SyncDropZones()
        {
            List<ConnectionLine> activeLines = getConnectionLines()
                .Where(line =>
                    !line.StartConnector.IsDisposed &&
                    !line.EndConnector.IsDisposed)
                .ToList();

            List<List<ConnectionLine>> connectionGroups =
                BuildConnectionGroups(activeLines);

            List<ConnectionLine> groupKeys = connectionGroups
                .Select(group => group[0])
                .ToList();

            foreach (ConnectionLine removedLine in
                dropZones.Keys.Except(groupKeys).ToList())
            {
                ConnectionDropZone dropZone = dropZones[removedLine];
                Controls.Remove(dropZone);
                dropZone.Dispose();
                dropZones.Remove(removedLine);
            }

            foreach (List<ConnectionLine> group in connectionGroups)
            {
                ConnectionLine groupKey = group[0];

                // 기존 배정이 있는 그룹에 새 분기가 추가되면 같은 케이블을 상속한다.
                CableInfo? assignedCable = group
                    .Select(line => line.CableInfo)
                    .FirstOrDefault(cableInfo => cableInfo != null);

                if (assignedCable != null)
                {
                    foreach (ConnectionLine line in group)
                    {
                        line.CableInfo = assignedCable;
                    }
                }

                if (!dropZones.TryGetValue(groupKey, out ConnectionDropZone? dropZone))
                {
                    dropZone = new ConnectionDropZone(group);
                    dropZone.CableAssignmentChanged += () =>
                        CableAssignmentChanged?.Invoke();
                    dropZones.Add(groupKey, dropZone);
                    Controls.Add(dropZone);
                }

                dropZone.UpdateConnectionLines(group);

                dropZone.Location = GetDropZoneLocation(group, dropZone.Size);
                dropZone.BringToFront();
                dropZone.Invalidate();
            }
        }

        private List<List<ConnectionLine>> BuildConnectionGroups(
            List<ConnectionLine> activeLines)
        {
            List<List<ConnectionLine>> groups = new();
            HashSet<ConnectionLine> visited = new();

            foreach (ConnectionLine startLine in activeLines)
            {
                if (!visited.Add(startLine))
                    continue;

                List<ConnectionLine> group = new();
                Queue<ConnectionLine> pending = new();
                pending.Enqueue(startLine);

                while (pending.Count > 0)
                {
                    ConnectionLine current = pending.Dequeue();
                    group.Add(current);

                    foreach (ConnectionLine candidate in activeLines)
                    {
                        if (visited.Contains(candidate) ||
                            !SharesConnector(current, candidate))
                        {
                            continue;
                        }

                        visited.Add(candidate);
                        pending.Enqueue(candidate);
                    }
                }

                groups.Add(group);
            }

            return groups;
        }

        private bool SharesConnector(ConnectionLine first, ConnectionLine second)
        {
            return ReferenceEquals(first.StartConnector, second.StartConnector) ||
                   ReferenceEquals(first.StartConnector, second.EndConnector) ||
                   ReferenceEquals(first.EndConnector, second.StartConnector) ||
                   ReferenceEquals(first.EndConnector, second.EndConnector);
        }

        private Point GetDropZoneLocation(
            IReadOnlyList<ConnectionLine> group,
            Size dropZoneSize)
        {
            ConnectionLine firstLine = group[0];
            int trackX = GetMiddleX(
                firstLine,
                firstLine.StartConnector.GetConnectionPoint(this),
                firstLine.EndConnector.GetConnectionPoint(this));
            List<int> connectorYs = group
                .SelectMany(line => new[]
                {
                    line.StartConnector.GetConnectionPoint(this).Y,
                    line.EndConnector.GetConnectionPoint(this).Y
                })
                .ToList();
            int topY = connectorYs.Min();
            int bottomY = connectorYs.Max();

            // 일반 케이블은 공통 트랙 중앙에, 시험 대상 케이블은 오른쪽 빈 영역에 붙인다.
            int x = firstLine.StartConnector.ConnectorType == ConnectorType.Test
                ? trackX + 2
                : trackX - dropZoneSize.Width / 2;
            int centerY = firstLine.StartConnector.ConnectorType == ConnectorType.Test
                ? (topY + bottomY) / 2
                : topY;
            int y = centerY - dropZoneSize.Height / 2;

            return new Point(
                Math.Clamp(x, 0, Math.Max(0, Width - dropZoneSize.Width)),
                Math.Clamp(y, 0, Math.Max(0, Height - dropZoneSize.Height)));
        }

        private Point[] GetLinePoints(ConnectionLine line)
        {
            // 기본 연결선은 가로-세로-가로의 직각 경로를 사용한다.
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
            const int trackSpacing = 12;
            int baseX = (start.X + end.X) / 2;
            // Test는 Left끼리 연결되므로 기존처럼 패널 중앙까지 들어갔다가 되돌아오게 한다.
            if (line.StartConnector.ConnectorType == ConnectorType.Test &&
                line.StartConnector.Side == ConnectorSide.Left &&
                line.EndConnector.Side == ConnectorSide.Left)
            {
                CablePanel? panel = FindCablePanel(line.StartConnector);

                if (panel != null)
                {
                    Point panelCenter = panel.PointToScreen(
                        new Point(panel.ClientSize.Width / 2, 0));

                    baseX = PointToClient(panelCenter).X;
                }
            }

            List<ConnectionLine> sameTypeLines = getConnectionLines()
                .Where(item =>
                    !item.StartConnector.IsDisposed &&
                    !item.EndConnector.IsDisposed &&
                    item.StartConnector.ConnectorType == line.StartConnector.ConnectorType)
                .ToList();

            List<List<ConnectionLine>> groups = BuildConnectionGroups(sameTypeLines)
                .OrderBy(group => group.Min(GetLineTopY))
                .ThenBy(group => group.Max(GetLineBottomY))
                .ToList();
            int groupIndex = groups.FindIndex(group =>
                group.Any(item => ReferenceEquals(item, line)));

            if (groupIndex < 0)
            {
                return baseX;
            }

            // 같은 그룹은 하나의 트랙으로 뭉치고 서로 다른 그룹에만 간격을 둔다.
            int trackX = baseX + (int)Math.Round(
                (groupIndex - (groups.Count - 1) / 2.0) * trackSpacing);

            if (line.StartConnector.ConnectorType != ConnectorType.Test)
            {
                int minimumX = Math.Min(start.X, end.X) + 20;
                int maximumX = Math.Max(start.X, end.X) - 20;
                trackX = Math.Clamp(trackX, minimumX, maximumX);
            }

            return trackX;
        }

        private int GetLineTopY(ConnectionLine line)
        {
            Point start = line.StartConnector.GetConnectionPoint(this);
            Point end = line.EndConnector.GetConnectionPoint(this);
            return Math.Min(start.Y, end.Y);
        }

        private int GetLineBottomY(ConnectionLine line)
        {
            Point start = line.StartConnector.GetConnectionPoint(this);
            Point end = line.EndConnector.GetConnectionPoint(this);
            return Math.Max(start.Y, end.Y);
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
