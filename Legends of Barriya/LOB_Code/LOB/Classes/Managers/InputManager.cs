using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LOB.Classes.Managers
{
    internal sealed class InputManager
    {
        private MouseState mPrevMouseState;
        private MouseState mNextMouseState = Mouse.GetState();
        private KeyboardState mPrevKeyboardState;
        private KeyboardState mNextKeyboardState = Keyboard.GetState();
        private Vector2 mWhichTileClicked = new Vector2(0, 0);
        private Vector2 mWhichTileReleased = new Vector2(0, 0);
        private bool mWasSelecting;
        private bool mIsSelecting;
        
        public InputData UpdateInput(bool isClickable, Vector2 mousePositionOnTile)
        {
            mPrevKeyboardState = mNextKeyboardState;
            mNextKeyboardState = Keyboard.GetState();
            mPrevMouseState = mNextMouseState;
            mNextMouseState = Mouse.GetState();
            var (released, down) = HandleInput(isClickable, mousePositionOnTile);
            var newSelection = !mIsSelecting && mWasSelecting;
            return new InputData(released, down, new InputData.MouseData(
                    GetSelectionRectangle(mousePositionOnTile),
                    newSelection,
                    mIsSelecting,
                    mPrevMouseState,
                    mNextMouseState,
                    mousePositionOnTile));
        }

        //Returns Rectangle with the area selected in the last selection.
        private Rectangle GetSelectionRectangle(Vector2 position)
        {
            if (mIsSelecting)
            {
                var startX = (int)Math.Min(mWhichTileClicked.X, position.X);
                var startY = (int)Math.Min(mWhichTileClicked.Y, position.Y);
                var width = (int)Math.Max(mWhichTileClicked.X, position.X) + 1 - startX;
                var height = (int)Math.Max(mWhichTileClicked.Y, position.Y) + 1 - startY;
                return new Rectangle(startX, startY, width, height);
            }
            else
            {
                var startX = (int)Math.Min(mWhichTileClicked.X, mWhichTileReleased.X);
                var startY = (int)Math.Min(mWhichTileClicked.Y, mWhichTileReleased.Y);
                var width = (int)Math.Max(mWhichTileClicked.X, mWhichTileReleased.X) + 1 - startX;
                var height = (int)Math.Max(mWhichTileClicked.Y, mWhichTileReleased.Y) + 1 - startY;
                return new Rectangle(startX, startY, width, height);
            }
        }

        private (List<Keys>, List<Keys>) HandleInput(bool isClickable, Vector2 position)
        {
            var down = mNextKeyboardState.GetPressedKeys().ToList();
            var released = (from pressedKey in mPrevKeyboardState.GetPressedKeys() where mNextKeyboardState.IsKeyUp(pressedKey) select (pressedKey)).ToList();

            mWasSelecting = mIsSelecting;
            if (mPrevMouseState.LeftButton != mNextMouseState.LeftButton && isClickable)
            {
                LeftMouseButtonChanged(mNextMouseState.LeftButton, position);
            }

            return (released, down);
        }

        private void LeftMouseButtonChanged(ButtonState e, Vector2 position)
        {
            if (e == ButtonState.Released)
            {
                mIsSelecting = false;
                mWasSelecting = true;
                mWhichTileReleased = position;
            }
            else
            {
                mWhichTileClicked = position;
                mIsSelecting = true;
            }
        }
    }
}