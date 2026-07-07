using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;

namespace PinConnectionDiagram.Helpers
{
    public static class DrawHelper
    {
        public static void DrawGlowLine(Graphics g, int width, int y, int thickness, Color color)
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
            using(Pen pen = new Pen(color, thickness))
            {
                pen.Alignment = PenAlignment.Inset;

                g.DrawRectangle(
                    pen,
                    0, 
                    0, 
                    width-1, 
                    height-1);
            }
        }
    }
}
