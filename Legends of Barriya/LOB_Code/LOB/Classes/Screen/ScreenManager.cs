using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LOB.Classes.Data;
using LOB.Classes.Managers;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using LOB.Classes.Screen.Buttons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#pragma warning disable 8632

namespace LOB.Classes.Screen
{
    internal sealed class ScreenManager
    {
        // Used to switch between menu stacks
        public enum State
        {
            Game,
            MainMenu
        }

        internal State mGameState = State.MainMenu;
        private readonly InputManager mInput;
        private readonly GameWindow mGameWindow;
        private GameObjectManagement mGameObjectManager;
        private readonly ScreenObjectFactory mScreenObjectFactory;
        private static int sPlayTime;
        private static readonly Stopwatch sNewPlayTime = new Stopwatch();
        internal static readonly Stopwatch sSessionTime = new Stopwatch();
        private bool mOpenPopUpNextUpdate;
        private bool mReopenPopUpNextUpdate;
        
        private Button mFarmModeButton;

        internal List<ScreenObject> CurrentStack => mGameState == State.MainMenu ? mScreenStackMenu : mScreenStackInGame;
        internal bool IsMainScreen => mGameState == State.MainMenu && CurrentStack.Count == 1;
        private List<ScreenObject> mScreenStackInGame = new List<ScreenObject>();
        private readonly List<ScreenObject> mScreenStackMenu;

        // If mLoadMap is not null, in the next Update, a Map with that name will be loaded, and mLoadMap will become null again.
        // If the Map is not loaded, the ScreenManager goes back to the main menu.
        private (string, int, int) mLoadMap;
        internal GameMap mTestMap;
        public Camera mCamera;
        private readonly Song mMenuMusic;
        private readonly SongManager mSongManager;
        private readonly Settings mSettings;
        private bool mResizeNextUpdate;
        private int mResourceMode = -1;
        private bool mRepairMode;
        private bool mSetSpecialLevelUpModeToNullInNextFrame;
        public (GameObject, PotionType)? mPotionMode;
        private bool mSetPotionModeToNullInNextFrame;
        private bool mSetRepairModeToFalseInNextFrame;
        private InputData mInputData;
        private Point mWindowPosition;
        private bool mBorderlessMode = true;
        private bool mZoomNextUpdate;
        private (int Width, int Height) mOldSize;


        public static bool mIsChoosingName;
        public static int mCursorPosition;


        internal ObjectType? mBuildMode;
        internal ObjectType? mGodMode;
        private GameObject? mSpecialLevelUpMode;

        public ScreenManager(ContentManager content, GameWindow gameWindow)
        {
            mInput = new InputManager();
            mGameWindow = gameWindow;
            mWindowPosition = mGameWindow.Position;
            mMenuMusic = content.Load<Song>("Opening_Credits");
            mScreenObjectFactory = new ScreenObjectFactory
            {
                mScreenWidth = Renderer.mGraphicsDeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Width
            };
            gameWindow.ClientSizeChanged += Zoom; 
            mOldSize = (Renderer.mSpriteBatch.GraphicsDevice.Viewport.Width, Renderer.mSpriteBatch.GraphicsDevice.Viewport.Height);
            mSettings = new Settings();
            var settings = mSettings.LoadSettings();
            Achievements.LoadAchievements();
            mScreenStackMenu = new List<ScreenObject> { mScreenObjectFactory.GetMainMenu() };
            mGameWindow.AllowUserResizing = true;
            if (settings == null)
            {
                mBorderlessMode = true;
                mSongManager = new SongManager(content);
                BorderModeOff();
            }
            else
            {
                if (settings.Value.borderless)
                {
                    BorderModeOff();
                }
                else
                {
                    mBorderlessMode = false;
                    BorderModeOn();
                }
                mSongManager = new SongManager(content, settings.Value.volume, settings.Value.effectsVolume);
                Renderer.mHudScale = settings.Value.hudScale;
            }
            
            mSongManager.PlaySongList(new List<Song> { mMenuMusic });
            sSessionTime.Start();
        }

        private void Zoom(object sender = null, EventArgs e = null)
        {
            mZoomNextUpdate = true;
        }

        internal bool Update(GraphicsDeviceManager graphicsDeviceManager, ContentManager content, GameTime gameTime)
        {
            if (mSetSpecialLevelUpModeToNullInNextFrame)
            {
                mSpecialLevelUpMode = null;
            }

            if (mSetPotionModeToNullInNextFrame)
            {
                mPotionMode = null;
            }

            if (mSetRepairModeToFalseInNextFrame)
            {
                RemoveMode();
            }

            mSetSpecialLevelUpModeToNullInNextFrame = false;
            mSetPotionModeToNullInNextFrame = false;
            mSetRepairModeToFalseInNextFrame = false;
            var shouldEndGame = false;
            if (mGameState == State.Game && !mTestMap.mIsPaused)
            {
                ParticleEmitter.Update(gameTime);
                for (var i = Particle.mForegroundParticles.Count - 1; i > -1; i--)
                {
                    Particle.mForegroundParticles[i].Update(gameTime, Particle.mForegroundParticles);
                }

                for (var i = Particle.mBackgroundParticles.Count - 1; i > -1; i--)
                {
                    Particle.mBackgroundParticles[i].Update(gameTime, Particle.mBackgroundParticles);
                }
            }

            var currentRects = new List<Rectangle>{ CurrentStack[^1].GetBackgroundRectangle(Renderer.CurrentScreenScale) };
            currentRects.AddRange(CurrentStack[^1].mButtons.Select(button => button.mData.GetAlignedRectangle(graphicsDeviceManager.GraphicsDevice, Renderer.CurrentScreenScale)));
            currentRects.Add(CurrentStack[0].GetButtons[^1].mData.GetAlignedRectangle(graphicsDeviceManager.GraphicsDevice, Renderer.CurrentScreenScale)); // stops mouse

            var isClickable = !(mGameState != State.Game && CurrentStack[^1].mScreenId <= 0 &&
                                CurrentStack[^1].mScreenId != -3);
            foreach (var screenObject in CurrentStack)
            {
                if (!isClickable)
                {

                }

                var rect = screenObject.GetBackgroundRectangle(Renderer.CurrentScreenScale);
                if (rect.Contains(Mouse.GetState().Position))
                {
                    isClickable = false;
                }
            }

            var inputData = mInput.UpdateInput(isClickable, MousePositionOnTile());
            mInputData = inputData;

            if (mIsChoosingName)
            {
                var isSmall = !(inputData.mDownKeys.Contains(Keys.LeftShift) ||
                                inputData.mDownKeys.Contains(Keys.RightShift));
                foreach (var key in inputData.mReleasedKeys)
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (key)
                    {
                        case Keys.Back when mScreenObjectFactory.mPlayerName.Length > 0:
                            if (mCursorPosition > 0)
                            {
                                mScreenObjectFactory.mPlayerName =
                                    mScreenObjectFactory.mPlayerName.Remove(mCursorPosition - 1, 1);
                                mCursorPosition--;
                            }
                            break;
                        case Keys.LeftShift:
                        case Keys.RightShift:
                            isSmall = false;
                            break;
                        case Keys.CapsLock:
                        case Keys.Back:
                            break;
                        case Keys.Escape:
                        case Keys.Enter:
                            mIsChoosingName = false;
                            break;
                        case Keys.Right:
                            if (mCursorPosition < mScreenObjectFactory.mPlayerName.Length)
                            {
                                mCursorPosition++;
                            }
                            break;
                        case Keys.Left:
                            if (mCursorPosition > 0)
                            {
                                mCursorPosition--;
                            }
                            break;
                        default:
                            if (mScreenObjectFactory.mPlayerName.Length == 20)
                            {
                                break;
                            }

                            var newLetter = key.ToString();
                            mScreenObjectFactory.mPlayerName = mScreenObjectFactory.mPlayerName.Insert(mCursorPosition, isSmall ? newLetter.ToLower() : newLetter.ToUpper());
                            mCursorPosition++;
                            break;
                    }
                }

                AddPopUpScreen(mScreenObjectFactory.GetNewGameMenu());
            }

            if (mInputData.mReleasedKeys.Contains(Keys.Escape))
            {
                mIsChoosingName = false;
                DataStorage.mBuildError.Item1 = string.Empty;
                EscapePressed();
            }

            if (DataStorage.mPopUpObjectHasDied)
            {
                DataStorage.mPopUpObjectHasDied = false;
                RemoveScreen();
            }

            if (DataStorage.mBuildError.Item1 != string.Empty)
            {
                if (DataStorage.mBuildError.Item2 == -2 || (GameObjectManagement.mSelectedObjects.Any() && GameObjectManagement.mSelectedObjects[GameObjectManagement.mCurrentSelectedUnit] == DataStorage.mBuildError.Item2))
                {
                    RemoveMode();
                    AddPopUpScreen(mScreenObjectFactory.OpenErrorScreen(DataStorage.mBuildError.Item1));
                }
                else
                {
                    DataStorage.mBuildError.Item1 = string.Empty;
                }
            }

            if (mGameState == State.Game)
            {
                if (mInputData.mReleasedKeys.Contains(Keys.F))
                {
                    Renderer.mDrawFog = !Renderer.mDrawFog;
                    DataStorage.mBuildError.Item1 = string.Empty;
                }

                if (mInputData.mReleasedKeys.Contains(Keys.G))
                {
                    DataStorage.mBuildError.Item1 = string.Empty;
                    AddPopUpScreen(mScreenObjectFactory.GetGodModeScreen());
                }

                if (mInputData.mReleasedKeys.Contains(Keys.P))
                {
                    Renderer.mDrawPaths = !Renderer.mDrawPaths;
                    DataStorage.mBuildError.Item1 = string.Empty;
                }

                if (mBuildMode != null) // Change color in case there are now enough resources
                {
                    CurrentStack[^1].mButtons[^1].mBackgroundColor = BuildingCosts.HasEnoughForCreation(mBuildMode.Value) ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : new[] { Color.OrangeRed, Color.DarkOrange };
                }

                if (inputData.mMouseData.mNextMouseState.RightButton == ButtonState.Pressed &&
                    inputData.mMouseData.mPrevMouseState.RightButton == ButtonState.Released)
                {
                    if (mBuildMode != null)
                    {
                        mGameObjectManager.MakePlayerEvents(2, mBuildMode, inputData.mMouseData.mMousePositionOnTile);
                    }

                    else if (mGodMode != null)
                    {
                        mGameObjectManager.MakePlayerEvents(9, mGodMode, inputData.mMouseData.mMousePositionOnTile);
                    }

                    else if (mRepairMode)
                    {
                        mGameObjectManager.MakePlayerEvents(8, null, inputData.mMouseData.mMousePositionOnTile);
                        mSetRepairModeToFalseInNextFrame = true;
                    }
                    else if (mResourceMode != -1)
                    {
                        mGameObjectManager.MakePlayerEvents(3, null, inputData.mMouseData.mMousePositionOnTile, mResourceMode);
                        mResourceMode = -1;
                        shouldEndGame = RemoveScreen();
                    }
                    else if (mSpecialLevelUpMode != null)
                    {
                        mGameObjectManager.MakeEvents(mSpecialLevelUpMode.mIsPlayer, 6, new List<int> { mSpecialLevelUpMode.mObjectId }, target: new Point((int)inputData.mMouseData.mMousePositionOnTile.X, (int)inputData.mMouseData.mMousePositionOnTile.Y + 1));
                        mSetSpecialLevelUpModeToNullInNextFrame = true;
                        mOpenPopUpNextUpdate = true;
                    }
                    else if (mPotionMode != null)
                    {
                        var (gameObject, potionType) = (ValueTuple<GameObject, PotionType>)mPotionMode;
                        mGameObjectManager.MakeEvents(gameObject.mIsPlayer, 7, new List<int> { gameObject.mObjectId }, target: new Point((int)inputData.mMouseData.mMousePositionOnTile.X, (int)inputData.mMouseData.mMousePositionOnTile.Y + 1), potionType: potionType);
                        mSetPotionModeToNullInNextFrame = true;
                        mOpenPopUpNextUpdate = true;
                    }
                    else
                    {
                        mGameObjectManager.MakePlayerEvents(10, null, inputData.mMouseData.mMousePositionOnTile);
                    }
                }
            }

            if (mInputData.mReleasedKeys.Contains(Keys.F11))
            {
                mOldSize = (Renderer.mSpriteBatch.GraphicsDevice.Viewport.Width, Renderer.mSpriteBatch.GraphicsDevice.Viewport.Height);
                SwitchBorderMode();
                mZoomNextUpdate = true;
            }

            if (mInputData.mReleasedKeys.Contains(Keys.F12))
            {
                var te = new int[graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth* graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight];
                var tex = new Texture2D(graphicsDeviceManager.GraphicsDevice, graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth, graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight);
                graphicsDeviceManager.GraphicsDevice.GetBackBufferData(te);
                tex.SetData(te);

                var path = ContentIo.GetPath + ContentIo.GetPathConnector + "Screenshots";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var file = File.Create(path + ContentIo.GetPathConnector + DateTime.Now.ToString(@"yyyy\_dd\_hh\_mm\_ss")+".png");
                tex.SaveAsPng(file, graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth, graphicsDeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight);
                file.Flush();
            }

            if (mZoomNextUpdate)
            {
                Renderer.mCurrentScreenScale =
                    (float)graphicsDeviceManager.GraphicsDevice.Viewport.Width / graphicsDeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                mZoomNextUpdate = false;
                if (mGameState == State.Game)
                {
                    mCamera.Zoom(mOldSize);
                    mOldSize = (Renderer.mSpriteBatch.GraphicsDevice.Viewport.Width, Renderer.mSpriteBatch.GraphicsDevice.Viewport.Height);
                }
            }

            if (mGameState == State.Game && !mTestMap.mIsPaused)
            {
                mGameObjectManager.Update(gameTime);
                if (GameObjectManagement.mSelectedObjects.Select(DataStorage.GetObject).Any(gameObject => gameObject is
                {
                    mHasChanged: true
                }))
                {
                    if (CurrentStack[^1].mButtons.Contains(mFarmModeButton))
                    {
                        CurrentStack[^1].mButtons.RemoveAt(CurrentStack[^1].mButtons.Count - 1);
                    }
                    else
                    {
                        mReopenPopUpNextUpdate = true;
                        RemoveScreen();
                        mOpenPopUpNextUpdate = true;
                        mReopenPopUpNextUpdate = false;
                    }
                    foreach (var gameObject in GameObjectManagement.mSelectedObjects.Select(DataStorage.GetObject))
                    {
                        if (gameObject == null)
                        {
                            continue;
                        }
                        gameObject.mHasChanged = false;
                    }
                }

                if (GameObjectManagement.mSelectedObjects.Any() && mOpenPopUpNextUpdate)
                {
                    var selectedObject = DataStorage.GetObject(GameObjectManagement.mSelectedObjects[GameObjectManagement.mCurrentSelectedUnit]);
                    mOpenPopUpNextUpdate = false;
                    var tmp = mSpecialLevelUpMode != null || mPotionMode != null;
                    AddPopUpScreen(mScreenObjectFactory.GetPopUpMenu(selectedObject.GetAttributes(), selectedObject.mObjectId));
                    if (tmp)
                    {
                        mSpecialLevelUpMode = DataStorage.mGameObjects.Values.First();
                    }
                }
            }

            if (mLoadMap != default)
            {
                LoadNewMap(graphicsDeviceManager, content, 0, (mLoadMap.Item2, mLoadMap.Item3));
                sNewPlayTime.Restart();
                sPlayTime = 0;
            }

            // Loops through all ScreenObjects in the current Stack, as long as a screen wants the one below it to be updated too.
            var currentPosition = CurrentStack.Count - 1;
            var goLower = true;

            var saveGame = -1;

            while (currentPosition > -1 && goLower)
            {
                goLower = CurrentStack[currentPosition].mUpdateLower;
                var effects = CurrentStack[currentPosition].Update(inputData);
                foreach (var (buttonActions, parameters) in effects)
                {
                    Button button;
                    switch (buttonActions)
                    {
                        case ButtonAction.AddScreen:
                            if (CurrentStack[^1].mScreenId > 0)
                            {
                                RemoveScreen();
                            }
                            AddScreen((ScreenObject) parameters);
                            break;
                        case ButtonAction.ReplaceScreen:
                            RemoveScreen();
                            AddScreen((ScreenObject)parameters);
                            break;
                        case ButtonAction.AddPopup:
                            DataStorage.mBuildError = (string.Empty, -1);
                            AddPopUpScreen((ScreenObject) parameters);
                            break;
                        case ButtonAction.OpenPopupMenu:
                            if (GameObjectManagement.mSelectedObjects.Count > 0)
                            {
                                mOpenPopUpNextUpdate = true;
                            }
                            break;
                        case ButtonAction.RemoveScreen:
                            mIsChoosingName = false;
                            shouldEndGame = shouldEndGame || RemoveScreen();
                            break;
                        case ButtonAction.LoadMap:
                            mIsChoosingName = false;
                            var data = ((string, object, object)) parameters;
                            mLoadMap = (data.Item1, (int)data.Item2, (int)data.Item3);
                            break;
                        case ButtonAction.BackToMainMenu:
                            mIsChoosingName = false;
                            BackToMainMenu();
                            break;
                        case ButtonAction.None:
                            break;
                        case ButtonAction.BorderSwitch:
                            SwitchBorderMode();
                            break;
                        case ButtonAction.PlaySoundEffect:
                            SongManager.PlayEffect((string) parameters);
                            break;
                        case ButtonAction.ChangeVolume:
                            var (changeAmount, isEffectsVolume) = ((float, bool))parameters;
                            if (isEffectsVolume)
                            {
                                mSongManager.ChangeEffectsVolume(changeAmount);
                                // To show the player what the current effects volume is like
                                SongManager.PlayEffect("Buzzer");
                            }
                            else
                            {
                                mSongManager.ChangeVolume(changeAmount);
                            }
                            break;
                        case ButtonAction.SetPlayerRace:
                            mIsChoosingName = false;
                            Game1.mPlayerRace = (Game1.Race) parameters;
                            AddPopUpScreen(mScreenObjectFactory.GetNewGameMenu());
                            break;
                        case ButtonAction.SetEnemyRace:
                            mIsChoosingName = false;
                            Game1.mEnemyRace = (Game1.Race) parameters;
                            AddPopUpScreen(mScreenObjectFactory.GetNewGameMenu());
                            break;
                        case ButtonAction.ChangeLevel:
                            var (type, level) = ((ObjectType, int)) parameters;

                            if (BuildingCosts.HasEnoughForUpgrade(type, level))
                            {
                                SongManager.PlayEffect("Upgrade");
                                mGameObjectManager.MakePlayerEvents(1, null, inputData.mMouseData.mMousePositionOnTile);
                                mOpenPopUpNextUpdate = true;
                                break;
                            }
                            SongManager.PlayEffect("Buzzer");
                            break;
                        case ButtonAction.BuildingMode:
                            RemoveMode();
                            MakeMenuSeeThrough();
                            mBuildMode = (ObjectType)parameters;
                            button = CurrentStack[^1].mButtons.FirstOrDefault(theButton =>
                                theButton.mData.mButtonText == mBuildMode!.Value.ToString().Replace("1", " "));
                            if (button != default)
                            {
                                button.mBackgroundColor = button.mHighlightColor;
                                CurrentStack[^1].mButtons[^1].mData.mButtonText = BuildingCosts.CostToText(mBuildMode.Value);
                                CurrentStack[^1].mButtons[^1].mDrawBackground = true;
                                CurrentStack[^1].mButtons[^1].mBackgroundColor = BuildingCosts.HasEnoughForCreation(mBuildMode.Value) ? new[] { Button.sLighterGreen, Button.sDarkerGreen } : new[] { Color.OrangeRed, Color.DarkOrange };
                            }

                            break;
                        case ButtonAction.SaveGame:
                            saveGame = (int)parameters;
                            break;
                        case ButtonAction.LoadSavedGame:
                            var (position, name, playTime) = ((int, string, int)) parameters;
                            LoadNewMap(graphicsDeviceManager, content, position);
                            mScreenObjectFactory.mPlayerName = name;
                            sPlayTime = playTime;
                            sNewPlayTime.Restart();
                            break;
                        case ButtonAction.GatherResource:
                            RemoveMode();
                            MakeMenuSeeThrough();
                            mResourceMode = (int)parameters;
                            mFarmModeButton = mScreenObjectFactory.GetFarmModeButton("Resource Selection");
                            if (!CurrentStack[^1].mButtons.Contains(mFarmModeButton))
                            {
                                CurrentStack[^1].mButtons.Add(mFarmModeButton);
                            }
                            break;
                        case ButtonAction.OpenPortal:
                            mGameObjectManager.MakePlayerEvents(4, null, inputData.mMouseData.mMousePositionOnTile, -1, (bool) parameters);
                            mOpenPopUpNextUpdate = true;
                            break;
                        case ButtonAction.BuildUnit:
                            var (amount, objectType) = (ValueTuple<int, ObjectType>)parameters;
                            if (objectType <= ObjectType.Dwarf1Hero && objectType >= ObjectType.Human1Hero)
                            {
                                mOpenPopUpNextUpdate = true;
                            }
                            mGameObjectManager.MakePlayerEvents(5, objectType, inputData.mMouseData.mMousePositionOnTile, amount: amount);
                            break;
                        case ButtonAction.ChooseName:
                            mIsChoosingName = true;
                            break;
                        case ButtonAction.SaveSettings:
                            var volumes = mSongManager.GetVolume();
                            mSettings.SaveSettings(mBorderlessMode, volumes.volume, volumes.effectsVolume, Renderer.mHudScale);
                            if (mGameState == State.MainMenu)
                            {
                                shouldEndGame = shouldEndGame || RemoveScreen();
                            }
                            else
                            {
                                AddPopUpScreen(mScreenObjectFactory.GetPauseMenu());
                            }
                            break;
                        case ButtonAction.StopActions:
                            mGameObjectManager.StopCurrentEvents();
                            break;
                        case ButtonAction.PhalanxAction:
                            foreach (var phalanxObject in GameObjectManagement.mSelectedObjects.Select(DataStorage.GetObject).Where(gObject => gObject.mName == ObjectType.Phalanx))
                            {
                                phalanxObject.mObjectEvents[EventType.PhalanxEvent].Add(new PhalanxEvent());
                            }
                            break;
                        case ButtonAction.SpecialAction:
                            var gameObject = (GameObject)parameters;
                            if (gameObject.mName == ObjectType.Human1Hero)
                            {
                                RemoveMode();
                                MakeMenuSeeThrough();
                                mSpecialLevelUpMode = gameObject;
                                mFarmModeButton = mScreenObjectFactory.GetFarmModeButton("Select the mine you want to upgrade:");
                                if (!CurrentStack[^1].mButtons.Contains(mFarmModeButton))
                                {
                                    CurrentStack[^1].mButtons.Add(mFarmModeButton);
                                }
                                break;
                            }
                            else if (gameObject.mName == ObjectType.Builder || gameObject.mName == ObjectType.Troll)
                            {
                                RemoveMode();
                                MakeMenuSeeThrough();
                                mRepairMode = true;
                                mFarmModeButton = mScreenObjectFactory.GetFarmModeButton("Select the building you want to repair:");
                                if (!CurrentStack[^1].mButtons.Contains(mFarmModeButton))
                                {
                                    CurrentStack[^1].mButtons.Add(mFarmModeButton);
                                }
                                break;
                            }
                            DataStorage.AddEvent(gameObject.mObjectId, new SpecialEvent(new Point(-1, -1)));
                            break;
                        case ButtonAction.PotionAction:
                            var (potionGameObject, potionType) = (ValueTuple<GameObject, PotionType>) parameters;
                            RemoveMode();
                            MakeMenuSeeThrough();
                            mPotionMode = (potionGameObject, potionType);
                            mFarmModeButton = mScreenObjectFactory.GetFarmModeButton("Select a square for the potion");
                            if (!CurrentStack[^1].mButtons.Contains(mFarmModeButton))
                            {
                                CurrentStack[^1].mButtons.Add(mFarmModeButton);
                            }
                            break;
                        case ButtonAction.GodCreation:
                            RemoveMode();
                            MakeMenuSeeThrough();
                            var (godMode, isBuilding) = (ValueTuple<ObjectType, bool>) parameters;
                            mGodMode = godMode;
                            button = CurrentStack[^1].mButtons.First(godButton =>
                                godButton.mData.mButtonText == mGodMode!.Value.ToString().Replace("1", " "));
                            if (button != default)
                            {
                                button.mBackgroundColor = button.mHighlightColor;
                            }

                            mFarmModeButton = isBuilding ? mScreenObjectFactory.GetBuildModeButton("Select a square to place the building") : mScreenObjectFactory.GetBuildModeButtonBig("Select a square to place the unit");
                            if (!CurrentStack[^1].mButtons.Contains(mFarmModeButton))
                            {
                                CurrentStack[^1].mButtons.Add(mFarmModeButton);
                            }
                            break;
                        case ButtonAction.RemoveBuilding:
                            var objectToRemove = DataStorage.GetObject(GameObjectManagement.mSelectedObjects[GameObjectManagement.mCurrentSelectedUnit]);
                            objectToRemove.mObjectEvents[EventType.RemoveBuildingEvent].Add(new RemoveBuildingEvent());
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                currentPosition -= 1;
            }

            if (mGameState == State.Game && inputData.mMouseData.mNewSelection && !mTestMap.mIsPaused)
            {
                if (DataStorage.mBuildError.Item1 == string.Empty && mGameObjectManager.FindObjectsInSelectionRect(inputData))
                {
                    mOpenPopUpNextUpdate = true;
                }
            }

            if (inputData.mMouseData.mIsSelecting && (CurrentStack[^1].mScreenId > 0))
            {
                shouldEndGame = shouldEndGame || RemoveScreen();
                DataStorage.mBuildError.Item1 = string.Empty;
            }

            if (mGameState != State.Game)
            {
                return shouldEndGame;
            }

            if (inputData.mMouseData.mPrevMouseState.RightButton == ButtonState.Pressed &&
                inputData.mMouseData.mNextMouseState.RightButton == ButtonState.Released)
            {
                if(GameObjectManagement.mSelectedObjects.Count > 0)
                {
                    var selectedObject =
                        DataStorage.GetObject(GameObjectManagement.mSelectedObjects[GameObjectManagement.mCurrentSelectedUnit]);
                    if (selectedObject != null && mBuildMode == null)
                    {
                        selectedObject.mHasChanged = true;
                    }
                }
            }

            if (goLower && currentPosition == -1 || mResizeNextUpdate)
            {
                if (mCamera.Update(inputData, currentRects.Any(rect => rect.Contains(inputData.mMouseData.mNextMouseState.Position))) && !mTestMap.mIsPaused)
                {
                    mCamera.ResizeToFitWindow();
                }
                mTestMap.SetPaused(false);
                mResizeNextUpdate = false;
            }
            else
            {
                mTestMap.SetPaused(true);
            }

            if (DataStorage.mGameStatistics[false][ResourceType.MainBuildingAlive] == 0)
            {
                OpenEndGameScreen(true);
            }

            else if (DataStorage.mGameStatistics[true][ResourceType.MainBuildingAlive] == 0)
            {
                OpenEndGameScreen(false);
            }

            if (shouldEndGame)
            {
                Achievements.SaveAchievements();
            }

            // Let all events happen before saving

            if (saveGame != -1)
            {
                new ContentIo(mTestMap).SaveMap(saveGame, mScreenObjectFactory.mPlayerName, GetTimeSeconds());
            }
            return shouldEndGame;
        }

        private void MakeMenuSeeThrough()
        {
            CurrentStack[^1].mButtons.ForEach(button => button.mBackgroundColor = button.mSeeThroughColor);
            CurrentStack[^1].mBackgroundColors = CurrentStack[^1].mSeeThroughColor;
        }

        private void RemoveMode()
        {
            mResourceMode = -1;
            mBuildMode = null;
            mSpecialLevelUpMode = null;
            mPotionMode = null;
            mRepairMode = false;
            mGodMode = null;
            
            CurrentStack[^1].mButtons.RemoveAll(button => button == mFarmModeButton);

            CurrentStack[^1].mButtons.ForEach(button => button.mBackgroundColor = button.mStandardColor);
            CurrentStack[^1].mBackgroundColors = CurrentStack[^1].mStandardColor;
        }

        private static int GetTimeSeconds()
        {
            var time = TimeSpan.FromSeconds(sPlayTime);
            return (int)sNewPlayTime.Elapsed.Add(time).TotalSeconds;
        }

        internal static string GetTimeString()
        {
            return GetTimeString(GetTimeSeconds());
        }

        internal static string GetTimeString(int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"dd\:hh\:mm\:ss");
        }

        public static string GetPlayTimeTotalString()
        {
            return "You played for: " + GetTimeString(Achievements.sAchievements[Achievements.AchievementType.AllTimeSeconds] + GetTimeSeconds());
        }

        private void LoadNewMap(GraphicsDeviceManager graphicsDeviceManager, ContentManager content, int position = 0, (int, int) size = default)
        {
            try
            {
                mTestMap = new GameMap(content, out mGameObjectManager, position, size);
                Renderer.Initialize(graphicsDeviceManager, content);
                mCamera = new Camera(GameMap.mStandardTileSize,
                    GameMap.mWidth,
                    GameMap.mHeight,
                    mInputData.mMouseData.mNextMouseState.ScrollWheelValue);
                mCamera.ResizeToFitWindow();
                sNewPlayTime.Restart();
            }
            catch
            {
                mLoadMap = default;
                BackToMainMenu();
                return;
            }

            mGameState = State.Game;
            mSongManager.PlaySongList(new List<Song>
                    { content.Load<Song>("Under_Siege_Intro"), content.Load<Song>("Under_Siege_Loop") },
                new List<int> { 0, -1 });
            mScreenStackInGame = new List<ScreenObject> { mScreenObjectFactory.GetHudMenu() };
            mLoadMap = default;
        }

        // Removes the screen on top of the stack 
        private bool RemoveScreen()
        {
            if (CurrentStack.Count > 1)
            {
                CurrentStack.RemoveAt(CurrentStack.Count - 1);
                if (mReopenPopUpNextUpdate)
                {
                    return false;
                }
                if (!mInputData.mDownKeys.Contains(Keys.LeftShift) && !mInputData.mDownKeys.Contains(Keys.RightShift))
                {
                    GameObjectManagement.mSelectedObjects = new List<int>();
                }
                GameObjectManagement.mCurrentSelectedUnit = 0;
                RemoveMode();
            }
            else if (mGameState == State.MainMenu)
            {
                if (IsMainScreen)
                {
                    mScreenObjectFactory.mPlayerName = string.Empty;
                }
                return true;
            } 
            return false;
        }

        private void BackToMainMenu()
        {
            mScreenObjectFactory.mPlayerName = string.Empty;
            mGameState = State.MainMenu;
            
            sNewPlayTime.Stop();

            while (CurrentStack.Count > 0)
            {
                mSongManager.PlaySongList(new List<Song> { mMenuMusic });
                CurrentStack.RemoveAt(CurrentStack.Count - 1);
            }
            AddScreen(mScreenObjectFactory.GetMainMenu());
        }

        private void AddScreen(ScreenObject screenObject)
        {
            CurrentStack.Add(screenObject);
        }

        private void AddPopUpScreen(ScreenObject screenObject)
        {
            RemoveMode();
            if (CurrentStack.Count == 1)
            {
                AddScreen(screenObject);
            }
            else
            {
                CurrentStack[^1] = screenObject;
            }
        }

        
        private Vector2 MousePositionOnTile()
        {
            if (mGameState != State.Game)
            {
                return Vector2.Zero;
            }
            var tileSize = GameMap.mStandardTileSize * Renderer.mScale * 0.5f;
            var mousePositionX = Mouse.GetState().Position.X - mCamera.GetCameraOffset().X;
            var mousePositionY = Mouse.GetState().Position.Y - mCamera.GetCameraOffset().Y;
            return new Vector2(MousePositionOnClosestTile(mousePositionX, GameMap.mWidth - 1, 0),
                MousePositionOnClosestTile(mousePositionY, GameMap.mHeight-1, 0));

            int MousePositionOnClosestTile(float position, int maxValue, int minValue)
            {
                return (int) Math.Min(Math.Max((float)Math.Floor(position / tileSize), minValue), maxValue);
            }
        }

#nullable enable
        private void EscapePressed()
        {
            mBuildMode = null;
            mResourceMode = -1;
            RemoveMode();
            if (mGameState == State.Game && mScreenStackInGame.Count == 1)
            {
                AddScreen(mScreenObjectFactory.GetPauseMenu());
            }
            else
            {
                RemoveScreen();
            }
        }

        public void Draw()
        {
            Renderer.Draw(this, mInputData);
        }

        private void SwitchBorderMode()
        {
            mBorderlessMode = !mBorderlessMode;
            var volumes = mSongManager.GetVolume();
            mSettings.SaveSettings(mBorderlessMode, volumes.volume, volumes.effectsVolume, Renderer.mHudScale);
            if (mBorderlessMode)
            {
                var (newX, newY) = (mGameWindow.ClientBounds.X + Mouse.GetState().Position.X,
                    mGameWindow.ClientBounds.Y + Mouse.GetState().Position.Y);
                BorderModeOff();
                Mouse.SetPosition(newX, newY);
            }
            else
            {
                BorderModeOn();
            }
        }

        private void BorderModeOn()
        {
            Renderer.mGraphicsDeviceManager.IsFullScreen = false;
            Renderer.mGraphicsDeviceManager.ApplyChanges();
            var preferredHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2;
            var preferredWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2;
            Renderer.mCurrentScreenScale = 0.5f;
            Renderer.mGraphicsDeviceManager.PreferredBackBufferHeight = preferredHeight;
            Renderer.mGraphicsDeviceManager.PreferredBackBufferWidth = preferredWidth;
            mGameWindow.IsBorderless = false;
            Renderer.mGraphicsDeviceManager.ApplyChanges();
            mGameWindow.Position = mWindowPosition;
        }

        private void BorderModeOff()
        {
            mWindowPosition = mGameWindow.Position;
            mGameWindow.Position = Point.Zero;
            Renderer.mGraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Renderer.mGraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Renderer.mCurrentScreenScale = 1f;
            Renderer.mGraphicsDeviceManager.ApplyChanges();
            Renderer.mGraphicsDeviceManager.IsFullScreen = true;
            mGameWindow.IsBorderless = true;
            Renderer.mGraphicsDeviceManager.ApplyChanges();
        }

        private void OpenEndGameScreen(bool won)
        {
            sNewPlayTime.Stop();
            AddScreen(mScreenObjectFactory.GetEndGameScreen(won));
        }
    }
}