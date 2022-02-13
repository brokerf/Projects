using System.IO;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class SpecialEvent : IEvent
    {
        public Point mPosition;

        public SpecialEvent(Point position)
        {
            mPosition = position;
        }

        public EventType GetEventType => EventType.SpecialEvent;

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mPosition.X + "|" + mPosition.Y + ")]";
        }

        public static IEvent LoadEvent(string data)
        {
            var pos = data.Split("|");
            var notFailed = int.TryParse(pos[0], out var x);
            notFailed = int.TryParse(pos[1], out var y) && notFailed;

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent AttackEvent");
            }

            return new SpecialEvent(new Point(x, y));
        }
    }
}
