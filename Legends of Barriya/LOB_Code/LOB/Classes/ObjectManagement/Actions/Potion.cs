using LOB.Classes.ObjectManagement.Events;
using LOB.Classes.ObjectManagement.Objects;
using System.Collections.Generic;
using LOB.Classes.Map;
using LOB.Classes.Rendering;
using Microsoft.Xna.Framework;

namespace LOB.Classes.ObjectManagement.Actions
{
    internal sealed class Potion : IAction
    {
        private GameObject mParentObject;
        private const int HealHealthPoints = 20;
        private const int DamageHealthPoints = 20;
        private const int TilePerSecondIncrease = 1;
        public const int HealRange = 2;
        public const int DamageRange = 1;
        public const int SpeedRange = 2;
        private const float SpeedEffectDuration = 45f;
        private const float CooldownHeal = 120;
        private const float CooldownDamage = 120;
        private const float CooldownSpeed = 240;
        private const float MillisecondsPerSecond = 1000.0f;
        private readonly List<GameObject> mSpedUpObjects;

        public Potion()
        {
            GetTimeToWaitHealPotion = 0;
            GetTimeToWaitDamagePotion = 0;
            GetTimeToWaitSpeedPotion = 0;
            GetTimeToWaitSpeedEffect = -1;
            mSpedUpObjects = new List<GameObject>();
        }

        public void Update(List<IEvent> events)
        {
            if (GetTimeToWaitHealPotion > 0)
            {
                GetTimeToWaitHealPotion -= DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
                if (GetTimeToWaitHealPotion < 0)
                {
                    GetTimeToWaitHealPotion = 0;
                }
            }

            if (GetTimeToWaitDamagePotion > 0)
            {
                GetTimeToWaitDamagePotion -= DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
                if (GetTimeToWaitDamagePotion < 0)
                {
                    GetTimeToWaitDamagePotion = 0;
                }
            }

            if (GetTimeToWaitSpeedPotion > 0)
            {
                GetTimeToWaitSpeedPotion -= DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
                if (GetTimeToWaitSpeedPotion < 0)
                {
                    GetTimeToWaitSpeedPotion = 0;
                    mParentObject.mHasChanged = true;
                }
            }

            if (GetTimeToWaitSpeedEffect > 0)
            {
                GetTimeToWaitSpeedEffect -= DataStorage.mGameTime.ElapsedGameTime.Milliseconds / MillisecondsPerSecond;
                if (GetTimeToWaitSpeedEffect < 0)
                {
                    GetTimeToWaitSpeedEffect = 0;
                }
            }

            if (events.Count == 0)
            {
                return;
            }
            
            var eEvent = (PotionEvent)events[0];
            int restriction;
            int points;
            switch (eEvent.mPotionType)
            {
                case PotionType.Damage1Potion:
                    if (GetTimeToWaitDamagePotion != 0)
                    {
                        events.Remove(eEvent);
                        return;
                    }
                    GetTimeToWaitDamagePotion = CooldownDamage;
                    points = DamageHealthPoints;
                    restriction = DamageRange;
                    break;
                case PotionType.Heal1Potion:
                    if (GetTimeToWaitHealPotion != 0)
                    {
                        events.Remove(eEvent);
                        return;
                    }
                    GetTimeToWaitHealPotion = CooldownHeal;
                    points = -HealHealthPoints;
                    restriction = HealRange;
                    break;
                case PotionType.Speed1Potion:
                    if (GetTimeToWaitSpeedEffect == 0)
                    {
                        GetTimeToWaitSpeedEffect = -1;
                    }

                    if (GetTimeToWaitSpeedPotion != 0)
                    {
                        events.Remove(eEvent);
                        return;
                    }

                    if (GetTimeToWaitSpeedEffect + 1 == 0)
                    {
                        GetTimeToWaitSpeedEffect = SpeedEffectDuration;
                        GetTimeToWaitSpeedPotion = CooldownSpeed;
                    }
                    points = 0;
                    restriction = SpeedRange;
                    break;
                default:
                    return;
            }
            Particle.PotionSpreadEffect(eEvent.mPosition, eEvent.mPotionType, restriction);
            Particle.MagicGlowEffect(mParentObject.mObjectPosition);
            for (var x = -restriction; x <= restriction; x++)
            {
                for (var y = -restriction; y <= restriction; y++)
                {
                    var gameObject = DataStorage.GetObject(new Point(eEvent.mPosition.X + x, eEvent.mPosition.Y + y));
                    if (gameObject == null || gameObject.mName > ObjectType.New1Building)
                    {
                        continue;
                    }

                    var (barrierPosition, barrierWidth) = GameMap.GetBarrierWidthAndPosition();
                    if (mParentObject.mObjectPosition.X < barrierPosition &&
                        gameObject.mObjectPosition.X >= barrierPosition)
                    {
                        continue;
                    }

                    if (mParentObject.mObjectPosition.X >= barrierPosition + barrierWidth &&
                        gameObject.mObjectPosition.X < barrierPosition + barrierWidth)
                    {
                        continue;
                    }

                    if (gameObject.mIsPlayer != mParentObject.mIsPlayer && eEvent.mPotionType == PotionType.Heal1Potion)
                    {
                        continue;
                    }

                    if (gameObject.mName > ObjectType.Builder && eEvent.mPotionType == PotionType.Damage1Potion)
                    {
                        continue;
                    }

                    if (points == 0)
                    {
                        gameObject.IncreaseSpeed(TilePerSecondIncrease);
                        if (!mSpedUpObjects.Contains(gameObject))
                        {
                            mSpedUpObjects.Add(gameObject);
                        }
                        continue;
                    }

                    gameObject.mObjectEvents[EventType.GetAttackedEvent].Add(new GetAttackedEvent(points, 0, Point.Zero));
                }
            }

            if (points == 0)
            {
                return;
            }

            events.Remove(eEvent);
        }

        public float GetTimeToWaitHealPotion { get; private set; }

        public float GetTimeToWaitDamagePotion { get; private set; }

        public float GetTimeToWaitSpeedPotion { get; private set; }

        private float GetTimeToWaitSpeedEffect { get; set; }

        public float GetCooldownHeal => CooldownHeal;
        public float GetCooldownDamage => CooldownDamage;
        public float GetCooldownSpeed => CooldownSpeed;

        public EventType GetEventType => EventType.PotionEvent;

        public IEnumerable<(string, int)> GetAttribute()
        {
            return new List<(string, int)>();
        }

        public string SaveAction()
        {
            return "[" + GetEventType + ":()]";
        }

        public static IAction LoadAction()
        {
            return new Potion();
        }

        public void SetParentObject(GameObject parent)
        {
            mParentObject = parent;
        }
    }
}
