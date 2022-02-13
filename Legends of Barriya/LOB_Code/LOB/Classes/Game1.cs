using System.IO;
using LOB.Classes.Data;
using LOB.Classes.Rendering;
using LOB.Classes.Screen;
using Microsoft.Xna.Framework;

namespace LOB.Classes
{
    internal sealed class Game1 : Game
    {
        internal enum Race{Human, Dwarf, Orc}

        internal static Race mPlayerRace = Race.Human;
        internal static Race mEnemyRace = Race.Orc;

        private readonly GraphicsDeviceManager mGraphics;
        private ScreenManager mScreenManager;

        public Game1()
        {
            var path = ContentIo.GetPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Renderer.Initialize(mGraphics, Content);
            mScreenManager = new ScreenManager(Content, Window);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (mScreenManager.Update(mGraphics, Content, gameTime))
            {
                Achievements.AddTime((int)ScreenManager.sSessionTime.Elapsed.TotalSeconds);
                Achievements.SaveAchievements();
                Exit();
                return;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            mScreenManager.Draw();
            base.Draw(gameTime);
        }
    }
}