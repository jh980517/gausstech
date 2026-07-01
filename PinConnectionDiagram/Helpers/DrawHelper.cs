using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;

namespace PinConnectionDiagram.Helpers
{
    public static class DrawHelper
    {
        public static void DrawGlowLine(Graphics g, int width, int y, int thickness)
        {
            Rectangle rect = new Rectangle(0, y, width, thickness);

            using(LinearGradientBrush brush = new LinearGradientBrush(
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
                        Color.FromArgb(180, 100, 220, 255),
                        Color.Transparent
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };

                g.FillRectangle(brush, rect);
            }
        }
    }
}
