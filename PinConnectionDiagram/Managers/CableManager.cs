using PinConnectionDiagram.Models;

namespace PinConnectionDiagram.Managers
{
    /// <summary>
    /// 현재 세션에서 등록한 시험 준비물 데이터를 관리한다.
    /// </summary>
    public class CableManager
    {
        public List<CableInfo> Cables { get; } = new();

        public void Add(CableInfo info)
        {
            Cables.Add(info);
        }

        public void Remove(CableInfo info)
        {
            Cables.Remove(info);
        }

        public void Replace(CableInfo current, CableInfo replacement)
        {
            int index = Cables.FindIndex(cable => cable.Id == current.Id);
            if (index >= 0)
                Cables[index] = replacement;
        }

        public void Clear()
        {
            Cables.Clear();
        }

        
    }
}
