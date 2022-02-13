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
    //building action of builder/troll
    internal sealed class ProgressBuilding: IAction
    {
        private GameObject mParentObject;
        private const float TimeNeeded = 0.5f;
        private float mProgress;
        private Point mTarget;

        public ProgressBuilding(float progress = 0, Point target = default)
        {
            mProgress = progress;
            mTarget = target;
        }

        public void Update(List<IEvent> events)
        {
            var nextBuild = DataStorage.GetObject(mTarget);

            if (mTarget != default && !(nextBuild is { mName: ObjectType.New1Building }))
            {
                mTarget = default;
                mParentObject.mObjectEvents[EventType.MoveEvent].Clear();
                return;
            }

            //resets if there is a new order
            if (mParentObject.mNewOrder)
            {
                mTarget = default;
                mParentObject.mNewOrder = false;
            }

            var currentPosition = mParentObject.mObjectPosition;

            if (mTarget == default && events.Count == 0)
            { 
                return;
            }
            //gets an order
            if (events.Count != 0 && mTarget == default)
            {
                mTarget = ((BuilderOrderEvent) events[0]).mTarget;
                mParentObject.mObjectEvents[EventType.BuilderOrderEvent] = new List<IEvent>();
            }
            

            if (nextBuild == null || nextBuild.mIsPlayer != mParentObject.mIsPlayer)
            {
                return;
            }

            
            

            //the actual build process
            if (!(DataStorage.mAStar.mCostFromZeroToPoint[currentPosition - mTarget] > 1 + DataStorage.mAStar.mSideCost))
            {
                mParentObject.mObjectState = ObjectState.Building;
                mProgress += DataStorage.mGameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
                
                Particle.mForegroundParticles.Add(Particle.BuildingEffect(mTarget.X,
                    mTarget.Y));
                

                if (mProgress < TimeNeeded)
                {
                    return;
                }
                mProgress = 0;
                //tries to find a new construction
                
                if (nextBuild.mName != ObjectType.New1Building)
                {
                    mTarget = mParentObject.mIsPlayer ? GameObjectManagement.mPlayerBuildingManager.GetNextStructurePosition(currentPosition) : GameObjectManagement.mEnemyBuildingManager.GetNextStructurePosition(currentPosition);

                    return;
                }

                //sends the progress Event
                DataStorage.GetObject(mTarget).mObjectEvents[EventType.BuildProgressEvent] = new List<IEvent> { new BuildProgressEvent()};
                return;
            }
            
            //new movement if builder is stuck
            if (mParentObject.mIsMoving)
            {
                return;
            }

            var requiredPositionOfBuilder = DataStorage.mAStar.mNeighbors[mTarget].OrderBy(position => Math.Sqrt(Math.Pow(currentPosition.X - position.X, 2) 
                                                                                                                 + Math.Pow(currentPosition.Y - position.Y, 2))).FirstOrDefault(point => !DataStorage.mGameObjectPositions.ContainsKey(point));

            //stops build attempt if position is unreachable(still builds New1Building)
            if (requiredPositionOfBuilder != default)
            {
                var path = DataStorage.mAStar.FindPath(new Node(0, currentPosition),
                    new Node(0, requiredPositionOfBuilder),
                    DataStorage.mGameObjectPositions.Keys.ToList(),
                    mParentObject.mIsPlayer);
                if (path.Count != 0)
                {
                    mParentObject.mObjectEvents[EventType.MoveEvent] =
                        new List<IEvent> {new MoveEvent(path)};
                    return;
                }
            }

            mTarget = default;
        }

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public void SetTargetToDefault()
        {
            mTarget = default;
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public EventType GetEventType => EventType.BuilderOrderEvent;

        public string SaveAction()
        {
            return "[" + GetEventType + ":("+ mProgress + "/" + mTarget.X + "|" + mTarget.Y + ")]";
        }

        //TODO should be loaded
        public static IAction LoadAction(string data)
        {
            var stats = data.Split("/");
            var notFailed = float.TryParse(stats[0], out var progress);
            notFailed = int.TryParse(stats[1].Split("|")[0], out var x) && notFailed;
            notFailed = int.TryParse(stats[1].Split("|")[1], out var y) && notFailed; 
            var target = new Point(x, y);
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction ProgressBuilding");
            }
            return new ProgressBuilding(progress, target);
        }
    }
}