using System;
using Microsoft.Xna.Framework;

namespace LOB.Classes.Screen.Buttons
{
    internal sealed class ChangingRectangle
    {
        private readonly Func<object, int> mX;
        private readonly Func<object, int> mY;
        private readonly Func<object, int> mWidth;
        private readonly Func<object, int> mHeight;
        private readonly object mDataSupplier;

        public ChangingRectangle(Func<object, int> x, Func<object, int> y, Func<object, int> width, Func<object, int> height, object dataSupplier)
        {
            mX = x;
            mY = y;
            mWidth = width;
            mHeight = height;
            mDataSupplier = dataSupplier;
        }

        public Rectangle GetRectangle()
        {
            var width = mWidth(mDataSupplier);
            
            var rect = new Rectangle(mX(mDataSupplier), mY(mDataSupplier), width, mHeight(mDataSupplier));
            return rect;
        }
    }
}