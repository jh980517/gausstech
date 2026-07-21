namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// 마우스 클릭은 허용하지만 키보드 포커스와 Enter 재실행을 받지 않는 버튼이다.
    /// </summary>
    public partial class NonSelectableButton : Button
    {
        public NonSelectableButton()
        {
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
        }

        protected override bool ShowFocusCues => false;
    }
}
