using System;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Events;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Objects
{
    internal static class ObjectFactory
    {
        private const int KnightHp = 50;
        private const int ArcherHp = 40;
        private const int HorsemanHp = 70;
        private const int MageHp = 60;
        private const int HumanHeroHp = 500;
        private const int AxemanHp = 60;
        private const int ArbalistHp = 50;
        private const int WolfRiderHp = 70;
        private const int PhalanxHp = 100;
        private const int DwarfHeroHp = 500;
        private const int PuncherHp = 30;
        private const int SlingshotHp = 60;
        private const int ShamanHp = 60;
        private const int OrkHeroHp = 500;
        private const int TrollHp = 40;
        private const int BuilderHp = 30;
        private const int WallHp = 1000;
        private const int GateHp = 170;
        private const int HouseHp = 80;
        private const int New1BuildingHp = 10;
        private const int MainBuildingHp = 300;
        private const int MineHp = 80;
        private const int TowerHp = 190;
        private const int Military1CampHp = 80;
        private const int Mage1TowerHp = 80;

        private const int HorsemanSpeed = 6;
        private const int PhalanxSpeed = 2;
        private const int WolfRiderSpeed = 4;
        private const int DwarfHeroSpeed = 3;
        private const int MageSpeed = 2;
        private const int ArbalistSpeed = 3;
        private const int OrkHeroSpeed = 3;
        private const int AxemanSpeed = 3;
        private const int ShamanSpeed = 2;
        private const int TrollSpeed = 3;
        private const int HumanHeroSpeed = 3;
        private const int PuncherSpeed = 3;
        private const int ArcherSpeed = 3;
        private const int KnightSpeed = 2;
        private const int BuilderSpeed = 3;
        private const int SlingshotSpeed = 3;


        private const int KnightDmg = 12;
        private const int ArcherDmg = 10;
        private const int HorsemanDmg = 15;
        private const int HumanHeroDmg = 20;
        private const int PuncherDmg = 8;
        private const int OrkHeroDmg = 20;
        private const int AxemanDmg = 18;
        private const int ArbalistDmg = 14;
        private const int PhalanxDmg = 10;
        private const int WolfRiderDmg = 15;
        private const int DwarfHeroDmg = 20;
        private const int TowerDmg = 15;
        private const int SlingshotDmg = 15;
        private const int TrollDmg = 5;
        private const int MageDmg = 0;
        private const int ShamanDmg = 0;


        private const int KnightRange = 1;
        private const int ArcherRange = 3;
        private const int HorsemanRange = 1;
        private const int HumanHeroRange = 1;
        private const int PuncherRange = 1;
        private const int TrollRange = 1;
        private const int OrkHeroRange = 1;
        private const int AxemanRange = 1;
        private const int ArbalistRange = 3;
        private const int PhalanxRange = 1;
        private const int WolfRiderRange = 1;
        private const int DwarfHeroRange = 1;
        private const int TowerRange = 4;
        private const int SlingshotRange = 3;
        private const int ShamanRange = 4;
        private const int MageRange = 4;


        private const int KnightVisionRange = 3;
        private const int ArcherVisionRange = 4;
        private const int HorsemanVisionRange = 5;
        private const int MageVisionRange = 3;
        private const int HumanHeroVisionRange = 3;
        private const int PuncherVisionRange = 3;
        private const int ShamanVisionRange = 3;
        private const int TrollVisionRange = 2;
        private const int OrkHeroVisionRange = 3;
        private const int AxemanVisionRange = 3;
        private const int ArbalistVisionRange = 3;
        private const int PhalanxVisionRange = 3;
        private const int WolfRiderVisionRange = 4;
        private const int DwarfHeroVisionRange = 3;
        private const int TowerVisionRange = 5;
        private const int SlingshotVisionRange = 3;
        private const int BuilderVisionRange = 3;

        private static List<int> sStackIds;

        internal static readonly Dictionary<ResourceType, Dictionary<int, int>> sLevelToAmount = new Dictionary<ResourceType, Dictionary<int, int>>
        {
            { ResourceType.Wood, new Dictionary<int, int> { { 1, 25 } } },
            { ResourceType.Iron, new Dictionary<int, int> { { 1, 10 }, { 2, 15 }, { 3, 20 } } },
            { ResourceType.Gold, new Dictionary<int, int> { { 1, 5 }, { 2, 10 }, { 3, 15 } } },
            { ResourceType.Mana, new Dictionary<int, int> { { 1, 1 }, { 2, 3 }, { 3, 5 } } }
        };

        public static readonly Dictionary<ObjectType, Dictionary<ResourceType, int>> sTypeToMoney = 
            new Dictionary<ObjectType, Dictionary<ResourceType, int>>
            {
                { ObjectType.Arbalist, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 50 },
                        { ResourceType.Gold, 20 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Archer, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 30 },
                        { ResourceType.Gold, 15 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Axeman, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 50 },
                        { ResourceType.Gold, 10 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Builder, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 30 },
                        { ResourceType.Gold, 10 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Dwarf1Hero, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 30 },
                        { ResourceType.Gold, 30 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Horseman, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 50 },
                        { ResourceType.Gold, 50 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Human1Hero, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 70 },
                        { ResourceType.Gold, 80 },
                        { ResourceType.Mana, 30 }
                    }
                },
                { ObjectType.Knight, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 30 },
                        { ResourceType.Gold, 10 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Mage, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 10 },
                        { ResourceType.Gold, 50 },
                        { ResourceType.Mana, 30 }
                    }
                },
                { ObjectType.Orc1Hero, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 0 },
                        { ResourceType.Gold, 50 },
                        { ResourceType.Mana, 70 }
                    }
                },
                { ObjectType.Phalanx, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 40 },
                        { ResourceType.Gold, 30 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Puncher, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 20 },
                        { ResourceType.Gold, 5 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Shaman, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 20 },
                        { ResourceType.Gold, 40 },
                        { ResourceType.Mana, 20 }
                    }
                },
                { ObjectType.Slingshot, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 20 },
                        { ResourceType.Gold, 10 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Troll, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 100 },
                        { ResourceType.Gold, 50 },
                        { ResourceType.Mana, 0 }
                    }
                },
                { ObjectType.Wolf1Rider, new Dictionary<ResourceType, int>
                    {
                        { ResourceType.Iron, 60 },
                        { ResourceType.Gold, 30 },
                        { ResourceType.Mana, 0 }
                    }
                }
            };

        //KI: id 0 ist used to signalize free units, 1 for units on the enemy's side
        public static void ObjectFactoryReset(int width, int height)
        {
            sStackIds = new List<int>();
            for (var i = 2; i < width*height; i++)
            {
                sStackIds.Add(i);
            }
            sStackIds = sStackIds.OrderBy(i => i).ToList();
        }

        public static void Remove(int i)
        {
            if (sStackIds.Contains(i))
            {
                sStackIds.Remove(i);
            }
        }

        public static int Pop()
        {
            var i = sStackIds[^1];
            sStackIds.RemoveAt(sStackIds.Count-1);
            return i;
        }

        private static void Push(int i)
        {
            if (sStackIds.Contains(i))
            {
                return;
            }

            sStackIds.Add(i);
            sStackIds = sStackIds.OrderBy(j => j).ToList();
        }

        public static void ReturnIdToPool(int idToAdd)
        {
            Push(idToAdd);
        }

        public static GameObject BuildObject(ObjectType type, Point position, int objectState = 0, ResourceType currentType = ResourceType.None, bool isPlayer = true, ObjectType objectToBuild = ObjectType.NoneObject, bool hasChanged = false)
        {
            var actions = new List<IAction>();
            var objectId = Pop();
            if (hasChanged)
            {
                GameObjectManagement.mSelectedObjects.Add(objectId);
                GameObjectManagement.mCurrentSelectedUnit += 1;
            }
            DataStorage.mPlayerById[isPlayer].Add(objectId);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (type)
            {
                case ObjectType.Barrier:
                    return new GameObject(ObjectType.Barrier, objectId, position, actions, barrierSprite: objectState, hasChanged: hasChanged);
                case ObjectType.BarrierLeft:
                    return new GameObject(ObjectType.Barrier, objectId, position, actions, hasChanged: hasChanged);
                case ObjectType.BarrierMiddle:
                    return new GameObject(ObjectType.Barrier, objectId, position, actions, barrierSprite: 1, hasChanged: hasChanged);
                case ObjectType.BarrierRight:
                    return new GameObject(ObjectType.Barrier, objectId, position, actions, barrierSprite: 2, hasChanged: hasChanged);
                case ObjectType.Tree:
                    {
                        break;
                    }
                case ObjectType.Gold1Vein:
                    {
                        break;
                    }
                case ObjectType.Iron1Source:
                    {
                        break;
                    }
                case ObjectType.Mana1Source:
                    {
                        break;
                    }
                case ObjectType.Rock:
                    {
                        break;
                    }
                case ObjectType.Knight:
                    {
                        actions.Add(new GetAttacked(KnightHp));
                        actions.Add(new Movement(KnightSpeed));
                        actions.Add(new AttackBehaviour(KnightDmg, KnightRange));
                        actions.Add(new IdleBehaviour(KnightVisionRange));
                        break;
                    }
                case ObjectType.Archer:
                    {
                        actions.Add(new GetAttacked(ArcherHp));
                        actions.Add(new Movement(ArcherSpeed));
                        actions.Add(new AttackBehaviour(ArcherDmg, ArcherRange));
                        actions.Add(new IdleBehaviour(ArcherVisionRange));
                        break;
                    }
                case ObjectType.Horseman:
                    {
                        actions.Add(new GetAttacked(HorsemanHp));
                        actions.Add(new Movement(HorsemanSpeed));
                        actions.Add(new AttackBehaviour(HorsemanDmg, HorsemanRange));
                        actions.Add(new IdleBehaviour(HorsemanVisionRange));
                        break;
                    }
                case ObjectType.Mage:
                    {
                        actions.Add(new GetAttacked(MageHp));
                        actions.Add(new Movement(MageSpeed));
                        actions.Add(new AttackBehaviour(MageDmg, MageRange));
                        actions.Add(new IdleBehaviour(MageVisionRange));
                        actions.Add(new Potion());
                        break;
                    }
                case ObjectType.Human1Hero:
                    {
                        actions.Add(new GetAttacked(HumanHeroHp));
                        actions.Add(new Movement(HumanHeroSpeed));
                        actions.Add(new AttackBehaviour(HumanHeroDmg, HumanHeroRange));
                        actions.Add(new IdleBehaviour(HumanHeroVisionRange));
                        actions.Add(new SpecialAction());
                        break;
                    }
                case ObjectType.Puncher:
                    {
                        actions.Add(new GetAttacked(PuncherHp));
                        actions.Add(new Movement(PuncherSpeed));
                        actions.Add(new AttackBehaviour(PuncherDmg, PuncherRange));
                        actions.Add(new IdleBehaviour(PuncherVisionRange));
                        break;
                    }
                case ObjectType.Slingshot:
                    {
                        actions.Add(new GetAttacked(SlingshotHp));
                        actions.Add(new Movement(SlingshotSpeed));
                        actions.Add(new AttackBehaviour(SlingshotDmg, SlingshotRange));
                        actions.Add(new IdleBehaviour(SlingshotVisionRange));
                        break;
                    }
                case ObjectType.Shaman:
                    {
                        actions.Add(new GetAttacked(ShamanHp));
                        actions.Add(new Movement(ShamanSpeed));
                        actions.Add(new AttackBehaviour(ShamanDmg, ShamanRange));
                        actions.Add(new IdleBehaviour(ShamanVisionRange));
                        actions.Add(new Potion());
                        break;
                    }
                case ObjectType.Troll:
                    {
                        actions.Add(new GetAttacked(TrollHp));
                        actions.Add(new Movement(TrollSpeed));
                        actions.Add(new AttackBehaviour(TrollDmg, TrollRange));
                        actions.Add(new ProgressBuilding());
                        var gather = new GatherResource(new List<ResourceType>
                                { ResourceType.Wood, ResourceType.Iron, ResourceType.Gold, ResourceType.Mana })
                            {
                                mCurrentResource = currentType
                            };
                        actions.Add(gather);
                        actions.Add(new IdleBehaviour(TrollVisionRange));
                        actions.Add(new SpecialAction());
                        break;
                    }
                case ObjectType.Orc1Hero:
                    {
                        actions.Add(new GetAttacked(OrkHeroHp));
                        actions.Add(new Movement(OrkHeroSpeed));
                        actions.Add(new AttackBehaviour(OrkHeroDmg, OrkHeroRange));
                        actions.Add(new IdleBehaviour(OrkHeroVisionRange));
                        actions.Add(new BuildPortal());
                        actions.Add(new SpecialAction());
                        break;
                    }
                case ObjectType.Axeman:
                    {
                        actions.Add(new GetAttacked(AxemanHp));
                        actions.Add(new Movement(AxemanSpeed));
                        actions.Add(new AttackBehaviour(AxemanDmg, AxemanRange));
                        actions.Add(new IdleBehaviour(AxemanVisionRange));
                        break;
                    }
                case ObjectType.Arbalist:
                    {
                        actions.Add(new GetAttacked(ArbalistHp));
                        actions.Add(new Movement(ArbalistSpeed));
                        actions.Add(new AttackBehaviour(ArbalistDmg, ArbalistRange));
                        actions.Add(new IdleBehaviour(ArbalistVisionRange));
                        break;
                    }
                case ObjectType.Phalanx:
                    {
                        actions.Add(new GetAttacked(PhalanxHp));
                        actions.Add(new Movement(PhalanxSpeed));
                        actions.Add(new AttackBehaviour(PhalanxDmg, PhalanxRange));
                        actions.Add(new IdleBehaviour(PhalanxVisionRange));
                        actions.Add(new PhalanxAction());
                        break;
                    }
                case ObjectType.Wolf1Rider:
                    {
                        actions.Add(new GetAttacked(WolfRiderHp));
                        actions.Add(new Movement(WolfRiderSpeed));
                        actions.Add(new AttackBehaviour(WolfRiderDmg, WolfRiderRange));
                        actions.Add(new IdleBehaviour(WolfRiderVisionRange));
                        break;
                    }
                case ObjectType.Dwarf1Hero:
                    {
                        actions.Add(new GetAttacked(DwarfHeroHp));
                        actions.Add(new Movement(DwarfHeroSpeed));
                        actions.Add(new AttackBehaviour(DwarfHeroDmg, DwarfHeroRange));
                        actions.Add(new IdleBehaviour(DwarfHeroVisionRange));
                        actions.Add(new SpecialAction());
                        var events = new Dictionary<EventType, List<IEvent>>
                        {
                            { EventType.SpecialEvent, new List<IEvent> {new SpecialEvent(new Point(0, 0))} }
                        };
                        return new GameObject(type, objectId, position, actions, objectEvents: events,isPlayer: isPlayer, hasChanged: hasChanged);
                    }
                case ObjectType.Builder:
                    {
                        actions.Add(new GetAttacked(BuilderHp));
                        actions.Add(new Movement(BuilderSpeed));
                        actions.Add(new ProgressBuilding());
                        actions.Add(new IdleBehaviour(BuilderVisionRange));
                        actions.Add(new GatherResource(new List<ResourceType>{ResourceType.Wood}));
                        actions.Add(new SpecialAction());
                        break;
                    }
                case ObjectType.Main1Building:
                    {
                        actions.Add(new GetAttacked(MainBuildingHp));
                        actions.Add(new BuildUnit());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        break;
                    }
                case ObjectType.House:
                    {
                        actions.Add(new GetAttacked(HouseHp));
                        actions.Add(new LevelUp());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.Mine:
                    {
                        actions.Add(new GetAttacked(MineHp));
                        actions.Add(new GatherResource(new List<ResourceType> { currentType }, currentType));
                        actions.Add(new LevelUp());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.Tower:
                    {
                        actions.Add(new GetAttacked(TowerHp));
                        actions.Add(new AttackBehaviour(TowerDmg, TowerRange));
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new LevelUp());
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.Military1Camp:
                    {
                        actions.Add(new GetAttacked(Military1CampHp));
                        actions.Add(new BuildUnit());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new LevelUp());
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.Mage1Tower:
                    {
                        actions.Add(new GetAttacked(Mage1TowerHp));
                        actions.Add(new BuildPortal());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.Wall:
                    {
                        actions.Add(new GetAttacked(WallHp));
                        actions.Add(new LevelUp());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.Gate:
                    {
                        actions.Add(new GetAttacked(GateHp));
                        actions.Add(new LevelUp());
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                case ObjectType.New1Building:
                    {
                        actions.Add(new GetAttacked(New1BuildingHp));
                        actions.Add(new ConstructBuilding(objectToBuild, currentType));
                        actions.Add(new IdleBehaviour(TowerVisionRange));
                        actions.Add(new RemoveBuilding());
                        break;
                    }
                default:
                    {
                        Console.WriteLine("ObjectType {0} does not exist yet.", type);
                        type = ObjectType.NoneObject;
                        break;
                    }
            }
            return new GameObject(type, objectId, position, actions, isPlayer: isPlayer, hasChanged: hasChanged);
        }
    }
}