namespace LOB.Classes.ObjectManagement.Events
{
    internal interface IEvent
    {
        public EventType GetEventType { get; }
        string SaveEvent();
    }
}