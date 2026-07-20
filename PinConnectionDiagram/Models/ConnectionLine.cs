using PinConnectionDiagram.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinConnectionDiagram.Models
{
    public class ConnectionLine
    {
        public Connector StartConnector { get; set; }
        public Connector EndConnector { get; set; }
        public CableInfo? CableInfo { get; set; }
        public bool Seleted { get; set; }
    }
}
