namespace PinConnectionDiagram.Helpers
{
    /// <summary>프로젝트 테마가 적용된 공통 메시지 대화상자를 제공한다.</summary>
    public static class ProjectMessageBox
    {
        public static DialogResult Show(
            string message,
            string title = "알림",
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            using ProjectMessageForm form = new ProjectMessageForm(
                message, title, buttons, icon, defaultButton);
            Form? owner = Form.ActiveForm;
            return owner == null ? form.ShowDialog() : form.ShowDialog(owner);
        }
    }
}
