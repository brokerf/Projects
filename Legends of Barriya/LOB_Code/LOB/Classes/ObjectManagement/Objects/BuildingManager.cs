using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Objects
{
    internal sealed class BuildingManager
    {
        public readonly List<Point> mBuildQueue = new List<Point>();

        public void AddQueue(Point newConstruct)
        {
            mBuildQueue.Add(newConstruct);
        }

        public void RemoveQueue(Point finishedConstruct)
        {
            mBuildQueue.Remove(finishedConstruct);
        }

        //returns the nearest structure from builderPosition, default if queue is empty
        public Point GetNextStructurePosition(Point builderPosition)
        {
            const double targetDist = 1000;
            Point target = default;
            foreach (var position in from position in mBuildQueue.ToList() let newDist = (builderPosition - position).ToVector2().Length() where newDist < targetDist select position)
            {
                if (!DataStorage.mGameObjectPositions.ContainsKey(position) && GameObjectManagement.mObjectsToAdd.All(gameObject => gameObject.mObjectPosition != position))
                {
                    mBuildQueue.Remove(position);
                    continue;
                }

                target = position;
            }
            
            return target;
        }

        //TODO make it loadable
    }
}