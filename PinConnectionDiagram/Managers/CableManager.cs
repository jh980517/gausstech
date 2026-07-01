using PinConnectionDiagram.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinConnectionDiagram.Managers
{
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

        
    }
}