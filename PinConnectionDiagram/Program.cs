namespace PinConnectionDiagram
{
    internal static class Program
    {
        /// <summary>
        /// 애플리케이션 환경을 초기화하고 메인 폼을 실행한다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 고해상도 DPI와 기본 글꼴 등 WinForms 공통 환경을 적용한다.
            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}
