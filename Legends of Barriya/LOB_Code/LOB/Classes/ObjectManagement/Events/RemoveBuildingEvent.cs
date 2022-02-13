namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class RemoveBuildingEvent : IEvent
    {
        public EventType GetEventType => EventType.RemoveBuildingEvent;

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + ")]";
        }

        public static IEvent LoadEvent()
        {
            return new RemoveBuildingEvent();
        }
    }
}
