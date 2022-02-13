#nullable enable
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LOB.Classes.Screen.Buttons
{
    /// <summary>
    /// A Button.
    /// </summary>
    internal sealed class Button
    {
        internal readonly List<IButtonEffect> mEffects;
        internal readonly ButtonData mData;
        internal bool mDrawBackground;
        internal Color[] mBackgroundColor;
        internal readonly Color[] mHighlightColor = {Color.DarkOrange, Color.Orange};
        internal readonly Color[] mStandardColor = {Color.Gray, Color.DarkGray};
        internal readonly Color[] mSeeThroughColor = {Color.Gray*0.5f, Color.DarkGray * 0.5f};

        // basically constants but Colors cant be constants
        internal static readonly Color sDarkerGreen = new Color(130, 200, 47, 255);
        internal static readonly Color sLighterGreen = new Color(130, 170, 67, 255);

        /// <summary>
        /// A Button.
        /// </summary>
        /// <param name="data">A <see cref="ButtonData"/> Object.</param>
        /// <param name="effects">All <see cref="IButtonEffect"/>s that should be executed when the button is clicked.</param>
        /// <param name="drawBackground">false if the <see cref="Button"/> should be drawn without a Background, can be used to add plain text.</param>
        /// <param name="backgroundColor"></param>
        /// <param name="seeThroughColor"></param>
        /// <param name="highlightColor"></param>
        public Button(ButtonData data, List<IButtonEffect> effects, bool drawBackground =
            true, Color[]? backgroundColor = null, Color[]? seeThroughColor = null, Color[]? highlightColor = null)
        {
            mData = data;
            mEffects = effects;
            mDrawBackground = drawBackground;
            mBackgroundColor = mStandardColor;
            if (backgroundColor != null)
            {
                mBackgroundColor = backgroundColor;
                mStandardColor = backgroundColor;
            }
            if (seeThroughColor != null)
            {
                mSeeThroughColor = seeThroughColor;
            }
            if (highlightColor != null)
            {
                mHighlightColor = highlightColor;
            }
        }

        /// <summary>
        /// Should be called when the Button is supposed to be clicked.
        /// </summary>
        /// <returns>A <see cref="List{ButtonAction}"/> of <see cref="ButtonAction"/> types and their parameters.</returns>
        public List<(ButtonAction, object)> Use()
        {
            return mEffects.Select(buttonEffect => buttonEffect.Use()).ToList();
        }
    }
}