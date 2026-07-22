using System.Text;

namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// 배포 환경에서 발생한 예외를 사용자 로컬 AppData에 기록한다.
    /// </summary>
    public static class ErrorLogger
    {
        private static readonly object SyncRoot = new();

        public static string LogDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GaussTech",
            "TestCableConnectionManager",
            "Logs");

        public static string Write(Exception exception, string source)
        {
            Directory.CreateDirectory(LogDirectory);
            string path = Path.Combine(LogDirectory, $"error-{DateTime.Now:yyyyMMdd}.log");
            StringBuilder message = new StringBuilder()
                .AppendLine(new string('=', 80))
                .AppendLine($"Time   : {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}")
                .AppendLine($"Source : {source}")
                .AppendLine($"Version: {Application.ProductVersion}")
                .AppendLine($"OS     : {Environment.OSVersion}")
                .AppendLine(exception.ToString());

            lock (SyncRoot)
                File.AppendAllText(path, message.ToString(), Encoding.UTF8);

            return path;
        }
    }
}
