using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Events
{
    internal sealed class MoveEvent : IEvent
    {
        public Point mDestination;
        public readonly List<Point> mPath;
        public float mProgress;
        public bool mReached;
        public EventType GetEventType => EventType.MoveEvent;

        public MoveEvent(List<Point> path, float progress = 0, bool reached = false)
        {
            mDestination = path[0];
            mPath = path;
            mPath.Remove(mDestination);
            mProgress = progress;
            mReached = reached;
        }

        public string SaveEvent()
        {
            mPath.Insert(0, mDestination);
            var pathData = "?";
            foreach (var (x, y) in mPath)
            {
                pathData += "<" + x + "," + y + ">|";
            }
            pathData = pathData.Remove(pathData.Length - 1).Replace("?", string.Empty);
            return "[" + GetEventType + ":(" + pathData + "/" + mProgress + "/" + mReached + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IEvent LoadEvent(string data)
        {
            var stats = data.Split("/");
            var path = new List<Point>();
            var notFailed = true;
            foreach (var stat in stats[0].Split("|").Select(stat => stat.Replace("<", string.Empty).Replace(">", string.Empty)))
            {
                var position = stat.Split(",");
                notFailed = int.TryParse(position[0], out var x) && notFailed;
                notFailed = int.TryParse(position[1], out var y) && notFailed;
                path.Add(new Point(x, y));
            }

            var progress = Convert.ToSingle(stats[1]);
            notFailed = bool.TryParse(stats[2], out var reached) && notFailed;
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IEvent MoveEvent");
            }
            return new MoveEvent(path, progress, reached);
        }
    }
}