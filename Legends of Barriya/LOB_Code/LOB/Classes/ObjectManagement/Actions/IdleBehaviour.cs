using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class IdleBehaviour : IAction
    {
        private readonly int mVisionRange;
        private int? mAttackRange;
        private GameObject mParentObject;

        public IdleBehaviour(int visionRange)
        {
            mVisionRange = visionRange;
        }

        public void Update(List<IEvent> events)
        {
            mParentObject.mSeesEnemy = false;
            var isGate = mParentObject.mName == ObjectType.Gate;

            if (!isGate)
            {
                if (!mParentObject.CanAttack() || mParentObject.IsAttacking)
                {
                    return;
                }
                mAttackRange ??= ((AttackBehaviour)mParentObject.mActions.First(action => action.GetEventType == EventType.AttackEvent)).GetAttackRange();

                if ((mParentObject.mObjectEvents.TryGetValue(EventType.MoveEvent, out var move) && move.Count > 0) || (mParentObject.mObjectEvents.TryGetValue(EventType.AttackEvent, out var attack) && attack.Count > 0))
                {
                    return;
                }
            }

            var positionFound = false;
            for (var x = 0; x <= mAttackRange && !positionFound; x++)
            {
                for (var y = 0; y <= mAttackRange && !positionFound; y++)
                {
                    positionFound = CheckPosition(-x, -y, isGate);
                    if (!positionFound)
                    {
                        positionFound = CheckPosition(-x, y, isGate);
                    }
                    if (!positionFound)
                    {
                        positionFound = CheckPosition(x, -y, isGate);
                    }
                    if (!positionFound)
                    {
                        positionFound = CheckPosition(x, y, isGate);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a position in the vision range of the parent object is occupied by an enemy
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isGate"></param>
        private bool CheckPosition(int x, int y, bool isGate)
        {
            var position = new Point(mParentObject.mObjectPosition.X + x,
                mParentObject.mObjectPosition.Y + y);
            var opponent = DataStorage.GetObject(position);

            if (opponent == null || mParentObject.mIsPlayer == opponent.mIsPlayer)
            {
                return false;
            }
            
            /* DataStorage.ClearEvents(mParentObject.mObjectId);
            
            foreach (var action in mParentObject.mActions)
            { 
                mParentObject.mObjectEvents[action.GetEventType] = new List<IEvent>();
            } */
            if (isGate)
            {
                mParentObject.mSeesEnemy = true;
                return true;
            }
            DataStorage.AddEvent(mParentObject.mObjectId, new AttackEvent(opponent.mObjectId, false));
            return true;
        }

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>{("Vision", mVisionRange) };
        }

        public EventType GetEventType => EventType.None;

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":(" + mVisionRange + ")]";
        }
        
        // Expected: Only receives data in brackets (), not including the brackets
        public static IAction LoadAction(string data)
        {
            var notFailed = int.TryParse(data, out var visionRange);
            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load IAction IdleBehaviour");
            }
            return new IdleBehaviour(visionRange);
        }
    }
}