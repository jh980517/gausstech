using System;
using System.Collections.Generic;
using System.Text;

namespace PinConnectionDiagram.Helpers
{
    public static class ColorHelper
    {
        public static Color GetCategoryColor(string category)
        {
            switch (category)
            {
                case "시험 대상 케이블":
                    return Color.FromArgb(255, 183, 99, 255); // Dark Purple

                case "지그 케이블":
                    return Color.FromArgb(255, 64, 139, 253); // Dark Blue

                case "어댑터 케이블":
                    return Color.FromArgb(255, 145, 223, 251); // Light Blue

                default:
                    return Color.Gray;
            }
        }
    }
}
