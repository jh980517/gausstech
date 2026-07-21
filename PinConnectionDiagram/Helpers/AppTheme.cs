namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 기본 테마와 국방 테마에서 공통으로 사용하는 화면 색상을 제공한다.
    /// </summary>
    public static class AppTheme
    {
        public static bool IsDefense { get; private set; }

        public static Color Background => IsDefense
            ? Color.FromArgb(32, 38, 31)
            : Color.FromArgb(38, 38, 38);
        public static Color BackgroundEnd => IsDefense
            ? Color.FromArgb(73, 78, 52)
            : Color.FromArgb(62, 67, 119);
        public static Color DarkAccent => IsDefense
            ? Color.FromArgb(54, 64, 43)
            : Color.FromArgb(30, 46, 69);
        public static Color Accent => IsDefense
            ? Color.FromArgb(213, 198, 133)
            : Color.FromArgb(145, 223, 251);
        public static Color AccentHover => IsDefense
            ? Color.FromArgb(160, 139, 88)
            : Color.FromArgb(183, 99, 255);
        public static Color ContentBackground => IsDefense
            ? Color.FromArgb(205, 204, 178)
            : Color.FromArgb(212, 219, 230);

        public static void Toggle() => IsDefense = !IsDefense;

        public static Color GetCategoryColor(string category) => category switch
        {
            "지그 케이블" => IsDefense
                ? Color.FromArgb(79, 96, 61)
                : Color.FromArgb(63, 111, 159),
            "어댑터 케이블" => IsDefense
                ? Color.FromArgb(169, 183, 105)
                : Color.FromArgb(63, 202, 255),
            "시험 대상 케이블" => IsDefense
                ? Color.FromArgb(112, 105, 68)
                : Color.FromArgb(121, 103, 168),
            _ => Color.Gray
        };
    }
}
