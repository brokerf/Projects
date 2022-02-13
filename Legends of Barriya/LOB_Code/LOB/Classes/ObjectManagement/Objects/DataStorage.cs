using System.Collections.Generic;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Objects
{
    // Provides the main data storage needed by several components
    internal static class DataStorage
    {
        public static Dictionary<int, List<IEvent>> mGameEvents = new Dictionary<int, List<IEvent>>();
        public static Dictionary<Point, int> mGameObjectPositions;
        public static Dictionary<int, GameObject> mGameObjects;
        public static Dictionary<bool, List<int>> mPlayerById; // true contains player's object IDs, false contains ai's object IDs
        public static AStar mAStar;

        public static Dictionary<bool, Dictionary<ResourceType, int>> mGameStatistics; // true contains player's statistics, false contains ai's statistics
        public static GameTime mGameTime;
        public static Dictionary<Point, Vector2> mUnitAnimationOffset = new Dictionary<Point, Vector2>();
        public static (string, int) mBuildError = (string.Empty, -1);
        public static bool mPopUpObjectHasDied = false;

        /// <summary>
        /// returns the object at the position, or null if there is none.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static GameObject GetObject(Point position)
        {
            return mGameObjectPositions.TryGetValue(position, out var objId) ? mGameObjects[objId] : null;
        }

        public static GameObject GetObject(int id)
        {
            return mGameObjects.TryGetValue(id, out var gameObject) ? gameObject : null;
        }

        public static void AddObject(GameObject obj)
        {
            var id = obj.mObjectId;
            mGameObjects[id] = obj;
            mGameObjectPositions[obj.mObjectPosition] = id;
        }

        /// <summary>
        /// Clears all Events for Objects
        /// </summary>
        /// <param name="objectId"></param>
        public static void ClearEvents(int objectId)
        {
            if (!mGameEvents.ContainsKey(objectId))
            {
                return;
            }
            mGameEvents[objectId].Clear();
        }

        /// <summary>
        /// Adds an event for a specified object
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="ev"></param>
        public static void AddEvent(int objectId, IEvent ev)
        {
            if (!mGameEvents.ContainsKey(objectId))
            {
                mGameEvents[objectId] = new List<IEvent>();
            }
            mGameEvents[objectId].Add(ev);
        }


        public static int GetResource(ResourceType type, bool isPlayer)
        {
            return mGameStatistics[isPlayer][type];
        }
    }
}