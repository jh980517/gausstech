namespace PinConnectionDiagram.Models
{
    /// <summary>다시 편집할 수 있는 시험 연결 프로젝트 파일의 최상위 데이터다.</summary>
    public sealed class ProjectFileData
    {
        public int FormatVersion { get; set; } = 1;
        public string ApplicationVersion { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public bool UseDefenseTheme { get; set; }
        public List<CableInfo> Supplies { get; set; } = new();
        public List<ProjectTJState> TJs { get; set; } = new();
        public List<ProjectConnectionState> Connections { get; set; } = new();
    }

    public sealed class ProjectTJState
    {
        public int TJNumber { get; set; }
        public bool IsOn { get; set; }
        public List<ProjectPanelState> Panels { get; set; } = new();
    }

    public sealed class ProjectPanelState
    {
        public ConnectorType ConnectorType { get; set; }
        public List<ProjectConnectorState> Connectors { get; set; } = new();
    }

    public sealed class ProjectConnectorState
    {
        public ConnectorSide Side { get; set; }
        public string ConnectorName { get; set; } = string.Empty;
    }

    public sealed class ProjectConnectionState
    {
        public ProjectConnectorKey Start { get; set; } = new();
        public ProjectConnectorKey End { get; set; } = new();
        public Guid? CableId { get; set; }
    }

    public sealed class ProjectConnectorKey
    {
        public int TJNumber { get; set; }
        public ConnectorType ConnectorType { get; set; }
        public ConnectorSide Side { get; set; }
        public int Index { get; set; }
    }
}
