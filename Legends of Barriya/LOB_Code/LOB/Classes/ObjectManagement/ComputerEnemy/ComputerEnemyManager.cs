using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using Microsoft.Xna.Framework;
using static LOB.Classes.ObjectManagement.Objects.DataStorage;

namespace LOB.Classes.ObjectManagement.ComputerEnemy
{
    internal sealed class ComputerEnemyManagement
    {
        //The not unit type Objects
        private readonly Dictionary<ObjectType, List<int>> mKiObjectsPerKind = new Dictionary<ObjectType, List<int>>();

        //The unit type Objects, ObjectId/Where in use (0 for mFreeUnits)
        private readonly Dictionary<int, int> mKiUnits = new Dictionary<int, int>();

        //ObjectId/ThreatLevel
        private readonly Dictionary<int, int> mThreatLevelsPerObject = new Dictionary<int, int>();

        //ObjectId/UnitsCurrentlyInUse
        private readonly Dictionary<int, HashSet<int>> mUnitsPerCase = new Dictionary<int, HashSet<int>>();

        //A "Stack" of free units.
        private HashSet<int> mFreeUnits = new HashSet<int>();

        //Buildings
        private readonly HashSet<int> mBuildings = new HashSet<int>();

        //What the Ki is currently saving for
        private ObjectType mPriority = ObjectType.NoneObject;

        //ResourceType if mPriority is mine
        private ObjectType mResourcePriority = ObjectType.Tree;

        //stopWatch for attack
        private readonly Stopwatch mAttackStopWatch = new Stopwatch();

        //stopWatch for resources
        private readonly Stopwatch mResourceStopWatch = new Stopwatch();

        private readonly Stopwatch mUnitBuild = new Stopwatch();

        private bool mMoveArmy;
        private readonly GameObjectManagement mGameObjectManagement;

        private readonly List<ObjectType> mGatheringTypes;

        private readonly Random mRandom = new Random();

        //if the construction spot is currently occupied by a unit
        private Point mNewSpot;

        //one gets randomly chosen as next priority
        private readonly List<ObjectType> mBuildingPossibilities = new List<ObjectType>() 
            {ObjectType.House, ObjectType.House, ObjectType.Military1Camp, ObjectType.Wall , ObjectType.Wall , ObjectType.Wall, ObjectType.Wall, ObjectType.Wall, ObjectType.Wall };

        private readonly List<int> mPlayerBuildings = new List<int>();



        //
        //TODO to save and Load
        //
        //Builder currently working on sth
        private readonly Dictionary<int, BuilderTask> mBuilderUsage = new Dictionary<int, BuilderTask>();

        private readonly LayoutManager mLayoutManager;

        private readonly WarManager mWarManager = new WarManager(new List<Troop>(), new Stack<int>());

        private readonly int mBuilderMinimum = Game1.mEnemyRace == Game1.Race.Orc ? 5 : 10;


        internal ComputerEnemyManagement(List<int> idsToAd, GameObjectManagement gameManage)
        {
            mResourceStopWatch.Start();
            mUnitBuild.Start();
            AddNew(idsToAd, true);
            mGameObjectManagement = gameManage;
            mLayoutManager = new LayoutManager(new List<int>{idsToAd.Select(GetObject).First(theObject => theObject.mName == ObjectType.Main1Building).mObjectId});
            mGatheringTypes = Game1.mEnemyRace == Game1.Race.Orc ? new List<ObjectType> {ObjectType.Tree, ObjectType.Iron1Source, ObjectType.Gold1Vein, ObjectType.Mana1Source} : new List<ObjectType> {ObjectType.Tree};
        }

        /*
         * IMPORTANT: id stack in mObjectFactory must not contain 0/1;
         */

        /*
         * Subroutines
         */

        private void AddNew(List<int> idsToAdd, bool loadingNewGame)
        {
            /*
             * Adds combat units to mKiUnits and mFreeUnits
             * Adds other objects to mKiObjectsPerKind
             */
            foreach (var id in idsToAdd)
            {
                var objectWithId = GetObject(id);
                if (objectWithId == null)
                {
                    continue;
                }

                //player buildings
                if (objectWithId.mIsPlayer)
                {
                    if (objectWithId.mName >= ObjectType.Main1Building &&
                        objectWithId.mName <= ObjectType.Mage1Tower)
                    {
                        mPlayerBuildings.Add(id);
                    }

                    continue;
                }

                if (objectWithId.mName == ObjectType.Human1Hero || objectWithId.mName == ObjectType.Orc1Hero)
                {
                    mKiObjectsPerKind[objectWithId.mName] = new List<int> {id};
                    continue;
                }

                //Combat units
                if (objectWithId.mName < ObjectType.Troll)
                {
                    mKiUnits[id] = 0;
                    mFreeUnits.Add(id);
                    if (!loadingNewGame)
                    {
                        mGameObjectManagement.MakeEvents(false, 0, new List<int>() { id }, null, new Point(GameMap.mWidth / 2 + 6, GameMap.mHeight / 2));
                    }
                    continue;
                }

                //Builder
                if (objectWithId.mName == ObjectType.Troll || objectWithId.mName == ObjectType.Builder)
                {
                    mBuilderUsage[objectWithId.mObjectId] = BuilderTask.None;
                    continue;
                }

                //other objects
                //Buildings
                if (objectWithId.mName >= ObjectType.Main1Building && objectWithId.mName <= ObjectType.Military1Camp || objectWithId.mName == ObjectType.Mage1Tower)
                {
                    mBuildings.Add(id);
                }

                if (mKiObjectsPerKind.ContainsKey(objectWithId.mName))
                {
                    mKiObjectsPerKind[objectWithId.mName].Add(id);
                    continue;
                }

                mKiObjectsPerKind[objectWithId.mName] = new List<int> {id};
            }
        }

        //Removes deleted objects
        private void RemoveOld(List<GameObject> objectsToRemove)
        {
            foreach (var objectDelete in objectsToRemove)
            {
                if (objectDelete.mIsPlayer )
                {
                    if (!objectDelete.IsMovable() && objectDelete.mName >= ObjectType.Main1Building &&
                        objectDelete.mName <= ObjectType.Mage1Tower)
                    {
                        mPlayerBuildings.Remove(objectDelete.mObjectId);
                    }

                    continue;
                }

                var id = objectDelete.mObjectId;
                //Removes units form all Ki data
                if (objectDelete.mName < ObjectType.Troll)
                {
                    if (mKiUnits.ContainsKey(id) && mKiUnits[id] != 0)
                    {
                        mUnitsPerCase[mKiUnits[id]].Remove(id);
                    }
                    else
                    {
                        mFreeUnits.Remove(id);
                    }

                    mKiUnits.Remove(id);
                    continue;
                }

                //Builder
                if (objectDelete.mName == ObjectType.Troll || objectDelete.mName == ObjectType.Builder)
                {
                    mBuilderUsage.Remove(objectDelete.mObjectId);
                    continue;
                }

                //Removes other objects
                //Buildings
                if (objectDelete.mName >= ObjectType.Main1Building && objectDelete.mName <= ObjectType.Military1Camp || objectDelete.mName == ObjectType.Mage1Tower)
                {
                    mBuildings.Remove(id);
                }

                if (!mKiObjectsPerKind.ContainsKey(objectDelete.mName))
                {
                    continue;
                }

                mKiObjectsPerKind[objectDelete.mName].Remove(id);

                //Removes mines
                if (objectDelete.mName != ObjectType.Mine)
                {
                    continue;
                }

                mThreatLevelsPerObject.Remove(objectDelete.mObjectId);
                if (!mUnitsPerCase.ContainsKey(objectDelete.mObjectId))
                {
                    continue;
                }

                foreach (var unit in mUnitsPerCase[objectDelete.mObjectId])
                {
                    mFreeUnits.Add(unit);
                    mKiUnits[unit] = 0;
                }

                mUnitsPerCase.Remove(objectDelete.mObjectId);
            }
        }

        //distributes builders to priority 
        private void ResourceGeneration(ObjectType resourcePriority)
        {

            var builderGathering = mBuilderUsage.Where(task => task.Value != BuilderTask.None && task.Value != BuilderTask.Building);
            foreach (var builder in builderGathering.ToList())
            {
                //Checks if the builders are gathering and redistributes them if not
                var builderObjects = GetObject(builder.Key);

                if (builderObjects.mIsMoving)
                {
                    continue;
                }
                if (builderObjects.mObjectState == ObjectState.Gathering)
                {
                    continue;
                }
                
                mGameObjectManagement.MakeEvents(false,
                    3,
                    new List<int> {builder.Key},
                    null,
                    GetNextResourcePoint(resourcePriority, builderObjects.mObjectPosition),
                    builder.Key);
            }
            
            //gives gathering task to builder/troll
            var element = mBuilderUsage.FirstOrDefault(element => element.Value == BuilderTask.None);
            if (element.Value != BuilderTask.None)
            {
                return;
            }

            mBuilderUsage[element.Key] = (BuilderTask)(resourcePriority-ObjectType.Tree);
            var builderObject = GetObject(element.Key);
            mGameObjectManagement.MakeEvents(false, 3, new List<int>{element.Key} , null, GetNextResourcePoint(resourcePriority, builderObject.mObjectPosition), element.Key);
            
        }


        /*
         * KI "Core"
         */


        internal void Update(List<int> idsToAd, List<GameObject> objectsToRemove)
        {
            AddNew(idsToAd, false);

            RemoveOld(objectsToRemove);

            UnitBehaviour();
            mWarManager.Update(mPlayerBuildings);

            var resourceTimer = mResourceStopWatch.Elapsed.TotalSeconds;
            const int buildCycle = 1;
            if (resourceTimer > buildCycle)
            {
                CalculatePriority();
                CheckFreeBuilder();
                mResourceStopWatch.Restart();
                if (mResourcePriority > ObjectType.Tree)
                {
                    LevelUpMines(mResourcePriority);
                }
            }
           

            var unitTimer = mUnitBuild.Elapsed.TotalSeconds;
            if (unitTimer < 2)
            {
                return;
            }

            if (mRandom.Next(1, 6) != 1)
            {
                mUnitBuild.Restart();
                return;
            }
            BuildUnits(1);
            mUnitBuild.Restart();
        }

        private void LevelUpMines(ObjectType resourcePriority)
        {
            foreach (var gameObject in mBuildings.Select(GetObject).Where(building => building.mName == ObjectType.Mine))
            {
                var level = gameObject.GetAttributes().Item2["Level"];
                if (level > 2)
                {
                    continue;
                }
                if (!BuildingCosts.HasEnoughForUpgrade(ObjectType.Mine, level, false))
                {
                    return;
                }
                var gEvent = (GatherResourceEvent)gameObject.mObjectEvents[EventType.GatherResourceEvent].FirstOrDefault();
                if (gEvent == default)
                {
                    continue;
                }
                if ((int)gEvent.mResourceType + ObjectType.Tree == resourcePriority)
                {
                    gameObject.mObjectEvents[EventType.LevelUpEvent].Add(new LevelUpEvent(false));
                }
            }
        }

        //determines the resource usage
        private void CalculatePriority()
        {
            //frees builder in construction
            if (GameObjectManagement.mEnemyBuildingManager.mBuildQueue.Count == 0)
            {
                var constructors = mBuilderUsage.Where(element => element.Value == BuilderTask.Building).ToList();
                foreach (var builders in constructors)
                {
                    mBuilderUsage[builders.Key] = BuilderTask.None;
                }
            }

            //new builders
            if (mBuilderUsage.Keys.Count <  Math.Max(GetResource(ResourceType.MaxPopulation, false)/mBuilderMinimum, 4))
            {
                var mainBuilding = mKiObjectsPerKind[ObjectType.Main1Building].FirstOrDefault();
                if (mainBuilding != default)
                {
                    var type = ObjectType.Builder;
                    if (Game1.mEnemyRace == Game1.Race.Orc)
                    {
                        type = ObjectType.Troll;
                    }
                    mGameObjectManagement.MakeEvents(false, 5, new List<int> { mainBuilding }, type, amount: 1);
                }
            }


            CalculateResourcePriority();
            ResourceGeneration(mResourcePriority);

            if (mPriority != ObjectType.NoneObject)
            {
                return;
            }

            //Only Random decides between house, militaryCamp and wall(tower/gate)
            var rnd = mRandom.Next(mBuildingPossibilities.Count);
            mPriority = mBuildingPossibilities[rnd];
        }

        //determines the current Priority by calculating the production rates
        private void CalculateResourcePriority()
        {
            if (Game1.mEnemyRace != Game1.Race.Orc && !BuildingCosts.HasEnoughForCreation(ObjectType.Mine, false))
            {
                mResourcePriority = ObjectType.Tree;
                return;
            }

            //wood iron gold mana
            var gathering = new List<int>() {0, 0, 0, 0};

            if (mKiObjectsPerKind.TryGetValue(ObjectType.Mine, out var mines))
            {
                foreach (var resourceTyp in mines.Select(GetObject).Select(mineObject => mineObject.GetAttributes().Item2["resourceTyp"]))
                {
                    if (resourceTyp < 4)
                    {
                        gathering[resourceTyp]++;
                    }
                }
            }

            foreach (var builder in mBuilderUsage.Where(builder => builder.Value < BuilderTask.Building))
            {
                gathering[(int) builder.Value]++;
            }

            var resCalc = new Dictionary<int, ObjectType>
            {
                [gathering[0]] = ObjectType.Tree,
                [gathering[1]] = ObjectType.Iron1Source,
                [gathering[2]] = ObjectType.Gold1Vein,
                [gathering[3]] = ObjectType.Mana1Source
            };

            var min = gathering.Min();
            mResourcePriority = resCalc[min];


            if (Game1.mEnemyRace == Game1.Race.Orc)
            {
                return;
            }


            if (!mGatheringTypes.Contains(mResourcePriority))
            {
                if (!BuildingCosts.HasEnoughForCreation(ObjectType.Mine, false))
                {
                    mResourcePriority = ObjectType.Tree;
                    return;
                }
                if (mines == null)
                {
                    mPriority = ObjectType.Mine;
                    return;
                }
                if (mines.Count < Math.Max(mBuildings.Count/3, 6))
                {
                    mPriority = ObjectType.Mine;
                }
            }
        }

        /*
         * Construction/Training Management
         */


        private void BuildUnits(double resourcePercentage)
        {
            if (!mKiObjectsPerKind.TryGetValue(ObjectType.Military1Camp, out var camps))
            {
                return;
            }

            int begin;
            int end;
            switch (Game1.mEnemyRace)
            {
                case Game1.Race.Human:
                    begin = 0;
                    end = (int)ObjectType.Mage+1;
                    break;
                case Game1.Race.Orc:
                    begin = (int)ObjectType.Puncher;
                    end = (int)ObjectType.Shaman+1;
                    break;
                case Game1.Race.Dwarf:
                    begin = (int)ObjectType.Axeman;
                    end = (int)ObjectType.Phalanx+1;
                    break;
                default: 
                    begin = 0; 
                     end = 0;
                    break;
            }
            var unit = mRandom.Next(begin, end);
            var unitType = (ObjectType)unit;
            //calculates the total number of units
            var buildTimes = 0;

            for (var times = 1; times < camps.Count+1; times *= 2)
            {
                if (CompareCosts(unitType, resourcePercentage, times))
                {
                    buildTimes = times;
                    continue;
                }

                break;
            }
            if (buildTimes == 0)
            {
                return;
            }
            foreach (var campObject in camps.Select(GetObject))
            {
                campObject?.mObjectEvents[EventType.BuildUnitEvent].Add(new BuildUnitEvent(unitType, buildTimes));
            }
        }

        private void CheckFreeBuilder()
        {
            //frees a builder if none is free, return if there is none
            if (mBuilderUsage.Count == 0 || mPriority == ObjectType.NoneObject)
            {
                return;
            }
            if (!BuildingCosts.HasEnoughForCreation(mPriority, false))
            {
                return;
            }
            var freeBuilder = 0;
            var element = mBuilderUsage.FirstOrDefault(element => element.Value == BuilderTask.None);
            if (element.Value == BuilderTask.None)
            {
                mBuilderUsage[element.Key] = BuilderTask.Building;
                freeBuilder = element.Key;
            }
            //asks for builders in gathering
            if (freeBuilder == 0)
            {
                element = mBuilderUsage.FirstOrDefault(newElement => newElement.Value != BuilderTask.Building);
                if(element.Key != 0)
                {
                    freeBuilder = element.Key;
                }
                if (freeBuilder == 0)
                {
                    return;
                }
            }

            BuildStructures(mPriority, freeBuilder);
        }

        private void BuildStructures(ObjectType construct, int builder)
        {
            //checks conditions
            var builderObject = GetObject(builder);
            if (mNewSpot == default)
            {

                var constructPosition = construct == ObjectType.Mine && Game1.mEnemyRace != Game1.Race.Orc
                    ? GetNextResourcePoint(mResourcePriority, builderObject.mObjectPosition)
                    : FreeBuildingPlace(builderObject.mObjectPosition);
                if (constructPosition == builderObject.mObjectPosition)
                {
                    mGameObjectManagement.MakeEvents(false, 0, new List<int> { builderObject.mObjectId }, null, DataStorage.mAStar.GetTargets(builderObject.mObjectPosition, 1, DataStorage.mGameObjectPositions.Keys.ToList()).First());
                    return;
                }

                if (constructPosition == default)
                {
                    return;
                }

                mNewSpot = constructPosition;
            }

            if (mPriority != ObjectType.Mine && GetObject(mNewSpot) != default)
            {
                mNewSpot = default;
                return;
            }

            //actual building
            mBuilderUsage[builder] = BuilderTask.Building;
            const int eventType = 2;
            mGameObjectManagement?.MakeEvents(false, eventType, new List<int>{builderObject.mObjectId}, mPriority, mNewSpot);
            mNewSpot = default;
            mPriority = ObjectType.NoneObject;
        }



        /*
         * AttackUnit Management
         */


        //handles attackUnits
        private void UnitBehaviour()
        {
            if (mMoveArmy && mAStar.mPortals.Length > 0)
            {
                mWarManager.AddTroops(mFreeUnits.ToList());
                mFreeUnits = new HashSet<int>();
                mMoveArmy = false;
            }

            //initiates combat behaviour
            if (mFreeUnits.Count > 20 && !mMoveArmy)
            {
                if (Game1.mEnemyRace == Game1.Race.Orc)
                {
                    if (mKiObjectsPerKind.TryGetValue(ObjectType.Orc1Hero, out var hero))
                    {
                        
                            GetObject(hero.First()).mObjectEvents[EventType.PortalEvent] =
                                new List<IEvent> {new PortalOpeningEvent(false)};

                            mMoveArmy = true;
                            mAttackStopWatch.Restart();
                    }
                    else
                    {
                        var mainBuilding = mKiObjectsPerKind[ObjectType.Main1Building].First();
                        mGameObjectManagement.MakeEvents(false, 5, new List<int>() {mainBuilding}, building: ObjectType.Orc1Hero, amount: 1);
                    }
                }
                else
                {
                    if (mKiObjectsPerKind.TryGetValue(ObjectType.Mage1Tower, out var mageTowers))
                    {
                        foreach (var mageTower in mageTowers)
                        {
                            GetObject(mageTower).mObjectEvents[EventType.PortalEvent] =
                                new List<IEvent> {new PortalOpeningEvent(false)};

                            mMoveArmy = true;
                            mAttackStopWatch.Restart();
                        }

                        mWarManager.AddTroops(mFreeUnits.ToList());
                    }
                    else
                    {
                        mPriority = ObjectType.Mage1Tower;
                    }
                }

            }

            //closes portals
            var wartime = mAttackStopWatch.Elapsed.TotalSeconds;
            if (wartime > 35)
            {
                if (mKiObjectsPerKind.TryGetValue(ObjectType.Mage1Tower, out var mageTowers))
                {
                    foreach (var mageTower in mageTowers)
                    {
                        GetObject(mageTower).mObjectEvents[EventType.PortalEvent] =
                            new List<IEvent> {new PortalOpeningEvent(true)};
                        mMoveArmy = false;
                        mAttackStopWatch.Stop();
                    }
                }
                else if (mKiObjectsPerKind.TryGetValue(ObjectType.Orc1Hero, out var hero))
                {
                    GetObject(hero.First()).mObjectEvents[EventType.PortalEvent] =
                            new List<IEvent> {new PortalOpeningEvent(true)};
                        mMoveArmy = false;
                        mAttackStopWatch.Stop();
                }
            }

            //updates threat zones
            foreach (var element in mBuildings)
            {
                var (x, y) = GetObject(element).mObjectPosition;
                var threatLevel = CalculateThreatLevelPerZone((x - 5, y - 5),
                    (x + 5, y + 5));
                mThreatLevelsPerObject[element] = threatLevel;
            }

            DistributeUnits();
        }

        //distributes units between threat zones
        private void DistributeUnits()
        {
            var totalThreat = mThreatLevelsPerObject.Values.Sum();
            foreach (var (mineId, threat) in mThreatLevelsPerObject)
            {
                if (mUnitsPerCase.ContainsKey(mineId))
                {
                    if (threat == 0)
                    {
                        foreach (var unit in mUnitsPerCase[mineId])
                        {
                            mFreeUnits.Add(unit);
                            mKiUnits[unit] = 0;
                        }

                        mUnitsPerCase.Remove(mineId);
                    }
                    continue;
                }
                if (threat == 0)
                {
                    continue;
                }

                if (mFreeUnits.Count == 0)
                {
                    return;
                }

                //actual distribution
                var unitsPercentageNeeded = 1/(totalThreat/(float)threat);
                var usableUnits = Math.Max((int)(mFreeUnits.Count*unitsPercentageNeeded), 1);
                usableUnits = Math.Min(usableUnits, threat);

                var (xPosMine, yPosMine) = GetObject(mineId).mObjectPosition;
                
                MoveUnits(usableUnits, xPosMine, yPosMine, mineId);
            }
        }

        //handles all unit movements
        private void MoveUnits(int usableUnits, int xTargetPos, int yTargetPos, int targetId)
        {
            mUnitsPerCase[targetId] = new HashSet<int>();
            for (var unitCount = 0; unitCount < usableUnits; unitCount++)
            {
                if (mFreeUnits.Count == 0)
                {
                    break;
                }
                var unit = mFreeUnits.First();
                mUnitsPerCase[targetId].Add(unit);
                mKiUnits[unit] = targetId;
                mFreeUnits.Remove(unit);
            }
            mGameObjectManagement.MakeEvents(false, 0, mUnitsPerCase[targetId].ToList(), null, new Point(xTargetPos, yTargetPos));
        }



        /*
         * Helper Functions
         */


        //returns the number of enemies in range
        private static int CalculateThreatLevelPerZone((int, int) startOfZone, (int, int) endOfZone)
        {
            var threatLevel = 0;
            //assuming startOfZone is "smaller"
            var (endOfZoneX, endOfZoneY) = endOfZone;
            for (var x = startOfZone.Item1; x <= endOfZoneX; x ++)
            {
                for (var y = startOfZone.Item2; y <= endOfZoneY; y ++)
                {
                    if (!mGameObjectPositions.TryGetValue(new Point(x, y), out var objectAt))
                    {
                        continue;
                    }
                    if (GetObject(objectAt).mIsPlayer)
                    {
                        threatLevel++;
                    }
                }
            }
            return threatLevel;
        }

        //returns a point which is not occupied, near other buildings, returns builder position if nothing found
        private Point FreeBuildingPlace(Point builderPosition)
        {
            var constructs = mBuildings.Where(element => GetObject(element).mName != ObjectType.Mine).ToList();

            //for wall/tower/gate building
            if (mPriority == ObjectType.Wall)
            {
                var toBuild = mLayoutManager.GetNextSpot(constructs);
                //no free place for wall
                if (toBuild == default)
                {
                    return builderPosition;
                }

                mPriority = toBuild.Item2;
                return toBuild.Item1;
            }

            var rnd = new Random();
            var number = rnd.Next(0, constructs.Count-1);
            var spot = constructs[number];

            //checks for a free spot besides a building 
            var objectPosition = GetObject(spot).mObjectPosition;
            const int maxX = 2, maxY = 2;
            var points = new List<Point>();

            for (var i = -maxX; i <= maxX; i++)
            {
                for (var j = -maxY; j <= maxY; j++)
                {
                    var checkPoint = new Point(objectPosition.X + i, objectPosition.Y + j);
                    points.Add(checkPoint);
                }
            }

            var times = points.Count;
            for (var x = 0; x < times; x++)
            {
                var nextPos = rnd.Next(0, points.Count-1);
                if (CheckPosition(points[nextPos]) && !mLayoutManager.WouldBlockLayout(points[nextPos]))
                {
                    return points[nextPos];
                }
                points.RemoveAt(nextPos);
            }

            return default;
        }

        //checks if less than 3 buildings are surrounding the position
        private static bool CheckPosition(Point possiblePosition)
        {
            var constructCount = -1; // Because it will count itself
            const int maxX = 1, maxY = 1;
            for (var i = -maxX; i <= maxX; i++)
            {
                for (var j = -maxY; j <= maxY; j++)
                {
                    var checkPoint = new Point(possiblePosition.X + i, possiblePosition.Y + j);
                    var objectAt = GetObject(checkPoint);
                    if (objectAt != null && !objectAt.IsMovable())
                    {
                        constructCount++;
                    }
                }
            }

            return constructCount < 2;
        }

        //returns the nearest resource point with type
        private Point GetNextResourcePoint(ObjectType type, Point currentPosition)
        {
            if (type == ObjectType.NoneObject) 
            {
                type = ObjectType.Tree;
            }
            var next = currentPosition;
            var nextDistance = (double)1000;
            foreach (var element in mKiObjectsPerKind[type])
            {
                var resourcePoint = GetObject(element);
                var position = resourcePoint.mObjectPosition;

                var distance = Math.Sqrt((Math.Pow(position.X - currentPosition.X, 2) + Math.Pow(position.Y - currentPosition.Y, 2)));
                if (!(distance < nextDistance))
                {
                    continue;
                }

                next = position;
                nextDistance = distance;
            }
            return next;
        }

        //
        private static bool CompareCosts(ObjectType type, double percentage,int times)
        {
            var statistics = mGameStatistics[false];
            var resources = ObjectFactory.sTypeToMoney[type];

            if (statistics[ResourceType.MaxPopulation] == statistics[ResourceType.Population]*times )
            {

                return true;
            }
            if (statistics[ResourceType.Iron]*percentage < resources[ResourceType.Iron]*times)
            {

                return false;
            }
            if (statistics[ResourceType.Gold]*percentage < resources[ResourceType.Gold]*times)
            {

                return false;
            }
            if (statistics[ResourceType.Mana]*percentage < resources[ResourceType.Mana]*times)
            {
                return false;
            }
            return true;
        }
    }
    
    internal sealed class WarManager
    {
        //max Size of a troop
        private const int TroopSize = 3;
        //active troops
        private readonly List<Troop> mTroops;
        //units yet to distribute
        private readonly Stack<int> mFreeUnits;
        //
        private readonly Point mGatherPoint = new Point(GameMap.mWidth / 2 -10, GameMap.mHeight / 2);
        //
        private readonly Stopwatch mDistributeWatch = new Stopwatch();
        

        private List<int> mPlayerBuildings = new List<int>();

        public WarManager(List<Troop> troops, Stack<int> freeUnits)
        {
            mDistributeWatch.Restart();
            mTroops = troops;
            mFreeUnits = freeUnits;
        }
       
        public void Update(List<int> playerBuildings)
        {
            mPlayerBuildings = playerBuildings;
            var passedTime = mDistributeWatch.Elapsed.TotalSeconds;
            if (passedTime < 3 || mTroops.Count == 0)
            {
                return;
            }
            mDistributeWatch.Restart();
            var newOrders = new Stack<Troop>();
            foreach (var troop in mTroops.Where(troop => troop.Update()).ToList())
            {
                if (troop.mUnits.Count <= TroopSize / 2)
                {
                    AddTroops(troop.mUnits);
                    mTroops.Remove(troop);
                }
                else
                {
                    newOrders.Push(troop);
                }
            }

            foreach (var troop in newOrders)
            {
                var target = NextEnemyBuilding(troop.mTroopPosition);
                troop.mTroopPosition = target == troop.mTroopPosition ? mGatherPoint : target;
                troop.MoveTroop();
            }
        }

        public void AddTroops(List<int> units)
        {
            foreach (var unit in units)
            {
                mFreeUnits.Push(unit);
            }

            while (mFreeUnits.Count > 2)
            {
                var newTroop = new List<int>() {mFreeUnits.Pop(), mFreeUnits.Pop(), mFreeUnits.Pop()};
                mTroops.Add(new Troop(newTroop, mGatherPoint));
            }
        }

        
        private  Point NextEnemyBuilding(Point currentPosition)
        {
            var next = currentPosition;
            var nextDistance = (double)1000;
            foreach (var element in mPlayerBuildings)
            {
                var building = GetObject(element);
                var position = building.mObjectPosition;

                var distance = Math.Sqrt((Math.Pow(position.X - currentPosition.X, 2) + Math.Pow(position.Y - currentPosition.Y, 2)));
                if (!(distance < nextDistance))
                {
                    continue;
                }

                next = position;
                nextDistance = distance;
            }
            return next;
        }
    }

    //troops consistent of n(3) units. act together in battle
    internal sealed class Troop
    {
        public readonly List<int> mUnits;
        public Point mTroopPosition;

        public Troop(List<int> units, Point position)
        {
            mUnits = units;
            mTroopPosition = position;
        }

        public void MoveTroop()
        {
            var newPoints =
                DataStorage.mAStar.GetTargets(mTroopPosition, 3, DataStorage.mGameObjectPositions.Keys.ToList()).ToList();
            var i = 0;
            foreach (var unit in mUnits)
            {
                GetObject(unit).mObjectEvents[EventType.MoveEvent] = new List<IEvent>();
                var path = DataStorage.mAStar.FindPath(new Node(0, GetObject(unit).mObjectPosition),
                    new Node(0, newPoints[i]), DataStorage.mGameObjectPositions.Keys.ToList(), false);

                if (path.Count == 0)
                {
                    continue;
                }

                DataStorage.AddEvent(unit, new MoveEvent(path));
                i++;
            }
            
            //management.MakeEvents(false, 0, mUnits, null, mTroopPosition);
        }

        public bool Update()
        {
            //checks if everyone is alive or idle
            var regroup = new List<int>();
            foreach (var unit in mUnits.ToList())
            {
                var objUnit = GetObject(unit);
                if (objUnit == null)
                {
                    mUnits.Remove(unit);
                    continue;
                }

                if (objUnit.mIsMoving || objUnit.IsAttacking)
                {
                    continue;
                }

                var tempPoint = objUnit.mObjectPosition - mTroopPosition;
                if (objUnit.IsMovable() && (objUnit.mObjectEvents[EventType.MoveEvent].Count == 0 || tempPoint.X > 2 ||
                                            tempPoint.Y > 2))
                {
                    regroup.Add(unit);
                }
            }

            //whole group needs new orders
            if (regroup.Count == mUnits.Count)
            {
                return true;
            }
            //some are not at assigned group place
            //management.MakeEvents(false, 2, regroup, null, mTroopPosition);
            return false;
        }
    }

    
    internal sealed class LayoutManager
    {

        private List<(Point, ObjectType)> mLayout;
        private Point mInPoint;
        private Point mMaxPoint;
        private readonly Random mRandom = new Random();
        private List<(Point, ObjectType)> mLastLayout = new List<(Point, ObjectType)>();

        public LayoutManager(IEnumerable<int> buildings)
        {
            mLayout = new List<(Point, ObjectType)>();
            CreateWallLayout(buildings);
        }

        private void CreateWallLayout(IEnumerable<int> buildings)
        {
            mMaxPoint = new Point(0, 0);
            mInPoint = new Point(1000, 1000);
            mLayout = new List<(Point, ObjectType)>();

            //creates smallest possible rectangle
            var buildingPositions = buildings.Select(buildingId => GetObject(buildingId).mObjectPosition).ToList();
            foreach (var valueTuple in mLastLayout)
            {
                buildingPositions.Add(valueTuple.Item1);
            }

            foreach (var buildingPosition in buildingPositions)
            {
                if (buildingPosition.X < mInPoint.X)
                {
                    mInPoint.X = buildingPosition.X;
                }

                if (buildingPosition.Y < mInPoint.Y)
                {
                    mInPoint.Y = buildingPosition.Y;
                }

                if (buildingPosition.X > mMaxPoint.X)
                {
                    mMaxPoint.X = buildingPosition.X;
                }

                if (buildingPosition.Y > mMaxPoint.Y)
                {
                    mMaxPoint.Y = buildingPosition.Y;
                }
            }

            //no buildings found
            if (mMaxPoint == new Point(0, 0))
            {
                return;
            }

            //enlarges the area a bit
            mInPoint = new Point(mInPoint.X - 3, mInPoint.Y - 3);
            mMaxPoint = new Point(mMaxPoint.X + 3, mMaxPoint.Y + 3);

            //calculates the needed spots
            var half = (mMaxPoint.Y - mInPoint.Y)/2;
            for (var yPos = mInPoint.Y + 1; yPos <= mMaxPoint.Y; yPos++)
            {
                if (yPos == mInPoint.Y + half)
                {
                    mLayout.Add((new Point(mInPoint.X, yPos), ObjectType.Gate));
                    mLayout.Add((new Point(mMaxPoint.X, yPos), ObjectType.Gate));
                    continue;
                }
                mLayout.Add((new Point(mInPoint.X, yPos), ObjectType.Wall));
                mLayout.Add((new Point(mMaxPoint.X, yPos), ObjectType.Wall));
            }

            for (var xPos = mInPoint.X; xPos <= mMaxPoint.X; xPos++)
            {
                mLayout.Add((new Point(xPos, mInPoint.Y), ObjectType.Wall));
                mLayout.Add((new Point(xPos, mMaxPoint.Y), ObjectType.Wall));
            }

            for (var towers = mLayout.Count / 10; towers > 0; towers--)
            {
                var rnd = mRandom.Next(0,mLayout.Count-1);
                if (mLayout[rnd].Item2 != ObjectType.Gate)
                {
                    mLayout[rnd]  = (mLayout[rnd].Item1, ObjectType.Tower);
                }
            }

            //TODO add gates/towers
            mLastLayout = mLayout.ToList();
        }

        public (Point, ObjectType) GetNextSpot(List<int> buildings)
        {
            if (mLayout == default || mLayout.Count == 0)
            {
                CreateWallLayout(buildings);
            }

            (Point, ObjectType) spot = default;
            var i = 0;
            while (spot == default && mLayout.Count > 0 && i < mLayout.Count)
            { 
                spot = mLayout[i];
                i++;
                var objSpot = GetObject(spot.Item1);
                if (objSpot != null)
                {
                    if (!objSpot.IsMovable())
                    {
                        mLayout.RemoveAt(i-1);
                    }

                    spot = default;
                    continue;
                }

                return spot;
            }
            return default;
        }

        public bool WouldBlockLayout(Point possiblePoint)
        {
            if (mLayout == null)
            {
                return false;
            }

            if (mLayout.Any(thing => thing.Item1 == possiblePoint))
            {
                return true;
            }

            if (mLayout.Any(thing => thing.Item2 == ObjectType.Gate && mAStar.mNeighbors[thing.Item1].Contains(possiblePoint)))
            {
                return true;
            }

            if (mAStar.mNeighbors[possiblePoint].Select(GetObject)
                .Any(theObject => theObject is { mName: ObjectType.Gate }))
            {
                return true;
            }
            return false;
        }
    }
}