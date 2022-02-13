using LOB.Classes.ObjectManagement.Objects;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class SpecialAction : IAction
    {
        private GameObject mParentObject;
        private const float DwarfBonus = 5.0f;
        private const float BuilderBonusHealthPoints = 0.02f;
        private const float HumanHeroCooldown = 30;
        private const float OrcHeroCooldown = 30;
        private const float MillisecondsPerSecond = 1000.0f;
        private float mTimeToWait;

        private float mTimeTillNextParticleEffect;

        public SpecialAction()
        {
            mTimeToWait = 0;
        }

        public void Update(List<IEvent> events)
        {
            var passedTime = DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
            if (mTimeToWait != 0 && mParentObject.mName != ObjectType.Dwarf1Hero)
            {
                mTimeToWait -= passedTime;
                if (mTimeToWait < 0)
                {
                    mTimeToWait = 0;
                }
                return;
            }

            if (events.Count == 0)
            {
                return;
            }

            var eEvent = events[0];
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (mParentObject.mName)
            {
                case ObjectType.Human1Hero:
                    Particle.MagicGlowEffect(mParentObject.mObjectPosition);
                    var mineEvent = (SpecialEvent) eEvent;
                    if (mineEvent.mPosition != new Point(-1, -1))
                    {
                        var mineToUpgrade = DataStorage.GetObject(mineEvent.mPosition);
                        if (!mineToUpgrade.mObjectEvents.ContainsKey(EventType.LevelUpEvent))
                        {
                            mineToUpgrade.mObjectEvents[EventType.LevelUpEvent] = new List<IEvent>();
                        }

                        mineToUpgrade.mObjectEvents[EventType.LevelUpEvent]
                            .Add(new LevelUpEvent(true));
                    }
                    events.Remove(eEvent);
                    mTimeToWait = HumanHeroCooldown;
                    break;
                case ObjectType.Dwarf1Hero:
                    mTimeTillNextParticleEffect -= passedTime;
                    
                    var vision = mParentObject.GetAttributes().Item2["Vision"];
                    var foundSubject = false;
                    for (var x = -vision; x <= vision; x++)
                    {
                        for (var y = -vision; y <= vision; y++)
                        {
                            var currentObject = DataStorage.GetObject(new Point(mParentObject.mObjectPosition.X + x,
                                mParentObject.mObjectPosition.Y + y));
                            if (currentObject == null || currentObject.mIsPlayer != mParentObject.mIsPlayer ||
                                (x == 0 && y == 0) || !currentObject.CanAttack())
                            {
                                continue;
                            }

                            currentObject.Strengthen(DwarfBonus);
                            foundSubject = true;
                        }
                    }
                    if (mTimeTillNextParticleEffect <= 0 && foundSubject)
                    {
                        mTimeTillNextParticleEffect = .5f;
                        Particle.MagicGlowEffect(mParentObject.mObjectPosition);
                    }
                    break;
                case ObjectType.Orc1Hero:
                    Particle.MagicGlowEffect(mParentObject.mObjectPosition);
                    mTimeToWait = OrcHeroCooldown;
                    mParentObject.mObjectEvents[EventType.PortalEvent] = new List<IEvent> { new PortalOpeningEvent(false) };
                    events.Remove(eEvent);
                    break;
                case ObjectType.Troll:
                case ObjectType.Builder:
                    var builderEvent = (SpecialEvent)eEvent;
                    var building = DataStorage.GetObject(builderEvent.mPosition);

                    if (builderEvent.mPosition == new Point(-1, -1) || building == null)
                    {
                        events.Remove(eEvent);
                        return;
                    }
                    if (mParentObject.mObjectEvents[EventType.MoveEvent].Any())
                    {
                        var moveEvent = (MoveEvent) mParentObject.mObjectEvents[EventType.MoveEvent][^1];
                        if (moveEvent.mPath.Any() && !DataStorage.mAStar.mNeighbors[moveEvent.mPath[^1]].Contains(builderEvent.mPosition))
                        {
                            events.Remove(eEvent);
                            return;
                        }
                    }

                    if (DataStorage.mAStar.mNeighbors[mParentObject.mObjectPosition].Contains(builderEvent.mPosition))
                    {
                        building.mObjectEvents[EventType.GetAttackedEvent].Add(new GetAttackedEvent(-BuilderBonusHealthPoints, 0f, mParentObject.mObjectPosition));
                    }
                    break;
                default:
                    return;
            }
        }

        public float GetTimeToWait()
        {
            return mTimeToWait;
        }

        public float GetMaxCooldown()
        {
            return mParentObject.mName == ObjectType.Human1Hero ? HumanHeroCooldown : OrcHeroCooldown;
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public EventType GetEventType => EventType.SpecialEvent;

        public string SaveAction()
        {
            return "[" + GetEventType + ":()]";
        }

        public static IAction LoadAction()
        {
            return new SpecialAction();
        }

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }
    }
}
