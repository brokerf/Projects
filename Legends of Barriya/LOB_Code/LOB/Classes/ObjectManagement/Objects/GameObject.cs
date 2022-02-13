using System;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Events;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.Data;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Objects
{
    internal sealed class GameObject
    {
        internal readonly ObjectType mName;
        internal readonly int mObjectId;
        internal Point mObjectPosition;
        internal readonly List<IAction> mActions;
        internal readonly Dictionary<EventType, List<IEvent>> mObjectEvents = new Dictionary<EventType, List<IEvent>>();
        private readonly int mBarrierSprite;
        internal readonly bool mIsPlayer;
        internal bool mIsMoving;
        private float mProgress;
        private const float MillisecondsPerSecond = 1000.0f;
        private readonly bool[,] mOtherWalls = new bool[3,3];
        public static readonly List<int> sHousePopulationTuple = new List<int> {5, 15, 15};
        public bool mHasChanged;

        public ObjectState mObjectState;

        internal bool mNewOrder = false;

        public event Action Death;

        public GameObject(ObjectType name,
            int id,
            Point position,
            List<IAction> actions,
            Dictionary<EventType, List<IEvent>> objectEvents = null,
            int barrierSprite = 0,
            bool isPlayer = true,
            bool hasChanged = false)
        {
            mName = name;
            mObjectId = id;
            mObjectPosition = position;
            mActions = actions;
            mIsPlayer = isPlayer;
            mBarrierSprite = barrierSprite;
            mIsPlayer = isPlayer;
            mProgress = 0f;
            mHasChanged = hasChanged;
            objectEvents ??= new Dictionary<EventType, List<IEvent>>();


            foreach (var theAction in mActions)
            {
                if (!mObjectEvents.ContainsKey(theAction.GetEventType))
                {
                    mObjectEvents[theAction.GetEventType] = new List<IEvent>();
                }
                if (objectEvents.ContainsKey(theAction.GetEventType))
                {
                    mObjectEvents[theAction.GetEventType] = objectEvents[theAction.GetEventType];
                }
                theAction.SetParentObject(this);
            }

            if (mName.ToString().Contains("Hero"))
            {
                Death += SetHeroDead;
            }
            else if (mName == ObjectType.House)
            {
                AddPopulation(sHousePopulationTuple[0]);
                Death += RemovePopulation;
            }
            else if (mName == ObjectType.Main1Building)
            {
                AddPopulation(10);
                Death += Lose;
            }

            if (mName != ObjectType.Wall && mName != ObjectType.Tower && mName != ObjectType.Gate)
            {
                return;
            }

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    var neighborPosition = new Point(position.X + x, position.Y + y);
                    if (neighborPosition == position)
                    {
                        mOtherWalls[x + 1, y + 1] = true;
                        continue;
                    }
                    var neighbor = DataStorage.GetObject(neighborPosition);
                    if (neighbor != null && neighbor.AddWall(position))
                    {
                        mOtherWalls[x+1, y+1] = true;
                    }
                }
            }

            mWallSprite = -1;
        }

        private bool AddWall(Point position)
        {
            var pos = position - mObjectPosition;
            if (mName != ObjectType.Wall && mName != ObjectType.Tower && mName != ObjectType.Gate || Math.Abs(pos.X) >= 2 || Math.Abs(pos.Y) >= 2)
            {
                return false;
            }

            mOtherWalls[pos.X+1, pos.Y+1] = true;

            SetWallSprite();
            return true;
        }

        private void RemoveWall(Point position)
        {
            var pos = position - mObjectPosition;
            if (mName != ObjectType.Wall && mName != ObjectType.Tower && mName != ObjectType.Gate || Math.Abs(pos.X) >= 2 || Math.Abs(pos.Y) >= 2)
            {
                return;
            }

            mOtherWalls[pos.X + 1, pos.Y + 1] = false;

            SetWallSprite();
        }

        private void SetWallSprite()
        {
            var hash = GetHashCode(mOtherWalls);
            mWallSprite = mName switch
            {
                ObjectType.Tower => sTowerSprites[hash],
                ObjectType.Wall => sWallSprites[hash],
                ObjectType.Gate => sGateSprites[hash],
                _ => 0
            };
            if (mName != ObjectType.Gate)
            {
                return;
            }
            if (DataStorage.mAStar == null)
            {
                mWallSprite = -1;
                return;
            }
            if (mWallSprite != 4)
            {
                DataStorage.mAStar.OpenGate(mWallSprite, mObjectPosition, mIsPlayer);
            }
            else
            {
                DataStorage.mAStar.ClosePoint(mObjectPosition, mIsPlayer, true);
            }
        }

        public float UpdateProgress()
        {
            mProgress += DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
            if (mProgress >= 1)
            {
                mProgress = 0;
            }

            return mProgress;
        }

        private void Lose()
        {
            if (DataStorage.mGameStatistics[!mIsPlayer][ResourceType.MainBuildingAlive] != 1)
            {
                return;
            }

            DataStorage.mGameStatistics[mIsPlayer][ResourceType.MainBuildingAlive] = 0;
            if (!mIsPlayer)
            {
                Achievements.AddWin();
            }
        }

        // TODO According to level
        private void AddPopulation(int amount)
        {
            DataStorage.mGameStatistics[mIsPlayer][ResourceType.MaxPopulation] += amount;
        }

        private void RemovePopulation()
        {
            var levelUp = (LevelUp) mActions.First(action => action is LevelUp);
            var level = levelUp.mCurrentLevel;
            while (level > 0)
            {
                DataStorage.mGameStatistics[mIsPlayer][ResourceType.MaxPopulation] -= sHousePopulationTuple[level - 1];
                level--;
            }
        }

        /*Reads the Events for current Object and calls subMethods
         Clears events in DataStorage
         */
        internal void Update()
        {
            if (DataStorage.mGameEvents.TryGetValue(mObjectId, out var objectEvents))
            {
                if (objectEvents.Count != 0)
                {
                    foreach (var element in objectEvents.ToList())
                    {
                        if (mActions.Exists(action => action.GetEventType == element.GetEventType))
                        {
                            mObjectEvents[element.GetEventType].Add(element);
                        }
                    }
                    DataStorage.ClearEvents(mObjectId);
                }
            }

            var isBuffed = mIsStrengthUp || mIsSpeedUp;
            if (isBuffed && (mBuffParticle == null || mBuffParticle.mTimeToLive <= 0))
            {
                mBuffParticle = Particle.BuffEffect(mObjectPosition);
                Particle.mForegroundParticles.Add(mBuffParticle);
            }
            if (mBuffParticle != null)
            {
                mBuffParticle.mPosition = mObjectPosition.ToVector2()-new Vector2(0, 2/7f);
                if (!isBuffed)
                {
                    mBuffParticle?.RemoveParticle(Particle.mForegroundParticles);
                    mBuffParticle = null;
                }
            }

            foreach (var action in mActions)
            {
                action.Update(mObjectEvents[action.GetEventType]);
            }
        }

        // Calls the GetAttributes Method for every component. Returns them in a list.
        public (ObjectType, Dictionary<string, int>) GetAttributes()
        {
            var objectStats = new Dictionary<string, int>();

            foreach (var (attrName, attrValue) in mActions.SelectMany(element => element.GetAttribute()))
            {
                objectStats[attrName] = attrValue;
            }

            return (mName, objectStats);
        }

        private static readonly Dictionary<int, int> sWallSprites = new Dictionary<int, int>
        {
            {729, 0},
            {810, 1},
            {7290, 2},
            {7371, 3},
            {738, 4},
            {59778, 4},
            {59787, 4},
            {66339, 5},
            {7299, 6},
            {819, 7},
            {59859, 8},
            {66348, 9},
            {59868, 10},
            {66420, 11},
            {7380, 12},
            {66429, 13},
        };

        private static readonly Dictionary<int, int> sTowerSprites = new Dictionary<int, int>
        {
            {729, 0},
            {810, 1},
            {7290, 0},
            {7371, 1},
            {738, 3},
            {59778, 2},
            {59787, 4},
            {66339, 2},
            {7299, 3},
            {819, 6},
            {59859, 5},
            {66348, 4},
            {59868, 7},
            {66420, 5},
            {7380, 6},
            {66429, 7},
        };

        private static readonly Dictionary<int, int> sGateSprites = new Dictionary<int, int>
        {
            {729, 4},
            {810, 4},
            {7290, 4},
            {7371, 2},
            {738, 4},
            {59778, 4},
            {59787, 0},
            {66339, 4},
            {7299, 4},
            {819, 4},
            {59859, 4},
            {66348, 4},
            {59868, 4},
            {66420, 4},
            {7380, 4},
            {66429, 4},
        };

        private int GetHashCode(bool[,] x)
        {
            var result = 0;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    if (i % 2 == 0 && j % 2 == 0)
                    {
                        continue;
                    }
                    if (x[i,j]) { result++; }
                    result *= 9;
                }
            }
            return result;
        }

        private int mWallSprite;
        internal bool mSeesEnemy;

        internal int GetCurrentSprite()
        {
            if (mWallSprite == -1)
            {
                SetWallSprite();
            }
            if (mName == ObjectType.Gate)
            {
                return mWallSprite + (mWallSprite != 4 && !mSeesEnemy ? 1 : 0);
            }
            if (mName == ObjectType.Wall || mName == ObjectType.Tower)
            {
                return mWallSprite;
            }

            if (IsAttacking)
            {
                return 3;
            }
            var action1 = mActions.FirstOrDefault(action => action.GetEventType == EventType.LevelUpEvent);
            var level = -1;
            if (action1 != default)
            {
                level = ((LevelUp)action1).mCurrentLevel - 1;
            }

            var amount = 1;
            if (level != -1)
            {
                amount = 3;
            }
            else
            {
                level = 0;
            }

            if (!IsAttacking)
            {
                return mBarrierSprite + level;
            }
            return mBarrierSprite + level + amount;
        }

        internal bool IsAttacking { set; get; }
        
        internal bool IsAttackable()
        {
            return mActions.Exists(action => action.GetEventType == EventType.GetAttackedEvent);
        }

        internal bool IsMovable()
        {
            return mActions.Exists(action => action.GetEventType == EventType.MoveEvent);
        }

        internal bool CanAttack()
        {
            return mActions.Exists(action => action.GetEventType == EventType.AttackEvent);
        }

        public Particle mBuffParticle;
        public bool mIsStrengthUp;
        public bool mIsSpeedUp;

        internal void Strengthen(float ratio)
        {
            var attack = mActions.FirstOrDefault(action => action is AttackBehaviour);
            if (attack == default)
            {
                return;
            }

            var attackBehaviour = (AttackBehaviour) attack;
            attackBehaviour.Strengthen(ratio);
            mIsStrengthUp = true;
        }

        internal void IncreaseSpeed(int tilePerSecondIncrease)
        {
            var movement = mActions.FirstOrDefault(action => action is Movement);
            if (movement == default)
            {
                return;
            }

            var moveAction = (Movement)movement;
            moveAction.IncreaseSpeed(tilePerSecondIncrease);
            mIsSpeedUp = true;
        }

        internal string SaveObject()
        {
            var data = mName + "$" + mObjectId + "$" + mObjectPosition.X + "|" + mObjectPosition.Y + "$";
            data = mActions.Aggregate(data, (current, objActions) => current + objActions.SaveAction()) + "$";
            data = mObjectEvents.Aggregate(data, (current1, keyValuePair) => keyValuePair.Value.Aggregate(current1, (current, @event) => current + @event.SaveEvent()));
            data += "$"+mBarrierSprite + "$" + mIsPlayer + "$" + mIsMoving;
            return data;
        }

        private void SetHeroDead()
        {
            DataStorage.mGameStatistics[mIsPlayer][ResourceType.HeroAlive] = 0;
        }

        internal void OnDeath()
        {
            Movement.RemoveMyReservation(mObjectId);
            if (mName <= ObjectType.Builder)
            {
                if (!mIsPlayer)
                {
                    DataStorage.mGameStatistics[true][ResourceType.KilledEnemies]++;
                    DataStorage.mGameStatistics[false][ResourceType.Population]--;
                    Achievements.AddKill();
                }
                else
                {
                    DataStorage.mGameStatistics[true][ResourceType.LostUnits]++;
                    DataStorage.mGameStatistics[true][ResourceType.Population]--;
                }
            }

            if (mOtherWalls[1,1])
            {
                for (var x = -1; x <= 1; x++)
                {
                    for (var y = -1; y <= 1; y++)
                    {
                        var neighborPosition = new Point(mObjectPosition.X + x, mObjectPosition.Y + y);
                        if (neighborPosition == mObjectPosition)
                        {
                            continue;
                        }
                        DataStorage.GetObject(neighborPosition)?.RemoveWall(mObjectPosition);
                    }
                }
            }
            
            Death?.Invoke();
        }
    }
}