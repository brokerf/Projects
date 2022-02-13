using System.IO;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class AttackEvent : IEvent
    {
        public int mVictimId;
        public readonly bool mShouldFollow;
        public EventType GetEventType => EventType.AttackEvent;

        public AttackEvent(int victimId, bool shouldFollow)
        {
            mVictimId = victimId;
            mShouldFollow = shouldFollow;
        }

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mVictimId + "/" + mShouldFollow + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");
            var notFailed = int.TryParse(stats[0], out var victimId);
            notFailed = bool.TryParse(stats[1], out var shouldFollow) && notFailed;

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent AttackEvent");
            }
            // TODO Save should follow / finished
            return new AttackEvent(victimId,  shouldFollow);
        }
    }
}