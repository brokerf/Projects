using LOB.Classes.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LOB.Classes.Rendering
{
    internal sealed class Camera
    {
        private Vector2 mCameraOffset;
        private readonly Vector2 mMapSize = new Vector2(0, 0);
        private int mLastScrollWheelValue;
        private GraphicsDevice mGraphicsDevice;
        public static Vector2 SideBarOffset => new Vector2(0 * Renderer.CurrentScreenScale, 50*Renderer.CurrentScreenScale);
        // mSideBarOffset is used to add a blank area to the top and/or left of the screen; to give space for the resource bar without making the top row of tiles unreachable
        internal Camera(int standardTileSize, int width, int height, int scrollWheelValue)
        {
            var graphicsDevice = Renderer.mGraphicsDeviceManager.GraphicsDevice;
            mGraphicsDevice = graphicsDevice;
            mMapSize.X = standardTileSize * width;
            mMapSize.Y = standardTileSize * height;
            mCameraOffset = new Vector2(0, 0);
            mLastScrollWheelValue = scrollWheelValue;
        }

        internal bool Update(InputData inputData, bool mouseOverMenu)
        {
            var returnValue = false;
            var graphicsDevice = Renderer.mGraphicsDeviceManager.GraphicsDevice;
            mGraphicsDevice = graphicsDevice;

            var edgeSize = Renderer.mGraphicsDeviceManager.GraphicsDevice.Viewport.Height / 10;

            if (inputData.mDownKeys.Contains(Keys.W) || inputData.mMouseData.mNextMouseState.Y < edgeSize && !mouseOverMenu)
            {
                mCameraOffset.Y += 10 * Renderer.mScale;
            }
            if (inputData.mDownKeys.Contains(Keys.S) || inputData.mMouseData.mNextMouseState.Y > Renderer.mGraphicsDeviceManager.GraphicsDevice.Viewport.Height - edgeSize && !mouseOverMenu)
            {
                mCameraOffset.Y -= 10 * Renderer.mScale;
            }
            if (inputData.mDownKeys.Contains(Keys.A) || inputData.mMouseData.mNextMouseState.X < edgeSize && !mouseOverMenu)
            {
                mCameraOffset.X += 10 * Renderer.mScale;
            }
            if (inputData.mDownKeys.Contains(Keys.D) || inputData.mMouseData.mNextMouseState.X > Renderer.mGraphicsDeviceManager.GraphicsDevice.Viewport.Width - edgeSize && !mouseOverMenu)
            {
                mCameraOffset.X -= 10 * Renderer.mScale;
            }
            // compares the last two ScrollValues, if they are different, it determines how far to Zoom in or out
            var currentScrollWheelValue = inputData.mMouseData.mNextMouseState.ScrollWheelValue;
            if (mLastScrollWheelValue != currentScrollWheelValue)
            {
                var difference = mLastScrollWheelValue - currentScrollWheelValue;
                var lastScale = Renderer.mScale;
                mLastScrollWheelValue = currentScrollWheelValue;
                Renderer.mScale -= 0.001f * difference;

                returnValue = true;
                // Maximum Zoom
                if (Renderer.mScale > 5f * Renderer.mCurrentScreenScale)
                {
                    Renderer.mScale = 5f * Renderer.mCurrentScreenScale;
                }
                // Moves the camera to accommodate for zoom, so that the mouse is the focus.
                var middle = new Vector2(SideBarOffset.X+ mCameraOffset.X - inputData.mMouseData.mNextMouseState.X, SideBarOffset.Y + mCameraOffset.Y - inputData.mMouseData.mNextMouseState.Y);
                var newMiddle = Vector2.Transform(middle, Matrix.CreateScale(Renderer.mScale / lastScale));
                mCameraOffset = new Vector2(newMiddle.X - SideBarOffset.X + inputData.mMouseData.mNextMouseState.X, newMiddle.Y - SideBarOffset.Y + inputData.mMouseData.mNextMouseState.Y);
            }
            // Stops the camera from leaving the tiled area and going off screen
            if (-mCameraOffset.Y + graphicsDevice.Viewport.Height - SideBarOffset.Y > mMapSize.Y * Renderer.mScale * 0.5f)
            {
                mCameraOffset.Y = -(mMapSize.Y * Renderer.mScale * 0.5f - graphicsDevice.Viewport.Height + SideBarOffset.Y);
            }

            if (mCameraOffset.Y > 0)
            {
                mCameraOffset.Y = 0;
            }

            if (-mCameraOffset.X + graphicsDevice.Viewport.Width - SideBarOffset.X  > mMapSize.X * Renderer.mScale * 0.5f)
            {
                mCameraOffset.X = -(mMapSize.X * Renderer.mScale * 0.5f - graphicsDevice.Viewport.Width + SideBarOffset.X);
            }

            if (mCameraOffset.X > 0)
            {
                mCameraOffset.X = 0;
            }
            return returnValue;
        }

        public void ResizeToFitWindow()
        {
            //If the Zoom makes the displayed map smaller than the window (going out of bounds), it sets the Zoom to the largest it can be without doing so.
            //Is currently used at creation of camera (and thereby creation of a map) to set the zoom all the way out, so that it will fit any display size and dimensions.
            if (mMapSize.Y * Renderer.mScale * 0.5f < mGraphicsDevice.Viewport.Height - SideBarOffset.Y )
            {
                
                Renderer.mScale = ((mGraphicsDevice.Viewport.Height - SideBarOffset.Y ) * 2) / mMapSize.Y;
            }
            if (mMapSize.X * Renderer.mScale * 0.5f < mGraphicsDevice.Viewport.Width - SideBarOffset.X)
            {
                Renderer.mScale = ((mGraphicsDevice.Viewport.Width - SideBarOffset.X) * 2) / mMapSize.X;
            }
            Renderer.mCurrentScreenScale =
                (float)mGraphicsDevice.Viewport.Width / mGraphicsDevice.Adapter.CurrentDisplayMode.Width;
        }

        public void Zoom((int, int) oldSize)
        {
            var (width, height) = (mGraphicsDevice.Viewport.Width, mGraphicsDevice.Viewport.Height);

            var lastScale = Renderer.mScale;
            Renderer.mScale *= ((float)width /oldSize.Item1);
            var middle = new Vector2(mCameraOffset.X - oldSize.Item1, mCameraOffset.Y - oldSize.Item2);
            var newMiddle = Vector2.Transform(middle, Matrix.CreateScale(Renderer.mScale / lastScale));
            mCameraOffset = new Vector2(newMiddle.X + width, newMiddle.Y + height);
            ResizeToFitWindow();
        }

        public Vector2 GetCameraOffset()
        {
            return new Vector2(mCameraOffset.X + SideBarOffset.X, mCameraOffset.Y + SideBarOffset.Y);
        }
    }
}