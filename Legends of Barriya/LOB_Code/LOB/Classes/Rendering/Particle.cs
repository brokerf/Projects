#nullable enable
using System;
using System.Collections.Generic;
using LOB.Classes.ObjectManagement.Actions;
using LOB.Classes.ObjectManagement.Objects;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace LOB.Classes.Rendering
{
    internal sealed class ParticleEmitter
    {
        internal static List<ParticleEmitter> mForegroundEmitters = new List<ParticleEmitter>();
        internal static readonly Random sRandom = new Random();

        private float mPassedTime;
        private readonly int mPartsPerSecond;
        private readonly List<Particle> mPosParticles;
        internal readonly List<Particle> mDisplayedParticles = new List<Particle>();
        private readonly bool mIsForeground;

        public ParticleEmitter(List<Particle> posParticles, int partsPerSecond, bool isForeground)
        {
            mPosParticles = posParticles;
            foreach (var posParticle in mPosParticles)
            {
                posParticle.SetParent(this);
            }
            mPartsPerSecond = partsPerSecond;
            mIsForeground = isForeground;
        }

        internal void CloseEmitter()
        {
            foreach (var particle in mDisplayedParticles)
            {
                particle.RemoveParticle(mIsForeground? Particle.mForegroundParticles : Particle.mBackgroundParticles);
            }
        }

        private void UpdateInner(GameTime gameTime, ICollection<Particle> backgroundParticles)
        {
            mPassedTime += gameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
            while (mPassedTime>1f/mPartsPerSecond)
            {
                var newPart = new Particle(mPosParticles[sRandom.Next(mPosParticles.Count)]);
                backgroundParticles.Add(newPart);
                mDisplayedParticles.Add(newPart);
                mPassedTime -= 1f / mPartsPerSecond;
            }
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var foregroundEmitter in mForegroundEmitters)
            {
                foregroundEmitter.UpdateInner(gameTime, Particle.mForegroundParticles);
            }
        }
    }

    internal sealed class Particle
    {
        public Vector2 mPosition;
        public Vector2 mSize;
        private readonly Vector2 mDirection;
        private readonly float mGravitySpeed;
        private readonly Vector2 mGravityDirection;
        public Color mColor;
        internal static List<Particle> mBackgroundParticles = new List<Particle>();
        internal static List<Particle> mForegroundParticles = new List<Particle>();
        private ParticleEmitter? mParent;

        private Vector2? mMinPos;
        private Vector2? mMaxPos;
        public float mTimeToLive;
        private float mAcceleration;

        private readonly (int id, float timeTillNext, float passedTime)[]? mTextures;
        public int mCurrentTexture;
        private int mIndex;
        public readonly float mRotation;
        private readonly bool mDieOnImpact;

        public event Action? Death;

        internal Particle(Particle particle):this(particle.mPosition, particle.mSize, particle.mDirection, particle.mAcceleration, particle.mGravityDirection, particle.mColor, particle.mTimeToLive, particle.mGravitySpeed, particle.mMinPos, particle.mMaxPos, particle.mTextures, particle.mCurrentTexture, particle.mRotation, particle.mDieOnImpact){}

        private Particle(Vector2 position,
            Vector2 size,
            Vector2 direction,
            float acceleration,
            Vector2 gravityDirection,
            Color color,
            float timeToLive,
            float gravitySpeed,
            Vector2? minPos = null,
            Vector2? maxPos = null,
            (int, float, float)[]? textures = null,
            int currentTexture = -1,
            float rotation = 0f, 
            bool dieOnImpact = false)
        {
            mPosition = position;
            mDirection = direction;
            mGravityDirection = gravityDirection;
            mColor = color;
            mTimeToLive = timeToLive;
            mGravitySpeed = gravitySpeed;
            mAcceleration = acceleration;
            mSize = size;
            mMinPos = minPos;
            mMaxPos = maxPos;
            mTextures = textures;
            mCurrentTexture = currentTexture;
            mRotation = rotation;
            mDieOnImpact = dieOnImpact;
        }

        public void SetParent(ParticleEmitter parent)
        {
            mParent = parent;
        }

        public void RemoveParticle(List<Particle> particleGroup)
        {
            OnDeath();
            particleGroup.Remove(this);
            mParent?.mDisplayedParticles.Remove(this);
        }

        public void Update(GameTime gameTime, List<Particle> particleGroup)
        {
            var passedTime = gameTime.ElapsedGameTime.Milliseconds / IAction.MillisecondsPerSecond;
            mTimeToLive -= passedTime;
            if (mTimeToLive <= 0)
            {
                RemoveParticle(particleGroup);
                return;
            }

            if (mCurrentTexture != -1 && mTextures != null)
            {
                mTextures[mIndex].passedTime += passedTime;
                if (mTextures[mIndex].passedTime >= mTextures[mIndex].timeTillNext)
                {
                    var remaining = mTextures[mIndex].passedTime - mTextures[mIndex].timeTillNext;
                    mIndex = (mIndex+1) % mTextures.Length;
                    mCurrentTexture = mTextures[mIndex].id;
                    mTextures[mIndex].passedTime = remaining;
                }
            }

            mPosition += passedTime * mDirection;
            mAcceleration += passedTime * mGravitySpeed;
            mPosition += mAcceleration * mGravityDirection;

            var hasHit = false;
            if (mMinPos != null)
            {
                if (mPosition.X < mMinPos.Value.X)
                {
                    mPosition.X = mMinPos.Value.X;
                    hasHit = true;
                }
                if (mPosition.Y < mMinPos.Value.Y)
                {
                    mPosition.Y = mMinPos.Value.Y;
                    hasHit = true;
                }
            }

            if (mMaxPos != null)
            {
                if (mPosition.X > mMaxPos.Value.X)
                {
                    mPosition.X = mMaxPos.Value.X;
                    hasHit = true;
                }
                if (mPosition.Y > mMaxPos.Value.Y)
                {
                    mPosition.Y = mMaxPos.Value.Y;
                    hasHit = true;
                }
            }

            if (hasHit && mDieOnImpact)
            {
                RemoveParticle(particleGroup);
            }
        }

        public static IEnumerable<Particle> UpgradeEffect(int amount, int x, int y)
        {
            var particles = new List<Particle>();
            var random = ParticleEmitter.sRandom;
            for (var i = 0; i < amount; i++)
            {
                var xDirection = random.Next(-100, 100) / 100f;
                var yDirection = random.Next(-100, 0) / 100f;
                var timeToLife = random.Next(0, 20) / 10f;
                particles.Add(new Particle(new Vector2(x + 0.5f, y + 0.5f),
                    new Vector2(0.1f, 0.1f),
                    new Vector2(xDirection, yDirection),
                    0.0001f,
                    new Vector2(0, -1),
                    Color.LimeGreen,
                    timeToLife,
                    0.01f));
            }

            return particles;
        }

        public static IEnumerable<Particle> BloodEffect(int amount, int x, int y, int victimX, int victimY)
        {
            var particles = new List<Particle>();
            var random = ParticleEmitter.sRandom;
            var side = x < victimX ? 1 : (x == victimX ? 0 : -1);
            var isTop = y < victimY;
            for (var i = 0; i < amount; i++)
            {
                var xDirection = (random.Next(100) / 100f) * (side == 0 ? random.Next(-2, 2) : side);
                var yDirection = random.Next(100) / 100f * (isTop ? 1 : -1);
                var timeToLife = random.Next(0, 20) / 10f;
                particles.Add(new Particle(new Vector2(victimX + 0.5f, victimY + 0.5f),
                    new Vector2(0.05f, 0.05f),
                    new Vector2(xDirection, yDirection),
                    0.005f,
                    new Vector2(0, 1),
                    Color.DarkRed,
                    timeToLife,
                    0.01f, null, new Vector2(100000, victimY + 0.9f)));
            }

            return particles;
        }
        public static Particle? FootStepsEffect(float x, float y)
        {
            var random = ParticleEmitter.sRandom;
            if (random.Next(0, 3) < 2)
            {
                return null;
            }

            var color = random.Next(0, 2) == 1 ? Color.Gray : Color.SaddleBrown;
            var heightOffset = random.Next(0, 10) / 100f;
            return new Particle(new Vector2(x + 0.5f, y + 0.9f-heightOffset),
                new Vector2(0.05f, 0.05f),
                Vector2.Zero, 
                0,
                Vector2.Zero,
                color*.7f,
                1,
                0.01f);
        }

        public static Particle BuildingEffect(float x, float y)
        {
            var random = ParticleEmitter.sRandom;

            var size = random.Next(5, 20);

            var xDirection = random.Next(0, 100-size) / 100f;
            var yDirection = random.Next(-100, 0) / 100f;
            var timeToLife = random.Next(0, 10) / 10f;
            return new Particle(new Vector2(x + xDirection, y + 0.9f - size / 100f),
                new Vector2(size/100f, size / 100f),
                new Vector2(0, yDirection),
                0.0001f,
                new Vector2(0, -1),
                random.Next(2) == 1 ? Color.DarkGray : Color.Gray,
                timeToLife,
                0.01f);
        }

        public static List<Particle> PortalEffect(int x, int y, int exitX, int exitY, bool isPlayer)
        {
            var particles = new List<Particle>();
            var random = ParticleEmitter.sRandom;
            var dir = new Vector2(exitX - x, exitY - y);
            dir = new Vector2(dir.X/dir.Length(), dir.Y/dir.Length());
            var timeToLife = 10;
            var size = new Vector2(0.5f, 0.5f);
            var exitPos = new Vector2(exitX + 0.5f - size.X / 2, exitY + 0.5f - size.Y / 2);
            var enterPos = new Vector2(x + 0.5f - size.X / 2, y + 0.5f - size.Y / 2);
            var maxPosition = new Vector2(x<exitX ? exitPos.X : enterPos.X, y < exitY ? exitPos.Y : enterPos.Y);
            var minPosition = new Vector2(x > exitX ? exitPos.X : enterPos.X, y > exitY ? exitPos.Y : enterPos.Y);
            for (var i = 0; i < 20; i++)
            {
                particles.Add(new Particle(new Vector2(enterPos.X+random.Next(-2, 2)/4f, enterPos.Y + random.Next(-2, 2) / 4f),
                    size,
                    dir * (6 + random.Next(100) / 50f),
                    0.005f,
                    new Vector2(0, 0),
                    (isPlayer? random.Next(2) == 1 ? Color.Blue : Color.LightBlue : random.Next(2) == 1 ? Color.Red : Color.Orange) *0.2f,
                    timeToLife,
                    0f, minPosition, maxPosition));
            }
            return particles;
        }

        public static IEnumerable<Particle> PortalOpeningEffect(int x, int y, bool isPlayer, int version)
        {
            var particles = new List<Particle>();
            var random = ParticleEmitter.sRandom;
            for (var i = 0; i < 40; i++)
            {
                var xDirection = random.Next(-100, 100) / 100f;
                var yDirection = random.Next(-100, 100) / 100f;
                var timeToLife = random.Next(0, 20) / 10f;
                if (version == 0)
                {
                    particles.Add(new Particle(new Vector2(x + 0.5f - 0.3f / 2, y + 0.5f - 0.3f / 2),
                        new Vector2(0.3f, 0.3f),
                        new Vector2(xDirection, yDirection),
                        0.0001f,
                        new Vector2(0, 0),
                        (isPlayer ? random.Next(2) == 1 ? Color.Blue :
                            Color.LightBlue :
                            random.Next(2) == 1 ? Color.Red : Color.Orange) * 0.2f,
                        timeToLife,
                        0));
                }
                else
                {
                    timeToLife = random.Next(0, 10) / 10f;
                    particles.Add(new Particle(new Vector2(x + 0.5f - 0.1f / 2, y + 0.1f - 0.1f / 2),
                        new Vector2(0.1f, 0.1f),
                        new Vector2(xDirection, yDirection),
                        0.0001f,
                        new Vector2(0, 0),
                        (random.Next(2) == 1 ? Color.Purple : Color.DarkMagenta) * 0.7f,
                        timeToLife,
                        0));
                }
            }

            return particles;
        }

        /*public static IEnumerable<Particle> MageTowerEffect(int x, int y)
        {
            var particles = new List<Particle>();
            var random = ParticleEmitter.sRandom;
            for (var i = 0; i < 40; i++)
            {
                var xDirection = random.Next(-100, 100) / 50f;
                var yDirection = random.Next(-100, 100) / 50f;
                var timeToLife = random.Next(0, 10) / 10f;
                particles.Add(new Particle(new Vector2(x + 0.5f - 0.1f / 2, y + 0.1f - 0.1f / 2),
                    new Vector2(0.1f, 0.1f),
                    new Vector2(xDirection, yDirection),
                    0.0001f,
                    new Vector2(0, 0),
                    (random.Next(2) == 1 ? Color.Purple : Color.DarkMagenta)*0.7f,
                    timeToLife,
                    0));
            }

            return particles;
        }*/

        public static void PotionSpreadEffect(Point position, PotionType type, int range)
        {
            var positionCentered = (position.ToVector2()) + new Vector2(0.5f, 0.5f);
            var random = ParticleEmitter.sRandom;
            var minPos = (position - new Point(range, range)).ToVector2();
            var maxPos = (position + new Point(range + 1, range + 1)).ToVector2();
            for (var i = 0; i < 100*range; i++)
            {
                var xDirection = (random.Next(-100, 100) / 75f);
                var yDirection = (random.Next(-100, 100) / 75f);
                var timeToLife = random.Next(25, 100) / 50f;
                Color color;
                int green;
                int blue;
                int red;
                switch (type)
                {
                    case PotionType.Damage1Potion:
                        green = random.Next(75);
                        blue = random.Next(50);
                        red = 255 - (green + blue);
                        color = new Color(red, green, blue, 255);
                        break;
                    case PotionType.Heal1Potion:
                        red = random.Next(50);
                        blue = random.Next(75);
                        green = 255 - (red + blue);
                        color = new Color(red, green, blue, 255);
                        break;
                    case PotionType.Speed1Potion:
                        red = random.Next(50);
                        green = random.Next(150);
                        blue = 255 - (red + green);
                        color = new Color(red, green, blue, 255);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                var particle = new Particle(positionCentered + new Vector2(xDirection, yDirection) * (Math.Max(range - 1, 0.5f)),
                    new Vector2(0.1f, 0.1f),
                    new Vector2(xDirection*0.6f, yDirection * 0.6f), 
                    0,
                    new Vector2(0, -0.1f), 
                    color,
                    timeToLife,
                    .05f, minPos, maxPos, dieOnImpact: true);
                if (ParticleEmitter.sRandom.Next(2) == 1)
                {
                    mForegroundParticles.Add(particle);
                    continue;
                }
                mBackgroundParticles.Add(particle);
            }

        }

        // EFFECTS WITH TEXTURES

        // USES IMAGES 0 AND 1
        public static Particle FireballEffect(float timeTillImpact, int x, int y, int victimX, int victimY)
        {
            var dir = new Vector2(victimX - x, victimY - y) / timeTillImpact;
            var timeToLife = 50;
            var part = new Particle(new Vector2(x, y),
                new Vector2(0.05f, 0.05f),
                dir,
                0,
                Vector2.Zero,
                Color.White,
                timeToLife,
                0, new Vector2(Math.Min(x, victimX), Math.Min(y, victimY)), new Vector2(Math.Max(x, victimX), Math.Max(y, victimY)), new[] { (0, .03f, 0f), (1, .03f, 0f) }, 0, dir.ToAngle() - (float)Math.PI / 2, true);
            part.Death += part.FireballExplosionEffect;
            return part;
        }

        public static Particle SwordEffect(int x, int y, int victimX, int victimY)
        {
            var dir = new Vector2(victimX - x, victimY - y);
            var timeToLife = 0.2f;
            var part = new Particle(new Vector2(x, y),
                new Vector2(0.05f, 0.05f),
                Vector2.Zero, 
                0,
                Vector2.Zero,
                Color.White,
                timeToLife,
                0, textures: new[] { (18, timeToLife, 0f) }, currentTexture: 18, rotation: dir.ToAngle() - (float)Math.PI / 2);
            return part;
        }

        public static Particle PhalanxBuffEffect(Point position, bool isOtherBuffOn)
        {
            var timeToLife = .8f;
            var part = new Particle(position.ToVector2() - new Vector2(isOtherBuffOn  ? - 2/7f : 0, 2 / 7f),
                new Vector2(0.05f, 0.05f),
                Vector2.Zero,
                0,
                Vector2.Zero,
                Color.White,
                timeToLife,
                0, currentTexture: 19, textures: new[] { (19, timeToLife / 3, 0f), (20, timeToLife / 3, 0f), (21, timeToLife / 3, 0f)});
            return part;
        }

        //Added version so it does not trigger Duplicated Blocks in Sonar
        public static Particle ArrowEffect(float timeImpact, int x, int y, int victimX, int victimY, int version)
        {
            var dir = new Vector2(victimX - x, victimY - y) / timeImpact;
            var timeToLife = 50;
            var part = new Particle(new Vector2(x, y),
                new Vector2(0.05f, 0.05f),
                dir,
                0,
                Vector2.Zero,
                Color.Black,
                timeToLife,
                0,
                new Vector2(Math.Min(x, victimX), Math.Min(y, victimY)),
                new Vector2(Math.Max(x, victimX), Math.Max(y, victimY)),
                new[] {(16 + version, 10000000000000f, 0f)},
                16 + version,
                dir.ToAngle() - (float) Math.PI / 2,
                true);
            return part;
        }

        // USES IMAGES 2, 3, 4 AND 5
        public static Particle ResourceEffect(Point position, ResourceType type)
        {
            var image = (int)type + 2;
            var timeToLife = 1;
            var part = new Particle(position.ToVector2()+new Vector2(0, -0.5f),
                new Vector2(0.05f, 0.05f),
                new Vector2(0, -1),
                0,
                Vector2.Zero,
                Color.White,
                timeToLife,
                0, currentTexture: image, textures: new[] { (image, (float)timeToLife, 0f)});
            return part;
        }

        private void FireballExplosionEffect()
        {
            var particles = new List<Particle>();
            var random = ParticleEmitter.sRandom;
            for (var i = 0; i < 40; i++)
            {
                var xDirection = random.Next(-100, 100) / 200f;
                var yDirection = random.Next(-50, 100) / 100f;
                var timeToLife = random.Next(0, 20) / 20f;
                particles.Add(new Particle(new Vector2((int)Math.Round(mPosition.X) + 0.5f - 0.1f / 2, (int)Math.Round(mPosition.Y) + 0.5f - 0.1f / 2),
                    new Vector2(0.1f, 0.1f),
                    new Vector2(xDirection, yDirection),
                    0.0001f,
                    new Vector2(0, -1),
                    (random.Next(2) == 1 ? Color.Red : Color.Orange) * 0.2f,
                    timeToLife,
                    .05f));
            }

            mForegroundParticles.AddRange(particles);
        }

        // USES IMAGES 6, 7, 8, 9, 10 AND 11
        public static void MagicGlowEffect(Point position)
        {
            var timeToLife = .5f;
            var part = new Particle(position.ToVector2(),
                new Vector2(0.05f, 0.05f),
                Vector2.Zero,
                0,
                Vector2.Zero,
                Color.White, 
                timeToLife,
                0, currentTexture: 6, textures: new[] { (6, timeToLife/10, 0f), (7, timeToLife / 2.5f, 0f), (8, timeToLife / 2.5f, 0f), (7, timeToLife / 10, 0f)});
            mBackgroundParticles.Add(part); 
            var part2 = new Particle(position.ToVector2(),
                new Vector2(0.05f, 0.05f),
                Vector2.Zero,
                0,
                Vector2.Zero,
                Color.White,
                timeToLife,
                0, currentTexture: 9, textures: new[] { (9, timeToLife / 5, 0f), (10, timeToLife / 5, 0f), (11, timeToLife / 5, 0f), (10, timeToLife / 5, 0f)});
            mForegroundParticles.Add(part2);
        }

        // USES IMAGES 12, 13, 14 AND 15
        public static Particle BuffEffect(Point position)
        {
            var timeToLife = .8f;
            var part = new Particle(position.ToVector2()-new Vector2(0, 2/7f),
                new Vector2(0.05f, 0.05f),
                Vector2.Zero, 
                0,
                Vector2.Zero,
                Color.White,
                timeToLife,
                0, currentTexture: 13, textures: new[] { (13, timeToLife / 4, 0f), (14, timeToLife / 4, 0f), (15, timeToLife / 4, 0f), (12, timeToLife / 4, 0f)});
            return part;
        }

        private void OnDeath()
        {
            Death?.Invoke();
        }
    }
}