using System;
using System.Collections.Generic;
using System.IO;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class LevelUp : IAction
    {
        public int mCurrentLevel;
        private GameObject mParentObject;
        private const int MaxLevel = 3;

        public LevelUp(int level = 1)
        {
            mCurrentLevel = level;
        }
        
        private const int UpgradeParticleAmount = 15;
        public void Update(List<IEvent> events)
        {
            if (events.Count == 0)
            {
                return;
            }

            var eEvent = (LevelUpEvent) events[0];

            if (!eEvent.mIsSpecialAction)
            {
                var costs = BuildingCosts.sUpgradeCosts[(mParentObject.mName, mCurrentLevel)];
                var i = (ResourceType)0;
                foreach (var cost in costs)
                {
                    if (DataStorage.mGameStatistics[mParentObject.mIsPlayer][i] < cost)
                    {
                        // TODO ERROR SCREEN
                        mParentObject.mObjectEvents[GetEventType].Remove(eEvent);
                        return;
                    }
                    i++;
                }

                i = 0;
                foreach (var cost in costs)
                {
                    DataStorage.mGameStatistics[mParentObject.mIsPlayer][i] -= cost;
                    i++;
                }

                if (mParentObject.mName == ObjectType.House && mCurrentLevel <= 2)
                {
                    DataStorage.mGameStatistics[mParentObject.mIsPlayer][ResourceType.MaxPopulation] += GameObject.sHousePopulationTuple[mCurrentLevel];
                }
            }

            Particle.mForegroundParticles.AddRange(Particle.UpgradeEffect(UpgradeParticleAmount, mParentObject.mObjectPosition.X, mParentObject.mObjectPosition.Y));
            mCurrentLevel = Math.Min(1 + mCurrentLevel, MaxLevel);
            mParentObject.mObjectEvents[GetEventType].Remove(eEvent);
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>
            {
                ("Level", mCurrentLevel)
            };
        }

        public EventType GetEventType => EventType.LevelUpEvent;

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":(" + mCurrentLevel + ")]";
        }


        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var notFailed = int.TryParse(data, out var currentLevel);
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction LevelUp");
            }
            return new LevelUp(currentLevel);
        }
    }
}