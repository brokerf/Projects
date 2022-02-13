using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class GetAttacked : IAction
    {
        private float mCurrentHealthPoints;
        private float mAnimationHealthPoints;
        private readonly float mMaxHealthPoints;
        private GameObject mParentObject;
        private const float Cooldown = 0.1f;
        private const float Delay = 1f;
        private float mDelay;
        private const float MillisecondsPerSecond = 1000.0f;
        private const float PhalanxDamageReduction = 0.2f;
        private float mLastDamage;

        public GetAttacked(int maxHealth, float currentHealth = -1)
        {
            mMaxHealthPoints = maxHealth;
            mCurrentHealthPoints = maxHealth;
            mAnimationHealthPoints = maxHealth;
            mDelay = 0;
            if (currentHealth > 0)
            {
                mCurrentHealthPoints = currentHealth;
            }
        }

        /* processes the events:
         applies damage
         destroys object */
        public void Update(List<IEvent> events)
        {
            var timePassed = DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
            mLastDamage -= timePassed;
            if (mLastDamage <= 0 && mCurrentHealthPoints < mMaxHealthPoints)
            {
                mCurrentHealthPoints += 1;
                mLastDamage = 0.5f;
                mDelay = Delay;
            }
            // animation cooldown begin
            if (mAnimationHealthPoints - mCurrentHealthPoints != 0)
            {
                mAnimationHealthPoints -= (mAnimationHealthPoints - mCurrentHealthPoints) * timePassed / Cooldown;
                if (mAnimationHealthPoints <= mCurrentHealthPoints + 5)
                {
                    mAnimationHealthPoints = mCurrentHealthPoints;
                }
                mAnimationHealthPoints -= (mAnimationHealthPoints - mCurrentHealthPoints) * timePassed / Cooldown;
            }
            else if (mDelay > 0)
            {
                mDelay -= timePassed;
                if (mDelay < 0)
                {
                    mDelay = 0;
                }
            }
            // animation cooldown end

            if (mCurrentHealthPoints == 0 && Math.Abs(mAnimationHealthPoints - mCurrentHealthPoints) == 0)
            {
                GameObjectManagement.mIdsToDelete.Add(mParentObject.mObjectId);
            }

            if (events.Count == 0)
            {
                return;
            }

            mDelay = Delay;

            var phalanxModeActive = false;
            if (mParentObject.mName == ObjectType.Phalanx)
            {
                var phalanxAction = (PhalanxAction)mParentObject.mActions.First(action => action is PhalanxAction);
                phalanxModeActive = phalanxAction.GetPhalanxMode();
            }

            foreach (var getAttackedEvent in events.Cast<GetAttackedEvent>().ToList())
            {
                getAttackedEvent.mTimeTillImpact -= timePassed;

                if (getAttackedEvent.mTimeTillImpact > 0)
                {
                    continue;
                }

                var damage = phalanxModeActive ? PhalanxDamageReduction * getAttackedEvent.mDamage : getAttackedEvent.mDamage;
                mCurrentHealthPoints -= damage;
                if (damage > 0)
                {
                    mLastDamage = 10f; // Change here for time until healing
                }
                if (mCurrentHealthPoints > mMaxHealthPoints)
                {
                    mCurrentHealthPoints = mMaxHealthPoints;
                }
                if (mCurrentHealthPoints > 0)
                {
                    if (getAttackedEvent.mDamage > 0)
                    {
                        Particle.mForegroundParticles.AddRange(Particle.BloodEffect(20, getAttackedEvent.mAttackerPosition.X, getAttackedEvent.mAttackerPosition.Y, mParentObject.mObjectPosition.X, mParentObject.mObjectPosition.Y));
                    }

                    mParentObject.mObjectEvents[GetEventType].Remove(getAttackedEvent);
                    continue;
                }

                mCurrentHealthPoints = 0;
                mParentObject.mObjectEvents[GetEventType].Remove(getAttackedEvent);
                break;
            }

            if (!mParentObject.mIsPlayer)
            {
                foreach (var action in mParentObject.mActions.Where(action => !(action is GetAttacked)))
                {
                    mParentObject.mObjectEvents[action.GetEventType].Clear();
                }
            }

            DataStorage.mUnitAnimationOffset[mParentObject.mObjectPosition] = Vector2.Zero;
        }

        public float GetDelay()
        {
            return mDelay;
        }

        public float GetAnimationHealthPoints()
        {
            return mAnimationHealthPoints;
        }

        public float GetMaxHealthPoints()
        {
            return mMaxHealthPoints;
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>()
            {
                ("Health", (int)mCurrentHealthPoints),
                ("MaxHealth", (int)mMaxHealthPoints)
            };
        }

        public EventType GetEventType => EventType.GetAttackedEvent;

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":(" + (int)mMaxHealthPoints + "/" + (int)mCurrentHealthPoints + ")]";
        }


        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");
            var notFailed = int.TryParse(stats[0], out var maxHealthPoints);
            notFailed = int.TryParse(stats[1], out var currentHealthPoints) && notFailed;
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction GetAttacked");
            }
            return new GetAttacked(maxHealthPoints, currentHealthPoints);
        }
    }
}