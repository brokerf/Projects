using System.Collections.Generic;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal interface IAction
    {
        internal const float MillisecondsPerSecond = 1000f;
        void Update(List<IEvent> events);
        IEnumerable<(string, int)> GetAttribute();
        public EventType GetEventType { get; }
        public void SetParentObject(GameObject parent);
        string SaveAction();
    }
}