using System;
using System.IO;
using LOB.Classes.ObjectManagement.Objects;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class GatherResourceEvent : IEvent
    {
        public Point? mPositionRequirement;
        public float mProgress;
        public readonly ResourceType mResourceType;

        public EventType GetEventType => EventType.GatherResourceEvent;

        public GatherResourceEvent(ResourceType resourceType, Point? positionRequirement = null, float progress = 0f)
        {
            mResourceType = resourceType;
            mPositionRequirement = positionRequirement;
            mProgress = progress;
        }

        public string SaveEvent()
        {
            return "[" + GetEventType + ":(" + mResourceType + "/" +
                   (mPositionRequirement.HasValue ? mPositionRequirement.Value.X + "|" + mPositionRequirement.Value.Y : string.Empty) 
                   + "/" + mProgress + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");
            var notFailed = Enum.TryParse<ResourceType>(stats[0], out var resourceType);
            Point? positionRequirement = null;
            if (stats[1] != string.Empty)
            {
                notFailed &= int.TryParse(stats[1].Split("|")[0], out var x);
                notFailed &= int.TryParse(stats[1].Split("|")[1], out var y);
                positionRequirement = new Point(x, y);
            }
            var progress = Convert.ToSingle(stats[2]);

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent GatherResourceEvent");
            }

            return new GatherResourceEvent(resourceType, positionRequirement, progress);
        }
    }
}