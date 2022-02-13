using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.Data;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class GatherResource : IAction
    {
        private readonly List<ResourceType> mPossibleResourceTypes;
        public ResourceType mCurrentResource;
        private GameObject mParentObject;
        public float mTimeNeeded;
        private const float TimeForResource = 20f;
        private const float TimeForResourceEnemy = 6f;

        public float GetTimeForResource() => TimeForResource;

        public EventType GetEventType => EventType.GatherResourceEvent;

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
            if (mParentObject.mName == ObjectType.Mine)
            {
                mParentObject.mObjectEvents[GetEventType] = new List<IEvent>{new GatherResourceEvent(mCurrentResource)};
            }
        }

        public string SaveAction()
        {
            var data = mPossibleResourceTypes.Select(resType => resType.ToString()).Aggregate((current, s) => current + "|" + s);
            return "[" + GetEventType + ":(" + data + "/" + mCurrentResource + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");
            var notFailed = Enum.TryParse<ResourceType>(stats[1], out var currentResource);
            stats = stats[0].Split("|");
            var possibleResources = new List<ResourceType>();
            foreach (var stat in stats)
            {
                notFailed = Enum.TryParse<ResourceType>(stat, out var possibleResource) && notFailed;
                possibleResources.Add(possibleResource);
            }
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction GatherResource");
            }
            return new GatherResource(possibleResources, currentResource);
        }

        public GatherResource(List<ResourceType> possibleResourceTypes, ResourceType currentResource = ResourceType.None)
        {
            mPossibleResourceTypes = possibleResourceTypes;
            mCurrentResource = currentResource;
        }

        public void Update(List<IEvent> events)
        {
           
            if (events.Count == 0)
            {
                return;
            }

            var resourceEvent = (GatherResourceEvent)events[^1];
            mParentObject.mObjectEvents[GetEventType] = new List<IEvent> {resourceEvent };

            if (resourceEvent.mPositionRequirement != null &&
                (mParentObject.mObjectPosition.X != resourceEvent.mPositionRequirement.Value.X ||
                 mParentObject.mObjectPosition.Y != resourceEvent.mPositionRequirement.Value.Y))
            {
                if (mParentObject.mObjectEvents[EventType.MoveEvent].Count == 0)
                {
                    mParentObject.mObjectState = ObjectState.Idle;
                    mParentObject.mObjectEvents[GetEventType] = new List<IEvent>();
                }
                else if (!mParentObject.mIsMoving)
                {
                    var newPointNextTo = GetNextResourcePoint(resourceEvent.mPositionRequirement.Value);
                    if (newPointNextTo == default)
                    {
                        mParentObject.mObjectState = ObjectState.Idle;
                        mParentObject.mObjectEvents[GetEventType] = new List<IEvent>();
                        return;
                    }

                    var path = DataStorage.mAStar.FindPath(new Node(0, mParentObject.mObjectPosition),
                        new Node(0, newPointNextTo),
                        DataStorage.mGameObjectPositions.Keys.ToList(),
                        mParentObject.mIsPlayer);
                    if (path.Count == 0)
                    {
                        return;
                    }

                    mParentObject.mObjectEvents[GetEventType] = new List<IEvent>();

                    DataStorage.AddEvent(mParentObject.mObjectId, new GatherResourceEvent(resourceEvent.mResourceType, newPointNextTo)); 
                    DataStorage.AddEvent(mParentObject.mObjectId, new MoveEvent(path));
                }
                return;
            }

            var newType = resourceEvent.mResourceType;
            if (!mPossibleResourceTypes.Contains(newType) || newType == ResourceType.None)
            {
                events.Remove(resourceEvent);
                return;
            }

            mParentObject.mObjectState = ObjectState.Gathering;
            if (newType != mCurrentResource)
            {
                mCurrentResource = newType;
                resourceEvent.mProgress = 0;
            }
            resourceEvent.mProgress += DataStorage.mGameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;

            TryMakeResource(resourceEvent);
        }

        private Point GetNextResourcePoint(Point oldPosition)
        {
            foreach (var point in DataStorage.mAStar.mNeighbors[oldPosition])
            {
                var resPoint = DataStorage.GetObject(point);
                if (resPoint == null)
                {
                    continue;
                }

                if (resPoint.mName >= ObjectType.Tree)
                {
                    return DataStorage.mAStar.mNeighbors[point].OrderBy(position => Math.Sqrt(Math.Pow(mParentObject.mObjectPosition.X - position.X, 2)
                        + Math.Pow(mParentObject.mObjectPosition.Y - position.Y, 2))).FirstOrDefault(originalPoint => !DataStorage.mGameObjectPositions.ContainsKey(originalPoint));
                }
            }

            return default;
        }
        
        private void TryMakeResource(GatherResourceEvent resourceEvent)
        {
            var level = mParentObject.GetAttributes().Item2.TryGetValue("Level", out var levelTry) ? levelTry : 1;

            ObjectFactory.sLevelToAmount[mCurrentResource].TryGetValue(level, out var amount);
            mTimeNeeded = (1f / amount) * (mParentObject.mIsPlayer ? TimeForResource : TimeForResourceEnemy);
            if (resourceEvent.mProgress < mTimeNeeded || mCurrentResource == ResourceType.None)
            {
                return;
            }
            resourceEvent.mProgress -= mTimeNeeded;
            if (mCurrentResource <= ResourceType.Mana && mParentObject.mIsPlayer)
            {
                Achievements.AddResource(mCurrentResource);
            }
            DataStorage.mGameStatistics[mParentObject.mIsPlayer][mCurrentResource] += 1;
            Particle.mForegroundParticles.Add(Particle.ResourceEffect(mParentObject.mObjectPosition, mCurrentResource));
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            if (mParentObject.mIsPlayer)
            {
                return new List<(string, int)>();
            }

            return new List<(string, int)>() {("resourceTyp", (int)mCurrentResource)}; // (List<(string, int)>)Enumerable.Empty<(string, int)>(); Vincent?? ;)
        }
    }
}