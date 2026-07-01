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
                    return Color.FromArgb(255, 183, 99, 255); // 보라색

                case "지그 케이블":
                    return Color.FromArgb(255, 51, 255, 190); // 민트색

                case "어댑터 케이블":
                    return Color.FromArgb(255, 51, 150, 255); // 파랑색

                default:
                    return Color.Gray;
            }
        }
    }
}
