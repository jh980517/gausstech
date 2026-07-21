using System;
using System.Drawing.Drawing2D;

namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 폼과 사용자 컨트롤에서 반복 사용하는 선 그리기 기능을 제공한다.
    /// </summary>
    public static class DrawHelper
    {
        public static void DrawGlowLine(Graphics g, int width, int y, int thickness, Color color)
        {
            // 양쪽 끝은 투명하고 중앙은 선명한 수평 그라데이션을 만든다.
            Rectangle rect = new Rectangle(0, y, width, thickness);

            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                Color.Transparent,
                Color.Transparent,
                LinearGradientMode.Horizontal))
            {
                brush.InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.Transparent,
                        color,
                        Color.Transparent
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };

                g.FillRectangle(brush, rect);
            }
        }

        public static void DrawBorderLine(Graphics g, int width, int height, int thickness, Color color)
        {
            // Inset 정렬로 테두리가 컨트롤의 바깥쪽에서 잘리지 않게 한다.
            using (Pen pen = new Pen(color, thickness))
            {
                pen.Alignment = PenAlignment.Inset;

                g.DrawRectangle(
                    pen,
                    0,
                    0,
                    width,
                    height);
            }
        }
    }
}
