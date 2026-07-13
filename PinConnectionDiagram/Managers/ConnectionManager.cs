using PinConnectionDiagram.Controls;
using PinConnectionDiagram.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinConnectionDiagram.Managers
{
    // Connection 생성
    // 선 그리기
    // DropZone 생성
    // DiagramCable 관리
    public class ConnectionManager
    {
        private PinConnector? firstConnector;

        public List<ConnectionInfo> Connections { get; } = new();

        public event Action<ConnectionInfo>? ConnectionCreated;

        public event Action<ConnectionInfo>? ConnectionRemoved;

        public void SelectConnector(PinConnector connector)
        {
            if (firstConnector == null)
            {
                firstConnector = connector;

                connector.Highlight(true);

                return;
            }
            
            if (firstConnector == connector)
            {
                connector.Highlight(false);

                firstConnector = null;

                return;
            }

            // 두번째 선택
            firstConnector.Highlight(false);

            ConnectionInfo connection = new ConnectionInfo
            {
                Start = firstConnector,
                End = connector
            };

            Connections.Add(connection);

            ConnectionCreated?.Invoke(connection);

            firstConnector = null;
        }

        public void Remove(ConnectionInfo connection)
        {
            if (!Connections.Contains(connection))
                return;

            Connections.Remove(connection);

            ConnectionRemoved?.Invoke(connection);
        }

        public void Clear()
        {
            foreach (ConnectionInfo connection in Connections.ToList())
            {
                Remove(connection);
            }

            firstConnector = null;
        }

        public void CancelSelectecd()
        {
            if (firstConnector == null)
                return;

            firstConnector.Highlight(false);

            firstConnector = null;
        }
    }
}
