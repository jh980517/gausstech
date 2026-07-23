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
        private Connector? hoveredBundleEndpoint;
        private string? hoveredBundleName;
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

                if (FindLineAt(cursorPoint, out _, out _) != null)
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
                ConnectionLine? line = FindLineAt(
                    e.Location,
                    out Connector? bundleEndpoint,
                    out string? bundleName);
                if (line != null)
                {
                    if (bundleName != null)
                    {
                        ShowBundleConnectionMenu(bundleName, bundleEndpoint, e.Location);
                        return;
                    }

                    ConnectionDeleteRequested?.Invoke(line);
                    return;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ConnectionLine? line = FindLineAt(
                e.Location,
                out Connector? bundleEndpoint,
                out string? bundleName);
            if (!ReferenceEquals(hoveredLine, line) ||
                !ReferenceEquals(hoveredBundleEndpoint, bundleEndpoint) ||
                !string.Equals(
                    hoveredBundleName,
                    bundleName,
                    StringComparison.OrdinalIgnoreCase))
            {
                hoveredLine = line;
                hoveredBundleEndpoint = bundleEndpoint;
                hoveredBundleName = bundleName;
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
                hoveredBundleEndpoint = null;
                hoveredBundleName = null;
                Cursor = Cursors.Default;
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        private ConnectionLine? FindLineAt(
            Point point,
            out Connector? bundleEndpoint,
            out string? bundleName)
        {
            const double hitDistance = 8;
            bundleEndpoint = null;
            bundleName = null;

            // 화면에 다시 구성한 H형 어댑터 몸통과 분기선도 하나의 케이블 Hover 영역으로 검사한다.
            foreach (IGrouping<string, ConnectionLine> bundle in
                GetPhysicalAdapterBundles(getConnectionLines()).Reverse())
            {
                Rectangle bounds = GetAdapterCableBodyBounds(bundle).Single();

                foreach (Connector source in bundle
                    .Select(GetAdapterSourceConnector)
                    .Distinct())
                {
                    Point sourcePoint = source.GetConnectionPoint(this);
                    if (GetDistanceToSegment(
                        point,
                        sourcePoint,
                        new Point(bounds.Left, sourcePoint.Y)) <= hitDistance)
                    {
                        bundleEndpoint = source;
                        bundleName = bundle.Key;
                        return bundle.First(line =>
                            ReferenceEquals(GetAdapterSourceConnector(line), source));
                    }
                }

                foreach (Connector target in bundle
                    .Select(GetAdapterTargetConnector)
                    .Distinct())
                {
                    Point targetPoint = target.GetConnectionPoint(this);
                    if (GetDistanceToSegment(
                        point,
                        new Point(bounds.Right, targetPoint.Y),
                        targetPoint) <= hitDistance)
                    {
                        bundleEndpoint = target;
                        bundleName = bundle.Key;
                        return bundle.First(line =>
                            ReferenceEquals(GetAdapterTargetConnector(line), target));
                    }
                }

                foreach (Point[] path in GetAdapterCableBodyPaths(bundle))
                {
                    for (int index = 0; index < path.Length - 1; index++)
                    {
                        if (GetDistanceToSegment(point, path[index], path[index + 1]) <= hitDistance)
                        {
                            bundleName = bundle.Key;
                            return bundle.First();
                        }
                    }
                }
            }

            // 위에 그려진 선부터 검사해 겹친 선에서도 사용자가 보는 선이 선택되게 한다.
            foreach (ConnectionLine line in getConnectionLines().Reverse())
            {
                if (line.StartConnector.IsDisposed || line.EndConnector.IsDisposed)
                    continue;
                if (IsPhysicalAdapterBundleLine(line))
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

        private void ShowBundleConnectionMenu(
            string bundleName,
            Connector? endpoint,
            Point location)
        {
            List<ConnectionLine> bundleLines = getConnectionLines()
                .Where(line =>
                    line.StartConnector.ConnectorType == ConnectorType.Adapter &&
                    string.Equals(
                        line.CableInfo?.Name,
                        bundleName,
                        StringComparison.OrdinalIgnoreCase))
                .Where(line =>
                    endpoint == null ||
                    ReferenceEquals(GetAdapterSourceConnector(line), endpoint) ||
                    ReferenceEquals(GetAdapterTargetConnector(line), endpoint))
                .OrderBy(line => GetAdapterSourceConnector(line).TJNumber)
                .ThenBy(line => GetAdapterSourceConnector(line).ConnectorName)
                .ThenBy(line => GetAdapterTargetConnector(line).ConnectorName)
                .ToList();

            if (bundleLines.Count == 0)
                return;

            ContextMenuStrip menu = new ContextMenuStrip
            {
                BackColor = Color.FromArgb(38, 38, 38),
                ForeColor = Color.WhiteSmoke,
                ShowImageMargin = false
            };
            menu.Closed += (_, _) =>
            {
                // WinForms 메뉴 필터가 닫기 메시지를 모두 처리하기 전에 즉시 Dispose하면
                // 빈 화면 클릭 시 이미 폐기된 핸들을 다시 참조해 ObjectDisposedException이 발생한다.
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(new Action(() => menu.Dispose()));
                }
            };

            foreach (ConnectionLine line in bundleLines)
            {
                Connector source = GetAdapterSourceConnector(line);
                Connector target = GetAdapterTargetConnector(line);
                ToolStripMenuItem item = new ToolStripMenuItem(
                    $"TJ{source.TJNumber} {source.ConnectorName} → " +
                    $"TJ{target.TJNumber} {target.ConnectorName} 삭제");
                item.ForeColor = Color.WhiteSmoke;
                item.Click += (_, _) => ConnectionDeleteRequested?.Invoke(line);
                menu.Items.Add(item);
            }

            menu.Show(this, location);
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
            List<ConnectionLine> drawableLines = getConnectionLines()
                .Where(line => !line.StartConnector.IsDisposed && !line.EndConnector.IsDisposed)
                .ToList();

            // 전체 지도 대신 선 주변만 오버레이 영역으로 만들어 화면 가림을 방지한다.
            using(GraphicsPath lineArea = new GraphicsPath())
            {
                SyncDropZones();

                foreach (ConnectionLine line in drawableLines)
                {
                    if (IsPhysicalAdapterBundleLine(line))
                        continue;

                    // 각 연결선을 별도의 도형으로 추가해야 서로 대각선으로 이어지지 않는다.
                    lineArea.StartFigure();
                    lineArea.AddLines(GetLinePoints(line));
                }
                foreach (Point[] bodyPath in GetAdapterCableBodyPaths(drawableLines))
                {
                    // 동일 어댑터 케이블의 물리적 H형 몸통도 선 영역으로만 포함한다.
                    lineArea.StartFigure();
                    lineArea.AddLines(bodyPath);
                }

                RectangleF contentBounds = lineArea.PointCount > 0
                    ? lineArea.GetBounds()
                    : RectangleF.Empty;
                foreach (ConnectionDropZone dropZone in dropZones.Values)
                    contentBounds = contentBounds.IsEmpty
                        ? dropZone.Bounds
                        : RectangleF.Union(contentBounds, dropZone.Bounds);

                // 준비물 추가 도중의 임시 0px 레이아웃으로 기존 정상 Region을 지우지 않는다.
                RectangleF viewportBounds = new RectangleF(0, 0, Width, Height);
                if (drawableLines.Count > 0 &&
                    (contentBounds.IsEmpty || !contentBounds.IntersectsWith(viewportBounds)))
                {
                    Invalidate();
                    return;
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

            // 동일한 어댑터 준비물이 배치된 독립 회로들을 하나의 물리적 케이블 외피로 먼저 그린다.
            // 실제 전기 연결선은 아래에서 다시 그리므로 서로 단락된 것처럼 처리되지 않는다.
            DrawAdapterCableBodies(graphics, includeHover);

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
                    if (IsPhysicalAdapterBundleLine(line))
                        continue;

                    Point[] points = GetLinePoints(line);
                    graphics.DrawLines(outlinePen, points);
                    if (includeHover && IsInHoveredBranch(line))
                        graphics.DrawLines(hoverPen, points);
                    graphics.DrawLines(linePen, points);
                }
            }

            DrawAdapterSharedJunctions(graphics);

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
                BuildDropZoneGroups(activeLines);

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
                    {
                        // 드롭으로 높이가 바뀐 즉시 위치와 표시 Region을 새 크기로 다시 만든다.
                        // Region 갱신이 늦으면 실제 크기는 커져도 기존 34px 영역 밖이 잘려 보인다.
                        RefreshConnections();
                        CableAssignmentChanged?.Invoke();
                    };
                    dropZones.Add(groupKey, dropZone);
                    Controls.Add(dropZone);
                }

                dropZone.UpdateConnectionLines(group);

                dropZone.Location = GetDropZoneLocation(group, dropZone.Size);
                dropZone.BringToFront();
                dropZone.Invalidate();
            }
        }

        /// <summary>
        /// 전기적으로 독립된 연결 그룹은 유지하되, 동일한 어댑터 준비물이 배치된 그룹은
        /// TJ가 달라도 하나의 물리적 어댑터 케이블로 묶어 DropZone을 하나만 표시한다.
        /// 지그 및 시험 대상 케이블은 기존 전기 연결 그룹 기준을 그대로 사용한다.
        /// </summary>
        private List<List<ConnectionLine>> BuildDropZoneGroups(
            List<ConnectionLine> activeLines)
        {
            List<List<ConnectionLine>> electricalGroups =
                BuildConnectionGroups(activeLines);
            List<List<ConnectionLine>> displayGroups = new();

            foreach (List<ConnectionLine> electricalGroup in electricalGroups)
            {
                ConnectionLine firstLine = electricalGroup[0];
                CableInfo? assignedCable = electricalGroup
                    .Select(line => line.CableInfo)
                    .FirstOrDefault(cable => cable != null);

                if (firstLine.StartConnector.ConnectorType != ConnectorType.Adapter ||
                    assignedCable == null)
                {
                    displayGroups.Add(new List<ConnectionLine>(electricalGroup));
                    continue;
                }

                List<ConnectionLine>? physicalAdapterGroup = displayGroups
                    .FirstOrDefault(group =>
                        group[0].StartConnector.ConnectorType == ConnectorType.Adapter &&
                        group.Any(line => string.Equals(
                            line.CableInfo?.Name,
                            assignedCable.Name,
                            StringComparison.OrdinalIgnoreCase)));

                if (physicalAdapterGroup == null)
                {
                    displayGroups.Add(new List<ConnectionLine>(electricalGroup));
                }
                else
                {
                    physicalAdapterGroup.AddRange(electricalGroup);
                }
            }

            return displayGroups;
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

            // 같은 이름의 어댑터 케이블로 묶인 복수 입력 그룹은 전체 케이블 외형 중앙에
            // DropItem을 하나만 표시한다.
            if (firstLine.StartConnector.ConnectorType == ConnectorType.Adapter &&
                group.Select(GetAdapterSourceConnector).Distinct().Count() > 1 &&
                group.Any(line => line.CableInfo != null))
            {
                Rectangle bodyBounds = GetAdapterCableBodyBounds(group).First();
                return new Point(
                    Math.Clamp(
                        bodyBounds.Left + (bodyBounds.Width - dropZoneSize.Width) / 2,
                        0,
                        Math.Max(0, Width - dropZoneSize.Width)),
                    Math.Clamp(
                        bodyBounds.Top + (bodyBounds.Height - dropZoneSize.Height) / 2,
                        0,
                        Math.Max(0, Height - dropZoneSize.Height)));
            }

            Point firstStart = firstLine.StartConnector.GetConnectionPoint(this);
            Point firstEnd = firstLine.EndConnector.GetConnectionPoint(this);
            // 지그와 어댑터 DropZone은 선 겹침 방지용 트랙 오프셋과 관계없이
            // 좌우 커넥터의 정중앙에 둔다.
            int trackX = firstLine.StartConnector.ConnectorType is
                ConnectorType.Jig or ConnectorType.Adapter
                ? (firstStart.X + firstEnd.X) / 2
                : GetMiddleX(firstLine, firstStart, firstEnd);
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
            int centerY = firstLine.StartConnector.ConnectorType switch
            {
                ConnectorType.Test => (topY + bottomY) / 2,
                // 일대다 어댑터 DropItem은 최상단 J가 아니라 P에서 몸통으로 들어가는
                // 주 연결선 높이에 배치해 커넥터 위치 변경을 그대로 따라가게 한다.
                ConnectorType.Adapter => (int)Math.Round(group
                    .Select(GetAdapterSourceConnector)
                    .Distinct()
                    .Average(connector => connector.GetConnectionPoint(this).Y)),
                _ => topY
            };
            int y = centerY - dropZoneSize.Height / 2;

            return new Point(
                Math.Clamp(x, 0, Math.Max(0, Width - dropZoneSize.Width)),
                Math.Clamp(y, 0, Math.Max(0, Height - dropZoneSize.Height)));
        }

        private Point[] GetLinePoints(ConnectionLine line)
        {
            if (line.StartConnector.ConnectorType == ConnectorType.Adapter)
                return GetAdapterLinePoints(line);

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

        /// <summary>
        /// 어댑터 하네스는 P 커넥터마다 독립된 세로 레인을 사용하고,
        /// 여러 P가 같은 J에 연결될 때는 J 앞의 Junction에서만 합류시킨다.
        /// </summary>
        private Point[] GetAdapterLinePoints(ConnectionLine line)
        {
            Connector source = GetAdapterSourceConnector(line);
            Connector target = GetAdapterTargetConnector(line);
            Point start = source.GetConnectionPoint(this);
            Point end = target.GetConnectionPoint(this);
            int trackX = GetAdapterSourceTrackX(line, source, start, end);

            List<ConnectionLine> incomingLines = getConnectionLines()
                .Where(candidate =>
                    candidate.StartConnector.ConnectorType == ConnectorType.Adapter &&
                    ReferenceEquals(GetAdapterTargetConnector(candidate), target))
                .OrderBy(candidate => GetAdapterSourceConnector(candidate).TJNumber)
                .ThenBy(candidate => GetAdapterSourceConnector(candidate).ConnectorName)
                .ToList();
            int incomingIndex = incomingLines.FindIndex(candidate => ReferenceEquals(candidate, line));
            int approachOffset = incomingLines.Count > 1 && incomingIndex >= 0
                ? (int)Math.Round((incomingIndex - (incomingLines.Count - 1) / 2.0) * 8)
                : 0;
            int approachY = end.Y + approachOffset;
            int junctionX = end.X - 16;

            return new[]
            {
                start,
                new Point(trackX, start.Y),
                new Point(trackX, approachY),
                new Point(junctionX, approachY),
                new Point(junctionX, end.Y),
                end
            };
        }

        private int GetAdapterSourceTrackX(
            ConnectionLine line,
            Connector source,
            Point start,
            Point end)
        {
            const int defaultTrackSpacing = 14;
            const int bundledTrackSpacing = 7;
            int trackX = (start.X + end.X) / 2;
            IEnumerable<ConnectionLine> adapterLines = getConnectionLines()
                .Where(candidate =>
                    candidate.StartConnector.ConnectorType == ConnectorType.Adapter);

            // 같은 준비물이 배치된 어댑터는 TJ가 달라도 하나의 몸통 안에서 가까운 평행 레인을 쓴다.
            bool hasAssignedCable = line.CableInfo != null;
            if (hasAssignedCable)
            {
                adapterLines = adapterLines.Where(candidate =>
                    string.Equals(
                        candidate.CableInfo?.Name,
                        line.CableInfo!.Name,
                        StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                adapterLines = adapterLines.Where(candidate =>
                    GetAdapterSourceConnector(candidate).TJNumber == source.TJNumber);
            }

            List<Connector> sources = adapterLines
                .Select(GetAdapterSourceConnector)
                .Distinct()
                .OrderBy(connector => connector.GetConnectionPoint(this).Y)
                .ThenBy(connector => connector.ConnectorName)
                .ToList();
            bool isPhysicalBundle = hasAssignedCable && sources.Count > 1;
            bool isOneToManyBranch = adapterLines
                .Where(candidate =>
                    ReferenceEquals(GetAdapterSourceConnector(candidate), source))
                .Select(GetAdapterTargetConnector)
                .Distinct()
                .Count() > 1;
            if (isPhysicalBundle || isOneToManyBranch)
            {
                // 다대다 몸통과 일대다 분기는 J 앞에 트랙을 두어 긴 수평 분기선을 만들지 않는다.
                trackX = Math.Max(start.X, end.X) - 32;
            }

            int sourceIndex = sources.FindIndex(connector => ReferenceEquals(connector, source));
            if (sourceIndex >= 0)
            {
                int trackSpacing = isPhysicalBundle
                    ? bundledTrackSpacing
                    : defaultTrackSpacing;
                trackX += (int)Math.Round(
                    (sourceIndex - (sources.Count - 1) / 2.0) * trackSpacing);
            }

            return Math.Clamp(
                trackX,
                Math.Min(start.X, end.X) + 20,
                Math.Max(start.X, end.X) - 20);
        }

        /// <summary>
        /// 같은 케이블명을 사용하는 복수의 어댑터 입력 회로 뒤에 공통 외피를 그린다.
        /// 외피는 물리적으로 하나의 케이블임을 나타낼 뿐 전기적 연결 데이터에는 관여하지 않는다.
        /// </summary>
        private void DrawAdapterCableBodies(Graphics graphics, bool includeHover)
        {
            using Pen outlinePen = new Pen(Color.FromArgb(155, 160, 170), 8);
            using Pen linePen = new Pen(Color.Black, 4);
            using Pen hoverPen = new Pen(Color.FromArgb(145, 223, 251), 9);
            foreach (Pen pen in new[] { outlinePen, linePen, hoverPen })
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
            }

            foreach (IGrouping<string, ConnectionLine> bundle in
                GetPhysicalAdapterBundles(getConnectionLines()))
            {
                bool isHovered = includeHover &&
                    hoveredLine != null &&
                    hoveredLine.StartConnector.ConnectorType == ConnectorType.Adapter &&
                    string.Equals(
                        hoveredBundleName,
                        bundle.Key,
                        StringComparison.OrdinalIgnoreCase);

                foreach (Point[] bodyPath in GetAdapterCableBodyPaths(bundle))
                {
                    graphics.DrawLines(outlinePen, bodyPath);
                    graphics.DrawLines(linePen, bodyPath);
                }

                if (!isHovered)
                    continue;

                IEnumerable<Point[]> hoverPaths = hoveredBundleEndpoint == null
                    ? GetAdapterCableBodyPaths(bundle)
                    : GetAdapterBundleEndpointPaths(bundle, hoveredBundleEndpoint);
                foreach (Point[] hoverPath in hoverPaths)
                {
                    graphics.DrawLines(hoverPen, hoverPath);
                    graphics.DrawLines(linePen, hoverPath);
                }
            }
        }

        /// <summary>
        /// 동일한 어댑터 케이블의 최상단·최하단 연결을 좌우 세로 몸통과
        /// 중앙 가로 몸통으로 묶어 사용자가 지정한 H형 케이블 선을 만든다.
        /// </summary>
        private List<Point[]> GetAdapterCableBodyPaths(
            IEnumerable<ConnectionLine> sourceLines)
        {
            List<Point[]> bodyPaths = new();
            List<ConnectionLine> lines = sourceLines.ToList();

            foreach (IGrouping<string, ConnectionLine> bundle in GetPhysicalAdapterBundles(lines))
            {
                Rectangle bounds = GetAdapterCableBodyBounds(bundle).Single();
                int centerY = bounds.Top + bounds.Height / 2;
                List<int> sourceYs = bundle
                    .Select(GetAdapterSourceConnector)
                    .Distinct()
                    .Select(connector => connector.GetConnectionPoint(this).Y)
                    .ToList();
                List<int> targetYs = bundle
                    .Select(GetAdapterTargetConnector)
                    .Distinct()
                    .Select(connector => connector.GetConnectionPoint(this).Y)
                    .ToList();

                // 좌우 몸통과 중앙 몸통
                bodyPaths.Add(new[]
                {
                    new Point(bounds.Left, Math.Min(sourceYs.Min(), centerY)),
                    new Point(bounds.Left, Math.Max(sourceYs.Max(), centerY))
                });
                bodyPaths.Add(new[]
                {
                    new Point(bounds.Right, Math.Min(targetYs.Min(), centerY)),
                    new Point(bounds.Right, Math.Max(targetYs.Max(), centerY))
                });
                bodyPaths.Add(new[]
                {
                    new Point(bounds.Left, centerY),
                    new Point(bounds.Right, centerY)
                });

                // 기존 개별 연결선 대신 각 P와 J를 몸통에 연결하는 짧은 분기선만 표시한다.
                foreach (Connector source in bundle
                    .Select(GetAdapterSourceConnector)
                    .Distinct())
                {
                    Point sourcePoint = source.GetConnectionPoint(this);
                    bodyPaths.Add(new[]
                    {
                        sourcePoint,
                        new Point(bounds.Left, sourcePoint.Y)
                    });
                }

                foreach (Connector target in bundle
                    .Select(GetAdapterTargetConnector)
                    .Distinct())
                {
                    Point targetPoint = target.GetConnectionPoint(this);
                    bodyPaths.Add(new[]
                    {
                        new Point(bounds.Right, targetPoint.Y),
                        targetPoint
                    });
                }
            }

            return bodyPaths;
        }

        private List<Point[]> GetAdapterBundleEndpointPaths(
            IEnumerable<ConnectionLine> sourceLines,
            Connector endpoint)
        {
            List<ConnectionLine> bundle = sourceLines.ToList();
            Rectangle bounds = GetAdapterCableBodyBounds(bundle).Single();
            List<ConnectionLine> relatedLines = bundle
                .Where(line =>
                    ReferenceEquals(GetAdapterSourceConnector(line), endpoint) ||
                    ReferenceEquals(GetAdapterTargetConnector(line), endpoint))
                .ToList();
            List<Point[]> paths = new();

            foreach (Connector source in relatedLines
                .Select(GetAdapterSourceConnector)
                .Distinct())
            {
                Point sourcePoint = source.GetConnectionPoint(this);
                paths.Add(new[]
                {
                    sourcePoint,
                    new Point(bounds.Left, sourcePoint.Y)
                });
            }

            foreach (Connector target in relatedLines
                .Select(GetAdapterTargetConnector)
                .Distinct())
            {
                Point targetPoint = target.GetConnectionPoint(this);
                paths.Add(new[]
                {
                    new Point(bounds.Right, targetPoint.Y),
                    targetPoint
                });
            }

            return paths;
        }

        private IEnumerable<IGrouping<string, ConnectionLine>> GetPhysicalAdapterBundles(
            IEnumerable<ConnectionLine> sourceLines)
        {
            return sourceLines
                .Where(line =>
                    !line.StartConnector.IsDisposed &&
                    !line.EndConnector.IsDisposed &&
                    line.StartConnector.ConnectorType == ConnectorType.Adapter &&
                    line.CableInfo != null)
                .GroupBy(
                    line => line.CableInfo!.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Where(group =>
                    group.Select(GetAdapterSourceConnector).Distinct().Count() > 1);
        }

        private bool IsPhysicalAdapterBundleLine(ConnectionLine line)
        {
            if (line.StartConnector.ConnectorType != ConnectorType.Adapter ||
                line.CableInfo == null)
            {
                return false;
            }

            return getConnectionLines()
                .Where(candidate =>
                    candidate.StartConnector.ConnectorType == ConnectorType.Adapter &&
                    string.Equals(
                        candidate.CableInfo?.Name,
                        line.CableInfo.Name,
                        StringComparison.OrdinalIgnoreCase))
                .Select(GetAdapterSourceConnector)
                .Distinct()
                .Count() > 1;
        }

        /// <summary>
        /// 복수 TJ에서 출발한 동일 어댑터 케이블의 P-J 전체 범위를 감싸는 몸통 영역을 계산한다.
        /// 개별 분기선에는 외피를 덧그리지 않아 기존 선 두께를 유지한다.
        /// </summary>
        private List<Rectangle> GetAdapterCableBodyBounds(
            IEnumerable<ConnectionLine> sourceLines)
        {
            List<Rectangle> bodyBounds = new();

            foreach (IGrouping<string, ConnectionLine> bundle in
                GetPhysicalAdapterBundles(sourceLines))
            {
                List<Point> connectorPoints = bundle
                    .SelectMany(line => new[]
                    {
                        line.StartConnector.GetConnectionPoint(this),
                        line.EndConnector.GetConnectionPoint(this)
                    })
                    .ToList();
                int left = connectorPoints.Min(point => point.X) + 24;
                int right = connectorPoints.Max(point => point.X) - 24;
                int top = connectorPoints.Min(point => point.Y);
                int bottom = connectorPoints.Max(point => point.Y);

                bodyBounds.Add(new Rectangle(
                    left,
                    top,
                    Math.Max(1, right - left),
                    Math.Max(1, bottom - top)));
            }

            return bodyBounds;
        }

        private static Connector GetAdapterSourceConnector(ConnectionLine line) =>
            line.StartConnector.Side == ConnectorSide.Left
                ? line.StartConnector
                : line.EndConnector;

        private static Connector GetAdapterTargetConnector(ConnectionLine line) =>
            line.StartConnector.Side == ConnectorSide.Right
                ? line.StartConnector
                : line.EndConnector;

        private bool IsInHoveredBranch(ConnectionLine line)
        {
            if (hoveredLine == null)
                return false;

            // 일대다 어댑터도 같은 P에서 출발한 전체 선이 아니라
            // 마우스가 올라간 개별 P→J 분기 하나만 강조한다.
            return ReferenceEquals(line, hoveredLine);
        }

        /// <summary>
        /// 둘 이상의 P가 공유하는 J 앞에 접속점을 표시해 선 겹침과 실제 합류를 구분한다.
        /// </summary>
        private void DrawAdapterSharedJunctions(Graphics graphics)
        {
            IEnumerable<IGrouping<Connector, ConnectionLine>> sharedTargets = getConnectionLines()
                .Where(line =>
                    !line.StartConnector.IsDisposed &&
                    !line.EndConnector.IsDisposed &&
                    line.StartConnector.ConnectorType == ConnectorType.Adapter)
                .GroupBy(GetAdapterTargetConnector)
                .Where(group => group.Select(GetAdapterSourceConnector).Distinct().Count() > 1);

            using SolidBrush outlineBrush = new SolidBrush(Color.FromArgb(155, 160, 170));
            using SolidBrush junctionBrush = new SolidBrush(Color.Black);
            foreach (IGrouping<Connector, ConnectionLine> targetGroup in sharedTargets)
            {
                Point targetPoint = targetGroup.Key.GetConnectionPoint(this);
                Point junctionPoint = new Point(targetPoint.X - 16, targetPoint.Y);
                graphics.FillEllipse(
                    outlineBrush,
                    junctionPoint.X - 5,
                    junctionPoint.Y - 5,
                    10,
                    10);
                graphics.FillEllipse(
                    junctionBrush,
                    junctionPoint.X - 3,
                    junctionPoint.Y - 3,
                    6,
                    6);
            }
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
