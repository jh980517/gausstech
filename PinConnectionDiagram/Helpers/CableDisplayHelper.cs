using PinConnectionDiagram.Models;

namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 케이블 종류에 따른 화면 표시 이름과 아이템 크기를 계산한다.
    /// </summary>
    public static class CableDisplayHelper
    {
        public const int NormalItemHeight = 34;
        public const int MultilineItemHeight = 50;

        public static bool IsTestCable(CableInfo info) =>
            info.Category == "시험 대상 케이블";

        public static string GetDisplayName(CableInfo info)
        {
            if (!IsTestCable(info))
                return info.Name;

            string name = info.Name.Trim();
            int parenthesisIndex = name.LastIndexOf('(');
            if (parenthesisIndex <= 0 || !name.EndsWith(')'))
                return name;

            return name[..parenthesisIndex].TrimEnd()
                + Environment.NewLine
                + name[parenthesisIndex..];
        }

        public static int GetItemHeight(CableInfo info) =>
            GetDisplayName(info).Contains(Environment.NewLine, StringComparison.Ordinal)
                ? MultilineItemHeight
                : NormalItemHeight;

        public static int GetItemWidth(CableInfo info, Font font)
        {
            int textWidth = GetDisplayName(info)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Max(line => TextRenderer.MeasureText(
                    line,
                    font,
                    new Size(int.MaxValue, MultilineItemHeight),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width);

            int minimumWidth = IsTestCable(info) ? 120 : 90;
            int maximumWidth = IsTestCable(info) ? 280 : 180;
            return Math.Clamp(textWidth + 34, minimumWidth, maximumWidth);
        }
    }
}
