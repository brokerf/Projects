using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LOB.Classes.Managers;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.ComputerEnemy;
using LOB.Classes.ObjectManagement.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using static LOB.Classes.ObjectManagement.Objects.DataStorage;

namespace LOB.Classes.ObjectManagement.Objects
{
    internal sealed class GameObjectManagement
    {
        public static List<int> mIdsToDelete;
        public static List<GameObject> mObjectsToAdd;

        public static List<int> mSelectedObjects;
        public static int mCurrentSelectedUnit;

        public static BuildingManager mPlayerBuildingManager;
        public static BuildingManager mEnemyBuildingManager;

        private ComputerEnemyManagement mComputerEnemy;

        public GameObjectManagement()
        {
            mSelectedObjects = new List<int>();
            mObjectsToAdd = new List<GameObject>();
            mIdsToDelete = new List<int>();
            mPlayerBuildingManager = new BuildingManager();
            mEnemyBuildingManager = new BuildingManager();
        }

        public void Update(GameTime gameTime)
        {
            mGameTime = gameTime;
            foreach (var element in mGameObjects.Keys)
            {
                GetObject(element).Update();
            }

            var temp = mPlayerById[false];
            temp.AddRange(mPlayerById[true]);
            mComputerEnemy ??= new ComputerEnemyManagement(temp, this);

            //Updates the ComputerEnemy

            var objectsToRemove = DeleteObjects();

            //Normal ObjectGeneration
            var iDsToAdd = mObjectsToAdd.Select(element => element.mObjectId).ToList();
            AddObjects(mObjectsToAdd);

            mObjectsToAdd = new List<GameObject>();
            mComputerEnemy.Update(iDsToAdd, objectsToRemove);

            if (mCurrentSelectedUnit >= mSelectedObjects.Count)
            {
                mCurrentSelectedUnit = Math.Max(mSelectedObjects.Count - 1, 0);
            }
        }

        private void AddObjects(ICollection<GameObject> objectsToAd)
        {
            foreach (var gameObject in objectsToAd.ToList())
            {
                if (!gameObject.GetAttributes().Item2.ContainsKey("Speed"))
                {
                    mAStar.ClosePoint(gameObject.mObjectPosition, gameObject.mIsPlayer);
                }

                objectsToAd.Remove(gameObject);
                AddObject(gameObject);
            }
        }

        private List<GameObject> DeleteObjects()
        {
            var objectsToRemove = new List<GameObject>();
            //deletes Objects with ID in mIdsToDelete
            foreach (var element in mIdsToDelete.ToList())
            {
                mIdsToDelete.Remove(element);
                var currentObject = GetObject(element);
                if ((currentObject.mName == ObjectType.Mine && mObjectsToAdd.All(gameObject =>
                    gameObject.mObjectPosition != currentObject.mObjectPosition)) || currentObject.mName == ObjectType.New1Building && mObjectsToAdd.All(gameObject => gameObject.mObjectPosition != currentObject.mObjectPosition))
                {
                    var resourceAction = (GatherResource) currentObject.mActions.FirstOrDefault(action => action is GatherResource);
                    var constructAction =
                        (ConstructBuilding)currentObject.mActions.FirstOrDefault(action => action is ConstructBuilding);
                    var resource = resourceAction?.mCurrentResource ?? constructAction!.mResource;

                    switch (resource)
                    {
                        case ResourceType.Gold:
                            mObjectsToAdd.Add(ObjectFactory.BuildObject(ObjectType.Gold1Vein, currentObject.mObjectPosition, isPlayer: currentObject.mIsPlayer));
                            break;
                        case ResourceType.Mana:
                            mObjectsToAdd.Add(ObjectFactory.BuildObject(ObjectType.Mana1Source, currentObject.mObjectPosition, isPlayer: currentObject.mIsPlayer));
                            break;
                        case ResourceType.Iron:
                            mObjectsToAdd.Add(ObjectFactory.BuildObject(ObjectType.Iron1Source, currentObject.mObjectPosition, isPlayer: currentObject.mIsPlayer));
                            break;
                    }
                }

                objectsToRemove.Add(currentObject);
                currentObject.OnDeath();

                var objectPosition = currentObject.mObjectPosition;
                if (mSelectedObjects.Contains(element) && (currentObject.mName != ObjectType.New1Building || mObjectsToAdd.All(gameObject => gameObject.mObjectPosition != currentObject.mObjectPosition)))
                {
                    mSelectedObjects.Remove(element);
                    mCurrentSelectedUnit = Math.Min(mSelectedObjects.Count-1, mCurrentSelectedUnit);
                    if (!mSelectedObjects.Any())
                    {
                        DataStorage.mPopUpObjectHasDied = true;
                    }
                }

                mPlayerById[currentObject.mIsPlayer].Remove(element);
                ObjectFactory.ReturnIdToPool(element);

                // Reopen point in AStar, if dying object can't move.
                if (!currentObject.GetAttributes().Item2.ContainsKey("Speed"))
                {
                    mAStar.OpenPoint(currentObject.mObjectPosition, currentObject.mIsPlayer);
                }

                mGameObjectPositions.Remove(objectPosition);
                mGameObjects.Remove(element);
            }
            return objectsToRemove;
        }

        public bool FindObjectsInSelectionRect(InputData inputData)
        {
            List<int> selectedObjects;
            if (inputData.mDownKeys.Contains(Keys.LeftShift) || inputData.mDownKeys.Contains(Keys.RightShift))
            {
                selectedObjects = mSelectedObjects;
            }
            else
            {
                selectedObjects = new List<int>();
            }
            mCurrentSelectedUnit = 0;
            mSelectedObjects = new List<int>();
            for (var i = inputData.mMouseData.mSelection.Y; i < inputData.mMouseData.mSelection.Bottom; i++)
            {
                for (var j = inputData.mMouseData.mSelection.X; j < inputData.mMouseData.mSelection.Right; j++)
                {
                    if (!mGameObjectPositions.TryGetValue(new Point(j, i + 1), out var objectId) || selectedObjects.Contains(objectId))
                    {
                        continue;
                    }
                    selectedObjects.Add(objectId);
                }
            }

            if (selectedObjects.Count == 1 && DataStorage.GetObject(selectedObjects[0]).mIsPlayer)
            {
                mSelectedObjects.Add(selectedObjects[0]);
                return true;

            }
            foreach (var selectedObject in selectedObjects)
            {
                var selectObject = GetObject(selectedObject);
                
                if (selectObject.mIsPlayer && selectObject.mName < ObjectType.Main1Building)
                {
                    mSelectedObjects.Add(selectedObject);
                }
            }
            return mSelectedObjects.Count != 0;
        }

        public void MakePlayerEvents(int type,
            ObjectType? building = null,
            Vector2 mousePosition = default,
            int resourceSelectionMode = -1,
            bool isOpen = false,
            int amount = 0, PotionType potionType = PotionType.Damage1Potion)
        {
            var target = new Point((int)mousePosition.X, (int)mousePosition.Y + 1);
            MakeEvents(true, type, mSelectedObjects, building, target, resourceSelectionMode, isOpen, amount, potionType, true);
        }

        private readonly List<ObjectType> mBuilds = new List<ObjectType>{ObjectType.House, ObjectType.Mine, ObjectType.Mage1Tower, ObjectType.Military1Camp};
        
        public void MakeEvents(bool isPlayer, int type, List<int> objectsToEvent,
            ObjectType? building = null,
            Point target = default,
            int resourceSelectionMode = -1,
            bool isOpen = false, int amount = 0, PotionType potionType = PotionType.Damage1Potion, bool eventIsPlayer = false)
        {

            if (type == 9)
            {
                var otherObject = GetObject(target);
                var resType2 = ResourceType.Iron;
                if (building == null)
                {
                    return;
                }
                if ((ObjectType)building! == ObjectType.Mine)
                {
                    if (otherObject != null)
                    {
                        resType2 = (ResourceType)(otherObject.mName - (int)ObjectType.Tree);
                    }
                }
                if (otherObject != null)
                {
                    mIdsToDelete.Add(otherObject.mObjectId);
                }

                mObjectsToAdd.Add(ObjectFactory.BuildObject(building.Value, target, 0, resType2, isPlayer));
                if (building.Value > ObjectType.Builder)
                {
                    return;
                }

                DataStorage.mGameStatistics[isPlayer][ResourceType.Population] += 1;
                DataStorage.mGameStatistics[isPlayer][ResourceType.MaxPopulation] += 1;
                return;
            }

            if (type == 2 && building != null)
            {
                var stoppingObjectType = GetObject(target);

                var selectedObject = mSelectedObjects.Count > 0 ? mSelectedObjects[mCurrentSelectedUnit] : (int?)null;

                //building mines
                if (stoppingObjectType != null)
                {
                    var resType = (ResourceType)stoppingObjectType.mName - (int)ObjectType.Tree;
                    if (building.Value != ObjectType.Mine || resType < ResourceType.Iron)
                    {
                        if (!isPlayer)
                        {
                            return;
                        }

                        if (selectedObject != null)
                        {
                            mBuildError = ("You can't build this here.", -2);
                        }
                        return;
                    }
                    
                    MakeBuildEvent(target, (ObjectType)building!, selectedObject, resType, eventIsPlayer, stoppingObjectType.mObjectId);
                }
                else
                {
                    //other buildings
                    var temp = GameMap.GetBarrierWidthAndPosition(GameMap.mWidth);
                    if (target.X >= temp.Item1 - 1 && target.X <= temp.Item2 + temp.Item1 + 2 || (target.X >= temp.Item2 + temp.Item1 + 2 && mBuilds.Contains((ObjectType)building) && isPlayer))
                    {
                        return;
                    }
                    MakeBuildEvent(target, (ObjectType)building!, selectedObject, ResourceType.None, eventIsPlayer);
                }
            }

            mUsedPlaces = new List<Point>();

            if (objectsToEvent == null || objectsToEvent.Count <= 0)
            {
                return;
            }

            

            if (type == 10 && target != default) // Automatic Event finder
            {
                var objectToInteractWith = GetObject(target);
                var selectedObject = GetObject(objectsToEvent[0]);
                if (objectToInteractWith != null)
                {

                    switch (selectedObject.mName)
                    {
                        case ObjectType.Troll:
                        case ObjectType.Builder:
                            if (objectToInteractWith.mName <= ObjectType.New1Building && objectToInteractWith.mName >= ObjectType.Main1Building) // repair / build object
                            {
                                type = 8;
                            }
                            else if (objectToInteractWith.mName >= ObjectType.Tree) // Get Resource from object
                            {
                                resourceSelectionMode = selectedObject.mObjectId;
                                type = 3;
                            }
                            break;
                        default:
                            if (objectToInteractWith.mIsPlayer != isPlayer && objectToInteractWith.mName <= ObjectType.New1Building) // Attack object
                            {
                                type = 0;
                            }
                            break;
                    }
                }
                else if(selectedObject.mName <= ObjectType.Builder)
                {
                    type = 0;
                }
            }

            if (type == 0)
            {
                MakeMoveEvents(objectsToEvent, target, isPlayer);
            }

            foreach (var objectId in objectsToEvent)
            {
                var selectedObject = GetObject(objectId);
                selectedObject.mNewOrder = true;

                

                switch (type)
                {
                    case 0://Move/AttackEvent
                        selectedObject.mObjectEvents.TryGetValue(EventType.GatherResourceEvent, out var gatherEvents);
                        gatherEvents?.Clear();

                        var targetObject = GetObject(target);
                        if (targetObject != null && targetObject.IsAttackable())
                        {
                            var attackingObject = GetObject(objectId);
                            if (!attackingObject.mObjectEvents.ContainsKey(EventType.AttackEvent))
                            {
                                attackingObject.mObjectEvents[EventType.AttackEvent] = new List<IEvent>();
                            }
                            attackingObject.mObjectEvents[EventType.AttackEvent].Add(new AttackEvent(targetObject.mObjectId, true));
                        }
                        else
                        {
                            return;
                        }
                        break;
                    case 1://LevelUpEvent
                        mGameEvents[objectId] = new List<IEvent>
                        {
                            new LevelUpEvent(false)
                        };
                        return;
                    case 2://BuildBuildingEvent
                        //if several builders are ordered
                        if (mObjectsToAdd.Count > 0)
                        {
                            selectedObject.mObjectEvents[EventType.BuilderOrderEvent] = new List<IEvent>
                                {new BuilderOrderEvent(target)};
                        }
                        break;
                    case 3://ResourceEvent
                        selectedObject.mObjectEvents[EventType.GatherResourceEvent] = new List<IEvent>();
                        MakeResourceGatherEvent(objectId, resourceSelectionMode, target, isPlayer);
                        break;
                    case 4://PortalEvent
                        selectedObject.mObjectEvents[EventType.PortalEvent] = new List<IEvent> { new PortalOpeningEvent(isOpen) };
                        break;
                    case 5://buildUnitEvent
                        if (building == null)
                        {
                            return;
                        }
                        selectedObject.mObjectEvents[EventType.BuildUnitEvent].Add(new BuildUnitEvent((ObjectType)building, amount));
                        break;
                    case 6: // LevelUpEvent from Hero
                        var mine = GetObject(target);
                        if (!(mine is {mName: ObjectType.Mine}) || mine.mIsPlayer != isPlayer)
                        {
                            return;
                        }

                        mine.GetAttributes().Item2.TryGetValue("Level", out var mineLevel);
                        if (mineLevel == 3)
                        {
                            return;
                        }

                        if (!mine.mObjectEvents.ContainsKey(EventType.LevelUpEvent))
                        {
                            mine.mObjectEvents[EventType.LevelUpEvent] = new List<IEvent>();
                        }
                        mine.mObjectEvents[EventType.LevelUpEvent].Add(new LevelUpEvent(true));
                        AddEvent(objectId, new SpecialEvent(new Point(-1, -1)));
                        break;
                    case 7: // PotionEvent
                        var (barrierPosition, barrierWidth) = GameMap.GetBarrierWidthAndPosition();
                        var gameObject = GetObject(objectId);
                        var attackAction =
                            (AttackBehaviour) gameObject.mActions.First(action => action is AttackBehaviour);
                        var attackRange = attackAction.GetAttackRange();
                        if (gameObject.mName == ObjectType.Mage)
                        { 
                            attackRange += 3;
                        }
                        //var attackRange = attackAction.GetAttackRange();
                        if (Math.Abs(target.X - gameObject.mObjectPosition.X) > attackRange ||
                            Math.Abs(target.Y - gameObject.mObjectPosition.Y) > attackRange)
                        {
                            return;
                        }

                        if (barrierPosition >= target.X && barrierPosition + barrierWidth < target.X)
                        {
                            return;
                        }

                        if ((gameObject.mObjectPosition.X < barrierPosition &&
                             barrierPosition + barrierWidth <= target.X) ||
                            (target.X < barrierPosition &&
                             barrierPosition + barrierWidth <= gameObject.mObjectPosition.X))
                        {
                            return;
                        }

                        if (!gameObject.mObjectEvents.ContainsKey(EventType.PotionEvent))
                        {
                            gameObject.mObjectEvents[EventType.PotionEvent] = new List<IEvent>();
                        }
                        gameObject.mObjectEvents[EventType.PotionEvent].Add(new PotionEvent(target, potionType));
                        break;
                    case 8: // RepairEvent
                        var builderObject = GetObject(objectId);
                        if (builderObject.mName != ObjectType.Builder && builderObject.mName != ObjectType.Troll)
                        {
                            continue;
                        }
                        var walkTarget = builderObject.mObjectPosition;
                        var buildingToRepair = GetObject(target);

                        if (buildingToRepair == null)
                        {
                            return;
                        }
                        
                        //for construction
                        if (buildingToRepair.mName == ObjectType.New1Building)
                        {

                            AddEvent(builderObject.mObjectId, new BuilderOrderEvent(target));
                            break;
                        }

                        var stats = buildingToRepair.GetAttributes().Item2;
                        if (stats["Health"] == stats["MaxHealth"])
                        {
                            return;
                        }

                        if (buildingToRepair == null || (buildingToRepair.CanAttack() && buildingToRepair.mName != ObjectType.Tower))
                        {
                            return;
                        }

                        if (mAStar.mCostFromZeroToPoint[walkTarget - target] > 1 + mAStar.mSideCost) // Builder only has to move if he is further away than one tile (mSideCost to include diagonals)
                        {
                            walkTarget = mAStar.mNeighbors[target].OrderBy(position => Math.Sqrt(Math.Pow(selectedObject.mObjectPosition.X - position.X, 2)
                                + Math.Pow(selectedObject.mObjectPosition.Y - position.Y, 2))).FirstOrDefault(point => !mGameObjectPositions.ContainsKey(point));
                        }
                        if (walkTarget == default)
                        {
                            return;
                        }
                        var path = mAStar.FindPath(new Node(0, builderObject.mObjectPosition), new Node(0, walkTarget), DataStorage.mGameObjectPositions.Keys.ToList(), builderObject.mIsPlayer);
                        if (path.Count == 0)
                        {
                            return;
                        }

                        AddEvent(builderObject.mObjectId, new MoveEvent(path));

                        if (!builderObject.mObjectEvents.ContainsKey(EventType.SpecialEvent))
                        {
                            builderObject.mObjectEvents[EventType.SpecialEvent] = new List<IEvent>();
                        }
                        builderObject.mObjectEvents[EventType.SpecialEvent].Add(new SpecialEvent(target));
                        break;
                }
                //so that the builder doesn't quit his current job if the build failed
                if (mBuildError.Item1 == string.Empty)
                {
                    continue;
                }

                selectedObject.mNewOrder = false;
                if (!isPlayer)
                {
                    mBuildError.Item1 = string.Empty;
                }
            }
        }

        private void MakeMoveEvents(List<int> selectedObjectIds, Point target, bool isPlayer)
        {
            foreach (var gameObject in selectedObjectIds.Select(GetObject).Where(gameObject => gameObject != null))
            {
                if (gameObject.mIsPlayer != isPlayer)
                { // Controlling enemies is now same as if KI moved them ()
                    isPlayer = !isPlayer;
                }
                gameObject.mObjectEvents[EventType.AttackEvent] = new List<IEvent>();
            }
            if (target.Y < 1)
            {
                return;
            }
            var thread = new Thread(() =>
            {
                var paths = mAStar.FindPath(selectedObjectIds,
                    new Node(0, target),
                    mGameObjectPositions.Keys.ToList(), isPlayer);
                for (var i = 0; i < paths.Count; i++)
                {
                    if (paths[i].Count != 0)
                    {
                        if (selectedObjectIds.Count <= i) //
                        {
                            return;
                        }
                        mGameEvents[selectedObjectIds[i]] = new List<IEvent> { new MoveEvent(paths[i]) };
                    }
                }
            });
            thread.Start();
        }

        private void MakeMoveEvent(int selectedObjectId, Point target, bool isPlayer)
        {
            var path = mAStar.FindPath(new Node(0, GetObject(selectedObjectId).mObjectPosition),
                new Node(0, target),
                mGameObjectPositions.Keys.ToList(), isPlayer);
            if (path.Count != 0) { 

                AddEvent(selectedObjectId, new MoveEvent(path));
            }
        }

        private List<Point> mUsedPlaces = new List<Point>();

        private void MakeResourceGatherEvent(int selectedObjectId, int resourceSelectionMode, Point targetPosition, bool isPlayer)
        {
            if (targetPosition.Y < 1 || !mSelectedObjects.Contains(resourceSelectionMode) && isPlayer)
            {
                return;
            }
            
            var targetResource = GetObject(targetPosition);
            if (targetResource == null)
            {
                return;
            }

            var selectedObject = GetObject(selectedObjectId);
            if (selectedObject == null)
            {
                return;
            }
            
            var walkTarget = mAStar.mNeighbors[targetResource.mObjectPosition].OrderBy(position => Math.Sqrt(Math.Pow(selectedObject.mObjectPosition.X - position.X, 2)
                + Math.Pow(selectedObject.mObjectPosition.Y - position.Y, 2))).Where(position => !mUsedPlaces.Contains(position)).FirstOrDefault(point => !mGameObjectPositions.ContainsKey(point));
            if (walkTarget == default || mUsedPlaces.Contains(walkTarget))
            {
                return;
            }

            mUsedPlaces.Add(walkTarget);

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (selectedObject.mName)
            {
                case ObjectType.Builder when targetResource.mName == ObjectType.Tree:
                    {
                        MakeMoveEvent(selectedObjectId, walkTarget, isPlayer);

                        selectedObject.mObjectEvents[EventType.GatherResourceEvent] =
                            new List<IEvent> { new GatherResourceEvent(ResourceType.Wood, walkTarget) };
                        return;
                    }
                case ObjectType.Troll when targetResource.mName >= ObjectType.Tree:
                    {
                        MakeMoveEvent(selectedObjectId, walkTarget, isPlayer);

                        var resType = (ResourceType)targetResource.mName - (int)ObjectType.Tree;
                        selectedObject.mObjectEvents[EventType.GatherResourceEvent] =
                            new List<IEvent> { new GatherResourceEvent(resType, walkTarget) };
                        return;
                    }
            }
        }

        private void MakeBuildEvent(
            Point target,
            ObjectType building,
            int? selectedObjectId = null,
            ResourceType resourceType = ResourceType.None, bool isPlayer = true, int? stoppingObjectId = null)
        {
            // TODO CREATE SPECIFIC VALUES / BALANCING
            var costs = BuildingCosts.sBuildingCosts[building];
            
            for (var i = 0; i < 4; i++)
            {
                if (mGameStatistics[isPlayer][(ResourceType) i] < costs[i])
                {
                    if (isPlayer)
                    {
                        mBuildError = ($"You do not have enough {(ResourceType)i}.", -2);
                    }

                    return;
                }
            }
            /*no Error was encountered */
            for (var i = 0; i < 4; i++)
            {
                mGameStatistics[isPlayer][(ResourceType)i] -= costs[i];
            }
            
            //mines
            if (stoppingObjectId != null)
            {
                mIdsToDelete.Add(stoppingObjectId.Value);
            }
            else if (building == ObjectType.Mine)
            {
                if (isPlayer)
                {
                    mBuildError = ("You can't build this here.", -2);
                }
                return;
            }
            //updates the buildManager
            if (isPlayer)
            {
                mPlayerBuildingManager.AddQueue(target);
            }
            else
            {
                mEnemyBuildingManager.AddQueue(target);
            }

            if (selectedObjectId != null)
            {
                var selectedObject = GetObject(selectedObjectId.Value);
                if (selectedObject != null && (selectedObject.mName == ObjectType.Troll || selectedObject.mName == ObjectType.Builder))
                {
                    selectedObject.mObjectEvents[EventType.BuilderOrderEvent] = 
                        new List<IEvent> { new BuilderOrderEvent(target) };
                }
            }
            
            mObjectsToAdd.Add(ObjectFactory.BuildObject(ObjectType.New1Building, target, 0, resourceType, isPlayer, building));
        }

        public void StopCurrentEvents()
        {
            foreach (var selectedObject in mSelectedObjects.Select(GetObject))
            {
                foreach (var action in selectedObject.mActions.Where(action => !(action is GetAttacked) && !(action is SpecialAction)))
                {
                    selectedObject.mObjectEvents[action.GetEventType] = new List<IEvent>();
                }
                selectedObject.mIsMoving = false;
                selectedObject.IsAttacking = false;
            }
        }
    }
}