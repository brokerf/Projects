using System;
using System.Collections.Generic;
using System.IO;
using LOB.Classes.Data;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;

namespace LOB.Classes.ObjectManagement.Actions
{
    //The new1Building action for construction
    internal sealed class ConstructBuilding : IAction
    {
        private GameObject mParentObject;
        private int mProgress;
        public readonly ObjectType mObjectToBuild;
        public readonly ResourceType mResource;

        public ConstructBuilding(ObjectType objectToBuild, ResourceType resource = ResourceType.None, int progress = 0)
        {
            mObjectToBuild = objectToBuild;
            mResource = resource;
            mProgress = progress;
        }

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public void Update(List<IEvent> events)
        {   
            
            if (events.Count == 0)
            {
                return;
            }
            
            var objectPosition = mParentObject.mObjectPosition;

            mProgress += 10*events.Count;
            mParentObject.mObjectEvents[EventType.BuildProgressEvent].Clear();
            if (mProgress < 100)
            {
                return;
            }

            if (mObjectToBuild == ObjectType.Wall && mParentObject.mIsPlayer)
            {
                Achievements.AddWall();
            }

            var hasChanged = GameObjectManagement.mSelectedObjects.Contains(mParentObject.mObjectId);

            if (mParentObject.mIsPlayer)
            {
                GameObjectManagement.mPlayerBuildingManager.RemoveQueue(objectPosition);
            }
            else
            {
                GameObjectManagement.mEnemyBuildingManager.RemoveQueue(objectPosition);
            }

            GameObjectManagement.mIdsToDelete.Add(DataStorage.mGameObjectPositions[objectPosition]);
            GameObjectManagement.mObjectsToAdd.Add(ObjectFactory.BuildObject(mObjectToBuild, objectPosition, 0, mResource != ResourceType.None ? mResource : ResourceType.None, mParentObject.mIsPlayer, hasChanged: hasChanged));
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public float ReturnProgress()
        {
            return mProgress;
        }

        public EventType GetEventType => EventType.BuildProgressEvent;

        public string SaveAction()
        {
            return "[" + GetEventType + ":(" + mProgress + "/" + mResource + "/" + mObjectToBuild + ")]";
        }

        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");
            var notFailed = int.TryParse(stats[0], out var progress);
            notFailed = Enum.TryParse<ResourceType>(stats[1], out var resource) && notFailed;
            notFailed = Enum.TryParse<ObjectType>(stats[2], out var objectToBuild) && notFailed;
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction ConstructBuilding");
            }
            return new ConstructBuilding(objectToBuild, resource, progress);
        }
    }
}