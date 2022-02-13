using LOB.Classes.ObjectManagement.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class PhalanxAction : IAction
    {
        private GameObject mParentObject;
        private bool mPhalanxMode;
        private Particle mBuffParticle;

        public PhalanxAction(bool phalanxMode = false)
        {
            mPhalanxMode = phalanxMode;
        }

        public void Update(List<IEvent> events)
        {
            var phalanxNearby = PhalanxNearby();
            if (mPhalanxMode)
            {
                if (mParentObject.mObjectEvents[EventType.MoveEvent].Any() || !phalanxNearby)
                {
                    mPhalanxMode = false;
                }
                else
                {
                    if (mBuffParticle == null || mBuffParticle.mTimeToLive <= 0)
                    {
                        var otherBuffActive = mParentObject.mIsSpeedUp || mParentObject.mIsStrengthUp;
                        mBuffParticle = Particle.PhalanxBuffEffect(mParentObject.mObjectPosition, otherBuffActive);
                        if (!Particle.mForegroundParticles.Contains(mBuffParticle))
                        {
                            Particle.mForegroundParticles.Add(mBuffParticle);
                        }
                    }
                }
            }

            if (events.Count == 0)
            {
                return;
            }

            var eEvent = events[0];

            if (phalanxNearby)
            {
                mPhalanxMode = true;
            }
            events.Remove(eEvent);
        }

        private bool PhalanxNearby()
        {
            var position = mParentObject.mObjectPosition;
            var currentObject = DataStorage.GetObject(new Point(position.X, position.Y - 1));
            if (currentObject != null && currentObject.mIsPlayer == mParentObject.mIsPlayer &&
                currentObject.mName == ObjectType.Phalanx)
            {
                return true;
            }

            currentObject = DataStorage.GetObject(new Point(position.X, position.Y + 1));
            if (currentObject != null && currentObject.mIsPlayer == mParentObject.mIsPlayer &&
                currentObject.mName == ObjectType.Phalanx)
            {
                return true;
            }

            currentObject = DataStorage.GetObject(new Point(position.X - 1, position.Y));
            if (currentObject != null && currentObject.mIsPlayer == mParentObject.mIsPlayer &&
                currentObject.mName == ObjectType.Phalanx)
            {
                return true;
            }

            currentObject = DataStorage.GetObject(new Point(position.X + 1, position.Y));
            return currentObject != null && currentObject.mIsPlayer == mParentObject.mIsPlayer &&
                   currentObject.mName == ObjectType.Phalanx;
        }

        public bool GetPhalanxMode() => mPhalanxMode;

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public EventType GetEventType => EventType.PhalanxEvent;

        public string SaveAction()
        {
            return "[" + GetEventType + ":("+mPhalanxMode+")]";
        }

        public static IAction LoadAction(string data)
        {
            if (!bool.TryParse(data, out var phalanxMode))
            {
                throw new FileLoadException("Couldn't load IAction PhalanxAction");
            }

            return new PhalanxAction(phalanxMode);
        }

        public void SetParentObject(GameObject gameObject)
        {
            mParentObject = gameObject;
        }
    }
}

