using PinConnectionDiagram.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinConnectionDiagram.Models
{
    public class ConnectionInfo
    {
        public PinConnector Start { get; set; }
        public PinConnector End { get; set; }
        public DiagramCable? cable { get; set; }
        public Panel DropZone { get; set; }
        public string StartPin => Start.SelectedPin;
        public string EndPin => End.SelectedPin;
    }
}
