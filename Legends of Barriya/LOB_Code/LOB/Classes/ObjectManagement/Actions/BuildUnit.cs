using System.Collections.Generic;
using System.Linq;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class BuildUnit : IAction
    {
        private GameObject mParentObject;
        private const float Cooldown = 1;
        private const float CooldownMain = 1;
        private float mTimeToWait;
        private const float MillisecondsPerSecond = 1000.0f;

        public BuildUnit()
        {
            mTimeToWait = -1;
        }

        public void Update( List<IEvent> events)
        {
            if (events.Count == 0)
            {
                return;
            }

            if (mTimeToWait > 0)
            {
                mTimeToWait -= DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
                if (mTimeToWait < 0)
                {
                    mTimeToWait = 0;
                }
            }

            if (mTimeToWait + 1 == 0)
            {
                mTimeToWait = mParentObject.mName == ObjectType.Main1Building ? CooldownMain : Cooldown;
            }

            var unitEvent = (BuildUnitEvent)events[0];
            var isPlayer = mParentObject.mIsPlayer;
            var type = unitEvent.mObjectType;

            var objectPositions = DataStorage.mGameObjectPositions.Keys.ToList();
            objectPositions.AddRange(GameObjectManagement.mObjectsToAdd.Select(newObject => newObject.mObjectPosition));
            var positions = DataStorage.mAStar.GetTargets(mParentObject.mObjectPosition, 1, objectPositions).ToList();

            if (type.ToString().Contains("Hero"))
            {
                DataStorage.mGameStatistics[isPlayer][ResourceType.HeroAlive] = 1;
            }

            var isHero = type.ToString().Contains("Hero");

            if (CheckResourceAvailability(isPlayer, type)) // if true, player does not have enough resources for the action
            {
                mTimeToWait = -1;
                if (isHero)
                {
                    DataStorage.mGameStatistics[isPlayer][ResourceType.HeroAlive] = 0;
                }
                mParentObject.mObjectEvents[EventType.BuildUnitEvent].Remove(unitEvent);
                return;
            }

            if (mTimeToWait != 0 && !isHero)
            {
                return;
            }

            var position2 = positions.ElementAtOrDefault(0);

            if (position2 == default)
            {
                if (isPlayer)
                {
                    DataStorage.mBuildError = ("Make some space for new units.", mParentObject.mObjectId);
                }
                mParentObject.mObjectEvents[EventType.BuildUnitEvent].Remove(unitEvent);
                return;
            }

            DataStorage.mGameStatistics[isPlayer][ResourceType.Population] += 1;
            var newObject = ObjectFactory.BuildObject(type, position2, 0, ResourceType.None, isPlayer);
            GameObjectManagement.mObjectsToAdd.Add(newObject);
            DataStorage.mGameStatistics[isPlayer][ResourceType.Iron] -= ObjectFactory.sTypeToMoney[type][ResourceType.Iron];
            DataStorage.mGameStatistics[isPlayer][ResourceType.Gold] -= ObjectFactory.sTypeToMoney[type][ResourceType.Gold];
            DataStorage.mGameStatistics[isPlayer][ResourceType.Mana] -= ObjectFactory.sTypeToMoney[type][ResourceType.Mana];
            
            unitEvent.mAmount -= 1;
            mTimeToWait = -1;

            if (unitEvent.mAmount == 0)
            {
                mParentObject.mObjectEvents[EventType.BuildUnitEvent].Remove(unitEvent);
            }
        }

        /// <summary>
        /// Checks if enough resources are available for the action, returns true, if not enough resources are there
        /// </summary>
        private bool CheckResourceAvailability(bool isPlayer, ObjectType type)
        {
            // checks if enough population is left and if there is sufficient gold, iron and mana to buy the units
            if (DataStorage.mGameStatistics[isPlayer][ResourceType.MaxPopulation] -
                DataStorage.mGameStatistics[isPlayer][ResourceType.Population] == 0)
            {
                if (isPlayer)
                {
                    DataStorage.mBuildError = ("You have reached the population limit.", mParentObject.mObjectId);
                }
                return true;
            }
            if (DataStorage.mGameStatistics[isPlayer][ResourceType.Iron] < ObjectFactory.sTypeToMoney[type][ResourceType.Iron])
            {
                if (isPlayer)
                {
                    DataStorage.mBuildError = ("You do not have enough iron to afford this unit.", mParentObject.mObjectId);
                }
                return true;
            }
            if (DataStorage.mGameStatistics[isPlayer][ResourceType.Gold] < ObjectFactory.sTypeToMoney[type][ResourceType.Gold])
            {
                if (isPlayer)
                {
                    DataStorage.mBuildError = ("You do not have enough gold to afford this unit.", mParentObject.mObjectId);
                }
                return true;
            }

            if (DataStorage.mGameStatistics[isPlayer][ResourceType.Mana] >=
                ObjectFactory.sTypeToMoney[type][ResourceType.Mana])
            {
                return false;
            }

            if (isPlayer)
            {
                DataStorage.mBuildError = ("You do not have enough mana to afford this unit.", mParentObject.mObjectId);
            }

            return true;
        }

        public float GetTimeToWait() => mTimeToWait;

        public float GetCooldown() => Cooldown;
        public float GetCooldownMain() => CooldownMain;

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public EventType GetEventType => EventType.BuildUnitEvent;

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":()]";
        }

        public static IAction LoadAction()
        {
            return new BuildUnit();
        }
    }
}