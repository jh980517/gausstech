using PinConnectionDiagram.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PinConnectionDiagram.Helpers
{
    /// <summary>프로젝트 데이터를 사람이 확인 가능한 JSON 형식의 .tccm 파일로 저장하고 읽는다.</summary>
    public static class ProjectFileService
    {
        public const string Extension = ".tccm";
        public const string DialogFilter = "시험 연결 프로젝트 (*.tccm)|*.tccm|JSON 파일 (*.json)|*.json";

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void Save(string path, ProjectFileData data)
        {
            string fullPath = Path.GetFullPath(path);
            string? directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException("저장 폴더를 확인할 수 없습니다.");

            Directory.CreateDirectory(directory);
            string temporaryPath = fullPath + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, JsonSerializer.Serialize(data, Options));
                File.Move(temporaryPath, fullPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        public static ProjectFileData Load(string path)
        {
            ProjectFileData? data = JsonSerializer.Deserialize<ProjectFileData>(
                File.ReadAllText(path), Options);
            if (data == null || data.FormatVersion != 1)
                throw new InvalidDataException("지원하지 않거나 손상된 프로젝트 파일입니다.");
            if (data.TJs.Count == 0)
                throw new InvalidDataException("TJ 설정이 없는 프로젝트 파일입니다.");

            return data;
        }
    }
}
