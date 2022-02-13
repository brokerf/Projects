using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    /// <summary>
    /// Movement Event for moving the unit from place A to place B.
    /// </summary>
    internal sealed class Movement : IAction
    {
        private static Dictionary<int, Point> sReservedTiles = new Dictionary<int, Point>();
        private float mSpeed;
        // TODO Save actual speed and changed Speed
        private readonly float mNormalSpeed;
        private GameObject mParentObject;
        private float mWaitingTimer;
        private int mWaitedTimes;

        public Movement(int speed, float waitingTimer = 0, int waitedTimes = 0)
        {
            mSpeed = 1f/speed;
            mNormalSpeed = 1f / speed;
            mWaitingTimer = waitingTimer;
            mWaitedTimes = waitedTimes;
        }

        internal static void RemoveMyReservation(int id)
        {
            if (sReservedTiles.ContainsKey(id))
            {
                sReservedTiles.Remove(id);
            }
        }

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":(" + (int)(1f/mSpeed) + "/" + mWaitingTimer + "/" + mWaitedTimes + ")]";
        }
        
        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");
            var notFailed = int.TryParse(stats[0], out var speed);
            var waitingTimer = Convert.ToSingle(stats[1]);
            notFailed = int.TryParse(stats[2], out var waitedTimes) && notFailed;
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction Movement");
            }
            return new Movement(speed, waitingTimer, waitedTimes);
        }

        internal static string SaveReservedTiles()
        {
            return sReservedTiles.Aggregate(string.Empty, (current, tile) => current + ("|<" + tile.Key + "<" + tile.Value.X + "," + tile.Value.Y + ">>"));
        }

        internal static void ResetReserved()
        {
            sReservedTiles = new Dictionary<int, Point>();
        }

        internal static void LoadReservedTiles(string data)
        {
            ResetReserved();
            if (data == "")
            {
                return;
            }
            var tiles = data.Replace(">>", string.Empty).Split("|");
            var notFailed = true;
            foreach (var tile in tiles)
            {
                if (tile == "")
                {
                    continue;
                }
                var tileData = tile.Split("<");
                notFailed = int.TryParse(tileData[1], out var id) && notFailed;
                tileData = tileData[2].Split(",");
                notFailed = int.TryParse(tileData[0], out var x) && notFailed;
                notFailed = int.TryParse(tileData[1], out var y) && notFailed;
                sReservedTiles[id] = new Point(x, y);
            }
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load mReservedTiles");
            }
        }

        /// <summary>
        /// Update function to create the new movement, in case an Order exists or the destination is not reached.
        /// </summary>
        /// <param name="events"></param>
        public void Update(List<IEvent> events)
        {
            var passedTime = DataStorage.mGameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
            mBuffTimeLeft -= passedTime;
            if (mBuffTimeLeft <= 0)
            {
                SetSpeedToNormal();
            }
            mParentObject.mIsMoving = false;
            var currentPosition = mParentObject.mObjectPosition;
            DataStorage.mUnitAnimationOffset[currentPosition] = new Vector2(0, 0);

            if (events.Count == 0)
            {
                return;
            }

            var moveEvent = (MoveEvent)events[0];
            
            if (CheckConditions(currentPosition, moveEvent))
            {
                return;
            }

            mWaitedTimes = Math.Max(mWaitedTimes-1, 0);
            mWaitingTimer = 0;

            if (moveEvent.mDestination == currentPosition)
            {
                return;
            }

            if (sReservedTiles.ContainsValue(moveEvent.mDestination) &&
                (!sReservedTiles.ContainsKey(mParentObject.mObjectId) ||
                 sReservedTiles[mParentObject.mObjectId] != moveEvent.mDestination))
            {
                return;
            }

            mParentObject.mIsMoving = true;
            sReservedTiles[mParentObject.mObjectId] = moveEvent.mDestination;
            if (moveEvent.mProgress >= mSpeed)
            {
                DataStorage.mGameObjectPositions.Remove(currentPosition);
                DataStorage.mGameObjects[mParentObject.mObjectId].mObjectPosition = moveEvent.mDestination;
                DataStorage.mGameObjectPositions[moveEvent.mDestination] = mParentObject.mObjectId;
                moveEvent.mProgress -= mSpeed;
                moveEvent.mReached = true;
                RemoveMyReservation(mParentObject.mObjectId);

                if (events.Count > 1)
                {
                    mParentObject.mObjectEvents[GetEventType].RemoveRange(0, mParentObject.mObjectEvents[GetEventType].Count - 1);
                    // Go back to previous point for correct start position of next path
                    var newDestination = ((MoveEvent)mParentObject.mObjectEvents[GetEventType][0]).mDestination;
                    if (DataStorage.mAStar.mCostFromZeroToPoint[moveEvent.mDestination- newDestination] > 1 + DataStorage.mAStar.mSideCost)
                    {
                        var inBetweenPoint = DataStorage.mAStar.mNeighbors[moveEvent.mDestination]
                            .OrderBy(move => DataStorage.mAStar.mCostFromZeroToPoint[newDestination - move]).First();
                        if (inBetweenPoint != moveEvent.mDestination)
                        {
                            ((MoveEvent)events[0]).mPath.Insert(0, newDestination);
                            ((MoveEvent)events[0]).mDestination = inBetweenPoint;
                        }
                        moveEvent = (MoveEvent)events[^1];
                    }
                }
                // Case: Next Tile Reached and there is a next Tile in the path
                else if (moveEvent.mPath.Count != 0)
                {
                    moveEvent.mDestination = moveEvent.mPath[0];
                    moveEvent.mPath.RemoveAt(0);
                    moveEvent.mReached = false;
                }
                else
                {
                    currentPosition = mParentObject.mObjectPosition; 
                    if (mParentObject.mBuffParticle != null)
                    {
                        mParentObject.mBuffParticle.mPosition = currentPosition.ToVector2() - new Vector2(0, 2/7f);
                    }
                    return;
                }
                currentPosition = mParentObject.mObjectPosition;
            }
            //updates the animation

            var dir = new Vector2(moveEvent.mDestination.X - currentPosition.X, moveEvent.mDestination.Y - currentPosition.Y);
            dir = new Vector2(Math.Abs(dir.X), Math.Abs(dir.Y)) / dir.Length();
            var reduction = Math.Max(dir.X, dir.Y);
            moveEvent.mProgress += (reduction*DataStorage.mGameTime.ElapsedGameTime.Milliseconds) / IAction.MillisecondsPerSecond;

            var percent = moveEvent.mProgress / mSpeed;
            var animationOffset = percent *(moveEvent.mDestination - currentPosition).ToVector2();
            DataStorage.mUnitAnimationOffset[currentPosition] = animationOffset;

            var walkParticle = Particle.FootStepsEffect(
                mParentObject.mObjectPosition.X + DataStorage.mUnitAnimationOffset[currentPosition].X,
                mParentObject.mObjectPosition.Y + DataStorage.mUnitAnimationOffset[currentPosition].Y);
            if (mParentObject.mBuffParticle != null)
            {
                mParentObject.mBuffParticle.mPosition = currentPosition.ToVector2()+animationOffset-new Vector2(0, 2 / 7f);
            }
            if (walkParticle != null) 
            { 
                Particle.mBackgroundParticles.Add(walkParticle);
            }
            
        }

        private bool CheckConditions(Point currentPosition, MoveEvent moveEvent)
        {
            var events = mParentObject.mObjectEvents;
            if (moveEvent.mPath.Count == 0 && moveEvent.mReached)
            {
                if (events[GetEventType].Count == 1)
                {
                    mParentObject.mObjectEvents[GetEventType] = new List<IEvent>();
                    RemoveMyReservation(mParentObject.mObjectId);
                }
                else
                {
                    mParentObject.mObjectEvents[GetEventType].RemoveRange(0, mParentObject.mObjectEvents[GetEventType].Count - 1);
                    return true;
                }
                return true;
            }

            var blockedPosition = DataStorage.GetObject(moveEvent.mDestination);
            if (blockedPosition != null)
            {
                if (blockedPosition == mParentObject)
                {
                    if (moveEvent.mPath.Count > 0)
                    {
                        moveEvent.mDestination = moveEvent.mPath[0];
                        moveEvent.mPath.RemoveAt(0);
                    }
                    else
                    {
                        mParentObject.mObjectEvents[GetEventType].Remove(moveEvent);
                    }
                    return true;
                }

                if (events[GetEventType].Count > 1)
                {
                    mParentObject.mObjectEvents[GetEventType].RemoveRange(0, mParentObject.mObjectEvents[GetEventType].Count - 1);
                    return true;
                }

                if (moveEvent.mPath.Count == 0)
                {
                    mWaitingTimer = 0;
                    return true;
                }

                var objectKeys = DataStorage.mGameObjectPositions.Keys.ToList();

                if (blockedPosition.mObjectEvents.TryGetValue(GetEventType, out var moveEvents) && moveEvents.Count > 0)
                {
                    if (((MoveEvent)moveEvents[^1]).mDestination != currentPosition)
                    {
                        return true;
                    }
                }

                mWaitingTimer += DataStorage.mGameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
                if (mWaitingTimer < mWaitedTimes)
                {
                    return true;
                }

                mWaitingTimer = 0;

                var newPath = DataStorage.mAStar.FindPath(new Node(0, currentPosition),
                    new Node(0, moveEvent.mPath[^1]),
                    objectKeys, mParentObject.mIsPlayer);
                if (newPath.Count == 0)
                { 
                    mWaitedTimes++;
                    mWaitingTimer = 0;
                    newPath = DataStorage.mAStar.FindPath(new Node(0, currentPosition), 
                        new Node(0, DataStorage.mAStar.GetTargets(moveEvent.mPath[^1], 1, 
                            objectKeys).First()), objectKeys, mParentObject.mIsPlayer);

                    if (newPath.Count == 0)
                    {
                        return true;
                    }

                    DataStorage.AddEvent(mParentObject.mObjectId, new MoveEvent(newPath));

                    return true;
                }

                DataStorage.AddEvent(mParentObject.mObjectId, new MoveEvent(newPath));

                //mParentObject.mObjectEvents[GetEventType].Remove(moveEvent);
                return true;
            }

            if (!GameMap.mBarrierData.Contains(new Point(moveEvent.mDestination.X + (!mParentObject.mIsPlayer ? 1 : -1),
                    moveEvent.mDestination.Y)) ||
                !GameMap.mBarrierData.Contains(new Point(currentPosition.X + (mParentObject.mIsPlayer ? 1 : -1),
                    currentPosition.Y)) || DataStorage.mAStar.mPortals[mParentObject.mIsPlayer ? 0 : 1].ContainsKey(currentPosition))
            {
                //return moveEvent.mDestination == currentPosition;
            }

            //mParentObject.mObjectEvents[GetEventType].Remove(moveEvent);
            //RemoveMyReservation(mParentObject.mObjectId);
            return false;

        }

        private float mBuffTimeLeft;

        public void IncreaseSpeed(int tilePerSecondIncrease)
        {
            mSpeed = mNormalSpeed + tilePerSecondIncrease;
            mBuffTimeLeft = 2f;
        }

        private void SetSpeedToNormal()
        {
            mSpeed = mNormalSpeed;
            mParentObject.mIsSpeedUp = false;
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>
            {
                ("Speed", (int)(1f/mSpeed)),
            };
        }

        public EventType GetEventType => EventType.MoveEvent;
    }
}