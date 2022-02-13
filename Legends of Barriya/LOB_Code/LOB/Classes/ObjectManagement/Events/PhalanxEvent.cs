namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class PhalanxEvent : IEvent
    {
        public EventType GetEventType => EventType.SpecialEvent;

        public string SaveEvent()
        {
            return "[" + GetEventType + ":()]";
        }

        public static IEvent LoadEvent() => new PhalanxEvent();
    }
}
