using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.Data
{
    internal sealed class ContentIo
    {
        private readonly GameMap mMap;

        public ContentIo(GameMap map)
        {
            mMap = map;
        }

        internal static string GetPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "LOB";
        internal static char GetPathConnector => Path.DirectorySeparatorChar;

        public static void InitializeDataStorage(int width, int height, out GameObjectManagement gameObjectManagement)
        {
            DataStorage.mGameObjects = new Dictionary<int, GameObject>();
            DataStorage.mGameEvents = new Dictionary<int, List<IEvent>>();
            DataStorage.mGameObjectPositions = new Dictionary<Point, int>();
            DataStorage.mPlayerById = new Dictionary<bool, List<int>>
                { { true, new List<int>() }, { false, new List<int>() } };
            GameObjectManagement.mIdsToDelete = new List<int>();
            DataStorage.mUnitAnimationOffset = new Dictionary<Point, Vector2>();
            Particle.mForegroundParticles = new List<Particle>();
            Particle.mBackgroundParticles = new List<Particle>();
            ParticleEmitter.mForegroundEmitters = new List<ParticleEmitter>();
            DataStorage.mGameStatistics = new Dictionary<bool, Dictionary<ResourceType, int>>
            {
                [true] = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Wood, 50 },
                    { ResourceType.Iron, 25 },
                    { ResourceType.Gold, 10 },
                    { ResourceType.Mana, 0 },
                    { ResourceType.Population, 1 },
                    { ResourceType.MaxPopulation, 0 },
                    { ResourceType.HeroAlive, 0 },
                    { ResourceType.MainBuildingAlive, 1 },
                    { ResourceType.KilledEnemies, 0},
                    { ResourceType.LostUnits, 0}
                },
                [false] = new Dictionary<ResourceType, int>
                {

                    { ResourceType.Wood, 0 },
                    { ResourceType.Iron, 600},
                    { ResourceType.Gold, 500 },
                    { ResourceType.Mana, 100 },
                    { ResourceType.Population, 1 },
                    { ResourceType.MaxPopulation, 0 },
                    { ResourceType.HeroAlive, 0 },
                    { ResourceType.MainBuildingAlive, 1 }
                }
            };
            DataStorage.mBuildError.Item1 = string.Empty;
            ObjectFactory.ObjectFactoryReset(width, height);
            gameObjectManagement = new GameObjectManagement();
        }

        public (int width, int height) Load(string path, out GameObjectManagement gameObjectManagement)
        {
            var lines = File.ReadAllLines(path).ToList();
            var data = lines[1].Split("\"");
            int.TryParse(data[9], out var width);
            int.TryParse(data[11], out var height);

            InitializeDataStorage(width, height, out gameObjectManagement);

            mMap.mTiles = new GameMap.Tile[width, height];
            int.TryParse(data[13], out GameMap.mStandardTileSize);

            int y;
            for (y = 0; y < height; y++)
            {
                int x;
                for (x = 0; x < width; x++)
                {
                    mMap.mTiles[x, y] = new GameMap.Tile(ObjectType.Grass);
                }
            }

            var portalData = string.Empty;

            for (var i = 5 + height; i < lines.Count; i++)
            {
                if (lines[i].Contains("<object id=\"4520\""))
                {
                    portalData = LoadState(lines[i+3].Split("\"")[3]);
                    continue;
                }
                if (!lines[i].Contains("<object "))
                {
                    continue;
                }

                i += 3;
                var objectData = lines[i].Split("\"")[3];
                var newObject = LoadObject(objectData);
                DataStorage.mPlayerById[newObject.mIsPlayer].Add(newObject.mObjectId);
                DataStorage.AddObject(newObject);
            }

            DataStorage.mAStar = new AStar(width, height);
            DataStorage.mAStar.LoadPortals(portalData);
            return (width, height);
        }

        private static string LoadState(string line)
        {
            var data = line.Split("$");
            Movement.LoadReservedTiles(data[1]);
            LoadStatistics(data[2]);
            DataStorage.mGameStatistics[true][ResourceType.MaxPopulation] = 0;
            DataStorage.mGameStatistics[false][ResourceType.MaxPopulation] = 0;
            var races = data[3].Split("/");
            Enum.TryParse(races[0], out Game1.mPlayerRace);
            Enum.TryParse(races[1], out Game1.mEnemyRace);
            Achievements.LoadSpecific(data[4]);
            return data[0];
        }

        private static GameObject LoadObject(string data)
        {
            var stats = data.Split("$");
            var notFailed = Enum.TryParse<ObjectType>(stats[0], out var name);
            notFailed = int.TryParse(stats[1], out var id) && notFailed;
            var pos = stats[2].Split("|");
            notFailed = int.TryParse(pos[0], out var x) && notFailed;
            notFailed = int.TryParse(pos[1], out var y) && notFailed;
            notFailed = LoadActions(stats[3], out var actions) && notFailed;
            notFailed = LoadEvents(stats[4], out var events) && notFailed;
            notFailed = int.TryParse(stats[5], out var objectState) && notFailed;
            notFailed = bool.TryParse(stats[6], out var isPlayer) && notFailed;
            notFailed = bool.TryParse(stats[7], out var isMoving) && notFailed;

            if (!notFailed)
            {
                throw new FileLoadException("Couldn't load GameObject");
            }

            ObjectFactory.Remove(id);
            return new GameObject(name,
                id,
                new Point(x, y),
                actions,
                events,
                objectState,
                isPlayer)
            {
                mIsMoving = isMoving
            };
        }

        private static bool LoadEvents(string data, out Dictionary<EventType, List<IEvent>> events)
        {
            events = new Dictionary<EventType, List<IEvent>>();
            if (data == string.Empty)
            {
                return true;
            }

            var notFailed = true;
            try
            {
                foreach (var eventData in data.Remove(0, 1).Split("["))
                {
                    var subData = eventData.Split(":(");
                    notFailed = Enum.TryParse<EventType>(subData[0], out var type) && notFailed;
                    switch (type)
                    {
                        case EventType.MoveEvent:
                            if (!events.ContainsKey(EventType.MoveEvent))
                            {
                                events[EventType.MoveEvent] = new List<IEvent>();
                            }
                            events[EventType.MoveEvent].Add(MoveEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.BuilderOrderEvent:
                            if (!events.ContainsKey(EventType.BuilderOrderEvent))
                            {
                                events[EventType.BuilderOrderEvent] = new List<IEvent>();
                            }
                            events[EventType.BuilderOrderEvent].Add(BuilderOrderEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.BuildProgressEvent:
                            if (!events.ContainsKey(EventType.BuildProgressEvent))
                            {
                                events[EventType.BuildProgressEvent] = new List<IEvent>();
                            }
                            events[EventType.BuildProgressEvent].Add(BuildProgressEvent.LoadEvent());
                            break;
                        case EventType.BuildUnitEvent:
                            if (!events.ContainsKey(EventType.BuildUnitEvent))
                            {
                                events[EventType.BuildUnitEvent] = new List<IEvent>();
                            }
                            events[EventType.BuildUnitEvent].Add(BuildUnitEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.AttackEvent:
                            if (!events.ContainsKey(EventType.AttackEvent))
                            {
                                events[EventType.AttackEvent] = new List<IEvent>();
                            }
                            events[EventType.AttackEvent].Add(AttackEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.GatherResourceEvent:
                            if (!events.ContainsKey(EventType.GatherResourceEvent))
                            {
                                events[EventType.GatherResourceEvent] = new List<IEvent>();
                            }
                            events[EventType.GatherResourceEvent].Add(GatherResourceEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.LevelUpEvent:
                            if (!events.ContainsKey(EventType.LevelUpEvent))
                            {
                                events[EventType.LevelUpEvent] = new List<IEvent>();
                            }
                            events[EventType.LevelUpEvent].Add(LevelUpEvent.LoadEvent());
                            break;
                        case EventType.GetAttackedEvent:
                            if (!events.ContainsKey(EventType.GetAttackedEvent))
                            {
                                events[EventType.GetAttackedEvent] = new List<IEvent>();
                            }
                            events[EventType.GetAttackedEvent].Add(GetAttackedEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.PortalEvent:
                            if (!events.ContainsKey(EventType.PortalEvent))
                            {
                                events[EventType.PortalEvent] = new List<IEvent>();
                            }
                            events[EventType.PortalEvent].Add(PortalOpeningEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.None:
                            break;
                        case EventType.SpecialEvent:
                            if (!events.ContainsKey(EventType.SpecialEvent))
                            {
                                events[EventType.SpecialEvent] = new List<IEvent>();
                            }
                            events[EventType.SpecialEvent].Add(SpecialEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.PotionEvent:
                            if (!events.ContainsKey(EventType.PotionEvent))
                            {
                                events[EventType.PotionEvent] = new List<IEvent>();
                            }
                            events[EventType.PotionEvent].Add(PotionEvent.LoadEvent(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.PhalanxEvent:
                            events[EventType.PhalanxEvent] = new List<IEvent> {PhalanxEvent.LoadEvent()};
                            break;
                        case EventType.RemoveBuildingEvent:
                            events[EventType.RemoveBuildingEvent] = new List<IEvent> {RemoveBuildingEvent.LoadEvent()};
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                return notFailed;
            }
            catch
            {
                return false;
            }
        }

        private static bool LoadActions(string data, out List<IAction> actions)
        {
            actions = new List<IAction>();
            if (data == string.Empty)
            {
                return true;
            }

            var notFailed = true;
            try
            {
                foreach (var actionData in data.Remove(0, 1).Split("["))
                {
                    var subData = actionData.Split(":(");
                    notFailed = Enum.TryParse<EventType>(subData[0], out var type) && notFailed;
                    switch (type)
                    {
                        case EventType.MoveEvent:
                            actions.Add(Movement.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.BuildProgressEvent:
                            actions.Add(ConstructBuilding.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.BuilderOrderEvent:
                            actions.Add(ProgressBuilding.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.BuildUnitEvent:
                            actions.Add(BuildUnit.LoadAction());
                            break;
                        case EventType.AttackEvent:
                            actions.Add(AttackBehaviour.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.GatherResourceEvent:
                            actions.Add(GatherResource.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.LevelUpEvent:
                            actions.Add(LevelUp.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.GetAttackedEvent:
                            actions.Add(GetAttacked.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.PortalEvent:
                            actions.Add(BuildPortal.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.None:
                            actions.Add(IdleBehaviour.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.SpecialEvent:
                            actions.Add(SpecialAction.LoadAction());
                            break;
                        case EventType.PotionEvent:
                            actions.Add(Potion.LoadAction());
                            break;
                        case EventType.PhalanxEvent:
                            actions.Add(PhalanxAction.LoadAction(subData[1].Replace(")]", string.Empty)));
                            break;
                        case EventType.RemoveBuildingEvent:
                            actions.Add(RemoveBuilding.LoadAction());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                return notFailed;
            }
            catch
            {
                return false;
            }
        }

        public void SaveMap(int position, string name, int playedTime)
        {
            if (name == string.Empty)
            {
                name = "Tom";
            }

            var path = GetPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += GetPathConnector;
            var lineTiles = string.Concat(Enumerable.Repeat("1,", GameMap.mWidth));
            
            var lines = new List<string>
            {
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
                "<map version=\"1.5\" tiledversion=\"1.7.2\" orientation=\"orthogonal\" renderorder=\"right-down\" width=\"" +
                GameMap.mWidth + "\" height=\"" + GameMap.mHeight +
                "\" tilewidth=\"77\" tileheight=\"77\" infinite=\"0\" nextlayerid=\"12\" nextobjectid=\"0\">",
                " <tileset firstgid=\"1\" source=\"" + path + "TileSet.tsx\"/>",
                " <layer id=\"1\" name=\"Background\" width=\"" + GameMap.mWidth + "\" height=\"" + GameMap.mHeight + "\">",
                "  <data encoding=\"csv\">"
            };
            lines.AddRange(Enumerable.Repeat(lineTiles, GameMap.mHeight - 1));
            lines.Add(lineTiles.Remove(lineTiles.Length - 1));

            var objectLines = SaveState();

            foreach (var gameObject in DataStorage.mGameObjects.Values.Where(obj => obj.mName != ObjectType.Barrier))
            {
                objectLines.AddRange(new List<string>
                {
                    "   <object id=\"" + (92 + 4500 - gameObject.mObjectId) + "\" gid=\"7\" x=\"" +
                    gameObject.mObjectPosition.X * GameMap.mStandardTileSize + "\" y=\"" +
                    gameObject.mObjectPosition.Y * GameMap.mStandardTileSize + "\" width=\"77\" height=\"77\">",
                    "    <properties>",
                    "     <property name=\"type\" value=\"" + gameObject.mName.ToString().Replace("1", " ") + "\"/>"
                });
                objectLines.Add("     <property name=\"data\" value=\"" + gameObject.SaveObject() + "\"/>");
                objectLines.AddRange(new List<string>
                {
                    "    </properties>",
                    "   </object>"
                });
            }

            // ReSharper disable StringLiteralTypo
            lines.AddRange(new List<string>
            {
                "</data>",
                " </layer>",
                " <group id=\"6\" name=\"GameObjects\">",
                "  <objectgroup id=\"5\" name=\"Barrier\">"
            });
            lines.AddRange(objectLines);
            lines.AddRange(new List<string>
            {
                "  </objectgroup>",
                "  <group id=\"8\" name=\"Preset Test Setup\">",
                "   <objectgroup id=\"9\" name=\"Houses\">",
                "   </objectgroup>",
                "   <objectgroup id=\"10\" name=\"ResourcePoints\">",
                "   </objectgroup>",
                "   <objectgroup id=\"7\" name=\"Trees\">",
                "   </objectgroup>",
                "   <objectgroup id=\"11\" name=\"Units\">",
                "   </objectgroup>",
                "  </group>",
                " </group>",
                "</map>"
            });

            position--;
            try
            {
                Save();
            }
            catch
            {
                Directory.CreateDirectory(path);
                Save();
            }

            void Save()
            {
                if (!File.Exists(path + "TileSetImage.png") ||
                    !File.Exists(path + "TileSet.tmx"))
                {

                    string[] tileSetLines =
                    {
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
                        "<tileset version=\"1.5\" tiledversion=\"1.7.2\" name=\"TileSet\" tilewidth=\"77\" tileheight=\"77\" tilecount=\"12\" columns=\"6\">",
                        "<image source=\"" + path + "TileSetImage.png\" width=\"462\" height=\"154\"/>",
                        "</tileset>"
                    };
                    Stream stream = File.Create(path + "TileSetImage.png");
                    mMap.mTileSetImage.SaveAsPng(stream,
                        mMap.mTileSetImage.Bounds.Width,
                        mMap.mTileSetImage.Bounds.Height);
                    stream.Close();
                    File.WriteAllLines(path + "TileSet.tsx", tileSetLines);

                }

                if (File.Exists(path + "Save" + position + ".tmx"))
                {
                    File.Delete(path + "Save" + position + ".tmx");
                }

                WriteName();

                File.WriteAllLines(path + "Save" + position + ".tmx", lines);
            }

            void WriteName()
            {
                var names = GetNames().ToList();
                var data = new List<string>();
                var i = 0;
                foreach (var (otherName, time) in names)
                {
                    if (i == position)
                    {
                        data.Add(i + "|" + name + ":" + playedTime);
                    }
                    else
                    {
                        data.Add(otherName == null ? i + "|" : i + "|" + otherName + ":" + time);
                    }

                    i++;
                }

                File.WriteAllLines(path + "SaveNames.txt", data);
            }
        }

        internal const int MaxNumberOfSaves = 5;

        internal static IEnumerable<(string, int)> GetNames()
        {
            var path = GetPath;
            var names = new List<(string, int)>();

            var namesShorter = new List<(string, int)>();
            if (File.Exists(path + GetPathConnector + "SaveNames.txt"))
            {
                var lines = File.ReadAllLines(path + GetPathConnector + "SaveNames.txt");
                var failed = false;

                // Finds all save names that work, and fills empty spaces with new slots

                foreach (var line in lines)
                {
                    try
                    { 
                        var data = line.Split("|");
                        while (Convert.ToInt32(data[0]) > names.Count)
                        {
                            names.Add((null, 0));
                        }
                        names.Add((data[1].Split(":")[0], Convert.ToInt32(data[1].Split(":")[1])));
                    }
                    catch
                    {
                        names.Add((null, 0));
                        failed = true;
                    }
                }

                // Removes empty / broken save names at the end of the names list (so that only one new empty save appears).
                // Empty files in between other saves are preserved

                var foundSave = false;
                for (var i = names.Count - 1; i > -1; i--)
                {
                    if (names[i].Item1 != null)
                    {
                        foundSave = true;
                    }

                    if (foundSave)
                    {
                        namesShorter.Insert(0, names[i]);
                    }
                }

                // Ads new empty save, if there is space for another one
                if (namesShorter.Count < MaxNumberOfSaves)
                {
                    names.Add((null, 0));
                }

                if (!failed)
                {
                    return names;
                }
            }


            var names2 = new string[namesShorter.Count];
            for (var i = 0; i < namesShorter.Count; i++)
            {
                names2[i] = i + "|" + (namesShorter[i].Item1 == null ? string.Empty : namesShorter[i].Item1+":"+ namesShorter[i].Item2);
            }

            File.WriteAllLines(path + GetPathConnector + "SaveNames.txt", names2);
            return names;
        }

        private static List<string> SaveState()
        {

            var stateLines = new List<string>();
            stateLines.AddRange(new List<string>
            {
                "   <object id=\"" + 4520 + "\" gid=\"7\" x=\"0\" y=\"0\" width=\"77\" height=\"77\">",
                "    <properties>",
                "     <property name=\"type\" value=\"Data\"/>"
            });
            stateLines.Add("     <property name=\"data\" value=\""+ DataStorage.mAStar.SavePortals() + "$" + Movement.SaveReservedTiles() + "$" + SaveStatistics() + "$" + SaveRaces() + "$" + Achievements.SaveSpecific() +"\"/>");
            stateLines.AddRange(new List<string>
            {
                "    </properties>",
                "   </object>"
            });
            return stateLines;
        }

        private static string SaveRaces()
        {
            return Game1.mPlayerRace + "/"+ Game1.mEnemyRace;
        }

        private static string SaveStatistics()
        {
            var data = DataStorage.mGameStatistics[true].Values.Aggregate("?", (current, keyValuePair) => current + "," + keyValuePair);
            data = DataStorage.mGameStatistics[false].Values.Aggregate(data + "|", (current, keyValuePair) => current + "," + keyValuePair);
            return data.Replace("?,", string.Empty);
        }

        private static void LoadStatistics(string data)
        {
            var resources = data.Split("|");
            var keys = DataStorage.mGameStatistics[true].Keys.ToList();
            var resValues = resources[0].Split(",");
            for (var i = 0; i < resValues.Length; i++) 
            {
                DataStorage.mGameStatistics[true][keys[i]] = Convert.ToInt32(resValues[i]);
            } 
        }
    }
}