using System;
using System.IO;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class PortalOpeningEvent : IEvent
    {
        public readonly bool mShouldClose;
        public float mOpenedTime;
        public EventType GetEventType => EventType.PortalEvent;

        public PortalOpeningEvent(bool shouldClose, float openedTime = 0)
        {
            mShouldClose = shouldClose;
            mOpenedTime = openedTime;
        }

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mShouldClose + "/" + mOpenedTime + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");
            var notFailed = bool.TryParse(stats[0], out var shouldClose);
            var openedTime = Convert.ToSingle(stats[1]);

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent PortalOpeningEvent");
            }
            return new PortalOpeningEvent(shouldClose, openedTime);
        }
    }
}