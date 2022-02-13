using System.IO;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class GetAttackedEvent : IEvent
    {
        public EventType GetEventType => EventType.GetAttackedEvent;
        public readonly float mDamage;
        public float mTimeTillImpact;
        public Point mAttackerPosition;

        public GetAttackedEvent(float damage, float timeTillImpact, Point attackerPosition)
        {
            mDamage = damage;
            mTimeTillImpact = timeTillImpact;
            mAttackerPosition = attackerPosition;
        }

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mDamage + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IEvent LoadEvent(string data)
        {
            var notFailed = float.TryParse(data, out var damage);
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent GetAttackedEvent");
            }
            return new GetAttackedEvent(damage, 0, Point.Zero); //TODO Save next damage and particle and position
        }
    }
}