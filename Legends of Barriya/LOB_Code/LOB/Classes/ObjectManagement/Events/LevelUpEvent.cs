namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class LevelUpEvent : IEvent
    {
        public readonly bool mIsSpecialAction;

        public LevelUpEvent(bool isSpecialAction)
        {
            mIsSpecialAction = isSpecialAction;
        }

        public EventType GetEventType => EventType.LevelUpEvent;

        public string SaveEvent()
        {
            return "[" + GetEventType + ":()]";
        }
        
        public static IEvent LoadEvent()
        {
            return new LevelUpEvent(false);
        }
    }
}