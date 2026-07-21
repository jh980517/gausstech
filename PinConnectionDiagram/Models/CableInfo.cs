using System;
namespace PinConnectionDiagram.Models
{
    /// <summary>
    /// 사용자가 등록한 시험 준비물 한 항목의 데이터를 나타낸다.
    /// </summary>
    public class CableInfo
    {
        // 이름이 같은 항목도 독립적으로 삭제할 수 있도록 고유 ID를 부여한다.
        public Guid Id { get; } = Guid.NewGuid();
        public required string Category { get; set; }
        public required string Name { get; set; }
        public int Count { get; set; }
    }
}
