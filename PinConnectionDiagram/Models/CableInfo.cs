using System;
using System.Collections.Generic;
using System.Text;

namespace PinConnectionDiagram.Models
{
    public class CableInfo
    {
        public Guid Id { get; } = Guid.NewGuid();
        public required string Category { get; set; }
        public required string Name { get; set; }
        public int Count { get; set; }
    }
}