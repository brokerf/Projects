using System;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class RemoveBuilding : IAction
    {
        private GameObject mParentObject;
        private float mProgress;
        private const float MillisecondsPerSecond = 1000.0f;

        public RemoveBuilding()
        {
            mProgress = -1;
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

            if (Math.Abs(mProgress - (-1)) == 0)
            {
                mProgress = 1;
                return;
            }

            mProgress -= DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
            var target = mParentObject.mObjectPosition;
            Particle.mForegroundParticles.Add(Particle.BuildingEffect(target.X,
                target.Y));

            if (mParentObject.mName == ObjectType.New1Building)
            {
                foreach (var gameObject in DataStorage.mGameObjects.Values)
                {
                    if (gameObject.mName != ObjectType.Builder && gameObject.mName != ObjectType.Troll)
                    {
                        continue;
                    }

                    var progress = (ProgressBuilding) gameObject.mActions.First(action => action is ProgressBuilding);
                    progress.SetTargetToDefault();
                    gameObject.mObjectEvents[EventType.MoveEvent] = new List<IEvent>();
                }
            }

            if (mProgress > 0)
            {
                return;
            }

            if (mParentObject.mName == ObjectType.Mage1Tower)
            {
                var buildPortal = (BuildPortal)mParentObject.mActions.First(action => action is BuildPortal);
                if (mParentObject.mObjectEvents[EventType.PortalEvent].Any())
                {
                    buildPortal.Close();
                }
            }

            if (!mParentObject.GetAttributes().Item2.TryGetValue("Level", out var level))
            {
                level = 1;
            }

            var construction = (ConstructBuilding)mParentObject.mActions.FirstOrDefault(action => action is ConstructBuilding);
            var objectType = mParentObject.mName == ObjectType.New1Building ? construction!.mObjectToBuild : mParentObject.mName;
            var buildingCost = BuildingCosts.sBuildingCosts[objectType];
            var woodCost = buildingCost[0];
            var levelsMade = level * (level + 1) / 2 - 1;
            var goldCost = levelsMade * 10 + buildingCost[1];
            var ironCost = levelsMade * 10 + buildingCost[2];
            var manaCost = buildingCost[3];
            ReturnResources(woodCost, goldCost, ironCost, manaCost);
            GameObjectManagement.mIdsToDelete.Add(mParentObject.mObjectId);
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        private void ReturnResources(int wood, int gold, int iron, int mana)
        {
            var ratio = mParentObject.mName == ObjectType.New1Building ? 1 : 0.5;
            var isPlayer = mParentObject.mIsPlayer;
            DataStorage.mGameStatistics[isPlayer][ResourceType.Wood] += (int) (wood * ratio);
            DataStorage.mGameStatistics[isPlayer][ResourceType.Gold] += (int) (gold * ratio);
            DataStorage.mGameStatistics[isPlayer][ResourceType.Iron] += (int) (iron * ratio);
            DataStorage.mGameStatistics[isPlayer][ResourceType.Mana] += (int) (mana * ratio);
        }

        public EventType GetEventType => EventType.RemoveBuildingEvent;

        public string SaveAction()
        {
            return "[" + GetEventType + ":()]";
        }

        public static IAction LoadAction()
        {
            return new SpecialAction();
        }
    }
}
