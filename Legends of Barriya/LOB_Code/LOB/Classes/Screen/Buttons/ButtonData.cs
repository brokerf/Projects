using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LOB.Classes.Screen.Buttons
{
    /// <summary>
    /// Holds the needed Data of a Button.
    /// </summary>
    internal sealed class ButtonData
    {
        private readonly ChangingRectangle mChangingBackgroundRectangle;
        private Rectangle mBackgroundRectangle;
        public string mButtonText;
        private readonly bool mAlignRight;
        private readonly bool mAlignBottom;


        /// <summary>
        /// Holds the needed Data of a Button.
        /// </summary>
        /// <param name="backgroundRectangle">This defines the area of the Button which is used to draw its background and center text.</param>
        /// <param name="text">Text to be displayed. \n and other escape characters work.</param>
        /// <param name="alignRight">When true, <see cref="GetAlignedRectangle"/> will return Window <see cref="Viewport.Width"/>+<see cref="Rectangle.X"/> instead of <see cref="Rectangle.X"/>, so <see cref="Rectangle.X"/> = -200 will be drawn 200 pixels from the right.</param>
        /// <param name="alignBottom">When true, <see cref="GetAlignedRectangle"/> will return Window <see cref="Viewport.Height"/>+<see cref="Rectangle.Y"/> instead of <see cref="Rectangle.Y"/>, so <see cref="Rectangle.Y"/> = -200 will be drawn 200 pixels from the bottom.</param>
        internal ButtonData(Rectangle backgroundRectangle, string text, bool alignRight = false, bool alignBottom = false)
        {
            mBackgroundRectangle = backgroundRectangle;
            mButtonText = text;
            mAlignRight = alignRight;
            mAlignBottom = alignBottom;
        }

        internal ButtonData(ChangingRectangle backgroundRectangle, string text, bool alignRight = false, bool alignBottom = false)
        {
            mChangingBackgroundRectangle = backgroundRectangle;
            mButtonText = text;
            mAlignRight = alignRight;
            mAlignBottom = alignBottom;
        }

        private Func<string> mTextFunction;

        public void SetChangingText(Func<string> textFunction)
        {
            mTextFunction = textFunction;
        }

        public string GetButtonText()
        {
            return mTextFunction != null ? mTextFunction.Invoke() : mButtonText;
        }

        /// <summary>
        /// Returns the <see cref="mBackgroundRectangle"/> of the Button, adjusted to <see cref="mAlignRight"/> and <see cref="mAlignBottom"/>, so that the Rectangle may be attached to the right and/or bottom of the window, meaning it will always have the same distance to the bottom/right, even when the Window size changes.
        /// </summary>
        /// <param name="graphicsDevice">A <see cref="GraphicsDevice"/></param>
        /// <param name="scale"></param>
        /// <param name="offset">If not null, this will be added to the <see cref="Rectangle.X"/> and <see cref="Rectangle.Y"/> values.</param>
        /// <returns>The adjusted Rectangle.</returns>
        public Rectangle GetAlignedRectangle(GraphicsDevice graphicsDevice, float scale, Vector2 offset = default)
        {
            var x = (int)(GetRectangle().X * scale);
            var y = (int)(GetRectangle().Y * scale);
            var width = GetRectangle().Width * scale;
            var height = GetRectangle().Height * scale;
            if (mAlignRight)
            {
                x = graphicsDevice.Viewport.Width + x;
            }

            if (mAlignBottom)
            {
                y = graphicsDevice.Viewport.Height + y;
            }

            x += (int)offset.X;
            y += (int)offset.Y;

            return new Rectangle(x, y, (int)width, (int)height);
        }

        /// <summary>
        /// Returns the <see cref="mBackgroundRectangle"/> as given on creation.
        /// </summary>
        /// <returns>The basic Rectangle</returns>
        public Rectangle GetRectangle()
        {
            return mChangingBackgroundRectangle?.GetRectangle()?? mBackgroundRectangle;
        }

        public void MoveRectangle(Vector2 newPosition)
        {
            mBackgroundRectangle.X = (int)newPosition.X;
            mBackgroundRectangle.Y = (int)newPosition.Y;
        }
    }
}