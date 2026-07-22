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
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, args) =>
                HandleFatalException(args.Exception, "Windows Forms UI");
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                Exception exception = args.ExceptionObject as Exception
                    ?? new Exception(args.ExceptionObject?.ToString() ?? "알 수 없는 오류");
                HandleFatalException(exception, "Application Domain");
            };
            Application.Run(new Main());
        }

        private static void HandleFatalException(Exception exception, string source)
        {
            string logPath;
            try
            {
                logPath = Helpers.ErrorLogger.Write(exception, source);
            }
            catch
            {
                logPath = "오류 로그를 저장하지 못했습니다.";
            }

            MessageBox.Show(
                $"예기치 않은 오류가 발생했습니다.\n프로그램을 다시 실행해 주세요.\n\n로그: {logPath}",
                "프로그램 오류",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
