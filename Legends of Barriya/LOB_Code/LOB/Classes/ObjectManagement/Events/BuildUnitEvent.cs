using System;
using System.IO;
using LOB.Classes.ObjectManagement.Objects;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class BuildUnitEvent : IEvent
    {
        public int mAmount;
        public readonly ObjectType mObjectType;

        public EventType GetEventType => EventType.BuildUnitEvent;

        public BuildUnitEvent(ObjectType objectType, int amount)
        {
            mObjectType = objectType;
            mAmount = amount;
        }

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mObjectType + "/" + mAmount + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");

            var notFailed = Enum.TryParse<ObjectType>(stats[0], out var objectType);
            notFailed = int.TryParse(stats[1], out var amount) && notFailed;

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent BuildUnitEvent");
            }
            return new BuildUnitEvent(objectType, amount);
        }
    }
}