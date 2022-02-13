using System.Collections.Generic;
using System.IO;
using LOB.Classes.Data;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class BuildPortal : IAction
    {
        public Node mPortalPosition;
        private Node mPortalExitPosition;
        private GameObject mParentObject;
        private ParticleEmitter mEmitter;

        public BuildPortal(Node portalExitPosition = default, Node portalPosition = default)
        {
            mPortalExitPosition = portalExitPosition;
            mPortalPosition = portalPosition;
        }

        private const int TimeUntilCost = 5;
        private const int ManaCost = 7;

        public void Update(List<IEvent> events)
        {
            if (events.Count == 0)
            {
                if (mPortalPosition == default)
                {
                    return;
                }
                Close();
                return;
            }

            var portalEvent = (PortalOpeningEvent)events[0];

            if (portalEvent.mShouldClose)
            {
                if (mPortalPosition == default)
                {
                    return;
                }
                Close();
                return;
            }

            if (mPortalPosition == default && !CloseIfNecessary(mParentObject.mIsPlayer, events))
            {
                if (mParentObject.mName != ObjectType.Orc1Hero)
                {
                    DataStorage.mGameStatistics[mParentObject.mIsPlayer][ResourceType.Mana] -= ManaCost;
                }
                (mPortalPosition, mPortalExitPosition) = DataStorage.mAStar.OpenPortal(mParentObject.mIsPlayer);
                Particle.mForegroundParticles.AddRange(Particle.PortalOpeningEffect(mPortalPosition.mPosition.X, mPortalPosition.mPosition.Y, mParentObject.mIsPlayer, 0));
                Particle.mForegroundParticles.AddRange(Particle.PortalOpeningEffect(mPortalExitPosition.mPosition.X, mPortalExitPosition.mPosition.Y, mParentObject.mIsPlayer, 0));
                Particle.mForegroundParticles.AddRange(Particle.PortalOpeningEffect(mParentObject.mObjectPosition.X, mParentObject.mObjectPosition.Y, mParentObject.mIsPlayer, 1));
                if (mParentObject.mIsPlayer)
                {
                    Achievements.OpenPortal();
                }
                return;
            }

            if (mEmitter == null && mPortalPosition != default)
            {
                mEmitter = new ParticleEmitter(Particle.PortalEffect(mPortalPosition.mPosition.X, mPortalPosition.mPosition.Y, mPortalExitPosition.mPosition.X, mPortalExitPosition.mPosition.Y, mParentObject.mIsPlayer), 20, true);
                ParticleEmitter.mForegroundEmitters.Add(mEmitter);
            }

            portalEvent.mOpenedTime += DataStorage.mGameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
            if (portalEvent.mOpenedTime < TimeUntilCost)
            {
                return;
            }

            if (CloseIfNecessary(mParentObject.mIsPlayer, events))
            {
                return;
            }

            portalEvent.mOpenedTime -= TimeUntilCost;
            DataStorage.mGameStatistics[mParentObject.mIsPlayer][ResourceType.Mana] -= ManaCost;
        }

        public float GetTimeUntilCost() => TimeUntilCost;
        public float GetManaCost() => ManaCost;

        public void Close()
        {
            DataStorage.mAStar.ClosePortal(mPortalPosition, mPortalExitPosition, mParentObject.mIsPlayer);
            ParticleEmitter.mForegroundEmitters.Remove(mEmitter);
            mEmitter?.CloseEmitter();
            mEmitter = null;
            mPortalPosition = default;
            mPortalExitPosition = default;
        }
        private bool CloseIfNecessary(bool isPlayer, List<IEvent> events)
        {
            if (DataStorage.mGameStatistics[isPlayer][ResourceType.Mana] >= ManaCost)
            {
                return false;
            }
            Close();
            events.RemoveRange(0, events.Count);
            return true;
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public EventType GetEventType => EventType.PortalEvent;

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
            mParentObject.Death += UnloadPortal;
        }

        private void UnloadPortal()
        {
            DataStorage.mAStar.ClosePortal(mPortalPosition, mPortalExitPosition, mParentObject.mIsPlayer);
        }

        public string SaveAction()
        {
            if (mPortalPosition == default)
            {
                return "[" + GetEventType + ":(-1|-1/-1|-1)]";
            }
            return "[" + GetEventType + ":(" + mPortalExitPosition.mPosition.X + "|" + mPortalExitPosition.mPosition.Y + "/" + mPortalPosition.mPosition.X+"|"+ mPortalPosition.mPosition.Y + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");
            var subStats = stats[0].Split("|");
            var notFailed = int.TryParse(subStats[0], out var x1);
            notFailed = int.TryParse(subStats[1], out var y1) && notFailed;
            subStats = stats[1].Split("|");
            var (x, y) = (-1, -1);
            notFailed = notFailed && int.TryParse(subStats[0], out x);
            notFailed = notFailed && int.TryParse(subStats[1], out y);
            if (x == -1 && y == -1)
            {
                return new BuildPortal();
            }
            var portalPosition = new Node(0, new Point(x, y));
            var portalExitPosition = new Node(0, new Point(x1, y1));
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction BuildPortal");
            }
            return new BuildPortal(portalExitPosition, portalPosition);
        }
    }
}