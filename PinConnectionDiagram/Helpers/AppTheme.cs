namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 기본 테마와 국방 테마에서 공통으로 사용하는 화면 색상을 제공한다.
    /// </summary>
    public static class AppTheme
    {
        private static Bitmap? defenseButton;
        private static Bitmap? defenseButtonPressed;
        private static Bitmap? defenseSupplyAdd;
        private static Bitmap? defenseSupplyAddPressed;
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
        public static Color ControllerHeader => IsDefense
            ? Color.FromArgb(128, 118, 78)
            : Color.FromArgb(145, 132, 238);

        /// <summary>
        /// 사용자가 선택한 테마를 명시적으로 적용한다.
        /// </summary>
        public static void SetDefense(bool isDefense) => IsDefense = isDefense;

        public static Image GetImage(string defaultResourceName, string defenseResourceName)
        {
            string resourceName = IsDefense ? defenseResourceName : defaultResourceName;
            return (Image)(Properties.Resources.ResourceManager.GetObject(resourceName)
                ?? throw new InvalidOperationException($"이미지 리소스를 찾을 수 없습니다: {resourceName}"));
        }

        public static Image GetStandardButtonImage(bool pressed)
        {
            if (!IsDefense)
                return pressed ? Properties.Resources.Button_push : Properties.Resources.Button;

            if (pressed)
                return defenseButtonPressed ??= CreateDefenseButtonImage(Properties.Resources.Button_push, true);

            return defenseButton ??= CreateDefenseButtonImage(Properties.Resources.Button, false);
        }

        public static Image GetSupplyAddImage(bool pressed)
        {
            if (!IsDefense)
                return pressed ? Properties.Resources.추가_push : Properties.Resources.추가;

            Bitmap source = (Bitmap)(Properties.Resources.ResourceManager.GetObject(
                pressed ? "추가_defense_push" : "추가_defense")
                ?? throw new InvalidOperationException("국방 테마 준비물 추가 이미지를 찾을 수 없습니다."));

            if (pressed)
                return defenseSupplyAddPressed ??= CreateShiftedImage(source, 1, 1);

            return defenseSupplyAdd ??= CreateShiftedImage(source, 1, 1);
        }

        private static Bitmap CreateShiftedImage(Bitmap source, int offsetX, int offsetY)
        {
            Bitmap result = new Bitmap(source.Width, source.Height);
            using Graphics graphics = Graphics.FromImage(result);
            graphics.Clear(Color.Transparent);
            graphics.DrawImageUnscaled(source, offsetX, offsetY);
            return result;
        }

        private static Bitmap CreateDefenseButtonImage(Bitmap source, bool pressed)
        {
            Bitmap result = new Bitmap(source.Width, source.Height);
            Color themeColor = pressed ? BackgroundEnd : DarkAccent;

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color sourceColor = source.GetPixel(x, y);
                    if (sourceColor.A == 0)
                    {
                        result.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    double brightness =
                        (sourceColor.R * 0.299D + sourceColor.G * 0.587D + sourceColor.B * 0.114D) / 255D;
                    double scale = 0.55D + brightness * 1.35D;
                    result.SetPixel(
                        x,
                        y,
                        Color.FromArgb(
                            sourceColor.A,
                            Math.Min(255, (int)Math.Round(themeColor.R * scale)),
                            Math.Min(255, (int)Math.Round(themeColor.G * scale)),
                            Math.Min(255, (int)Math.Round(themeColor.B * scale))));
                }
            }

            return result;
        }

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
