using System.Collections.Generic;
using System.IO;

namespace LOB.Classes.Data
{
    internal sealed class Settings
    {
        public void SaveSettings(bool isBorderlessMode, float volume, float effectsVolume, float hudScale)
        {
            var path = ContentIo.GetPath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += ContentIo.GetPathConnector;

            var lines = new List<string>()
            {
                "Borderless Mode = " + (isBorderlessMode ? "true" : "false"),
                "Volume = " + volume,
                "HUDScale = " + hudScale,
                "Effects Volume = " + effectsVolume
            };

            if (File.Exists(path + "Settings.txt"))
            {
                File.Delete(path + "Settings.txt");
            }

            var newFile = File.Create(path + "Settings.txt");
            newFile.Close();
            File.WriteAllLines(path + "Settings.txt", lines);
        }

        public (bool borderless, float volume, float effectsVolume, float hudScale)? LoadSettings()
        {
            var path = ContentIo.GetPath + ContentIo.GetPathConnector;

            if (!File.Exists(path + "Settings.txt") || new FileInfo(path + "Settings.txt").Length == 0)
            {
                return null;
            }

            var lines = File.ReadAllLines(path + "Settings.txt");

            var borderless = lines[0].Split(" = ")[1] == "true";
            var volume = float.Parse(lines[1].Split(" = ")[1]);
            var hudScale = 1f;
            if (lines.Length >= 3)
            {
                float.TryParse(lines[2].Split(" = ")[1], out hudScale);
            }

            var effectsVolume = volume;
            if (lines.Length >= 4)
            {
                float.TryParse(lines[3].Split(" = ")[1], out effectsVolume);
            }

            return (borderless, volume, effectsVolume, hudScale);
        }
    }
}
