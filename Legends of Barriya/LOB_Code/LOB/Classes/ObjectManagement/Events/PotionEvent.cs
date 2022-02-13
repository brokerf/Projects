using System;
using System.IO;
using LOB.Classes.ObjectManagement.Objects;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class PotionEvent : IEvent
    {
        public Point mPosition;
        public readonly PotionType mPotionType;

        public PotionEvent(Point position, PotionType potionType)
        {
            mPosition = position;
            mPotionType = potionType;
        }

        public EventType GetEventType => EventType.PotionEvent;

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mPosition.X + "|" + mPosition.Y + "/" + mPotionType + ")]";
        }

        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");
            var pos = stats[0].Split("|");
            var notFailed = int.TryParse(pos[0], out var x);
            notFailed = int.TryParse(pos[1], out var y) && notFailed;
            notFailed = Enum.TryParse<PotionType>(stats[1], out var potionType) && notFailed;

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent AttackEvent");
            }

            return new PotionEvent(new Point(x, y), potionType);
        }
    }
}
