namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 여러 화면에서 사용하는 이미지 버튼의 공통 동작을 설정한다.
    /// </summary>
    public static class ButtonHelper
    {
        private sealed class ButtonImageState
        {
            public required Image Normal { get; set; }
            public required Image Pressed { get; set; }
        }

        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Button, ButtonImageState>
            ImageStates = new();

        public static void ApplyButtonEffect(
            Button btn,
            Image normal_image,
            Image pressed_image)
        {
            if (ImageStates.TryGetValue(btn, out ButtonImageState? existingState))
            {
                existingState.Normal = normal_image;
                existingState.Pressed = pressed_image;
                btn.BackgroundImage = normal_image;
                return;
            }

            ButtonImageState state = new ButtonImageState
            {
                Normal = normal_image,
                Pressed = pressed_image
            };
            ImageStates.Add(btn, state);

            // 클릭 가능한 버튼임을 직관적으로 알 수 있도록 손 모양 커서를 사용한다.
            btn.Cursor = Cursors.Hand;

            // 기본 WinForms 강조색 대신 준비된 이미지로 호버 효과를 표현한다.
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            btn.BackgroundImage = normal_image;

            btn.MouseEnter += (s, e) =>
            {
                if (btn.Enabled)
                    btn.BackgroundImage = state.Pressed;
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn.Enabled)
                    btn.BackgroundImage = state.Normal;
            };
        }

        public static void CancelButtonFunction(Button btn, Form form)
        {
            // Esc 키와 버튼 클릭이 모두 동일하게 폼을 닫도록 연결한다.
            btn.Cursor = Cursors.Hand;
            form.CancelButton = btn;
            btn.Click += (s, e) =>
            {
                form.Close();
            };
        }
    }
}
