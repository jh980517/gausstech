using PinConnectionDiagram.Controls;
namespace PinConnectionDiagram.Models
{
    /// <summary>
    /// 두 커넥터 사이에 생성된 논리적 연결과 화면 표시 상태를 보관한다.
    /// </summary>
    public class ConnectionLine
    {
        public required Connector StartConnector { get; set; }
        public required Connector EndConnector { get; set; }
        public CableInfo? CableInfo { get; set; }
    }
}
