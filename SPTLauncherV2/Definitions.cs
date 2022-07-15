using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTLauncherV2 {
    internal class Definitions {

    }

    public class Profile {
        public string ProfileName;
        public Config ProfileConfig;

        public Profile(string ProfileName, Config ProfileConfig) {
            this.ProfileName = ProfileName;
            this.ProfileConfig = ProfileConfig;
        }
    }

    public class Config {
        public string LauncherLocation;
        public string ServerLocation;
        public string BaseLocation;
        public string GameVersion;
        public List<Mod> ModList;
        public List<string> ModFilters;

        public Config(List<Mod> ModList, List<string> ModFilters, string LauncherLocation, 
            string ServerLocation, string BaseLocation, string GameVersion) {

            this.ModList = ModList;
            this.ModFilters = ModFilters;
            this.LauncherLocation = LauncherLocation;   
            this.ServerLocation = ServerLocation;
            this.BaseLocation = BaseLocation;
            this.GameVersion = GameVersion;
        }
    }

    public class Modpack {
        public string Name;
        public string TargetVersion;
        public string Author;
        public List<Mod> ModList;

        public Modpack(string Name, string TargetVersion, string Author, List<Mod> ModList) {
            this.Name = Name;
            this.TargetVersion = TargetVersion;
            this.Author = Author;
            this.ModList = ModList;
        }
    }

    public class Mod {
        public string ModName;
        public string ModAuthor;
        public string ModLocation;
        public string ModVersion;
        public string GameVersion;
        public string MainFileLocation;
        public bool ModEnabled;

        public Mod(string modName, string modAuthor, string modLocation, 
            string modVersion, string gameVersion, string mainFileLocation, 
            bool modEnabled) {

            ModName = modName;
            ModAuthor = modAuthor;
            ModLocation = modLocation;
            ModVersion = modVersion;
            GameVersion = gameVersion;
            MainFileLocation = mainFileLocation;
            ModEnabled = modEnabled;
        }
    }
}
