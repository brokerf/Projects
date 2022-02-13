using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LOB.Classes.ObjectManagement.Objects;
namespace LOB.Classes.Data
{
    internal static class Achievements
    {
        internal enum AchievementType
        {
            AllTimeSeconds,
            WonGames,
            BuiltWalls,
            Built10Walls,
            Built100Walls,
            KilledEnemies,
            KilledEnemiesMax,
            Gold,
            GoldMaxGotten,
            OpenedPortal,
            BuiltWallsInAGame,
            KilledEnemiesInAGame,
            WoodInAGame,
            IronInAGame,
            GoldInAGame,
            ManaInAGame,
        }

        internal static readonly Dictionary<AchievementType, int> sAchievements = new Dictionary<AchievementType, int>();

        internal static void SaveAchievements()
        {
            var lines = sAchievements.Where(type => type.Key < AchievementType.BuiltWallsInAGame)
                .Select(keyValuePair => keyValuePair.Key + "/" + keyValuePair.Value).ToList();

            File.WriteAllLines(ContentIo.GetPath + ContentIo.GetPathConnector + "Achievements.txt", lines);
        }

        internal static void LoadAchievements()
        {
            var path = ContentIo.GetPath + ContentIo.GetPathConnector + "Achievements.txt";

            if (!File.Exists(path))
            {
                var achievements = new List<string>();
                for (var i = 0; i < 10; i++)
                {
                    achievements.Add((AchievementType) i + "/" + 0);
                }
                File.WriteAllLines(path, achievements);
            }

            var data = File.ReadAllLines(path);
            foreach (var stat in data)
            {
                var keyValue = stat.Split("/");
                if (!Enum.TryParse(keyValue[0], out AchievementType achieve) ||
                    !int.TryParse(keyValue[1], out var value))
                {
                    continue;
                }

                sAchievements[achieve] = value;
            }
        }

        internal static string SaveSpecific()
        {
            var data = sAchievements.Where(achieve => achieve.Key >= AchievementType.BuiltWallsInAGame)
                .Aggregate(string.Empty, (current, keyValuePair) => current + (keyValuePair.Key + "," + keyValuePair.Value + "/"));

            data = data.Remove(data.Length - 1, 1);
            return data;
        }

        internal static void LoadSpecific(string data)
        {
            foreach (var keyValuePair in data.Split("/"))
            {
                var keyValue = keyValuePair.Split(",");
                if (Enum.TryParse(keyValue[0], out AchievementType achieve) && int.TryParse(keyValue[1], out var value))
                {
                    sAchievements[achieve] = value;
                }
            }
        }

        internal static void LoadSpecificNew()
        {
            for (var i = (int) AchievementType.BuiltWallsInAGame; i <= (int) AchievementType.ManaInAGame ; i++)
            {
                sAchievements[(AchievementType) i] = 0;
            }
        }

        internal static void AddResource(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Wood:
                case ResourceType.Iron:
                case ResourceType.Mana:
                    var resType = (int)type + AchievementType.WoodInAGame;
                    sAchievements[resType]++;
                    break;
                case ResourceType.Gold:
                    AddAchievement(AchievementType.GoldInAGame, 1);
                    break;
                case ResourceType.Population:
                case ResourceType.MaxPopulation:
                case ResourceType.HeroAlive:
                case ResourceType.MainBuildingAlive:
                case ResourceType.KilledEnemies:
                case ResourceType.LostUnits:
                case ResourceType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        internal static void AddTime(int seconds)
        {
            AddAchievement(AchievementType.AllTimeSeconds, seconds);
        }

        public static void AddWin()
        {
            AddAchievement(AchievementType.WonGames, 1);
        }

        public static void AddKill()
        {
            AddAchievement(AchievementType.KilledEnemiesInAGame, 1);
        }

        public static void OpenPortal()
        {
            sAchievements[AchievementType.OpenedPortal]++;
            if (sAchievements[AchievementType.OpenedPortal] == 1)
            {
                CheckAchievement(AchievementType.OpenedPortal);
            }
        }

        private static void AddAchievement(AchievementType type, int amount)
        {
            sAchievements[type] += amount;
            CheckAchievement(type);
        }

        public static void AddWall()
        {
            AddAchievement(AchievementType.BuiltWallsInAGame, 1);
        }

        internal static readonly List<(string, string)> sNewAchievements = new List<(string, string)>();

        public static readonly Dictionary<(AchievementType, int), (string, string)> sText = new Dictionary<(AchievementType, int), (string, string)>
        {
            {(AchievementType.WonGames, 1), ("Winner, winner, chicken dinner!", "You've won your first game.")},
            {(AchievementType.WonGames, 10), ("Stroke of Luck", "You've won 10 games.")},
            {(AchievementType.WonGames, 100), ("Legendary Fighter", "You've won 100 games.")},
            {(AchievementType.Built10Walls, 1), ("Structural integrity", "You've built 10 walls.")},
            {(AchievementType.Built100Walls, 1), ("Homeland Security", "You've built 100 walls.")},
            {(AchievementType.BuiltWalls, 1000), ("Fort Knox", "You've built 1000 walls. (Overall)")},
            {(AchievementType.KilledEnemiesMax, 10), ("Blood thirst", "You've killed 10 enemies.")},
            {(AchievementType.KilledEnemiesMax, 100), ("Destruction", "You've killed 100 enemies.")},
            {(AchievementType.KilledEnemies, 1000), ("Calamity", "You've killed 1000 enemies. (Overall)")},
            {(AchievementType.OpenedPortal, 1), ("Open Sesame", "Open a portal.")},
            {(AchievementType.GoldMaxGotten, 100), ("Its raining money", "You've collected 100 pieces of gold.")},
            {(AchievementType.GoldMaxGotten, 500), ("Treasurer", "You've collected 500 pieces of gold.")},
            {(AchievementType.GoldMaxGotten, 1000), ("Endless Wealth", "You've collected 1000 pieces of gold.")},
        }; 
        private static void CheckAchievement(AchievementType type)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (type)
            {
                case AchievementType.WonGames:
                    if (sAchievements[AchievementType.WonGames] == 1 || sAchievements[AchievementType.WonGames] == 10 ||
                        sAchievements[AchievementType.WonGames] == 100)
                    {
                        sNewAchievements.Add(sText[(AchievementType.WonGames, sAchievements[AchievementType.WonGames])]);
                        SaveAchievements();
                    }

                    break;
                case AchievementType.BuiltWallsInAGame:
                    sAchievements[AchievementType.BuiltWalls]++;
                    switch (sAchievements[AchievementType.BuiltWallsInAGame])
                    {
                        case 10:
                            if (sAchievements[AchievementType.Built10Walls] == 1)
                            {
                                return;
                            }
                            sNewAchievements.Add(sText[(AchievementType.Built10Walls, 1)]);
                            sAchievements[AchievementType.Built10Walls] = 1;
                            SaveAchievements();
                            break;
                        case 100:
                            if (sAchievements[AchievementType.Built100Walls] == 1)
                            {
                                return;
                            }
                            sNewAchievements.Add(sText[(AchievementType.Built100Walls, 1)]);
                            sAchievements[AchievementType.Built100Walls] = 1;
                            SaveAchievements();
                            break;
                    }
                    if (sAchievements[AchievementType.BuiltWalls] == 1000)
                    {
                        sNewAchievements.Add(sText[(AchievementType.BuiltWalls, 1000)]);
                        SaveAchievements();
                    }
                    break;
                case AchievementType.KilledEnemiesInAGame:
                    sAchievements[AchievementType.KilledEnemies]++;
                    switch (sAchievements[AchievementType.KilledEnemiesInAGame])
                    {
                        case 10:
                            if (sAchievements[AchievementType.KilledEnemiesMax] >= 10)
                            {
                                return;
                            }
                            sNewAchievements.Add(sText[(AchievementType.KilledEnemiesMax, 10)]);
                            sAchievements[AchievementType.KilledEnemiesMax] = 10;
                            SaveAchievements();
                            break;
                        case 100:
                            if(sAchievements[AchievementType.KilledEnemiesMax] == 100)
                            {
                             return;
                            }
                            sNewAchievements.Add(sText[(AchievementType.KilledEnemiesMax, 100)]);
                            sAchievements[AchievementType.KilledEnemiesMax] = 100;
                            SaveAchievements();
                            break;
                    }
                    if (sAchievements[AchievementType.KilledEnemies] == 1000)
                    {
                        sNewAchievements.Add(sText[(AchievementType.KilledEnemies, 1000)]);
                        SaveAchievements();
                    }
                    break;
                case AchievementType.OpenedPortal when sAchievements[AchievementType.OpenedPortal] == 1:
                    sNewAchievements.Add(sText[(AchievementType.OpenedPortal, 1)]);
                    SaveAchievements();
                    break;
                case AchievementType.GoldInAGame:
                    sAchievements[AchievementType.Gold]++;
                    if (sAchievements[AchievementType.GoldInAGame] == 100 ||
                        sAchievements[AchievementType.GoldInAGame] == 500 ||
                        sAchievements[AchievementType.GoldInAGame] == 1000)
                    {
                        if (sAchievements[AchievementType.GoldMaxGotten] >= sAchievements[AchievementType.GoldInAGame])
                        {
                            return;
                        }

                        sAchievements[AchievementType.GoldMaxGotten] = sAchievements[AchievementType.GoldInAGame];
                        sNewAchievements.Add(sText[(AchievementType.GoldMaxGotten,
                            sAchievements[AchievementType.GoldInAGame])]);
                        SaveAchievements();
                        
                    }

                    break;
            }
        }
    }
}
