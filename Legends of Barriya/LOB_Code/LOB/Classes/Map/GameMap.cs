#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.Data;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;

namespace LOB.Classes.Map
{
    internal sealed class GameMap
    {
        internal readonly Texture2D mTileSetImage;
        internal static int mWidth;
        internal static int mHeight;
        internal static List<Point> mBarrierData = new List<Point>();
        internal static int mStandardTileSize;
        internal Tile[,] mTiles;
        internal bool mIsPaused;
        private static readonly bool sRealMap = true;
        //private static List<ObjectType> sBuilds = new List<ObjectType>(4);

        public GameMap(ContentManager content, out GameObjectManagement gameObjectManagement, int position = 0, (int, int) size = default)
        {
            //sBuilds.Add(ObjectType.House);
            //sBuilds.Add(ObjectType.Mine);
            //sBuilds.Add(ObjectType.Mage1Tower);
            //sBuilds.Add(ObjectType.Military1Camp);
            Movement.ResetReserved();
            DataStorage.mAStar = null;
            mTiles = new Tile[1,1];
            mTileSetImage = content.Load<Texture2D>("TileSetImage");
            // Tries loading a previously saved map, if the one specified doesn't exist, it loads the standard map
            if (!(position < 1 || position > ContentIo.MaxNumberOfSaves))
            {
                try
                {
                    var path = ContentIo.GetPath + ContentIo.GetPathConnector;
                    position--;
                    (mWidth, mHeight) = new ContentIo(this).Load(path + "Save" + position + ".tmx", out gameObjectManagement);
                    CreateBarrier();
                    return;
                }
                catch
                {
                    //
                }
            }

            if (sRealMap)
            {
                mStandardTileSize = 77;
                mWidth = size.Item1;
                mHeight = size.Item2;
                ContentIo.InitializeDataStorage(mWidth, mHeight, out gameObjectManagement);
                CreateTiles();
                CreateBarrier();
                var distanceToSide = (int)Math.Floor(mWidth / 4f);
                var distanceToTop = (int)Math.Floor(mHeight / 2f);
                var buildPlayer = ObjectFactory.BuildObject(Game1.mPlayerRace != Game1.Race.Orc ? ObjectType.Builder : ObjectType.Troll, new Point(distanceToSide+1, distanceToTop));
                DataStorage.mGameObjects[buildPlayer.mObjectId] = buildPlayer;
                DataStorage.mGameObjectPositions[buildPlayer.mObjectPosition] = buildPlayer.mObjectId;
                var mainPlayer = ObjectFactory.BuildObject(ObjectType.Main1Building, new Point(distanceToSide, distanceToTop));
                DataStorage.mGameObjects[mainPlayer.mObjectId] = mainPlayer;
                DataStorage.mGameObjectPositions[mainPlayer.mObjectPosition] = mainPlayer.mObjectId;

                var buildEnemy = ObjectFactory.BuildObject(Game1.mEnemyRace != Game1.Race.Orc ? ObjectType.Builder : ObjectType.Troll, new Point(mWidth-distanceToSide-1, distanceToTop), 0, ResourceType.None, false);
                DataStorage.mGameObjects[buildEnemy.mObjectId] = buildEnemy;
                DataStorage.mGameObjectPositions[buildEnemy.mObjectPosition] = buildEnemy.mObjectId;
                var mainEnemy = ObjectFactory.BuildObject(ObjectType.Main1Building, new Point(mWidth - distanceToSide, distanceToTop), 0, ResourceType.None, false);
                DataStorage.mGameObjects[mainEnemy.mObjectId] = mainEnemy;
                DataStorage.mGameObjectPositions[mainEnemy.mObjectPosition] = mainEnemy.mObjectId;

                CreateResourcePoints(false);
                CreateResourcePoints(true);

                DataStorage.mAStar = new AStar(mWidth, mHeight);
                Achievements.LoadSpecificNew();
                return;

            }

            TiledMap tiledMap = content.Load<TiledMap>("VorstellungsKarte");

            mStandardTileSize = tiledMap.TileHeight;
            mWidth = tiledMap.Width;
            mHeight = tiledMap.Height;

            CreateTiles();

            ContentIo.InitializeDataStorage(mWidth, mHeight, out gameObjectManagement);

            CreateBarrier();

            // Creates the Objects in all ObjectLayers of the Map, reads a user defined value from each object to create an IGameObject (SpriteID)
            var objectsInLayers = tiledMap.ObjectLayers;
            foreach (var tiledMapObjectLayer in objectsInLayers)
            {
                foreach (var tiledMapObject in tiledMapObjectLayer.Objects)
                {
                    var x = (int)tiledMapObject.Position.X / mStandardTileSize;
                    var y = (int)tiledMapObject.Position.Y / mStandardTileSize;
                    if (!tiledMapObject.Properties.TryGetValue("type", out var type))
                    {
                        continue;
                    }

                    if (!Enum.TryParse<ObjectType>(type.Replace(" ", "1"), out var enumType) ||
                        enumType >= ObjectType.BarrierLeft && enumType <= ObjectType.Barrier)
                    {
                        continue;
                    }

                    var isPlayer = false;
                    if (sRealMap && tiledMapObject.Properties.TryGetValue("isPlayer", out type))
                    {
                        bool.TryParse(type, out isPlayer);
                    }

                    var tempObject = ObjectFactory.BuildObject(enumType, new Point(x, y), 0, enumType == ObjectType.Mine? ResourceType.Iron : ResourceType.None, isPlayer);
                    GameObjectManagement.mObjectsToAdd.Add(tempObject);
                }
            }

            DataStorage.mAStar = new AStar(mWidth, mHeight);
            Achievements.LoadSpecificNew();

            if (!sRealMap)
            {
                CreateTestObjects();
            }
        }

        private const float RocksPerTile = 1f / 200;
        private const float TreesPerTile = 1f / 20;
        private const float IronPerTile = 1f / 30;
        private const float GoldPerTile = 1f / 100;
        private const float ManaPerTile = 1f / 150;

        private void CreateResourcePoints(bool isPlayer)
        {
            var (barrierStart, barrierWidth) = GetBarrierWidthAndPosition();
            var startX = isPlayer ? 0 : barrierStart + barrierWidth + 3;
            var width = startX+mWidth - (barrierStart + barrierWidth + 3);
            CreateTrees(startX, width, isPlayer, ObjectType.Tree, TreesPerTile);
            CreateTrees(startX, width, isPlayer, ObjectType.Iron1Source, IronPerTile);
            CreateTrees(startX, width, isPlayer, ObjectType.Gold1Vein, GoldPerTile);
            CreateTrees(startX, width, isPlayer, ObjectType.Mana1Source, ManaPerTile);
            CreateTrees(startX, width, isPlayer, ObjectType.Rock, RocksPerTile);
        }

        private void CreateTrees(int startX, int maxX, bool isPlayer, ObjectType type, float amount)
        {
            var amountTrees = mHeight * amount;
            var toSpawn = 0f;
            for (var x = startX; x < maxX; x++)
            {
                var side = ((x - startX) / (float)(maxX - startX) - 0.5f) * (amount / TreesPerTile);
                toSpawn += amountTrees;
                for (var i = 0; i < toSpawn; i++)
                {
                    toSpawn--;

                    var y = GetNextRandomHeight(x, side);
                    var tempObject = ObjectFactory.BuildObject(type, new Point(x, y), 0, ResourceType.None, isPlayer);
                    GameObjectManagement.mObjectsToAdd.Add(tempObject);
                }
            }
        }

        private int GetNextRandomHeight(int x, float closeness)
        {
            var y = ParticleEmitter.sRandom.Next((int)Math.Ceiling(mHeight * Math.Min(Math.Abs(closeness)+0.2, 1)));
            if (ParticleEmitter.sRandom.Next(2) == 1)
            {
                y = mHeight - y;
            }
            while (GameObjectManagement.mObjectsToAdd.Exists(gameObject => gameObject.mObjectPosition == new Point(x, y)))
            {
                y++;
                if (y >= mHeight)
                {
                    y = 0;
                }
            }
            return y;
        }

        private void CreateTiles()
        {
            mTiles = new Tile[mWidth, mHeight];

            for (var y = 0; y < mHeight; y++)
            {
                for (var x = 0; x < mWidth; x++)
                {
                    mTiles[x, y] = new Tile(ObjectType.Grass);
                }
            }
        }

        private void CreateBarrier()
        {
            mBarrierData = new List<Point>();
            //Creates the barriers
            foreach (var (objectPosition, objState) in GetBarrierPositions(mHeight))
            {
                var tempObject = ObjectFactory.BuildObject(ObjectType.Barrier, objectPosition, objState);
                DataStorage.AddObject(tempObject);
                mBarrierData.Add(objectPosition);
            }
        }

        private void CreateTestObjects()
        {
            // FROM HERE ONLY TEST OBJECTS

            var exampleObjects = new List<ObjectType> { ObjectType.Tree, ObjectType.Gold1Vein, ObjectType.Iron1Source, ObjectType.Mana1Source, ObjectType.Rock,
                ObjectType.Knight, ObjectType.Archer, ObjectType.Horseman, ObjectType.Mage, ObjectType.Puncher, ObjectType.Slingshot, ObjectType.Shaman, ObjectType.Axeman, ObjectType.Arbalist, ObjectType.Phalanx, ObjectType.Wolf1Rider,
                ObjectType.Human1Hero, ObjectType.Orc1Hero, ObjectType.Dwarf1Hero,
                ObjectType.Builder, ObjectType.Troll, ObjectType.Main1Building, ObjectType.House, ObjectType.Mine, ObjectType.Tower, ObjectType.Military1Camp, ObjectType.Mage1Tower, ObjectType.Wall, ObjectType.Gate };
            var x = 0;
            var y = 1;
            foreach (var tempObject in exampleObjects.Select(objectName => ObjectFactory.BuildObject(objectName, new Point(x, y))))
            {
                y = 1;
                DataStorage.AddObject(tempObject);
                x++;
            }

            //enemy objects
            var enemyCamp = ObjectFactory.BuildObject(ObjectType.Military1Camp, new Point(80, 20), 0, ResourceType.None, false);
            DataStorage.AddObject(enemyCamp);

            var enemyBuilder = ObjectFactory.BuildObject(ObjectType.Builder, new Point(80, 21), 0, ResourceType.None, false);
            DataStorage.AddObject(enemyBuilder);

            var enemyResource =
                ObjectFactory.BuildObject(ObjectType.Gold1Vein, new Point(80, 22), 0, ResourceType.Gold, false);
            DataStorage.AddObject(enemyResource);

            for (var i = 0; i < 8; i++)
            {
                var tempEnemyMine = new GameObject(ObjectType.Mine,
                    ObjectFactory.Pop(),
                    new Point(mWidth / 2 + 10, 20 + i),
                    new List<IAction>
                        { new GatherResource(new List<ResourceType> { ResourceType.Mana }, ResourceType.Mana), new LevelUp(3), new GetAttacked(100, 100) },
                    isPlayer: false);
                DataStorage.AddObject(tempEnemyMine);
                DataStorage.mPlayerById[false].Add(tempEnemyMine.mObjectId);
            }

            for(var i = 0; i < 8; i++)
            {
                var tempEnemySoldier = new GameObject(ObjectType.Knight,
                    ObjectFactory.Pop(),
                    new Point(mWidth / 2 + 10 + i, 20 + 8),
                    new List<IAction>
                        {new Movement(10), new AttackBehaviour(10, 2), new GetAttacked(100, 100), new IdleBehaviour(3)},
                    isPlayer: false);
                DataStorage.AddObject(tempEnemySoldier);
                DataStorage.mPlayerById[false].Add(tempEnemySoldier.mObjectId);
            }

            var tempEnemyMageTower = new GameObject(ObjectType.Mage1Tower,
                ObjectFactory.Pop(),
                new Point(mWidth / 2 + 10, 20 + 9),
                new List<IAction>
                    { new BuildPortal(), new GetAttacked(100, 100)},
                isPlayer: false);
            DataStorage.AddObject(tempEnemyMageTower);
            DataStorage.mPlayerById[false].Add(tempEnemyMageTower.mObjectId);

        }

        internal sealed class Tile
        {
            internal readonly ObjectType mTileType;
            internal const int TileState = 0;

            /// <summary>
            /// A Tile is used by the Map to store data.
            /// </summary>
            public Tile(ObjectType tileType)
            {
                mTileType = tileType;
            }
        }

        public void SetPaused(bool paused)
        {
            mIsPaused = paused;
        }

        // width is actually referring to the middle part.
        public static (int, int) GetBarrierWidthAndPosition()
        {
            return GetBarrierWidthAndPosition(mWidth);
        }

        //
        internal static (int, int) GetBarrierWidthAndPosition(int width)
        {
            return ((int)Math.Ceiling((width / 2f) - 2), 2 - width % 2);
        }

        private IEnumerable<(Point, int)> GetBarrierPositions(int height)
        {
            var positions = new List<(Point, int)>();
            var (startBarrier, widthOfBarrier) = GetBarrierWidthAndPosition();

            for (var y = 1; y <= height; y++)
            {
                positions.Add((new Point(startBarrier, y), 0));
                positions.Add((new Point(startBarrier+1, y), 1));
                if (widthOfBarrier == 2)
                {
                    positions.Add((new Point(startBarrier + widthOfBarrier, y), 1));
                }
                positions.Add((new Point(startBarrier + widthOfBarrier+1, y), 2));
            }

            return positions;
        }
    }
}
