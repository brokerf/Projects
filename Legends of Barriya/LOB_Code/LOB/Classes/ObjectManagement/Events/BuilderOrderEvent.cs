using System.IO;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class BuilderOrderEvent : IEvent
    {
        public readonly Point mTarget;

        public BuilderOrderEvent(Point target)
        {
            mTarget = target;
        }

        public EventType GetEventType => EventType.BuilderOrderEvent;
        
        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mTarget.X + "|" + mTarget.Y + ")]";
        }

        //TODO should be loaded
        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");
            var notFailed = int.TryParse(stats[0].Split("|")[0], out var x);
            notFailed = int.TryParse(stats[0].Split("|")[1], out var y) && notFailed; 
            var target = new Point(x, y);
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent BuildEvent");
            }
            return new BuilderOrderEvent(target);
        }
    }
}