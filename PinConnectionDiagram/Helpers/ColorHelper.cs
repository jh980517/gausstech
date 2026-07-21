using System;
namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 케이블 분류에 맞는 연결도 헤더 색상을 제공한다.
    /// </summary>
    public static class ColorHelper
    {
        public static Color GetMapHeaderColor(string category) =>
            AppTheme.GetCategoryColor(category);
    }
}
