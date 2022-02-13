#nullable enable
using System.Collections.Generic;
using System.Linq;
using LOB.Classes.Data;
using LOB.Classes.Managers;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Objects;
using LOB.Classes.Rendering;
using LOB.Classes.Screen.Buttons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LOB.Classes.Screen
{
    /// <summary>
    /// A <see cref="ScreenObject"/> represents a layer of <see cref="Button"/>s, with all information needed to Update and Draw all layers below it.
    /// </summary>
    internal sealed class ScreenObject
    {
        public readonly bool mUpdateLower;
        public readonly int mScreenId;
        public readonly bool mDrawLower;
        public readonly List<Button> mButtons;
        private readonly bool mDrawHorizontalCentered;
        private readonly bool mDrawVerticalCentered;
        private Rectangle? mBackgroundRectangle;
        public Color[] mBackgroundColors;

        internal readonly Color[] mStandardColor = { Color.Gray, Color.DarkGray };
        internal readonly Color[] mSeeThroughColor = { Color.Gray * 0.5f, Color.DarkGray * 0.5f };

        private Button? mAchievementButton;
        private float mAchievementTime;
        public List<Button> GetButtons
        {
            get
            {
                var buttons = mButtons.ToList();
                if (mAchievementButton != null)
                {
                    buttons.Add(mAchievementButton);
                }
                return buttons;
            }
        }

        /// <summary>
        /// A <see cref="ScreenObject"/> represents a layer of <see cref="Button"/>s, with all information needed to Update and Draw all layers below it.
        /// </summary>
        /// <param name="buttons">A <see cref="List{Button}"/> of <see cref="Button"/>s.</param>
        /// <param name="drawLower">If true, the layer below this one will also be drawn.</param>
        /// <param name="updateLower">If true, the layer below this one will also be updated.</param>
        /// <param name="drawHorizontalCentered">If true, the <see cref="mBackgroundRectangle"/> and <see cref="Button"/>s will be centered, by adding half of the windows width.</param>
        /// <param name="drawVerticalCentered">If true, the <see cref="mBackgroundRectangle"/> and <see cref="Button"/>s will be centered, by adding half of the windows height.</param>
        /// <param name="screenId">PopUpScreens should have a number >= 1, normal screens that stack and don't replace each other should use 0.</param>
        /// <param name="backgroundRectangle">If a Rectangle is given, its area will be drawn. It is affected by <see cref="drawVerticalCentered"/> and <see cref="drawHorizontalCentered"/>.</param>
        /// <param name="backgroundColors"></param>
        /// <param name="seeThroughColors"></param>
        public ScreenObject(List<Button> buttons,
            bool drawLower,
            bool updateLower,
            bool drawHorizontalCentered,
            bool drawVerticalCentered,
            int screenId,
            Rectangle? backgroundRectangle = null,
            Color[]? backgroundColors = null,
            Color[]? seeThroughColors = null)
        {
            mButtons = buttons;
            mDrawLower = drawLower;
            mUpdateLower = updateLower;
            mDrawHorizontalCentered = drawHorizontalCentered;
            mDrawVerticalCentered = drawVerticalCentered;
            mScreenId = screenId;
            mBackgroundRectangle = backgroundRectangle;
            mBackgroundColors = mStandardColor;
            if (backgroundColors != null)
            {
                mBackgroundColors = backgroundColors;
                mStandardColor = backgroundColors;
            }

            if (seeThroughColors != null)
            {
                mSeeThroughColor = seeThroughColors;
            }
        }

        /// <summary>
        /// Updates the <see cref="Button"/>s.
        /// </summary>
        /// <param name="inputData">An <see cref="InputData"/> object.</param>
        /// <returns>Returns the return value from <see cref="Button.Use"/> of the first <see cref="Button"/> at <see cref="InputData"/>.<see cref="MouseState.Position"/>, if <see cref="InputData"/>.<see cref="MouseState.LeftButton"/> was Released</returns>
        public List<(ButtonAction, object)> Update(InputData inputData)
        {
            if (mScreenId == -3) // HUD
            {
                if (mAchievementButton != null)
                {
                    var amount = DataStorage.mGameTime.ElapsedGameTime.Milliseconds /
                                 IAction.MillisecondsPerSecond;
                    mAchievementTime += amount;
                    if (mAchievementTime > 3)
                    {
                        mAchievementButton.mData.MoveRectangle(new Vector2(Camera.SideBarOffset.X - ((mAchievementTime-3) / 2) * 400, (int)Camera.SideBarOffset.Y / Renderer.CurrentScreenScale));
                    }
                    if (mAchievementTime > 5)
                    {
                        mAchievementButton = null;
                    }
                }
                else if (Achievements.sNewAchievements.Count != 0)
                {
                    var text = Achievements.sNewAchievements[0];
                    Achievements.sNewAchievements.Remove(text);
                    mAchievementButton =
                        new Button(new ButtonData(
                                new Rectangle((int)(Camera.SideBarOffset.X * Renderer.mHudScale), (int)(Camera.SideBarOffset.Y / Renderer.CurrentScreenScale), 400, 200),
                                text.Item1 + "\n" + text.Item2),
                            new List<IButtonEffect>());
                    mAchievementTime = 0;
                }
            }

            if (inputData.mMouseData.mPrevMouseState.LeftButton != ButtonState.Pressed ||
                inputData.mMouseData.mNextMouseState.LeftButton != ButtonState.Released)
            {
                return new List<(ButtonAction, object)>{(ButtonAction.None, null)!};
            }
            var graphicsDevice = Renderer.mGraphicsDeviceManager.GraphicsDevice;
            
            var offset = GetOffsetToCenterButtons(Renderer.CurrentScreenScale);
            for (var i = 0; i < mButtons.Count; i++)
            {
                if (!mButtons[i].mData.GetAlignedRectangle(graphicsDevice, Renderer.CurrentScreenScale, offset[i]).Contains(inputData.mMouseData.mNextMouseState.Position))
                {
                    continue;
                }

                return mButtons[i].Use();
            }
            return new List<(ButtonAction, object)> { (ButtonAction.None, null)! };
        }

        /// <summary>
        /// If <see cref="mBackgroundRectangle"/>'s x and/or y are -1, the <see cref="mBackgroundRectangle"/> will be stuck to the side/bottom, otherwise x and y are determined through <see cref="mDrawHorizontalCentered"/> and <see cref="mDrawVerticalCentered"/>
        /// </summary>
        /// <param name="currentScreenScale"></param>
        /// <returns>Returns the <see cref="mBackgroundRectangle"/> of this <see cref="ScreenObject"/>, adjusted to given parameters.</returns>
        public Rectangle GetBackgroundRectangle(float currentScreenScale)
        {
            var graphicsDevice = Renderer.mGraphicsDeviceManager.GraphicsDevice;
            var offset = new Vector2(0, 0);
            if (mBackgroundRectangle == null)
            {
                // -5 to ensure it is not visible on the screen.
                return new Rectangle(-5, -5, 0, 0);
            }
            if (mDrawHorizontalCentered)
            {
                offset.X = (graphicsDevice.Viewport.Width - mBackgroundRectangle.Value.Width * currentScreenScale) / 2;
            }

            if (mDrawVerticalCentered)
            {
                offset.Y = (graphicsDevice.Viewport.Height - mBackgroundRectangle.Value.Height * currentScreenScale) / 2;
            }

            var rect = new Rectangle(0, 0, 0, 0);
            if (mBackgroundRectangle != null)
            {
                rect = (Rectangle)mBackgroundRectangle!;
            }

            var x = (rect.X == -1 ? Renderer.mGraphicsDeviceManager.GraphicsDevice.Viewport.Width - rect.Width * currentScreenScale : 0) +
                    (int)offset.X;
            var y = (rect.Y == -1 ? Renderer.mGraphicsDeviceManager.GraphicsDevice.Viewport.Height - rect.Height * currentScreenScale : 0) +
                    (int)offset.Y;
            return new Rectangle((int)x, (int)y, (int)(rect.Width * currentScreenScale), (int)(rect.Height * currentScreenScale));
        }

        /// <summary>
        /// Computes the offset required so that each <see cref="Button"/> is centered according to <see cref="mDrawHorizontalCentered"/> and <see cref="mDrawVerticalCentered"/>
        /// </summary>
        /// <param name="currentScreenScale"></param>
        /// <returns>An array with individual x offsets in case some buttons are wider, and the same y offset so that all buttons have the distance given when creating it.</returns>
        public Vector2[] GetOffsetToCenterButtons(float currentScreenScale)
        {
            var graphicsDevice = Renderer.mGraphicsDeviceManager.GraphicsDevice;
            var offset = new Vector2[GetButtons.Count];
            var lowestY = 0f;
            var highestY = 0f;
            for (var i = 0; i < GetButtons.Count; i++)
            {
                offset[i] = new Vector2(0, 0);
                if (mDrawHorizontalCentered)
                {
                    offset[i].X = (graphicsDevice.Viewport.Width - GetButtons[i].mData.GetRectangle().Width * currentScreenScale) / 2;
                }

                if (!mDrawVerticalCentered)
                {
                    continue;
                }

                var newLow = GetButtons[i].mData.GetRectangle().Y * currentScreenScale;
                lowestY = lowestY < newLow ? lowestY : newLow;
                var newHigh = GetButtons[i].mData.GetRectangle().Y * currentScreenScale + GetButtons[i].mData.GetRectangle().Height * currentScreenScale;
                highestY = highestY > newHigh ? highestY : newHigh;
            }

            if (!mDrawVerticalCentered)
            {
                return offset;
            }

            var height = highestY - lowestY;
            var offsetY = (graphicsDevice.Viewport.Height - height) / 2;
            for (var i = 0; i < offset.Length; i++)
            {
                offset[i].Y = offsetY;
            }

            return offset;
        }
    }
}