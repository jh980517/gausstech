namespace PinConnectionDiagram.Helpers
{
    /// <summary>프로젝트 파일과 출력 파일의 최근 저장 폴더를 서로 독립적으로 관리한다.</summary>
    public static class DialogDirectoryStore
    {
        private static readonly string RootDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Test Cable Connection Manager");
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GaussTech",
            "TestCableConnectionManager",
            "dialog-directories.json");

        public static string ProjectDirectory { get; private set; } =
            Path.Combine(RootDirectory, "Projects");
        public static string ExportDirectory { get; private set; } =
            Path.Combine(RootDirectory, "Exports");

        static DialogDirectoryStore()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                    return;

                DirectorySettings? settings = System.Text.Json.JsonSerializer.Deserialize<DirectorySettings>(
                    File.ReadAllText(SettingsPath));
                if (!string.IsNullOrWhiteSpace(settings?.ProjectDirectory))
                    ProjectDirectory = settings.ProjectDirectory;
                if (!string.IsNullOrWhiteSpace(settings?.ExportDirectory))
                    ExportDirectory = settings.ExportDirectory;
            }
            catch
            {
                // 경로 설정이 손상돼도 파일 대화상자는 기본 폴더로 정상 동작해야 한다.
            }
        }

        public static string GetProjectDirectory() => EnsureDirectory(ProjectDirectory);
        public static string GetExportDirectory() => EnsureDirectory(ExportDirectory);

        public static void RememberProjectPath(string path)
        {
            ProjectDirectory = GetContainingDirectory(path, ProjectDirectory);
            SaveSettings();
        }

        public static void RememberExportPath(string path)
        {
            ExportDirectory = GetContainingDirectory(path, ExportDirectory);
            SaveSettings();
        }

        private static string GetContainingDirectory(string path, string fallback)
        {
            string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
            return string.IsNullOrWhiteSpace(directory) ? fallback : directory;
        }

        private static string EnsureDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static void SaveSettings()
        {
            try
            {
                string? directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(
                    SettingsPath,
                    System.Text.Json.JsonSerializer.Serialize(new DirectorySettings
                    {
                        ProjectDirectory = ProjectDirectory,
                        ExportDirectory = ExportDirectory
                    }));
            }
            catch
            {
                // 최근 폴더 저장 실패는 실제 프로젝트 및 출력 저장을 방해하지 않는다.
            }
        }

        private sealed class DirectorySettings
        {
            public string ProjectDirectory { get; set; } = string.Empty;
            public string ExportDirectory { get; set; } = string.Empty;
        }
    }
}
