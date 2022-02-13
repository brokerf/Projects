using System.Collections.Generic;

namespace LOB.Classes.ObjectManagement.Objects
{
    internal static class BuildingCosts
    {
        internal static readonly Dictionary<ObjectType, List<int>> sBuildingCosts = new Dictionary<ObjectType, List<int>>
        {
            {ObjectType.Mage1Tower, new List<int>{ 30, 10, 0, 15}},
            {ObjectType.House, new List<int>{ 50, 10, 0, 0}},
            {ObjectType.Mine, new List<int>{ 30, 0, 0, 0}},
            {ObjectType.Military1Camp, new List<int>{ 50, 25, 0, 0}},
            {ObjectType.Wall, new List<int>{ 50, 25, 0, 0}},
            {ObjectType.Tower, new List<int>{ 80, 25, 0, 0}},
            {ObjectType.Gate, new List<int>{ 45, 30, 0, 0}}
        };

        internal static readonly Dictionary<(ObjectType mName, int mCurrentLevel), List<int>> sUpgradeCosts = new Dictionary<(ObjectType mName, int mCurrentLevel), List<int>>
        {
            { (ObjectType.House, 1), new List<int>{ 70, 15, 0, 0}},
            { (ObjectType.House, 2), new List<int>{ 80, 20, 0, 0}},
            { (ObjectType.Mine, 1), new List<int>{ 50, 15, 0, 0}},
            { (ObjectType.Mine, 2), new List<int>{ 70, 30, 15, 0}},
            { (ObjectType.Military1Camp, 1), new List<int>{ 50, 35, 5, 0}},
            { (ObjectType.Military1Camp, 2), new List<int>{ 50, 50, 5, 1}},
            { (ObjectType.Wall, 1), new List<int>{ 70, 30, 0, 0}},
            { (ObjectType.Wall, 2), new List<int>{ 80, 40, 0, 0}},
            { (ObjectType.Tower, 1), new List<int>{ 80, 40, 5, 0}},
            { (ObjectType.Tower, 2), new List<int>{ 80, 55, 10, 0}},
            { (ObjectType.Gate, 1), new List<int>{ 70, 30, 0, 0}},
            { (ObjectType.Gate, 2), new List<int>{ 80, 60, 0, 0}}
        };

        internal static bool HasEnoughForCreation(ObjectType building, bool isPlayer = true, double percentage = 1)
        {
            var costs = sBuildingCosts[building];
            ResourceType i = 0;
            foreach (var cost in costs)
            {
                if (cost > DataStorage.mGameStatistics[isPlayer][i]*percentage)
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        internal static bool HasEnoughForUpgrade(ObjectType building, int level, bool isPlayer = true)
        {
            var costs = sUpgradeCosts[(building, level)];
            ResourceType i = 0;
            foreach (var cost in costs)
            {
                if (cost > DataStorage.mGameStatistics[isPlayer][i])
                {
                    return false;
                }
                i++;
            }
            return true;
        }

        internal static string CostToText(ObjectType building)
        {
            var costs = sBuildingCosts[building];
            var text = "Cost: ";
            ResourceType i = 0;
            foreach (var cost in costs)
            {
                if (cost == 0)
                {
                    i++;
                    continue;
                }
                text += cost + " " + i + ", ";
                i++;
            }
            text = text.Remove(text.Length - 2, 2);
            return text;
        }

        internal static string UpgradeCostToText(ObjectType building, int level)
        {
            var costs = sUpgradeCosts[(building, level)];
            var text = "Cost: ";
            ResourceType i = 0;
            foreach (var cost in costs)
            {
                if (cost == 0)
                {
                    i++;
                    continue;
                }
                text += cost + " " + i + ", ";
                i++;
            }
            text = text.Remove(text.Length - 2, 2);
            return text;
        }
    }
}