#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.Data;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using LOB.Classes.Screen.Buttons;
using Microsoft.Xna.Framework;


namespace LOB.Classes.Screen
{
    // Used to prepare and get all ScreenObjects and IButtonEffects used for the Game, without needing an instance of ScreenObjectFactory.
    /// <summary>
    /// The <see cref="ScreenObjectFactory"/> contains methods that return predefined <see cref="ScreenObject"/>s.
    /// </summary>
    internal sealed class ScreenObjectFactory
    {
        // Used to add a big enough background for the resource bar (Rectangle of HUDMenu)
        public int mScreenWidth;
        private const int LiterallyJust2 = 2;
        private const int LiterallyJust4 = 4;
        internal string mPlayerName = string.Empty;

        public ScreenObject GetGodModeScreen()
        {
            GameObjectManagement.mCurrentSelectedUnit = 0;
            GameObjectManagement.mSelectedObjects = new List<int>();
            var buttons = GetStandardButtons("GOD MODE");
            buttons.AddRange(new List<Button>
            {
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValue34Ths-GetValue4Ths/8+GetValue4Ths/LiterallyJust2, GetValue34Ths, GetValue4Ths/2), "Resources", false, true), 
                    new List<IButtonEffect>{new OpenPopUpScreenEffect(GetGodResourceScreen()) }), 
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValueHalf-GetValue4Ths/4-GetValue4Ths/8+GetValue4Ths/LiterallyJust2, GetValue34Ths, GetValue4Ths/2), "Buildings", false, true),
                    new List<IButtonEffect>{new BuildingGodModeEffect(this, true)}),
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValue4Ths-GetValue4Ths/2-GetValue4Ths/8+GetValue4Ths/LiterallyJust2, GetValue34Ths, GetValue4Ths/2), "Units", false, true),
                    new List<IButtonEffect>{new BuildingGodModeEffect(this, false)})
            });
            return new ScreenObject(
                buttons,
                true,
                true,
                false,
                false,
                1,
                new Rectangle(0, -1, GetValueMax, GetValueMax));
        }

        private ScreenObject GetGodResourceScreen()
        {
            var buttons = new List<Button>
            {
                new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValue34Ths+GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "Back", false, true),
                    new List<IButtonEffect> { new BackToGodModeBuildingScreen(this)}),
                new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2 + GetValue34Ths, -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "X", false, true),
                    new List<IButtonEffect> { new GoBackEffect() })
            };
            buttons.AddRange(new List<Button>
            {
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValue34Ths-GetValue4Ths/8, GetValue34Ths, GetValue4Ths/2), "Wood", false, true),
                    new List<IButtonEffect>{new AddGodlyResources(ResourceType.Wood, 5000)}),
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValueHalf-GetValue4Ths/4-GetValue4Ths/8, GetValue34Ths, GetValue4Ths/2), "Iron", false, true),
                    new List<IButtonEffect>{new AddGodlyResources(ResourceType.Iron, 5000)}),
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValue4Ths-GetValue4Ths/2-GetValue4Ths/8, GetValue34Ths, GetValue4Ths/2), "Gold", false, true),
                    new List<IButtonEffect>{new AddGodlyResources(ResourceType.Gold, 5000)}),
                new Button(new ButtonData(new Rectangle(GetValue4Ths/2, -GetValue4Ths+GetValue4Ths/4-GetValue4Ths/8, GetValue34Ths, GetValue4Ths/2), "Mana", false, true),
                    new List<IButtonEffect>{new AddGodlyResources(ResourceType.Mana, 5000)})
            });
            return new ScreenObject(
                buttons,
                true,
                true,
                false,
                false,
                1,
                new Rectangle(0, -1, GetValueMax, GetValueMax));
        }

        public ScreenObject GetEndGameScreen(bool won)
        {
            var buttons = new List<Button>
            {
                new Button(new ButtonData(new Rectangle(0, -GetValue4Ths/4, GetValueHalf+GetValue4Ths/LiterallyJust4+GetValue4Ths/8, GetValue4Ths / LiterallyJust2), won ? "Victory!" : "Defeat..."),
                    new List<IButtonEffect>()),
                new Button(new ButtonData(new Rectangle(0, GetValue4Ths/LiterallyJust2+GetValue4Ths/4, GetValueHalf, GetValue4Ths / LiterallyJust2), "Time: "+ScreenManager.GetTimeString()),
                    new List<IButtonEffect>()),
                new Button(new ButtonData(new Rectangle(0, 2*(GetValue4Ths / LiterallyJust2 + GetValue4Ths / 4), GetValueHalf, GetValue4Ths / LiterallyJust2), "Kills: "+DataStorage.mGameStatistics[true][ResourceType.KilledEnemies]),
                    new List<IButtonEffect>()),
                new Button(new ButtonData(new Rectangle(0, 3*(GetValue4Ths / LiterallyJust2 + GetValue4Ths / 4), GetValueHalf, GetValue4Ths / LiterallyJust2), "Losses: "+DataStorage.mGameStatistics[true][ResourceType.LostUnits]),
                    new List<IButtonEffect>()),
                new Button(new ButtonData(new Rectangle(0, GetValueMax-GetValue4Ths+GetValue4Ths/LiterallyJust4, GetValueHalf+GetValue4Ths/LiterallyJust4+GetValue4Ths/8, GetValue4Ths / LiterallyJust2), "Back to Menu"),
                new List<IButtonEffect> { new GoToMainMenuEffect() })
            };
            return new ScreenObject(buttons, false, false, true, true, 0, new Rectangle(0, 0, GetValue34Ths, GetValueMax));
        }

        public ScreenObject GetMainMenu()
        {

            return new ScreenObject(
                new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(0, 0, GetValueHalf, GetValue4Ths / LiterallyJust2), "New Game"),
                        new List<IButtonEffect> { new OpenNewGameScreenEffect(this) }),
                    new Button(new ButtonData(new Rectangle(0, GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Load Game"),
                        new List<IButtonEffect> { new OpenLoadScreenEffect(this) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueHalf, GetValueHalf, GetValue4Ths / LiterallyJust2), "Settings"),
                        new List<IButtonEffect> { new OpenDelayedMenu(GetSettingsMenu) }),
                    new Button(new ButtonData(new Rectangle(0, GetValue34Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Achievements"),
                        new List<IButtonEffect> { new OpenDelayedMenu(GetAchievementMenu) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax, GetValueHalf, GetValue4Ths / LiterallyJust2), "Exit"),
                        new List<IButtonEffect> { new GoBackEffect() })
                },
                true,
                false,
                true,
                true,
                0,
                new Rectangle(0, 0, GetValue34Ths, GetValueMax + GetValue4Ths + GetValue4Ths/2));
        }

        private List<Button> GetAchievements()
        {
            var buttons = new List<Button>{
                new Button(new ButtonData(new Rectangle(0, 5 * (GetValue4Ths + GetValue4Ths / 4), GetValueHalf, GetValue4Ths / LiterallyJust2), "Return"),
                new List<IButtonEffect> { new GoBackEffect() })};
            foreach (var ((achievementType, required), (name, text)) in Achievements.sText)
            {
                var amount = Achievements.sAchievements[achievementType];
                Point position;
                switch (achievementType)
                {
                    case Achievements.AchievementType.WonGames:
                        position = required switch
                        {
                            1 => new Point(-GetValueMax-GetValue4Ths, 0),
                            10 => new Point(0, 0),
                            100 => new Point(GetValueMax+GetValue4Ths, 0),
                            _ => new Point()
                        };
                        buttons.Add(new Button(new ButtonData(new Rectangle(position.X, position.Y, GetValueMax, GetValue4Ths), name + "\n" + text + " " + Math.Min(amount, required) + "/" + required), new List<IButtonEffect>(), true, amount >= required ? new []{Button.sLighterGreen, Button.sDarkerGreen} : null));
                        break;
                    case Achievements.AchievementType.BuiltWalls:
                        buttons.Add(new Button(new ButtonData(new Rectangle(GetValueMax + GetValue4Ths, GetValue4Ths+ GetValue4Ths/4, GetValueMax, GetValue4Ths), name + "\n" + text + " " + Math.Min(amount, required) + "/" + required), new List<IButtonEffect>(), true, amount >= required ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.Built10Walls:
                        buttons.Add(new Button(new ButtonData(new Rectangle(-GetValueMax - GetValue4Ths, GetValue4Ths + GetValue4Ths / 4, GetValueMax, GetValue4Ths), name + "\n" + text + " " + (amount == 1 ? "Completed" : "Not Completed")), new List<IButtonEffect>(), true, amount == 1 ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.Built100Walls:
                         buttons.Add(new Button(new ButtonData(new Rectangle(0, GetValue4Ths + GetValue4Ths / 4, GetValueMax, GetValue4Ths), name + "\n" + text + " " + (amount == 1 ? "Completed" : "Not Completed")), new List<IButtonEffect>(), true, amount == 1 ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.KilledEnemies:
                        buttons.Add(new Button(new ButtonData(new Rectangle(GetValueMax + GetValue4Ths, LiterallyJust2 * (GetValue4Ths + GetValue4Ths / 4), GetValueMax, GetValue4Ths), name + "\n" + text + " " + Math.Min(amount, required) + "/" + required), new List<IButtonEffect>(), true, amount >= required ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.KilledEnemiesMax:
                        position = required switch
                        {
                            10 => new Point(-GetValueMax - GetValue4Ths, LiterallyJust2* (GetValue4Ths + GetValue4Ths / 4)),
                            100 => new Point(0, LiterallyJust2 * (GetValue4Ths + GetValue4Ths / 4)),
                            _ => new Point()
                        };
                        buttons.Add(new Button(new ButtonData(new Rectangle(position.X, position.Y, GetValueMax, GetValue4Ths), name + "\n" + text + " " + (amount >= required ? "Completed" : "Not Completed")), new List<IButtonEffect>(), true, amount >= required ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.GoldMaxGotten:
                        position = required switch
                        {
                            100 => new Point(-GetValueMax - GetValue4Ths, 3 * (GetValue4Ths + GetValue4Ths / 4)),
                            500 => new Point(0, 3 * (GetValue4Ths + GetValue4Ths / 4)),
                            1000 => new Point(GetValueMax + GetValue4Ths, 3 * (GetValue4Ths + GetValue4Ths / 4)),
                            _ => new Point()
                        };
                        buttons.Add(new Button(new ButtonData(new Rectangle(position.X, position.Y, GetValueMax, GetValue4Ths), name + "\n" + text + " " + (amount >= required ? "Completed" : "Not Completed")), new List<IButtonEffect>(), true, amount >= required ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.OpenedPortal:
                        buttons.Add(new Button(new ButtonData(new Rectangle(0, LiterallyJust4 * (GetValue4Ths + GetValue4Ths / 4), GetValueMax, GetValue4Ths), name + "\n" + text + " " + (amount >= 1 ? "Completed" : "Not Completed")), new List<IButtonEffect>(), true, amount >= 1 ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : null));
                        break;
                    case Achievements.AchievementType.Gold:
                    case Achievements.AchievementType.AllTimeSeconds:
                    case Achievements.AchievementType.BuiltWallsInAGame:
                    case Achievements.AchievementType.KilledEnemiesInAGame:
                    case Achievements.AchievementType.WoodInAGame:
                    case Achievements.AchievementType.IronInAGame:
                    case Achievements.AchievementType.GoldInAGame:
                    case Achievements.AchievementType.ManaInAGame:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            buttons.Add(new Button(new ButtonData(new Rectangle(-GetValueMax - GetValue4Ths, LiterallyJust4 * (GetValue4Ths + GetValue4Ths / 4), GetValueMax, GetValue4Ths), ""), new List<IButtonEffect>(), false));
            buttons[^1].mData.SetChangingText(ScreenManager.GetPlayTimeTotalString);
            return buttons;
        }

        private ScreenObject GetAchievementMenu()
        {
            return new ScreenObject(GetAchievements(), false, false, true, true, 0, 
                new Rectangle(0, 0, 3*(GetValueMax + GetValue4Ths)-GetValue4Ths/2, 5 * (GetValue4Ths + GetValue4Ths / 4) + GetValue4Ths));
        }

        private ScreenObject GetStatisticsMenu()
        {
            var buttons = new List<Button>();
            var i = 0;
            
            foreach (var (achievementType, value) in Achievements.sAchievements)
            {
                if (achievementType < Achievements.AchievementType.BuiltWallsInAGame)
                {
                    continue;
                }
                var x = GetValue34Ths / LiterallyJust2 + GetValue4Ths / LiterallyJust4;
                var j = i % 4;
                if (achievementType >= Achievements.AchievementType.WoodInAGame)
                {
                    x = -x;
                }
                var line = achievementType.ToString().Replace("InAGame", string.Empty).Replace("E", " E").Replace("tW", "t W") + ": " + value;
                buttons.Add(new Button(new ButtonData(new Rectangle(x, j * GetValue4Ths, GetValue34Ths, GetValue4Ths / LiterallyJust2), line), new List<IButtonEffect>()));
                i++;
            }
            
            buttons.Add(new Button(new ButtonData(new Rectangle(GetValue34Ths / LiterallyJust2 + GetValue4Ths / LiterallyJust4, (i % 4) * GetValue4Ths, GetValue34Ths, GetValue4Ths/LiterallyJust2), "Lost Units: " + DataStorage.mGameStatistics[true][ResourceType.LostUnits]), new List<IButtonEffect>()));
            i++;
            buttons.Add(new Button(new ButtonData(new Rectangle(GetValue34Ths / LiterallyJust2 + GetValue4Ths / LiterallyJust4, (i % 4) * GetValue4Ths, GetValue34Ths, GetValue4Ths / LiterallyJust2), "Back"), new List<IButtonEffect>{new GoBackEffect()}));
            return new ScreenObject(buttons, false, false, true, true, 0,
                new Rectangle(0, 0, GetValueMax*LiterallyJust2- GetValue4Ths / LiterallyJust4, GetValueMax+GetValue4Ths/LiterallyJust2));
        }

        private int mWidth;
        private int mHeight;
        public ScreenObject GetNewGameMenu(int width = -1, int height = -1)
        {
            width = mWidth = width == -1 ? mWidth : width;
            height = mHeight = height == -1 ? mHeight : height;
            if (ScreenManager.mCursorPosition > mPlayerName.Length)
            {
                ScreenManager.mCursorPosition = 0;
            }
            return new ScreenObject(
                new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(-(GetValueHalf-GetValue4Ths/LiterallyJust2), 0, GetValueHalf-GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "Your Name:"+(ScreenManager.mIsChoosingName ? "\n(Press 'Enter' to exit,\nor click here)" : string.Empty)),
                        new List<IButtonEffect>{ new ChangeByAmountEffect(this, width, height, true, 0)}, false),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, 0, GetValue34Ths-GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), mPlayerName.Insert(ScreenManager.mCursorPosition, ScreenManager.mIsChoosingName ? "|" : string.Empty)),
                        new List<IButtonEffect>{new ChooseNameEffect()}, backgroundColor: ScreenManager.mIsChoosingName ? new[] { Color.DarkOrange, Color.Orange } : null),
                    new Button(new ButtonData(new Rectangle(0, GetValue4Ths/LiterallyJust2, GetValueHalf, GetValue4Ths/LiterallyJust2), "Your race"),
                        new List<IButtonEffect>(),
                        false),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths, GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Human"),
                        new List<IButtonEffect> { new ChangePlayerRaceEffect(Game1.Race.Human) }, backgroundColor: Game1.mPlayerRace != Game1.Race.Human ? null : new[] { Color.DarkOrange, Color.Orange }),
                    new Button(new ButtonData(new Rectangle(0, GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Dwarf"),
                        new List<IButtonEffect> { new ChangePlayerRaceEffect(Game1.Race.Dwarf) }, backgroundColor: Game1.mPlayerRace != Game1.Race.Dwarf ? null : new[] { Color.DarkOrange, Color.Orange }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths, GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Orc"),
                        new List<IButtonEffect> { new ChangePlayerRaceEffect(Game1.Race.Orc) }, backgroundColor: Game1.mPlayerRace != Game1.Race.Orc ? null : new[] { Color.DarkOrange, Color.Orange }),
                    new Button(new ButtonData(new Rectangle(0, GetValueHalf - GetValue4Ths / LiterallyJust2, GetValueHalf, GetValue4Ths / LiterallyJust2), "The Enemy's race"),
                        new List<IButtonEffect>(),
                        false),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths, GetValueHalf, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Human"),
                        new List<IButtonEffect> { new ChangeEnemyRaceEffect(Game1.Race.Human) }, backgroundColor: Game1.mEnemyRace != Game1.Race.Human ? null : new[] { Color.DarkOrange, Color.Orange }),
                    new Button(new ButtonData(new Rectangle(0, GetValueHalf, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Dwarf"),
                        new List<IButtonEffect> { new ChangeEnemyRaceEffect(Game1.Race.Dwarf) }, backgroundColor: Game1.mEnemyRace != Game1.Race.Dwarf ? null : new[] { Color.DarkOrange, Color.Orange }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths, GetValueHalf, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Orc"),
                        new List<IButtonEffect> { new ChangeEnemyRaceEffect(Game1.Race.Orc) }, backgroundColor: Game1.mEnemyRace != Game1.Race.Orc ? null : new[] { Color.DarkOrange, Color.Orange }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths+GetValue4Ths/LiterallyJust4, GetValue34Ths, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "+5"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, width, height, true, 5) }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths-GetValue4Ths/LiterallyJust4, GetValue34Ths, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "+1"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, width, height, true, 1) }),
                    new Button(new ButtonData(new Rectangle(0, GetValue34Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Width: "+width),
                        new List<IButtonEffect>()),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths+GetValue4Ths/LiterallyJust4, GetValue34Ths, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "-1"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, width, height, true, -1) }),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths-GetValue4Ths/LiterallyJust4, GetValue34Ths, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "-5"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, width, height, true, -5) }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths+GetValue4Ths/LiterallyJust4, GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "+5"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, height, width, false, 5) }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths-GetValue4Ths/LiterallyJust4, GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "+1"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, height, width, false, 1) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Height: "+height),
                        new List<IButtonEffect>()),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths+GetValue4Ths/LiterallyJust4, GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "-1"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, height, width, false, -1) }),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths-GetValue4Ths/LiterallyJust4, GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "-5"),
                        new List<IButtonEffect> { new ChangeByAmountEffect(this, height, width, false, -5) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax+GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Start Game"),
                        new List<IButtonEffect> { new LoadGameEffect("ErsteKarte", width, height)}),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax+GetValueHalf, GetValueHalf, GetValue4Ths / LiterallyJust2), "Back"),
                        new List<IButtonEffect> { new GoBackEffect() })
                },
                false,
                false,
                true,
                true,
                0,
                new Rectangle(0, 0, GetValueMax + GetValue4Ths, GetValueMax*2-GetValue4Ths/2));
        }

        private ScreenObject GetLoadGameMenu()
        {
            var names = ContentIo.GetNames();
            var loadButtons = new List<Button>();

            var i = 0;
            foreach (var (name, time) in names)
            {
                loadButtons.Add(new Button(new ButtonData(new Rectangle(0, i * GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Load " +
                    (name == null ? "new standard Game" : name + "\nTime: " + ScreenManager.GetTimeString(time))), name == null ? new List<IButtonEffect> { new LoadGameEffect("ErsteKarte", 90, 50)} : new List<IButtonEffect> { new LoadSavedGameEffect(i + 1, (name, time))}));
                i++;
            }

            if (i == 0)
            {
                loadButtons.Add(new Button(new ButtonData(new Rectangle(0, 0, GetValueHalf, GetValue4Ths / LiterallyJust2), "No Saved Games"),
                    new List<IButtonEffect> (), false));
                i++;
            }

            loadButtons.Add(new Button(new ButtonData(new Rectangle(0, i * GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Back"),
                new List<IButtonEffect> { new GoBackEffect() }));

            return new ScreenObject(
                loadButtons,
                false,
                false,
                true,
                true,
                0,
                new Rectangle(0, 0, GetValue34Ths, GetValue4Ths*(i+1) + GetValue4Ths / 2));
        }

        public ScreenObject GetHudMenu()
        {
            return new ScreenObject(
                new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(-GetValueHalf, 0, GetValueHalf, GetValue4Ths / LiterallyJust2), "Pause", true),
                        new List<IButtonEffect> { new OpenScreenEffect(GetPauseMenu()) }),
                    new Button(new ButtonData(new Rectangle(-GetValueMax, 0, GetValueHalf, GetValue4Ths / LiterallyJust2), "Construction", true),
                        new List<IButtonEffect> { new BuildingSelectionEffect() }),
                    new Button(new ButtonData(new Rectangle(-GetValueMax, 0, GetValueMax+GetValue4Ths, GetValue4Ths+GetValue4Ths/LiterallyJust2), "", true),
                        new List<IButtonEffect>(), false) // Blocks the mouse from moving at the top corner
                },
                true,
                true,
                false,
                false,
                -3, 
                new Rectangle(0, 0, mScreenWidth, GetValue4Ths / LiterallyJust2));
        }

        public ScreenObject GetPauseMenu()
        {
            return new ScreenObject(
                new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(0, 0, GetValueHalf, GetValue4Ths / LiterallyJust2), "Continue"),
                        new List<IButtonEffect> { new GoBackEffect() }),
                    new Button(new ButtonData(new Rectangle(0, GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Settings"),
                        new List<IButtonEffect> { new OpenDelayedMenu(GetSettingsMenu) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueHalf, GetValueHalf, GetValue4Ths / LiterallyJust2), "Statistics"), 
                        new List<IButtonEffect> { new OpenDelayedMenu(GetStatisticsMenu) }),
                    new Button(new ButtonData(new Rectangle(0, GetValue34Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Achievements"),
                        new List<IButtonEffect> { new OpenDelayedMenu(GetAchievementMenu) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax, GetValueHalf, GetValue4Ths / LiterallyJust2), "Save"), 
                        new List<IButtonEffect>{new OpenDelayedMenu(GetSaveMenu) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax+GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Load"), 
                        new List<IButtonEffect>{new OpenLoadScreenEffect(this)}),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax+GetValueHalf, GetValueHalf, GetValue4Ths / LiterallyJust2), "Back to Menu"),
                        new List<IButtonEffect> { new GoToMainMenuEffect() })
                },
                true,
                false,
                true,
                true,
                0,
                new Rectangle(0, 0, GetValue34Ths, GetValueMax*LiterallyJust2 - GetValue4Ths/LiterallyJust2));
        }

        private ScreenObject GetSettingsMenu()
        {
            return new ScreenObject(
                new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(0, 0, GetValue34Ths, GetValue4Ths / LiterallyJust2), "Bordered Mode Switch (F11)"),
                        new List<IButtonEffect> { new BorderModeEffect() }),
                    new Button(new ButtonData(new Rectangle(0, GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Volume"),
                        new List<IButtonEffect>(),
                        false),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths, GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "+"),
                        new List<IButtonEffect> { new ChangeVolumeEffect(.1f, false) }),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths, GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "-"),
                        new List<IButtonEffect> { new ChangeVolumeEffect(-.1f, false) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueHalf, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Effects\nVolume"),
                        new List<IButtonEffect>(),
                        false),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths, GetValueHalf, GetValue4Ths, GetValue4Ths / LiterallyJust2), "+"),
                        new List<IButtonEffect> { new ChangeVolumeEffect(.1f, true) }),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths, GetValueHalf, GetValue4Ths, GetValue4Ths / LiterallyJust2), "-"),
                        new List<IButtonEffect> { new ChangeVolumeEffect(-.1f, true) }),
                    new Button(new ButtonData(new Rectangle(0, GetValue34Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "HUD Size\n"+(int)Math.Round(Renderer.mHudScale*100)+"%"),
                        new List<IButtonEffect>(),
                        false),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths, GetValue34Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "+5%"),
                        new List<IButtonEffect> { new ChangeScaleEffect(.05f, this) }),
                    new Button(new ButtonData(new Rectangle(-GetValue4Ths, GetValue34Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "-5%"),
                        new List<IButtonEffect> { new ChangeScaleEffect(-.05f, this) }),
                    new Button(new ButtonData(new Rectangle(0, GetValueMax, GetValueHalf, GetValue4Ths / LiterallyJust2), "Back"),
                        new List<IButtonEffect> { new SaveSettingsEffect() })
                },
                false,
                false,
                true,
                true,
                0,
                new Rectangle(0, 0, GetValueMax, GetValueMax + GetValueHalf - GetValue4Ths/LiterallyJust2));
        }

        private ScreenObject GetSaveMenu()
        {
            var names = ContentIo.GetNames();
            // Re sharper makes no sense
            // ReSharper disable PossibleMultipleEnumeration
            if (!names.Any()) // For first time without opening any other save or load menus
            {
                names = ContentIo.GetNames();
            }
            var saveButtons = new List<Button>();

            var i = 0;
            foreach (var (name, time) in names)
            {
                saveButtons.Add(new Button(new ButtonData(new Rectangle(0, i* GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Save " +
                    (name == null ? "in empty Slot" : "over " + name + "\nTime: " + ScreenManager.GetTimeString(time))), new List<IButtonEffect> { new SaveGameEffect(i+1) }));
                i++;
            }

            saveButtons.Add(new Button(new ButtonData(new Rectangle(0, i * GetValue4Ths, GetValueHalf, GetValue4Ths / LiterallyJust2), "Back"),
                new List<IButtonEffect> { new GoBackEffect() }));

            return new ScreenObject(
                saveButtons,
                false,
                false,
                true,
                true,
                0,
                new Rectangle(0, 0, GetValue34Ths, GetValue4Ths * (i + 1) + GetValue4Ths / 2));
        }
        
        /// <returns>Returns flavor text, dependent on which <see cref="GameObjectManagement.mSelectedObjects"/> is the <see cref="GameObjectManagement.mCurrentSelectedUnit"/></returns>
        private string GetObjectText()
        {
            return DataStorage.GetObject(GameObjectManagement.mSelectedObjects[GameObjectManagement.mCurrentSelectedUnit]).mName switch
            {
                var x when
                    x == ObjectType.Gold1Vein ||
                    x == ObjectType.Iron1Source ||
                    x == ObjectType.Mana1Source => x.ToString().Replace("1", " ") + "s produce " + x.ToString().Split("1")[0] + ".\nUse " + (Game1.mPlayerRace == Game1.Race.Orc ? "Trolls to farm resources" : "Builders to build mines."),
                ObjectType.Barrier => "The Barrier.\nYou will have to open a portal if you ever hope to cross it.",
                ObjectType.Tree => "Trees produce Wood.\nUse " + (Game1.mPlayerRace == Game1.Race.Orc ? "Trolls" : "Builders") + " to farm resources.",
                ObjectType.Rock => "A nuisance.\nDoesn't do anything but stand in your way.",
                ObjectType.Knight => "The standard human unit.\nA proud fighter in your army.",
                ObjectType.Archer => "The Archer can attack from further away,\nbut will go down a lot faster when enemies get too close.",
                ObjectType.Horseman => "While also short ranged,\nthe Horseman can run through enemy troops\nwith its speed and annihilate slower and weaker units.",
                ObjectType.Mage => "Can use magic to shield or heal your units.\nThe mage is the source of your Portals,\nas some of them work in the mage towers",
                ObjectType.Puncher => "A primitive fighter, only using a club.\nThey might be weak,\nbut their strength lies in numbers and speed.",
                ObjectType.Slingshot => "A primitive long range unit. Better not get hit.",
                ObjectType.Shaman => "The Shaman uses the great magical potential of the orcs,\n but it's blatant destruction might and will backfire.",
                ObjectType.Axeman => "A mighty warrior, using an axe they can throw.",
                ObjectType.Arbalist => "A unit with extreme range,\nthanks to superior dwarven technology.",
                ObjectType.Phalanx => "The strongest of short range fighters.\nThese units can stand in formation with others of their\nkind to barricade on the spot and gain more health.\nIn this mode, they are unable to move.",
                ObjectType.Wolf1Rider => "This warrior moves at high speed,\n fighting with both their axe and their wolf.",
                ObjectType.Dwarf1Hero => "His power and tactical prowess make\nthe dwarfs around him stronger.",
                ObjectType.Human1Hero => "The productivity of the Human Empire\nis known throughout the lands.\nHe can upgrade a mine for free every " + DataStorage.mGameObjects.Values.First(gameObject => gameObject.mName == ObjectType.Human1Hero).mActions.OfType<SpecialAction>().First().GetMaxCooldown() + " seconds.",
                ObjectType.Orc1Hero => "The ork with the highest magical ability.\nHe can raise a portal on his own,\nbut it still costs mana crystals.",
                _ => "" +
                     "Not found" +
                     "" +
                     "" +
                     ""
            };
        }

        private const int GetValueMax = 400;

        private const int GetValue34Ths = 300;

        private const int GetValueHalf = 200;

        private const int GetValue4Ths = 100;

        private Rectangle GetStandardRectangle()
        {
            return new Rectangle(0, -1, GetValueMax, GetValueMax);
        }

        public Button GetBuildModeButton(string text)
        {
            return new Button(new ButtonData(new Rectangle(0, -GetValueMax-GetValue4Ths/LiterallyJust2, GetValueMax+GetValue4Ths+GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), text, false, true), new List<IButtonEffect>(), true, new[] { Button.sLighterGreen, Button.sDarkerGreen });
        }

        public Button GetBuildModeButtonBig(string text)
        {
            return new Button(new ButtonData(new Rectangle(0, -GetValueMax - GetValue4Ths / LiterallyJust2, GetValueMax + GetValueHalf + GetValue4Ths / LiterallyJust2, GetValue4Ths / LiterallyJust2), text, false, true), new List<IButtonEffect>(), true, new[] { Button.sLighterGreen, Button.sDarkerGreen });
        }

        public Button GetFarmModeButton(string text)
        {
            return new Button(new ButtonData(new Rectangle(0, -GetValueMax - GetValue4Ths / LiterallyJust2, GetValueMax, GetValue4Ths / LiterallyJust2), text, false, true), new List<IButtonEffect>(), true, new[] { Button.sLighterGreen, Button.sDarkerGreen });
        }

        private IEnumerable<Button> GetStandardButtons(ObjectType objectName)
        {
            return GetStandardButtons(objectName.ToString().Replace("1", " "));
        }

        private List<Button> GetStandardButtons(string text)
        {
            return new List<Button>
            {
                new Button(new ButtonData(new Rectangle(0, -GetValueMax, (GetValueMax - GetValue4Ths / LiterallyJust2), GetValue4Ths/LiterallyJust2),
                        text,
                        false,
                        true),
                    new List<IButtonEffect>()),
                new Button(new ButtonData(new Rectangle((GetValueMax - GetValue4Ths / LiterallyJust2), -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "X", false, true),
                    new List<IButtonEffect> { new GoBackEffect() })
            };
        }

        private List<Button> GetStandardTextButton(ObjectType objectName)
        {
            var standardTextButtons = new List<Button>{new Button(new ButtonData(new Rectangle(0, -(GetValueMax - GetValue4Ths / LiterallyJust2), GetValueMax, (GetValueMax - GetValue4Ths / LiterallyJust2)),
                    GetObjectText(),
                    false,
                    true),
                new List<IButtonEffect>(),
                false)};
            if (objectName == ObjectType.New1Building)
            {
                standardTextButtons[0] = new Button(new ButtonData(new Rectangle(0, -(GetValueMax - GetValue4Ths / LiterallyJust2), GetValueMax, GetValueMax - GetValue4Ths),
                    GetObjectText(),
                    false,
                    true),
                    new List<IButtonEffect>(),
                    false);
                standardTextButtons.Add(new Button(new ButtonData(
                        new Rectangle(GetValue4Ths / LiterallyJust2,
                            -GetValue4Ths / LiterallyJust2,
                            GetValueHalf + GetValue4Ths,
                            GetValue4Ths / LiterallyJust2),
                        "Remove unfinished Building",
                        false,
                        true),
                    new List<IButtonEffect> { new RemoveBuildingEffect() }));
            }
            standardTextButtons.AddRange(GetStandardButtons(objectName));
            return standardTextButtons;
        }

        private ScreenObject GetStandardScreenObject(ObjectType objectName)
        {
            return new ScreenObject(
                GetStandardTextButton(objectName),
                true,
                true,
                false,
                false,
                1,
                GetStandardRectangle()
            );
        }
        
        public ScreenObject OpenErrorScreen(string errorMessage)
        {
            var buttons = new List<Button>
            {
                new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueMax, GetValue4Ths/LiterallyJust2), "Return", false, true),
                    new List<IButtonEffect> { new GoBackErrorEffect() }),
                new Button(new ButtonData(new Rectangle(0, -GetValueMax+GetValue4Ths/LiterallyJust2, GetValueMax, GetValueMax-GetValue4Ths/LiterallyJust2),
                        errorMessage,
                        false,
                        true),
                    new List<IButtonEffect>(),
                    false),
            };
            return new ScreenObject(buttons,
                drawLower: true,
                updateLower: true,
                drawHorizontalCentered: false,
                drawVerticalCentered: false,
                screenId: 1, GetStandardRectangle());
        }

        private ScreenObject GetStandardUnitScreenObject((ObjectType, Dictionary<string, int>) objectAttributes)
        {
            var objectType = objectAttributes.Item1;
            return new ScreenObject(
                new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(GetValueHalf+GetValue4Ths/LiterallyJust2, -GetValueMax, GetValue4Ths, GetValue4Ths/LiterallyJust2), "Stats", false, true),
                        new List<IButtonEffect>
                        {
                            new OpenPopUpScreenEffect(
                                new ScreenObject(GetStats(objectAttributes),
                                    true,
                                    true,
                                    false,
                                    false,
                                    1,
                                    GetStandardRectangle()))
                        }),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -GetValueMax, (GetValue4Ths / LiterallyJust2 + GetValue4Ths), GetValue4Ths/LiterallyJust2),
                            objectType.ToString().Replace("1", " "),
                            false,
                            true),
                        new List<IButtonEffect>()),
                    new Button(new ButtonData(new Rectangle((GetValueMax - GetValue4Ths / LiterallyJust2), -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "X", false, true),
                        new List<IButtonEffect> { new GoBackEffect() }),
                    new Button(new ButtonData(new Rectangle(GetValueHalf, -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "+", false, true),
                        new List<IButtonEffect> { new ChangeCurrentUnitEffect(1) }),
                    new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "-", false, true),
                        new List<IButtonEffect> { new ChangeCurrentUnitEffect(-1) }),
                    new Button(new ButtonData(new Rectangle(0, -(GetValueMax - GetValue4Ths / LiterallyJust2), GetValueMax, (GetValue4Ths+GetValue4Ths/LiterallyJust4)),
                            GetObjectText(),
                            false,
                            true),
                        new List<IButtonEffect>(),
                        false),
                    new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValueHalf+GetValue4Ths/LiterallyJust4), GetValue34Ths, GetValue4Ths/LiterallyJust2+GetValue4Ths),
                            "Stop current action",
                            false,
                            true),
                        new List<IButtonEffect> { new StopActionsEffect() })
                },
                true,
                true,
                false,
                false,
                1,
                GetStandardRectangle());
        }

        private ScreenObject GetStandardHouseScreenObject((ObjectType, Dictionary<string, int>) objectAttributes)
        {
            var objectType = objectAttributes.Item1;
            var buttons = new List<Button>();
            if (objectType > ObjectType.Main1Building && objectType < ObjectType.New1Building && objectType != ObjectType.Mine)
            {
                buttons.Add(new Button(new ButtonData(
                        new Rectangle(GetValue4Ths / LiterallyJust2,
                            -GetValue4Ths / LiterallyJust2,
                            GetValueHalf + GetValue4Ths,
                            GetValue4Ths / LiterallyJust2),
                        "Remove " + objectType.ToString().ToLower().Replace("1", " "),
                        false,
                        true),
                    new List<IButtonEffect> { new GetRemoveBuildingEffect(this) }));
            }
            buttons.AddRange(new List<Button>
            {
                new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueHalf+GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2),
                        objectType.ToString().Replace("1", " "),
                        false,
                        true),
                    new List<IButtonEffect>()),
                new Button(new ButtonData(new Rectangle(GetValueHalf+GetValue4Ths/LiterallyJust2, -GetValueMax, GetValue4Ths, GetValue4Ths/LiterallyJust2), "Stats", false, true),
                    new List<IButtonEffect>
                    {
                        new OpenPopUpScreenEffect(
                            new ScreenObject(GetStats(objectAttributes),
                                true,
                                true,
                                false,
                                false,
                                1,
                                GetStandardRectangle()))
                    }),
                new Button(new ButtonData(new Rectangle((GetValueMax - GetValue4Ths / LiterallyJust2), -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "X", false, true),
                    new List<IButtonEffect> { new GoBackEffect() }),
                new Button(new ButtonData(new Rectangle(0, -(GetValueMax - GetValue4Ths / LiterallyJust2), GetValueMax, (GetValueMax - GetValue4Ths / LiterallyJust2)), "", false, true),
                    new List<IButtonEffect>(), false)});
            return new ScreenObject(buttons,
                true,
                true,
                false,
                false,
                1,
                GetStandardRectangle());
        }
            

        public ScreenObject GetPopUpMenu((ObjectType, Dictionary<string, int>) objectAttributes, int objectId)
        {
            var objectName = objectAttributes.Item1;
            if (!DataStorage.GetObject(objectId).mIsPlayer)
            {
                GetStandardScreenObject(objectName);
            }
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (objectName)
            {
                case ObjectType.New1Building:
                case ObjectType.Barrier:
                case ObjectType.Tree:
                case ObjectType.Gold1Vein:
                case ObjectType.Iron1Source:
                case ObjectType.Mana1Source:
                case ObjectType.Rock:
                    return GetStandardScreenObject(objectName);
                case ObjectType.Knight:
                case ObjectType.Archer:
                case ObjectType.Horseman:
                case ObjectType.Puncher:
                case ObjectType.Slingshot:
                case ObjectType.Axeman:
                case ObjectType.Arbalist:
                case ObjectType.Wolf1Rider:
                    return GetStandardUnitScreenObject(objectAttributes);
                case ObjectType.Shaman:
                    var shaman = DataStorage.GetObject(objectId);
                    if (shaman == null)
                    {
                        return GetSettingsMenu();
                    }
                    var shamanScreen = GetStandardUnitScreenObject(objectAttributes);
                    shamanScreen.mButtons.Remove(shamanScreen.mButtons[^1]);
                    var potion =
                        (Potion?)shaman.mActions.FirstOrDefault(action => action is Potion);
                    if (potion == default)
                    {
                        return GetSettingsMenu();
                    }
                    var shamanCooldown = potion.GetCooldownSpeed;

                    shamanScreen.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -GetValue34Ths+50, GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Stop current action",
                                false,
                                true),
                            new List<IButtonEffect> { new StopActionsEffect() }),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -GetValueHalf+25, GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Speed Potion",
                                false,
                                true),
                            new List<IButtonEffect>{new PotionEffect(DataStorage.GetObject(objectId), PotionType.Speed1Potion)}),
                        new Button(new ButtonData(
                            new ChangingRectangle(shamanObj => GetValue4Ths / LiterallyJust2
                                ,shamanObj => -(GetValueHalf - 80)
                                ,shamanObj => (int)(GetValue34Ths *
                                    ((Potion?)((GameObject)shamanObj).mActions.FirstOrDefault(action =>
                                        action is Potion)!).GetTimeToWaitSpeedPotion / shamanCooldown)
                                ,shamanObj => GetValue4Ths / LiterallyJust2 / 20, shaman), "", false, true),
                            new List<IButtonEffect>(), potion.GetTimeToWaitSpeedPotion != 0, new [] { Color.Blue, Color.LightBlue}, new [] { Color.Blue, Color.LightBlue})
                    });
                    return shamanScreen;
                case ObjectType.Mage:
                    var mage = DataStorage.GetObject(objectId);
                    if (mage == null)
                    {
                        return GetSettingsMenu();
                    }
                    var mageScreen = GetStandardUnitScreenObject(objectAttributes);
                    mageScreen.mButtons.Remove(mageScreen.mButtons[^1]);
                    var potionAction =
                        (Potion?)mage.mActions.FirstOrDefault(action => action is Potion);
                    if (potionAction == default)
                    {
                        return GetSettingsMenu();
                    }
                    var cooldownHeal = potionAction.GetCooldownHeal;
                    var cooldownDamage = potionAction.GetCooldownDamage;

                    mageScreen.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -GetValue34Ths+50, GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Stop current action",
                                false,
                                true),
                            new List<IButtonEffect> { new StopActionsEffect() }),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -GetValueHalf+25, GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Heal Potion",
                                false,
                                true),
                            new List<IButtonEffect>{new PotionEffect(DataStorage.GetObject(objectId), PotionType.Heal1Potion)}),
                        new Button(new ButtonData(
                                new ChangingRectangle(mageObj => GetValue4Ths / LiterallyJust2
                                ,mageObj => -(GetValueHalf - 80)
                                ,mageObj => (int)(GetValue34Ths *
                                    ((Potion?)((GameObject)mageObj).mActions.FirstOrDefault(action =>
                                        action is Potion)!).GetTimeToWaitHealPotion / cooldownHeal)
                                ,mageObj => GetValue4Ths / LiterallyJust2 / 20, mage), "", false, true),
                            new List<IButtonEffect>(), potionAction.GetTimeToWaitHealPotion != 0, new [] { Color.Blue, Color.LightBlue}, new [] { Color.Blue, Color.LightBlue}),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValue4Ths), GetValue34Ths, GetValue4Ths/LiterallyJust2),
                        "Damage Potion",
                        false,
                        true),
                            new List<IButtonEffect>{new PotionEffect(DataStorage.GetObject(objectId), PotionType.Damage1Potion)}),
                        new Button(new ButtonData(
                                new ChangingRectangle(mageObj => GetValue4Ths / LiterallyJust2
                                    ,mageObj => -(GetValue4Ths - 55)
                                    ,mageObj => (int)(GetValue34Ths *
                                        ((Potion?)((GameObject)mageObj).mActions.FirstOrDefault(action =>
                                            action is Potion)!).GetTimeToWaitDamagePotion / cooldownDamage)
                                    ,mageObj => GetValue4Ths / LiterallyJust2 / 20, mage), "", false, true),
                            new List<IButtonEffect>(), potionAction.GetTimeToWaitDamagePotion != 0, new [] { Color.Blue, Color.LightBlue}, new [] { Color.Blue, Color.LightBlue})
                    });
                    

                    return mageScreen;
                case ObjectType.Phalanx:
                    var phalanxScreen = GetStandardUnitScreenObject(objectAttributes);
                    phalanxScreen.mButtons.Remove(phalanxScreen.mButtons[^1]);
                    IButtonEffect phalanxEffect = new ButtonShinyEffect();
                    var phalanxObject = DataStorage.GetObject(objectId);

                    if (phalanxObject.mObjectEvents[EventType.MoveEvent].Count == 0)
                    {
                        phalanxEffect = new PhalanxEffect();
                    }

                    phalanxScreen.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValueHalf+GetValue4Ths/LiterallyJust4), GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Stop current action",
                                false,
                                true),
                            new List<IButtonEffect> { new StopActionsEffect() }),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValue4Ths), GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Stand your ground",
                                false,
                                true),
                            new List<IButtonEffect> { phalanxEffect })
                    });
                    return phalanxScreen;
                case ObjectType.Builder:
                case ObjectType.Troll:
                    var builderScreen = GetStandardUnitScreenObject(objectAttributes);
                    builderScreen.mButtons.RemoveRange(builderScreen.mButtons.Count - LiterallyJust2, LiterallyJust2);
                    builderScreen.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, (int)(-GetValueHalf-GetValue4Ths/1.5f), GetValue34Ths, GetValue4Ths/LiterallyJust2), "Farm Resource", false, true),
                            new List<IButtonEffect> { new GatherResourceEffect(objectId) }),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2,
                                (int)(-GetValueHalf + GetValue4Ths / 1.5f), GetValue34Ths, GetValue4Ths/LiterallyJust2), "Repair Building", false, true),
                            new List<IButtonEffect> { new SpecialActionEffect(DataStorage.GetObject(objectId)) })
                    });
                    return builderScreen;
                case ObjectType.Dwarf1Hero:
                    var dwarfHeroScreen = GetStandardUnitScreenObject(objectAttributes);
                    dwarfHeroScreen.mButtons.Remove(dwarfHeroScreen.mButtons[^1]);
                    dwarfHeroScreen.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValueHalf+GetValue4Ths/LiterallyJust4), GetValue34Ths, GetValue4Ths/LiterallyJust2), "Strengthens nearby allies", false, true),
                            new List<IButtonEffect>(), false),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValue4Ths+GetValue4Ths/LiterallyJust4), GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Stop current action",
                                false,
                                true),
                            new List<IButtonEffect> { new StopActionsEffect() })
                    });
                    return dwarfHeroScreen;
                case ObjectType.Human1Hero:
                case ObjectType.Orc1Hero:
                    var heroObject = DataStorage.GetObject(objectId);
                    var heroScreen = GetStandardUnitScreenObject(objectAttributes);
                    heroScreen.mButtons.Remove(heroScreen.mButtons[^1]);
                    var specialAction =
                        (SpecialAction) heroObject.mActions.First(action => action is SpecialAction);
                    var cooldownMax = specialAction.GetMaxCooldown();
                    
                    heroScreen.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValueHalf+GetValue4Ths/LiterallyJust4), GetValue34Ths, GetValue4Ths/LiterallyJust2), "Special action", false, true),
                            new List<IButtonEffect>{new SpecialActionEffect(DataStorage.GetObject(objectId))}),
                        new Button(new ButtonData(new ChangingRectangle(heroObj => GetValue4Ths / LiterallyJust2
                            ,heroObj => -(GetValueHalf+GetValue4Ths/LiterallyJust4 - 60)
                            ,heroObj => (int)(GetValue34Ths *
                                ((SpecialAction?)((GameObject)heroObj).mActions.FirstOrDefault(action =>
                                    action is SpecialAction)!).GetTimeToWait() / cooldownMax)
                            ,heroObj => GetValue4Ths / LiterallyJust2 / 20, heroObject), "", false, true),
                            new List<IButtonEffect>(), specialAction.GetTimeToWait() != 0, new [] { Color.Blue, Color.LightBlue}, new [] { Color.Blue, Color.LightBlue}),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValue4Ths+GetValue4Ths/LiterallyJust4), GetValue34Ths, GetValue4Ths/LiterallyJust2),
                                "Stop current action",
                                false,
                                true),
                            new List<IButtonEffect> { new StopActionsEffect() })
                    });
                    return heroScreen;
                case ObjectType.Main1Building:
                    List<Button> buttonsForRace = new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueHalf+GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2),
                                objectName.ToString().Replace("1", " "),
                                false,
                                true),
                            new List<IButtonEffect>()),
                        new Button(new ButtonData(new Rectangle(GetValueHalf+GetValue4Ths/LiterallyJust2, -GetValueMax, GetValue4Ths, GetValue4Ths/LiterallyJust2), "Stats", false, true),
                            new List<IButtonEffect>
                            {
                                new OpenPopUpScreenEffect(
                                    new ScreenObject(GetStats(objectAttributes),
                                        true,
                                        true,
                                        false,
                                        false,
                                        1,
                                        GetStandardRectangle()))
                            }),
                        new Button(new ButtonData(new Rectangle((GetValueMax - GetValue4Ths / LiterallyJust2), -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "X", false, true),
                            new List<IButtonEffect> { new GoBackEffect() }),
                        new Button(new ButtonData(new Rectangle(0, -(GetValueMax - GetValue4Ths / LiterallyJust2) + 15, GetValueMax, GetValue4Ths/LiterallyJust2), "Your Main building.\nIt is at the heart of your kingdom.\nIf it falls, so will you.", false, true),
                            new List<IButtonEffect>(), false),
                    };

                    var isPlayer = DataStorage.GetObject(objectId).mIsPlayer;
                    switch (Game1.mPlayerRace)
                    {
                        case Game1.Race.Human:
                            if (DataStorage.mGameStatistics[isPlayer][ResourceType.HeroAlive] == 0)
                            {
                                buttonsForRace.Add(new Button(
                                    new ButtonData(
                                        new Rectangle(GetValue4Ths / LiterallyJust2, -(GetValue4Ths / LiterallyJust2 + GetValue4Ths), GetValue34Ths, GetValue4Ths),
                                        "Revive Human Hero\nFor: " + ObjectFactory.sTypeToMoney[ObjectType.Human1Hero][ResourceType.Gold] + " Gold, "
                                        + ObjectFactory.sTypeToMoney[ObjectType.Human1Hero][ResourceType.Iron] + " Iron and "
                                        + ObjectFactory.sTypeToMoney[ObjectType.Human1Hero][ResourceType.Mana] + " Mana",
                                        false,
                                        true),
                                    new List<IButtonEffect> {new BuildUnitEffect(1, ObjectType.Human1Hero)}));
                            }
                            buttonsForRace.Add(new Button(
                                new ButtonData(new Rectangle(GetValue4Ths / LiterallyJust2,
                                    -GetValue34Ths + 30,
                                    GetValue34Ths,
                                    GetValue4Ths), "Builder", false, true),
                                new List<IButtonEffect> { new GetUnitBuyScreenEffect(1, ObjectType.Builder, 1, this) }));
                            break;
                        case Game1.Race.Dwarf:
                            if (DataStorage.mGameStatistics[isPlayer][ResourceType.HeroAlive] == 0)
                            {
                                buttonsForRace.Add(new Button(
                                    new ButtonData(
                                        new Rectangle(GetValue4Ths / LiterallyJust2,
                                            -GetValue34Ths,
                                            GetValue34Ths,
                                            GetValue4Ths),
                                        "Revive Dwarf Hero\nFor: " + ObjectFactory.sTypeToMoney[ObjectType.Human1Hero][ResourceType.Gold] + " Gold, "
                                        + ObjectFactory.sTypeToMoney[ObjectType.Dwarf1Hero][ResourceType.Iron] + " Iron and "
                                        + ObjectFactory.sTypeToMoney[ObjectType.Dwarf1Hero][ResourceType.Mana] + " Mana",
                                        false,
                                        true),
                                    new List<IButtonEffect> {new BuildUnitEffect(1, ObjectType.Dwarf1Hero)}));
                            }
                            buttonsForRace.Add(new Button(
                                new ButtonData(new Rectangle(GetValue4Ths / LiterallyJust2, -(GetValue4Ths / LiterallyJust2 + GetValue4Ths), GetValue34Ths, GetValue4Ths), "Builder", false, true),
                                new List<IButtonEffect> { new GetUnitBuyScreenEffect(1, ObjectType.Builder, 1, this) }));
                            break;
                        default:
                            if (DataStorage.mGameStatistics[isPlayer][ResourceType.HeroAlive] == 0)
                            {
                                buttonsForRace.Add(new Button(
                                    new ButtonData(new Rectangle(GetValue4Ths / LiterallyJust2,
                                            -GetValue34Ths,
                                            GetValue34Ths,
                                            GetValue4Ths),
                                        "Revive Ork Hero\nFor: " + ObjectFactory.sTypeToMoney[ObjectType.Human1Hero][ResourceType.Gold] + " Gold, "
                                        + ObjectFactory.sTypeToMoney[ObjectType.Orc1Hero][ResourceType.Iron] + " Iron and "
                                        + ObjectFactory.sTypeToMoney[ObjectType.Orc1Hero][ResourceType.Mana] + " Mana",
                                        false,
                                        true),
                                    new List<IButtonEffect> {new BuildUnitEffect(1, ObjectType.Orc1Hero)}));
                            }
                            buttonsForRace.Add(new Button(
                                new ButtonData(new Rectangle(GetValue4Ths / LiterallyJust2, -(GetValue4Ths / LiterallyJust2 + GetValue4Ths), GetValue34Ths, GetValue4Ths), "Troll", false, true),
                                new List<IButtonEffect> { new GetUnitBuyScreenEffect(1, ObjectType.Troll, 1, this) }));
                            break;
                    }
                    return new ScreenObject(buttonsForRace,
                        true,
                        true,
                        false,
                        false,
                        1,
                        GetStandardRectangle());
                case ObjectType.House:
                case ObjectType.Mine:
                case ObjectType.Wall:
                case ObjectType.Gate:
                case ObjectType.Tower:
#pragma warning disable 8509
                    var textAtLevel = objectName switch
#pragma warning restore 8509
                    {
                        ObjectType.House => objectAttributes.Item2["Level"] switch
                        {
                            1 => "A small house. Gives you " + GameObject.sHousePopulationTuple[0] + " Population.",
                            LiterallyJust2 => "A larger house. Gives you " +
                                              (GameObject.sHousePopulationTuple[0] +
                                               GameObject.sHousePopulationTuple[1]) + " Population.",
                            3 => "A giant house. Gives you " +
                                 (GameObject.sHousePopulationTuple[0] + GameObject.sHousePopulationTuple[1] +
                                  GameObject.sHousePopulationTuple[2]) + " Population.",
                            _ => "Level not found."
                        },
                        ObjectType.Mine => objectAttributes.Item2["Level"] switch
                        {
                            1 => "A simple mine.\n" + "Produces " + (ObjectFactory.sLevelToAmount[DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().mCurrentResource][DataStorage.GetObject(objectId).GetAttributes().Item2
                                .TryGetValue("Level", out var level) ? level : 1]) + " " + DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().mCurrentResource + " every " + DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().GetTimeForResource() + " seconds.",
                            LiterallyJust2 => "A deeper mine.\n" + "Produces " + (ObjectFactory.sLevelToAmount[DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().mCurrentResource][DataStorage.GetObject(objectId).GetAttributes().Item2
                                .TryGetValue("Level", out var level) ? level : 1]) + " " + DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().mCurrentResource + " every " + DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().GetTimeForResource() + " seconds.",
                            3 => "An incredible mine.\n" + "Produces " + (ObjectFactory.sLevelToAmount[DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().mCurrentResource][DataStorage.GetObject(objectId).GetAttributes().Item2
                                .TryGetValue("Level", out var level) ? level : 1]) + " " + DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().mCurrentResource + " every " + DataStorage.GetObject(objectId).mActions.OfType<GatherResource>().First().GetTimeForResource() + " seconds.",
                            _ => "Level not found."
                        },
                        ObjectType.Wall => objectAttributes.Item2["Level"] switch
                        {
                            1 => "A Wall to keep out your enemies,\nbut it blocks your path too.",
                            LiterallyJust2 => "A reinforced Wall.",
                            3 => "This wall is very good.",
                            _ => "Level not found."
                        },
                        ObjectType.Gate => objectAttributes.Item2["Level"] switch
                        {
                            1 => "Not as strong as a Wall,\nbut your units will be able to pass through.",
                            LiterallyJust2 => "A reinforced gate.",
                            3 => "This gate is very good.",
                            _ => "Level not found."
                        },
                        ObjectType.Tower => objectAttributes.Item2["Level"] switch
                        {
                            1 => "Defends your Kingdom.\n The Tower stands mightily with superior range.",
                            LiterallyJust2 => "a",
                            3 => "a",
                            _ => "Level not found."
                        }
                    };
                    var houseObject = GetStandardHouseScreenObject(objectAttributes);
                    houseObject.mButtons[^1].mData.mButtonText = textAtLevel;
                    return houseObject;
                case ObjectType.Mage1Tower:
                    var mageTower = DataStorage.GetObject(objectId);
                    var isOpen =
                        ((BuildPortal)DataStorage.GetObject(objectId).mActions.First(action => action.GetEventType == EventType.PortalEvent)).mPortalPosition != default;
                    var mageTowerObject = GetStandardHouseScreenObject(objectAttributes);
                    var portal = (BuildPortal) mageTower.mActions.First(action => action is BuildPortal);
                    mageTowerObject.mButtons.Remove(mageTowerObject.mButtons[^1]);
                    mageTowerObject.mButtons.AddRange(new List<Button>
                    {
                        new Button(new ButtonData(new Rectangle(0, -GetValue34Ths, GetValueMax, GetValue4Ths),
                                "Creates a portal.\nCosts " + portal.GetManaCost() +
                                (Math.Abs(portal.GetManaCost() - 1) == 0 ? " mana crystal" : " mana crystals") +
                                (Math.Abs(portal.GetTimeUntilCost() - 1) == 0 ? " per second." : " every " + portal.GetTimeUntilCost() + " seconds."),
                                false,
                                true),
                            new List<IButtonEffect>(),
                            false),
                        new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -(GetValue4Ths / LiterallyJust2 + GetValue4Ths + 10), GetValue34Ths, GetValue4Ths), isOpen? "Close Portal" : "Open Portal", false, true),
                            new List<IButtonEffect> { new OpenPortalEffect(isOpen) })
                    });
                    return mageTowerObject;
                case ObjectType.Military1Camp:
                    return new ScreenObject(GetUnits(objectAttributes),
                            true,
                            true,
                            false,
                            false,
                            1,
                            GetStandardRectangle());
                default:
                    {
                        var defaultScreen = GetStandardScreenObject(objectName);
                        return defaultScreen;
                    }
            }
        }

        /// <summary>
        /// Returns the attributes of an Object, as a List of <see cref="Button"/>s.
        /// </summary>
        /// <param name="objectAttributes">The Dictionary returned by <see cref="GameObject.GetAttributes()"/>.</param>
        /// <param name="offset">Can be used to change the positioning of the <see cref="Button"/>s, if not (0, 0), standard <see cref="Button"/>s like X will not be added.</param>
        /// <returns>Returns the attributes of an Object, as a List of <see cref="Button"/>s.</returns>
        private List<Button> GetStats((ObjectType, Dictionary<string, int>) objectAttributes, Vector2 offset = default)
        {
            var attributes = objectAttributes.Item2;
            var buttons = new List<Button>();
            if (offset == Vector2.Zero)
            {
                buttons.AddRange(new List<Button>()
                {
                    new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueHalf+GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), objectAttributes.Item1.ToString().Replace("1"," "), false, true),
                        new List<IButtonEffect> { new GoBackToPreviousEffect() }),
                    new Button(new ButtonData(new Rectangle(GetValueHalf+GetValue4Ths/LiterallyJust2, -GetValueMax, GetValue4Ths, GetValue4Ths/LiterallyJust2), "Stats", false, true),
                        new List<IButtonEffect>()),
                    new Button(new ButtonData(new Rectangle((GetValueMax - GetValue4Ths / LiterallyJust2), -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "X", false, true),
                        new List<IButtonEffect> { new GoBackEffect() })
                });
            }

            attributes.Remove("resourceTyp");
            if (attributes.Count <= 0)
            {
                return buttons;
            }

            var spacePer = (GetValueMax - GetValue4Ths / LiterallyJust2) / attributes.Count;
            var yPositionDifference = 0;
            foreach (var keyValuePair in attributes)
            {
                switch (keyValuePair.Key)
                {
                    case "Health":
                        var health = "Health: " + keyValuePair.Value + "/" + attributes["MaxHealth"];
                        spacePer = (GetValueMax - GetValue4Ths / LiterallyJust2) / (attributes.Count - 1);
                        buttons.Add(new Button(new ButtonData(new Rectangle((int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust2) + spacePer * yPositionDifference, GetValueMax, spacePer),
                                health,
                                false,
                                true),
                            new List<IButtonEffect>(),
                            false));
                        yPositionDifference++;
                        break;
                    case "Level":
                        
                        switch (keyValuePair.Value)
                        {
                            case 1:
                            case LiterallyJust2:
                                buttons.Add(new Button(new ButtonData(new Rectangle((int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust2) + spacePer * yPositionDifference + (int)offset.Y, GetValueMax / LiterallyJust2, spacePer),
                                        "Level " + keyValuePair.Value,
                                        false,
                                        true),
                                    new List<IButtonEffect>(),
                                    false));
                                // TODO SHOW COSTS WHEN ADDED
                                buttons.Add(new Button(new ButtonData(new Rectangle(GetValueMax / LiterallyJust2 + (int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust4) + spacePer * yPositionDifference + spacePer / LiterallyJust4 + (int)offset.Y, (GetValue4Ths / LiterallyJust2 + GetValue4Ths), GetValue4Ths / LiterallyJust2),
                                        "Level Up",
                                        false,
                                        true),
                                    new List<IButtonEffect>{ new ButtonLevelUpEffect(objectAttributes) }));
                                buttons.Add(new Button(new ButtonData(new Rectangle(GetValueMax / LiterallyJust2 + (int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust4) + spacePer * yPositionDifference + (int)(spacePer /1.5f) + (int)offset.Y, (GetValue4Ths / LiterallyJust2 + GetValue4Ths), GetValue4Ths / LiterallyJust2),
                                        BuildingCosts.UpgradeCostToText(objectAttributes.Item1, keyValuePair.Value),
                                        false,
                                        true),
                                    new List<IButtonEffect>(), drawBackground: false));
                                break;
                            default:
                                buttons.Add(new Button(new ButtonData(new Rectangle((int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust2) + spacePer * yPositionDifference + (int)offset.Y, GetValueMax, spacePer),
                                        "Level " + keyValuePair.Value,
                                        false,
                                        true),
                                    new List<IButtonEffect>(),
                                    false));
                                break;
                        }

                        yPositionDifference++;
                        break;
                    case "MaxHealth":
                    case "resourceTyp":
                        break;
                    default:
                        buttons.Add(new Button(new ButtonData(new Rectangle((int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust2) + spacePer * yPositionDifference + (int)offset.Y, GetValueMax, spacePer),
                                keyValuePair.Key + " " + keyValuePair.Value.ToString(),
                                false,
                                true),
                            new List<IButtonEffect>(),
                            false));
                        buttons.Add(new Button(new ButtonData(new Rectangle((int)offset.X, -(GetValueMax - GetValue4Ths / LiterallyJust2) + spacePer * yPositionDifference + (int)offset.Y, GetValueMax, spacePer),
                                keyValuePair.Key + " " + keyValuePair.Value.ToString(),
                                false,
                                true),
                            new List<IButtonEffect>(),
                            false));
                        yPositionDifference++;
                        break;
                }
            }
            return buttons;
        }

        /// <summary>
        /// Returns a list of units as <see cref="Button"/>s, dependent on the player race and GameObject Level.
        /// </summary>
        /// <param name="objectAttributes">The Dictionary returned by <see cref="GameObject.GetAttributes()"/>.</param>
        /// <returns>Returns a list of units as <see cref="Button"/>s, dependent on the player race and GameObject Level.</returns>
        private List<Button> GetUnits((ObjectType, Dictionary<string, int>) objectAttributes)
        {
            List<ObjectType> units = new List<ObjectType>();
            var level = objectAttributes.Item2["Level"];
            units = level switch
            {
                1 => Game1.mPlayerRace switch
                {
                    Game1.Race.Human => new List<ObjectType> { ObjectType.Knight },
                    Game1.Race.Dwarf => new List<ObjectType> { ObjectType.Axeman },
                    _ => new List<ObjectType> { ObjectType.Puncher }
                },
                LiterallyJust2 => Game1.mPlayerRace switch
                {
                    Game1.Race.Human => new List<ObjectType> { ObjectType.Knight, ObjectType.Archer },
                    Game1.Race.Dwarf => new List<ObjectType> { ObjectType.Axeman, ObjectType.Arbalist },
                    _ => new List<ObjectType> { ObjectType.Puncher, ObjectType.Slingshot }
                },
                3 => Game1.mPlayerRace switch
                {
                    Game1.Race.Human => new List<ObjectType> { ObjectType.Knight, ObjectType.Archer, ObjectType.Horseman, ObjectType.Mage },
                    Game1.Race.Dwarf => new List<ObjectType> { ObjectType.Axeman, ObjectType.Arbalist, ObjectType.Phalanx, ObjectType.Wolf1Rider },
                    _ => new List<ObjectType> { ObjectType.Puncher, ObjectType.Slingshot, ObjectType.Shaman }
                },
                _ => units
            };

            var buttons = new List<Button>
            {
                new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueHalf+GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), objectAttributes.Item1.ToString().Replace("1"," "), false, true),
                    new List<IButtonEffect> { new GoBackToPreviousEffect() }),
                new Button(new ButtonData(
                        new Rectangle(GetValue4Ths/LiterallyJust2,
                            -GetValue4Ths / LiterallyJust2,
                            GetValueHalf + GetValue4Ths,
                            GetValue4Ths / LiterallyJust2),
                        "Remove Military Camp",
                        false,
                        true),
                    new List<IButtonEffect> { new GetRemoveBuildingEffect(this) }),
                new Button(new ButtonData(new Rectangle(GetValueHalf+GetValue4Ths/LiterallyJust2, -GetValueMax, GetValue4Ths, GetValue4Ths/LiterallyJust2), "Stats", false, true),
                    new List<IButtonEffect>
                    {
                        new OpenPopUpScreenEffect(
                            new ScreenObject(GetStats(objectAttributes),
                                true,
                                true,
                                false,
                                false,
                                1,
                                GetStandardRectangle()))
                    }),
                new Button(new ButtonData(new Rectangle((GetValueMax-GetValue4Ths/LiterallyJust2), -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "X", false, true),
                    new List<IButtonEffect> { new GoBackEffect() })
            };

            var spacePer = (GetValueMax - GetValue4Ths / LiterallyJust2 * 2) / units.Count;
            buttons.AddRange(units.Select((t, i) =>
                new Button(new ButtonData(new Rectangle(GetValue4Ths - GetValue4Ths / LiterallyJust4, -(GetValueMax - GetValue4Ths / LiterallyJust2) + spacePer * i + spacePer / LiterallyJust4, GetValueHalf + GetValue4Ths / LiterallyJust2, spacePer / LiterallyJust2), t.ToString().Replace("1", " "), false, true),
                    new List<IButtonEffect> { new GetUnitBuyScreenEffect(1, t, 1, this) })));
            return buttons;
        }

        private sealed class OpenScreenEffect : IButtonEffect
        {
            private readonly ScreenObject mNextScreen;

            public OpenScreenEffect(ScreenObject nextScreen)
            {
                mNextScreen = nextScreen;
            }

            public (ButtonAction, object) Use()
            {
                DataStorage.mBuildError.Item1 = string.Empty;
                return (ButtonAction.AddScreen, mNextScreen);
            }
        }

        private sealed class OpenLoadScreenEffect : IButtonEffect
        {
            private readonly ScreenObjectFactory mFactory;

            public OpenLoadScreenEffect(ScreenObjectFactory factory)
            {
                mFactory = factory;
            }

            public (ButtonAction, object) Use()
            {
                DataStorage.mBuildError.Item1 = string.Empty;
                return (ButtonAction.AddScreen, mFactory.GetLoadGameMenu());
            }
        }

        private sealed class OpenDelayedMenu : IButtonEffect
        {
            private readonly Func<ScreenObject> mGetScreen;

            public OpenDelayedMenu(Func<ScreenObject> getScreen)
            {
                mGetScreen = getScreen;
            }

            public (ButtonAction, object) Use()
            {
                DataStorage.mBuildError.Item1 = string.Empty;
                return (ButtonAction.AddScreen, mGetScreen.Invoke());
            }
        }

        private sealed class OpenNewGameScreenEffect : IButtonEffect
        {
            private readonly ScreenObjectFactory mFactory;

            public OpenNewGameScreenEffect(ScreenObjectFactory factory)
            {
                mFactory = factory;
            }

            public (ButtonAction, object) Use()
            {
                DataStorage.mBuildError.Item1 = string.Empty;
                Game1.mPlayerRace = Game1.Race.Human;
                Game1.mEnemyRace = Game1.Race.Orc;
                ScreenManager.mIsChoosingName = false;
                mFactory.mPlayerName = string.Empty;
                return (ButtonAction.AddPopup, mFactory.GetNewGameMenu(90, 50));
            }
        }

        private sealed class OpenPopUpScreenEffect : IButtonEffect
        {
            private readonly ScreenObject mNextScreen;

            public OpenPopUpScreenEffect(ScreenObject nextScreen)
            {
                mNextScreen = nextScreen;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.AddPopup, mNextScreen);
            }
        }

        private sealed class LoadGameEffect : IButtonEffect
        {
            private readonly string mMapName;
            private readonly object mWidth;
            private readonly object mHeight;

            public LoadGameEffect(string mapName, object width, object height)
            {
                mMapName = mapName;
                mWidth = width;
                mHeight = height;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.LoadMap, (mMapName, mWidth, mHeight));
            }
        }

        private sealed class GoBackEffect : IButtonEffect
        {
            public (ButtonAction, object) Use ()
            {
                return (ButtonAction.RemoveScreen, null)!;
            }
        }

        private sealed class SaveSettingsEffect : IButtonEffect
        {
            public (ButtonAction, object) Use()
            {
                return (ButtonAction.SaveSettings, null)!;
            }
        }

        private sealed class GoBackErrorEffect : IButtonEffect
        {
            public (ButtonAction, object) Use()
            {
                DataStorage.mBuildError.Item1 = string.Empty;
                if (GameObjectManagement.mSelectedObjects.Any())
                {
                    return (ButtonAction.OpenPopupMenu, null)!;
                }

                return (ButtonAction.RemoveScreen, null)!;
            }
        }

        private sealed class GoBackToPreviousEffect : IButtonEffect
        {
            public (ButtonAction, object) Use()
            {
                return (ButtonAction.OpenPopupMenu, null)!;
            }
        }

        private sealed class GoToMainMenuEffect : IButtonEffect
        {
            public (ButtonAction, object) Use()
            {
                return (ButtonAction.BackToMainMenu, null)!;
            }
        }

        private sealed class BorderModeEffect : IButtonEffect
        {
            public (ButtonAction, object) Use()
            {
                return (ButtonAction.BorderSwitch, null)!;
            }
        }

        // ReSharper disable once UnusedType.Local
        private sealed class ButtonSoundEffectEffect : IButtonEffect
        {
            private readonly string mSoundEffectName;

            public ButtonSoundEffectEffect(string soundEffectName)
            {
                mSoundEffectName = soundEffectName;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.PlaySoundEffect, mSoundEffectName);
            }
        }

        private sealed class ButtonShinyEffect : IButtonEffect
        {
            // So that it still looks like a button, even if the effects list is empty
            public (ButtonAction, object) Use()
            {
                // is empty because it has no effects
                return (ButtonAction.None, null)!;
            }
        }

        /// <summary>
        /// The Button used for Leveling up Buildings.
        /// </summary>
        private sealed class ButtonLevelUpEffect : IButtonEffect
        {
            private readonly (ObjectType, Dictionary<string, int>) mStats;

            public ButtonLevelUpEffect((ObjectType, Dictionary<string, int>) stats)
            {
                mStats = stats;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.ChangeLevel, (mStats.Item1, mStats.Item2["Level"]));
            }
        }

        private sealed class ChangeVolumeEffect : IButtonEffect
        {
            private readonly float mAddThisAmount;
            private readonly bool mIsEffectsVolume;

            public ChangeVolumeEffect(float addThisAmount, bool isEffectsVolume)
            {
                mAddThisAmount = addThisAmount;
                mIsEffectsVolume = isEffectsVolume;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.ChangeVolume, (mAddThisAmount, mIsEffectsVolume));
            }
        }

        private sealed class ChangeScaleEffect : IButtonEffect
        {
            private readonly float mAddThisAmount;
            private readonly ScreenObjectFactory mFactory;

            public ChangeScaleEffect(float addThisAmount, ScreenObjectFactory factory)
            {
                mAddThisAmount = addThisAmount;
                mFactory = factory;
            }

            public (ButtonAction, object) Use()
            {
                Renderer.mHudScale += mAddThisAmount;
                Renderer.mHudScale = Math.Min(Math.Max(Renderer.mHudScale, 1), LiterallyJust2);
                return (ButtonAction.ReplaceScreen, mFactory.GetSettingsMenu());
            }
        }

        private sealed class ChangePlayerRaceEffect : IButtonEffect
        {
            private readonly Game1.Race mRace;

            public ChangePlayerRaceEffect(Game1.Race race)
            {
                mRace = race;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.SetPlayerRace, mRace);
            }
        }

        private sealed class ChangeEnemyRaceEffect : IButtonEffect
        {
            private readonly Game1.Race mRace;

            public ChangeEnemyRaceEffect(Game1.Race race)
            {
                mRace = race;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.SetEnemyRace, mRace);
            }
        }

        private sealed class ChangeCurrentUnitEffect : IButtonEffect
        {
            private readonly int mChangeBy;

            public ChangeCurrentUnitEffect(int changeBy)
            {
                mChangeBy = changeBy;
            }


            public (ButtonAction, object) Use()
            {
                GameObjectManagement.mCurrentSelectedUnit += mChangeBy;
                if (GameObjectManagement.mCurrentSelectedUnit >= GameObjectManagement.mSelectedObjects.Count)
                {
                    GameObjectManagement.mCurrentSelectedUnit = 0;
                }
                else if (GameObjectManagement.mCurrentSelectedUnit < 0)
                {
                    GameObjectManagement.mCurrentSelectedUnit = GameObjectManagement.mSelectedObjects.Count - 1;
                }
                return (ButtonAction.OpenPopupMenu, null)!;
            }
        }

        private sealed class OpenPortalEffect : IButtonEffect
        {
            private readonly bool mWillCloseIfFalse;

            public OpenPortalEffect(bool willCloseIfFalse)
            {
                mWillCloseIfFalse = willCloseIfFalse;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.OpenPortal, mWillCloseIfFalse);
            }
        }
        private sealed class ChangeUnitBuyScreenEffect : IButtonEffect
        {
            private readonly int mChangeBy;
            private readonly ObjectType mUnitName;
            private int mCurrent;
            private readonly ScreenObjectFactory mScreenObjectFactory;

            public ChangeUnitBuyScreenEffect(int changeBy, ObjectType unitName, int current, ScreenObjectFactory screenObjectFactory)
            {
                mChangeBy = changeBy;
                mUnitName = unitName;
                mCurrent = current;
                mScreenObjectFactory = screenObjectFactory;
            }

            public (ButtonAction, object) Use()
            {
                mCurrent += mChangeBy;
                if (mCurrent < 1)
                {
                    mCurrent = 1;
                }
                return (ButtonAction.AddPopup, mScreenObjectFactory.OpenUnitBuyScreen(mCurrent, mUnitName, mScreenObjectFactory));
            }
        }

        private sealed class GetUnitBuyScreenEffect : IButtonEffect
        {
            private readonly ObjectType mUnitName;
            private readonly int mCurrent;
            private readonly ScreenObjectFactory mScreenObjectFactory;

            public GetUnitBuyScreenEffect(int startAmount, ObjectType unitName, int current, ScreenObjectFactory screenObjectFactory)
            {
                mCurrent = startAmount;
                mUnitName = unitName;
                mCurrent = current;
                mScreenObjectFactory = screenObjectFactory;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.AddPopup, mScreenObjectFactory.OpenUnitBuyScreen(mCurrent, mUnitName, mScreenObjectFactory));
            }
        }

        private sealed class GetRemoveBuildingEffect : IButtonEffect
        {
            private readonly ScreenObjectFactory mScreenObjectFactory;

            public GetRemoveBuildingEffect(ScreenObjectFactory screenObjectFactory)
            {
                mScreenObjectFactory = screenObjectFactory;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.AddPopup, mScreenObjectFactory.OpenRemoveBuildingScreen());
            }
        }

        private sealed class BuildUnitEffect : IButtonEffect
        {
            private readonly int mAmount;
            private readonly ObjectType mObjectType;

            public BuildUnitEffect(int amount, ObjectType objectType)
            {
                mAmount = amount;
                mObjectType = objectType;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.BuildUnit, (mAmount, mObjectType));
            }
        }

        private sealed class BuildingCreationModeEffect : IButtonEffect
        {
            private readonly ObjectType mBuildingType;

            public BuildingCreationModeEffect(ObjectType buildingType)
            {
                mBuildingType = buildingType;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.BuildingMode, mBuildingType);
            }
        }

        private sealed class SaveGameEffect : IButtonEffect
        {
            private readonly int mPosition;
            public SaveGameEffect(int position)
            {
                mPosition = position;
            }


            public (ButtonAction, object) Use()
            {
                return (ButtonAction.SaveGame, mPosition);
            }
        }

        private sealed class LoadSavedGameEffect : IButtonEffect
        {
            private readonly int mPosition;
            private readonly string mName;
            private readonly int mTime;

            public LoadSavedGameEffect(int position, (string, int) nameAndTime)
            {
                mPosition = position;
                mName = nameAndTime.Item1;
                mTime = nameAndTime.Item2;
            }


            public (ButtonAction, object) Use()
            {
                return (ButtonAction.LoadSavedGame, (mPosition, mName, mTime));
            }
        }

        private sealed class GatherResourceEffect : IButtonEffect
        {
            private readonly int mObjId;

            public GatherResourceEffect(int objId)
            {
                mObjId = objId;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.GatherResource, mObjId);
            }
        }

        private sealed class ChangeByAmountEffect : IButtonEffect
        {
            private readonly ScreenObjectFactory mScreenObjectFactory;
            private readonly int mChangingValue;
            private readonly int mOtherValue;
            private readonly bool mIsWidth;
            private readonly int mAmount;

            public ChangeByAmountEffect(ScreenObjectFactory screenObjectFactory, int changingValue, int otherValue, bool isWidth, int amount)
            {
                mScreenObjectFactory = screenObjectFactory;
                mChangingValue = changingValue;
                mOtherValue = otherValue;
                mIsWidth = isWidth;
                mAmount = amount;
            }

            public (ButtonAction, object) Use()
            {
                ScreenManager.mIsChoosingName = false;
                return (ButtonAction.AddPopup, mScreenObjectFactory.GetNewGameMenu(Math.Min(Math.Max(mIsWidth
                    ? mChangingValue + mAmount : mOtherValue, 15), 200),
                    Math.Min(Math.Max(!mIsWidth? mChangingValue +mAmount : mOtherValue, 10), 200)));
            }
        }

        private sealed class AddGodlyResources : IButtonEffect
        {
            private readonly ResourceType mType;
            private readonly int mAmount;

            public AddGodlyResources(ResourceType type, int amount)
            {
                mType = type;
                mAmount = amount;
            }


            public (ButtonAction, object) Use()
            {
                DataStorage.mGameStatistics[true][mType] += mAmount;
                return (ButtonAction.None, null)!;
            }
        }

        private sealed class BuildingSelectionEffect : IButtonEffect
        {

            private ScreenObject OpenBuildingSelectionScreenObject()
            {
                var buttons = new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(0, -GetValueMax-GetValue4Ths/LiterallyJust2, GetValueMax + GetValue4Ths + GetValue4Ths/LiterallyJust2, GetValue4Ths/LiterallyJust2), "Leave Construction Mode", false, true),
                        new List<IButtonEffect> { new GoBackEffect() })
                };
                var buildings = new List<ObjectType>
                {
                    ObjectType.House,
                    ObjectType.Mine,
                    ObjectType.Military1Camp,
                    ObjectType.Mage1Tower,
                    ObjectType.Gate,
                    ObjectType.Tower,
                    ObjectType.Wall
                };
                if (Game1.mPlayerRace == Game1.Race.Orc)
                {
                    buildings.Remove(
                        ObjectType.Mine); 
                    buildings.Remove(
                        ObjectType.Mage1Tower);
                }
                var offsetX = 0;
                var offsetY = 0;
                var heightY = (GetValueHalf- GetValue4Ths/LiterallyJust4) /(int)Math.Ceiling((float)buildings.Count / LiterallyJust2);
                foreach (var building in buildings)
                {
                    buttons.Add(new Button(new ButtonData(
                            new Rectangle(offsetX * (GetValueHalf+ GetValue4Ths/LiterallyJust2) + GetValue4Ths / LiterallyJust2,
                                -GetValue34Ths- GetValue4Ths/LiterallyJust4 + offsetY * heightY*LiterallyJust2,
                                GetValueHalf,
                                heightY),
                            building.ToString().Replace("1", " "),
                            false,
                            true),
                        new List<IButtonEffect>{new BuildingCreationModeEffect(building)}));
                    offsetY++;
                    if (offsetY <= buildings.Count / LiterallyJust2)
                    {
                        continue;
                    }
                    offsetY = 0;
                    offsetX = 1;
                }
                buttons.Add(new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueMax + GetValue4Ths + GetValue4Ths / LiterallyJust2, GetValue4Ths / LiterallyJust2), string.Empty, false, true),
                    new List<IButtonEffect>(), false));
                return new ScreenObject(buttons,
                    true,
                    true,
                    false,
                    false,
                    1,
                    new Rectangle(0, -1, GetValueMax+ GetValueHalf- GetValue4Ths/LiterallyJust2, GetValueMax));
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.AddPopup, OpenBuildingSelectionScreenObject());
            }
        }

        private sealed class GodCreationModeEffect : IButtonEffect
        {
            private readonly ObjectType mBuildingType;
            private readonly bool mIsBuilding;

            public GodCreationModeEffect(ObjectType buildingType, bool isBuilding)
            {
                mBuildingType = buildingType;
                mIsBuilding = isBuilding;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.GodCreation, (mBuildingType, mIsBuilding));
            }
        }

        private sealed class BuildingGodModeEffect : IButtonEffect
        {
            private readonly ScreenObjectFactory mFactory;
            private readonly bool mIsBuilding;

            public BuildingGodModeEffect(ScreenObjectFactory factory, bool isBuilding)
            {
                mFactory = factory;
                mIsBuilding = isBuilding;
            }
            private ScreenObject OpenBuildingPlaceObject(bool isBuilding)
            {
                var buttons = new List<Button>
                {
                    new Button(new ButtonData(new Rectangle(0, -GetValueMax, GetValueMax + (isBuilding ? 0 : GetValue4Ths) + GetValue4Ths, GetValue4Ths/LiterallyJust2), "Back", false, true),
                        new List<IButtonEffect> { new BackToGodModeBuildingScreen(mFactory)}),
                    new Button(new ButtonData(new Rectangle(GetValueMax + (isBuilding ? 0 : GetValue4Ths) + GetValue4Ths, -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "X", false, true),
                        new List<IButtonEffect> { new GoBackEffect() })
                };

                List<ObjectType> buildings;
                if (isBuilding)
                {
                    buildings = new List<ObjectType>
                    {
                        ObjectType.House,
                        ObjectType.Mine,
                        ObjectType.Military1Camp,
                        ObjectType.Mage1Tower,
                        ObjectType.Gate,
                        ObjectType.Tower,
                        ObjectType.Wall
                    };
                }
                else
                {
                    buildings = new List<ObjectType>
                    {
                        ObjectType.Knight,
                        ObjectType.Archer,
                        ObjectType.Horseman,
                        ObjectType.Mage,
                        ObjectType.Human1Hero,
                        ObjectType.Axeman,
                        ObjectType.Arbalist,
                        ObjectType.Wolf1Rider,
                        ObjectType.Phalanx,
                        ObjectType.Dwarf1Hero,
                        ObjectType.Puncher,
                        ObjectType.Slingshot,
                        ObjectType.Shaman,
                        ObjectType.Orc1Hero,
                        ObjectType.Troll,
                        ObjectType.Builder
                    };
                }
                var offsetX = 0;
                var offsetY = 0;
                var pageSize = (isBuilding ? LiterallyJust2 : 3);
                var heightY = (GetValueHalf - GetValue4Ths / LiterallyJust4) / (int)Math.Ceiling((float)buildings.Count / pageSize - (isBuilding ? 0 : 1));
                foreach (var building in buildings)
                {
                    buttons.Add(new Button(new ButtonData(
                            new Rectangle(offsetX * ((isBuilding ? GetValueHalf : GetValue4Ths) + GetValue4Ths / LiterallyJust2) + GetValue4Ths / LiterallyJust2,
                                -GetValue34Ths - GetValue4Ths / (isBuilding ? LiterallyJust4 : 3) + offsetY * heightY * LiterallyJust2,
                                 isBuilding ? GetValueHalf : GetValue4Ths,
                                heightY),
                            building.ToString().Replace("1", " "),
                            false,
                            true),
                        new List<IButtonEffect> { new GodCreationModeEffect(building, isBuilding)}));
                    offsetY++;
                    if (offsetY <= buildings.Count / (pageSize + (isBuilding ? 0 : 1)))
                    {
                        continue;
                    }
                    offsetY = 0;
                    offsetX++;
                }

                return new ScreenObject(buttons,
                    true,
                    true,
                    false,
                    false,
                    1,
                    new Rectangle(0, -1, GetValueMax + (isBuilding ? GetValueHalf : GetValue34Ths) - GetValue4Ths / LiterallyJust2, GetValueMax));
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.AddPopup, OpenBuildingPlaceObject(mIsBuilding));
            }
        }

        private sealed class BackToGodModeBuildingScreen : IButtonEffect
        {
            private readonly ScreenObjectFactory mFactory;

            public BackToGodModeBuildingScreen(ScreenObjectFactory factory)
            {
                mFactory = factory;
            }

            public (ButtonAction, object) Use()
            {
                return (ButtonAction.AddPopup, mFactory.GetGodModeScreen());
            }
        }

        private ScreenObject OpenUnitBuyScreen(int current, ObjectType unitName, ScreenObjectFactory screenObjectFactory)
        {
            var ironCost = ObjectFactory.sTypeToMoney[unitName][ResourceType.Iron];
            var goldCost = ObjectFactory.sTypeToMoney[unitName][ResourceType.Gold];
            var manaCost = ObjectFactory.sTypeToMoney[unitName][ResourceType.Mana];
            var buttons = new List<Button>
            {
                new Button(new ButtonData(new Rectangle(GetValue4Ths/LiterallyJust2, -GetValueMax, GetValueMax * LiterallyJust2-GetValue4Ths, GetValue4Ths/LiterallyJust2), "Back", false, true),
                    new List<IButtonEffect> { new GoBackToPreviousEffect() }),
                new Button(new ButtonData(new Rectangle(GetValueMax * LiterallyJust2 - GetValue4Ths / LiterallyJust2, -GetValueMax, GetValue4Ths/LiterallyJust2, GetValue4Ths / LiterallyJust2), "X", false, true),
                    new List<IButtonEffect> { new GoBackEffect() }),
                new Button(new ButtonData(new Rectangle(GetValue34Ths, -GetValue34Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "+", false, true),
                    new List<IButtonEffect> { new ChangeUnitBuyScreenEffect(1, unitName, current, screenObjectFactory) }),
                new Button(new ButtonData(new Rectangle(0, -GetValue34Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "-", false, true),
                    new List<IButtonEffect> { new ChangeUnitBuyScreenEffect(-1, unitName, current, screenObjectFactory) }),
                new Button(new ButtonData(new Rectangle(GetValue4Ths / LiterallyJust2, -GetValueHalf, GetValue34Ths, GetValue4Ths / LiterallyJust2), "Costs: "
                        + (ironCost > 0 ? ironCost*current+" Iron " : string.Empty)
                        + (goldCost > 0 ? goldCost*current+" Gold " : string.Empty)
                        + (manaCost > 0 ? manaCost*current+" Mana crystals" : string.Empty)
                        , false, true),
                    new List<IButtonEffect>(), false),
                new Button(new ButtonData(new Rectangle((GetValue4Ths / LiterallyJust2 + GetValue4Ths), -GetValue4Ths, GetValue4Ths, GetValue4Ths / LiterallyJust2), "Buy", false, true),
                    new List<IButtonEffect> { new BuildUnitEffect(current, unitName) })};
            if (current == 1)
            {
                buttons.Add(new Button(
                    new ButtonData(
                        new Rectangle(GetValue4Ths, -GetValue34Ths, GetValueHalf, GetValue4Ths / LiterallyJust2),
                        "Current: " + current + " " + unitName,
                        false,
                        true),
                    new List<IButtonEffect>(),
                    false));
            }
            else
            {
                if (unitName.ToString().Contains("man") && unitName != ObjectType.Shaman)
                {
                    buttons.Add(new Button(
                        new ButtonData(
                            new Rectangle(GetValue4Ths, -GetValue34Ths, GetValueHalf, GetValue4Ths / LiterallyJust2),
                            "Current: " + current + " " + unitName.ToString().Replace("man", "men"),
                            false,
                            true),
                        new List<IButtonEffect>(),
                        false));
                }
                else if (unitName == ObjectType.Phalanx)
                {
                    buttons.Add(new Button(
                        new ButtonData(
                            new Rectangle(GetValue4Ths, -GetValue34Ths, GetValueHalf, GetValue4Ths / LiterallyJust2),
                            "Current: " + current + " Phalanxes",
                            false,
                            true),
                        new List<IButtonEffect>(),
                        false));
                }
                else
                {
                    buttons.Add(new Button(
                        new ButtonData(
                            new Rectangle(GetValue4Ths, -GetValue34Ths, GetValueHalf, GetValue4Ths / LiterallyJust2),
                            "Current: " + current + " " + unitName + "s",
                            false,
                            true),
                        new List<IButtonEffect>(),
                        false));
                }
            }
            buttons.AddRange(GetStats((ObjectFactory.BuildObject(unitName, new Point(-1, -1)).GetAttributes()), new Vector2(GetValueMax, 0)));
            return new ScreenObject(buttons,
                true,
                true,
                false,
                false,
                1,
                new Rectangle(0, -1, GetValueMax * LiterallyJust2, GetValueMax));
        }

        private ScreenObject OpenRemoveBuildingScreen()
        {
            var buttons = new List<Button>
            {
                new Button(new ButtonData(new Rectangle(GetValue4Ths / LiterallyJust2,
                            -GetValue34Ths-GetValue4Ths/LiterallyJust2,
                            GetValueMax,
                            GetValue4Ths / LiterallyJust2),
                        "Are you sure you want to remove this building?\nYou will only be returned half the building's value!",
                        false,
                        true),
                    new List<IButtonEffect>(), backgroundColor: new [] {Color.Gray, Color.Gray}),
                new Button(new ButtonData(new Rectangle(GetValueMax - GetValue4Ths -GetValue4Ths / LiterallyJust4,
                            -GetValueMax / LiterallyJust2,
                            GetValueHalf-GetValue4Ths / LiterallyJust4,
                            GetValue4Ths),
                        "No",
                        false,
                        true),
                    new List<IButtonEffect> {new GoBackToPreviousEffect()}),
                new Button(new ButtonData(
                        new Rectangle(GetValue4Ths / LiterallyJust2, -GetValueMax / LiterallyJust2, GetValueHalf-GetValue4Ths / LiterallyJust4, GetValue4Ths),
                        "Yes",
                        false,
                        true),
                    new List<IButtonEffect> {new RemoveBuildingEffect()})
            };
            return new ScreenObject(buttons,
                true,
                true,
                false,
                false,
                1,
                new Rectangle(0, -1, GetValueMax + GetValue4Ths, GetValueMax));
        }
    }

    internal sealed class ChooseNameEffect : IButtonEffect
    {
        public (ButtonAction, object) Use()
        {
            return (ButtonAction.ChooseName, null)!;
        }
    }

    internal sealed class StopActionsEffect : IButtonEffect
    {
        public (ButtonAction, object) Use()
        {
            return (ButtonAction.StopActions, null)!;
        }
    }

    internal sealed class SpecialActionEffect : IButtonEffect
    {
        private readonly GameObject mGameObject;

        public SpecialActionEffect(GameObject gameObject)
        {
            mGameObject = gameObject;
        }

        public (ButtonAction, object) Use()
        {
            return (ButtonAction.SpecialAction, mGameObject);
        }
    }

    internal sealed class PhalanxEffect : IButtonEffect
    {
        public (ButtonAction, object) Use()
        {
            return (ButtonAction.PhalanxAction, null)!;
        }
    }

    internal sealed class PotionEffect : IButtonEffect
    {
        private readonly GameObject mGameObject;
        private readonly PotionType mPotionType;

        public PotionEffect(GameObject gameObject, PotionType potionType)
        {
            mGameObject = gameObject;
            mPotionType = potionType;
        }

        public (ButtonAction, object) Use()
        {
            var potionAction =
                (Potion?)mGameObject.mActions.FirstOrDefault(action => action is Potion);
            switch (mPotionType)
            {
                case PotionType.Heal1Potion:
                    if (potionAction!.GetTimeToWaitHealPotion != 0)
                    {
                        return (ButtonAction.PlaySoundEffect, "Buzzer");
                    }
                    break;
                case PotionType.Damage1Potion:
                    if (potionAction!.GetTimeToWaitDamagePotion != 0)
                    {
                        return (ButtonAction.PlaySoundEffect, "Buzzer");
                    }
                    break;
                case PotionType.Speed1Potion:
                    if (potionAction!.GetTimeToWaitSpeedPotion != 0)
                    {
                        return (ButtonAction.PlaySoundEffect, "Buzzer");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return (ButtonAction.PotionAction, (mGameObject, mPotionType));
        }
    }

    internal sealed class RemoveBuildingEffect : IButtonEffect
    {
        public (ButtonAction, object) Use()
        {
            return (ButtonAction.RemoveBuilding, null)!;
        }
    }
}