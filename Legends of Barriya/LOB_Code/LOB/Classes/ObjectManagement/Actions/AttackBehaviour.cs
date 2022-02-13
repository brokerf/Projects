using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using static LOB.Classes.Managers.SongManager;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class AttackBehaviour : IAction
    {
        private const double Cooldown = 0.5;

        private readonly int mNormalObjectDamage;
        private int mObjectDamage;
        private readonly int mAttackRange;
        private GameObject mParentObject;
        private double mTimeToWait;
        private float mTimeTillNewPath;

        private float mBuffTimeLeft;

        public AttackBehaviour(int damage, int attackRange, double timeToWait = 0)
        {
            mNormalObjectDamage = damage;
            mObjectDamage = damage;
            mAttackRange = attackRange;
            mTimeToWait = timeToWait;
            mTimeTillNewPath = 0;
        }

        public EventType GetEventType => EventType.AttackEvent;

        public int GetAttackRange() => mAttackRange;

        public void Strengthen(float ratio)
        {
            mObjectDamage = (int) (mObjectDamage * ratio);
            mBuffTimeLeft = 2f;
        }

        private void Destrengthen()
        {
            mObjectDamage = mNormalObjectDamage;
            mParentObject.mIsStrengthUp = false;
        }

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":(" + mObjectDamage + "/" + mAttackRange + "/" + mTimeToWait + ")]";
        }

        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");

            var notFailed = int.TryParse(stats[0], out var objectDamage);
            notFailed = int.TryParse(stats[1], out var attackRange) && notFailed;
            notFailed = double.TryParse(stats[2], out var timeToWait) && notFailed;
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction AttackBehaviour");
            }
            
            return new AttackBehaviour(objectDamage, attackRange, timeToWait);
        }

        public void Update(List<IEvent> events)
        {
            var passedTime = DataStorage.mGameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
            mBuffTimeLeft -= passedTime;
            if (mBuffTimeLeft <= 0)
            {
                Destrengthen();
            }
            if (mTimeToWait != 0)
            {
                mTimeToWait -= passedTime;
                if (mTimeToWait > 0)
                {
                    return;
                }
                mTimeToWait = 0;
                mParentObject.IsAttacking = false;
            }

            var atkEvents = events.Cast<AttackEvent>().ToList();
            if (atkEvents.Count == 0)
            {
                return;
            }

            var attackEvent = atkEvents.LastOrDefault(attack => attack.mShouldFollow) ?? atkEvents[^1];
            // takes the latest event with mShouldFollow and the latest, if there is none
            events.Clear();
            events.Add(attackEvent);

            var enemy = DataStorage.GetObject(attackEvent.mVictimId);
            if (enemy == null)
            {
                if (!attackEvent.mShouldFollow)
                {
                    events.Remove(attackEvent);
                    mParentObject.mObjectEvents[EventType.MoveEvent] = new List<IEvent>();
                    return;
                }

                var newEnemyId = NewGameObjectAfterDeath();
                if (newEnemyId == -1)
                {
                    events.Remove(attackEvent);
                    return;
                }
                attackEvent.mVictimId = newEnemyId;
                enemy = DataStorage.GetObject(newEnemyId);
            }

            if (!enemy.IsAttackable())
            {
                events.Remove(attackEvent);
                return;
            }

            if (enemy.mIsPlayer == mParentObject.mIsPlayer)
                // checks that objects are not from the same player               
            {
                events.Remove(attackEvent);
                return;
            }

            if (mAttackRange < Math.Abs(mParentObject.mObjectPosition.X - enemy.mObjectPosition.X) ||
                mAttackRange < Math.Abs(mParentObject.mObjectPosition.Y - enemy.mObjectPosition.Y))
            // checks if the enemy is in the attacker's range
            {
                if (!attackEvent.mShouldFollow)
                {
                    events.Remove(attackEvent);
                    return;
                }

                var latestMovement = (MoveEvent)mParentObject.mObjectEvents[EventType.MoveEvent].LastOrDefault();
                mTimeTillNewPath += passedTime;
                if (mTimeTillNewPath<1)
                {
                    return;
                }

                if (latestMovement != default && latestMovement.mProgress > 0)
                {
                    return;
                }

                mTimeTillNewPath -= 1;
                var newDestination = NextNeighborInAttackRange(enemy.mObjectPosition);
                if (newDestination == new Point(-1, -1))
                {
                    attackEvent.mVictimId =
                        DataStorage.GetObject(NextToAttackIfSurrounded(enemy.mObjectPosition)).mObjectId;
                    return;
                }
                var path = DataStorage.mAStar.FindPath(new Node(0, mParentObject.mObjectPosition), new Node(0, newDestination), DataStorage.mGameObjectPositions.Keys.ToList(), mParentObject.mIsPlayer);
                if (path.Count == 0)
                {
                    return;
                }
                DataStorage.mGameEvents[mParentObject.mObjectId] = new List<IEvent> { new MoveEvent(path) };
                return;
            }

            mParentObject.mObjectEvents[EventType.MoveEvent] = new List<IEvent>();

            const float timeTillImpact = 0.5f;
            var getAttackedEvent = new GetAttackedEvent(mObjectDamage, timeTillImpact, mParentObject.mObjectPosition);
            DataStorage.AddEvent(attackEvent.mVictimId, getAttackedEvent);
            switch (mParentObject.mName)
            {
                case ObjectType.Archer:
                case ObjectType.Slingshot:
                case ObjectType.Tower:
                case ObjectType.Arbalist:
                    PlayEffect("arrow");
                    Particle.mForegroundParticles.Add(Particle.ArrowEffect(timeTillImpact, mParentObject.mObjectPosition.X, mParentObject.mObjectPosition.Y, enemy.mObjectPosition.X, enemy.mObjectPosition.Y, 0));
                    break;
                case ObjectType.Shaman:
                    Particle.mForegroundParticles.Add(Particle.ArrowEffect(timeTillImpact, mParentObject.mObjectPosition.X, mParentObject.mObjectPosition.Y, enemy.mObjectPosition.X, enemy.mObjectPosition.Y, 1));
                    break;
                case ObjectType.Mage:
                    Particle.mForegroundParticles.Add(Particle.FireballEffect(timeTillImpact, mParentObject.mObjectPosition.X, mParentObject.mObjectPosition.Y, enemy.mObjectPosition.X, enemy.mObjectPosition.Y));
                    break;
                default:
                    PlayEffect("sword"+ (ParticleEmitter.sRandom.Next(3) + 1));
                    Particle.mForegroundParticles.Add(Particle.SwordEffect(
                        mParentObject.mObjectPosition.X,
                        mParentObject.mObjectPosition.Y,
                        enemy.mObjectPosition.X,
                        enemy.mObjectPosition.Y));
                    break;

            }
            mParentObject.IsAttacking = true;
            mTimeToWait = Cooldown;
            if (!attackEvent.mShouldFollow)
            {
                events.Remove(attackEvent);
            }
        }

        /*
         * Returns a new enemy id in the vision range that surrounds the destination of the unit
         * preferably attacking object, then attackable objects and -1 is there is no such enemy
         */
        private int NewGameObjectAfterDeath()
        {
            var position = mParentObject.mObjectPosition;
            if (mParentObject.mObjectEvents[EventType.MoveEvent].Any())
            {
                var latestMoveEvent = (MoveEvent)mParentObject.mObjectEvents[EventType.MoveEvent].Last();
                position = latestMoveEvent.mDestination;
            }

            mParentObject.GetAttributes().Item2.TryGetValue("Vision", out var vision);
            var nonAttacking = -1;

            for (var x = 0; x <= vision; x++)
            {
                for (var y = 0; y <= vision; y++)
                {
                    var currentPosition = new Point(position.X - x, position.Y - y);
                    var currentObject = DataStorage.GetObject(currentPosition);
                    if (currentObject != null && currentObject.mIsPlayer != mParentObject.mIsPlayer &&
                        currentObject.IsAttackable())
                    {
                        if (currentObject.CanAttack())
                        {
                            return currentObject.mObjectId;
                        }
                        if (nonAttacking == -1)
                        {
                            nonAttacking = currentObject.mObjectId;
                        }
                    }
                    currentPosition = new Point(position.X - x, position.Y + y);
                    currentObject = DataStorage.GetObject(currentPosition);
                    if (currentObject != null && currentObject.mIsPlayer != mParentObject.mIsPlayer &&
                        currentObject.IsAttackable())
                    {
                        if (currentObject.CanAttack())
                        {
                            return currentObject.mObjectId;
                        }
                        if (nonAttacking == -1)
                        {
                            nonAttacking = currentObject.mObjectId;
                        }
                    }
                    currentPosition = new Point(position.X + x, position.Y - y);
                    currentObject = DataStorage.GetObject(currentPosition);
                    if (currentObject != null && currentObject.mIsPlayer != mParentObject.mIsPlayer &&
                        currentObject.IsAttackable())
                    {
                        if (currentObject.CanAttack())
                        {
                            return currentObject.mObjectId;
                        }
                        if (nonAttacking == -1)
                        {
                            nonAttacking = currentObject.mObjectId;
                        }
                    }
                    currentPosition = new Point(position.X + x, position.Y + y);
                    currentObject = DataStorage.GetObject(currentPosition);
                    if (currentObject == null || currentObject.mIsPlayer == mParentObject.mIsPlayer ||
                        !currentObject.IsAttackable())
                    {
                        continue;
                    }

                    if (currentObject.CanAttack())
                    {
                        return currentObject.mObjectId;
                    }
                    if (nonAttacking == -1)
                    {
                        nonAttacking = currentObject.mObjectId;
                    }
                }
            }

            return nonAttacking;
        }

        private Point NextNeighborInAttackRange(Point enemyPosition)
        {
            var distancePoints = new List<List<Point>>();
            for (var i = 0; i < mAttackRange; i++)
            {
                distancePoints.Add(new List<Point>());
            }

            for (var x = -mAttackRange; x <= mAttackRange; x++)
            {
                for (var y = -mAttackRange; y <= mAttackRange; y++)
                {
                    var currentPosition = new Point(enemyPosition.X + x, enemyPosition.Y + y);
                    if (DataStorage.GetObject(currentPosition) != null)
                    {
                        continue;
                    }

                    var distance = Math.Max(Math.Abs(x), Math.Abs(y));
                    distancePoints[distance-1].Add(currentPosition);
                }
            }

            var neighbor = distancePoints.LastOrDefault(list => list.Any());

            return neighbor == default
                ? new Point(-1, -1)
                : neighbor.OrderBy(position => Math.Abs(position.X - mParentObject.mObjectPosition.X)).First();
        }

        private Point NextToAttackIfSurrounded(Point opponentPosition)
        {
            var pointsWithDistanceVisionRange = new List<Point>
            {
                new Point(opponentPosition.X - mAttackRange, opponentPosition.Y - mAttackRange),
                new Point(opponentPosition.X - mAttackRange, opponentPosition.Y + mAttackRange),
                new Point(opponentPosition.X + mAttackRange, opponentPosition.Y - mAttackRange),
                new Point(opponentPosition.X + mAttackRange, opponentPosition.Y + mAttackRange)
            };
            for (var i = -(mAttackRange - 1); i <= mAttackRange - 1; i++)
            {
                pointsWithDistanceVisionRange.AddRange(new List<Point>
                {
                    new Point(opponentPosition.X - mAttackRange, opponentPosition.Y + i),
                    new Point(opponentPosition.X + mAttackRange, opponentPosition.Y + i),
                    new Point(opponentPosition.X + i, opponentPosition.Y - mAttackRange),
                    new Point(opponentPosition.X + i, opponentPosition.Y + mAttackRange)
                });
            }

            return pointsWithDistanceVisionRange.OrderBy(point => Math.Abs(point.X - mParentObject.mObjectPosition.X))
                .First(point => point.X >= 0 && point.X < GameMap.mWidth && point.Y >= 0 && point.Y < GameMap.mHeight);
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>
            {
                ("Damage", mObjectDamage)
            };
        }
    }
}