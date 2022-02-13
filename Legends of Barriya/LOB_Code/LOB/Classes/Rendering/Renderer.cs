using System;
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.Managers;
using LOB.Classes.Map;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Screen;
using LOB.Classes.Screen.Buttons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace LOB.Classes.Rendering
{
    internal static class Renderer
    {
        public static SpriteBatch mSpriteBatch;
        internal static bool mDrawFog = true;
        internal static GraphicsDeviceManager mGraphicsDeviceManager;
        internal static float mScale;
        internal static float mHudScale = 1f;
        private static SpriteFont sFont;
        private static Dictionary<ObjectType, List<Texture2D>> sTextures;
        private static List<Texture2D> sProjectileTextures;
        private static Texture2D sTitleScreen;
        private static Dictionary<Point, Texture2D> sObjectsDrawn;
        public static float mCurrentScreenScale = 1f;
        public static float CurrentScreenScale => mCurrentScreenScale * mHudScale;
        private static bool sIsPaused;

        /// <summary>
        /// Initializes the whole rendering process + all the textures needed for the units
        /// The units all have 4 Textures, 1 for idling, 2 for walking, 1 for attacking
        /// </summary>
        /// <param name="graphicsDeviceManager"></param>
        /// <param name="content"></param>
        public static void Initialize(GraphicsDeviceManager graphicsDeviceManager, ContentManager content)
        {
            sObjectsDrawn = new Dictionary<Point, Texture2D>();
            mGraphicsDeviceManager = graphicsDeviceManager;
            mScale = 0;
            mSpriteBatch = new SpriteBatch(graphicsDeviceManager.GraphicsDevice);
            sFont = content.Load<SpriteFont>("Arial");
            sTitleScreen = content.Load<Texture2D>("sprites\\TitleScreen");
            sTextures = new Dictionary<ObjectType, List<Texture2D>>
            {
                { ObjectType.Grass, new List<Texture2D>{content.Load<Texture2D>("sprites\\Grass")}},
                { ObjectType.Barrier, new List<Texture2D>{content.Load<Texture2D>("sprites\\BarrierLeft"),content.Load<Texture2D>("sprites\\BarrierMiddle"),content.Load<Texture2D>("sprites\\BarrierRight") }},
                { ObjectType.Tree, new List<Texture2D>{content.Load<Texture2D>("sprites\\Trees")}},
                { ObjectType.Knight, new List<Texture2D>{content.Load<Texture2D>("sprites\\Knight"), content.Load<Texture2D>("sprites\\Animations\\Knight-Left"), content.Load<Texture2D>("sprites\\Animations\\Knight-Right"), content.Load<Texture2D>("sprites\\Knight-Attack") }},
                { ObjectType.Archer, new List<Texture2D>{content.Load<Texture2D>("sprites\\Archer"), content.Load<Texture2D>("sprites\\Archer-Left"), content.Load<Texture2D>("sprites\\Archer-Right"), content.Load<Texture2D>("sprites\\Archer-Attack") }},
                { ObjectType.Horseman, new List<Texture2D>{content.Load<Texture2D>("sprites\\human-rider"), content.Load<Texture2D>("sprites\\human-rider"), content.Load<Texture2D>("sprites\\human-rider"), content.Load<Texture2D>("sprites\\human-rider") }},
                { ObjectType.Mage, new List<Texture2D>{content.Load<Texture2D>("sprites\\human-mage"), content.Load<Texture2D>("sprites\\human-mage-left"), content.Load<Texture2D>("sprites\\human-mage-right"), content.Load<Texture2D>("sprites\\human-mage-Attack") }},
                { ObjectType.Human1Hero, new List<Texture2D>{content.Load<Texture2D>("sprites\\human-hero"), content.Load<Texture2D>("sprites\\human-hero-left"), content.Load<Texture2D>("sprites\\human-hero-right"), content.Load<Texture2D>("sprites\\human-hero-Attack") }},
                { ObjectType.Orc1Hero, new List<Texture2D>{content.Load<Texture2D>("sprites\\Orcs\\Orc-hero"),content.Load<Texture2D>("sprites\\Orcs\\orc-hero-left"), content.Load<Texture2D>("sprites\\Orcs\\orc-hero-right"), content.Load<Texture2D>("sprites\\Orc-hero") }},
                { ObjectType.Dwarf1Hero, new List<Texture2D>{content.Load<Texture2D>("sprites\\dwarf\\dwarf-hero"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-hero-left"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-hero-right"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-hero-right") }},
                { ObjectType.Puncher, new List<Texture2D>{content.Load<Texture2D>("sprites\\Orcs\\Orc-Warrior"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Warrior-Left"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Warrior-Right"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Warrior-Attack") }},
                { ObjectType.Slingshot, new List<Texture2D>{content.Load<Texture2D>("sprites\\Orcs\\Orc-Archer"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Archer-Left"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Archer-Right"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Archer-Attack") }},
                { ObjectType.Shaman, new List<Texture2D>{content.Load<Texture2D>("sprites\\Orcs\\Orc-Mage"), content.Load<Texture2D>("sprites\\Orcs\\orc-mage-left"), content.Load<Texture2D>("sprites\\Orcs\\orc-mage-right"), content.Load<Texture2D>("sprites\\Orc-Mage") }},
                { ObjectType.Troll, new List<Texture2D>{content.Load<Texture2D>("sprites\\Orcs\\Orc-troll"), content.Load<Texture2D>("sprites\\Orcs\\Orc-troll-Left"), content.Load<Texture2D>("sprites\\Orcs\\Orc-troll"), content.Load<Texture2D>("sprites\\Orcs\\Orc-troll-Attack") }},
                { ObjectType.Axeman, new List<Texture2D>{content.Load<Texture2D>("sprites\\dwarf\\dwarf-axe"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-axe-left"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-axe-right"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-axe-attack") }},
                { ObjectType.Arbalist, new List<Texture2D>{content.Load<Texture2D>("sprites\\dwarf\\dwarf-archer"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-archer-left"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-archer-right"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-archer-attack") }},
                { ObjectType.Wolf1Rider, new List<Texture2D>{content.Load<Texture2D>("sprites\\dwarf\\dwarf-wolf"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-wolf-left"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-wolf-right"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-wolf-attack") }},
                { ObjectType.Phalanx, new List<Texture2D>{content.Load<Texture2D>("sprites\\dwarf\\dwarf-phalanx"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-phalanx-left"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-phalanx-right"), content.Load<Texture2D>("sprites\\dwarf\\dwarf-phalanx-attack") }},
                { ObjectType.Builder, new List<Texture2D>{content.Load<Texture2D>("sprites\\Animations\\Builder"), content.Load<Texture2D>("sprites\\Animations\\Builder-Left"), content.Load<Texture2D>("sprites\\Animations\\Builder-Right"), content.Load<Texture2D>("sprites\\Animations\\Builder") }},
                { ObjectType.Mana1Source, new List<Texture2D>{content.Load<Texture2D>("sprites\\Mana") }},
                { ObjectType.Gold1Vein, new List<Texture2D>{content.Load<Texture2D>("sprites\\Gold") }},
                { ObjectType.Iron1Source, new List<Texture2D>{content.Load<Texture2D>("sprites\\Iron"),  content.Load<Texture2D>("sprites\\Wood")}},
                { ObjectType.Mine, new List<Texture2D>{content.Load<Texture2D>("sprites\\MineL1"), content.Load<Texture2D>("sprites\\MineL2"), content.Load<Texture2D>("sprites\\MineL3") }},
                { ObjectType.Main1Building, new List<Texture2D>{content.Load<Texture2D>("sprites\\main-building"), content.Load<Texture2D>("sprites\\Orcs\\Orc-Main") }},
                { ObjectType.House, new List<Texture2D>{content.Load<Texture2D>("sprites\\House"), content.Load<Texture2D>("sprites\\House1"), content.Load<Texture2D>("sprites\\House2") }},
                { ObjectType.Tower, new List<Texture2D>{
                    content.Load<Texture2D>("sprites\\Wall\\TowerAlone"), content.Load<Texture2D>("sprites\\Wall\\TowerBottom"), content.Load<Texture2D>("sprites\\Wall\\TowerLeft"), content.Load<Texture2D>("sprites\\Wall\\TowerRight"), 
                    content.Load<Texture2D>("sprites\\Wall\\TowerRightLeft"), content.Load<Texture2D>("sprites\\Wall\\TowerBottomLeft"), content.Load<Texture2D>("sprites\\Wall\\TowerBottomRight"), content.Load<Texture2D>("sprites\\Wall\\TowerBottomRightLeft"),}},
                { ObjectType.Military1Camp, new List<Texture2D>{content.Load<Texture2D>("sprites\\Armory"), content.Load<Texture2D>("sprites\\Armory-2"), content.Load<Texture2D>("sprites\\Armory-3") }},
                { ObjectType.Mage1Tower, new List<Texture2D>{content.Load<Texture2D>("sprites\\WizardTower") }},
                { ObjectType.Wall, new List<Texture2D>
                {
                    content.Load<Texture2D>("sprites\\Wall\\WallAlone"), content.Load<Texture2D>("sprites\\Wall\\WallBottom"), content.Load<Texture2D>("sprites\\Wall\\WallTop"), content.Load<Texture2D>("sprites\\Wall\\WallTopBottom"), content.Load<Texture2D>("sprites\\Wall\\WallLeftAndOrRight"),
                    content.Load<Texture2D>("sprites\\Wall\\WallTopLeft"), content.Load<Texture2D>("sprites\\Wall\\WallTopRight"), content.Load<Texture2D>("sprites\\Wall\\WallBottomRight"), content.Load<Texture2D>("sprites\\Wall\\WallBottomLeft"),
                    content.Load<Texture2D>("sprites\\Wall\\WallTopRightLeft"), content.Load<Texture2D>("sprites\\Wall\\WallBottomRightLeft"), content.Load<Texture2D>("sprites\\Wall\\WallTopBottomLeft"), content.Load<Texture2D>("sprites\\Wall\\WallTopBottomRight"), content.Load<Texture2D>("sprites\\Wall\\WallTopBottomRightLeft")
                }},
                { ObjectType.Gate, new List<Texture2D>
                {
                    content.Load<Texture2D>("sprites\\GateLeftRightClosed"), content.Load<Texture2D>("sprites\\GateLeftRightOpen"),
                    content.Load<Texture2D>("sprites\\GateUpDownClosed"), content.Load<Texture2D>("sprites\\GateUpDownOpen"), content.Load<Texture2D>("sprites\\gateNotWorking")
                }},
                { ObjectType.Rock, new List<Texture2D>{content.Load<Texture2D>("sprites\\Rock") }},
                { ObjectType.New1Building, new List<Texture2D>{content.Load<Texture2D>("sprites\\NewBuilding") }}
            };

            sProjectileTextures = new List<Texture2D>
            {
                content.Load<Texture2D>("sprites\\orcMageFireAttackF1"), content.Load<Texture2D>("sprites\\orcMageFireAttackF2"), sTextures[ObjectType.Iron1Source][1], sTextures[ObjectType.Iron1Source][0], sTextures[ObjectType.Gold1Vein][0], sTextures[ObjectType.Mana1Source][0],
                content.Load<Texture2D>("sprites\\SpecialActionMagic\\MagicBackground1"),content.Load<Texture2D>("sprites\\SpecialActionMagic\\MagicBackground2"),content.Load<Texture2D>("sprites\\SpecialActionMagic\\MagicBackground3"),
                content.Load<Texture2D>("sprites\\SpecialActionMagic\\MagicForeground1"),content.Load<Texture2D>("sprites\\SpecialActionMagic\\MagicForeground2"),content.Load<Texture2D>("sprites\\SpecialActionMagic\\MagicForeground3"),
                content.Load<Texture2D>("sprites\\Buff\\Up1"),content.Load<Texture2D>("sprites\\Buff\\Up2"),content.Load<Texture2D>("sprites\\Buff\\Up3"),content.Load<Texture2D>("sprites\\Buff\\Up4"), content.Load<Texture2D>("sprites\\Arrow"), content.Load<Texture2D>("sprites\\lighting"),
                content.Load<Texture2D>("sprites\\SwordAttack"), content.Load<Texture2D>("sprites\\Buff\\BuffShield1"),content.Load<Texture2D>("sprites\\Buff\\BuffShield2"),content.Load<Texture2D>("sprites\\Buff\\BuffShield3")
            };
        }

        public static void Draw(ScreenManager screenManager, InputData inputData)
        {
            mSpriteBatch.GraphicsDevice.Clear(Color.LightGray);
            mSpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            if (screenManager.mGameState == ScreenManager.State.Game)
            {
                PrepareGameMap(screenManager, inputData);
            }
            // Loops through all ScreenObjects in the current Stack, as long as a screen wants the one below it to be updated too.
            // Uses Layers to draw the upper screens first but still above the ones below. 
            var screenStack = screenManager.CurrentStack;
            var currentPosition = screenStack.Count - 1;
            var goLower = true;
            var screensToDraw = new List<int>();
            while (currentPosition > -1 && goLower)
            {
                goLower = screenStack[currentPosition].mDrawLower;
                screensToDraw.Insert(0, currentPosition);
                currentPosition -= 1;
            }

            mSpriteBatch.End();
            mSpriteBatch.Begin(samplerState: mHudScale >= 1.5f ? SamplerState.AnisotropicClamp : SamplerState.LinearWrap);
            foreach (var i in screensToDraw)
            {
                DrawScreenObject(screenStack[i], (float)i / screenStack.Count, inputData.mMouseData.mNextMouseState.Position, screenManager);
            }
            
            if (screenManager.mGameState == ScreenManager.State.Game)
            {
                // draws statistics, only if in game
                mSpriteBatch.DrawString(sFont,
                    DataStorage.mGameStatistics[true][ResourceType.Wood] + " Wood    " +
                    DataStorage.mGameStatistics[true][ResourceType.Iron] + " Iron    " +
                    DataStorage.mGameStatistics[true][ResourceType.Gold] + " Gold    " +
                    DataStorage.mGameStatistics[true][ResourceType.Mana] + " Mana    " +
                    "Population: " +
                    DataStorage.mGameStatistics[true][ResourceType.Population] + "/" +
                    DataStorage.mGameStatistics[true][ResourceType.MaxPopulation],
                    new Vector2(20,
                        (Camera.SideBarOffset.Y - sFont.MeasureString("W").Y/2 * CurrentScreenScale) /2),
                    Color.Black, 0, Vector2.Zero, CurrentScreenScale/2, SpriteEffects.None, 1);
            }

            mSpriteBatch.End();
        }

        private static void DrawParticles(ScreenManager screenManager, IReadOnlyCollection<Particle> particles, float tileSize, bool[,] toDraw, float scale)
        {
            if (particles.Count == 0)
            {
                return;
            }
            foreach (var particle in from particle in particles let pos = particle.mPosition where !(pos.X > toDraw.GetLength(0)) && !(pos.Y > toDraw.GetLength(1)) && pos.X>=0 && pos.Y >=0 && (toDraw[(int)pos.X, (int)pos.Y] || !mDrawFog) select particle)
            {
                if (particle.mCurrentTexture == -1)
                {
                    mSpriteBatch.FillRectangle(screenManager.mCamera.GetCameraOffset().X + tileSize * particle.mPosition.X, screenManager.mCamera.GetCameraOffset().Y + tileSize * (particle.mPosition.Y - 1), 1f * tileSize * particle.mSize.X, 1f * tileSize * particle.mSize.Y, particle.mColor);
                }
                else
                {
                    var texture = sProjectileTextures[particle.mCurrentTexture];
                    var center = new Vector2(texture.Width / 2f, texture.Height / 2f);
                    mSpriteBatch.Draw(texture, new Vector2( screenManager.mCamera.GetCameraOffset().X + tileSize * particle.mPosition.X, screenManager.mCamera.GetCameraOffset().Y + tileSize * (particle.mPosition.Y - 1))+center*scale, texture.Bounds, Color.White, particle.mRotation,
                        center, scale, SpriteEffects.None, 0f);
                }
            }
        }

        private static void DrawScreenObject(ScreenObject screenObject, float layer, Point mousePosition, ScreenManager screenManager)
        {
            var offset = screenObject.GetOffsetToCenterButtons(CurrentScreenScale);
            var backgroundRectangle = screenObject.GetBackgroundRectangle(CurrentScreenScale);
            mSpriteBatch.FillRectangle(backgroundRectangle, screenObject.mBackgroundColors[0], layer);
            backgroundRectangle = new Rectangle(backgroundRectangle.X + 4, backgroundRectangle.Y + 4, backgroundRectangle.Width - 8, backgroundRectangle.Height - 8); 
            if (screenManager.IsMainScreen)
            {
                var width = sTitleScreen.Width * CurrentScreenScale;
                var height = sTitleScreen.Height * CurrentScreenScale;
                mSpriteBatch.Draw(sTitleScreen, new Rectangle(
                    (int)((mGraphicsDeviceManager.GraphicsDevice.Viewport.Width - width) / 2f), (int)(backgroundRectangle.Y - height * 1.5f), (int)width, (int)height), Color.White);
                    
            }
            mSpriteBatch.FillRectangle(backgroundRectangle, screenObject.mBackgroundColors[1], layer);
            for (var i = 0; i < screenObject.GetButtons.Count; i++)
            {
                DrawButtonObject(screenObject.GetButtons[i], offset[i], (screenObject.GetButtons[i].mData.GetAlignedRectangle(mSpriteBatch.GraphicsDevice, CurrentScreenScale, offset[i]).Contains(mousePosition) && screenObject.GetButtons[i].mEffects.Count > 0), layer);
            }
        }

        private static void DrawButtonObject(Button button, Vector2 offset, bool hovering, float layer)
        {
            var alignedRectangle = button.mData.GetAlignedRectangle(mSpriteBatch.GraphicsDevice, CurrentScreenScale, offset);
            if (button.mDrawBackground)
            {
                mSpriteBatch.FillRectangle(alignedRectangle.X,
                    alignedRectangle.Y,
                    alignedRectangle.Width,
                    alignedRectangle.Height, hovering ? button.mHighlightColor[0] : button.mBackgroundColor[0], layer);
                mSpriteBatch.FillRectangle(alignedRectangle.X + 4,
                    alignedRectangle.Y + 4,
                    alignedRectangle.Width < 8 ? 0 : alignedRectangle.Width
                                                     - 8,
                    alignedRectangle.Height - 8, hovering ? button.mHighlightColor[1] : button.mBackgroundColor[1], layer);
            }

            var stringSize = sFont.MeasureString(button.mData.GetButtonText())/2;
            mSpriteBatch.DrawString(sFont, button.mData.GetButtonText(), new Vector2(alignedRectangle.X + (int)(((button.mData.GetRectangle().Width - stringSize.X) * CurrentScreenScale) / 2), 
                alignedRectangle.Y + (int)(((button.mData.GetRectangle().Height - stringSize.Y) * CurrentScreenScale) / 2)), Color.Black, 0f, Vector2.Zero, CurrentScreenScale/2, SpriteEffects.None, layer);
        }

        private static void PrepareGameMap(ScreenManager screenManager, InputData inputData)
        {
            var toDraw = new bool[GameMap.mWidth, GameMap.mHeight];
            foreach (var gameObjectsValue in DataStorage.mGameObjects.Values)
            {
                gameObjectsValue.GetAttributes().Item2.TryGetValue("Vision", out var tRange);
                if (tRange == 0 || !gameObjectsValue.mIsPlayer)
                {
                    continue;
                }
                for (var y = Math.Max(gameObjectsValue.mObjectPosition.Y - tRange -1, 0); y < Math.Min(gameObjectsValue.mObjectPosition.Y + tRange, GameMap.mHeight); y++)
                {
                    for (var x = Math.Max(gameObjectsValue.mObjectPosition.X - tRange, 0);
                        x < Math.Min(gameObjectsValue.mObjectPosition.X + tRange +1, GameMap.mWidth-1);
                        x++)
                    {
                        sObjectsDrawn.Remove(new Point(x, y));
                        toDraw[x, y] = true;
                    }
                }
            }
            AddPlayerPortals(toDraw);
            DrawGameMap(screenManager, toDraw, inputData);
        }

        private static Dictionary<Point, List<Point>> sRenderedPortals = new Dictionary<Point, List<Point>>();
        public static bool mDrawPaths = false;

        // Using Bresenham's line algorithm
        private static void AddPlayerPortals(bool[,] toDraw)
        {
            var portals = DataStorage.mAStar.mPortals[0];
            // Used to filter out closed Portals and to only compute each line once
            var newPortals = new Dictionary<Point, List<Point>>();
            foreach (var (entrance, exit) in portals)
            {
                if (sRenderedPortals.TryGetValue(entrance, out var oldLine))
                {
                    newPortals.Add(entrance, oldLine);

                    foreach (var point in oldLine)
                    {
                        DrawAroundPoint(point.X, point.Y, toDraw);
                    }

                    continue;
                }

                var direction = exit - entrance;
                var wasFlippedDegree = false;
                var wasFlippedAxis = false;
                if (direction.Y < 0)
                {
                    wasFlippedAxis = true;
                    direction.Y = -direction.Y;
                }
                if (direction.Y > direction.X)
                {
                    wasFlippedDegree = true;
                    (direction.X, direction.Y) = (direction.Y, direction.X);
                }

                var newM = 2 * direction.Y;
                var slopeErrorNew = newM - direction.X;

                var points = new List<Point>();
                for (int x = 0, y = 0; x <= direction.X; x++)
                {
                    points.Add(new Point(x, y));
                    
                    // Add slope to increment angle formed
                    slopeErrorNew += newM;

                    // Slope error reached limit, time to
                    // increment y and update slope error.
                    if (slopeErrorNew < 0)
                    {
                        continue;
                    }

                    y++;
                    slopeErrorNew -= 2 * direction.X;
                }

                if (points[^1].Y > direction.Y)
                {
                    points[^1] += new Point(0,-1);
                }

                var line = new List<Point>();
                foreach (var point in points)
                {
                    var y = point.Y;
                    var x = point.X;
                    if (wasFlippedDegree)
                    {
                        if (wasFlippedAxis)
                        {
                            line.Add(new Point(y + entrance.X, -x - 1 + entrance.Y));
                            DrawAroundPoint(y + entrance.X, -x - 1 + entrance.Y, toDraw);
                        }
                        else
                        {
                            line.Add(new Point(y + entrance.X, x - 1 + entrance.Y));
                            DrawAroundPoint(y + entrance.X, x - 1 + entrance.Y, toDraw);
                        }
                    }
                    else
                    {
                        if (wasFlippedAxis)
                        {
                            line.Add(new Point(x + entrance.X, -y - 1 + entrance.Y));
                            DrawAroundPoint(x + entrance.X, -y - 1 + entrance.Y, toDraw);
                        }
                        else
                        {
                            line.Add(new Point(x + entrance.X, y - 1 + entrance.Y));
                            DrawAroundPoint(x + entrance.X, y - 1 + entrance.Y, toDraw);
                        }
                    }
                }
                newPortals.Add(entrance, line);
            }

            sRenderedPortals = newPortals;

        }

        private static void DrawAroundPoint(int x, int y, bool[,] toDraw)
        {
            for (var i = -1; i < 2; i++)
            {
                if (y<1)
                {
                    continue;
                }
                for (var j = -1; j < 2; j++)
                {
                    toDraw[x+j, y+i] = true;
                }
            }
        }
        
        private static void DrawGameMap(
            ScreenManager screenManager,
            bool[,] toDraw,
            InputData inputData)
        {
            var scale = mScale * 0.5f;
            var (tileSize, tilePosition, startX, startY) = GetMapSizeAndPosition(screenManager, scale);
            var startXOnScreen = tilePosition.X;
            var objectsToDraw = new List<(GameObject, (int, int))>();
            sIsPaused = screenManager.mTestMap.mIsPaused;
            for (var y = startY; y < GameMap.mHeight; y++)
            {
                if (tilePosition.Y > mSpriteBatch.GraphicsDevice.Viewport.Height)
                {
                    break;
                }

                tilePosition.X = startXOnScreen;
                for (var x = startX; x < GameMap.mWidth; x++)
                {
                    if (tilePosition.X > mSpriteBatch.GraphicsDevice.Viewport.Width)
                    {
                        break;
                    }

                    var highlightColor = GetHighlightColor(screenManager, inputData, x, y, toDraw);

                    var tile = screenManager.mTestMap.mTiles[x, y];
                    var texture = sTextures[tile.mTileType];
                    mSpriteBatch.Draw(texture[GameMap.Tile.TileState],
                        tilePosition, texture[GameMap.Tile.TileState].Bounds,
                        highlightColor, 0f, Vector2.Zero, scale,
                        SpriteEffects.None, 0f);

                    
                    var objectDraw = DataStorage.GetObject(new Point(x, y + 1));
                    var objectExists = objectDraw != null;
                    
                    if (inputData.mMouseData.mMousePositionOnTile == new Vector2(x, y) && toDraw[x,y])
                    {
                        var buildMode = screenManager.mBuildMode;
                        buildMode ??= screenManager.mGodMode;
                        switch (buildMode)
                        {
                            case ObjectType.Mine:
                                if (objectExists && objectDraw.mName >= ObjectType.Iron1Source && objectDraw.mName <= ObjectType.Mana1Source)
                                {
                                    DrawChosenBuilding(buildMode.Value);

                                    tilePosition.X += tileSize;
                                    continue;
                                }
                                break;
                            case null:
                                break;
                            default:
                                if (!objectExists)
                                {
                                    DrawChosenBuilding(buildMode.Value);
                                }
                                break;
                        }
                    }

                    void DrawChosenBuilding(ObjectType objectType)
                    {
                        texture = sTextures[objectType];
                        mSpriteBatch.Draw(texture[0],
                            new Vector2(tilePosition.X,
                                tilePosition.Y - (objectType == ObjectType.Tower ? tileSize : 0)), texture[0].Bounds,
                            toDraw[x, y] || !mDrawFog ? Color.White : Color.Gray*0.7f, 0f, Vector2.Zero, scale,
                            SpriteEffects.None, 0f);
                    }

                    if (objectExists && !(!toDraw[x, y] && mDrawFog && objectDraw.mName != ObjectType.Barrier))
                    {
                        objectsToDraw.Add((objectDraw, (x, y)));
                    }

                    tilePosition.X += tileSize;
                }
                tilePosition.Y += tileSize;
            }

            DrawParticles(screenManager, Particle.mBackgroundParticles, tileSize, toDraw, scale);

            var (barrierPosition, barrierWidth) = GameMap.GetBarrierWidthAndPosition();
            if (screenManager.mPotionMode != null)
            {
                if (!(screenManager.mPotionMode.Value.Item1.mObjectPosition.X < barrierPosition &&
                      inputData.mMouseData.mMousePositionOnTile.X >= barrierPosition) && !(screenManager.mPotionMode.Value.Item1.mObjectPosition.X >= barrierPosition + barrierWidth && inputData.mMouseData.mMousePositionOnTile.X < barrierPosition + barrierWidth))
                {
                    var potion = screenManager.mPotionMode;
                    var spellCaster = potion.Value.Item1;
                    // Disable because it switches between suggestions
                    // ReSharper disable once PossibleNullReferenceException
                    var attackRange = ((AttackBehaviour)spellCaster.mActions.First(action => action is AttackBehaviour))
                        .GetAttackRange() + 1;
                    /*var attackRange =
                        ((AttackBehaviour) spellCaster.mActions.First(action => action is AttackBehaviour))
                        .GetAttackRange() + 4;*/
                    var distances = spellCaster.mObjectPosition.ToVector2() - inputData.mMouseData.mMousePositionOnTile;
                    if (Math.Abs(distances.X) < attackRange && Math.Abs(distances.Y - 1) < attackRange)
                    {
                        var range = potion.Value.Item2 switch
                        {
                            PotionType.Heal1Potion => Potion.HealRange,
                            PotionType.Damage1Potion => Potion.DamageRange,
                            PotionType.Speed1Potion => Potion.SpeedRange,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        var color = potion.Value.Item2 switch
                        {
                            PotionType.Heal1Potion => Color.YellowGreen,
                            PotionType.Damage1Potion => Color.Red,
                            PotionType.Speed1Potion => Color.Blue,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        mSpriteBatch.FillRectangle(
                            (inputData.mMouseData.mMousePositionOnTile.X - range) * tileSize +
                            screenManager.mCamera.GetCameraOffset().X,
                            (inputData.mMouseData.mMousePositionOnTile.Y - range) * tileSize +
                            screenManager.mCamera.GetCameraOffset().Y,
                            (1 + range * 2) * tileSize,
                            (1 + range * 2) * tileSize,
                            color * 0.5f);
                    }
                }
            }

            foreach (var texture in sObjectsDrawn)
            {
                tilePosition = new Vector2(screenManager.mCamera.GetCameraOffset().X + tileSize * texture.Key.X, screenManager.mCamera.GetCameraOffset().Y + tileSize *
                    (texture.Key.Y - (texture.Value.Height > sTextures[ObjectType.Grass][0].Height ? 1 : 0)));
                mSpriteBatch.Draw(texture.Value,
                    tilePosition,
                    texture.Value.Bounds,
                    Color.Gray,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0.1f);
            }

            foreach (var (gameObject, position) in objectsToDraw)
            {
                tilePosition = new Vector2(screenManager.mCamera.GetCameraOffset().X + tileSize * position.Item1, screenManager.mCamera.GetCameraOffset().Y + tileSize * position.Item2);
                DrawGameObjects(
                    DataStorage.mUnitAnimationOffset.TryGetValue(new Point(position.Item1, position.Item2 + 1), out var offset)
                        ? new Vector2(tilePosition.X + offset.X * tileSize, tilePosition.Y + offset.Y * tileSize)
                        : new Vector2(tilePosition.X, tilePosition.Y),
                    gameObject, scale, tileSize, screenManager);
            }

            DrawParticles(screenManager, Particle.mForegroundParticles, tileSize, toDraw, scale);
        }

        private static (float tileSize, Vector2 tilePosition, int startX, int startY) GetMapSizeAndPosition(ScreenManager screenManager, float scale)
        {
            // How big tiles should be displayed with the current zoom
            var tileSize = GameMap.mStandardTileSize * scale;
            var tilePosition = new Vector2(screenManager.mCamera.GetCameraOffset().X, screenManager.mCamera.GetCameraOffset().Y);

            //Determine where to start, to skip all tiles not on screen
            var startY = 0;
            while (tilePosition.Y + tileSize < 0)
            {
                startY += 1;
                tilePosition.Y += tileSize;
            }
            var startX = 0;
            while (tilePosition.X + tileSize < 0)
            {
                startX += 1;
                tilePosition.X += tileSize;
            }
            return (tileSize, tilePosition, startX, startY);
        }

        private static Color GetHighlightColor(ScreenManager screenManager, InputData inputData, int x, int y, bool[,] toDraw)
        {
            var highlightColor = Color.White;
            if (!screenManager.mTestMap.mIsPaused)
            {
                if (inputData.mMouseData.mIsSelecting && inputData.mMouseData.mSelection.Contains(x, y))
                {
                    highlightColor = Color.Pink;
                }
                if (inputData.mMouseData.mMousePositionOnTile == new Vector2(x, y))
                {
                    highlightColor = Color.Orange;
                }
            }

            if (DataStorage.mAStar.mPortals[0].ContainsKey(new Point(x, y + 1)) || DataStorage.mAStar.mPortals[0].ContainsValue(new Point(x, y + 1)) || 
                DataStorage.mAStar.mPortals[1].ContainsKey(new Point(x, y + 1)) || DataStorage.mAStar.mPortals[1].ContainsValue(new Point(x, y + 1)))
            {
                highlightColor = Color.Orange;
            }

            if (!toDraw[x, y] && mDrawFog)
            {
                highlightColor = Color.Gray * 0.7f;
            }

            return highlightColor;
        }

        private static void DrawGameObjects(Vector2 position, GameObject gameObject, float scale, float tileSize, ScreenManager screenManager)
        {
            var textures = sTextures[gameObject.mName];
            if (gameObject.GetCurrentSprite() <= -1)
            {
                return;
            }

            var texture = textures[gameObject.GetCurrentSprite()];
            if ((gameObject.mName < ObjectType.Main1Building) && gameObject.mObjectEvents.TryGetValue(EventType.MoveEvent, out var events) && events.Count != 0 && gameObject.mIsMoving && !sIsPaused)
            {
                var progress = gameObject.UpdateProgress()  * 2;
                if (progress > 1)
                {
                    progress -= 1;
                }
                if (progress >= 0.8f || progress <= 0.3)
                {
                    texture = textures[0];
                }
                else if (progress >= 0.5f)
                {
                    texture = textures[1];
                }
                else
                {
                    texture = textures[2];
                }
            }

            var drawPosition = new Vector2(position.X,
                position.Y - (gameObject.mName == ObjectType.Tower ? tileSize : 0));

            if (GameObjectManagement.mSelectedObjects.Contains(gameObject.mObjectId))
            {
                var highlightColor = GameObjectManagement.mSelectedObjects[GameObjectManagement.mCurrentSelectedUnit] == gameObject.mObjectId ? Color.Black * 0.4f : Color.Black * 0.2f;
                mSpriteBatch.FillRectangle(position.X, position.Y, GameMap.mStandardTileSize * scale,  GameMap.mStandardTileSize * scale, highlightColor);
            }

            var effect = gameObject.mIsPlayer || gameObject.mName > ObjectType.Builder ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (gameObject.mName > ObjectType.Builder && gameObject.mName != ObjectType.Barrier && (!gameObject.mIsPlayer || gameObject.mName >= ObjectType.Grass))
            {
                sObjectsDrawn[gameObject.mObjectPosition - new Point(0, 1)] = texture;
            }

            //mSpriteBatch.DrawString(sFont, gameObject.mObjectId+"", drawPosition, Color.White);

            if (mDrawPaths)
            {
                if (gameObject.mObjectEvents.TryGetValue(EventType.MoveEvent, out var moveEvents) && moveEvents.Count > 0)
                {
                    var theMove = (MoveEvent)moveEvents[0];
                    var path = theMove.mPath.ToList();
                    path.Insert(0, gameObject.mObjectPosition);
                    path.Insert(1, theMove.mDestination);
                    var path2 = path.ToList();
                    path.RemoveAt(path.Count-1);
                    var i = 1;
                    foreach (var point in path)
                    {
                        mSpriteBatch.DrawLine(tileSize * (point.ToVector2()+new Vector2(0.5f,-0.5f)) + screenManager.mCamera.GetCameraOffset(),
                            tileSize * (path2[i].ToVector2() + new Vector2(0.5f, -0.5f)) + screenManager.mCamera.GetCameraOffset(), i == 1 ? Color.Red : Color.White);
                        i++;
                    }
                }
            }

            mSpriteBatch.Draw(texture,
                drawPosition,
                texture.Bounds,
                Color.White,
                0f,
                Vector2.Zero,
                scale,
                effect,
                0.1f);
            if (gameObject.mName <= ObjectType.Builder)
            {
                mSpriteBatch.Draw(texture,
                    drawPosition,
                    texture.Bounds,
                    (gameObject.mIsPlayer ? Color.Blue : Color.Red) * 0.4f,
                    0f,
                    Vector2.Zero,
                    scale,
                    effect,
                    0.1f);
            }
            var getAttacked = gameObject.mActions.FirstOrDefault(action => action is GetAttacked);

            if (getAttacked is GetAttacked attacked && (attacked.GetDelay() != 0 || Math.Abs(attacked.GetAnimationHealthPoints() - attacked.GetMaxHealthPoints()) > 0.5))
            {
                var maxHealth = attacked.GetMaxHealthPoints();
                var animationHealth = attacked.GetAnimationHealthPoints();

                var color = gameObject.mIsPlayer ? Color.Blue : Color.Red;

                DrawProgressBar(animationHealth / maxHealth, Color.White,  color, position, scale);
            }
            else if (gameObject.mName == ObjectType.Military1Camp || gameObject.mName == ObjectType.Main1Building)
            {
                var buildUnitAction = (BuildUnit)gameObject.mActions.FirstOrDefault(action => action is BuildUnit);
                var maxCooldown = gameObject.mName == ObjectType.Military1Camp ? buildUnitAction!.GetCooldown() : buildUnitAction!.GetCooldownMain();
                var timeToWait = buildUnitAction.GetTimeToWait();
                if (timeToWait + 1 == 0)
                {
                    return;
                }
                var buildUnitProgress = maxCooldown - timeToWait;

                DrawProgressBar(buildUnitProgress / maxCooldown, Color.White, Color.Gray, position, scale);
            }
            else if (gameObject.mName == ObjectType.New1Building)
            {
                var constructBuildingAction = (ConstructBuilding)gameObject.mActions.FirstOrDefault(action => action is ConstructBuilding);
                var timeToWait = constructBuildingAction!.ReturnProgress();

                DrawProgressBar(timeToWait / 100f, Color.White, Color.Gray, position, scale);
            }
            else if (gameObject.mName == ObjectType.Mine)
            {
                var gatherResourceEvent = ((GatherResourceEvent)gameObject.mObjectEvents.FirstOrDefault(action => action.Value.Count > 0 && action.Value[0] is GatherResourceEvent)!.Value[0]);
                var gatherResourceAction = (GatherResource)gameObject.mActions.FirstOrDefault(action => action is GatherResource);
                var current = gatherResourceEvent.mProgress;
                var max = gatherResourceAction!.mTimeNeeded;

                DrawProgressBar(current / max, Color.White, Color.Gray, position, scale);
            }
        }
        
        private static void DrawProgressBar(float progress, Color colorBackground, Color colorForeground, Vector2 position, float scale)
        {
            position.Y -= GameMap.mStandardTileSize * scale * 0.1f;
            mSpriteBatch.FillRectangle(position.X, position.Y, GameMap.mStandardTileSize * scale, GameMap.mStandardTileSize * scale / 10, colorBackground);

            mSpriteBatch.FillRectangle(position.X, position.Y, GameMap.mStandardTileSize * scale * progress, GameMap.mStandardTileSize * scale / 10, colorForeground);
        }
    }
}