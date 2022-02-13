using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LOB.Classes.Managers
{
    internal sealed class InputData
    {
        public readonly MouseData mMouseData;
        public readonly List<Keys> mReleasedKeys;
        public readonly List<Keys> mDownKeys;

        public InputData(List<Keys> releasedKeys, List<Keys> downKeys, MouseData mouseData)
        {
            mReleasedKeys = releasedKeys;
            mDownKeys = downKeys;
            mMouseData = mouseData;
        }

        internal sealed class MouseData
        {
            public Rectangle mSelection;
            public readonly bool mNewSelection;
            public readonly bool mIsSelecting;
            public readonly MouseState mPrevMouseState;
            public readonly MouseState mNextMouseState;
            public readonly Vector2 mMousePositionOnTile;

            public MouseData(Rectangle selection, bool newSelection, bool isSelecting, MouseState prevMouseState, MouseState nextMouseState, Vector2 mousePositionOnTile)
            {
                mSelection = selection;
                mNewSelection = newSelection;
                mIsSelecting = isSelecting;
                mPrevMouseState = prevMouseState;
                mNextMouseState = nextMouseState;
                mMousePositionOnTile = mousePositionOnTile;
            }
        }
    }
}