
namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class BuildProgressEvent : IEvent
    {

        public EventType GetEventType => EventType.BuildProgressEvent;
        
        public string SaveEvent()
        {
            return "[" + GetEventType + "]";
        }

        //TODO should be loaded
        public static IEvent LoadEvent()
        {
            return new BuildProgressEvent();
        }
    }
}