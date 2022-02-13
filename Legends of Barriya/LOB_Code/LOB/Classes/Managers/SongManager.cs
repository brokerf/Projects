using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace LOB.Classes.Managers
{
    internal sealed class SongManager
    {
        private static List<Song> sSongs = new List<Song>();
        private static List<int> sRepeatTimes = new List<int>();
        private static float sVolume;
        private static float sEffectVolume;
        private static ContentManager sContentManager;

        private static readonly Dictionary<string, SoundEffect> sLoadedSounds = new Dictionary<string, SoundEffect>();

        /// <summary>
        /// Initiate a new SongManager with loaded Songs. Initializes the first Song loaded as
        /// the first song played.
        /// </summary>
        public SongManager(ContentManager content, float volume = 0.1f, float effectsVolume = 0.1f)
        {
            sVolume = volume;
            sEffectVolume = effectsVolume;
            MediaPlayer.Volume = sVolume;
            sContentManager = content;
            MediaPlayer.MediaStateChanged += CurrentSongStopped;

            sLoadedSounds.Add("Buzzer", content.Load<SoundEffect>("Buzzer"));
            sLoadedSounds.Add("sword1", content.Load<SoundEffect>("sword1"));
            sLoadedSounds.Add("sword2", content.Load<SoundEffect>("sword2"));
            sLoadedSounds.Add("sword3", content.Load<SoundEffect>("sword3"));
            sLoadedSounds.Add("arrow", content.Load<SoundEffect>("arrow"));
        }

        public (float volume, float effectsVolume) GetVolume()
        {
            return (sVolume, sEffectVolume);
        }

#nullable enable
        private void CurrentSongStopped(object? sender, EventArgs e)
        {
            if (MediaPlayer.State != MediaState.Stopped)
            {
                return;
            }
            var x = sSongs.FindIndex(song => song.Name == MediaPlayer.Queue.ActiveSong.Name);
            if (sRepeatTimes != null)
            {
                var repeatsLeft = sRepeatTimes[x];
                if (repeatsLeft > 0)
                { 
                    sRepeatTimes[x] -= 1; 
                    x -= 1;
                }
                else if (repeatsLeft == -1)
                { 
                    x -= 1;
                }
            }
            x += 1;
            
            if (x >= sSongs.Count)
            {
                x = 0;
            }

            MediaPlayer.Play(sSongs[x]);
        }

        /// <summary>
        /// Play an Effect once the Condition is met.
        /// </summary>
        /// <param name="effect"></param>
        public static void PlayEffect(string effect)
        {
            if (!sLoadedSounds.TryGetValue(effect, out var sound))
            {
                sound = sContentManager.Load<SoundEffect>(effect);
                sLoadedSounds.Add(effect, sound);
            }
            sound.Play(sEffectVolume, 0f, 0f);
        }

        
        public void ChangeVolume(float amount)
        {
            sVolume += amount;
            if (sVolume < 0)
            {
                sVolume = 0;
            }

            if (sVolume > 1)
            {
                sVolume = 1;
            }
            MediaPlayer.Volume = sVolume;
        }

        public void ChangeEffectsVolume(float amount)
        {
            sEffectVolume += amount;
            if (sEffectVolume < 0)
            {
                sEffectVolume = 0;
            }

            if (sEffectVolume > 1)
            {
                sEffectVolume = 1;
            }
        }

        public void PlaySongList(List<Song> newSongList, List<int> repeatTimes = null!)
        {
            
            if (newSongList.Count < 1  && repeatTimes.Count < newSongList.Count)
            {
                return;
            }
            
            sRepeatTimes = repeatTimes;
            sSongs = newSongList;
            MediaPlayer.Play(sSongs[0]);
        }
    }
}
